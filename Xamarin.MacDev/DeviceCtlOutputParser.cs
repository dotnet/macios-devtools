// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
	/// <remarks>
	/// This parser is lenient: null, empty, malformed or unexpected JSON yields an empty list, and every
	/// entry is returned, including simulators (see <see cref="PhysicalDeviceInfo.IsSimulated"/>).
	/// If devicectl did not report a hardware UDID, <see cref="PhysicalDeviceInfo.Udid"/> falls back to the
	/// CoreDevice identifier (see <see cref="PhysicalDeviceInfo.HasHardwareUdid"/>).
	/// Use <see cref="TryParseDevices"/> to tell an empty device list apart from invalid output.
	/// </remarks>
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

				foreach (var device in devicesArray.EnumerateArray ())
					devices.Add (ParseDevice (device));
			}
		} catch (JsonException ex) {
			log?.LogInfo ("DeviceCtlOutputParser.ParseDevices failed: {0}", ex.Message);
		} catch (InvalidOperationException ex) {
			log?.LogInfo ("DeviceCtlOutputParser.ParseDevices failed: {0}", ex.Message);
		}

		return devices;
	}

	/// <summary>
	/// Strictly parses the JSON output of <c>xcrun devicectl list devices</c>.
	/// Unlike <see cref="ParseDevices"/> this distinguishes a valid, empty device list (returns <c>true</c>
	/// with an empty list) from output that is missing, malformed or not in the expected format
	/// (returns <c>false</c> with a description in <paramref name="error"/>).
	/// Every entry must have a text <c>identifier</c>, and a text hardware <c>udid</c> if it has one;
	/// an entry that does not makes the whole output invalid rather than becoming a device nobody can address.
	/// </summary>
	/// <param name="json">The contents of the devicectl JSON output.</param>
	/// <param name="devices">All parsed entries in the order devicectl reported them, including simulators and
	/// repeated reports of the same device. Empty if parsing failed.</param>
	/// <param name="error">A description of why parsing failed, or <c>null</c> on success.</param>
	public static bool TryParseDevices (string? json, out List<PhysicalDeviceInfo> devices, [NotNullWhen (false)] out string? error)
	{
		devices = new List<PhysicalDeviceInfo> ();
		error = null;

		if (string.IsNullOrWhiteSpace (json)) {
			error = "devicectl did not produce any JSON output.";
			return false;
		}

		try {
			using (var doc = JsonDocument.Parse (json!, JsonOptions)) {
				if (doc.RootElement.ValueKind != JsonValueKind.Object) {
					error = "The devicectl JSON output is not a JSON object.";
					return false;
				}
				if (!doc.RootElement.TryGetProperty ("result", out var result) || result.ValueKind != JsonValueKind.Object) {
					error = "The devicectl JSON output does not contain a 'result' object.";
					return false;
				}
				if (!result.TryGetProperty ("devices", out var devicesArray) || devicesArray.ValueKind != JsonValueKind.Array) {
					error = "The devicectl JSON output does not contain a 'result.devices' array.";
					return false;
				}

				var parsed = new List<PhysicalDeviceInfo> ();
				foreach (var device in devicesArray.EnumerateArray ()) {
					if (device.ValueKind != JsonValueKind.Object) {
						error = "The devicectl JSON output contains a 'result.devices' entry that is not an object.";
						return false;
					}
					error = GetIdentityError (device);
					if (error is not null)
						return false;
					parsed.Add (ParseDevice (device));
				}

				devices = parsed;
				return true;
			}
		} catch (JsonException ex) {
			error = $"The devicectl JSON output is not valid JSON: {ex.Message}";
			return false;
		} catch (InvalidOperationException ex) {
			error = $"The devicectl JSON output has an unexpected structure: {ex.Message}";
			return false;
		}
	}

	/// <summary>
	/// Gets the error devicectl reported in its JSON output when a command fails
	/// (the <c>error</c> object), or <c>null</c> if the output does not contain one.
	/// </summary>
	internal static string? GetErrorMessage (string? json)
	{
		if (string.IsNullOrWhiteSpace (json))
			return null;

		try {
			using (var doc = JsonDocument.Parse (json!, JsonOptions)) {
				if (doc.RootElement.ValueKind != JsonValueKind.Object ||
					!doc.RootElement.TryGetProperty ("error", out var error) ||
					error.ValueKind != JsonValueKind.Object)
					return null;

				var description = GetLocalizedDescription (error);
				if (!string.IsNullOrEmpty (description))
					return description;

				var domain = GetString (error, "domain");
				var code = GetString (error, "code");
				if (domain.Length > 0 && code.Length > 0)
					return $"{domain} error {code}";
				if (code.Length > 0)
					return $"devicectl error {code}";
				return "devicectl reported an error.";
			}
		} catch (JsonException) {
			return null;
		}
	}

	static string GetLocalizedDescription (JsonElement error)
	{
		if (!error.TryGetProperty ("userInfo", out var userInfo) || userInfo.ValueKind != JsonValueKind.Object)
			return "";
		if (!userInfo.TryGetProperty ("NSLocalizedDescription", out var description))
			return "";

		// devicectl wraps the description as { "string": "..." }.
		if (description.ValueKind == JsonValueKind.String)
			return description.GetString () ?? "";
		if (description.ValueKind == JsonValueKind.Object)
			return GetString (description, "string");
		return "";
	}

	// devicectl names every device with its CoreDevice identifier, and with its hardware UDID when the hardware has one.
	// The lenient parser keeps whatever it finds; the strict one needs an identity it can rely on.
	static string? GetIdentityError (JsonElement device)
	{
		if (!device.TryGetProperty ("identifier", out var identifier) ||
			identifier.ValueKind != JsonValueKind.String ||
			string.IsNullOrWhiteSpace (identifier.GetString ()))
			return "The devicectl JSON output contains a 'result.devices' entry without a valid 'identifier'.";

		var properties = GetObject (device, "properties");
		foreach (var hardware in new [] { GetObject (properties, "hardware"), properties, GetObject (device, "hardwareProperties") }) {
			if (hardware.HasValue &&
				hardware.Value.TryGetProperty ("udid", out var udid) &&
				udid.ValueKind != JsonValueKind.String &&
				udid.ValueKind != JsonValueKind.Null)
				return "The devicectl JSON output contains a 'result.devices' entry whose hardware 'udid' is not text.";
		}

		return null;
	}

	static PhysicalDeviceInfo ParseDevice (JsonElement device)
	{
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
		info.HardwareUdid = GetString ("udid", newHardwareProperties, properties, hardwareProperties);
		info.Udid = info.HardwareUdid;
		info.DeviceClass = GetString ("deviceType", newHardwareProperties, properties, hardwareProperties);
		info.HardwareModel = GetString ("hardwareModel", newHardwareProperties, properties, hardwareProperties);
		info.Platform = GetString ("platform", newHardwareProperties, properties, hardwareProperties);
		info.ProductType = GetString ("productType", newHardwareProperties, properties, hardwareProperties);
		info.MarketingName = GetString ("marketingName", newHardwareProperties, properties, hardwareProperties);
		info.SerialNumber = GetString ("serialNumber", newHardwareProperties, properties, hardwareProperties);
		info.UniqueChipID = GetUInt64 ("ecid", newHardwareProperties, properties, hardwareProperties);

		info.Reality = GetString ("reality", newHardwareProperties, properties, hardwareProperties);
		if (string.IsNullOrEmpty (info.Reality) &&
			string.Equals (GetString ("visibilityClass", device, stateProperties), "simulators", StringComparison.OrdinalIgnoreCase))
			info.Reality = "simulated";

		var newCpuType = GetObject (newHardwareProperties, "cpuType");
		var cpuType = GetObject (properties, "cpuType");
		var legacyCpuType = GetObject (hardwareProperties, "cpuType");
		info.CpuArchitecture = GetCpuArchitecture (newCpuType);
		if (string.IsNullOrEmpty (info.CpuArchitecture))
			info.CpuArchitecture = GetString ("name", newCpuType, cpuType, legacyCpuType);

		info.TransportType = GetString ("transportType", newConnectionProperties, stateProperties, properties, connectionProperties);
		info.PairingState = GetString ("pairingState", newConnectionProperties, stateProperties, properties, connectionProperties);

		// The connection state is "tunnelState" in the legacy layout; JSON version 5 reports it as
		// "state" inside the connection object ("properties.state" is a different, unrelated object).
		info.ConnectionState = GetString ("tunnelState", newConnectionProperties, stateProperties, properties, connectionProperties);
		if (string.IsNullOrEmpty (info.ConnectionState))
			info.ConnectionState = GetString ("state", newConnectionProperties);

		// Fallback: use identifier as UDID if hardware UDID is missing
		if (string.IsNullOrEmpty (info.Udid))
			info.Udid = info.Identifier;

		info.Connections.Add (new PhysicalDeviceConnection {
			Identifier = info.Identifier,
			TransportType = info.TransportType,
			PairingState = info.PairingState,
			ConnectionState = info.ConnectionState,
		});

		return info;
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
		// TryGet* throws for a value of another kind, and these properties are optional metadata, so check first.
		if (!cpuType.HasValue ||
			!cpuType.Value.TryGetProperty ("type", out var typeElement) ||
			typeElement.ValueKind != JsonValueKind.Number ||
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
				subtypeElement.ValueKind == JsonValueKind.Number &&
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
			if (value.ValueKind == JsonValueKind.Number && value.TryGetUInt64 (out var result))
				return result;
		}
		return null;
	}
}
