// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.MacDev;
using Xamarin.MacDev.Models;
using static tests.DeviceCtlFixtures;

#nullable enable

namespace tests;

/// <summary>
/// Runs the devicectl scenarios of the dotnet/macios <c>GetAvailableDevicesTests</c> through <see cref="PhysicalDeviceService"/>,
/// the way that task could use it: with the same documents, faking devicectl the same way (a subclass overrides the method that
/// runs it, as their <c>GetAvailableDevicesTaskWrapper</c> overrides <c>ExecuteCtlAsync</c>) and with the same expectations,
/// where <see cref="MaciosDeviceMapping"/> stands in for the task. The scenarios that need simctl are not repeated here.
/// </summary>
[TestFixture]
public class MaciosParityTests {

	/// <summary>The counterpart of <c>GetAvailableDevicesTaskWrapper</c> in the macios tests.</summary>
	sealed class PhysicalDeviceServiceWrapper : PhysicalDeviceService {

		public DeviceCtlRunResult DeviceCtlRun = DeviceCtlRunResult.FromJson (string.Empty);

		public PhysicalDeviceServiceWrapper (ICustomLogger log)
			: base (log)
		{
		}

		protected override Task<DeviceCtlRunResult> ExecuteDeviceCtlAsync (CancellationToken cancellationToken)
			=> Task.FromResult (DeviceCtlRun);
	}

	RecordingLogger log = new RecordingLogger ();

	[SetUp]
	public void SetUp ()
	{
		log = new RecordingLogger ();
	}

	Task<PhysicalDeviceDiscoveryResult> ListAsync (DeviceCtlRunResult run)
		=> new PhysicalDeviceServiceWrapper (log) { DeviceCtlRun = run }.ListAsync ();

	Task<PhysicalDeviceDiscoveryResult> ListAsync (string deviceCtlJson)
		=> ListAsync (DeviceCtlRunResult.FromJson (deviceCtlJson));

	#region The macios expectations

	[Test]
	[TestCase ("")]
	[TestCase ("{}")]
	[TestCase ("[]")]
	[TestCase ("{\"result\" : {} }")]
	[TestCase ("{\"result\" : { \"devices\": {} } }")]
	public async Task EmptyJsons (string deviceCtlJson)
	{
		var discovery = await ListAsync (deviceCtlJson);
		var task = MaciosDeviceMapping.Map (discovery.Devices);

		Assert.That (task.Devices.Count, Is.EqualTo (0), "Devices should be empty.");
		Assert.That (task.DiscardedDevices.Count, Is.EqualTo (0), "No devices should have been discarded.");

		// The parser the task uses takes all of these for an empty list. The service tells them from one:
		// it reports that it cannot use the output, which comes to the same for a task that lists no devices.
		Assert.That (discovery.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.InvalidOutput));
	}

	[Test]
	public async Task DeviceCtl1 ()
	{
		var discovery = await ListAsync (MaciosDeviceCtlFixtures.DeviceCtlJson1);
		var task = MaciosDeviceMapping.Map (discovery.Devices);

		Assert.That (discovery.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success), "Discovery should have succeeded.");
		Assert.That (task.Devices.Count, Is.EqualTo (3), "Devices count mismatch.");
		Assert.That (task.DiscardedDevices.Count, Is.EqualTo (1), "Discarded device count mismatch.");
		Assert.Multiple (() => {
			Assert.That (task.Devices [0].ItemSpec, Is.EqualTo ("00008003-012301230123ABCD"), "Device 1 mismatch.");
			Assert.That (task.Devices [0].Description, Is.EqualTo ("Rolf's iPhone 15 - iOS 26.1"), "Device 1 Name mismatch.");
			Assert.That (task.Devices [0].OSVersion, Is.EqualTo ("26.1"), "Device 1 Platform mismatch.");
			Assert.That (task.Devices [0].Udid, Is.EqualTo ("00008003-012301230123ABCD"), "Device 1 UDID mismatch.");
			Assert.That (task.Devices [0].RuntimeIdentifier, Is.EqualTo ("ios-arm64"), "Device 1 RuntimeIdentifier mismatch.");
			Assert.That (task.Devices [0].Status, Is.EqualTo ("Paired"), "Device 1 Status mismatch.");
			Assert.That (task.Devices [0].DiscardedReason, Is.Empty, "Device 1 discarded reason mismatch.");

			Assert.That (task.Devices [1].ItemSpec, Is.EqualTo ("00008001-012301230123ABCD"), "Device 2 mismatch.");
			Assert.That (task.Devices [1].Description, Is.EqualTo ("Rolf's iPad Pro 3rd Gen - iOS 26.0"), "Device 2 Name mismatch.");
			Assert.That (task.Devices [1].OSVersion, Is.EqualTo ("26.0"), "Device 2 Platform mismatch.");
			Assert.That (task.Devices [1].Udid, Is.EqualTo ("00008001-012301230123ABCD"), "Device 2 UDID mismatch.");
			Assert.That (task.Devices [1].RuntimeIdentifier, Is.EqualTo ("ios-arm64"), "Device 2 RuntimeIdentifier mismatch.");
			Assert.That (task.Devices [1].Status, Is.EqualTo ("Unpaired"), "Device 2 Status mismatch.");
			Assert.That (task.Devices [1].DiscardedReason, Is.Empty, "Device 2 discarded reason mismatch.");

			Assert.That (task.Devices [2].ItemSpec, Is.EqualTo ("00008002-012301230123ABCD"), "Device 3 mismatch.");
			Assert.That (task.Devices [2].Description, Is.EqualTo ("Rolf's iPhone 13 - iOS 18.7.1"), "Device 3 Name mismatch.");
			Assert.That (task.Devices [2].OSVersion, Is.EqualTo ("18.7.1"), "Device 3 Platform mismatch.");
			Assert.That (task.Devices [2].Udid, Is.EqualTo ("00008002-012301230123ABCD"), "Device 3 UDID mismatch.");
			Assert.That (task.Devices [2].RuntimeIdentifier, Is.EqualTo ("ios-arm64"), "Device 3 RuntimeIdentifier mismatch.");
			Assert.That (task.Devices [2].Status, Is.EqualTo ("Paired"), "Device 3 Status mismatch.");
			Assert.That (task.Devices [2].DiscardedReason, Is.Empty, "Device 3 discarded reason mismatch.");

			// The apostrophe of this device name is U+2019, not the ASCII one of the other names, and a non-breaking space (U+00A0) separates "Apple" and "Watch".
			Assert.That (task.DiscardedDevices [0].ItemSpec, Is.EqualTo ("00008004-012301230123ABCD"), "Discarded Device 1 ItemSpec mismatch.");
			Assert.That (task.DiscardedDevices [0].Description, Is.EqualTo ("Rolf\u2019s Apple\u00a0Watch Series 7 - watchOS 11.5"), "Discarded Device 1 Description mismatch.");
			Assert.That (task.DiscardedDevices [0].OSVersion, Is.EqualTo ("11.5"), "Discarded Device 1 OSVersion mismatch.");
			Assert.That (task.DiscardedDevices [0].Udid, Is.EqualTo ("00008004-012301230123ABCD"), "Discarded Device 1 UDID mismatch.");
			Assert.That (task.DiscardedDevices [0].RuntimeIdentifier, Is.EqualTo (""), "Discarded Device 1 RuntimeIdentifier mismatch.");
			Assert.That (task.DiscardedDevices [0].DiscardedReason, Is.EqualTo ("'appleWatch' devices are not supported"), "Discarded Device 1 reason mismatch.");
		});
	}

	[Test]
	public async Task DeviceCtl2_Mac ()
	{
		var discovery = await ListAsync (MaciosDeviceCtlFixtures.DeviceCtlJson2);
		var task = MaciosDeviceMapping.Map (discovery.Devices);

		Assert.That (discovery.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success), "Discovery should have succeeded.");
		Assert.That (task.DiscardedDevices.Count, Is.EqualTo (1), "Discarded device count mismatch.");
		Assert.Multiple (() => {
			Assert.That (task.DiscardedDevices [0].ItemSpec, Is.EqualTo ("12345678-1234-1234-ABCD-1234567980AB"), "Discarded Device 1 itemspec mismatch.");
			Assert.That (task.DiscardedDevices [0].Description, Is.EqualTo (""), "Discarded Device 1 Name mismatch.");
			Assert.That (task.DiscardedDevices [0].OSVersion, Is.EqualTo (""), "Discarded Device 1 OSVersion mismatch.");
			Assert.That (task.DiscardedDevices [0].Udid, Is.EqualTo ("12345678-1234-1234-ABCD-1234567980AB"), "Discarded Device 1 UDID mismatch.");
			Assert.That (task.DiscardedDevices [0].RuntimeIdentifier, Is.EqualTo (""), "Discarded Device 1 RuntimeIdentifier mismatch.");
			Assert.That (task.DiscardedDevices [0].DiscardedReason, Is.EqualTo ("'mac' devices are not supported"), "Discarded Device 1 reason mismatch.");
		});
	}

	[Test]
	public async Task DeviceCtlJsonVersion5 ()
	{
		// macios has this test too, but without the [Test] attribute, so it never runs there.
		var discovery = await ListAsync (MaciosDeviceCtlFixtures.DeviceCtlJsonVersion5);
		var task = MaciosDeviceMapping.Map (discovery.Devices);

		Assert.That (discovery.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success), "Discovery should have succeeded.");
		Assert.That (task.Devices.Count, Is.EqualTo (1), "Devices count mismatch.");
		Assert.Multiple (() => {
			Assert.That (task.Devices [0].ItemSpec, Is.EqualTo ("00008003-012301230123ABCD"), "Device UDID mismatch.");
			Assert.That (task.Devices [0].Description, Is.EqualTo ("Rolf's iPhone 15 - iOS 27.0"), "Device description mismatch.");
			Assert.That (task.Devices [0].RuntimeIdentifier, Is.EqualTo ("ios-arm64"), "Device RuntimeIdentifier mismatch.");
			Assert.That (task.Devices [0].Status, Is.EqualTo ("Paired"), "Device Status mismatch.");
		});
	}

	#endregion

	#region Where the service tells more than the parser the task uses

	[Test]
	public async Task EmptyDeviceList_IsSuccessWithoutDevices ()
	{
		var discovery = await ListAsync ("{\"result\":{\"devices\":[]}}");

		Assert.That (discovery.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success));
		Assert.That (discovery.Devices, Is.Empty);
		Assert.That (MaciosDeviceMapping.Map (discovery.Devices).Devices, Is.Empty);
	}

	[Test]
	public async Task FailedDiscovery_MapsToNoDevices ()
	{
		var failures = new [] {
			DeviceCtlRunResult.NotFound ("xcrun was not found."),
			DeviceCtlRunResult.UnsupportedHost (),
			DeviceCtlRunResult.Completed (1, "", "", ErrorDocument ("CoreDevice is not available.")),
		};

		foreach (var failure in failures) {
			var discovery = await ListAsync (failure);
			var task = MaciosDeviceMapping.Map (discovery.Devices);

			Assert.That (discovery.IsSuccess, Is.False, "Expected a failure, got " + discovery.Status);
			Assert.That (task.Devices, Is.Empty, "A failure is no devices, as with the parser. " + discovery.Status);
			Assert.That (task.DiscardedDevices, Is.Empty, "A failure is no devices, as with the parser. " + discovery.Status);
		}
	}

	[Test]
	public async Task DuplicateConnections_AreOneItemInsteadOfTwoWithTheSameUdid ()
	{
		// The same hardware listed twice, once per connection path (here USB and the network).
		var json = LegacyDocument (
			LegacyDevice ("COREDEVICE-USB", udid: IPhoneUdid, name: "Rolf's iPhone", transportType: "wired", tunnelState: "connected"),
			LegacyDevice ("COREDEVICE-NETWORK", udid: IPhoneUdid, name: "Rolf's iPhone", transportType: "localNetwork", tunnelState: "disconnected"));

		var viaParser = MaciosDeviceMapping.Map (DeviceCtlOutputParser.ParseDevices (json));
		var discovery = await ListAsync (json);
		var viaService = MaciosDeviceMapping.Map (discovery.Devices);

		Assert.That (viaParser.Devices.Select (d => d.ItemSpec), Is.EqualTo (new [] { IPhoneUdid, IPhoneUdid }), "The parser reports every entry, so the same hardware appears twice.");
		Assert.That (viaService.Devices.Select (d => d.ItemSpec), Is.EqualTo (new [] { IPhoneUdid }), "The service reports it once.");
		Assert.That (discovery.Devices [0].Connections, Has.Count.EqualTo (2), "Both connections are kept.");
	}

	[Test]
	public async Task LegacyIPadThatCoreDeviceCannotManage_IsListedWithWhyItIsNotUsable ()
	{
		var discovery = await ListAsync (LegacyIPadDocument);

		Assert.That (discovery.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success));
		Assert.That (discovery.Devices, Has.Count.EqualTo (1));
		var device = discovery.Devices [0];
		Assert.Multiple (() => {
			Assert.That (device.Kind, Is.EqualTo (PhysicalDeviceKind.IPad));
			Assert.That (device.ProductType, Is.EqualTo ("iPad6,3"));
			Assert.That (device.PairingState, Is.EqualTo ("unsupported"));
			Assert.That (device.ConnectionState, Is.EqualTo ("unavailable"));
			Assert.That (device.Availability, Is.EqualTo (PhysicalDeviceAvailability.Unavailable));
			Assert.That (device.HasHardwareUdid, Is.False);
			Assert.That (device.Udid, Is.EqualTo (LegacyIPadIdentifier), "Without a hardware UDID the CoreDevice identifier is all there is to tell it by.");
			Assert.That (device.SerialNumber, Is.EqualTo ("SERIAL_LEGACY"));
			Assert.That (device.UniqueChipID, Is.EqualTo (1234567890123456UL));
			Assert.That (device.Name, Is.Empty);
			Assert.That (device.OSVersion, Is.Empty);
			Assert.That (device.CpuArchitecture, Is.Empty);
		});

		// The task discards such a device because of its missing CPU architecture, which does not say that the device cannot be used.
		// PairingState and Availability say it.
		var task = MaciosDeviceMapping.Map (discovery.Devices);
		Assert.That (task.Devices, Is.Empty);
		Assert.That (task.DiscardedDevices, Has.Count.EqualTo (1));
		Assert.That (task.DiscardedDevices [0].DiscardedReason, Is.EqualTo ("Unknown CPU architecture ''"));
		Assert.That (task.DiscardedDevices [0].Status, Is.EqualTo ("Unsupported"));
	}

	#endregion
}
