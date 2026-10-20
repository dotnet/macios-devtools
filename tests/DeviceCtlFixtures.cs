// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Linq;

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

	/// <summary>A devicectl document (JSON version 5) containing the given device entries.</summary>
	public static string Document (params string [] devices) => Wrap (5, devices);

	/// <summary>A devicectl document in the older JSON layout containing the given device entries.</summary>
	public static string LegacyDocument (params string [] devices) => Wrap (2, devices);

	/// <summary>What devicectl writes when a command fails.</summary>
	public static string ErrorDocument (string description) =>
		"{\"error\":{\"code\":1000,\"domain\":\"com.apple.dt.CoreDeviceError\",\"userInfo\":{\"NSLocalizedDescription\":{\"string\":\"" + description + "\"}}},\"info\":{\"outcome\":\"failed\"}}";

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
