// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Linq;

using Xamarin.MacDev.Models;

#nullable enable

namespace Xamarin.MacDev {

	/// <summary>
	/// Performs a comprehensive check of the Apple development environment.
	/// Aggregates results from <see cref="CommandLineTools"/>,
	/// <see cref="XcodeManager"/>, and <see cref="RuntimeService"/>.
	/// Also validates Xcode license acceptance and first-launch state,
	/// patterns from ClientTools.Platform iOSSshCommandsExtensions.
	/// </summary>
	public class EnvironmentChecker {

		readonly ICustomLogger log;

		public EnvironmentChecker (ICustomLogger log)
		{
			this.log = log ?? throw new ArgumentNullException (nameof (log));
		}

		/// <summary>
		/// Runs a full environment check and returns the results.
		/// </summary>
		public EnvironmentCheckResult Check ()
		{
			var result = new EnvironmentCheckResult ();

			// 1. Check Xcode
			var xcodeManager = new XcodeManager (log);
			var xcode = xcodeManager.GetBest ();
			result.Xcode = xcode;

			if (xcode is not null) {
				log.LogInfo ("Xcode {0} found at '{1}'.", xcode.Version, xcode.Path);

				// Check license acceptance (pattern from ClientTools.Platform)
				if (IsXcodeLicenseAccepted (xcode.Path))
					log.LogInfo ("Xcode license is accepted.");
				else
					log.LogInfo ("Xcode license may not be accepted. Run 'sudo xcodebuild -license accept'.");

				// Collect platform SDKs
				result.Platforms = GetPlatforms (xcode.Path);
			} else {
				log.LogInfo ("No Xcode installation found.");
			}

			// 2. Check Command Line Tools
			var clt = new CommandLineTools (log);
			result.CommandLineTools = clt.Check ();

			// 3. Check runtimes
			var runtimeService = new RuntimeService (log);
			result.Runtimes = runtimeService.List (availableOnly: true);

			// 4. Derive overall status
			result.DeriveStatus ();

			log.LogInfo ("Environment check complete. Status: {0}.", result.Status);
			return result;
		}

		/// <summary>
		/// Checks whether the Xcode license has been accepted by running
		/// <c>xcodebuild -license check</c>.
		/// Pattern from ClientTools.Platform iOSSshCommandsExtensions.CheckXcodeLicenseAsync.
		/// </summary>
		public bool IsXcodeLicenseAccepted (string xcodePath)
		{
			var xcodebuildPath = Path.Combine (xcodePath, "Contents", "Developer", "usr", "bin", "xcodebuild");
			if (!File.Exists (xcodebuildPath))
				return false;

			try {
				var (exitCode, _, _) = ProcessUtils.Exec (xcodebuildPath, "-license", "check");
				return exitCode == 0;
			} catch (System.ComponentModel.Win32Exception) {
				return false;
			}
		}

		/// <summary>
		/// Runs <c>xcodebuild -runFirstLaunch</c> to ensure packages are installed.
		/// Pattern from ClientTools.Platform iOSSshCommandsExtensions.RunXcodeBuildFirstLaunchAsync.
		/// Returns true if the command succeeded.
		/// </summary>
		public bool RunFirstLaunch (string xcodePath)
		{
			var xcodebuildPath = Path.Combine (xcodePath, "Contents", "Developer", "usr", "bin", "xcodebuild");
			if (!File.Exists (xcodebuildPath)) {
				log.LogInfo ("xcodebuild not found at '{0}'.", xcodebuildPath);
				return false;
			}

			try {
				log.LogInfo ("Running xcodebuild -runFirstLaunch...");
				var (exitCode, _, stderr) = ProcessUtils.Exec (xcodebuildPath, "-runFirstLaunch");
				if (exitCode != 0) {
					log.LogInfo ("xcodebuild -runFirstLaunch failed (exit {0}): {1}", exitCode, stderr.Trim ());
					return false;
				}

				log.LogInfo ("xcodebuild -runFirstLaunch completed successfully.");
				return true;
			} catch (System.ComponentModel.Win32Exception ex) {
				log.LogInfo ("Could not run xcodebuild: {0}", ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Gets the list of available platform SDK directories in the Xcode bundle.
		/// </summary>
		System.Collections.Generic.List<string> GetPlatforms (string xcodePath)
		{
			var platforms = new System.Collections.Generic.List<string> ();
			var platformsDir = Path.Combine (xcodePath, "Contents", "Developer", "Platforms");

			if (!Directory.Exists (platformsDir))
				return platforms;

			try {
				foreach (var dir in Directory.GetDirectories (platformsDir, "*.platform")) {
					var name = Path.GetFileNameWithoutExtension (dir);
					// Convert "iPhoneOS" to "iOS", "AppleTVOS" to "tvOS", etc.
					var friendly = MapPlatformName (name);
					if (!platforms.Contains (friendly))
						platforms.Add (friendly);
				}
			} catch (UnauthorizedAccessException ex) {
				log.LogInfo ("Could not read platforms directory: {0}", ex.Message);
			}

			return platforms;
		}

		/// <summary>
		/// Maps Apple platform directory names to friendly names.
		/// </summary>
		public static string MapPlatformName (string sdkName)
		{
			switch (sdkName) {
			case "iPhoneOS":
			case "iPhoneSimulator":
				return "iOS";
			case "AppleTVOS":
			case "AppleTVSimulator":
				return "tvOS";
			case "WatchOS":
			case "WatchSimulator":
				return "watchOS";
			case "XROS":
			case "XRSimulator":
				return "visionOS";
			case "MacOSX":
				return "macOS";
			default:
				return sdkName;
			}
		}
	}
}
