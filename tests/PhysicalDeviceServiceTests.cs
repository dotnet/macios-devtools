// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.MacDev;
using Xamarin.MacDev.Models;
using static tests.DeviceCtlFixtures;
using static tests.FakeDeviceCtl;

#nullable enable

namespace tests;

/// <summary>
/// Tests how <see cref="PhysicalDeviceService"/> turns devicectl output into typed devices:
/// device sets, identity and merging, connection states and JSON layouts.
/// </summary>
[TestFixture]
public class PhysicalDeviceServiceTests {

	// Modeled on what Xcode 27 writes: simulators are listed next to physical devices, flagged by
	// "reality" and/or "visibilityClass". The identifiers are made up.
	const string ListingWithSimulators = @"{
  ""info"": { ""jsonVersion"": 5, ""outcome"": ""success"" },
  ""result"": {
    ""devices"": [
      {
        ""identifier"": ""AAAAAAAA-1111-2222-3333-444444444444"",
        ""properties"": {
          ""connection"": { ""pairingState"": ""paired"", ""state"": ""connected"", ""transportType"": ""sameMachine"" },
          ""hardware"": { ""deviceType"": ""iPhone"", ""platform"": ""iOS"", ""productType"": ""iPhone17,1"", ""reality"": ""simulated"", ""udid"": ""AAAAAAAA-1111-2222-3333-444444444444"" },
          ""state"": { ""name"": ""iPhone 16 Pro"", ""visibilityClass"": ""simulators"" }
        },
        ""visibilityClass"": ""simulators""
      },
      {
        ""identifier"": ""11111111-AAAA-BBBB-CCCC-111111111111"",
        ""properties"": {
          ""connection"": { ""pairingState"": ""paired"", ""state"": ""connected"", ""transportType"": ""wired"" },
          ""hardware"": { ""deviceType"": ""iPhone"", ""platform"": ""iOS"", ""productType"": ""iPhone16,1"", ""reality"": ""physical"", ""udid"": ""00008130-001A2B3C4D5E6F78"" },
          ""state"": { ""name"": ""Test iPhone"" }
        }
      },
      {
        ""identifier"": ""BBBBBBBB-1111-2222-3333-444444444444"",
        ""properties"": {
          ""connection"": { ""pairingState"": ""paired"", ""state"": ""connected"", ""transportType"": ""sameMachine"" },
          ""hardware"": { ""deviceType"": ""iPad"", ""platform"": ""iOS"", ""productType"": ""iPad16,3"", ""udid"": ""BBBBBBBB-1111-2222-3333-444444444444"" },
          ""state"": { ""name"": ""iPad Pro 13-inch"", ""visibilityClass"": ""simulators"" }
        },
        ""visibilityClass"": ""simulators""
      }
    ]
  }
}";

	RecordingLogger log = new RecordingLogger ();

	[SetUp]
	public void SetUp ()
	{
		log = new RecordingLogger ();
	}

	Task<PhysicalDeviceDiscoveryResult> ListAsync (string json) => CreateService (log, ReturnsJson (json)).ListAsync ();

	#region Device sets

	[Test]
	public async Task ListAsync_NoDevices_ReturnsSuccessWithEmptyList ()
	{
		var result = await ListAsync (Document ());

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success));
		Assert.That (result.IsSuccess, Is.True);
		Assert.That (result.Devices, Is.Empty);
		Assert.That (result.ErrorMessage, Is.Empty);
		Assert.That (result.ExitCode, Is.EqualTo (0));
	}

	[Test]
	public async Task ListAsync_SingleDevice_ReturnsTypedRecord ()
	{
		var result = await ListAsync (Document (Device (
			IPhoneIdentifier,
			udid: IPhoneUdid,
			name: "Test iPhone",
			marketingName: "iPhone 15 Pro",
			ecid: 1234567,
			serialNumber: "SERIAL1")));

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success));
		var device = result.Devices.Single ();
		Assert.That (device.Name, Is.EqualTo ("Test iPhone"));
		Assert.That (device.HardwareUdid, Is.EqualTo (IPhoneUdid));
		Assert.That (device.HasHardwareUdid, Is.True);
		Assert.That (device.Udid, Is.EqualTo (IPhoneUdid));
		Assert.That (device.Identifier, Is.EqualTo (IPhoneIdentifier));
		Assert.That (device.Kind, Is.EqualTo (PhysicalDeviceKind.IPhone));
		Assert.That (device.DeviceClass, Is.EqualTo ("iPhone"));
		Assert.That (device.Platform, Is.EqualTo ("iOS"));
		Assert.That (device.ProductType, Is.EqualTo ("iPhone16,1"));
		Assert.That (device.HardwareModel, Is.EqualTo ("D83AP"));
		Assert.That (device.MarketingName, Is.EqualTo ("iPhone 15 Pro"));
		Assert.That (device.OSVersion, Is.EqualTo ("18.1"));
		Assert.That (device.BuildVersion, Is.EqualTo ("22B83"));
		Assert.That (device.SerialNumber, Is.EqualTo ("SERIAL1"));
		Assert.That (device.UniqueChipID, Is.EqualTo (1234567UL));
		Assert.That (device.TransportType, Is.EqualTo ("wired"));
		Assert.That (device.PairingState, Is.EqualTo ("paired"));
		Assert.That (device.ConnectionState, Is.EqualTo ("connected"));
		Assert.That (device.Availability, Is.EqualTo (PhysicalDeviceAvailability.Connected));
		Assert.That (device.IsReachable, Is.True);
		Assert.That (device.IsSimulated, Is.False);
	}

	[Test]
	public async Task ListAsync_MultipleDevices_KeepsReportedOrderAndClassifiesEachKind ()
	{
		var result = await ListAsync (Document (
			Device ("id-phone", udid: "UDID-PHONE", name: "Phone", deviceType: "iPhone", productType: "iPhone16,1", platform: "iOS"),
			Device ("id-pad", udid: "UDID-PAD", name: "Pad", deviceType: "iPad", productType: "iPad14,3", platform: "iPadOS"),
			Device ("id-watch", udid: "UDID-WATCH", name: "Watch", deviceType: "appleWatch", productType: "Watch6,6", platform: "watchOS"),
			Device ("id-tv", udid: "UDID-TV", name: "TV", deviceType: "appleTV", productType: "AppleTV14,1", platform: "tvOS"),
			Device ("id-vision", udid: "UDID-VISION", name: "Vision", deviceType: "realityDevice", productType: "RealityDevice14,1", platform: "visionOS"),
			Device ("id-mac", udid: "UDID-MAC", name: "Mac", deviceType: "mac", productType: "MacBookPro17,1", platform: "macOS")));

		Assert.That (result.Devices.Select (d => d.Name), Is.EqualTo (new [] { "Phone", "Pad", "Watch", "TV", "Vision", "Mac" }));
		Assert.That (result.Devices.Select (d => d.Kind), Is.EqualTo (new [] {
			PhysicalDeviceKind.IPhone,
			PhysicalDeviceKind.IPad,
			PhysicalDeviceKind.AppleWatch,
			PhysicalDeviceKind.AppleTV,
			PhysicalDeviceKind.AppleVision,
			PhysicalDeviceKind.Mac,
		}));
	}

	[Test]
	public async Task ListAsync_IPhonesAndIPads_AreToldApartWithoutLookingAtTheName ()
	{
		var result = await ListAsync (Document (
			Device ("id-1", udid: "UDID-1", name: "Mobile", deviceType: null, productType: "iPad14,3", platform: "iOS"),
			Device ("id-2", udid: "UDID-2", name: "Mobile", deviceType: null, productType: "iPhone16,1", platform: "iOS")));

		Assert.That (result.Devices.Select (d => d.Kind), Is.EqualTo (new [] { PhysicalDeviceKind.IPad, PhysicalDeviceKind.IPhone }));
	}

	[Test]
	public async Task ListAsync_IncludesRememberedDevicesThatAreNotReachable ()
	{
		var result = await ListAsync (Document (
			Device ("id-1", udid: "UDID-1", name: "Remembered", transportType: null, connectionState: "unavailable"),
			Device ("id-2", udid: "UDID-2", name: "Attached", connectionState: "connected")));

		Assert.That (result.Devices.Select (d => d.Name), Is.EqualTo (new [] { "Remembered", "Attached" }));
		Assert.That (result.Devices.Select (d => d.IsReachable), Is.EqualTo (new [] { false, true }));
	}

	[Test]
	public async Task ListAsync_ExcludesSimulators ()
	{
		var result = await ListAsync (Document (
			Device ("SIM-1", udid: "SIM-1", name: "iPhone 16 Pro", reality: "simulated", transportType: "sameMachine"),
			Device (IPhoneIdentifier, udid: IPhoneUdid, name: "Test iPhone"),
			Device ("SIM-2", udid: "SIM-2", name: "iPad Pro", reality: "simulated", transportType: "sameMachine")));

		var device = result.Devices.Single ();
		Assert.That (device.Name, Is.EqualTo ("Test iPhone"));
		Assert.That (device.IsSimulated, Is.False);
	}

	[Test]
	public async Task ListAsync_ExcludesSimulatorsFlaggedOnlyByVisibilityClass ()
	{
		var result = await ListAsync (ListingWithSimulators);

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success));
		Assert.That (result.Devices.Select (d => d.Name), Is.EqualTo (new [] { "Test iPhone" }));
	}

	[Test]
	public async Task ListAsync_OnlySimulators_ReturnsSuccessWithEmptyList ()
	{
		var result = await ListAsync (Document (
			Device ("SIM-1", udid: "SIM-1", reality: "simulated")));

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success));
		Assert.That (result.Devices, Is.Empty);
	}

	[Test]
	public async Task ListAsync_RepeatedEnumeration_KeepsTheHardwareIdentityStable ()
	{
		// CoreDevice identifiers and transports can change between runs; the hardware UDID does not.
		var first = await ListAsync (Document (Device ("COREDEVICE-1", udid: IPhoneUdid, transportType: "wired")));
		var second = await ListAsync (Document (Device ("COREDEVICE-2", udid: IPhoneUdid, transportType: "localNetwork")));

		var before = first.Devices.Single ();
		var after = second.Devices.Single ();
		Assert.That (after.Identifier, Is.Not.EqualTo (before.Identifier));
		Assert.That (after.HardwareUdid, Is.EqualTo (before.HardwareUdid));
	}

	#endregion

	#region Identity and merging

	[Test]
	public async Task ListAsync_HardwareUdidAndCoreDeviceIdentifier_StayDistinct ()
	{
		var result = await ListAsync (Document (Device (IPhoneIdentifier, udid: IPhoneUdid)));

		var device = result.Devices.Single ();
		Assert.That (device.HardwareUdid, Is.EqualTo (IPhoneUdid));
		Assert.That (device.Identifier, Is.EqualTo (IPhoneIdentifier));
		Assert.That (device.HardwareUdid, Is.Not.EqualTo (device.Identifier));
	}

	[Test]
	public async Task ListAsync_MissingHardwareUdid_IsReportedAsSuchAndNeverInventedFromTheIdentifier ()
	{
		var result = await ListAsync (Document (Device (WatchIdentifier, udid: null)));

		var device = result.Devices.Single ();
		Assert.That (device.HasHardwareUdid, Is.False);
		Assert.That (device.HardwareUdid, Is.Empty);
		// The legacy Udid property keeps falling back to the CoreDevice identifier.
		Assert.That (device.Udid, Is.EqualTo (WatchIdentifier));
	}

	[Test]
	public async Task ListAsync_SameDeviceOverSeveralTransports_IsMergedIntoOneDevice ()
	{
		var result = await ListAsync (Document (
			Device ("COREDEVICE-NET", udid: IPhoneUdid, transportType: "localNetwork", connectionState: "disconnected"),
			Device ("COREDEVICE-USB", udid: IPhoneUdid, transportType: "wired", connectionState: "connected")));

		var device = result.Devices.Single ();
		Assert.That (device.HardwareUdid, Is.EqualTo (IPhoneUdid));
		Assert.That (device.Identifier, Is.EqualTo ("COREDEVICE-USB"));
		Assert.That (device.TransportType, Is.EqualTo ("wired"));
		Assert.That (device.Availability, Is.EqualTo (PhysicalDeviceAvailability.Connected));
		Assert.That (device.Connections.Select (c => c.Identifier), Is.EqualTo (new [] { "COREDEVICE-USB", "COREDEVICE-NET" }));
		Assert.That (device.Connections.Select (c => c.TransportType), Is.EqualTo (new [] { "wired", "localNetwork" }));
		Assert.That (device.Connections.Select (c => c.Availability), Is.EqualTo (new [] { PhysicalDeviceAvailability.Connected, PhysicalDeviceAvailability.Available }));
	}

	[Test]
	public async Task ListAsync_MergedDevice_PrefersTheMostAvailableConnectionOverTheWiredOne ()
	{
		var result = await ListAsync (Document (
			Device ("COREDEVICE-USB", udid: IPhoneUdid, transportType: "wired", connectionState: "unavailable"),
			Device ("COREDEVICE-NET", udid: IPhoneUdid, transportType: "localNetwork", connectionState: "disconnected")));

		var device = result.Devices.Single ();
		Assert.That (device.Identifier, Is.EqualTo ("COREDEVICE-NET"));
		Assert.That (device.Availability, Is.EqualTo (PhysicalDeviceAvailability.Available));
		Assert.That (device.IsReachable, Is.True);
		Assert.That (device.Connections.Select (c => c.TransportType), Is.EqualTo (new [] { "localNetwork", "wired" }));
	}

	[Test]
	public async Task ListAsync_MergedDevice_PrefersWiredWhenAvailabilityIsEqual ()
	{
		var result = await ListAsync (Document (
			Device ("COREDEVICE-NET", udid: IPhoneUdid, transportType: "localNetwork", connectionState: "disconnected"),
			Device ("COREDEVICE-USB", udid: IPhoneUdid, transportType: "wired", connectionState: "disconnected")));

		Assert.That (result.Devices.Single ().Identifier, Is.EqualTo ("COREDEVICE-USB"));
	}

	[Test]
	public async Task ListAsync_MergedDevice_KeepsReportedOrderWhenConnectionsRankEqually ()
	{
		var result = await ListAsync (Document (
			Device ("COREDEVICE-1", udid: IPhoneUdid, transportType: "wired", connectionState: "connected"),
			Device ("COREDEVICE-2", udid: IPhoneUdid, transportType: "wired", connectionState: "connected")));

		var device = result.Devices.Single ();
		Assert.That (device.Identifier, Is.EqualTo ("COREDEVICE-1"));
		Assert.That (device.Connections.Select (c => c.Identifier), Is.EqualTo (new [] { "COREDEVICE-1", "COREDEVICE-2" }));
	}

	[Test]
	public async Task ListAsync_MergedDevice_FillsMissingMetadataFromTheOtherConnections ()
	{
		var result = await ListAsync (Document (
			Device ("COREDEVICE-USB", udid: IPhoneUdid, name: null, osVersion: null, buildVersion: null, marketingName: null, transportType: "wired", connectionState: "connected"),
			Device ("COREDEVICE-NET", udid: IPhoneUdid, name: "Test iPhone", osVersion: "17.5", buildVersion: "21F79", marketingName: "iPhone 15", ecid: 99, serialNumber: "SERIAL1", transportType: "localNetwork", connectionState: "disconnected")));

		var device = result.Devices.Single ();
		Assert.That (device.Name, Is.EqualTo ("Test iPhone"));
		Assert.That (device.OSVersion, Is.EqualTo ("17.5"));
		Assert.That (device.BuildVersion, Is.EqualTo ("21F79"));
		Assert.That (device.MarketingName, Is.EqualTo ("iPhone 15"));
		Assert.That (device.SerialNumber, Is.EqualTo ("SERIAL1"));
		Assert.That (device.UniqueChipID, Is.EqualTo (99UL));
		// The connection-level properties still describe the preferred connection.
		Assert.That (device.Identifier, Is.EqualTo ("COREDEVICE-USB"));
		Assert.That (device.TransportType, Is.EqualTo ("wired"));
		Assert.That (device.ConnectionState, Is.EqualTo ("connected"));
	}

	[Test]
	public async Task ListAsync_MergedDevice_DoesNotOverwriteMetadataItAlreadyHas ()
	{
		var result = await ListAsync (Document (
			Device ("COREDEVICE-USB", udid: IPhoneUdid, name: "Preferred", osVersion: "18.1", transportType: "wired", connectionState: "connected"),
			Device ("COREDEVICE-NET", udid: IPhoneUdid, name: "Other", osVersion: "17.0", transportType: "localNetwork", connectionState: "disconnected")));

		var device = result.Devices.Single ();
		Assert.That (device.Name, Is.EqualTo ("Preferred"));
		Assert.That (device.OSVersion, Is.EqualTo ("18.1"));
	}

	[Test]
	public async Task ListAsync_MergesHardwareUdidsCaseInsensitively ()
	{
		var result = await ListAsync (Document (
			Device ("COREDEVICE-1", udid: IPhoneUdid.ToUpperInvariant (), transportType: "wired"),
			Device ("COREDEVICE-2", udid: IPhoneUdid.ToLowerInvariant (), transportType: "localNetwork")));

		Assert.That (result.Devices, Has.Count.EqualTo (1));
		Assert.That (result.Devices [0].Connections, Has.Count.EqualTo (2));
	}

	[Test]
	public async Task ListAsync_SameEntryReportedTwice_ListsTheConnectionOnce ()
	{
		var result = await ListAsync (Document (
			Device ("COREDEVICE-1", udid: IPhoneUdid),
			Device ("COREDEVICE-1", udid: IPhoneUdid)));

		var device = result.Devices.Single ();
		Assert.That (device.Connections, Has.Count.EqualTo (1));
	}

	[Test]
	public async Task ListAsync_DevicesWithoutHardwareUdid_AreNeverMerged ()
	{
		var result = await ListAsync (Document (
			Device ("COREDEVICE-1", udid: null, name: "Same name", transportType: "wired"),
			Device ("COREDEVICE-2", udid: null, name: "Same name", transportType: "localNetwork"),
			Device ("COREDEVICE-3", udid: IPhoneUdid, name: "Same name", transportType: "wired")));

		Assert.That (result.Devices, Has.Count.EqualTo (3));
		Assert.That (result.Devices.Select (d => d.Identifier), Is.EqualTo (new [] { "COREDEVICE-1", "COREDEVICE-2", "COREDEVICE-3" }));
		Assert.That (result.Devices.Select (d => d.HasHardwareUdid), Is.EqualTo (new [] { false, false, true }));
		Assert.That (result.Devices.Select (d => d.Connections.Count), Is.EqualTo (new [] { 1, 1, 1 }));
	}

	[Test]
	public async Task ListAsync_MergedDevices_KeepTheOrderOfFirstAppearance ()
	{
		var result = await ListAsync (Document (
			Device ("COREDEVICE-PHONE-1", udid: IPhoneUdid, name: "Phone", transportType: "localNetwork", connectionState: "disconnected"),
			Device ("COREDEVICE-PAD", udid: IPadUdid, name: "Pad"),
			Device ("COREDEVICE-PHONE-2", udid: IPhoneUdid, name: "Phone", transportType: "wired", connectionState: "connected")));

		Assert.That (result.Devices.Select (d => d.HardwareUdid), Is.EqualTo (new [] { IPhoneUdid, IPadUdid }));
		Assert.That (result.Devices [0].Connections, Has.Count.EqualTo (2));
		Assert.That (result.Devices [1].Connections, Has.Count.EqualTo (1));
	}

	[Test]
	public async Task ListAsync_MinimalEntry_LeavesOptionalMetadataEmpty ()
	{
		var result = await ListAsync (Document ("{\"identifier\":\"COREDEVICE-1\"}"));

		var device = result.Devices.Single ();
		Assert.That (device.Identifier, Is.EqualTo ("COREDEVICE-1"));
		Assert.That (device.Name, Is.Empty);
		Assert.That (device.OSVersion, Is.Empty);
		Assert.That (device.BuildVersion, Is.Empty);
		Assert.That (device.MarketingName, Is.Empty);
		Assert.That (device.CpuArchitecture, Is.Empty);
		Assert.That (device.UniqueChipID, Is.Null);
		Assert.That (device.Kind, Is.EqualTo (PhysicalDeviceKind.Unknown));
		Assert.That (device.Availability, Is.EqualTo (PhysicalDeviceAvailability.Unknown));
		Assert.That (device.Connections, Has.Count.EqualTo (1));
	}

	#endregion

	#region Connection states

	[TestCase ("connected", PhysicalDeviceAvailability.Connected, true)]
	[TestCase ("disconnected", PhysicalDeviceAvailability.Available, true)]
	[TestCase ("unavailable", PhysicalDeviceAvailability.Unavailable, false)]
	[TestCase (null, PhysicalDeviceAvailability.Unknown, false)]
	[TestCase ("someFutureState", PhysicalDeviceAvailability.Unknown, false)]
	public async Task ListAsync_ConnectionState_IsMappedToAvailability (string? connectionState, PhysicalDeviceAvailability expected, bool reachable)
	{
		var result = await ListAsync (Document (Device ("COREDEVICE-1", udid: IPhoneUdid, connectionState: connectionState)));

		var device = result.Devices.Single ();
		Assert.That (device.Availability, Is.EqualTo (expected));
		Assert.That (device.IsReachable, Is.EqualTo (reachable));
		Assert.That (device.ConnectionState, Is.EqualTo (connectionState ?? ""));
	}

	[Test]
	public async Task ListAsync_PairedButUnavailableDevice_IsNotReachable ()
	{
		var result = await ListAsync (Document (Device ("COREDEVICE-1", udid: IPhoneUdid, pairingState: "paired", connectionState: "unavailable")));

		var device = result.Devices.Single ();
		Assert.That (device.PairingState, Is.EqualTo ("paired"));
		Assert.That (device.Availability, Is.EqualTo (PhysicalDeviceAvailability.Unavailable));
		Assert.That (device.IsReachable, Is.False);
	}

	[Test]
	public async Task ListAsync_PairedDeviceWithoutConnectionState_IsUnknownRatherThanConnected ()
	{
		var result = await ListAsync (Document (Device ("COREDEVICE-1", udid: IPhoneUdid, pairingState: "paired", connectionState: null)));

		var device = result.Devices.Single ();
		Assert.That (device.PairingState, Is.EqualTo ("paired"));
		Assert.That (device.Availability, Is.EqualTo (PhysicalDeviceAvailability.Unknown));
		Assert.That (device.IsReachable, Is.False);
	}

	[Test]
	public async Task ListAsync_UnpairedDevice_KeepsItsPairingState ()
	{
		var result = await ListAsync (Document (Device ("COREDEVICE-1", udid: IPhoneUdid, pairingState: "unpaired", connectionState: "unavailable")));

		var device = result.Devices.Single ();
		Assert.That (device.PairingState, Is.EqualTo ("unpaired"));
		Assert.That (device.Availability, Is.EqualTo (PhysicalDeviceAvailability.Unavailable));
	}

	[Test]
	public async Task ListAsync_MissingPairingState_IsEmpty ()
	{
		var result = await ListAsync (Document (Device ("COREDEVICE-1", udid: IPhoneUdid, pairingState: null)));

		Assert.That (result.Devices.Single ().PairingState, Is.Empty);
	}

	#endregion

	#region JSON layouts

	[Test]
	public async Task ListAsync_LegacyLayout_ReturnsDevices ()
	{
		var result = await ListAsync (LegacyDocument (LegacyDevice (
			IPhoneIdentifier,
			udid: IPhoneUdid,
			name: "Test iPhone",
			tunnelState: "connected")));

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success));
		var device = result.Devices.Single ();
		Assert.That (device.Name, Is.EqualTo ("Test iPhone"));
		Assert.That (device.HardwareUdid, Is.EqualTo (IPhoneUdid));
		Assert.That (device.Identifier, Is.EqualTo (IPhoneIdentifier));
		Assert.That (device.Kind, Is.EqualTo (PhysicalDeviceKind.IPhone));
		Assert.That (device.OSVersion, Is.EqualTo ("18.1"));
		Assert.That (device.BuildVersion, Is.EqualTo ("22B83"));
		Assert.That (device.CpuArchitecture, Is.EqualTo ("arm64e"));
		Assert.That (device.TransportType, Is.EqualTo ("wired"));
		Assert.That (device.PairingState, Is.EqualTo ("paired"));
		Assert.That (device.Availability, Is.EqualTo (PhysicalDeviceAvailability.Connected));
	}

	[Test]
	public async Task ListAsync_LegacyLayout_MergesTransportsToo ()
	{
		var result = await ListAsync (LegacyDocument (
			LegacyDevice ("COREDEVICE-NET", udid: IPhoneUdid, transportType: "localNetwork", tunnelState: "disconnected"),
			LegacyDevice ("COREDEVICE-USB", udid: IPhoneUdid, transportType: "wired", tunnelState: "connected")));

		var device = result.Devices.Single ();
		Assert.That (device.Identifier, Is.EqualTo ("COREDEVICE-USB"));
		Assert.That (device.Connections, Has.Count.EqualTo (2));
	}

	[Test]
	public async Task ListAsync_LegacyUnavailableDevice_IsNotReachable ()
	{
		var result = await ListAsync (LegacyDocument (LegacyDevice ("COREDEVICE-1", udid: IPhoneUdid, transportType: null, tunnelState: "unavailable")));

		var device = result.Devices.Single ();
		Assert.That (device.Availability, Is.EqualTo (PhysicalDeviceAvailability.Unavailable));
		Assert.That (device.TransportType, Is.Empty);
	}

	#endregion

	#region Result and diagnostics

	[Test]
	public async Task ListAsync_ReturnsAReadOnlyListOfDevices ()
	{
		var result = await ListAsync (Document (Device ("COREDEVICE-1", udid: IPhoneUdid)));

		var devices = (System.Collections.Generic.ICollection<PhysicalDeviceInfo>) result.Devices;
		Assert.That (devices.IsReadOnly, Is.True);
	}

	[Test]
	public async Task ListAsync_Success_LogsASummaryThroughTheLogger ()
	{
		await ListAsync (Document (
			Device ("SIM-1", udid: "SIM-1", reality: "simulated"),
			Device ("COREDEVICE-1", udid: IPhoneUdid, transportType: "wired"),
			Device ("COREDEVICE-2", udid: IPhoneUdid, transportType: "localNetwork")));

		Assert.That (log.Messages, Has.Some.Contain ("devicectl reported 3 entries: ignored 1 simulator(s), merged 2 physical entries into 1 device(s)."));
		Assert.That (log.Warnings, Is.Empty);
	}

	#endregion
}
