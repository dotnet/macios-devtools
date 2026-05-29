// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using System.IO;
using NUnit.Framework;
using Xamarin.MacDev;

namespace tests {

	[TestFixture]
	public class PathUtilsTests {

		[Test]
		[Platform ("MacOsX")]
		public void IsSymlink_ReturnsFalse_ForNonExistentFile ()
		{
			var path = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			// Should not throw; returns false for ENOENT
			Assert.That (PathUtils.IsSymlink (path), Is.False);
		}

		[Test]
		[Platform ("MacOsX")]
		public void IsSymlink_ReturnsFalse_ForRegularFile ()
		{
			var path = Path.GetTempFileName ();
			try {
				Assert.That (PathUtils.IsSymlink (path), Is.False);
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		[Platform ("MacOsX")]
		public void IsSymlink_ReturnsTrue_ForSymlink ()
		{
			var target = Path.GetTempFileName ();
			var link = target + ".link";
			try {
#if NET
				File.CreateSymbolicLink (link, target);
#else
				// File.CreateSymbolicLink is not available on net472.
				// Use a shell command to create the symlink on macOS.
				var psi = new System.Diagnostics.ProcessStartInfo ("ln", $"-s \"{target}\" \"{link}\"") {
					UseShellExecute = false,
				};
				System.Diagnostics.Process.Start (psi)!.WaitForExit ();
#endif
				Assert.That (PathUtils.IsSymlink (link), Is.True);
			} finally {
				File.Delete (link);
				File.Delete (target);
			}
		}

		[Test]
		[Platform ("MacOsX")]
		public void IsSymlinkOrHasParentSymlink_ReturnsFalse_ForNonExistentPath ()
		{
			var path = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			Assert.That (PathUtils.IsSymlinkOrHasParentSymlink (path), Is.False);
		}

		[Test]
		[Platform ("MacOsX")]
		public void IsSymlink_ReturnsFalse_WhenPathComponentIsNotDirectory ()
		{
			// /etc/hosts is a file, so /etc/hosts/bogus triggers ENOTDIR
			var path = Path.Combine ("/etc/hosts", "bogus");
			Assert.That (PathUtils.IsSymlink (path), Is.False);
		}

		[Test]
		[Platform ("MacOsX")]
		public void IsSymlinkOrHasParentSymlink_ReturnsTrue_WhenParentIsSymlink ()
		{
			var realDir = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			Directory.CreateDirectory (realDir);
			var childDir = Path.Combine (realDir, "subdir");
			Directory.CreateDirectory (childDir);

			var linkDir = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			try {
#if NET
				Directory.CreateSymbolicLink (linkDir, realDir);
#else
				var psi = new System.Diagnostics.ProcessStartInfo ("ln", $"-s \"{realDir}\" \"{linkDir}\"") {
					UseShellExecute = false,
				};
				System.Diagnostics.Process.Start (psi)!.WaitForExit ();
#endif
				var childViaLink = Path.Combine (linkDir, "subdir");
				Assert.That (PathUtils.IsSymlinkOrHasParentSymlink (childViaLink), Is.True);
			} finally {
				if (Directory.Exists (linkDir))
					Directory.Delete (linkDir);
				Directory.Delete (realDir, recursive: true);
			}
		}
	}
}
