// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xamarin.MacDev.Models;

#nullable enable

namespace tests;

/// <summary>Tests the public model types of physical device discovery.</summary>
[TestFixture]
public class PhysicalDeviceInfoTests {

	static readonly PhysicalDeviceDiscoveryStatus [] FailureStatuses = Enum.GetValues (typeof (PhysicalDeviceDiscoveryStatus))
		.Cast<PhysicalDeviceDiscoveryStatus> ()
		.Where (s => s != PhysicalDeviceDiscoveryStatus.Success)
		.ToArray ();

	#region Kind

	[TestCase ("iPhone", "", "", PhysicalDeviceKind.IPhone)]
	[TestCase ("iPad", "", "", PhysicalDeviceKind.IPad)]
	[TestCase ("iPod", "", "", PhysicalDeviceKind.IPod)]
	[TestCase ("appleWatch", "", "", PhysicalDeviceKind.AppleWatch)]
	[TestCase ("watch", "", "", PhysicalDeviceKind.AppleWatch)]
	[TestCase ("appleTV", "", "", PhysicalDeviceKind.AppleTV)]
	[TestCase ("tv", "", "", PhysicalDeviceKind.AppleTV)]
	[TestCase ("realityDevice", "", "", PhysicalDeviceKind.AppleVision)]
	[TestCase ("appleVision", "", "", PhysicalDeviceKind.AppleVision)]
	[TestCase ("appleVisionPro", "", "", PhysicalDeviceKind.AppleVision)]
	[TestCase ("visionPro", "", "", PhysicalDeviceKind.AppleVision)]
	[TestCase ("mac", "", "", PhysicalDeviceKind.Mac)]
	[TestCase ("IPHONE", "", "", PhysicalDeviceKind.IPhone)]
	[TestCase ("ipad", "", "", PhysicalDeviceKind.IPad)]
	public void Kind_IsClassifiedFromTheDeviceClass (string deviceClass, string productType, string platform, PhysicalDeviceKind expected)
	{
		AssertKind (deviceClass, productType, platform, expected);
	}

	[TestCase ("iPhone16,1", PhysicalDeviceKind.IPhone)]
	[TestCase ("iPad14,3", PhysicalDeviceKind.IPad)]
	[TestCase ("iPod9,1", PhysicalDeviceKind.IPod)]
	[TestCase ("Watch6,6", PhysicalDeviceKind.AppleWatch)]
	[TestCase ("AppleTV14,1", PhysicalDeviceKind.AppleTV)]
	[TestCase ("RealityDevice14,1", PhysicalDeviceKind.AppleVision)]
	[TestCase ("MacBookPro17,1", PhysicalDeviceKind.Mac)]
	[TestCase ("Mac14,3", PhysicalDeviceKind.Mac)]
	[TestCase ("iMac21,1", PhysicalDeviceKind.Mac)]
	[TestCase ("VirtualMac2,1", PhysicalDeviceKind.Mac)]
	[TestCase ("IPAD14,3", PhysicalDeviceKind.IPad)]
	[TestCase ("SomethingNew1,1", PhysicalDeviceKind.Unknown)]
	public void Kind_FallsBackToTheProductType (string productType, PhysicalDeviceKind expected)
	{
		AssertKind ("", productType, "", expected);
	}

	[TestCase ("iPadOS", PhysicalDeviceKind.IPad)]
	[TestCase ("watchOS", PhysicalDeviceKind.AppleWatch)]
	[TestCase ("tvOS", PhysicalDeviceKind.AppleTV)]
	[TestCase ("visionOS", PhysicalDeviceKind.AppleVision)]
	[TestCase ("xrOS", PhysicalDeviceKind.AppleVision)]
	[TestCase ("macOS", PhysicalDeviceKind.Mac)]
	[TestCase ("WATCHOS", PhysicalDeviceKind.AppleWatch)]
	[TestCase ("iOS", PhysicalDeviceKind.Unknown)]
	[TestCase ("", PhysicalDeviceKind.Unknown)]
	public void Kind_FallsBackToThePlatform (string platform, PhysicalDeviceKind expected)
	{
		AssertKind ("", "", platform, expected);
	}

	[Test]
	public void Kind_PrefersTheDeviceClassOverTheProductTypeAndPlatform ()
	{
		AssertKind ("iPad", "iPhone16,1", "watchOS", PhysicalDeviceKind.IPad);
	}

	[Test]
	public void Kind_PrefersTheProductTypeOverThePlatform ()
	{
		AssertKind ("", "iPad14,3", "iOS", PhysicalDeviceKind.IPad);
	}

	[Test]
	public void Kind_ContinuesWithTheProductTypeWhenTheDeviceClassIsNotRecognized ()
	{
		AssertKind ("somethingNew", "iPhone16,1", "iOS", PhysicalDeviceKind.IPhone);
	}

	[Test]
	public void Kind_IsNeverInferredFromTheName ()
	{
		var device = new PhysicalDeviceInfo { Name = "Rui's iPhone" };
		var other = new PhysicalDeviceInfo { Name = "My iPhone", DeviceClass = "iPad" };

		Assert.That (device.Kind, Is.EqualTo (PhysicalDeviceKind.Unknown));
		Assert.That (other.Kind, Is.EqualTo (PhysicalDeviceKind.IPad));
	}

	[Test]
	public void Kind_ForANewDeviceIsUnknown ()
	{
		Assert.That (new PhysicalDeviceInfo ().Kind, Is.EqualTo (PhysicalDeviceKind.Unknown));
		Assert.That (default (PhysicalDeviceKind), Is.EqualTo (PhysicalDeviceKind.Unknown));
	}

	static void AssertKind (string deviceClass, string productType, string platform, PhysicalDeviceKind expected)
	{
		var device = new PhysicalDeviceInfo { DeviceClass = deviceClass, ProductType = productType, Platform = platform };

		Assert.That (device.Kind, Is.EqualTo (expected));
	}

	#endregion

	#region Availability

	[TestCase ("connected", PhysicalDeviceAvailability.Connected, true)]
	[TestCase ("Connected", PhysicalDeviceAvailability.Connected, true)]
	[TestCase ("CONNECTED", PhysicalDeviceAvailability.Connected, true)]
	[TestCase ("disconnected", PhysicalDeviceAvailability.Available, true)]
	[TestCase ("unavailable", PhysicalDeviceAvailability.Unavailable, false)]
	[TestCase ("", PhysicalDeviceAvailability.Unknown, false)]
	[TestCase ("something", PhysicalDeviceAvailability.Unknown, false)]
	public void Availability_IsDerivedFromTheConnectionState (string connectionState, PhysicalDeviceAvailability expected, bool reachable)
	{
		var device = new PhysicalDeviceInfo { ConnectionState = connectionState };
		var connection = new PhysicalDeviceConnection { ConnectionState = connectionState };

		Assert.That (device.Availability, Is.EqualTo (expected));
		Assert.That (device.IsReachable, Is.EqualTo (reachable));
		Assert.That (connection.Availability, Is.EqualTo (expected));
	}

	[Test]
	public void Availability_IsNeverDerivedFromThePairingState ()
	{
		var device = new PhysicalDeviceInfo { PairingState = "paired", TransportType = "wired" };

		Assert.That (device.Availability, Is.EqualTo (PhysicalDeviceAvailability.Unknown));
		Assert.That (device.IsReachable, Is.False);
	}

	[Test]
	public void Availability_DefaultsToUnknown ()
	{
		Assert.That (default (PhysicalDeviceAvailability), Is.EqualTo (PhysicalDeviceAvailability.Unknown));
		Assert.That (new PhysicalDeviceInfo ().Availability, Is.EqualTo (PhysicalDeviceAvailability.Unknown));
		Assert.That (new PhysicalDeviceConnection ().Availability, Is.EqualTo (PhysicalDeviceAvailability.Unknown));
	}

	#endregion

	#region PhysicalDeviceInfo

	[TestCase ("simulated", true)]
	[TestCase ("SIMULATED", true)]
	[TestCase ("physical", false)]
	[TestCase ("", false)]
	public void IsSimulated_ReflectsTheReportedReality (string reality, bool expected)
	{
		Assert.That (new PhysicalDeviceInfo { Reality = reality }.IsSimulated, Is.EqualTo (expected));
	}

	[Test]
	public void HasHardwareUdid_IsTrueOnlyForARealHardwareUdid ()
	{
		Assert.That (new PhysicalDeviceInfo ().HasHardwareUdid, Is.False);
		Assert.That (new PhysicalDeviceInfo { Udid = "CD-1", Identifier = "CD-1" }.HasHardwareUdid, Is.False);
		Assert.That (new PhysicalDeviceInfo { HardwareUdid = "00008130-001A2B3C4D5E6F78" }.HasHardwareUdid, Is.True);
	}

	[Test]
	public void NewDevice_HasNoConnectionsAndEmptyStrings ()
	{
		var device = new PhysicalDeviceInfo ();

		Assert.That (device.Connections, Is.Empty);
		Assert.That (device.HardwareUdid, Is.Empty);
		Assert.That (device.MarketingName, Is.Empty);
		Assert.That (device.Reality, Is.Empty);
		Assert.That (device.ConnectionState, Is.Empty);
		Assert.That (device.IsSimulated, Is.False);
	}

	[Test]
	public void ToString_KeepsDescribingNameUdidAndDeviceClass ()
	{
		var device = new PhysicalDeviceInfo { Name = "Test iPhone", Udid = "UDID-1", DeviceClass = "iPhone" };

		Assert.That (device.ToString (), Is.EqualTo ("Test iPhone (UDID-1) [iPhone]"));
	}

	#endregion

	#region PhysicalDeviceConnection

	[Test]
	public void Connection_ToString_DescribesIdentifierTransportAndState ()
	{
		var connection = new PhysicalDeviceConnection { Identifier = "CD-1", TransportType = "wired", ConnectionState = "connected" };

		Assert.That (connection.ToString (), Is.EqualTo ("CD-1 [wired, connected]"));
	}

	[Test]
	public void Connection_NewInstance_HasEmptyStrings ()
	{
		var connection = new PhysicalDeviceConnection ();

		Assert.That (connection.Identifier, Is.Empty);
		Assert.That (connection.TransportType, Is.Empty);
		Assert.That (connection.PairingState, Is.Empty);
		Assert.That (connection.ConnectionState, Is.Empty);
	}

	#endregion

	#region PhysicalDeviceDiscoveryResult

	[Test]
	public void Result_Success_ExposesTheDevices ()
	{
		var devices = new [] { new PhysicalDeviceInfo { Name = "One" }, new PhysicalDeviceInfo { Name = "Two" } };

		var result = new PhysicalDeviceDiscoveryResult (PhysicalDeviceDiscoveryStatus.Success, devices, exitCode: 0);

		Assert.That (result.IsSuccess, Is.True);
		Assert.That (result.Devices.Select (d => d.Name), Is.EqualTo (new [] { "One", "Two" }));
		Assert.That (result.ErrorMessage, Is.Empty);
		Assert.That (result.ExitCode, Is.EqualTo (0));
	}

	[Test]
	public void Result_WithoutDevices_IsEmptyAndHasNoErrorOrExitCode ()
	{
		var result = new PhysicalDeviceDiscoveryResult (PhysicalDeviceDiscoveryStatus.Success);

		Assert.That (result.Devices, Is.Empty);
		Assert.That (result.ErrorMessage, Is.Empty);
		Assert.That (result.ExitCode, Is.Null);
	}

	[TestCaseSource (nameof (FailureStatuses))]
	public void Result_Failure_NeverExposesDevices (PhysicalDeviceDiscoveryStatus status)
	{
		var result = new PhysicalDeviceDiscoveryResult (status, new [] { new PhysicalDeviceInfo { Name = "One" } }, "It failed.", 1);

		Assert.That (result.IsSuccess, Is.False);
		Assert.That (result.Status, Is.EqualTo (status));
		Assert.That (result.Devices, Is.Empty);
		Assert.That (result.ErrorMessage, Is.EqualTo ("It failed."));
		Assert.That (result.ExitCode, Is.EqualTo (1));
	}

	[Test]
	public void Result_CopiesTheDevicesItIsGiven ()
	{
		var devices = new List<PhysicalDeviceInfo> { new PhysicalDeviceInfo { Name = "One" } };
		var result = new PhysicalDeviceDiscoveryResult (PhysicalDeviceDiscoveryStatus.Success, devices);

		devices.Clear ();

		Assert.That (result.Devices, Has.Count.EqualTo (1));
	}

	[Test]
	public void Result_DevicesCannotBeModifiedThroughTheList ()
	{
		var result = new PhysicalDeviceDiscoveryResult (PhysicalDeviceDiscoveryStatus.Success, new [] { new PhysicalDeviceInfo () });

		Assert.That (((ICollection<PhysicalDeviceInfo>) result.Devices).IsReadOnly, Is.True);
	}

	[Test]
	public void Result_ToString_DescribesTheOutcome ()
	{
		var success = new PhysicalDeviceDiscoveryResult (PhysicalDeviceDiscoveryStatus.Success, new [] { new PhysicalDeviceInfo (), new PhysicalDeviceInfo () });
		var failure = new PhysicalDeviceDiscoveryResult (PhysicalDeviceDiscoveryStatus.TimedOut, errorMessage: "Took too long.");

		Assert.That (success.ToString (), Is.EqualTo ("Success (2 devices)"));
		Assert.That (failure.ToString (), Is.EqualTo ("TimedOut: Took too long."));
	}

	#endregion
}
