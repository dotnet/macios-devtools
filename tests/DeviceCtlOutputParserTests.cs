// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using NUnit.Framework;
using Xamarin.MacDev;

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
}
