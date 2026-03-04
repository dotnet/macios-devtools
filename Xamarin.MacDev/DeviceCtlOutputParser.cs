// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json;
using Xamarin.MacDev.Models;

#nullable enable

namespace Xamarin.MacDev;

/// <summary>
/// Pure parsing of <c>xcrun devicectl list devices</c> JSON output into model objects.
/// JSON structure follows Apple's devicectl output format, validated against
/// parsing patterns from dotnet/macios GetAvailableDevices task.
/// </summary>
public static class DeviceCtlOutputParser {

	static readonly JsonDocumentOptions JsonOptions = new JsonDocumentOptions {
		AllowTrailingCommas = true,
		CommentHandling = JsonCommentHandling.Skip,
	};

	/// <summary>
	/// Parses the JSON output of <c>xcrun devicectl list devices</c>
	/// into a list of <see cref="PhysicalDeviceInfo"/>.
	/// </summary>
	public static List<PhysicalDeviceInfo> ParseDevices (string? json, ICustomLogger? log = null)
	{
		var devices = new List<PhysicalDeviceInfo> ();
		if (string.IsNullOrEmpty (json))
			return devices;

		try {
			using (var doc = JsonDocument.Parse (json!, JsonOptions)) {
				// Navigate to result.devices array
				if (!doc.RootElement.TryGetProperty ("result", out var result))
					return devices;
				if (!result.TryGetProperty ("devices", out var devicesArray))
					return devices;
				if (devicesArray.ValueKind != JsonValueKind.Array)
					return devices;

				foreach (var device in devicesArray.EnumerateArray ()) {
					var info = new PhysicalDeviceInfo {
						Identifier = GetString (device, "identifier"),
					};

					// deviceProperties
					if (device.TryGetProperty ("deviceProperties", out var deviceProps)) {
						info.Name = GetString (deviceProps, "name");
						info.BuildVersion = GetString (deviceProps, "osBuildUpdate");
						info.OSVersion = GetString (deviceProps, "osVersionNumber");
					}

					// hardwareProperties
					if (device.TryGetProperty ("hardwareProperties", out var hwProps)) {
						info.Udid = GetString (hwProps, "udid");
						info.DeviceClass = GetString (hwProps, "deviceType");
						info.HardwareModel = GetString (hwProps, "hardwareModel");
						info.Platform = GetString (hwProps, "platform");
						info.ProductType = GetString (hwProps, "productType");
						info.SerialNumber = GetString (hwProps, "serialNumber");

						if (hwProps.TryGetProperty ("ecid", out var ecidElement)) {
							if (ecidElement.TryGetUInt64 (out var ecid))
								info.UniqueChipID = ecid;
						}

						// cpuType.name
						if (hwProps.TryGetProperty ("cpuType", out var cpuType))
							info.CpuArchitecture = GetString (cpuType, "name");
					}

					// connectionProperties
					if (device.TryGetProperty ("connectionProperties", out var connProps)) {
						info.TransportType = GetString (connProps, "transportType");
						info.PairingState = GetString (connProps, "pairingState");
					}

					// Fallback: use identifier as UDID if hardware UDID is missing
					if (string.IsNullOrEmpty (info.Udid))
						info.Udid = info.Identifier;

					devices.Add (info);
				}
			}
		} catch (JsonException ex) {
			log?.LogInfo ("DeviceCtlOutputParser.ParseDevices failed: {0}", ex.Message);
		} catch (InvalidOperationException ex) {
			log?.LogInfo ("DeviceCtlOutputParser.ParseDevices failed: {0}", ex.Message);
		}

		return devices;
	}

	static string GetString (JsonElement element, string property)
	{
		if (element.TryGetProperty (property, out var value)) {
			if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
				return "";
			if (value.ValueKind == JsonValueKind.String)
				return value.GetString () ?? "";
			return value.ToString ();
		}
		return "";
	}
}
