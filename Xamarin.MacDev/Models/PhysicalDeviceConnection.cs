// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

#nullable enable

namespace Xamarin.MacDev.Models {

	/// <summary>
	/// One way a physical device is (or was) reachable, as reported by a single devicectl entry.
	/// A device that is attached over USB and also visible on the local network is reported
	/// once per transport; see <see cref="PhysicalDeviceInfo.Connections"/>.
	/// </summary>
	public class PhysicalDeviceConnection {
		/// <summary>The CoreDevice identifier of the devicectl entry that reported this connection.</summary>
		public string Identifier { get; set; } = "";

		/// <summary>The raw transport type (e.g. "wired", "localNetwork"). Empty if not reported.</summary>
		public string TransportType { get; set; } = "";

		/// <summary>
		/// The raw pairing state (e.g. "paired") that this entry reported. Empty if not reported, even when
		/// <see cref="PhysicalDeviceInfo.PairingState"/> of the merged device was filled in from another entry.
		/// </summary>
		public string PairingState { get; set; } = "";

		/// <summary>The raw connection (tunnel) state: "connected", "disconnected" or "unavailable". Empty if not reported.</summary>
		public string ConnectionState { get; set; } = "";

		/// <summary>The reachability derived from <see cref="ConnectionState"/>.</summary>
		public PhysicalDeviceAvailability Availability => GetAvailability (ConnectionState);

		internal static PhysicalDeviceAvailability GetAvailability (string? connectionState)
		{
			if (string.Equals (connectionState, "connected", StringComparison.OrdinalIgnoreCase))
				return PhysicalDeviceAvailability.Connected;
			if (string.Equals (connectionState, "disconnected", StringComparison.OrdinalIgnoreCase))
				return PhysicalDeviceAvailability.Available;
			if (string.Equals (connectionState, "unavailable", StringComparison.OrdinalIgnoreCase))
				return PhysicalDeviceAvailability.Unavailable;
			return PhysicalDeviceAvailability.Unknown;
		}

		public override string ToString () => $"{Identifier} [{TransportType}, {ConnectionState}]";
	}
}
