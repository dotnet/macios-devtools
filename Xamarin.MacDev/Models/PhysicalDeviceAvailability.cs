// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Xamarin.MacDev.Models {

	/// <summary>
	/// Whether a physical device can currently be reached, derived only from the connection
	/// (tunnel) state reported by devicectl. Pairing is deliberately not considered: a device
	/// can be paired but unreachable, or reachable but not paired.
	/// </summary>
	public enum PhysicalDeviceAvailability {
		/// <summary>The connection state was not reported or is not recognized.</summary>
		Unknown = 0,

		/// <summary>CoreDevice remembers the device, but it is not currently reachable.</summary>
		Unavailable,

		/// <summary>
		/// The device is reachable (for example over USB or the local network) but no connection
		/// is established yet. devicectl reports this as the "disconnected" connection state,
		/// together with the transport the device was found on.
		/// </summary>
		Available,

		/// <summary>A connection to the device is established.</summary>
		Connected,
	}
}
