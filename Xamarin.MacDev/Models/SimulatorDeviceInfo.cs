// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Xamarin.MacDev.Models {

	/// <summary>
	/// Information about a simulator device from xcrun simctl.
	/// Fields aligned with dotnet/macios GetAvailableDevices task.
	/// </summary>
	public class SimulatorDeviceInfo {
		/// <summary>The simulator display name (e.g. "iPhone 16 Pro").</summary>
		public string Name { get; set; } = "";

		/// <summary>The simulator UDID.</summary>
		public string Udid { get; set; } = "";

		/// <summary>The device state (e.g. "Shutdown", "Booted").</summary>
		public string State { get; set; } = "";

		/// <summary>The runtime identifier (e.g. "com.apple.CoreSimulator.SimRuntime.iOS-18-2").</summary>
		public string RuntimeIdentifier { get; set; } = "";

		/// <summary>The device type identifier (e.g. "com.apple.CoreSimulator.SimDeviceType.iPhone-16-Pro").</summary>
		public string DeviceTypeIdentifier { get; set; } = "";

		/// <summary>Whether this simulator is available.</summary>
		public bool IsAvailable { get; set; }

		/// <summary>Availability error message when IsAvailable is false.</summary>
		public string AvailabilityError { get; set; } = "";

		/// <summary>The runtime version for this device (e.g. "18.2"), derived from the runtime.</summary>
		public string OSVersion { get; set; } = "";

		/// <summary>The platform (e.g. "iOS", "tvOS"), derived from the runtime identifier.</summary>
		public string Platform { get; set; } = "";

		public bool IsBooted => State == "Booted";

		public override string ToString () => $"{Name} ({Udid}) [{State}]";
	}
}
