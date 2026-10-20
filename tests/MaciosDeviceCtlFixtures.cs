// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace tests;

/// <summary>
/// The devicectl documents the dotnet/macios <c>GetAvailableDevicesTests</c> use, copied from
/// <c>tests/msbuild/Xamarin.MacDev.Tasks.Tests/TaskTests/GetAvailableDevicesTest.cs</c> in https://github.com/dotnet/macios
/// (MIT licensed), so that the same inputs are checked here. The comments give the names they have there.
/// They are identical to the originals, except that the non-breaking spaces of <see cref="DeviceCtlJson1"/> are written as an escape.
/// The UDIDs, identifiers and serial numbers are made up.
/// </summary>
static class MaciosDeviceCtlFixtures {

	/// <summary>
	/// <c>DEVICECTL_JSON_1</c>: four devices in the JSON version 2 layout of Xcode 26 (an iPad, two iPhones and an Apple Watch).
	/// It contains trailing commas, which strict JSON parsers reject. The name and the marketing name of the Apple Watch have a
	/// non-breaking space (U+00A0) between "Apple" and "Watch", as they do in the original.
	/// </summary>
	public static readonly string DeviceCtlJson1 = DeviceCtlJson1Source.Replace ("Apple Watch Series 7", "Apple\u00a0Watch Series 7");

	// The original has those non-breaking spaces in the text itself. An editor or a tool can silently turn such a character into a
	// plain space, so they are written as an escape and put in above.
	const string DeviceCtlJson1Source =
	"""
	{
	"info" : {
		"arguments" : [
		"devicectl",
		"list",
		"devices",
		"--json-output",
		"x.json"
		],
		"commandType" : "devicectl.list.devices",
		"environment" : {
		"TERM" : "xterm-256color"
		},
		"jsonVersion" : 2,
		"outcome" : "success",
		"version" : "477.39"
	},
	"result" : {
		"devices" : [
		{
			"connectionProperties" : {
			"tunnelState" : "unavailable"
			},
			"deviceProperties" : {
			"name" : "Rolf's iPad Pro 3rd Gen",
			"osBuildUpdate" : "23A341",
			"osVersionNumber" : "26.0",
			"releaseType" : "Beta"
			},
			"hardwareProperties" : {
			"cpuType" : {
				"name" : "arm64e",
				"subType" : 2,
				"type" : 16777228
			},
			"deviceType" : "iPad",
			"ecid" : 18446744073709551615,
			"hardwareModel" : "J317AP",
			"internalStorageCapacity" : 64000000000,
			"isProductionFused" : true,
			"marketingName" : "iPad Pro (11-inch)",
			"platform" : "iOS",
			"productType" : "iPad8,1",
			"reality" : "physical",
			"serialNumber" : "SERIAL_1",
			"supportedCPUTypes" : [
				{
				"name" : "arm64e",
				"subType" : 2,
				"type" : 16777228
				},
				{
				"name" : "arm64",
				"subType" : 0,
				"type" : 16777228
				}
			],
			"supportedDeviceFamilies" : [
				1,
				2
			],
			"thinningProductType" : "iPad8,1",
			"udid" : "00008001-012301230123ABCD"
			},
			"identifier" : "11111111-AAAA-BBBB-CCCC-DDDDDDDDDDDD",
			"tags" : [

			],
			"visibilityClass" : "default"
		},
		{
			"connectionProperties" : {
			"pairingState" : "paired",
			},
			"deviceProperties" : {
			"bootState" : "booted",
			"name" : "Rolf's iPhone 13",
			"osBuildUpdate" : "22H31",
			"osVersionNumber" : "18.7.1"
			},
			"hardwareProperties" : {
			"cpuType" : {
				"name" : "arm64e",
				"subType" : 2,
				"type" : 16777228
			},
			"deviceType" : "iPhone",
			"ecid" : 0,
			"hardwareModel" : "D63AP",
			"internalStorageCapacity" : 128000000000,
			"isProductionFused" : true,
			"marketingName" : "iPhone 13 Pro",
			"platform" : "iOS",
			"productType" : "iPhone14,2",
			"reality" : "physical",
			"serialNumber" : "SERIAL_2",
			"supportedCPUTypes" : [
				{
				"name" : "arm64e",
				"subType" : 2,
				"type" : 16777228
				},
				{
				"name" : "arm64",
				"subType" : 0,
				"type" : 16777228
				},
				{
				"name" : "armv8",
				"subType" : 13,
				"type" : 12
				}
			],
			"supportedDeviceFamilies" : [
				1
			],
			"thinningProductType" : "iPhone14,2",
			"udid" : "00008002-012301230123ABCD"
			},
			"identifier" : "22222222-AAAA-BBBB-CCCC-DDDDDDDDDDDD",
			"tags" : [

			],
			"visibilityClass" : "default"
		},
		{
			"connectionProperties" : {
			"pairingState" : "paired",
			"tunnelState" : "unavailable"
			},
			"deviceProperties" : {
			"name" : "Rolf's iPhone 15",
			"osBuildUpdate" : "23B85",
			"osVersionNumber" : "26.1"
			},
			"hardwareProperties" : {
			"cpuType" : {
				"name" : "arm64e",
				"subType" : 2,
				"type" : 16777228
			},
			"deviceType" : "iPhone",
			"ecid" : 1,
			"hardwareModel" : "D83AP",
			"internalStorageCapacity" : 128000000000,
			"isProductionFused" : true,
			"marketingName" : "iPhone 15 Pro",
			"platform" : "iOS",
			"productType" : "iPhone16,1",
			"reality" : "physical",
			"serialNumber" : "SERIAL_3",
			"supportedCPUTypes" : [
				{
				"name" : "arm64e",
				"subType" : 2,
				"type" : 16777228
				},
				{
				"name" : "arm64",
				"subType" : 0,
				"type" : 16777228
				}
			],
			"supportedDeviceFamilies" : [
				1
			],
			"thinningProductType" : "iPhone16,1",
			"udid" : "00008003-012301230123ABCD"
			},
			"identifier" : "33333333-AAAA-BBBB-CCCC-DDDDDDDDDDDD",
			"tags" : [

			],
			"visibilityClass" : "default"
		},
		{
			"connectionProperties" : {
			"pairingState" : "paired",
			"transportType" : "localNetwork",
			"tunnelState" : "disconnected",
			"tunnelTransportProtocol" : "tcp"
			},
			"deviceProperties" : {
			"name" : "Rolf’s Apple Watch Series 7",
			"osBuildUpdate" : "22T572",
			"osVersionNumber" : "11.5",
			},
			"hardwareProperties" : {
			"cpuType" : {
				"name" : "arm64_32",
				"subType" : 1,
				"type" : 33554444
			},
			"deviceType" : "appleWatch",
			"ecid" : 2,
			"hardwareModel" : "N187sAP",
			"isProductionFused" : true,
			"marketingName" : "Apple Watch Series 7",
			"platform" : "watchOS",
			"productType" : "Watch6,6",
			"reality" : "physical",
			"serialNumber" : "SERIAL_4",
			"thinningProductType" : "Watch6,6",
			"udid" : "00008004-012301230123ABCD"
			},
			"identifier" : "44444444-AAAA-BBBB-CCCC-DDDDDDDDDDDD",
			"tags" : [

			],
			"visibilityClass" : "default"
		}
		]
	}
	}
	""";

	/// <summary>
	/// <c>DEVICECTL_JSON_2</c>: a Mac, which devicectl describes with far fewer properties than a phone
	/// (no name, UDID, operating system or CPU).
	/// </summary>
	public const string DeviceCtlJson2 =
	"""
	{
		"result" : {
			"devices" : [
			{
				"connectionProperties" : {
				"isMobileDeviceOnly" : false,
				"pairingState" : "unsupported",
				"potentialHostnames" : [
					"12345678-1234-1234-ABCD-1234567980AB.coredevice.local"
				],
				"tunnelState" : "unavailable"
				},
				"deviceProperties" : {
				"bootState" : "booted",
				"ddiServicesAvailable" : false,
				"providerSpecificValues" : {
					"hasAMRestorableDeviceRef" : "true"
				}
				},
				"hardwareProperties" : {
				"deviceType" : "mac",
				"platform" : "macOS",
				"productType" : "MacBookPro17,1"
				},
				"identifier" : "12345678-1234-1234-ABCD-1234567980AB",
				"visibilityClass" : "default"
			}
			]
		}
	}
	""";

	/// <summary>
	/// <c>DEVICECTL_JSON_VERSION_5</c>: an iPhone in the JSON version 5 layout, which has a <c>properties</c> object instead of
	/// <c>hardwareProperties</c>, <c>deviceProperties</c> and <c>connectionProperties</c>. It was written by hand, so it
	/// does not have exactly the shape devicectl produces.
	/// </summary>
	public const string DeviceCtlJsonVersion5 =
	"""
	{
		"info": {
			"jsonVersion": 5
		},
		"result": {
			"devices": [
				{
					"_deprecationNotice": {
						"deprecatedFields": [ "hardwareProperties", "deviceProperties", "connectionProperties" ],
						"replacement": "properties"
					},
					"identifier": "33333333-AAAA-BBBB-CCCC-DDDDDDDDDDDD",
					"properties": {
						"device": {
							"name": "Rolf's iPhone 15"
						},
						"hardware": {
							"cpuType": {
								"name": "arm64e"
							},
							"deviceType": "iPhone",
							"platform": "iOS",
							"udid": "00008003-012301230123ABCD"
						},
						"state": {
							"osVersionNumber": "27.0",
							"pairingState": "paired"
						}
					}
				}
			]
		}
	}
	""";
}
