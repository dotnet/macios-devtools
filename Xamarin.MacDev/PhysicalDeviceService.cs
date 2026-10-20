// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.MacDev.Models;

#nullable enable

namespace Xamarin.MacDev {

	/// <summary>
	/// Discovers the physical Apple devices (iPhone, iPad, Apple Watch, Apple TV, Apple Vision Pro and Mac)
	/// that CoreDevice knows about by running <c>xcrun devicectl list devices</c>.
	/// This is the physical-device counterpart of <see cref="SimulatorService"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Failures are reported through <see cref="PhysicalDeviceDiscoveryResult.Status"/> rather than exceptions,
	/// so callers can tell an empty device list (<see cref="PhysicalDeviceDiscoveryStatus.Success"/> with no
	/// devices) from a missing tool, a failing tool, a timeout or unusable output. Only invalid arguments,
	/// caller cancellation and unexpected internal errors throw.
	/// </para>
	/// <para>
	/// Requirements: macOS with Xcode 15 or later (devicectl is not part of older Xcode versions or of the Command
	/// Line Tools, and Xcode 15 requires macOS 13.5 or later). By default the Xcode that the environment selects
	/// (<c>DEVELOPER_DIR</c> or <c>xcode-select</c>) is used; to use a specific one, pass its developer directory to the
	/// constructor. devicectl manages devices through CoreDevice, which targets iOS and iPadOS 17, tvOS 17, watchOS 10
	/// and visionOS devices; devices CoreDevice cannot manage, such as ones running older operating systems, may be missing
	/// from the list or listed with little information (for example no name, OS version or hardware UDID, and a pairing
	/// state of <c>unsupported</c>). On any other host, discovery returns
	/// <see cref="PhysicalDeviceDiscoveryStatus.UnsupportedHost"/> without running anything.
	/// </para>
	/// <para>
	/// Discovery is read-only: it never pairs with, trusts, connects to or modifies a device.
	/// </para>
	/// <para>
	/// The result contains every physical device CoreDevice knows about, including remembered devices that are
	/// currently not reachable; use <see cref="PhysicalDeviceInfo.Availability"/> to tell them apart.
	/// Simulators, which newer Xcode versions also list in devicectl, are excluded; use
	/// <see cref="SimulatorService"/> for those. If devicectl reports the same device more than once (for example
	/// once per connection path, such as over USB and over the network), those reports are merged into a single
	/// <see cref="PhysicalDeviceInfo"/>, correlated by <see cref="PhysicalDeviceInfo.HardwareUdid"/>, with each
	/// transport listed in <see cref="PhysicalDeviceInfo.Connections"/>. Devices for which devicectl reports no
	/// hardware UDID cannot be correlated and are never merged (see <see cref="PhysicalDeviceInfo.HasHardwareUdid"/>).
	/// </para>
	/// <para>
	/// All diagnostics go to the <see cref="ICustomLogger"/>; nothing is written to stdout. A failed discovery is
	/// logged at info level because it is already reported in the result; only an abnormal condition (a devicectl runner
	/// that does not stop when cancelled) is logged as a warning.
	/// </para>
	/// <para>
	/// To test code that uses this service without Xcode or a device, derive from it and override
	/// <see cref="ExecuteDeviceCtlAsync"/> to return canned devicectl output.
	/// </para>
	/// </remarks>
	public class PhysicalDeviceService {

		/// <summary>The time <see cref="ListAsync(CancellationToken)"/> waits for devicectl to finish before giving up.</summary>
		public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds (30);

		/// <summary>How long a runner that ignores cancellation is waited for before it is abandoned.</summary>
		static readonly TimeSpan DefaultGracePeriod = TimeSpan.FromSeconds (5);

		readonly ICustomLogger log;
		readonly DeviceCtl deviceCtl;
		readonly TimeSpan gracePeriod;

		/// <summary>Creates a service that runs <c>xcrun devicectl</c> on the current host, with the Xcode the environment selects.</summary>
		public PhysicalDeviceService (ICustomLogger log)
			: this (log, developerRoot: null)
		{
		}

		/// <summary>
		/// Creates a service that runs <c>xcrun devicectl</c> on the current host, with the Xcode installed at
		/// <paramref name="developerRoot"/> instead of the one <c>xcode-select</c> or <c>DEVELOPER_DIR</c> selects.
		/// </summary>
		/// <param name="log">Receives all diagnostics.</param>
		/// <param name="developerRoot">The Xcode developer directory, for example <c>/Applications/Xcode.app/Contents/Developer</c>
		/// (see <see cref="AppleSdkSettings.DeveloperRoot"/>). It is passed as <c>DEVELOPER_DIR</c> to the devicectl process
		/// only; the environment of the current process is not changed, so services for different Xcode installations can
		/// be used concurrently. <c>null</c>, empty or white space uses the Xcode the environment selects.
		/// A directory that does not exist or is not an Xcode results in <see cref="PhysicalDeviceDiscoveryStatus.ToolUnavailable"/>.</param>
		public PhysicalDeviceService (ICustomLogger log, string? developerRoot)
			: this (log, developerRoot, DefaultGracePeriod)
		{
		}

		internal PhysicalDeviceService (ICustomLogger log, string? developerRoot, TimeSpan gracePeriod)
		{
			this.log = log ?? throw new ArgumentNullException (nameof (log));
			this.deviceCtl = new DeviceCtl (log, developerRoot: developerRoot);
			this.gracePeriod = gracePeriod;
		}

		/// <summary>
		/// <c>true</c> if the current host can run devicectl (macOS). On any other host
		/// <see cref="ListAsync(CancellationToken)"/> returns <see cref="PhysicalDeviceDiscoveryStatus.UnsupportedHost"/>.
		/// </summary>
		public static bool IsSupportedHost => RuntimeInformation.IsOSPlatform (OSPlatform.OSX);

		/// <summary>
		/// Lists the physical devices known to CoreDevice, waiting at most <see cref="DefaultTimeout"/>.
		/// </summary>
		/// <param name="cancellationToken">Cancels the operation and terminates devicectl.</param>
		/// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
		public Task<PhysicalDeviceDiscoveryResult> ListAsync (CancellationToken cancellationToken = default)
		{
			return ListAsync (DefaultTimeout, cancellationToken);
		}

		/// <summary>
		/// Lists the physical devices known to CoreDevice, waiting at most <paramref name="timeout"/>.
		/// If devicectl does not finish in time it is terminated
		/// and the result has status <see cref="PhysicalDeviceDiscoveryStatus.TimedOut"/>.
		/// </summary>
		/// <param name="timeout">The maximum time to wait. Must be positive.</param>
		/// <param name="cancellationToken">Cancels the operation and terminates devicectl.</param>
		/// <exception cref="ArgumentOutOfRangeException"><paramref name="timeout"/> is not positive or is too large.</exception>
		/// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> was cancelled.</exception>
		public async Task<PhysicalDeviceDiscoveryResult> ListAsync (TimeSpan timeout, CancellationToken cancellationToken = default)
		{
			if (timeout <= TimeSpan.Zero || timeout.TotalMilliseconds > int.MaxValue)
				throw new ArgumentOutOfRangeException (nameof (timeout), timeout, "The timeout must be positive and no longer than int.MaxValue milliseconds.");

			cancellationToken.ThrowIfCancellationRequested ();

			DeviceCtlRunResult run;
			using (var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource (cancellationToken)) {
				timeoutSource.CancelAfter (timeout);
				try {
					run = await RunBoundedAsync (timeoutSource.Token).ConfigureAwait (false);
				} catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested) {
					var seconds = timeout.TotalSeconds.ToString ("0.###", CultureInfo.InvariantCulture);
					return Fail (PhysicalDeviceDiscoveryStatus.TimedOut, $"devicectl did not finish within {seconds} seconds and was terminated.");
				}
			}

			cancellationToken.ThrowIfCancellationRequested ();
			return CreateResult (run);
		}

		/// <summary>
		/// Runs <c>xcrun devicectl list devices</c> and returns what it produced, without interpreting it. It is the only place
		/// <see cref="ListAsync(TimeSpan, CancellationToken)"/> starts a process, so code that uses this service can be tested
		/// without Xcode or a device by deriving from <see cref="PhysicalDeviceService"/> and returning canned output from here,
		/// the way the dotnet/macios tests override <c>ExecuteCtlAsync</c>. For example,
		/// <c>Task.FromResult (DeviceCtlRunResult.FromJson (json))</c>.
		/// </summary>
		/// <param name="cancellationToken">Cancelled when the caller cancels or the timeout elapses. An implementation that waits for
		/// something should stop promptly and throw <see cref="OperationCanceledException"/>; one that does not is abandoned after a short grace period.</param>
		/// <remarks>
		/// The default implementation returns <see cref="DeviceCtlRunResult.UnsupportedHost"/> without running anything on any host
		/// other than macOS. An override that returns canned output is not subject to that, so it works on every host.
		/// Exceptions other than <see cref="OperationCanceledException"/> are not caught: they surface from <see cref="ListAsync(TimeSpan, CancellationToken)"/>.
		/// An implementation must return its task promptly and do any waiting asynchronously. The timeout and the grace period
		/// bound the task that is returned, not a call that blocks the calling thread before it returns one.
		/// </remarks>
		protected virtual Task<DeviceCtlRunResult> ExecuteDeviceCtlAsync (CancellationToken cancellationToken)
		{
			if (!IsSupportedHost)
				return Task.FromResult (DeviceCtlRunResult.UnsupportedHost ());

			return deviceCtl.ListDevicesAsync (cancellationToken);
		}

		// The runner is expected to stop promptly when cancelled. Should it not, give it a short grace period and
		// then stop waiting for it, so that a hung runner can never block the caller beyond the requested timeout.
		async Task<DeviceCtlRunResult> RunBoundedAsync (CancellationToken token)
		{
			var run = ExecuteDeviceCtlAsync (token);

			var cancelled = new TaskCompletionSource<bool> (TaskCreationOptions.RunContinuationsAsynchronously);
			using (token.Register (() => cancelled.TrySetResult (true))) {
				if (await Task.WhenAny (run, cancelled.Task).ConfigureAwait (false) == run)
					return await run.ConfigureAwait (false);
			}

			using (var graceSource = new CancellationTokenSource ()) {
				var finished = await Task.WhenAny (run, Task.Delay (gracePeriod, graceSource.Token)).ConfigureAwait (false);
				graceSource.Cancel ();
				if (finished == run)
					return await run.ConfigureAwait (false);
			}

			log.LogWarning ("The devicectl runner did not stop within {0} seconds after cancellation and was abandoned.", gracePeriod.TotalSeconds);
			_ = run.ContinueWith (t => t.Exception, TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously);
			throw new OperationCanceledException (token);
		}

		PhysicalDeviceDiscoveryResult CreateResult (DeviceCtlRunResult run)
		{
			if (run.HostNotSupported)
				return Fail (PhysicalDeviceDiscoveryStatus.UnsupportedHost, "Physical device discovery requires macOS with Xcode installed (it runs 'xcrun devicectl').");

			if (run.ToolNotFound)
				return Fail (PhysicalDeviceDiscoveryStatus.ToolUnavailable, string.IsNullOrWhiteSpace (run.Message) ? "xcrun could not be started." : run.Message);

			var stderr = run.StdErr.Trim ();

			if (run.ExitCode != 0) {
				if (IsXcrunFailure (run, stderr))
					return Fail (PhysicalDeviceDiscoveryStatus.ToolUnavailable, stderr.Length > 0 ? stderr : $"xcrun could not run devicectl (exit code {run.ExitCode}).", run.ExitCode);

				var message = DeviceCtlOutputParser.GetErrorMessage (run.Json);
				if (string.IsNullOrEmpty (message))
					message = stderr.Length > 0 ? stderr : run.StdOut.Trim ();
				if (string.IsNullOrEmpty (message))
					message = $"devicectl exited with code {run.ExitCode}.";
				return Fail (PhysicalDeviceDiscoveryStatus.ExecutionFailed, message!, run.ExitCode);
			}

			// devicectl reports a failure as an error object. A document that has one is a failure even when the process
			// exited with code 0, and a device list next to it must not be mistaken for the result of a successful run.
			var reported = DeviceCtlOutputParser.GetErrorMessage (run.Json);
			if (reported is not null)
				return Fail (PhysicalDeviceDiscoveryStatus.ExecutionFailed, reported, run.ExitCode);

			if (!DeviceCtlOutputParser.TryParseDevices (run.Json, out var entries, out var parseError))
				return Fail (PhysicalDeviceDiscoveryStatus.InvalidOutput, parseError, run.ExitCode);

			var physical = entries.Where (d => !d.IsSimulated).ToList ();
			var devices = MergeByHardwareUdid (physical);
			log.LogInfo ("devicectl reported {0} entries: ignored {1} simulator(s), merged {2} physical entries into {3} device(s).", entries.Count, entries.Count - physical.Count, physical.Count, devices.Count);
			return new PhysicalDeviceDiscoveryResult (PhysicalDeviceDiscoveryStatus.Success, devices, exitCode: run.ExitCode);
		}

		PhysicalDeviceDiscoveryResult Fail (PhysicalDeviceDiscoveryStatus status, string message, int? exitCode = null)
		{
			// The failure is part of the result and the caller decides how loud to be: MSBuild tasks turn ICustomLogger
			// warnings into build warnings, which would fail builds that treat warnings as errors.
			log.LogInfo ("Physical device discovery failed ({0}): {1}", status, message);
			return new PhysicalDeviceDiscoveryResult (status, errorMessage: message, exitCode: exitCode);
		}

		static bool IsXcrunFailure (DeviceCtlRunResult run, string stderr)
		{
			// xcrun and xcode-select report problems locating or launching the tool with these messages, before devicectl runs.
			// Until the Xcode license is accepted xcrun refuses to run any tool and points at 'sudo xcodebuild -license'.
			if (Contains (stderr, "xcrun: error:") ||
				Contains (stderr, "xcode-select: error:") ||
				Contains (stderr, "no developer tools were found") ||
				Contains (stderr, "xcodebuild -license"))
				return true;

			// EX_OSFILE is the exit code xcrun uses when it cannot find the tool. devicectl itself
			// writes JSON for the failures it reports, so the absence of JSON points at xcrun.
			return run.ExitCode == 72 && string.IsNullOrWhiteSpace (run.Json);
		}

		static bool Contains (string text, string value) => text.IndexOf (value, StringComparison.OrdinalIgnoreCase) >= 0;

		/// <summary>
		/// Merges entries that describe the same physical device, identified by its hardware UDID (case-insensitive),
		/// into one device. Entries without a hardware UDID are never merged. The order of first appearance is kept.
		/// </summary>
		/// <remarks>
		/// The merged device takes its connection-level properties (identifier, transport and connection state)
		/// from its best connection: the most available one, preferring a wired transport over a network one.
		/// Properties the best connection did not report are filled in from the other entries,
		/// and every distinct connection is listed in <see cref="PhysicalDeviceInfo.Connections"/>, best first.
		/// The best entry is updated in place and returned; an entry that is not merged with any other is returned as is.
		/// </remarks>
		internal static List<PhysicalDeviceInfo> MergeByHardwareUdid (IEnumerable<PhysicalDeviceInfo> devices)
		{
			var groups = new List<List<PhysicalDeviceInfo>> ();
			var groupsByUdid = new Dictionary<string, List<PhysicalDeviceInfo>> (StringComparer.OrdinalIgnoreCase);

			foreach (var device in devices) {
				List<PhysicalDeviceInfo>? group;
				if (!device.HasHardwareUdid || !groupsByUdid.TryGetValue (device.HardwareUdid, out group)) {
					group = new List<PhysicalDeviceInfo> ();
					groups.Add (group);
					if (device.HasHardwareUdid)
						groupsByUdid [device.HardwareUdid] = group;
				}
				group.Add (device);
			}

			return groups.Select (Merge).ToList ();
		}

		static PhysicalDeviceInfo Merge (List<PhysicalDeviceInfo> entries)
		{
			if (entries.Count == 1)
				return entries [0];

			// OrderBy is stable, so entries that rank equally keep the order devicectl reported them in.
			var ranked = entries
				.OrderByDescending (d => d.Availability)
				.ThenByDescending (d => GetTransportRank (d.TransportType))
				.ToList ();

			var merged = ranked [0];
			for (var i = 1; i < ranked.Count; i++) {
				var other = ranked [i];
				merged.Name = Prefer (merged.Name, other.Name);
				merged.BuildVersion = Prefer (merged.BuildVersion, other.BuildVersion);
				merged.OSVersion = Prefer (merged.OSVersion, other.OSVersion);
				merged.DeviceClass = Prefer (merged.DeviceClass, other.DeviceClass);
				merged.HardwareModel = Prefer (merged.HardwareModel, other.HardwareModel);
				merged.Platform = Prefer (merged.Platform, other.Platform);
				merged.ProductType = Prefer (merged.ProductType, other.ProductType);
				merged.MarketingName = Prefer (merged.MarketingName, other.MarketingName);
				merged.SerialNumber = Prefer (merged.SerialNumber, other.SerialNumber);
				merged.CpuArchitecture = Prefer (merged.CpuArchitecture, other.CpuArchitecture);
				merged.Reality = Prefer (merged.Reality, other.Reality);
				// Pairing is a relationship between the host and the device, not with a transport.
				merged.PairingState = Prefer (merged.PairingState, other.PairingState);
				if (merged.UniqueChipID is null)
					merged.UniqueChipID = other.UniqueChipID;
				foreach (var connection in other.Connections) {
					if (!merged.Connections.Any (c => IsSameConnection (c, connection)))
						merged.Connections.Add (connection);
				}
			}

			return merged;
		}

		static bool IsSameConnection (PhysicalDeviceConnection a, PhysicalDeviceConnection b) =>
			string.Equals (a.Identifier, b.Identifier, StringComparison.OrdinalIgnoreCase) &&
			string.Equals (a.TransportType, b.TransportType, StringComparison.OrdinalIgnoreCase) &&
			string.Equals (a.PairingState, b.PairingState, StringComparison.OrdinalIgnoreCase) &&
			string.Equals (a.ConnectionState, b.ConnectionState, StringComparison.OrdinalIgnoreCase);

		static string Prefer (string current, string fallback) => string.IsNullOrEmpty (current) ? fallback : current;

		static int GetTransportRank (string transportType)
		{
			if (string.Equals (transportType, "wired", StringComparison.OrdinalIgnoreCase))
				return 2;
			if (string.Equals (transportType, "localNetwork", StringComparison.OrdinalIgnoreCase))
				return 1;
			return 0;
		}
	}
}
