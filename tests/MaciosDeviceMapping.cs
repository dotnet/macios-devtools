// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.MacDev.Models;

#nullable enable

namespace tests;

/// <summary>
/// What the dotnet/macios <c>GetAvailableDevices</c> task makes of the devices that devicectl reports: the metadata of the MSBuild
/// item it creates for each one, which devices it discards and why, and the order in which it returns the others. It is a port of
/// that task's <c>RunDeviceCtlAsync</c> and of its final sort, where <see cref="PhysicalDeviceInfo"/> takes the place of the
/// devices <see cref="Xamarin.MacDev.DeviceCtlOutputParser.ParseDevices"/> returns. It shows that the devices of
/// <see cref="Xamarin.MacDev.PhysicalDeviceService"/> carry everything the task needs, so that its tests can run against them;
/// it is not part of the library. Simulators, the filters on the target platform, the runtime identifier and the app manifest,
/// and the preference for booted simulators in the sort are left out because they do not apply to the devicectl devices.
/// </summary>
static class MaciosDeviceMapping {

	/// <summary>The MSBuild item the task creates for a device, with the metadata its tests check.</summary>
	public sealed class Item {

		public Item (string udid, string name, string description, string osVersion, string status, string runtimeIdentifier, string discardedReason, Version minimumOSVersion)
		{
			Udid = udid;
			Name = name;
			Description = description;
			OSVersion = osVersion;
			Status = status;
			RuntimeIdentifier = runtimeIdentifier;
			DiscardedReason = discardedReason;
			MinimumOSVersion = minimumOSVersion;
		}

		/// <summary>The UDID, which is both the item spec and the <c>UDID</c> metadata.</summary>
		public string Udid { get; }

		public string ItemSpec => Udid;

		public string Name { get; }

		public string Description { get; }

		public string OSVersion { get; }

		public string Status { get; }

		public string RuntimeIdentifier { get; }

		public string DiscardedReason { get; }

		public Version MinimumOSVersion { get; }

		public bool Discarded => DiscardedReason.Length > 0;
	}

	/// <summary>The <c>Devices</c> and <c>DiscardedDevices</c> outputs of the task.</summary>
	public sealed class Result {

		public Result (IReadOnlyList<Item> devices, IReadOnlyList<Item> discardedDevices)
		{
			Devices = devices;
			DiscardedDevices = discardedDevices;
		}

		public IReadOnlyList<Item> Devices { get; }

		public IReadOnlyList<Item> DiscardedDevices { get; }
	}

	/// <summary>
	/// Maps the devices the way the task does. A failed discovery has no devices, so it maps to no items:
	/// the same as the task's lenient parser returning nothing for output it cannot use.
	/// </summary>
	public static Result Map (IEnumerable<PhysicalDeviceInfo> devices)
	{
		var items = new List<Item> ();

		foreach (var device in devices) {
			var udid = device.Udid;
			if (string.IsNullOrEmpty (udid))
				udid = $"<unknown udid #{items.Count + 1}>";

			var description = device.Name;
			if (!string.IsNullOrEmpty (device.OSVersion) && !description.Contains (device.OSVersion))
				description = $"{description} - {device.Platform} {device.OSVersion}";

			// A device that does not report a pairing state is not paired.
			var pairingState = string.IsNullOrEmpty (device.PairingState) ? "unpaired" : device.PairingState;
			var status = char.ToUpperInvariant (pairingState [0]) + pairingState.Substring (1);

			var runtimeIdentifier = "";
			var discardedReason = "";
			switch (device.DeviceClass.ToLowerInvariant ()) {
			case "iphone":
			case "ipod":
			case "ipad":
				runtimeIdentifier = "ios-";
				break;
			case "appletv":
				runtimeIdentifier = "tvos-";
				break;
			default:
				discardedReason = $"'{device.DeviceClass}' devices are not supported";
				break;
			}

			if (discardedReason.Length == 0) {
				switch (device.CpuArchitecture.ToLowerInvariant ()) {
				case "arm64":
				case "arm64e":
					// arm64 and arm64e are both arm64 for the task's purposes.
					runtimeIdentifier += "arm64";
					break;
				default:
					discardedReason = $"Unknown CPU architecture '{device.CpuArchitecture}'";
					break;
				}
			}

			if (!Version.TryParse (device.OSVersion, out var minimumOSVersion))
				minimumOSVersion = new Version (0, 0);

			items.Add (new Item (udid, device.Name, description, device.OSVersion, status, runtimeIdentifier, discardedReason, minimumOSVersion));
		}

		var kept = items
			.Where (i => !i.Discarded)
			.OrderByDescending (i => i.MinimumOSVersion) // newer devices first
			.ThenBy (i => i.Name, StringComparer.Ordinal)
			.ThenBy (i => i.Udid, StringComparer.Ordinal)
			.ToList ();

		return new Result (kept, items.Where (i => i.Discarded).ToList ());
	}
}
