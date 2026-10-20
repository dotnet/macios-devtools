// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;
using NUnit.Framework;
using Xamarin.MacDev;
using Xamarin.MacDev.Models;

namespace Xamarin.MacDev.Tests;

[TestFixture]
public class DeviceCtlOutputParserTests {

	[Test]
	public void ParseDevices_ValidJson_ParsesAllFields ()
	{
		var json = @"{
			""result"": {
				""devices"": [
					{
						""connectionProperties"": {
							""pairingState"": ""paired"",
							""transportType"": ""localNetwork""
						},
						""deviceProperties"": {
							""name"": ""Rolf's iPhone 15"",
							""osBuildUpdate"": ""23B85"",
							""osVersionNumber"": ""18.1""
						},
						""hardwareProperties"": {
							""cpuType"": { ""name"": ""arm64e"" },
							""deviceType"": ""iPhone"",
							""ecid"": 12345678,
							""hardwareModel"": ""D83AP"",
							""platform"": ""iOS"",
							""productType"": ""iPhone16,1"",
							""serialNumber"": ""SERIAL_1"",
							""udid"": ""00008003-012301230123ABCD""
						},
						""identifier"": ""33333333-AAAA-BBBB-CCCC-DDDDDDDDDDDD""
					}
				]
			}
		}";

		var result = DeviceCtlOutputParser.ParseDevices (json);
		Assert.That (result.Count, Is.EqualTo (1));

		var device = result [0];
		Assert.That (device.Name, Is.EqualTo ("Rolf's iPhone 15"));
		Assert.That (device.Udid, Is.EqualTo ("00008003-012301230123ABCD"));
		Assert.That (device.Identifier, Is.EqualTo ("33333333-AAAA-BBBB-CCCC-DDDDDDDDDDDD"));
		Assert.That (device.BuildVersion, Is.EqualTo ("23B85"));
		Assert.That (device.OSVersion, Is.EqualTo ("18.1"));
		Assert.That (device.DeviceClass, Is.EqualTo ("iPhone"));
		Assert.That (device.HardwareModel, Is.EqualTo ("D83AP"));
		Assert.That (device.Platform, Is.EqualTo ("iOS"));
		Assert.That (device.ProductType, Is.EqualTo ("iPhone16,1"));
		Assert.That (device.SerialNumber, Is.EqualTo ("SERIAL_1"));
		Assert.That (device.UniqueChipID, Is.EqualTo ((ulong) 12345678));
		Assert.That (device.CpuArchitecture, Is.EqualTo ("arm64e"));
		Assert.That (device.TransportType, Is.EqualTo ("localNetwork"));
		Assert.That (device.PairingState, Is.EqualTo ("paired"));
	}

	[Test]
	public void ParseDevices_JsonVersion5_ParsesProperties ()
	{
		var json = @"{
			""info"": {
				""jsonVersion"": 5
			},
			""result"": {
				""devices"": [
					{
						""_deprecationNotice"": {
							""deprecatedFields"": [ ""hardwareProperties"", ""deviceProperties"", ""connectionProperties"" ],
							""replacement"": ""properties""
						},
						""deviceProperties"": {
							""name"": ""Deprecated name"",
							""osBuildUpdate"": ""22A1"",
							""osVersionNumber"": ""17.0""
						},
						""hardwareProperties"": {
							""hardwareModel"": ""D83AP""
						},
						""identifier"": ""33333333-AAAA-BBBB-CCCC-DDDDDDDDDDDD"",
						""properties"": {
							""connection"": {
								""pairingState"": ""paired"",
								""transportType"": ""localNetwork""
							},
							""hardware"": {
								""cpuType"": {
									""subtype"": 18446744071562067970,
									""type"": 16777228
								},
								""deviceType"": ""iPhone"",
								""ecid"": 12345678,
								""platform"": ""iOS"",
								""productType"": ""iPhone16,1"",
								""serialNumber"": ""SERIAL_1"",
								""udid"": ""00008003-012301230123ABCD""
							},
							""software"": {
								""osBuildVersions"": {
									""buildVersion"": {
										""name"": ""23B85""
									}
								},
								""osVersionNumber"": {
									""stringValue"": ""18.1""
								}
							},
							""state"": {
								""name"": ""Rolf's iPhone 15""
							}
						}
					}
				]
			}
		}";

		var result = DeviceCtlOutputParser.ParseDevices (json);
		Assert.That (result.Count, Is.EqualTo (1));

		var device = result [0];
		Assert.That (device.Name, Is.EqualTo ("Rolf's iPhone 15"));
		Assert.That (device.Udid, Is.EqualTo ("00008003-012301230123ABCD"));
		Assert.That (device.Identifier, Is.EqualTo ("33333333-AAAA-BBBB-CCCC-DDDDDDDDDDDD"));
		Assert.That (device.BuildVersion, Is.EqualTo ("23B85"));
		Assert.That (device.OSVersion, Is.EqualTo ("18.1"));
		Assert.That (device.DeviceClass, Is.EqualTo ("iPhone"));
		Assert.That (device.HardwareModel, Is.EqualTo ("D83AP"));
		Assert.That (device.Platform, Is.EqualTo ("iOS"));
		Assert.That (device.ProductType, Is.EqualTo ("iPhone16,1"));
		Assert.That (device.SerialNumber, Is.EqualTo ("SERIAL_1"));
		Assert.That (device.UniqueChipID, Is.EqualTo ((ulong) 12345678));
		Assert.That (device.CpuArchitecture, Is.EqualTo ("arm64e"));
		Assert.That (device.TransportType, Is.EqualTo ("localNetwork"));
		Assert.That (device.PairingState, Is.EqualTo ("paired"));
	}

	[Test]
	public void ParseDevices_JsonVersion5_Arm64CpuType_ParsesArchitecture ()
	{
		var json = @"{
			""info"": {
				""jsonVersion"": 5
			},
			""result"": {
				""devices"": [
					{
						""identifier"": ""ID-1"",
						""properties"": {
							""hardware"": {
								""cpuType"": {
									""subtype"": 0,
									""type"": 16777228
								}
							}
						}
					}
				]
			}
		}";

		var result = DeviceCtlOutputParser.ParseDevices (json);
		Assert.That (result.Count, Is.EqualTo (1));
		Assert.That (result [0].CpuArchitecture, Is.EqualTo ("arm64"));
	}

	[Test]
	public void ParseDevices_MultipleDevices_ParsesAll ()
	{
		var json = @"{
			""result"": {
				""devices"": [
					{
						""deviceProperties"": { ""name"": ""iPad Pro"", ""osVersionNumber"": ""26.0"" },
						""hardwareProperties"": { ""deviceType"": ""iPad"", ""platform"": ""iOS"", ""udid"": ""UDID-1"" },
						""identifier"": ""ID-1""
					},
					{
						""deviceProperties"": { ""name"": ""Apple Watch"", ""osVersionNumber"": ""11.5"" },
						""hardwareProperties"": { ""deviceType"": ""appleWatch"", ""platform"": ""watchOS"", ""udid"": ""UDID-2"" },
						""identifier"": ""ID-2""
					}
				]
			}
		}";

		var result = DeviceCtlOutputParser.ParseDevices (json);
		Assert.That (result.Count, Is.EqualTo (2));
		Assert.That (result [0].Name, Is.EqualTo ("iPad Pro"));
		Assert.That (result [0].DeviceClass, Is.EqualTo ("iPad"));
		Assert.That (result [1].Name, Is.EqualTo ("Apple Watch"));
		Assert.That (result [1].Platform, Is.EqualTo ("watchOS"));
	}

	[Test]
	public void ParseDevices_MissingUdid_FallsBackToIdentifier ()
	{
		var json = @"{
			""result"": {
				""devices"": [
					{
						""deviceProperties"": { ""name"": ""Mac"" },
						""hardwareProperties"": { ""deviceType"": ""mac"", ""platform"": ""macOS"" },
						""identifier"": ""12345678-1234-1234-ABCD-1234567980AB""
					}
				]
			}
		}";

		var result = DeviceCtlOutputParser.ParseDevices (json);
		Assert.That (result.Count, Is.EqualTo (1));
		Assert.That (result [0].Udid, Is.EqualTo ("12345678-1234-1234-ABCD-1234567980AB"));
	}

	[Test]
	public void ParseDevices_EmptyJson_ReturnsEmptyList ()
	{
		Assert.That (DeviceCtlOutputParser.ParseDevices (null).Count, Is.EqualTo (0));
		Assert.That (DeviceCtlOutputParser.ParseDevices ("").Count, Is.EqualTo (0));
		Assert.That (DeviceCtlOutputParser.ParseDevices ("{}").Count, Is.EqualTo (0));
		Assert.That (DeviceCtlOutputParser.ParseDevices ("{\"result\":{}}").Count, Is.EqualTo (0));
		Assert.That (DeviceCtlOutputParser.ParseDevices ("{\"result\":{\"devices\":[]}}").Count, Is.EqualTo (0));
	}

	[Test]
	public void ParseDevices_LargeEcid_ParsesCorrectly ()
	{
		var json = @"{
			""result"": {
				""devices"": [
					{
						""deviceProperties"": { ""name"": ""Device"" },
						""hardwareProperties"": {
							""ecid"": 18446744073709551615,
							""udid"": ""UDID-X""
						},
						""identifier"": ""ID-X""
					}
				]
			}
		}";

		var result = DeviceCtlOutputParser.ParseDevices (json);
		Assert.That (result [0].UniqueChipID, Is.EqualTo (ulong.MaxValue));
	}

	[Test]
	public void ParseDevices_MissingConnectionProperties_DefaultsToEmpty ()
	{
		var json = @"{
			""result"": {
				""devices"": [
					{
						""deviceProperties"": { ""name"": ""Device"" },
						""hardwareProperties"": { ""udid"": ""U1"" },
						""identifier"": ""I1""
					}
				]
			}
		}";

		var result = DeviceCtlOutputParser.ParseDevices (json);
		Assert.That (result [0].TransportType, Is.EqualTo (""));
		Assert.That (result [0].PairingState, Is.EqualTo (""));
	}

	[Test]
	public void ParseDevices_MalformedJson_ReturnsPartialResults ()
	{
		var result = DeviceCtlOutputParser.ParseDevices ("{ not valid json");
		Assert.That (result.Count, Is.EqualTo (0));
	}

	#region Hardware identity, connection state and simulators

	[Test]
	public void ParseDevices_LegacyLayout_ParsesHardwareUdidAndConnectionState ()
	{
		var json = @"{
			""result"": {
				""devices"": [
					{
						""connectionProperties"": {
							""pairingState"": ""paired"",
							""transportType"": ""wired"",
							""tunnelState"": ""connected""
						},
						""deviceProperties"": { ""name"": ""Test iPhone"" },
						""hardwareProperties"": {
							""deviceType"": ""iPhone"",
							""udid"": ""00008130-001A2B3C4D5E6F78""
						},
						""identifier"": ""11111111-AAAA-BBBB-CCCC-111111111111""
					}
				]
			}
		}";

		var device = DeviceCtlOutputParser.ParseDevices (json).Single ();

		Assert.That (device.HardwareUdid, Is.EqualTo ("00008130-001A2B3C4D5E6F78"));
		Assert.That (device.HasHardwareUdid, Is.True);
		Assert.That (device.Udid, Is.EqualTo (device.HardwareUdid));
		Assert.That (device.ConnectionState, Is.EqualTo ("connected"));
		Assert.That (device.Availability, Is.EqualTo (PhysicalDeviceAvailability.Connected));
		Assert.That (device.Reality, Is.EqualTo (""));
		Assert.That (device.IsSimulated, Is.False);
		Assert.That (device.MarketingName, Is.EqualTo (""));

		var connection = device.Connections.Single ();
		Assert.That (connection.Identifier, Is.EqualTo ("11111111-AAAA-BBBB-CCCC-111111111111"));
		Assert.That (connection.TransportType, Is.EqualTo ("wired"));
		Assert.That (connection.PairingState, Is.EqualTo ("paired"));
		Assert.That (connection.ConnectionState, Is.EqualTo ("connected"));
	}

	[Test]
	public void ParseDevices_JsonVersion5_ParsesHardwareIdentityAndConnectionState ()
	{
		var json = @"{
			""info"": { ""jsonVersion"": 5 },
			""result"": {
				""devices"": [
					{
						""identifier"": ""11111111-AAAA-BBBB-CCCC-111111111111"",
						""properties"": {
							""connection"": {
								""pairingState"": ""paired"",
								""state"": ""disconnected"",
								""transportType"": ""localNetwork""
							},
							""hardware"": {
								""deviceType"": ""iPhone"",
								""marketingName"": ""iPhone 15 Pro"",
								""platform"": ""iOS"",
								""productType"": ""iPhone16,1"",
								""reality"": ""physical"",
								""udid"": ""00008130-001A2B3C4D5E6F78""
							},
							""software"": {
								""osBuildVersions"": { ""buildVersion"": { ""name"": ""22B83"" } },
								""osVersionNumber"": { ""stringValue"": ""18.1"" }
							},
							""state"": { ""name"": ""Test iPhone"" }
						}
					}
				]
			}
		}";

		var device = DeviceCtlOutputParser.ParseDevices (json).Single ();

		Assert.That (device.Name, Is.EqualTo ("Test iPhone"));
		Assert.That (device.HardwareUdid, Is.EqualTo ("00008130-001A2B3C4D5E6F78"));
		Assert.That (device.Identifier, Is.EqualTo ("11111111-AAAA-BBBB-CCCC-111111111111"));
		Assert.That (device.MarketingName, Is.EqualTo ("iPhone 15 Pro"));
		Assert.That (device.Reality, Is.EqualTo ("physical"));
		Assert.That (device.OSVersion, Is.EqualTo ("18.1"));
		Assert.That (device.BuildVersion, Is.EqualTo ("22B83"));
		Assert.That (device.TransportType, Is.EqualTo ("localNetwork"));
		Assert.That (device.PairingState, Is.EqualTo ("paired"));
		Assert.That (device.ConnectionState, Is.EqualTo ("disconnected"));
		Assert.That (device.Availability, Is.EqualTo (PhysicalDeviceAvailability.Available));
		Assert.That (device.Connections.Single ().ConnectionState, Is.EqualTo ("disconnected"));
	}

	[Test]
	public void ParseDevices_JsonVersion5_StateObjectIsNotMistakenForTheConnectionState ()
	{
		var json = @"{
			""result"": {
				""devices"": [
					{
						""identifier"": ""CD-1"",
						""properties"": {
							""connection"": { ""pairingState"": ""paired"", ""transportType"": ""wired"" },
							""hardware"": { ""udid"": ""UDID-1"" },
							""state"": { ""name"": ""Test iPhone"", ""preparednessState"": ""unprepared"" }
						}
					}
				]
			}
		}";

		var device = DeviceCtlOutputParser.ParseDevices (json).Single ();

		Assert.That (device.ConnectionState, Is.EqualTo (""));
		Assert.That (device.Availability, Is.EqualTo (PhysicalDeviceAvailability.Unknown));
		Assert.That (device.PairingState, Is.EqualTo ("paired"));
	}

	[Test]
	public void ParseDevices_SimulatorEntries_AreReturnedAndFlagged ()
	{
		// Xcode 27 lists simulators next to physical devices, flagged by "reality" and/or "visibilityClass".
		var json = @"{
			""result"": {
				""devices"": [
					{
						""identifier"": ""SIM-1"",
						""properties"": {
							""hardware"": { ""reality"": ""simulated"", ""udid"": ""SIM-1"" },
							""state"": { ""name"": ""iPhone 16 Pro"" }
						}
					},
					{
						""identifier"": ""SIM-2"",
						""properties"": {
							""hardware"": { ""udid"": ""SIM-2"" },
							""state"": { ""name"": ""iPad Pro"", ""visibilityClass"": ""simulators"" }
						}
					},
					{
						""identifier"": ""SIM-3"",
						""visibilityClass"": ""simulators"",
						""properties"": {
							""hardware"": { ""udid"": ""SIM-3"" },
							""state"": { ""name"": ""Apple Watch"" }
						}
					},
					{
						""identifier"": ""CD-1"",
						""visibilityClass"": ""default"",
						""properties"": {
							""hardware"": { ""reality"": ""physical"", ""udid"": ""UDID-1"" },
							""state"": { ""name"": ""Test iPhone"" }
						}
					}
				]
			}
		}";

		var devices = DeviceCtlOutputParser.ParseDevices (json);

		Assert.That (devices.Select (d => d.Name), Is.EqualTo (new [] { "iPhone 16 Pro", "iPad Pro", "Apple Watch", "Test iPhone" }));
		Assert.That (devices.Select (d => d.IsSimulated), Is.EqualTo (new [] { true, true, true, false }));
		Assert.That (devices.Select (d => d.Reality), Is.EqualTo (new [] { "simulated", "simulated", "simulated", "physical" }));
	}

	[Test]
	public void ParseDevices_MissingHardwareUdid_KeepsTheUdidFallbackButReportsThatItWasMissing ()
	{
		var json = @"{
			""result"": {
				""devices"": [
					{
						""hardwareProperties"": { ""deviceType"": ""appleWatch"" },
						""identifier"": ""CD-1""
					}
				]
			}
		}";

		var device = DeviceCtlOutputParser.ParseDevices (json).Single ();

		Assert.That (device.HasHardwareUdid, Is.False);
		Assert.That (device.HardwareUdid, Is.EqualTo (""));
		Assert.That (device.Udid, Is.EqualTo ("CD-1"));
	}

	[Test]
	public void ParseDevices_SameDeviceReportedTwice_ReturnsBothEntries ()
	{
		// The lenient parser never merges entries; PhysicalDeviceService does.
		var json = @"{
			""result"": {
				""devices"": [
					{ ""hardwareProperties"": { ""udid"": ""UDID-1"" }, ""identifier"": ""CD-1"" },
					{ ""hardwareProperties"": { ""udid"": ""UDID-1"" }, ""identifier"": ""CD-2"" }
				]
			}
		}";

		var devices = DeviceCtlOutputParser.ParseDevices (json);

		Assert.That (devices.Select (d => d.Identifier), Is.EqualTo (new [] { "CD-1", "CD-2" }));
	}

	#endregion

	#region TryParseDevices

	[Test]
	public void TryParseDevices_EmptyDeviceList_Succeeds ()
	{
		var success = DeviceCtlOutputParser.TryParseDevices ("{\"result\":{\"devices\":[]}}", out var devices, out var error);

		Assert.That (success, Is.True);
		Assert.That (devices, Is.Empty);
		Assert.That (error, Is.Null);
	}

	[Test]
	public void TryParseDevices_ReturnsEveryEntryIncludingSimulatorsAndRepeats ()
	{
		var json = @"{
			""result"": {
				""devices"": [
					{ ""identifier"": ""CD-1"", ""properties"": { ""hardware"": { ""udid"": ""UDID-1"" } } },
					{ ""identifier"": ""CD-2"", ""properties"": { ""hardware"": { ""udid"": ""UDID-1"" } } },
					{ ""identifier"": ""SIM-1"", ""properties"": { ""hardware"": { ""reality"": ""simulated"", ""udid"": ""SIM-1"" } } }
				]
			}
		}";

		var success = DeviceCtlOutputParser.TryParseDevices (json, out var devices, out var error);

		Assert.That (success, Is.True, error);
		Assert.That (devices.Select (d => d.Identifier), Is.EqualTo (new [] { "CD-1", "CD-2", "SIM-1" }));
		Assert.That (devices.Select (d => d.IsSimulated), Is.EqualTo (new [] { false, false, true }));
	}

	[TestCase (null)]
	[TestCase ("")]
	[TestCase ("  \n")]
	[TestCase ("not json")]
	[TestCase ("{ \"result\": { \"devices\": [ ")]
	[TestCase ("[]")]
	[TestCase ("42")]
	[TestCase ("null")]
	[TestCase ("{}")]
	[TestCase ("{\"result\":{}}")]
	[TestCase ("{\"result\":[]}")]
	[TestCase ("{\"result\":{\"devices\":{}}}")]
	[TestCase ("{\"result\":{\"devices\":[42]}}")]
	[TestCase ("{\"result\":{\"devices\":[{\"identifier\":\"CD-1\"},\"oops\"]}}")]
	public void TryParseDevices_OutputThatIsNotADeviceList_Fails (string json)
	{
		var success = DeviceCtlOutputParser.TryParseDevices (json, out var devices, out var error);

		Assert.That (success, Is.False);
		Assert.That (devices, Is.Empty, "A failed parse must not return partial results.");
		Assert.That (error, Is.Not.Null.And.Not.Empty);
	}

	#endregion

	#region GetErrorMessage

	[TestCase ("{\"error\":{\"code\":1000,\"domain\":\"com.apple.dt.CoreDeviceError\",\"userInfo\":{\"NSLocalizedDescription\":{\"string\":\"Wrapped message.\"}}}}", "Wrapped message.")]
	[TestCase ("{\"error\":{\"userInfo\":{\"NSLocalizedDescription\":\"Plain message.\"}}}", "Plain message.")]
	[TestCase ("{\"error\":{\"code\":42,\"domain\":\"com.apple.dt.CoreDeviceError\"}}", "com.apple.dt.CoreDeviceError error 42")]
	[TestCase ("{\"error\":{\"code\":7,\"domain\":\"d\",\"userInfo\":{\"NSLocalizedDescription\":{\"string\":\"\"}}}}", "d error 7")]
	[TestCase ("{\"error\":{\"code\":42}}", "devicectl error 42")]
	[TestCase ("{\"error\":{}}", "devicectl reported an error.")]
	public void GetErrorMessage_ReturnsWhatDeviceCtlReported (string json, string expected)
	{
		Assert.That (DeviceCtlOutputParser.GetErrorMessage (json), Is.EqualTo (expected));
	}

	[TestCase (null)]
	[TestCase ("")]
	[TestCase ("   ")]
	[TestCase ("not json")]
	[TestCase ("[]")]
	[TestCase ("{\"result\":{\"devices\":[]}}")]
	[TestCase ("{\"error\":\"text\"}")]
	[TestCase ("{\"error\":null}")]
	public void GetErrorMessage_WithoutAnErrorObject_ReturnsNull (string json)
	{
		Assert.That (DeviceCtlOutputParser.GetErrorMessage (json), Is.Null);
	}

	#endregion
}
