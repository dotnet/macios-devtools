// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Xamarin.MacDev.Models {

	/// <summary>
	/// Information about Xcode Command Line Tools installation.
	/// </summary>
	public class CommandLineToolsInfo {
		/// <summary>Whether the Command Line Tools are installed.</summary>
		public bool IsInstalled { get; set; }

		/// <summary>The Command Line Tools version string (e.g. "16.2.0.0.1.1733547573"), or null if not installed.</summary>
		public string? Version { get; set; }

		/// <summary>The Command Line Tools install path (e.g. "/Library/Developer/CommandLineTools"), or null if not installed.</summary>
		public string? Path { get; set; }

		public override string ToString () => IsInstalled ? $"Command Line Tools {Version} at {Path}" : "Command Line Tools not installed";
	}
}
