// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using NUnit.Framework;
using Xamarin.MacDev;

#nullable enable

namespace tests;

[TestFixture]
public class AppleInstallerTests {

	[Test]
	public void Constructor_ThrowsOnNullLogger ()
	{
		Assert.Throws<System.ArgumentNullException> (() => new AppleInstaller (null!));
	}

	[Test]
	[Platform ("MacOsX")]
	public void Install_DryRun_DoesNotThrow ()
	{
		var installer = new AppleInstaller (ConsoleLogger.Instance);
		Assert.DoesNotThrow (() => installer.Install (dryRun: true));
	}

	[Test]
	[Platform ("MacOsX")]
	public void Install_DryRun_ReturnsValidResult ()
	{
		var installer = new AppleInstaller (ConsoleLogger.Instance);
		var result = installer.Install (dryRun: true);
		Assert.That (result, Is.Not.Null);
		Assert.That (result.Status, Is.AnyOf (
			Xamarin.MacDev.Models.EnvironmentStatus.Ok,
			Xamarin.MacDev.Models.EnvironmentStatus.Partial,
			Xamarin.MacDev.Models.EnvironmentStatus.Missing));
	}

	[Test]
	[Platform ("MacOsX")]
	public void Install_WithPlatforms_DryRun_DoesNotThrow ()
	{
		var installer = new AppleInstaller (ConsoleLogger.Instance);
		Assert.DoesNotThrow (() => installer.Install (
			requestedPlatforms: new [] { "iOS", "macOS" },
			dryRun: true));
	}
}
