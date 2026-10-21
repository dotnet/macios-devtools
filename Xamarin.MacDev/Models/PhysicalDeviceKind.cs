// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Xamarin.MacDev.Models {

	/// <summary>
	/// The kind of Apple device, classified from the device class, product type and platform
	/// reported by devicectl (never from the device name).
	/// </summary>
	public enum PhysicalDeviceKind {
		/// <summary>The kind could not be determined from the data devicectl reported.</summary>
		Unknown = 0,
		IPhone,
		IPad,
		IPod,
		AppleWatch,
		AppleTV,
		/// <summary>Apple Vision Pro and other visionOS devices.</summary>
		AppleVision,
		/// <summary>A Mac, which devicectl also lists.</summary>
		Mac,
	}
}
