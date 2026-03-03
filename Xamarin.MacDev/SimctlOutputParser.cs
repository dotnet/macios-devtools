// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json;
using Xamarin.MacDev.Models;

#nullable enable

namespace Xamarin.MacDev;

/// <summary>
/// Pure parsing of <c>xcrun simctl list</c> JSON output into model objects.
/// JSON structure follows Apple's simctl output format, validated against
/// parsing patterns from ClientTools.Platform RemoteSimulatorValidator and
/// Redth/AppleDev.Tools SimCtl.
/// </summary>
public static class SimctlOutputParser {

	static readonly JsonDocumentOptions JsonOptions = new JsonDocumentOptions {
		AllowTrailingCommas = true,
		CommentHandling = JsonCommentHandling.Skip,
	};

	/// <summary>
	/// Parses the JSON output of <c>xcrun simctl list devices --json</c>
	/// into a list of <see cref="SimulatorDeviceInfo"/>.
	/// Device keys are runtime identifiers like
	/// "com.apple.CoreSimulator.SimRuntime.iOS-18-2".
	/// </summary>
	public static List<SimulatorDeviceInfo> ParseDevices (string? json)
	{
		var devices = new List<SimulatorDeviceInfo> ();
		if (string.IsNullOrEmpty (json))
			return devices;

		try {
			using (var doc = JsonDocument.Parse (json!, JsonOptions)) {
				if (!doc.RootElement.TryGetProperty ("devices", out var devicesElement))
					return devices;

				foreach (var runtimeProp in devicesElement.EnumerateObject ()) {
					var runtimeId = runtimeProp.Name;
					// Derive platform and version from runtime identifier
					// e.g. "com.apple.CoreSimulator.SimRuntime.iOS-18-2" → platform="iOS", version="18.2"
					var (platform, osVersion) = ParseRuntimeIdentifier (runtimeId);

					foreach (var device in runtimeProp.Value.EnumerateArray ()) {
						var info = new SimulatorDeviceInfo {
							RuntimeIdentifier = runtimeId,
							Name = GetString (device, "name"),
							Udid = GetString (device, "udid"),
							State = GetString (device, "state"),
							DeviceTypeIdentifier = GetString (device, "deviceTypeIdentifier"),
							IsAvailable = GetBool (device, "isAvailable"),
							AvailabilityError = GetString (device, "availabilityError"),
							Platform = platform,
							OSVersion = osVersion,
						};

						devices.Add (info);
					}
				}
			}
		} catch (JsonException) {
			// Malformed simctl output — return whatever we parsed so far
		} catch (InvalidOperationException) {
			// Unexpected JSON structure (e.g. wrong ValueKind) — return partial results
		}

		return devices;
	}

	/// <summary>
	/// Parses the JSON output of <c>xcrun simctl list runtimes --json</c>
	/// into a list of <see cref="SimulatorRuntimeInfo"/>.
	/// </summary>
	public static List<SimulatorRuntimeInfo> ParseRuntimes (string? json)
	{
		var runtimes = new List<SimulatorRuntimeInfo> ();
		if (string.IsNullOrEmpty (json))
			return runtimes;

		try {
			using (var doc = JsonDocument.Parse (json!, JsonOptions)) {
				if (!doc.RootElement.TryGetProperty ("runtimes", out var runtimesArray))
					return runtimes;

				foreach (var rt in runtimesArray.EnumerateArray ()) {
					var info = new SimulatorRuntimeInfo {
						Name = GetString (rt, "name"),
						Identifier = GetString (rt, "identifier"),
						Version = GetString (rt, "version"),
						BuildVersion = GetString (rt, "buildversion"),
						Platform = GetString (rt, "platform"),
						IsAvailable = GetBool (rt, "isAvailable"),
						IsBundled = string.Equals (GetString (rt, "contentType"), "bundled", StringComparison.OrdinalIgnoreCase),
					};

					if (rt.TryGetProperty ("supportedArchitectures", out var archArray) &&
						archArray.ValueKind == JsonValueKind.Array) {
						foreach (var arch in archArray.EnumerateArray ()) {
							var a = arch.ValueKind == JsonValueKind.String ? arch.GetString () : null;
							if (!string.IsNullOrEmpty (a))
								info.SupportedArchitectures.Add (a!);
						}
					}

					runtimes.Add (info);
				}
			}
		} catch (JsonException) {
			// Malformed simctl output — return whatever we parsed so far
		} catch (InvalidOperationException) {
			// Unexpected JSON structure (e.g. wrong ValueKind) — return partial results
		}

		return runtimes;
	}

	/// <summary>
	/// Parses the JSON output of <c>xcrun simctl list devicetypes --json</c>
	/// into a list of <see cref="SimulatorDeviceTypeInfo"/>.
	/// </summary>
	public static List<SimulatorDeviceTypeInfo> ParseDeviceTypes (string? json)
	{
		var deviceTypes = new List<SimulatorDeviceTypeInfo> ();
		if (string.IsNullOrEmpty (json))
			return deviceTypes;

		try {
			using (var doc = JsonDocument.Parse (json!, JsonOptions)) {
				if (!doc.RootElement.TryGetProperty ("devicetypes", out var dtArray))
					return deviceTypes;

				foreach (var dt in dtArray.EnumerateArray ()) {
					var info = new SimulatorDeviceTypeInfo {
						Identifier = GetString (dt, "identifier"),
						Name = GetString (dt, "name"),
						ProductFamily = GetString (dt, "productFamily"),
						MinRuntimeVersionString = GetString (dt, "minRuntimeVersionString"),
						MaxRuntimeVersionString = GetString (dt, "maxRuntimeVersionString"),
						ModelIdentifier = GetString (dt, "modelIdentifier"),
					};

					deviceTypes.Add (info);
				}
			}
		} catch (JsonException) {
			// Malformed simctl output — return whatever we parsed so far
		} catch (InvalidOperationException) {
			// Unexpected JSON structure — return partial results
		}

		return deviceTypes;
	}

	/// <summary>
	/// Parses the UDID from the output of <c>xcrun simctl create</c>.
	/// The command outputs just the UDID on a single line.
	/// </summary>
	public static string? ParseCreateOutput (string? output)
	{
		if (string.IsNullOrEmpty (output))
			return null;

		var udid = output!.Trim ();
		return udid.Length > 0 ? udid : null;
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

	static bool GetBool (JsonElement element, string property)
	{
		if (element.TryGetProperty (property, out var value)) {
			if (value.ValueKind == JsonValueKind.True)
				return true;
			if (value.ValueKind == JsonValueKind.False)
				return false;
			// simctl sometimes returns "true"/"false" as strings
			if (value.ValueKind == JsonValueKind.String)
				return string.Equals (value.GetString (), "true", StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	/// <summary>
	/// Parses a runtime identifier like "com.apple.CoreSimulator.SimRuntime.iOS-18-2"
	/// into a (platform, version) tuple e.g. ("iOS", "18.2").
	/// Pattern from dotnet/macios GetAvailableDevices.
	/// </summary>
	public static (string platform, string version) ParseRuntimeIdentifier (string identifier)
	{
		if (string.IsNullOrEmpty (identifier))
			return ("", "");

		// Strip prefix "com.apple.CoreSimulator.SimRuntime."
		const string prefix = "com.apple.CoreSimulator.SimRuntime.";
		var name = identifier.StartsWith (prefix, StringComparison.Ordinal)
			? identifier.Substring (prefix.Length)
			: identifier;

		// Split "iOS-18-2" → ["iOS", "18", "2"]
		var parts = name.Split ('-');
		if (parts.Length < 2)
			return (name, "");

		var platform = parts [0];
		var version = string.Join (".", parts, 1, parts.Length - 1);
		return (platform, version);
	}
}
