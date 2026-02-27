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
	}
}
