// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using NUnit.Framework;

using Xamarin.MacDev;
using Xamarin.MacDev.Models;

namespace Tests {

	[TestFixture]
	public class AppleInstallerTests {

		[Test]
		public void Constructor_ThrowsOnNullLogger ()
		{
			Assert.Throws<System.ArgumentNullException> (() => new AppleInstaller (null));
		}

		[Test]
		public void DeriveStatus_MissingWhenNoXcode ()
		{
			var result = new EnvironmentCheckResult {
				Xcode = null,
				CommandLineTools = new CommandLineToolsInfo { IsInstalled = true },
			};
			result.DeriveStatus ();
			Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Missing));
		}

		[Test]
		public void DeriveStatus_MissingWhenNoCLT ()
		{
			var result = new EnvironmentCheckResult {
				Xcode = new XcodeInfo { Path = "/Applications/Xcode.app", Version = new System.Version (16, 0) },
				CommandLineTools = new CommandLineToolsInfo { IsInstalled = false },
			};
			result.DeriveStatus ();
			Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Missing));
		}

		[Test]
		public void DeriveStatus_PartialWhenNoRuntimes ()
		{
			var result = new EnvironmentCheckResult {
				Xcode = new XcodeInfo { Path = "/Applications/Xcode.app", Version = new System.Version (16, 0) },
				CommandLineTools = new CommandLineToolsInfo { IsInstalled = true, Version = "16.0" },
			};
			result.DeriveStatus ();
			Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Partial));
		}

		[Test]
		public void DeriveStatus_OkWhenEverythingPresent ()
		{
			var result = new EnvironmentCheckResult {
				Xcode = new XcodeInfo { Path = "/Applications/Xcode.app", Version = new System.Version (16, 0) },
				CommandLineTools = new CommandLineToolsInfo { IsInstalled = true, Version = "16.0" },
			};
			result.Runtimes.Add (new SimulatorRuntimeInfo {
				Platform = "iOS",
				Version = "18.0",
				Identifier = "com.apple.CoreSimulator.SimRuntime.iOS-18-0",
				IsAvailable = true,
			});
			result.DeriveStatus ();
			Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Ok));
		}

		[Test]
		public void DefaultResult_HasMissingStatus ()
		{
			var result = new EnvironmentCheckResult ();
			Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Missing));
			Assert.That (result.Xcode, Is.Null);
			Assert.That (result.Runtimes, Is.Empty);
			Assert.That (result.Platforms, Is.Empty);
		}
	}
}
