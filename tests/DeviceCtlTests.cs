// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;
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
/// Tests the default devicectl runner against a fake <c>xcrun</c> shell script, so the real command line,
/// the temporary JSON file, failure capture and process-tree cleanup are exercised without Xcode or a device.
/// </summary>
[TestFixture]
public class DeviceCtlTests {

	static readonly TimeSpan Generous = TimeSpan.FromSeconds (30);

	string directory = "";
	string xcrun = "";
	RecordingLogger log = new RecordingLogger ();

	[SetUp]
	public void SetUp ()
	{
		log = new RecordingLogger ();
		directory = Path.Combine (Path.GetTempPath (), "devicectl-tests-" + Path.GetRandomFileName ());
		Directory.CreateDirectory (directory);
		xcrun = InDirectory ("xcrun");
	}

	[TearDown]
	public void TearDown ()
	{
		try {
			Directory.Delete (directory, recursive: true);
		} catch (IOException) {
			// Best effort: it is a temporary directory.
		} catch (UnauthorizedAccessException) {
		}
	}

	string InDirectory (string name) => Path.Combine (directory, name);

	/// <summary>
	/// Writes a fake xcrun. Before the given <paramref name="body"/> runs, it records its arguments in
	/// <c>args.txt</c> and the <c>--json-output</c> path in <c>output-path.txt</c>, and leaves that path in <c>$out</c>.
	/// </summary>
	void WriteXcrun (bool executable, params string [] body)
	{
		var preamble = new [] {
			"#!/bin/sh",
			$"dir='{directory}'",
			"printf '%s\\n' \"$@\" > \"$dir/args.txt\"",
			"out=''",
			"while [ $# -gt 0 ]; do",
			"  if [ \"$1\" = '--json-output' ]; then out=\"$2\"; fi",
			"  shift",
			"done",
			"printf '%s' \"$out\" > \"$dir/output-path.txt\"",
		};
		File.WriteAllText (xcrun, string.Join ("\n", preamble.Concat (body)) + "\n");
		if (executable)
			ProcessTestHelper.MakeExecutable (xcrun);
	}

	void WriteXcrun (params string [] body) => WriteXcrun (true, body);

	/// <summary>A fake xcrun whose devicectl succeeds and writes <paramref name="json"/> to its JSON output file.</summary>
	void WriteXcrunThatWrites (string json, int exitCode = 0)
	{
		File.WriteAllText (InDirectory ("payload.json"), json);
		WriteXcrun (
			"cp \"$dir/payload.json\" \"$out\"",
			"[ -f \"$out\" ] && echo yes > \"$dir/output-existed.txt\"",
			"echo 'to stdout'",
			"echo 'to stderr' >&2",
			$"exit {exitCode}");
	}

	/// <summary>A fake xcrun that starts a long-running child, records both process ids and waits.</summary>
	void WriteXcrunThatHangs () => WriteXcrun (
		": > \"$out\"",
		"sleep 60 &",
		"echo $! > \"$dir/child.pid\"",
		"echo $$ > \"$dir/sh.pid\"",
		"wait");

	string ReadRecorded (string name) => File.ReadAllText (InDirectory (name));

	#region Tool not found

	[Test]
	public async Task ListDevicesAsync_XcrunMissing_ReportsToolNotFound ()
	{
		var missing = InDirectory ("no-such-xcrun");

		var result = await new DeviceCtl (log, missing).ListDevicesAsync (CancellationToken.None);

		Assert.That (result.ToolNotFound, Is.True);
		Assert.That (result.Message, Does.Contain (missing));
		Assert.That (result.ExitCode, Is.EqualTo (-1));
		Assert.That (result.Json, Is.Null);
		Assert.That (log.Messages, Has.Some.Contain (missing));
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListDevicesAsync_XcrunCannotBeStarted_ReportsToolNotFound ()
	{
		WriteXcrun (false, "exit 0");

		var result = await new DeviceCtl (log, xcrun).ListDevicesAsync (CancellationToken.None);

		Assert.That (result.ToolNotFound, Is.True);
		Assert.That (result.Message, Does.Contain ("could not be started"));
		Assert.That (File.Exists (InDirectory ("args.txt")), Is.False);
	}

	[Test]
	public void Constructor_NullLogger_Throws ()
	{
		Assert.Throws<ArgumentNullException> (() => new DeviceCtl (null!));
	}

	#endregion

	#region Command line and output

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListDevicesAsync_RunsDeviceCtlListDevicesWritingJsonToATemporaryFile ()
	{
		WriteXcrunThatWrites (Document ());

		await new DeviceCtl (log, xcrun).ListDevicesAsync (CancellationToken.None);

		var arguments = ReadRecorded ("args.txt").Split ('\n').Where (a => a.Length > 0).ToArray ();
		var jsonPath = ReadRecorded ("output-path.txt");
		Assert.That (arguments, Is.EqualTo (new [] { "devicectl", "list", "devices", "--json-output", jsonPath }));
		Assert.That (Path.GetDirectoryName (jsonPath), Is.EqualTo (Path.GetDirectoryName (Path.Combine (Path.GetTempPath (), "x"))));
		Assert.That (Path.GetExtension (jsonPath), Is.EqualTo (".json"));
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListDevicesAsync_ReturnsWhatDeviceCtlWrote ()
	{
		var json = Document (Device (IPhoneIdentifier, udid: IPhoneUdid));
		WriteXcrunThatWrites (json);

		var result = await new DeviceCtl (log, xcrun).ListDevicesAsync (CancellationToken.None);

		Assert.That (result.ToolNotFound, Is.False);
		Assert.That (result.ExitCode, Is.EqualTo (0));
		Assert.That (result.Json, Is.EqualTo (json));
		Assert.That (result.StdOut, Is.EqualTo ("to stdout\n"));
		Assert.That (result.StdErr, Is.EqualTo ("to stderr\n"));
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListDevicesAsync_DeletesTheTemporaryJsonFile ()
	{
		WriteXcrunThatWrites (Document ());

		await new DeviceCtl (log, xcrun).ListDevicesAsync (CancellationToken.None);

		Assert.That (File.Exists (InDirectory ("output-existed.txt")), Is.True, "devicectl should have been able to write the file.");
		Assert.That (File.Exists (ReadRecorded ("output-path.txt")), Is.False, "The temporary file must not be left behind.");
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListDevicesAsync_UsesADifferentTemporaryFileEachTime ()
	{
		WriteXcrunThatWrites (Document ());
		var deviceCtl = new DeviceCtl (log, xcrun);

		await deviceCtl.ListDevicesAsync (CancellationToken.None);
		var first = ReadRecorded ("output-path.txt");
		await deviceCtl.ListDevicesAsync (CancellationToken.None);
		var second = ReadRecorded ("output-path.txt");

		Assert.That (second, Is.Not.EqualTo (first));
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListDevicesAsync_FailureWithoutJson_CapturesExitCodeAndOutput ()
	{
		WriteXcrun ("echo 'ERROR: boom' >&2", "exit 3");

		var result = await new DeviceCtl (log, xcrun).ListDevicesAsync (CancellationToken.None);

		Assert.That (result.ToolNotFound, Is.False);
		Assert.That (result.ExitCode, Is.EqualTo (3));
		Assert.That (result.StdErr, Is.EqualTo ("ERROR: boom\n"));
		Assert.That (result.Json, Is.Null);
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListDevicesAsync_FailureWithErrorJson_ReturnsTheJson ()
	{
		var json = ErrorDocument ("CoreDevice is not available.");
		WriteXcrunThatWrites (json, exitCode: 1);

		var result = await new DeviceCtl (log, xcrun).ListDevicesAsync (CancellationToken.None);

		Assert.That (result.ExitCode, Is.EqualTo (1));
		Assert.That (result.Json, Is.EqualTo (json));
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListDevicesAsync_LogsTheCommandThroughTheLogger ()
	{
		WriteXcrunThatWrites (Document ());

		await new DeviceCtl (log, xcrun).ListDevicesAsync (CancellationToken.None);

		Assert.That (log.Messages, Has.Some.Contain ("devicectl list devices --json-output"));
		Assert.That (log.Warnings, Is.Empty);
	}

	#endregion

	#region Cancellation

	[Test]
	[Platform (Exclude = "Win")]
	public void ListDevicesAsync_TokenAlreadyCancelled_ThrowsWithoutStartingAProcess ()
	{
		WriteXcrunThatWrites (Document ());
		using var cts = new CancellationTokenSource ();
		cts.Cancel ();

		Assert.CatchAsync<OperationCanceledException> (async () => await new DeviceCtl (log, xcrun).ListDevicesAsync (cts.Token));
		Assert.That (File.Exists (InDirectory ("args.txt")), Is.False);
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListDevicesAsync_Cancelled_KillsTheProcessTreeAndDeletesTheTemporaryFile ()
	{
		WriteXcrunThatHangs ();
		using var cts = new CancellationTokenSource ();

		var pending = new DeviceCtl (log, xcrun).ListDevicesAsync (cts.Token);
		var shellPid = await ProcessTestHelper.WaitForPidAsync (InDirectory ("sh.pid"), Generous);
		var childPid = await ProcessTestHelper.WaitForPidAsync (InDirectory ("child.pid"), Generous);
		Assert.That (ProcessTestHelper.IsAlive (shellPid), Is.True, "The fake xcrun should be running.");
		Assert.That (ProcessTestHelper.IsAlive (childPid), Is.True, "The process it started should be running.");

		cts.Cancel ();

		Assert.CatchAsync<OperationCanceledException> (async () => await pending);
		Assert.That (await ProcessTestHelper.WaitUntilGoneAsync (shellPid, Generous), Is.True, "xcrun was not killed.");
		Assert.That (await ProcessTestHelper.WaitUntilGoneAsync (childPid, Generous), Is.True, "A process started by xcrun was left behind.");
		Assert.That (File.Exists (ReadRecorded ("output-path.txt")), Is.False, "The temporary file must not be left behind.");
	}

	#endregion

	#region Through PhysicalDeviceService

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListAsync_WithFakeXcrun_ReturnsTheDevicesWithoutSimulators ()
	{
		WriteXcrunThatWrites (Document (
			Device ("SIM-1", udid: "SIM-1", reality: "simulated", transportType: "sameMachine"),
			Device (IPhoneIdentifier, udid: IPhoneUdid, name: "Test iPhone"),
			Device (IPadIdentifier, udid: IPadUdid, name: "Test iPad", deviceType: "iPad", productType: "iPad14,3", connectionState: "disconnected")));
		var service = CreateService (log, new DeviceCtl (log, xcrun).ListDevicesAsync);

		var result = await service.ListAsync ();

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success), result.ErrorMessage);
		Assert.That (result.Devices.Select (d => d.Name), Is.EqualTo (new [] { "Test iPhone", "Test iPad" }));
		Assert.That (result.Devices.Select (d => d.Kind), Is.EqualTo (new [] { PhysicalDeviceKind.IPhone, PhysicalDeviceKind.IPad }));
		Assert.That (result.Devices.Select (d => d.Availability), Is.EqualTo (new [] { PhysicalDeviceAvailability.Connected, PhysicalDeviceAvailability.Available }));
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListAsync_WithFakeXcrunThatListsNothing_ReturnsSuccessWithEmptyList ()
	{
		WriteXcrunThatWrites (Document ());
		var service = CreateService (log, new DeviceCtl (log, xcrun).ListDevicesAsync);

		var result = await service.ListAsync ();

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success), result.ErrorMessage);
		Assert.That (result.Devices, Is.Empty);
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListAsync_WithFakeXcrunThatFails_ReturnsExecutionFailed ()
	{
		WriteXcrunThatWrites (ErrorDocument ("CoreDevice is not available."), exitCode: 1);
		var service = CreateService (log, new DeviceCtl (log, xcrun).ListDevicesAsync);

		var result = await service.ListAsync ();

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.ExecutionFailed));
		Assert.That (result.ErrorMessage, Is.EqualTo ("CoreDevice is not available."));
		Assert.That (result.ExitCode, Is.EqualTo (1));
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListAsync_WithFakeXcrunThatCannotFindDeviceCtl_ReturnsToolUnavailable ()
	{
		WriteXcrun (
			"echo 'xcrun: error: unable to find utility \"devicectl\", not a developer tool or in PATH' >&2",
			"exit 72");
		var service = CreateService (log, new DeviceCtl (log, xcrun).ListDevicesAsync);

		var result = await service.ListAsync ();

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.ToolUnavailable));
		Assert.That (result.ErrorMessage, Does.StartWith ("xcrun: error: unable to find utility"));
		Assert.That (result.ExitCode, Is.EqualTo (72));
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListAsync_WithFakeXcrunThatWritesGarbage_ReturnsInvalidOutput ()
	{
		WriteXcrunThatWrites ("this is not json");
		var service = CreateService (log, new DeviceCtl (log, xcrun).ListDevicesAsync);

		var result = await service.ListAsync ();

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.InvalidOutput));
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListAsync_WithFakeXcrunThatNeverWritesJson_ReturnsInvalidOutput ()
	{
		WriteXcrun ("exit 0");
		var service = CreateService (log, new DeviceCtl (log, xcrun).ListDevicesAsync);

		var result = await service.ListAsync ();

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.InvalidOutput));
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListAsync_WithFakeXcrun_WritesNothingToStandardOutput ()
	{
		WriteXcrunThatWrites (Document (Device (IPhoneIdentifier, udid: IPhoneUdid)));
		var service = CreateService (log, new DeviceCtl (log, xcrun).ListDevicesAsync);
		var originalOut = Console.Out;
		var originalError = Console.Error;
		using var stdout = new StringWriter ();
		using var stderr = new StringWriter ();
		Console.SetOut (stdout);
		Console.SetError (stderr);
		try {
			await service.ListAsync ();
		} finally {
			Console.SetOut (originalOut);
			Console.SetError (originalError);
		}

		Assert.That (stdout.ToString (), Is.Empty);
		Assert.That (stderr.ToString (), Is.Empty);
		Assert.That (log.Messages, Is.Not.Empty);
	}

	[Test]
	[Platform (Exclude = "Win")]
	public async Task ListAsync_WithFakeXcrunThatHangs_TimesOutAndKillsTheProcessTree ()
	{
		WriteXcrunThatHangs ();
		var service = CreateService (log, new DeviceCtl (log, xcrun).ListDevicesAsync, gracePeriod: Generous);

		var result = await service.ListAsync (TimeSpan.FromSeconds (3));

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.TimedOut), result.ErrorMessage);
		var shellPid = ProcessTestHelper.TryReadPid (InDirectory ("sh.pid"));
		var childPid = ProcessTestHelper.TryReadPid (InDirectory ("child.pid"));
		if (shellPid is null || childPid is null)
			Assert.Inconclusive ("The fake xcrun was killed before it started its child process; the machine is too slow to tell.");

		Assert.That (await ProcessTestHelper.WaitUntilGoneAsync (shellPid!.Value, Generous), Is.True, "xcrun was not killed.");
		Assert.That (await ProcessTestHelper.WaitUntilGoneAsync (childPid!.Value, Generous), Is.True, "A process started by xcrun was left behind.");
		Assert.That (File.Exists (ReadRecorded ("output-path.txt")), Is.False, "The temporary file must not be left behind.");
	}

	#endregion
}
