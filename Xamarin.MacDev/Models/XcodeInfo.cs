// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

#nullable enable

namespace Xamarin.MacDev.Models {

	/// <summary>
	/// Information about an Xcode installation.
	/// </summary>
	public class XcodeInfo {
		/// <summary>The path to the Xcode.app bundle (e.g. /Applications/Xcode.app).</summary>
		public string Path { get; set; } = "";

		/// <summary>The Xcode version (e.g. 16.2).</summary>
		public Version Version { get; set; } = new Version (0, 0);

		/// <summary>The Xcode build number (e.g. 16C5032a).</summary>
		public string Build { get; set; } = "";

		/// <summary>The DTXcode value from the version plist.</summary>
		public string DTXcode { get; set; } = "";

		/// <summary>Whether this is the currently selected Xcode (via xcode-select).</summary>
		public bool IsSelected { get; set; }

		/// <summary>Whether the Xcode path is or contains a symlink.</summary>
		public bool IsSymlink { get; set; }

		public override string ToString () => $"{Path} ({Version}, {Build})";
	}
}
