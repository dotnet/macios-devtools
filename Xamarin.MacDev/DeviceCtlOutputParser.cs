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

					var properties = GetObject (device, "properties");
					var newDeviceProperties = GetObject (properties, "device");
					var stateProperties = GetObject (properties, "state");
					var softwareProperties = GetObject (properties, "software");
					var osBuildVersions = GetObject (softwareProperties, "osBuildVersions");
					var buildVersion = GetObject (osBuildVersions, "buildVersion");
					var osVersionNumber = GetObject (softwareProperties, "osVersionNumber");
					var newHardwareProperties = GetObject (properties, "hardware");
					var newConnectionProperties = GetObject (properties, "connection");
					var deviceProperties = GetObject (device, "deviceProperties");
					var hardwareProperties = GetObject (device, "hardwareProperties");
					var connectionProperties = GetObject (device, "connectionProperties");

					info.Name = GetString ("name", newDeviceProperties, stateProperties, properties, deviceProperties);
					info.BuildVersion = GetString ("name", buildVersion);
					if (string.IsNullOrEmpty (info.BuildVersion))
						info.BuildVersion = GetString ("osBuildUpdate", stateProperties, newDeviceProperties, properties, deviceProperties);
					info.OSVersion = GetString ("stringValue", osVersionNumber);
					if (string.IsNullOrEmpty (info.OSVersion))
						info.OSVersion = GetString ("osVersionNumber", stateProperties, newDeviceProperties, properties, deviceProperties);
					info.Udid = GetString ("udid", newHardwareProperties, properties, hardwareProperties);
					info.DeviceClass = GetString ("deviceType", newHardwareProperties, properties, hardwareProperties);
					info.HardwareModel = GetString ("hardwareModel", newHardwareProperties, properties, hardwareProperties);
					info.Platform = GetString ("platform", newHardwareProperties, properties, hardwareProperties);
					info.ProductType = GetString ("productType", newHardwareProperties, properties, hardwareProperties);
					info.SerialNumber = GetString ("serialNumber", newHardwareProperties, properties, hardwareProperties);
					info.UniqueChipID = GetUInt64 ("ecid", newHardwareProperties, properties, hardwareProperties);

					var newCpuType = GetObject (newHardwareProperties, "cpuType");
					var cpuType = GetObject (properties, "cpuType");
					var legacyCpuType = GetObject (hardwareProperties, "cpuType");
					info.CpuArchitecture = GetCpuArchitecture (newCpuType);
					if (string.IsNullOrEmpty (info.CpuArchitecture))
						info.CpuArchitecture = GetString ("name", newCpuType, cpuType, legacyCpuType);

					info.TransportType = GetString ("transportType", newConnectionProperties, stateProperties, properties, connectionProperties);
					info.PairingState = GetString ("pairingState", newConnectionProperties, stateProperties, properties, connectionProperties);

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

	static string GetCpuArchitecture (JsonElement? cpuType)
	{
		if (!cpuType.HasValue ||
			!cpuType.Value.TryGetProperty ("type", out var typeElement) ||
			!typeElement.TryGetInt32 (out var type))
			return "";

		switch (type) {
		case 7:
			return "i386";
		case 12:
			return "arm";
		case 0x01000007:
			return "x86_64";
		case 0x0100000c:
			if (cpuType.Value.TryGetProperty ("subtype", out var subtypeElement) &&
				subtypeElement.TryGetUInt64 (out var subtype) &&
				(subtype & 0xff) == 2)
				return "arm64e";
			return "arm64";
		case 0x0200000c:
			return "arm64_32";
		default:
			return "";
		}
	}

	static JsonElement? GetObject (JsonElement? element, string property)
	{
		if (element.HasValue && element.Value.TryGetProperty (property, out var value) && value.ValueKind == JsonValueKind.Object)
			return value;
		return null;
	}

	static string GetString (string property, params JsonElement? [] elements)
	{
		foreach (var element in elements) {
			if (!element.HasValue)
				continue;
			var value = GetString (element.Value, property);
			if (!string.IsNullOrEmpty (value))
				return value;
		}
		return "";
	}

	static ulong? GetUInt64 (string property, params JsonElement? [] elements)
	{
		foreach (var element in elements) {
			if (!element.HasValue || !element.Value.TryGetProperty (property, out var value))
				continue;
			if (value.TryGetUInt64 (out var result))
				return result;
		}
		return null;
	}
}
