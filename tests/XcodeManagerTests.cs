// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.MacDev;

namespace tests;

[TestFixture]
public class XcodeManagerTests {

	[Test]
	public void ParseMdfindOutput_ParsesMultiplePaths ()
	{
		var output = "/Applications/Xcode.app\n/Applications/Xcode-beta.app\n";
		var paths = XcodeManager.ParseMdfindOutput (output);
		Assert.That (paths.Count, Is.EqualTo (2));
		Assert.That (paths [0], Is.EqualTo ("/Applications/Xcode.app"));
		Assert.That (paths [1], Is.EqualTo ("/Applications/Xcode-beta.app"));
	}

	[Test]
	public void ParseMdfindOutput_IgnoresBlankLines ()
	{
		var output = "/Applications/Xcode.app\n\n\n/Applications/Xcode-beta.app\n\n";
		var paths = XcodeManager.ParseMdfindOutput (output);
		Assert.That (paths.Count, Is.EqualTo (2));
	}

	[Test]
	public void ParseMdfindOutput_ReturnsEmptyForNullOrEmpty ()
	{
		Assert.That (XcodeManager.ParseMdfindOutput (""), Is.Empty);
		Assert.That (XcodeManager.ParseMdfindOutput ((string) null), Is.Empty);
	}

	[Test]
	public void ParseMdfindOutput_HandlesWindowsLineEndings ()
	{
		var output = "/Applications/Xcode.app\r\n/Applications/Xcode-beta.app\r\n";
		var paths = XcodeManager.ParseMdfindOutput (output);
		Assert.That (paths.Count, Is.EqualTo (2));
		Assert.That (paths [0], Is.EqualTo ("/Applications/Xcode.app"));
	}

	[Test]
	public void ParseMdfindOutput_TrimsWhitespace ()
	{
		var output = "  /Applications/Xcode.app  \n  /Applications/Xcode-beta.app  \n";
		var paths = XcodeManager.ParseMdfindOutput (output);
		Assert.That (paths [0], Is.EqualTo ("/Applications/Xcode.app"));
		Assert.That (paths [1], Is.EqualTo ("/Applications/Xcode-beta.app"));
	}

	[Test]
	public void CanonicalizeXcodePath_ReturnsNullForNullOrEmpty ()
	{
		Assert.That (XcodeManager.CanonicalizeXcodePath (null), Is.Null);
		Assert.That (XcodeManager.CanonicalizeXcodePath (""), Is.Null);
	}

	[Test]
	public void List_DoesNotThrow ()
	{
		var mgr = new XcodeManager (ConsoleLogger.Instance);
		List<Xamarin.MacDev.Models.XcodeInfo> results = null;
		Assert.DoesNotThrow (() => results = mgr.List ());
		Assert.That (results, Is.Not.Null);
	}

	[Test]
	public void GetSelected_DoesNotThrow ()
	{
		var mgr = new XcodeManager (ConsoleLogger.Instance);
		Assert.DoesNotThrow (() => mgr.GetSelected ());
	}

	[Test]
	public void GetBest_DoesNotThrow ()
	{
		var mgr = new XcodeManager (ConsoleLogger.Instance);
		Assert.DoesNotThrow (() => mgr.GetBest ());
	}
}
