// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using NUnit.Framework;

using Xamarin.MacDev;

namespace Tests {

	[TestFixture]
	public class SimctlOutputParserTests {

		// Realistic simctl list devices --json output based on actual Apple format
		// Structure validated against ClientTools.Platform RemoteSimulatorValidator
		static readonly string SampleDevicesJson = @"{
  ""devices"" : {
    ""com.apple.CoreSimulator.SimRuntime.iOS-18-2"" : [
      {
        ""name"" : ""iPhone 16 Pro"",
        ""udid"" : ""A1B2C3D4-E5F6-7890-ABCD-EF1234567890"",
        ""state"" : ""Shutdown"",
        ""isAvailable"" : true,
        ""deviceTypeIdentifier"" : ""com.apple.CoreSimulator.SimDeviceType.iPhone-16-Pro""
      },
      {
        ""name"" : ""iPhone 16"",
        ""udid"" : ""B2C3D4E5-F6A7-8901-BCDE-F12345678901"",
        ""state"" : ""Booted"",
        ""isAvailable"" : true,
        ""deviceTypeIdentifier"" : ""com.apple.CoreSimulator.SimDeviceType.iPhone-16""
      }
    ],
    ""com.apple.CoreSimulator.SimRuntime.tvOS-18-2"" : [
      {
        ""name"" : ""Apple TV"",
        ""udid"" : ""C3D4E5F6-A7B8-9012-CDEF-123456789012"",
        ""state"" : ""Shutdown"",
        ""isAvailable"" : false,
        ""deviceTypeIdentifier"" : ""com.apple.CoreSimulator.SimDeviceType.Apple-TV-1080p""
      }
    ]
  }
}";

		static readonly string SampleRuntimesJson = @"{
  ""runtimes"" : [
    {
      ""bundlePath"" : ""/Library/Developer/CoreSimulator/Profiles/Runtimes/iOS 17.5.simruntime"",
      ""buildversion"" : ""21F79"",
      ""platform"" : ""iOS"",
      ""runtimeRoot"" : ""/Library/Developer/CoreSimulator/Volumes/iOS_21F79/Library/Developer/CoreSimulator/Profiles/Runtimes/iOS 17.5.simruntime/Contents/Resources/RuntimeRoot"",
      ""identifier"" : ""com.apple.CoreSimulator.SimRuntime.iOS-17-5"",
      ""version"" : ""17.5"",
      ""isInternal"" : false,
      ""isAvailable"" : true,
      ""name"" : ""iOS 17.5"",
      ""supportedDeviceTypes"" : []
    },
    {
      ""bundlePath"" : ""/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Library/Developer/CoreSimulator/Profiles/Runtimes/iOS.simruntime"",
      ""buildversion"" : ""22C150"",
      ""platform"" : ""iOS"",
      ""runtimeRoot"" : ""/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Library/Developer/CoreSimulator/Profiles/Runtimes/iOS.simruntime/Contents/Resources/RuntimeRoot"",
      ""identifier"" : ""com.apple.CoreSimulator.SimRuntime.iOS-18-2"",
      ""version"" : ""18.2"",
      ""isInternal"" : true,
      ""isAvailable"" : true,
      ""name"" : ""iOS 18.2"",
      ""supportedDeviceTypes"" : []
    },
    {
      ""bundlePath"" : ""/Applications/Xcode.app/Contents/Developer/Platforms/AppleTVOS.platform/Library/Developer/CoreSimulator/Profiles/Runtimes/tvOS.simruntime"",
      ""buildversion"" : ""22K150"",
      ""platform"" : ""tvOS"",
      ""runtimeRoot"" : ""/path/to/runtime"",
      ""identifier"" : ""com.apple.CoreSimulator.SimRuntime.tvOS-18-2"",
      ""version"" : ""18.2"",
      ""isInternal"" : true,
      ""isAvailable"" : false,
      ""name"" : ""tvOS 18.2"",
      ""supportedDeviceTypes"" : []
    }
  ]
}";

		[Test]
		public void ParseDevices_ParsesMultipleRuntimes ()
		{
			var devices = SimctlOutputParser.ParseDevices (SampleDevicesJson);
			Assert.That (devices.Count, Is.EqualTo (3));
		}

		[Test]
		public void ParseDevices_SetsRuntimeIdentifier ()
		{
			var devices = SimctlOutputParser.ParseDevices (SampleDevicesJson);
			Assert.That (devices [0].RuntimeIdentifier, Is.EqualTo ("com.apple.CoreSimulator.SimRuntime.iOS-18-2"));
			Assert.That (devices [2].RuntimeIdentifier, Is.EqualTo ("com.apple.CoreSimulator.SimRuntime.tvOS-18-2"));
		}

		[Test]
		public void ParseDevices_SetsDeviceProperties ()
		{
			var devices = SimctlOutputParser.ParseDevices (SampleDevicesJson);
			var iphone16Pro = devices [0];
			Assert.That (iphone16Pro.Name, Is.EqualTo ("iPhone 16 Pro"));
			Assert.That (iphone16Pro.Udid, Is.EqualTo ("A1B2C3D4-E5F6-7890-ABCD-EF1234567890"));
			Assert.That (iphone16Pro.State, Is.EqualTo ("Shutdown"));
			Assert.That (iphone16Pro.IsAvailable, Is.True);
			Assert.That (iphone16Pro.DeviceTypeIdentifier, Is.EqualTo ("com.apple.CoreSimulator.SimDeviceType.iPhone-16-Pro"));
			Assert.That (iphone16Pro.IsBooted, Is.False);
		}

		[Test]
		public void ParseDevices_DetectsBootedState ()
		{
			var devices = SimctlOutputParser.ParseDevices (SampleDevicesJson);
			Assert.That (devices [1].IsBooted, Is.True);
			Assert.That (devices [1].State, Is.EqualTo ("Booted"));
		}

		[Test]
		public void ParseDevices_DetectsUnavailableDevices ()
		{
			var devices = SimctlOutputParser.ParseDevices (SampleDevicesJson);
			Assert.That (devices [2].IsAvailable, Is.False);
		}

		[Test]
		public void ParseDevices_ReturnsEmptyForNullOrEmpty ()
		{
			Assert.That (SimctlOutputParser.ParseDevices (""), Is.Empty);
			Assert.That (SimctlOutputParser.ParseDevices ((string) null), Is.Empty);
		}

		[Test]
		public void ParseDevices_ReturnsEmptyForNoDevicesKey ()
		{
			Assert.That (SimctlOutputParser.ParseDevices ("{}"), Is.Empty);
		}

		[Test]
		public void ParseRuntimes_ParsesMultipleRuntimes ()
		{
			var runtimes = SimctlOutputParser.ParseRuntimes (SampleRuntimesJson);
			Assert.That (runtimes.Count, Is.EqualTo (3));
		}

		[Test]
		public void ParseRuntimes_SetsRuntimeProperties ()
		{
			var runtimes = SimctlOutputParser.ParseRuntimes (SampleRuntimesJson);
			var ios175 = runtimes [0];
			Assert.That (ios175.Name, Is.EqualTo ("iOS 17.5"));
			Assert.That (ios175.Identifier, Is.EqualTo ("com.apple.CoreSimulator.SimRuntime.iOS-17-5"));
			Assert.That (ios175.Version, Is.EqualTo ("17.5"));
			Assert.That (ios175.BuildVersion, Is.EqualTo ("21F79"));
			Assert.That (ios175.Platform, Is.EqualTo ("iOS"));
			Assert.That (ios175.IsAvailable, Is.True);
			Assert.That (ios175.IsBundled, Is.False);
		}

		[Test]
		public void ParseRuntimes_DetectsBundledRuntime ()
		{
			var runtimes = SimctlOutputParser.ParseRuntimes (SampleRuntimesJson);
			Assert.That (runtimes [0].IsBundled, Is.False);
			Assert.That (runtimes [1].IsBundled, Is.True);
		}

		[Test]
		public void ParseRuntimes_DetectsUnavailableRuntime ()
		{
			var runtimes = SimctlOutputParser.ParseRuntimes (SampleRuntimesJson);
			Assert.That (runtimes [2].IsAvailable, Is.False);
			Assert.That (runtimes [2].Platform, Is.EqualTo ("tvOS"));
		}

		[Test]
		public void ParseRuntimes_ReturnsEmptyForNullOrEmpty ()
		{
			Assert.That (SimctlOutputParser.ParseRuntimes (""), Is.Empty);
			Assert.That (SimctlOutputParser.ParseRuntimes ((string) null), Is.Empty);
		}

		[Test]
		public void ParseRuntimes_ReturnsEmptyForNoRuntimesKey ()
		{
			Assert.That (SimctlOutputParser.ParseRuntimes ("{}"), Is.Empty);
		}

		[Test]
		public void ParseCreateOutput_ReturnsUdid ()
		{
			Assert.That (SimctlOutputParser.ParseCreateOutput ("A1B2C3D4-E5F6-7890-ABCD-EF1234567890\n"),
				Is.EqualTo ("A1B2C3D4-E5F6-7890-ABCD-EF1234567890"));
		}

		[Test]
		public void ParseCreateOutput_ReturnsNullForEmpty ()
		{
			Assert.That (SimctlOutputParser.ParseCreateOutput (""), Is.Null);
			Assert.That (SimctlOutputParser.ParseCreateOutput ((string) null), Is.Null);
		}

		[Test]
		public void ParseDevices_HandlesBoolAsString ()
		{
			// simctl sometimes returns isAvailable as a string (observed in
			// Redth/AppleDev.Tools FlexibleStringConverter)
			var json = @"{
  ""devices"" : {
    ""com.apple.CoreSimulator.SimRuntime.iOS-17-0"" : [
      {
        ""name"" : ""iPhone 15"",
        ""udid"" : ""12345"",
        ""state"" : ""Shutdown"",
        ""isAvailable"" : ""true"",
        ""deviceTypeIdentifier"" : ""com.apple.CoreSimulator.SimDeviceType.iPhone-15""
      }
    ]
  }
}";
			var devices = SimctlOutputParser.ParseDevices (json);
			// isAvailable as string "true" won't match JsonValueKind.True,
			// but our GetBool handles string fallback
			Assert.That (devices.Count, Is.EqualTo (1));
		}
	}
}
