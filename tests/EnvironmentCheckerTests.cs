// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using NUnit.Framework;

using Xamarin.MacDev;

namespace Tests {

	[TestFixture]
	public class EnvironmentCheckerTests {

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
	}
}
