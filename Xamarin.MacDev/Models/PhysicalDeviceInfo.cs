// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

#nullable enable

namespace Xamarin.MacDev.Models;

/// <summary>
/// Information about a physical Apple device from xcrun devicectl.
/// Corresponds to entries in the "result.devices" section of devicectl JSON.
/// </summary>
public class PhysicalDeviceInfo {
	/// <summary>The device display name (e.g. "Rolf's iPhone 15").</summary>
	public string Name { get; set; } = "";

	/// <summary>
	/// The best available stable identifier for the device: the hardware UDID when devicectl reports one,
	/// otherwise the CoreDevice <see cref="Identifier"/>. Use <see cref="HasHardwareUdid"/> to tell which.
	/// </summary>
	public string Udid { get; set; } = "";

	/// <summary>
	/// The hardware UDID reported by devicectl, or an empty string if devicectl did not report one.
	/// Unlike <see cref="Udid"/> this never falls back to <see cref="Identifier"/>.
	/// </summary>
	public string HardwareUdid { get; set; } = "";

	/// <summary>
	/// <c>true</c> if devicectl reported a real hardware UDID. Only such devices can be reliably
	/// correlated across enumerations and connection paths.
	/// </summary>
	public bool HasHardwareUdid => !string.IsNullOrEmpty (HardwareUdid);

	/// <summary>The CoreDevice identifier (GUID from devicectl). This is not the hardware UDID.</summary>
	public string Identifier { get; set; } = "";

	/// <summary>The OS build version (e.g. "23B85").</summary>
	public string BuildVersion { get; set; } = "";

	/// <summary>The OS version number (e.g. "18.1").</summary>
	public string OSVersion { get; set; } = "";

	/// <summary>The device class (e.g. "iPhone", "iPad", "appleWatch").</summary>
	public string DeviceClass { get; set; } = "";

	/// <summary>The hardware model (e.g. "D83AP").</summary>
	public string HardwareModel { get; set; } = "";

	/// <summary>The platform (e.g. "iOS", "watchOS").</summary>
	public string Platform { get; set; } = "";

	/// <summary>The product type (e.g. "iPhone16,1").</summary>
	public string ProductType { get; set; } = "";

	/// <summary>The serial number.</summary>
	public string SerialNumber { get; set; } = "";

	/// <summary>The ECID (unique chip identifier).</summary>
	public ulong? UniqueChipID { get; set; }

	/// <summary>The CPU architecture (e.g. "arm64e").</summary>
	public string CpuArchitecture { get; set; } = "";

	/// <summary>
	/// The connection transport type (e.g. "wired", "localNetwork"). When the device is reported over
	/// several transports this is the transport of the preferred connection; see <see cref="Connections"/>.
	/// </summary>
	public string TransportType { get; set; } = "";

	/// <summary>The pairing state (e.g. "paired"). Empty if devicectl did not report one. Pairing does not imply the device is reachable.</summary>
	public string PairingState { get; set; } = "";

	/// <summary>The marketing name (e.g. "iPhone 15 Pro Max"), or an empty string if not reported.</summary>
	public string MarketingName { get; set; } = "";

	/// <summary>
	/// Whether the entry describes a "physical" or a "simulated" device, as reported by devicectl.
	/// Empty if not reported.
	/// </summary>
	public string Reality { get; set; } = "";

	/// <summary>
	/// The raw connection (tunnel) state: "connected", "disconnected" or "unavailable".
	/// Empty if devicectl did not report one. See <see cref="Availability"/>.
	/// </summary>
	public string ConnectionState { get; set; } = "";

	/// <summary>The reachability of the device derived from <see cref="ConnectionState"/>. Never derived from <see cref="PairingState"/>.</summary>
	public PhysicalDeviceAvailability Availability => PhysicalDeviceConnection.GetAvailability (ConnectionState);

	/// <summary><c>true</c> if the device is currently <see cref="PhysicalDeviceAvailability.Connected"/> or <see cref="PhysicalDeviceAvailability.Available"/>.</summary>
	public bool IsReachable => Availability == PhysicalDeviceAvailability.Connected || Availability == PhysicalDeviceAvailability.Available;

	/// <summary><c>true</c> if devicectl reported the entry as a simulator rather than a physical device.</summary>
	public bool IsSimulated => string.Equals (Reality, "simulated", StringComparison.OrdinalIgnoreCase);

	/// <summary>
	/// The kind of device, classified from <see cref="DeviceClass"/>, <see cref="ProductType"/> and
	/// <see cref="Platform"/> (never from <see cref="Name"/>). <see cref="PhysicalDeviceKind.Unknown"/> if
	/// devicectl did not report enough information.
	/// </summary>
	public PhysicalDeviceKind Kind {
		get {
			var kind = ClassifyDeviceClass (DeviceClass);
			if (kind == PhysicalDeviceKind.Unknown)
				kind = ClassifyProductType (ProductType);
			if (kind == PhysicalDeviceKind.Unknown)
				kind = ClassifyPlatform (Platform);
			return kind;
		}
	}

	/// <summary>
	/// Every connection devicectl reported for this device, one per devicectl entry. The same physical
	/// device can be reported more than once, for example over USB and over the local network; the
	/// <see cref="PhysicalDeviceService"/> merges those entries (matched by <see cref="HardwareUdid"/>)
	/// into a single device and lists each transport here, preferred connection first.
	/// </summary>
	public List<PhysicalDeviceConnection> Connections { get; } = new List<PhysicalDeviceConnection> ();

	public override string ToString () => $"{Name} ({Udid}) [{DeviceClass}]";

	static PhysicalDeviceKind ClassifyDeviceClass (string? deviceClass)
	{
		switch (deviceClass?.ToLowerInvariant ()) {
		case "iphone":
			return PhysicalDeviceKind.IPhone;
		case "ipad":
			return PhysicalDeviceKind.IPad;
		case "ipod":
			return PhysicalDeviceKind.IPod;
		case "applewatch":
		case "watch":
			return PhysicalDeviceKind.AppleWatch;
		case "appletv":
		case "tv":
			return PhysicalDeviceKind.AppleTV;
		case "realitydevice":
		case "applevision":
		case "applevisionpro":
		case "visionpro":
			return PhysicalDeviceKind.AppleVision;
		case "mac":
			return PhysicalDeviceKind.Mac;
		default:
			return PhysicalDeviceKind.Unknown;
		}
	}

	static PhysicalDeviceKind ClassifyProductType (string? productType)
	{
		if (HasPrefix (productType, "iPhone"))
			return PhysicalDeviceKind.IPhone;
		if (HasPrefix (productType, "iPad"))
			return PhysicalDeviceKind.IPad;
		if (HasPrefix (productType, "iPod"))
			return PhysicalDeviceKind.IPod;
		if (HasPrefix (productType, "Watch"))
			return PhysicalDeviceKind.AppleWatch;
		if (HasPrefix (productType, "AppleTV"))
			return PhysicalDeviceKind.AppleTV;
		if (HasPrefix (productType, "RealityDevice"))
			return PhysicalDeviceKind.AppleVision;
		if (HasPrefix (productType, "Mac") || HasPrefix (productType, "iMac") || HasPrefix (productType, "VirtualMac"))
			return PhysicalDeviceKind.Mac;
		return PhysicalDeviceKind.Unknown;
	}

	static PhysicalDeviceKind ClassifyPlatform (string? platform)
	{
		// The "iOS" platform alone is deliberately not classified: it covers iPhone, iPad and iPod.
		switch (platform?.ToLowerInvariant ()) {
		case "ipados":
			return PhysicalDeviceKind.IPad;
		case "watchos":
			return PhysicalDeviceKind.AppleWatch;
		case "tvos":
			return PhysicalDeviceKind.AppleTV;
		case "visionos":
		case "xros":
			return PhysicalDeviceKind.AppleVision;
		case "macos":
			return PhysicalDeviceKind.Mac;
		default:
			return PhysicalDeviceKind.Unknown;
		}
	}

	static bool HasPrefix (string? value, string prefix) => value is not null && value.StartsWith (prefix, StringComparison.OrdinalIgnoreCase);
}
