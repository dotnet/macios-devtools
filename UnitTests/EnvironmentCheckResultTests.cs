// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Xamarin.MacDev.Models;

namespace UnitTests {

	[TestFixture]
	public class EnvironmentCheckResultTests {

		[Test]
		public void DeriveStatus_Missing_WhenNoXcode ()
		{
			var result = new EnvironmentCheckResult {
				Xcode = null,
				CommandLineTools = new CommandLineToolsInfo { IsInstalled = true },
			};
			result.DeriveStatus ();
			Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Missing));
		}

		[Test]
		public void DeriveStatus_Missing_WhenNoClt ()
		{
			var result = new EnvironmentCheckResult {
				Xcode = new XcodeInfo { Path = "/Applications/Xcode.app", Version = new Version (16, 2) },
				CommandLineTools = new CommandLineToolsInfo { IsInstalled = false },
			};
			result.DeriveStatus ();
			Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Missing));
		}

		[Test]
		public void DeriveStatus_Partial_WhenNoRuntimes ()
		{
			var result = new EnvironmentCheckResult {
				Xcode = new XcodeInfo { Path = "/Applications/Xcode.app", Version = new Version (16, 2) },
				CommandLineTools = new CommandLineToolsInfo { IsInstalled = true },
				Runtimes = new List<SimulatorRuntimeInfo> (),
			};
			result.DeriveStatus ();
			Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Partial));
		}

		[Test]
		public void DeriveStatus_Ok_WhenEverythingPresent ()
		{
			var result = new EnvironmentCheckResult {
				Xcode = new XcodeInfo { Path = "/Applications/Xcode.app", Version = new Version (16, 2) },
				CommandLineTools = new CommandLineToolsInfo { IsInstalled = true },
				Runtimes = new List<SimulatorRuntimeInfo> {
					new SimulatorRuntimeInfo { Platform = "iOS", Version = "18.2", IsAvailable = true }
				},
			};
			result.DeriveStatus ();
			Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Ok));
		}

		[Test]
		public void DefaultStatus_IsMissing ()
		{
			var result = new EnvironmentCheckResult ();
			Assert.That (result.Status, Is.EqualTo (EnvironmentStatus.Missing));
		}

		[Test]
		public void SimulatorDeviceInfo_IsBooted ()
		{
			var device = new SimulatorDeviceInfo { State = "Booted" };
			Assert.That (device.IsBooted, Is.True);

			device.State = "Shutdown";
			Assert.That (device.IsBooted, Is.False);
		}

		[Test]
		public void SimulatorRuntimeInfo_ToString ()
		{
			var runtime = new SimulatorRuntimeInfo {
				Name = "iOS 18.2",
				Identifier = "com.apple.CoreSimulator.SimRuntime.iOS-18-2",
			};
			Assert.That (runtime.ToString (), Does.Contain ("iOS 18.2"));
		}

		[Test]
		public void CommandLineToolsInfo_ToString ()
		{
			var clt = new CommandLineToolsInfo { IsInstalled = true, Version = "16.2.0", Path = "/Library/Developer/CommandLineTools" };
			Assert.That (clt.ToString (), Does.Contain ("16.2.0"));

			var missing = new CommandLineToolsInfo { IsInstalled = false };
			Assert.That (missing.ToString (), Does.Contain ("not installed"));
		}
	}
}
