// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.MacDev;
using Xamarin.MacDev.Models;

#nullable enable

namespace tests;

[TestFixture]
public class EnvironmentCheckerTests {

	/// <summary>
	/// Subclass that overrides external dependencies so Check() can be
	/// unit-tested without invoking Xcode, xcode-select, or simctl.
	/// </summary>
	class TestableEnvironmentChecker : EnvironmentChecker {

		public XcodeInfo? XcodeResult { get; set; }
		public CommandLineToolsInfo CltResult { get; set; } = new CommandLineToolsInfo ();
		public List<SimulatorRuntimeInfo> RuntimesResult { get; set; } = new List<SimulatorRuntimeInfo> ();
		public bool LicenseAccepted { get; set; } = true;
		public List<string> PlatformsResult { get; set; } = new List<string> ();
		public bool ThrowOnClt { get; set; }
		public bool ThrowOnRuntimes { get; set; }

		public TestableEnvironmentChecker () : base (ConsoleLogger.Instance) { }

		protected override XcodeInfo? GetBestXcode () => XcodeResult;

		protected override CommandLineToolsInfo CheckCommandLineTools ()
		{
			if (ThrowOnClt)
				throw new InvalidOperationException ("simulated CLT failure");
			return CltResult;
		}

		protected override List<SimulatorRuntimeInfo> ListRuntimes ()
		{
			if (ThrowOnRuntimes)
				throw new InvalidOperationException ("simulated runtime failure");
			return RuntimesResult;
		}

		public override bool IsXcodeLicenseAccepted () => LicenseAccepted;

		protected override List<string> GetPlatforms (string xcodePath) => PlatformsResult;
	}

	// ── Constructor ──

	[Test]
	public void Constructor_ThrowsOnNullLogger ()
	{
		Assert.Throws<ArgumentNullException> (() => new EnvironmentChecker (null!));
	}

	// ── Check() aggregation tests ──

	[Test]
	public void Check_NoXcode_ReturnsMissing ()
	{
		var checker = new TestableEnvironmentChecker {
			XcodeResult = null,
			CltResult = new CommandLineToolsInfo { IsInstalled = true },
		};
		var result = checker.Check ();
		Assert.That (result.Xcode, Is.Null);
		Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Missing));
	}

	[Test]
	public void Check_XcodeAndClt_NoRuntimes_ReturnsPartial ()
	{
		var checker = new TestableEnvironmentChecker {
			XcodeResult = new XcodeInfo { Path = "/Applications/Xcode.app", Version = new Version (16, 2) },
			CltResult = new CommandLineToolsInfo { IsInstalled = true },
			RuntimesResult = new List<SimulatorRuntimeInfo> (),
			PlatformsResult = new List<string> { "iOS", "macOS" },
		};
		var result = checker.Check ();
		Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Partial));
		Assert.That (result.Xcode, Is.Not.Null);
		Assert.That (result.CommandLineTools.IsInstalled, Is.True);
		Assert.That (result.Platforms, Has.Count.EqualTo (2));
	}

	[Test]
	public void Check_EverythingPresent_ReturnsOk ()
	{
		var checker = new TestableEnvironmentChecker {
			XcodeResult = new XcodeInfo { Path = "/Applications/Xcode.app", Version = new Version (16, 2) },
			CltResult = new CommandLineToolsInfo { IsInstalled = true },
			RuntimesResult = new List<SimulatorRuntimeInfo> {
				new SimulatorRuntimeInfo { Platform = "iOS", Version = "18.2", IsAvailable = true },
			},
			PlatformsResult = new List<string> { "iOS", "macOS" },
		};
		var result = checker.Check ();
		Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Ok));
	}

	[Test]
	public void Check_CltThrows_DoesNotCrash_ReturnsMissing ()
	{
		var checker = new TestableEnvironmentChecker {
			XcodeResult = new XcodeInfo { Path = "/Applications/Xcode.app", Version = new Version (16, 2) },
			ThrowOnClt = true,
			RuntimesResult = new List<SimulatorRuntimeInfo> {
				new SimulatorRuntimeInfo { Platform = "iOS", Version = "18.2", IsAvailable = true },
			},
		};
		var result = checker.Check ();
		Assert.That (result.CommandLineTools.IsInstalled, Is.False);
		Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Missing));
	}

	[Test]
	public void Check_RuntimesThrows_DoesNotCrash ()
	{
		var checker = new TestableEnvironmentChecker {
			XcodeResult = new XcodeInfo { Path = "/Applications/Xcode.app", Version = new Version (16, 2) },
			CltResult = new CommandLineToolsInfo { IsInstalled = true },
			ThrowOnRuntimes = true,
		};
		var result = checker.Check ();
		Assert.That (result.Runtimes, Is.Empty);
		Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Partial));
	}

	[Test]
	public void Check_NoClt_ReturnsMissing ()
	{
		var checker = new TestableEnvironmentChecker {
			XcodeResult = new XcodeInfo { Path = "/Applications/Xcode.app", Version = new Version (16, 2) },
			CltResult = new CommandLineToolsInfo { IsInstalled = false },
			RuntimesResult = new List<SimulatorRuntimeInfo> {
				new SimulatorRuntimeInfo { Platform = "iOS", Version = "18.2", IsAvailable = true },
			},
		};
		var result = checker.Check ();
		Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Missing));
	}

	[Test]
	public void Check_NoXcode_SkipsPlatformsAndLicense ()
	{
		var checker = new TestableEnvironmentChecker {
			XcodeResult = null,
			PlatformsResult = new List<string> { "iOS" },
		};
		var result = checker.Check ();
		Assert.That (result.Platforms, Is.Empty);
	}

	// ── Smoke tests (macOS only) ──

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

	// ── MapDirectoryNamesToPlatforms ──

	[Test]
	public void MapDirectoryNamesToPlatforms_DeduplicatesIOS ()
	{
		var result = EnvironmentChecker.MapDirectoryNamesToPlatforms (
			new [] { "iPhoneOS", "iPhoneSimulator", "MacOSX" });
		Assert.That (result, Is.EqualTo (new List<string> { "iOS", "macOS" }));
	}

	[Test]
	public void MapDirectoryNamesToPlatforms_AllApplePlatforms ()
	{
		var result = EnvironmentChecker.MapDirectoryNamesToPlatforms (
			new [] { "iPhoneOS", "iPhoneSimulator", "AppleTVOS", "AppleTVSimulator",
					 "WatchOS", "WatchSimulator", "XROS", "XRSimulator", "MacOSX" });
		Assert.That (result, Is.EqualTo (new List<string> { "iOS", "tvOS", "watchOS", "visionOS", "macOS" }));
	}

	[Test]
	public void MapDirectoryNamesToPlatforms_Empty ()
	{
		var result = EnvironmentChecker.MapDirectoryNamesToPlatforms (Array.Empty<string> ());
		Assert.That (result, Is.Empty);
	}

	[Test]
	public void MapDirectoryNamesToPlatforms_UnknownPassedThrough ()
	{
		var result = EnvironmentChecker.MapDirectoryNamesToPlatforms (
			new [] { "DriverKit", "MacOSX" });
		Assert.That (result, Is.EqualTo (new List<string> { "DriverKit", "macOS" }));
	}

	// ── MapPlatformName ──

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
