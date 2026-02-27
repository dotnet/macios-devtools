// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Xamarin.MacDev.Models {

	/// <summary>
	/// Information about a simulator runtime from xcrun simctl.
	/// </summary>
	public class SimulatorRuntimeInfo {
		/// <summary>The platform name (e.g. "iOS", "tvOS", "watchOS", "visionOS").</summary>
		public string Platform { get; set; } = "";

		/// <summary>The runtime version (e.g. "18.2").</summary>
		public string Version { get; set; } = "";

		/// <summary>The build version (e.g. "22C150").</summary>
		public string BuildVersion { get; set; } = "";

		/// <summary>The runtime identifier (e.g. "com.apple.CoreSimulator.SimRuntime.iOS-18-2").</summary>
		public string Identifier { get; set; } = "";

		/// <summary>The display name (e.g. "iOS 18.2").</summary>
		public string Name { get; set; } = "";

		/// <summary>Whether this runtime is available for use.</summary>
		public bool IsAvailable { get; set; }

		/// <summary>Whether this runtime is bundled with Xcode (vs downloaded separately).</summary>
		public bool IsBundled { get; set; }

		public override string ToString () => $"{Name} ({Identifier})";
	}
}
