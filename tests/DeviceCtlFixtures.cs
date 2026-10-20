// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;

#nullable enable

namespace tests;

/// <summary>
/// Builds <c>xcrun devicectl list devices --json-output</c> documents for tests, modeled on the files
/// devicectl writes. A <c>null</c> argument leaves the property out, as devicectl does for values it does not know.
/// </summary>
static class DeviceCtlFixtures {

	public const string IPhoneUdid = "00008130-001A2B3C4D5E6F78";
	public const string IPadUdid = "00008112-000E1F2A3B4C5D6E";
	public const string WatchUdid = "00008310-000A1B2C3D4E5F60";

	public const string IPhoneIdentifier = "11111111-AAAA-BBBB-CCCC-111111111111";
	public const string IPadIdentifier = "22222222-AAAA-BBBB-CCCC-222222222222";
	public const string WatchIdentifier = "33333333-AAAA-BBBB-CCCC-333333333333";

	public const string LegacyIPadIdentifier = "AAAAAAAA-1111-2222-3333-BBBBBBBBBBBB";

	public const string UnpairedIPhoneIdentifier = "44444444-AAAA-BBBB-CCCC-444444444444";
	public const string UnpairedIPhoneUdid = "00008030-000F1E2D3C4B5A69";

	/// <summary>
	/// What Xcode 27 (devicectl 642.16, JSON version 5) writes for an iPad that CoreDevice cannot manage (an iPad Pro 9.7-inch,
	/// which cannot run iPadOS 17): the device is listed, but with few properties, and without a name, UDID, operating system or CPU.
	/// It has the <c>properties</c> object as well as the deprecated properties it replaces. The identifiers are made up.
	/// </summary>
	public const string LegacyIPadDocument =
	"""
	{
		"info": {
			"arguments": [ "devicectl", "list", "devices", "--json-output", "devices.json" ],
			"commandType": "devicectl.list.devices",
			"environment": {},
			"jsonVersion": 5,
			"outcome": "success",
			"version": "642.16"
		},
		"result": {
			"devices": [
				{
					"_deprecationNotice": {
						"deprecatedFields": [ "hardwareProperties", "deviceProperties", "connectionProperties" ],
						"message": "The 'hardwareProperties', 'deviceProperties', 'connectionProperties' fields are deprecated and will be removed in a future release. Use the 'properties' dictionary instead.",
						"replacement": "properties"
					},
					"capabilities": [],
					"connectionProperties": {
						"isMobileDeviceOnly": false,
						"pairingState": "unsupported",
						"potentialHostnames": [ "AAAAAAAA-1111-2222-3333-BBBBBBBBBBBB.coredevice.local" ],
						"tunnelState": "unavailable"
					},
					"deviceProperties": {
						"bootState": "booted",
						"ddiServicesAvailable": false
					},
					"hardwareProperties": {
						"deviceType": "iPad",
						"ecid": 1234567890123456,
						"isProductionFused": true,
						"platform": "iOS",
						"productType": "iPad6,3",
						"serialNumber": "SERIAL_LEGACY"
					},
					"identifier": "AAAAAAAA-1111-2222-3333-BBBBBBBBBBBB",
					"properties": {
						"connection": {
							"pairingState": "unsupported"
						},
						"hardware": {
							"deviceType": "iPad",
							"ecid": 1234567890123456,
							"platform": "iOS",
							"productType": "iPad6,3",
							"serialNumber": "SERIAL_LEGACY"
						},
						"state": {
							"bootState": "booted"
						}
					},
					"propertyDisplayNames": null,
					"visibilityClass": "default"
				}
			]
		}
	}
	""";

	/// <summary>
	/// What Xcode 27 (devicectl 642.16, JSON version 5) writes for an iPhone that CoreDevice can manage, connected over USB, that has not been
	/// paired with the Mac: all of the <c>properties</c> object, the pairing state <c>unpaired</c> and the connection state <c>disconnected</c>
	/// (which devicectl shows as "available"). Like all version 5 output it also has the deprecated properties, and two values are only there:
	/// the hardware model and the name of the CPU type, whose <c>subtype</c> is written unsigned in <c>properties</c> and signed in the
	/// deprecated object. The identifiers, the name and the host names are made up, and what does not matter here (the capabilities and the
	/// details of the build version) is left out. <see cref="WithoutDeprecatedProperties"/> gives the output without the deprecated properties.
	/// </summary>
	public const string UnpairedIPhoneDocument =
	"""
	{
		"info": {
			"arguments": [ "devicectl", "list", "devices", "--json-output", "devices.json" ],
			"commandType": "devicectl.list.devices",
			"environment": {},
			"jsonVersion": 5,
			"outcome": "success",
			"version": "642.16"
		},
		"result": {
			"devices": [
				{
					"_deprecationNotice": {
						"deprecatedFields": [ "hardwareProperties", "deviceProperties", "connectionProperties" ],
						"message": "The 'hardwareProperties', 'deviceProperties', 'connectionProperties' fields are deprecated and will be removed in a future release. Use the 'properties' dictionary instead.",
						"replacement": "properties"
					},
					"capabilities": [],
					"connectionProperties": {
						"isMobileDeviceOnly": false,
						"pairingState": "unpaired",
						"potentialHostnames": [
							"Test-iPhone.coredevice.local",
							"44444444-AAAA-BBBB-CCCC-444444444444.coredevice.local",
							"00008030-000F1E2D3C4B5A69.coredevice.local"
						],
						"transportType": "wired",
						"tunnelState": "disconnected",
						"tunnelTransportProtocol": "tcp"
					},
					"deviceProperties": {
						"bootState": "booted",
						"ddiServicesAvailable": false,
						"name": "Test iPhone",
						"osBuildUpdate": "24B5089g",
						"osVersionNumber": "27.2"
					},
					"hardwareProperties": {
						"cpuType": {
							"name": "arm64e",
							"subType": -2147483646,
							"type": 16777228
						},
						"deviceType": "iPhone",
						"ecid": 2345678901234567,
						"hardwareModel": "N104AP",
						"isProductionFused": true,
						"platform": "iOS",
						"productType": "iPhone12,1",
						"supportedCPUTypes": [
							{ "name": "arm64e", "subType": -2147483646, "type": 16777228 },
							{ "name": "arm64", "subType": 0, "type": 16777228 }
						],
						"supportedDeviceFamilies": [ 1 ],
						"udid": "00008030-000F1E2D3C4B5A69"
					},
					"identifier": "44444444-AAAA-BBBB-CCCC-444444444444",
					"properties": {
						"connection": {
							"pairingState": "unpaired",
							"state": "disconnected",
							"transportType": "wired",
							"tunnelTransportProtocol": "tcp"
						},
						"hardware": {
							"cpuType": {
								"subtype": 18446744071562067970,
								"type": 16777228
							},
							"deviceType": "iPhone",
							"ecid": 2345678901234567,
							"platform": "iOS",
							"productType": "iPhone12,1",
							"supportedCPUTypes": [
								{ "subtype": 18446744071562067970, "type": 16777228 },
								{ "subtype": 0, "type": 16777228 }
							],
							"supportedDeviceFamilies": [ 1 ],
							"udid": "00008030-000F1E2D3C4B5A69"
						},
						"software": {
							"osBuildVersions": {
								"buildVersion": {
									"name": "24B5089g"
								}
							},
							"osVersionNumber": {
								"stringValue": "27.2"
							}
						},
						"state": {
							"bootState": "booted",
							"name": "Test iPhone"
						}
					},
					"propertyDisplayNames": null,
					"visibilityClass": "default"
				}
			]
		}
	}
	""";

	/// <summary>A devicectl document (JSON version 5) containing the given device entries.</summary>
	public static string Document (params string [] devices) => Wrap (5, devices);

	/// <summary>A devicectl document in the older JSON layout containing the given device entries.</summary>
	public static string LegacyDocument (params string [] devices) => Wrap (2, devices);

	/// <summary>What devicectl writes when a command fails.</summary>
	public static string ErrorDocument (string description) =>
		"{" + ErrorObject (description) + ",\"info\":{\"outcome\":\"failed\"}}";

	/// <summary>
	/// An error report that also carries a device list. devicectl does not write one,
	/// but a list next to an error must not be mistaken for the result of a successful run.
	/// </summary>
	public static string ErrorDocumentWithDevices (string description, params string [] devices) =>
		"{" + ErrorObject (description) + ",\"info\":{\"outcome\":\"failed\"},\"result\":{\"devices\":[" + string.Join (",", devices) + "]}}";

	static string ErrorObject (string description) =>
		"\"error\":{\"code\":1000,\"domain\":\"com.apple.dt.CoreDeviceError\",\"userInfo\":{\"NSLocalizedDescription\":{\"string\":\"" + description + "\"}}}";

	/// <summary>
	/// The document as <c>devicectl list devices --omit-deprecated-fields-in-json</c> writes it: without the deprecated properties
	/// (<c>hardwareProperties</c>, <c>deviceProperties</c>, <c>connectionProperties</c> and the notice about them) of every device.
	/// devicectl says that a future release will not write them either.
	/// </summary>
	public static string WithoutDeprecatedProperties (string document)
	{
		var root = JsonNode.Parse (document) ?? throw new ArgumentException ("The document is not JSON.", nameof (document));
		var devices = root ["result"]? ["devices"]?.AsArray () ?? throw new ArgumentException ("The document has no devices.", nameof (document));

		foreach (var device in devices.OfType<JsonObject> ()) {
			foreach (var deprecated in new [] { "_deprecationNotice", "hardwareProperties", "deviceProperties", "connectionProperties" })
				device.Remove (deprecated);
		}

		return root.ToJsonString ();
	}

	/// <summary>A device entry in the JSON version 5 layout (the <c>properties</c> object).</summary>
	public static string Device (
		string identifier,
		string? udid = null,
		string? name = "Test Device",
		string? deviceType = "iPhone",
		string? productType = "iPhone16,1",
		string? platform = "iOS",
		string? hardwareModel = "D83AP",
		string? marketingName = null,
		string? osVersion = "18.1",
		string? buildVersion = "22B83",
		string? transportType = "wired",
		string? pairingState = "paired",
		string? connectionState = "connected",
		string? reality = "physical",
		ulong? ecid = null,
		string? serialNumber = null)
	{
		var hardware = Obj (
			Str ("deviceType", deviceType),
			ecid.HasValue ? Raw ("ecid", ecid.Value.ToString (CultureInfo.InvariantCulture)) : null,
			Str ("hardwareModel", hardwareModel),
			Str ("marketingName", marketingName),
			Str ("platform", platform),
			Str ("productType", productType),
			Str ("reality", reality),
			Str ("serialNumber", serialNumber),
			Str ("udid", udid));

		var software = Obj (
			buildVersion is null ? null : Raw ("osBuildVersions", Obj (Raw ("buildVersion", Obj (Str ("name", buildVersion))))),
			osVersion is null ? null : Raw ("osVersionNumber", Obj (Str ("stringValue", osVersion))));

		return Obj (
			Str ("identifier", identifier),
			Raw ("properties", Obj (
				Raw ("connection", Obj (Str ("pairingState", pairingState), Str ("state", connectionState), Str ("transportType", transportType))),
				Raw ("hardware", hardware),
				Raw ("software", software),
				Raw ("state", Obj (Str ("name", name))))));
	}

	/// <summary>A device entry in the older layout (<c>connectionProperties</c>, <c>deviceProperties</c> and <c>hardwareProperties</c>).</summary>
	public static string LegacyDevice (
		string identifier,
		string? udid = null,
		string? name = "Test Device",
		string? deviceType = "iPhone",
		string? productType = "iPhone16,1",
		string? platform = "iOS",
		string? hardwareModel = "D83AP",
		string? osVersion = "18.1",
		string? buildVersion = "22B83",
		string? transportType = "wired",
		string? pairingState = "paired",
		string? tunnelState = "connected",
		string? cpuName = "arm64e")
	{
		return Obj (
			Raw ("connectionProperties", Obj (Str ("pairingState", pairingState), Str ("transportType", transportType), Str ("tunnelState", tunnelState))),
			Raw ("deviceProperties", Obj (Str ("name", name), Str ("osBuildUpdate", buildVersion), Str ("osVersionNumber", osVersion))),
			Raw ("hardwareProperties", Obj (
				cpuName is null ? null : Raw ("cpuType", Obj (Str ("name", cpuName))),
				Str ("deviceType", deviceType),
				Str ("hardwareModel", hardwareModel),
				Str ("platform", platform),
				Str ("productType", productType),
				Str ("udid", udid))),
			Str ("identifier", identifier));
	}

	static string Wrap (int jsonVersion, string [] devices) =>
		"{\"info\":{\"jsonVersion\":" + jsonVersion.ToString (CultureInfo.InvariantCulture) + ",\"outcome\":\"success\"},\"result\":{\"devices\":[" + string.Join (",", devices) + "]}}";

	static string Obj (params string? [] properties) => "{" + string.Join (",", properties.Where (p => p is not null)) + "}";

	static string? Str (string name, string? value) => value is null ? null : "\"" + name + "\":\"" + value + "\"";

	static string Raw (string name, string json) => "\"" + name + "\":" + json;
}
