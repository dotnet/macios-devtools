// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections.Generic;

#nullable enable

namespace Xamarin.MacDev.Models {

	/// <summary>
	/// Overall status of the Apple development environment.
	/// </summary>
	public enum EnvironmentStatus {
		/// <summary>All required components are present.</summary>
		Ok,
		/// <summary>Some optional components are missing.</summary>
		Partial,
		/// <summary>Required components are missing.</summary>
		Missing,
	}

	/// <summary>
	/// Result of a comprehensive Apple environment check.
	/// </summary>
	public class EnvironmentCheckResult {
		/// <summary>Information about the active Xcode installation, or null if none found.</summary>
		public XcodeInfo? Xcode { get; set; }

		/// <summary>Information about the Command Line Tools.</summary>
		public CommandLineToolsInfo CommandLineTools { get; set; } = new CommandLineToolsInfo ();

		/// <summary>Installed simulator runtimes.</summary>
		public List<SimulatorRuntimeInfo> Runtimes { get; set; } = new List<SimulatorRuntimeInfo> ();

		/// <summary>Enabled development platforms (e.g. "iOS", "macOS").</summary>
		public List<string> Platforms { get; set; } = new List<string> ();

		/// <summary>Overall environment status.</summary>
		public EnvironmentStatus Status { get; set; } = EnvironmentStatus.Missing;

		/// <summary>
		/// Derives the <see cref="Status"/> from the current state of the environment.
		/// </summary>
		public void DeriveStatus ()
		{
			if (Xcode is null || !CommandLineTools.IsInstalled) {
				Status = EnvironmentStatus.Missing;
				return;
			}

			if (Runtimes.Count == 0) {
				Status = EnvironmentStatus.Partial;
				return;
			}

			Status = EnvironmentStatus.Ok;
		}
	}
}
