// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Collections.ObjectModel;

#nullable enable

namespace Xamarin.MacDev.Models {

	/// <summary>
	/// The result of <see cref="PhysicalDeviceService.ListAsync(System.Threading.CancellationToken)"/>.
	/// Check <see cref="Status"/> before using <see cref="Devices"/>: an empty list is only meaningful
	/// when the status is <see cref="PhysicalDeviceDiscoveryStatus.Success"/>.
	/// </summary>
	public sealed class PhysicalDeviceDiscoveryResult {
		/// <summary>Creates a result.</summary>
		/// <param name="status">The outcome of the discovery.</param>
		/// <param name="devices">The discovered devices. Ignored unless <paramref name="status"/> is <see cref="PhysicalDeviceDiscoveryStatus.Success"/>.</param>
		/// <param name="errorMessage">A human-readable description of the failure, for non-success statuses.</param>
		/// <param name="exitCode">The exit code of the devicectl process, when one was run to completion.</param>
		public PhysicalDeviceDiscoveryResult (PhysicalDeviceDiscoveryStatus status, IEnumerable<PhysicalDeviceInfo>? devices = null, string? errorMessage = null, int? exitCode = null)
		{
			Status = status;
			Devices = new ReadOnlyCollection<PhysicalDeviceInfo> (
				status == PhysicalDeviceDiscoveryStatus.Success && devices is not null
					? new List<PhysicalDeviceInfo> (devices)
					: new List<PhysicalDeviceInfo> ());
			ErrorMessage = errorMessage ?? "";
			ExitCode = exitCode;
		}

		/// <summary>The outcome of the discovery.</summary>
		public PhysicalDeviceDiscoveryStatus Status { get; }

		/// <summary><c>true</c> if devicectl ran and its output was understood; <see cref="Devices"/> may still be empty.</summary>
		public bool IsSuccess => Status == PhysicalDeviceDiscoveryStatus.Success;

		/// <summary>
		/// The physical devices known to CoreDevice, in the order devicectl reported them. This includes
		/// devices that are currently not reachable (see <see cref="PhysicalDeviceInfo.Availability"/>);
		/// simulators are excluded. Always empty unless <see cref="IsSuccess"/> is <c>true</c>.
		/// </summary>
		public IReadOnlyList<PhysicalDeviceInfo> Devices { get; }

		/// <summary>A human-readable description of the failure. <see cref="PhysicalDeviceService"/> leaves it empty when <see cref="IsSuccess"/> is <c>true</c>.</summary>
		public string ErrorMessage { get; }

		/// <summary>The exit code of the devicectl (or xcrun) process, or <c>null</c> if no process ran to completion.</summary>
		public int? ExitCode { get; }

		public override string ToString () => IsSuccess
			? $"{Status} ({Devices.Count} devices)"
			: $"{Status}: {ErrorMessage}";
	}
}
