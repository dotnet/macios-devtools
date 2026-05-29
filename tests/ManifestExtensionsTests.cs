// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using NUnit.Framework;
using Xamarin.MacDev;

namespace tests {

	[TestFixture]
	public class ManifestExtensionsTests {

		[Test]
		public void GetWKAppBundleIdentifier_ReturnsNull_WhenNSExtensionMissing ()
		{
			var dict = new PDictionary ();
			var result = dict.GetWKAppBundleIdentifier ();
			Assert.That (result, Is.Null);
		}

		[Test]
		public void GetWKAppBundleIdentifier_ReturnsNull_WhenNSExtensionAttributesMissing ()
		{
			var dict = new PDictionary ();
			dict.Add ("NSExtension", new PDictionary ());
			var result = dict.GetWKAppBundleIdentifier ();
			Assert.That (result, Is.Null);
		}

		[Test]
		public void GetWKAppBundleIdentifier_ReturnsValue_WhenPresent ()
		{
			var dict = new PDictionary ();
			var ext = new PDictionary ();
			var extAttr = new PDictionary ();
			extAttr.Add ("WKAppBundleIdentifier", new PString ("com.test.watchapp"));
			ext.Add ("NSExtensionAttributes", extAttr);
			dict.Add ("NSExtension", ext);

			var result = dict.GetWKAppBundleIdentifier ();
			Assert.That (result, Is.EqualTo ("com.test.watchapp"));
		}

		[Test]
		public void SetWKAppBundleIdentifier_CreatesStructure_WhenNSExtensionMissing ()
		{
			var dict = new PDictionary ();
			dict.SetWKAppBundleIdentifier ("com.test.app");
			Assert.That (dict.GetWKAppBundleIdentifier (), Is.EqualTo ("com.test.app"));
		}

		[Test]
		public void SetWKAppBundleIdentifier_RemoveIsNoOp_WhenNSExtensionMissing ()
		{
			var dict = new PDictionary ();
			Assert.DoesNotThrow (() => dict.SetWKAppBundleIdentifier (""));
		}

		[Test]
		public void GetUIDeviceFamily_SkipsUnknownDeviceFamilyNumber ()
		{
			var dict = new PDictionary ();
			var arr = new PArray ();
			arr.Add (new PNumber (99));
			dict.Add ("UIDeviceFamily", arr);

			var result = dict.GetUIDeviceFamily ("UIDeviceFamily");
			Assert.That (result, Is.EqualTo (IPhoneDeviceType.NotSet));
		}

		[Test]
		public void GetUIDeviceFamily_MixedKnownAndUnknown_SkipsUnknown ()
		{
			var dict = new PDictionary ();
			var arr = new PArray ();
			arr.Add (new PNumber (2)); // IPad
			arr.Add (new PNumber (99)); // unknown — should be skipped
			dict.Add ("UIDeviceFamily", arr);

			var result = dict.GetUIDeviceFamily ("UIDeviceFamily");
			Assert.That (result.HasFlag (IPhoneDeviceType.IPad), Is.True);
			Assert.That (result.HasFlag (IPhoneDeviceType.IPhone), Is.False);
		}

		[Test]
		public void GetUIDeviceFamily_ParsesKnownDeviceFamilies ()
		{
			var dict = new PDictionary ();
			var arr = new PArray ();
			arr.Add (new PNumber (1));
			arr.Add (new PNumber (2));
			dict.Add ("UIDeviceFamily", arr);

			var result = dict.GetUIDeviceFamily ("UIDeviceFamily");
			Assert.That (result.HasFlag (IPhoneDeviceType.IPhone), Is.True);
			Assert.That (result.HasFlag (IPhoneDeviceType.IPad), Is.True);
		}
	}
}
