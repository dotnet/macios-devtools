// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Xamarin.MacDev.Models;

/// <summary>
/// Information about a physical Apple device from xcrun devicectl.
/// Corresponds to entries in the "result.devices" section of devicectl JSON.
/// </summary>
public class PhysicalDeviceInfo {
	/// <summary>The device display name (e.g. "Rolf's iPhone 15").</summary>
	public string Name { get; set; } = "";

	/// <summary>The device UDID.</summary>
	public string Udid { get; set; } = "";

	/// <summary>The device identifier (GUID from devicectl).</summary>
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

	/// <summary>The connection transport type (e.g. "localNetwork").</summary>
	public string TransportType { get; set; } = "";

	/// <summary>The pairing state (e.g. "paired").</summary>
	public string PairingState { get; set; } = "";

	public override string ToString () => $"{Name} ({Udid}) [{DeviceClass}]";
}
