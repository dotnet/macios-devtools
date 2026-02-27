// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using NUnit.Framework;

using Xamarin.MacDev;

namespace Tests {

	[TestFixture]
	public class CommandLineToolsTests {

		[Test]
		public void ParsePkgutilVersion_ReturnsVersion ()
		{
			var output = @"package-id: com.apple.pkg.CLTools_Executables
version: 16.2.0.0.1.1733547573
volume: /
location: /
install-time: 1733547600
";
			var version = CommandLineTools.ParsePkgutilVersion (output);
			Assert.That (version, Is.EqualTo ("16.2.0.0.1.1733547573"));
		}

		[Test]
		public void ParsePkgutilVersion_HandlesWhitespace ()
		{
			var output = "  version:   26.2.0.0.1.1764812424  \nvolume: /\n";
			var version = CommandLineTools.ParsePkgutilVersion (output);
			Assert.That (version, Is.EqualTo ("26.2.0.0.1.1764812424"));
		}

		[Test]
		public void ParsePkgutilVersion_ReturnsNullForEmptyInput ()
		{
			Assert.That (CommandLineTools.ParsePkgutilVersion (""), Is.Null);
			Assert.That (CommandLineTools.ParsePkgutilVersion ((string) null), Is.Null);
		}

		[Test]
		public void ParsePkgutilVersion_ReturnsNullWhenNoVersionLine ()
		{
			var output = "package-id: com.apple.pkg.CLTools_Executables\nvolume: /\n";
			Assert.That (CommandLineTools.ParsePkgutilVersion (output), Is.Null);
		}

		[Test]
		public void ParsePkgutilVersion_ReturnsNullForEmptyVersion ()
		{
			var output = "version: \nvolume: /\n";
			Assert.That (CommandLineTools.ParsePkgutilVersion (output), Is.Null);
		}

		[Test]
		public void ParsePkgutilVersion_HandlesWindowsLineEndings ()
		{
			var output = "package-id: com.apple.pkg.CLTools_Executables\r\nversion: 15.1.0.0.1.1700000000\r\nvolume: /\r\n";
			var version = CommandLineTools.ParsePkgutilVersion (output);
			Assert.That (version, Is.EqualTo ("15.1.0.0.1.1700000000"));
		}

		[Test]
		public void ParsePkgutilVersion_IgnoresVersionSubstringsInOtherFields ()
		{
			var output = "package-id: com.apple.pkg.CLTools_Executables\nlocation: /version:/fake\nversion: 16.0.0.0.1\n";
			var version = CommandLineTools.ParsePkgutilVersion (output);
			Assert.That (version, Is.EqualTo ("16.0.0.0.1"));
		}
	}
}
