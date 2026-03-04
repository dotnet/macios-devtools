// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Xamarin.MacDev.Models;

/// <summary>
/// Information about a simulator device type from xcrun simctl.
/// Corresponds to entries in the "devicetypes" section of simctl JSON.
/// </summary>
public class SimulatorDeviceTypeInfo {
	/// <summary>The device type identifier (e.g. "com.apple.CoreSimulator.SimDeviceType.iPhone-16-Pro").</summary>
	public string Identifier { get; set; } = "";

	/// <summary>The display name (e.g. "iPhone 16 Pro").</summary>
	public string Name { get; set; } = "";

	/// <summary>The product family (e.g. "iPhone", "iPad", "Apple TV").</summary>
	public string ProductFamily { get; set; } = "";

	/// <summary>The minimum runtime version string (e.g. "13.0.0").</summary>
	public string MinRuntimeVersionString { get; set; } = "";

	/// <summary>The maximum runtime version string (e.g. "65535.255.255").</summary>
	public string MaxRuntimeVersionString { get; set; } = "";

	/// <summary>The model identifier (e.g. "iPhone12,1").</summary>
	public string ModelIdentifier { get; set; } = "";

	public override string ToString () => $"{Name} ({Identifier})";
}
