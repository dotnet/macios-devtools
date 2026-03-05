// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using NUnit.Framework;
using Xamarin.MacDev;
using Xamarin.MacDev.Models;

namespace tests;

[TestFixture]
public class EnvironmentCheckerTests {

	[Test]
	public void Constructor_ThrowsOnNullLogger ()
	{
		Assert.Throws<ArgumentNullException> (() => new EnvironmentChecker (null!));
	}

	[Test]
	[Platform ("MacOsX")]
	public void Check_DoesNotThrow ()
	{
		var checker = new EnvironmentChecker (ConsoleLogger.Instance);
		Assert.DoesNotThrow (() => checker.Check ());
	}

	[Test]
	[Platform ("MacOsX")]
	public void Check_ReturnsValidResult ()
	{
		var checker = new EnvironmentChecker (ConsoleLogger.Instance);
		var result = checker.Check ();
		Assert.That (result, Is.Not.Null);
		Assert.That (result.Status, Is.AnyOf (EnvironmentStatus.Ok, EnvironmentStatus.Partial, EnvironmentStatus.Missing));
	}

	[Test]
	[Platform ("MacOsX")]
	public void IsXcodeLicenseAccepted_DoesNotThrow ()
	{
		var checker = new EnvironmentChecker (ConsoleLogger.Instance);
		Assert.DoesNotThrow (() => checker.IsXcodeLicenseAccepted ());
	}

	[Test]
	[Platform ("MacOsX")]
	public void RunFirstLaunch_DoesNotThrow ()
	{
		var checker = new EnvironmentChecker (ConsoleLogger.Instance);
		Assert.DoesNotThrow (() => checker.RunFirstLaunch ());
	}

	[TestCase ("iPhoneOS", "iOS")]
	[TestCase ("iPhoneSimulator", "iOS")]
	[TestCase ("AppleTVOS", "tvOS")]
	[TestCase ("AppleTVSimulator", "tvOS")]
	[TestCase ("WatchOS", "watchOS")]
	[TestCase ("WatchSimulator", "watchOS")]
	[TestCase ("XROS", "visionOS")]
	[TestCase ("XRSimulator", "visionOS")]
	[TestCase ("MacOSX", "macOS")]
	[TestCase ("UnknownPlatform", "UnknownPlatform")]
	public void MapPlatformName_MapsCorrectly (string input, string expected)
	{
		Assert.That (EnvironmentChecker.MapPlatformName (input), Is.EqualTo (expected));
	}

	[Test]
	public void MapPlatformName_ReturnsSameForEmpty ()
	{
		Assert.That (EnvironmentChecker.MapPlatformName (""), Is.EqualTo (""));
	}

	[Test]
	public void MapPlatformName_IsCaseSensitive ()
	{
		Assert.That (EnvironmentChecker.MapPlatformName ("iphoneos"), Is.EqualTo ("iphoneos"));
		Assert.That (EnvironmentChecker.MapPlatformName ("MACOSX"), Is.EqualTo ("MACOSX"));
	}
}
