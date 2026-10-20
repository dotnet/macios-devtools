// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.MacDev;
using Xamarin.MacDev.Models;
using static tests.DeviceCtlFixtures;
using static tests.FakeDeviceCtl;

#nullable enable

namespace tests;

/// <summary>
/// Tests how <see cref="PhysicalDeviceService"/> tells an empty device list apart from a missing tool,
/// a failing tool, a timeout, unusable output, an unsupported host and cancellation.
/// </summary>
[TestFixture]
public class PhysicalDeviceServiceFailureTests {

	static readonly TimeSpan Generous = TimeSpan.FromSeconds (30);

	RecordingLogger log = new RecordingLogger ();

	[SetUp]
	public void SetUp ()
	{
		log = new RecordingLogger ();
	}

	static void AssertFailure (PhysicalDeviceDiscoveryResult result, PhysicalDeviceDiscoveryStatus status)
	{
		Assert.That (result.Status, Is.EqualTo (status));
		Assert.That (result.IsSuccess, Is.False);
		Assert.That (result.Devices, Is.Empty, "A failed discovery must never look like a list of devices.");
		Assert.That (result.ErrorMessage, Is.Not.Empty);
	}

	#region Unsupported host

	[Test]
	public async Task ListAsync_UnsupportedHost_ReturnsUnsupportedHostWithoutRunningAnything ()
	{
		var invoked = false;
		var service = CreateService (log, _ => {
			invoked = true;
			return Task.FromResult (DeviceCtlRunResult.Completed (0, "", "", Document ()));
		}, supportedHost: false);

		var result = await service.ListAsync ();

		AssertFailure (result, PhysicalDeviceDiscoveryStatus.UnsupportedHost);
		Assert.That (invoked, Is.False);
		Assert.That (result.ExitCode, Is.Null);
		Assert.That (result.ErrorMessage, Does.Contain ("macOS"));
	}

	[Test]
	public void IsSupportedHost_IsTrueOnlyOnMacOS ()
	{
		Assert.That (PhysicalDeviceService.IsSupportedHost, Is.EqualTo (RuntimeInformation.IsOSPlatform (OSPlatform.OSX)));
	}

	[Test]
	[Platform (Exclude = "MacOsX")]
	public async Task ListAsync_RealServiceOnANonMacHost_ReturnsUnsupportedHost ()
	{
		var result = await new PhysicalDeviceService (log).ListAsync ();

		AssertFailure (result, PhysicalDeviceDiscoveryStatus.UnsupportedHost);
	}

	[Test]
	[Platform ("MacOsX")]
	public async Task ListAsync_RealDeviceCtl_ReturnsACoherentResult ()
	{
		// Runs the real devicectl. No device is needed: without one the list is simply empty.
		var result = await new PhysicalDeviceService (log).ListAsync (TimeSpan.FromSeconds (60));

		Assert.That (result.Status, Is.Not.EqualTo (PhysicalDeviceDiscoveryStatus.InvalidOutput), result.ErrorMessage);
		if (!result.IsSuccess)
			Assert.Inconclusive ($"devicectl is not usable on this machine: {result}");

		Assert.That (result.Devices.Where (d => d.IsSimulated), Is.Empty);
		Assert.That (result.Devices.Where (d => d.Udid.Length == 0), Is.Empty);
		Assert.That (result.Devices.Where (d => d.Connections.Count == 0), Is.Empty);
	}

	#endregion

	#region Tool unavailable

	[Test]
	public async Task ListAsync_ToolNotFound_ReturnsToolUnavailable ()
	{
		var service = CreateService (log, Returns (DeviceCtlRunResult.NotFound ("xcrun was not found at '/usr/bin/xcrun'.")));

		var result = await service.ListAsync ();

		AssertFailure (result, PhysicalDeviceDiscoveryStatus.ToolUnavailable);
		Assert.That (result.ErrorMessage, Is.EqualTo ("xcrun was not found at '/usr/bin/xcrun'."));
		Assert.That (result.ExitCode, Is.Null);
	}

	[TestCase (72, "xcrun: error: unable to find utility \"devicectl\", not a developer tool or in PATH")]
	[TestCase (1, "xcrun: error: invalid active developer path (/Library/Developer/CommandLineTools), missing xcrun at: /Library/Developer/CommandLineTools/usr/bin/xcrun")]
	[TestCase (1, "xcrun: error: missing DEVELOPER_DIR path: /nonexistent")]
	[TestCase (1, "xcode-select: error: tool 'devicectl' requires Xcode, but active developer directory '/Library/Developer/CommandLineTools' is a command line tools instance")]
	[TestCase (1, "No developer tools were found, requesting install.")]
	[TestCase (1, "XCRUN: ERROR: something else")]
	public async Task ListAsync_XcrunCannotRunDeviceCtl_ReturnsToolUnavailable (int exitCode, string stderr)
	{
		var service = CreateService (log, ReturnsJson (null, exitCode: exitCode, stderr: stderr + "\n"));

		var result = await service.ListAsync ();

		AssertFailure (result, PhysicalDeviceDiscoveryStatus.ToolUnavailable);
		Assert.That (result.ErrorMessage, Is.EqualTo (stderr));
		Assert.That (result.ExitCode, Is.EqualTo (exitCode));
	}

	[Test]
	public async Task ListAsync_ExitCode72WithoutJsonOrMessage_ReturnsToolUnavailable ()
	{
		var service = CreateService (log, ReturnsJson (null, exitCode: 72));

		var result = await service.ListAsync ();

		AssertFailure (result, PhysicalDeviceDiscoveryStatus.ToolUnavailable);
		Assert.That (result.ErrorMessage, Does.Contain ("72"));
	}

	[Test]
	public async Task ListAsync_ExitCode72WithADeviceCtlError_IsAnExecutionFailure ()
	{
		// devicectl wrote an error document, so it did run: this is not a missing tool.
		var service = CreateService (log, ReturnsJson (ErrorDocument ("Something went wrong."), exitCode: 72));

		var result = await service.ListAsync ();

		AssertFailure (result, PhysicalDeviceDiscoveryStatus.ExecutionFailed);
		Assert.That (result.ErrorMessage, Is.EqualTo ("Something went wrong."));
	}

	#endregion

	#region Execution failure

	[Test]
	public async Task ListAsync_DeviceCtlReportsAnError_ReturnsExecutionFailedWithItsMessage ()
	{
		var service = CreateService (log, ReturnsJson (ErrorDocument ("CoreDevice is not available."), exitCode: 1, stderr: "ERROR: CoreDevice is not available. (com.apple.dt.CoreDeviceError error 1000.)\n"));

		var result = await service.ListAsync ();

		AssertFailure (result, PhysicalDeviceDiscoveryStatus.ExecutionFailed);
		Assert.That (result.ErrorMessage, Is.EqualTo ("CoreDevice is not available."));
		Assert.That (result.ExitCode, Is.EqualTo (1));
	}

	[Test]
	public async Task ListAsync_FailureWithoutJson_UsesTrimmedStandardError ()
	{
		var service = CreateService (log, ReturnsJson (null, exitCode: 64, stderr: "  ERROR: Invalid filter.\n"));

		var result = await service.ListAsync ();

		AssertFailure (result, PhysicalDeviceDiscoveryStatus.ExecutionFailed);
		Assert.That (result.ErrorMessage, Is.EqualTo ("ERROR: Invalid filter."));
		Assert.That (result.ExitCode, Is.EqualTo (64));
	}

	[Test]
	public async Task ListAsync_FailureWithOnlyStandardOutput_UsesStandardOutput ()
	{
		var service = CreateService (log, ReturnsJson (null, exitCode: 2, stdout: "Something failed\n"));

		var result = await service.ListAsync ();

		AssertFailure (result, PhysicalDeviceDiscoveryStatus.ExecutionFailed);
		Assert.That (result.ErrorMessage, Is.EqualTo ("Something failed"));
	}

	[Test]
	public async Task ListAsync_FailureWithoutAnyOutput_MentionsTheExitCode ()
	{
		var service = CreateService (log, ReturnsJson (null, exitCode: 3));

		var result = await service.ListAsync ();

		AssertFailure (result, PhysicalDeviceDiscoveryStatus.ExecutionFailed);
		Assert.That (result.ErrorMessage, Is.EqualTo ("devicectl exited with code 3."));
	}

	[Test]
	public async Task ListAsync_ErrorDocumentWithExitCodeZero_ReturnsExecutionFailed ()
	{
		var service = CreateService (log, ReturnsJson (ErrorDocument ("Operation failed."), exitCode: 0));

		var result = await service.ListAsync ();

		AssertFailure (result, PhysicalDeviceDiscoveryStatus.ExecutionFailed);
		Assert.That (result.ErrorMessage, Is.EqualTo ("Operation failed."));
		Assert.That (result.ExitCode, Is.EqualTo (0));
	}

	#endregion

	#region Invalid output

	[TestCase (null)]
	[TestCase ("")]
	[TestCase ("   \n")]
	[TestCase ("this is not json")]
	[TestCase ("{ \"result\": { \"devices\": [ ")]
	[TestCase ("[]")]
	[TestCase ("{}")]
	[TestCase ("{\"info\":{\"jsonVersion\":5}}")]
	[TestCase ("{\"result\":{}}")]
	[TestCase ("{\"result\":[]}")]
	[TestCase ("{\"result\":{\"devices\":{}}}")]
	[TestCase ("{\"result\":{\"devices\":[42]}}")]
	public async Task ListAsync_OutputThatIsNotADeviceList_ReturnsInvalidOutput (string? json)
	{
		var service = CreateService (log, ReturnsJson (json));

		var result = await service.ListAsync ();

		AssertFailure (result, PhysicalDeviceDiscoveryStatus.InvalidOutput);
		Assert.That (result.ExitCode, Is.EqualTo (0));
	}

	[Test]
	public async Task ListAsync_EmptyDeviceList_IsNotConfusedWithInvalidOutput ()
	{
		var empty = await CreateService (log, ReturnsJson (Document ())).ListAsync ();
		var invalid = await CreateService (log, ReturnsJson ("")).ListAsync ();

		Assert.That (empty.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success));
		Assert.That (invalid.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.InvalidOutput));
	}

	#endregion

	#region Timeout

	[Test]
	public async Task ListAsync_RunnerExceedsTheTimeout_ReturnsTimedOutAndCancelsTheRunner ()
	{
		var observed = default (CancellationToken);
		var service = CreateService (log, token => {
			observed = token;
			return WaitUntilCancelled (token);
		});

		var stopwatch = Stopwatch.StartNew ();
		var result = await service.ListAsync (TimeSpan.FromMilliseconds (100));

		AssertFailure (result, PhysicalDeviceDiscoveryStatus.TimedOut);
		Assert.That (result.ErrorMessage, Does.Contain ("did not finish within 0.1 seconds"));
		Assert.That (result.ExitCode, Is.Null);
		Assert.That (observed.IsCancellationRequested, Is.True, "The runner has to be told to stop.");
		Assert.That (stopwatch.Elapsed, Is.LessThan (Generous));
	}

	[Test]
	public async Task ListAsync_RunnerThatIgnoresCancellation_IsAbandonedAfterTheGracePeriod ()
	{
		var hung = new TaskCompletionSource<DeviceCtlRunResult> ();
		var service = CreateService (log, _ => hung.Task, gracePeriod: TimeSpan.FromMilliseconds (100));

		var stopwatch = Stopwatch.StartNew ();
		var result = await service.ListAsync (TimeSpan.FromMilliseconds (100));

		AssertFailure (result, PhysicalDeviceDiscoveryStatus.TimedOut);
		Assert.That (stopwatch.Elapsed, Is.LessThan (Generous), "A hung runner must not block the caller.");
		Assert.That (log.Warnings, Has.Some.Contain ("abandoned"));

		// Whatever the abandoned runner does later must not surface anywhere.
		hung.SetException (new InvalidOperationException ("late failure"));
	}

	[Test]
	public async Task ListAsync_RunnerThatFinishesDuringTheGracePeriod_HasItsResultUsed ()
	{
		var finished = DeviceCtlRunResult.Completed (0, "", "", Document (Device ("COREDEVICE-1", udid: IPhoneUdid)));
		var service = CreateService (log, async token => {
			try {
				await Task.Delay (Timeout.Infinite, token).ConfigureAwait (false);
			} catch (OperationCanceledException) {
			}
			await Task.Delay (20).ConfigureAwait (false);
			return finished;
		}, gracePeriod: Generous);

		var result = await service.ListAsync (TimeSpan.FromMilliseconds (100));

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success));
		Assert.That (result.Devices, Has.Count.EqualTo (1));
	}

	[Test]
	public async Task ListAsync_RunnerThatThrowsAfterTheTimeout_ReturnsTimedOut ()
	{
		var service = CreateService (log, async token => {
			await Task.Delay (Timeout.Infinite, token).ConfigureAwait (false);
			return DeviceCtlRunResult.NotFound ("unreachable");
		});

		var result = await service.ListAsync (TimeSpan.FromMilliseconds (50));

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.TimedOut));
	}

	[Test]
	public async Task ListAsync_RunnerThatFinishesInTime_IsNotTimedOut ()
	{
		var service = CreateService (log, ReturnsJson (Document ()));

		var result = await service.ListAsync (TimeSpan.FromMinutes (5));

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success));
	}

	[Test]
	public void DefaultTimeout_IsBounded ()
	{
		Assert.That (PhysicalDeviceService.DefaultTimeout, Is.GreaterThan (TimeSpan.Zero));
		Assert.That (PhysicalDeviceService.DefaultTimeout, Is.LessThanOrEqualTo (TimeSpan.FromMinutes (5)));
	}

	#endregion

	#region Cancellation

	[Test]
	public void ListAsync_TokenAlreadyCancelled_ThrowsWithoutRunningAnything ()
	{
		using var cts = new CancellationTokenSource ();
		cts.Cancel ();
		var invoked = false;
		var service = CreateService (log, _ => {
			invoked = true;
			return Task.FromResult (DeviceCtlRunResult.Completed (0, "", "", Document ()));
		});

		Assert.CatchAsync<OperationCanceledException> (async () => await service.ListAsync (cts.Token));
		Assert.That (invoked, Is.False);
	}

	[Test]
	public async Task ListAsync_CancelledWhileRunning_ThrowsAndTellsTheRunnerToStop ()
	{
		using var cts = new CancellationTokenSource ();
		var started = new TaskCompletionSource<bool> ();
		var observed = default (CancellationToken);
		var service = CreateService (log, token => {
			observed = token;
			started.SetResult (true);
			return WaitUntilCancelled (token);
		});

		var pending = service.ListAsync (Generous, cts.Token);
		await started.Task;
		cts.Cancel ();

		Assert.CatchAsync<OperationCanceledException> (async () => await pending);
		Assert.That (observed.IsCancellationRequested, Is.True);
	}

	[Test]
	public void ListAsync_CancelledWhileRunning_IsNotReportedAsATimeout ()
	{
		using var cts = new CancellationTokenSource ();
		var service = CreateService (log, async token => {
			cts.Cancel ();
			await Task.Delay (Timeout.Infinite, token).ConfigureAwait (false);
			return DeviceCtlRunResult.NotFound ("unreachable");
		});

		Assert.CatchAsync<OperationCanceledException> (async () => await service.ListAsync (TimeSpan.FromMilliseconds (50), cts.Token));
	}

	[Test]
	public void ListAsync_CancelledWhileARunnerThatIgnoresCancellationHangs_StillThrows ()
	{
		using var cts = new CancellationTokenSource ();
		var hung = new TaskCompletionSource<DeviceCtlRunResult> ();
		var service = CreateService (log, _ => {
			cts.Cancel ();
			return hung.Task;
		}, gracePeriod: TimeSpan.FromMilliseconds (50));

		Assert.CatchAsync<OperationCanceledException> (async () => await service.ListAsync (Generous, cts.Token));
	}

	[Test]
	public void ListAsync_CancelledAsTheRunnerCompletes_Throws ()
	{
		using var cts = new CancellationTokenSource ();
		var service = CreateService (log, _ => {
			cts.Cancel ();
			return Task.FromResult (DeviceCtlRunResult.Completed (0, "", "", Document ()));
		});

		Assert.CatchAsync<OperationCanceledException> (async () => await service.ListAsync (cts.Token));
	}

	#endregion

	#region Arguments

	[Test]
	public void ListAsync_TimeoutThatIsNotPositive_Throws ()
	{
		var service = CreateService (log, ReturnsJson (Document ()));

		Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await service.ListAsync (TimeSpan.Zero));
		Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await service.ListAsync (TimeSpan.FromSeconds (-1)));
		Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await service.ListAsync (Timeout.InfiniteTimeSpan));
	}

	[Test]
	public void ListAsync_TimeoutThatIsTooLarge_Throws ()
	{
		var service = CreateService (log, ReturnsJson (Document ()));

		Assert.ThrowsAsync<ArgumentOutOfRangeException> (async () => await service.ListAsync (TimeSpan.FromMilliseconds ((double) int.MaxValue + 1)));
	}

	[Test]
	public void Constructor_NullArguments_Throw ()
	{
		var runner = ReturnsJson (Document ());

		Assert.Throws<ArgumentNullException> (() => new PhysicalDeviceService (null!));
		Assert.Throws<ArgumentNullException> (() => new PhysicalDeviceService (null!, runner, () => true, ShortGracePeriod));
		Assert.Throws<ArgumentNullException> (() => new PhysicalDeviceService (log, null!, () => true, ShortGracePeriod));
		Assert.Throws<ArgumentNullException> (() => new PhysicalDeviceService (log, runner, null!, ShortGracePeriod));
	}

	#endregion

	#region Logging

	[Test]
	public async Task ListAsync_WritesNothingToStandardOutput ()
	{
		var originalOut = Console.Out;
		var originalError = Console.Error;
		using var stdout = new StringWriter ();
		using var stderr = new StringWriter ();
		Console.SetOut (stdout);
		Console.SetError (stderr);
		try {
			await CreateService (log, ReturnsJson (Document (Device ("COREDEVICE-1", udid: IPhoneUdid), Device ("SIM-1", udid: "SIM-1", reality: "simulated")))).ListAsync ();
			await CreateService (log, ReturnsJson (null, exitCode: 1, stderr: "boom")).ListAsync ();
			await CreateService (log, ReturnsJson ("garbage")).ListAsync ();
			await CreateService (log, Returns (DeviceCtlRunResult.NotFound ("xcrun was not found.")), supportedHost: false).ListAsync ();
			await CreateService (log, WaitUntilCancelled).ListAsync (TimeSpan.FromMilliseconds (50));
		} finally {
			Console.SetOut (originalOut);
			Console.SetError (originalError);
		}

		Assert.That (stdout.ToString (), Is.Empty);
		Assert.That (stderr.ToString (), Is.Empty);
		Assert.That (log.Messages, Is.Not.Empty);
	}

	[Test]
	public async Task ListAsync_Failure_IsLoggedAsAWarningWithTheStatusAndMessage ()
	{
		await CreateService (log, ReturnsJson (null, exitCode: 64, stderr: "ERROR: Invalid filter.")).ListAsync ();

		Assert.That (log.Warnings, Has.Count.EqualTo (1));
		Assert.That (log.Warnings [0], Does.Contain ("ExecutionFailed").And.Contain ("ERROR: Invalid filter."));
	}

	[Test]
	public async Task ListAsync_MessagesThatLookLikeFormatStrings_AreLoggedVerbatim ()
	{
		await CreateService (log, ReturnsJson (null, exitCode: 1, stderr: "failed {0} {unclosed")).ListAsync ();

		Assert.That (log.Warnings, Has.Some.Contain ("failed {0} {unclosed"));
	}

	[Test]
	public async Task ListAsync_FailureResult_DescribesItself ()
	{
		var failure = await CreateService (log, ReturnsJson (null, exitCode: 64, stderr: "bad")).ListAsync ();
		var success = await CreateService (log, ReturnsJson (Document (Device ("COREDEVICE-1", udid: IPhoneUdid)))).ListAsync ();

		Assert.That (failure.ToString (), Is.EqualTo ("ExecutionFailed: bad"));
		Assert.That (success.ToString (), Is.EqualTo ("Success (1 devices)"));
	}

	#endregion
}
