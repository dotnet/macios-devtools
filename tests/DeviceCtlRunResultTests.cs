// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using NUnit.Framework;
using Xamarin.MacDev;

#nullable enable

namespace tests;

/// <summary>
/// Tests the factories of <see cref="DeviceCtlRunResult"/>, which code that depends on <see cref="PhysicalDeviceService"/>
/// uses to supply canned devicectl output.
/// </summary>
[TestFixture]
public class DeviceCtlRunResultTests {

	[Test]
	public void NotFound_DescribesAToolThatCouldNotBeStarted ()
	{
		var result = DeviceCtlRunResult.NotFound ("xcrun was not found.");

		Assert.Multiple (() => {
			Assert.That (result.ToolNotFound, Is.True);
			Assert.That (result.HostNotSupported, Is.False);
			Assert.That (result.Message, Is.EqualTo ("xcrun was not found."));
			Assert.That (result.ExitCode, Is.EqualTo (-1));
			Assert.That (result.StdOut, Is.Empty);
			Assert.That (result.StdErr, Is.Empty);
			Assert.That (result.Json, Is.Null);
		});
	}

	[Test]
	public void UnsupportedHost_DescribesAHostThatCannotRunDevicectl ()
	{
		var result = DeviceCtlRunResult.UnsupportedHost ();

		Assert.Multiple (() => {
			Assert.That (result.HostNotSupported, Is.True);
			Assert.That (result.ToolNotFound, Is.False);
			Assert.That (result.Message, Is.Empty);
			Assert.That (result.ExitCode, Is.EqualTo (-1));
			Assert.That (result.StdOut, Is.Empty);
			Assert.That (result.StdErr, Is.Empty);
			Assert.That (result.Json, Is.Null);
		});
	}

	[Test]
	public void Completed_KeepsWhatTheProcessProduced ()
	{
		var result = DeviceCtlRunResult.Completed (3, "some output", "some errors", "{}");

		Assert.Multiple (() => {
			Assert.That (result.ToolNotFound, Is.False);
			Assert.That (result.HostNotSupported, Is.False);
			Assert.That (result.Message, Is.Empty);
			Assert.That (result.ExitCode, Is.EqualTo (3));
			Assert.That (result.StdOut, Is.EqualTo ("some output"));
			Assert.That (result.StdErr, Is.EqualTo ("some errors"));
			Assert.That (result.Json, Is.EqualTo ("{}"));
		});
	}

	[Test]
	public void Completed_WithoutOutput_HasEmptyStreamsAndNoJson ()
	{
		var result = DeviceCtlRunResult.Completed (1, null, null, null);

		Assert.Multiple (() => {
			Assert.That (result.ExitCode, Is.EqualTo (1));
			Assert.That (result.StdOut, Is.Empty, "A missing stream must read as empty, never as null.");
			Assert.That (result.StdErr, Is.Empty, "A missing stream must read as empty, never as null.");
			Assert.That (result.Json, Is.Null);
		});
	}

	[Test]
	public void FromJson_IsASuccessfulRunThatWroteTheJson ()
	{
		var result = DeviceCtlRunResult.FromJson ("{\"result\":{\"devices\":[]}}");

		Assert.Multiple (() => {
			Assert.That (result.ToolNotFound, Is.False);
			Assert.That (result.HostNotSupported, Is.False);
			Assert.That (result.ExitCode, Is.EqualTo (0));
			Assert.That (result.StdOut, Is.Empty);
			Assert.That (result.StdErr, Is.Empty);
			Assert.That (result.Json, Is.EqualTo ("{\"result\":{\"devices\":[]}}"));
		});
	}

	[Test]
	public void FromJson_WithoutJson_IsASuccessfulRunThatWroteNoFile ()
	{
		var result = DeviceCtlRunResult.FromJson (null);

		Assert.Multiple (() => {
			Assert.That (result.ExitCode, Is.EqualTo (0));
			Assert.That (result.Json, Is.Null);
		});
	}
}
