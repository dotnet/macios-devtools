// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Xamarin.MacDev.Models {

	/// <summary>
	/// The outcome of a physical device discovery operation.
	/// Only <see cref="Success"/> means the device list can be trusted; an empty list
	/// with <see cref="Success"/> means devicectl ran and no physical devices are known.
	/// </summary>
	public enum PhysicalDeviceDiscoveryStatus {
		/// <summary>devicectl ran and produced valid output. The device list may be empty.</summary>
		Success = 0,

		/// <summary>The current host is not macOS, so devicectl cannot run. Nothing was executed.</summary>
		UnsupportedHost,

		/// <summary>xcrun or devicectl is not available (for example Xcode is missing, too old, or not selected).</summary>
		ToolUnavailable,

		/// <summary>devicectl was started but reported a failure (a non-zero exit code, or an error in its output).</summary>
		ExecutionFailed,

		/// <summary>devicectl did not finish within the allowed time and was terminated.</summary>
		TimedOut,

		/// <summary>devicectl exited successfully, but its output was missing, malformed or not in the expected format.</summary>
		InvalidOutput,
	}
}
