// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Text.Json;

using Xamarin.MacDev.Models;

#nullable enable

namespace Xamarin.MacDev {

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
		public static List<SimulatorDeviceInfo> ParseDevices (string json)
		{
			var devices = new List<SimulatorDeviceInfo> ();
			if (string.IsNullOrEmpty (json))
				return devices;

			using (var doc = JsonDocument.Parse (json, JsonOptions)) {
				if (!doc.RootElement.TryGetProperty ("devices", out var devicesElement))
					return devices;

				foreach (var runtimeProp in devicesElement.EnumerateObject ()) {
					var runtimeId = runtimeProp.Name;

					foreach (var device in runtimeProp.Value.EnumerateArray ()) {
						var info = new SimulatorDeviceInfo {
							RuntimeIdentifier = runtimeId,
							Name = GetString (device, "name"),
							Udid = GetString (device, "udid"),
							State = GetString (device, "state"),
							DeviceTypeIdentifier = GetString (device, "deviceTypeIdentifier"),
							IsAvailable = GetBool (device, "isAvailable"),
						};

						devices.Add (info);
					}
				}
			}

			return devices;
		}

		/// <summary>
		/// Parses the JSON output of <c>xcrun simctl list runtimes --json</c>
		/// into a list of <see cref="SimulatorRuntimeInfo"/>.
		/// </summary>
		public static List<SimulatorRuntimeInfo> ParseRuntimes (string json)
		{
			var runtimes = new List<SimulatorRuntimeInfo> ();
			if (string.IsNullOrEmpty (json))
				return runtimes;

			using (var doc = JsonDocument.Parse (json, JsonOptions)) {
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
						IsBundled = GetBool (rt, "isInternal"),
					};

					runtimes.Add (info);
				}
			}

			return runtimes;
		}

		/// <summary>
		/// Parses the UDID from the output of <c>xcrun simctl create</c>.
		/// The command outputs just the UDID on a single line.
		/// </summary>
		public static string? ParseCreateOutput (string output)
		{
			if (string.IsNullOrEmpty (output))
				return null;

			var udid = output.Trim ();
			return udid.Length > 0 ? udid : null;
		}

		static string GetString (JsonElement element, string property)
		{
			if (element.TryGetProperty (property, out var value)) {
				// Handle simctl sometimes returning non-string types where
				// strings are expected (pattern from Redth/AppleDev.Tools
				// FlexibleStringConverter)
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
	}
}
