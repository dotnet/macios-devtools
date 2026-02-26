// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

using NUnit.Framework;

using Xamarin.MacDev.Models;

namespace UnitTests {

	[TestFixture]
	public class XcodeInfoTests {

		[Test]
		public void DefaultValues ()
		{
			var info = new XcodeInfo ();
			Assert.That (info.Path, Is.EqualTo (""));
			Assert.That (info.Version, Is.EqualTo (new Version (0, 0)));
			Assert.That (info.Build, Is.EqualTo (""));
			Assert.That (info.DTXcode, Is.EqualTo (""));
			Assert.That (info.IsSelected, Is.False);
			Assert.That (info.IsSymlink, Is.False);
		}

		[Test]
		public void ToString_IncludesPathAndVersion ()
		{
			var info = new XcodeInfo {
				Path = "/Applications/Xcode.app",
				Version = new Version (16, 2),
				Build = "16C5032a",
			};
			Assert.That (info.ToString (), Does.Contain ("/Applications/Xcode.app"));
			Assert.That (info.ToString (), Does.Contain ("16.2"));
			Assert.That (info.ToString (), Does.Contain ("16C5032a"));
		}
	}
}
