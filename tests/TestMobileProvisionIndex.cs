//
// TestMobileProvisionIndex.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2017 Microsoft Corp.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;

using NUnit.Framework;

using Xamarin.MacDev;

namespace Tests {
	[TestFixture]
	public class TestMobileProvisionIndex {
		static readonly string [] ProfileDirectories;

		static TestMobileProvisionIndex ()
		{
			ProfileDirectories = new string [] {
				Path.Combine (TestHelper.ProjectDir, "TestData", "Provisioning Profiles")
			};
		}

		[Test]
		public void TestCreateIndex ()
		{
			var index = MobileProvisionIndex.CreateIndex (ProfileDirectories, "profiles.index");

			Assert.That (index.ProvisioningProfiles.Count, Is.EqualTo (2));

			var idCompanyName = index.ProvisioningProfiles.FindIndex ((v) => v.ApplicationIdentifier.Contains ("companyname"));
			var idXamarin = index.ProvisioningProfiles.FindIndex ((v) => v.ApplicationIdentifier.Contains ("xamarin"));
			Assert.That (idCompanyName, Is.Not.EqualTo (-1), "Company Name Index");
			Assert.That (idXamarin, Is.Not.EqualTo (-1), "Xamarin Index");
			Assert.That (idCompanyName, Is.Not.EqualTo (idXamarin), "Indices");
			Assert.That (index.ProvisioningProfiles [idCompanyName].ApplicationIdentifier, Is.EqualTo ("YHT9CR87YA.com.companyname.*"));
			Assert.That (index.ProvisioningProfiles [idCompanyName].CreationDate, Is.EqualTo (new DateTime (2017, 07, 19, 19, 43, 45, DateTimeKind.Utc)));
			Assert.That (index.ProvisioningProfiles [idCompanyName].DeveloperCertificates.Count, Is.EqualTo (1));
			Assert.That (index.ProvisioningProfiles [idCompanyName].DeveloperCertificates [0].Name, Is.EqualTo ("iPhone Developer: Jeffrey Stedfast (FZ77UAV9SW)"));
			Assert.That (index.ProvisioningProfiles [idCompanyName].DeveloperCertificates [0].Thumbprint, Is.EqualTo ("2097D37F4D16AB7D8D927E7C1872F2A94D8DC718"));
			Assert.That (index.ProvisioningProfiles [idCompanyName].Distribution, Is.EqualTo (MobileProvisionDistributionType.Development));
			Assert.That (index.ProvisioningProfiles [idCompanyName].ExpirationDate, Is.EqualTo (new DateTime (2018, 07, 19, 19, 43, 45, DateTimeKind.Utc)));
			Assert.That (Path.GetFileName (index.ProvisioningProfiles [idCompanyName].FileName), Is.EqualTo ("29cbf4b4-a170-4c74-a29a-64ecd55b102e.mobileprovision"));
			//Assert.AreEqual (index.ProvisioningProfiles[0].LastModified);
			Assert.That (index.ProvisioningProfiles [idCompanyName].Name, Is.EqualTo ("CompanyName Development Profile"));
			Assert.That (index.ProvisioningProfiles [0].Platforms.Count, Is.EqualTo (1));
			Assert.That (index.ProvisioningProfiles [idCompanyName].Platforms [0], Is.EqualTo (MobileProvisionPlatform.iOS));
			Assert.That (index.ProvisioningProfiles [idCompanyName].Uuid, Is.EqualTo ("29cbf4b4-a170-4c74-a29a-64ecd55b102e"));

			Assert.That (index.ProvisioningProfiles [idXamarin].ApplicationIdentifier, Is.EqualTo ("YHT9CR87YA.com.xamarin.*"));
			Assert.That (index.ProvisioningProfiles [idXamarin].CreationDate, Is.EqualTo (new DateTime (2017, 07, 19, 19, 44, 0, DateTimeKind.Utc)));
			Assert.That (index.ProvisioningProfiles [idXamarin].DeveloperCertificates.Count, Is.EqualTo (1));
			Assert.That (index.ProvisioningProfiles [idXamarin].DeveloperCertificates [0].Name, Is.EqualTo ("iPhone Developer: Jeffrey Stedfast (FZ77UAV9SW)"));
			Assert.That (index.ProvisioningProfiles [idXamarin].DeveloperCertificates [0].Thumbprint, Is.EqualTo ("2097D37F4D16AB7D8D927E7C1872F2A94D8DC718"));
			Assert.That (index.ProvisioningProfiles [idXamarin].Distribution, Is.EqualTo (MobileProvisionDistributionType.Development));
			Assert.That (index.ProvisioningProfiles [idXamarin].ExpirationDate, Is.EqualTo (new DateTime (2018, 07, 19, 19, 44, 0, DateTimeKind.Utc)));
			Assert.That (Path.GetFileName (index.ProvisioningProfiles [idXamarin].FileName), Is.EqualTo ("7079f389-6ff4-4290-bf76-c8a222947616.mobileprovision"));
			//Assert.AreEqual (index.ProvisioningProfiles[0].LastModified);
			Assert.That (index.ProvisioningProfiles [idXamarin].Name, Is.EqualTo ("Xamarin Development Profile"));
			Assert.That (index.ProvisioningProfiles [idXamarin].Platforms.Count, Is.EqualTo (1));
			Assert.That (index.ProvisioningProfiles [idXamarin].Platforms [0], Is.EqualTo (MobileProvisionPlatform.iOS));
			Assert.That (index.ProvisioningProfiles [idXamarin].Uuid, Is.EqualTo ("7079f389-6ff4-4290-bf76-c8a222947616"));
		}

		[Test]
		public void TestOpenIndex ()
		{
			var index = MobileProvisionIndex.OpenIndex (ProfileDirectories, "profiles.index");

			Assert.That (index.ProvisioningProfiles.Count, Is.EqualTo (2));

			var idCompanyName = index.ProvisioningProfiles.FindIndex ((v) => v.ApplicationIdentifier.Contains ("companyname"));
			var idXamarin = index.ProvisioningProfiles.FindIndex ((v) => v.ApplicationIdentifier.Contains ("xamarin"));
			Assert.That (idCompanyName, Is.Not.EqualTo (-1), "Company Name Index");
			Assert.That (idXamarin, Is.Not.EqualTo (-1), "Xamarin Index");
			Assert.That (idCompanyName, Is.Not.EqualTo (idXamarin), "Indices");
			Assert.That (index.ProvisioningProfiles [idCompanyName].ApplicationIdentifier, Is.EqualTo ("YHT9CR87YA.com.companyname.*"));
			Assert.That (index.ProvisioningProfiles [idCompanyName].CreationDate, Is.EqualTo (new DateTime (2017, 07, 19, 19, 43, 45, DateTimeKind.Utc)));
			Assert.That (index.ProvisioningProfiles [idCompanyName].DeveloperCertificates.Count, Is.EqualTo (1));
			Assert.That (index.ProvisioningProfiles [idCompanyName].DeveloperCertificates [0].Name, Is.EqualTo ("iPhone Developer: Jeffrey Stedfast (FZ77UAV9SW)"));
			Assert.That (index.ProvisioningProfiles [idCompanyName].DeveloperCertificates [0].Thumbprint, Is.EqualTo ("2097D37F4D16AB7D8D927E7C1872F2A94D8DC718"));
			Assert.That (index.ProvisioningProfiles [idCompanyName].Distribution, Is.EqualTo (MobileProvisionDistributionType.Development));
			Assert.That (index.ProvisioningProfiles [idCompanyName].ExpirationDate, Is.EqualTo (new DateTime (2018, 07, 19, 19, 43, 45, DateTimeKind.Utc)));
			Assert.That (Path.GetFileName (index.ProvisioningProfiles [idCompanyName].FileName), Is.EqualTo ("29cbf4b4-a170-4c74-a29a-64ecd55b102e.mobileprovision"));
			//Assert.AreEqual (index.ProvisioningProfiles[0].LastModified);
			Assert.That (index.ProvisioningProfiles [idCompanyName].Name, Is.EqualTo ("CompanyName Development Profile"));
			Assert.That (index.ProvisioningProfiles [idCompanyName].Platforms.Count, Is.EqualTo (1));
			Assert.That (index.ProvisioningProfiles [idCompanyName].Platforms [0], Is.EqualTo (MobileProvisionPlatform.iOS));
			Assert.That (index.ProvisioningProfiles [idCompanyName].Uuid, Is.EqualTo ("29cbf4b4-a170-4c74-a29a-64ecd55b102e"));

			Assert.That (index.ProvisioningProfiles [idXamarin].ApplicationIdentifier, Is.EqualTo ("YHT9CR87YA.com.xamarin.*"));
			Assert.That (index.ProvisioningProfiles [idXamarin].CreationDate, Is.EqualTo (new DateTime (2017, 07, 19, 19, 44, 0, DateTimeKind.Utc)));
			Assert.That (index.ProvisioningProfiles [idXamarin].DeveloperCertificates.Count, Is.EqualTo (1));
			Assert.That (index.ProvisioningProfiles [idXamarin].DeveloperCertificates [0].Name, Is.EqualTo ("iPhone Developer: Jeffrey Stedfast (FZ77UAV9SW)"));
			Assert.That (index.ProvisioningProfiles [idXamarin].DeveloperCertificates [0].Thumbprint, Is.EqualTo ("2097D37F4D16AB7D8D927E7C1872F2A94D8DC718"));
			Assert.That (index.ProvisioningProfiles [idXamarin].Distribution, Is.EqualTo (MobileProvisionDistributionType.Development));
			Assert.That (index.ProvisioningProfiles [idXamarin].ExpirationDate, Is.EqualTo (new DateTime (2018, 07, 19, 19, 44, 0, DateTimeKind.Utc)));
			Assert.That (Path.GetFileName (index.ProvisioningProfiles [idXamarin].FileName), Is.EqualTo ("7079f389-6ff4-4290-bf76-c8a222947616.mobileprovision"));
			//Assert.AreEqual (index.ProvisioningProfiles[0].LastModified);
			Assert.That (index.ProvisioningProfiles [idXamarin].Name, Is.EqualTo ("Xamarin Development Profile"));
			Assert.That (index.ProvisioningProfiles [idXamarin].Platforms.Count, Is.EqualTo (1));
			Assert.That (index.ProvisioningProfiles [idXamarin].Platforms [0], Is.EqualTo (MobileProvisionPlatform.iOS));
			Assert.That (index.ProvisioningProfiles [idXamarin].Uuid, Is.EqualTo ("7079f389-6ff4-4290-bf76-c8a222947616"));
		}

		static void WriteProfile (string path, string name, string uuid, DateTime creationDate, DateTime expirationDate)
		{
			Directory.CreateDirectory (Path.GetDirectoryName (path));
			File.WriteAllText (path, $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
	<key>Name</key>
	<string>{name}</string>
	<key>UUID</key>
	<string>{uuid}</string>
	<key>CreationDate</key>
	<date>{creationDate:yyyy-MM-ddTHH:mm:ssZ}</date>
	<key>ExpirationDate</key>
	<date>{expirationDate:yyyy-MM-ddTHH:mm:ssZ}</date>
	<key>Platform</key>
	<array>
		<string>iOS</string>
	</array>
	<key>DeveloperCertificates</key>
	<array/>
	<key>Entitlements</key>
	<dict/>
</dict>
</plist>
");
		}

		[Test]
		public void GetMobileProvision_PrefersNewestUnexpiredProfile ()
		{
			var tmpdir = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			try {
				var newDir = Path.Combine (tmpdir, "new");
				var oldDir = Path.Combine (tmpdir, "old");
				var now = DateTime.UtcNow;

				// A profile named after the requested profile in the old directory used to win, even when it was expired.
				WriteProfile (Path.Combine (oldDir, "Profile.mobileprovision"), "Profile", "OLD-UUID", now.AddYears (-3), now.AddYears (-2));
				WriteProfile (Path.Combine (newDir, "NEW-UUID.mobileprovision"), "Profile", "NEW-UUID", now.AddDays (-10), now.AddDays (300));
				WriteProfile (Path.Combine (newDir, "OLDER-UUID.mobileprovision"), "Profile", "OLDER-UUID", now.AddDays (-20), now.AddDays (290));

				var profile = MobileProvisionIndex.GetMobileProvision (MobileProvisionPlatform.iOS, "Profile", new [] { newDir, oldDir }, Path.Combine (tmpdir, "profiles.index"));

				Assert.That (profile?.Uuid, Is.EqualTo ("NEW-UUID"));
			} finally {
				Directory.Delete (tmpdir, true);
			}
		}

		[Test]
		public void GetMobileProvision_FallsBackToExpiredProfile ()
		{
			var tmpdir = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			try {
				var now = DateTime.UtcNow;

				WriteProfile (Path.Combine (tmpdir, "profiles", "Profile.mobileprovision"), "Profile", "EXPIRED-UUID", now.AddYears (-3), now.AddYears (-2));

				var profile = MobileProvisionIndex.GetMobileProvision (MobileProvisionPlatform.iOS, "Profile", new [] { Path.Combine (tmpdir, "profiles") }, Path.Combine (tmpdir, "profiles.index"));

				Assert.That (profile?.Uuid, Is.EqualTo ("EXPIRED-UUID"));
			} finally {
				Directory.Delete (tmpdir, true);
			}
		}

		[Test]
		public void GetMobileProvision_MatchesFilenameCaseInsensitively ()
		{
			var tmpdir = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			try {
				var now = DateTime.UtcNow;
				var fileName = Path.Combine (tmpdir, "profiles", "PROFILE.MOBILEPROVISION");
				WriteProfile (fileName, "Other Profile", "OTHER-UUID", now.AddDays (-1), now.AddDays (10));

				var profile = MobileProvisionIndex.GetMobileProvision (MobileProvisionPlatform.iOS, "profile", new [] { Path.GetDirectoryName (fileName) }, Path.Combine (tmpdir, "profiles.index"));

				Assert.That (profile?.Uuid, Is.EqualTo ("OTHER-UUID"));
			} finally {
				Directory.Delete (tmpdir, true);
			}
		}

		[Test]
		public void OpenIndex_ReindexesPreviouslyUnreadableProfileWhenRepaired ()
		{
			var tmpdir = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			try {
				var profileDir = Path.Combine (tmpdir, "profiles");
				var indexPath = Path.Combine (tmpdir, "profiles.index");
				Directory.CreateDirectory (profileDir);
				WriteProfile (Path.Combine (profileDir, "Other.mobileprovision"), "Other", "OTHER-UUID", DateTime.UtcNow, DateTime.UtcNow.AddDays (10));

				var repairedFile = Path.Combine (profileDir, "Repaired.mobileprovision");
				File.WriteAllText (repairedFile, "Invalid profile");
				var index = MobileProvisionIndex.CreateIndex (new [] { profileDir }, indexPath);

				WriteProfile (repairedFile, "Repaired", "REPAIRED-UUID", DateTime.UtcNow, DateTime.UtcNow.AddDays (10));
				File.SetLastWriteTimeUtc (repairedFile, index.LastModified.AddMinutes (1));
				Directory.SetLastWriteTimeUtc (profileDir, index.LastModified);

				var profile = MobileProvisionIndex.GetMobileProvision (MobileProvisionPlatform.iOS, "Repaired", new [] { profileDir }, indexPath);

				Assert.That (profile?.Uuid, Is.EqualTo ("REPAIRED-UUID"));
			} finally {
				Directory.Delete (tmpdir, true);
			}
		}

#if NET10_0_OR_GREATER
		[TestCase (false, false)]
		[TestCase (false, true)]
		[TestCase (true, false)]
		[TestCase (true, true)]
		[Platform (Exclude = "Win")]
		public void GetMobileProvision_SkipsUnreadableDirectory (bool existingIndex, bool unreadableFirst)
		{
			if (OperatingSystem.IsWindows ())
				return;

			var tmpdir = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			var unreadableDir = Path.Combine (tmpdir, "unreadable");
			var permissions = UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute;
			try {
				var readableDir = Path.Combine (tmpdir, "readable");
				var indexPath = Path.Combine (tmpdir, "profiles.index");
				var now = DateTime.UtcNow;
				WriteProfile (Path.Combine (readableDir, "Profile.mobileprovision"), "Profile", "READABLE-UUID", now.AddDays (-2), now.AddDays (10));
				WriteProfile (Path.Combine (unreadableDir, "Profile.mobileprovision"), "Profile", "UNREADABLE-UUID", now.AddDays (-1), now.AddDays (10));
				Directory.SetLastWriteTimeUtc (unreadableDir, now.AddHours (-1));
				Directory.SetLastWriteTimeUtc (readableDir, now);

				var dirs = unreadableFirst ? new [] { unreadableDir, readableDir } : new [] { readableDir, unreadableDir };
				if (existingIndex)
					MobileProvisionIndex.CreateIndex (dirs, indexPath);

				permissions = File.GetUnixFileMode (unreadableDir);
				File.SetUnixFileMode (unreadableDir, UnixFileMode.None);
				Assert.That (() => Directory.GetFiles (unreadableDir), Throws.TypeOf<UnauthorizedAccessException> (), "The test directory must be unreadable.");

				var profile = MobileProvisionIndex.GetMobileProvision (MobileProvisionPlatform.iOS, "Profile", dirs, indexPath);
				Assert.That (profile?.Uuid, Is.EqualTo ("READABLE-UUID"));

				File.SetUnixFileMode (unreadableDir, permissions);
				profile = MobileProvisionIndex.GetMobileProvision (MobileProvisionPlatform.iOS, "Profile", dirs, indexPath);
				Assert.That (profile?.Uuid, Is.EqualTo ("UNREADABLE-UUID"), "Retry the skipped directory even when its modification time has not changed.");
			} finally {
				if (Directory.Exists (unreadableDir))
					File.SetUnixFileMode (unreadableDir, permissions);
				if (Directory.Exists (tmpdir))
					Directory.Delete (tmpdir, true);
			}
		}
#endif
	}
}
