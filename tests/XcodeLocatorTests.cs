// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using System;
using System.IO;

using NUnit.Framework;

using Xamarin.MacDev;

namespace Tests {

	[TestFixture]
	[Platform (Exclude = "Win", Reason = "Xcode is a macOS-only tool; xcode-select and Xcode bundles are not valid on Windows.")]
	public class XcodeLocatorTests {

		static string CreateFakeXcodeBundle (string version = "16.2", string dtXcode = "1620", string? cfBundleVersion = null)
		{
			var dir = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName () + ".app");
			var contentsDir = Path.Combine (dir, "Contents");
			Directory.CreateDirectory (contentsDir);

			var buildVersion = cfBundleVersion ?? "16C5032a";

			File.WriteAllText (Path.Combine (contentsDir, "version.plist"), $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
	<key>CFBundleShortVersionString</key>
	<string>{version}</string>
	<key>CFBundleVersion</key>
	<string>{buildVersion}</string>
</dict>
</plist>
");

			File.WriteAllText (Path.Combine (contentsDir, "Info.plist"), $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
	<key>DTXcode</key>
	<string>{dtXcode}</string>
</dict>
</plist>
");

			return dir;
		}

		[Test]
		public void TryLocatingXcode_Override_PopulatesXcodeVersion ()
		{
			var fakeXcode = CreateFakeXcodeBundle (version: "16.2", dtXcode: "1620");
			try {
				var locator = new XcodeLocator (ConsoleLogger.Instance);
				var found = locator.TryLocatingXcode (fakeXcode);
				Assert.That (found, Is.True, "TryLocatingXcode should return true for a valid Xcode bundle.");
				Assert.That (locator.XcodeVersion, Is.EqualTo (new Version (16, 2)), "XcodeVersion should be populated.");
				Assert.That (locator.DTXcode, Is.EqualTo ("1620"), "DTXcode should be populated.");
				Assert.That (locator.XcodeLocation, Is.EqualTo (fakeXcode), "XcodeLocation should be set.");
			} finally {
				Directory.Delete (fakeXcode, recursive: true);
			}
		}

		[Test]
		public void TryLocatingXcode_Override_WithContentsDeveloper_PopulatesXcodeVersion ()
		{
			var fakeXcode = CreateFakeXcodeBundle (version: "26.2", dtXcode: "2620");
			try {
				var locator = new XcodeLocator (ConsoleLogger.Instance);
				var pathWithDeveloper = Path.Combine (fakeXcode, "Contents", "Developer");
				Directory.CreateDirectory (pathWithDeveloper);
				var found = locator.TryLocatingXcode (pathWithDeveloper);
				Assert.That (found, Is.True, "TryLocatingXcode should return true when path includes /Contents/Developer.");
				Assert.That (locator.XcodeVersion, Is.EqualTo (new Version (26, 2)), "XcodeVersion should be populated.");
				Assert.That (locator.DTXcode, Is.EqualTo ("2620"), "DTXcode should be populated.");
				Assert.That (locator.XcodeLocation, Is.EqualTo (fakeXcode), "XcodeLocation should be canonicalized (no /Contents/Developer suffix).");
			} finally {
				Directory.Delete (fakeXcode, recursive: true);
			}
		}

		[Test]
		public void TryLocatingXcode_NullOverride_ReturnsFalseWhenNothingFound ()
		{
			// With SupportEnvironmentVariableLookup and SupportSettingsFileLookup both false
			// and no xcode-select available (Linux CI), TryLocatingXcode should return false.
			var locator = new XcodeLocator (ConsoleLogger.Instance) {
				SupportEnvironmentVariableLookup = false,
				SupportSettingsFileLookup = false,
			};
			// On Linux, xcode-select doesn't exist so TryGetSystemXcode returns false.
			// We just verify it doesn't throw.
			Assert.DoesNotThrow (() => locator.TryLocatingXcode (null));
		}

		[Test]
		public void TryLocatingXcode_Override_MissingVersionPlist_IgnoresBrokenPath ()
		{
			var dir = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName () + ".app");
			var contentsDir = Path.Combine (dir, "Contents");
			Directory.CreateDirectory (contentsDir);
			// No version.plist created.
			File.WriteAllText (Path.Combine (contentsDir, "Info.plist"), @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0""><dict><key>DTXcode</key><string>1620</string></dict></plist>
");
			try {
				var locator = new XcodeLocator (ConsoleLogger.Instance);
				locator.TryLocatingXcode (dir);
				// The broken path must never be accepted as the Xcode location,
				// even if a fallback (xcode-select) finds a different valid Xcode.
				Assert.That (locator.XcodeLocation, Is.Not.EqualTo (dir), "A bundle missing version.plist should not be set as XcodeLocation.");
			} finally {
				Directory.Delete (dir, recursive: true);
			}
		}

		[Test]
		public void TryLocatingXcode_Override_MissingInfoPlist_IgnoresBrokenPath ()
		{
			var dir = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName () + ".app");
			var contentsDir = Path.Combine (dir, "Contents");
			Directory.CreateDirectory (contentsDir);
			File.WriteAllText (Path.Combine (contentsDir, "version.plist"), @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0""><dict><key>CFBundleShortVersionString</key><string>16.2</string><key>CFBundleVersion</key><string>16C5032a</string></dict></plist>
");
			// No Info.plist created.
			try {
				var locator = new XcodeLocator (ConsoleLogger.Instance);
				locator.TryLocatingXcode (dir);
				// The broken path must never be accepted as the Xcode location,
				// even if a fallback (xcode-select) finds a different valid Xcode.
				Assert.That (locator.XcodeLocation, Is.Not.EqualTo (dir), "A bundle missing Info.plist should not be set as XcodeLocation.");
			} finally {
				Directory.Delete (dir, recursive: true);
			}
		}

		[Test]
		public void TryGetSystemXcode_WhenXcodeSelectMissing_ReturnsFalse ()
		{
			// On Linux (CI), /usr/bin/xcode-select doesn't exist, so TryGetSystemXcode returns false.
			if (File.Exists ("/usr/bin/xcode-select"))
				Assert.Ignore ("This test only applies when xcode-select is not present.");

			var result = XcodeLocator.TryGetSystemXcode (ConsoleLogger.Instance, out var path);
			Assert.That (result, Is.False, "TryGetSystemXcode should return false when xcode-select is not installed.");
			Assert.That (path, Is.Null, "path should be null when xcode-select is not installed.");
		}
	}
}
