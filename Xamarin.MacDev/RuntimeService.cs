// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Xamarin.MacDev.Models;

#nullable enable

namespace Xamarin.MacDev {

	/// <summary>
	/// Manages simulator runtimes via <c>xcrun simctl</c> and <c>xcodebuild</c>.
	/// Lists installed runtimes and supports downloading new platform runtimes.
	/// Download approach from ClientTools.Platform: <c>xcodebuild -downloadPlatform iOS</c>.
	/// </summary>
	public class RuntimeService {

		static readonly string XcrunPath = "/usr/bin/xcrun";
		static readonly string XcodebuildRelativePath = "Contents/Developer/usr/bin/xcodebuild";

		readonly ICustomLogger log;

		public RuntimeService (ICustomLogger log)
		{
			this.log = log ?? throw new ArgumentNullException (nameof (log));
		}

		/// <summary>
		/// Lists installed simulator runtimes. Optionally filters by availability.
		/// Uses <c>xcrun simctl list runtimes --json</c>.
		/// </summary>
		public List<SimulatorRuntimeInfo> List (bool availableOnly = false)
		{
			var json = RunSimctl ("list", "runtimes", "--json");
			if (json is null)
				return new List<SimulatorRuntimeInfo> ();

			var runtimes = SimctlOutputParser.ParseRuntimes (json);

			if (availableOnly)
				runtimes.RemoveAll (r => !r.IsAvailable);

			log.LogInfo ("Found {0} simulator runtime(s).", runtimes.Count);
			return runtimes;
		}

		/// <summary>
		/// Lists runtimes for a specific platform (e.g. "iOS", "tvOS", "watchOS", "visionOS").
		/// </summary>
		public List<SimulatorRuntimeInfo> ListByPlatform (string platform, bool availableOnly = false)
		{
			var all = List (availableOnly);
			return all.Where (r => string.Equals (r.Platform, platform, StringComparison.OrdinalIgnoreCase)).ToList ();
		}

		/// <summary>
		/// Downloads a platform runtime using <c>xcodebuild -downloadPlatform</c>.
		/// Pattern from ClientTools.Platform RemoteSimulatorValidator.
		/// </summary>
		/// <param name="platform">The platform to download (e.g. "iOS", "tvOS", "watchOS", "visionOS").</param>
		/// <param name="xcodePath">The Xcode.app path. If null, looks for xcodebuild in PATH via xcrun.</param>
		/// <returns>True if the download command succeeded.</returns>
		public bool DownloadPlatform (string platform, string? xcodePath = null)
		{
			if (string.IsNullOrEmpty (platform))
				throw new ArgumentException ("Platform must not be null or empty.", nameof (platform));

			var xcodebuildPath = ResolveXcodebuild (xcodePath);
			if (xcodebuildPath is null) {
				log.LogInfo ("Cannot download platform: xcodebuild not found.");
				return false;
			}

			log.LogInfo ("Downloading {0} platform runtime via xcodebuild...", platform);

			try {
				var (exitCode, stdout, stderr) = ProcessUtils.Exec (xcodebuildPath, "-downloadPlatform", platform);
				if (exitCode != 0) {
					log.LogInfo ("xcodebuild -downloadPlatform {0} failed (exit {1}): {2}", platform, exitCode, stderr.Trim ());
					return false;
				}

				log.LogInfo ("Successfully downloaded {0} platform runtime.", platform);
				return true;
			} catch (System.ComponentModel.Win32Exception ex) {
				log.LogInfo ("Could not run xcodebuild: {0}", ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Resolves the path to xcodebuild. If xcodePath is given, looks inside the Xcode bundle.
		/// Otherwise falls back to /usr/bin/xcrun xcodebuild.
		/// </summary>
		string? ResolveXcodebuild (string? xcodePath)
		{
			if (!string.IsNullOrEmpty (xcodePath)) {
				var path = Path.Combine (xcodePath!, XcodebuildRelativePath);
				if (File.Exists (path))
					return path;
			}

			// Fall back to xcrun to find xcodebuild
			if (File.Exists (XcrunPath)) {
				try {
					var (exitCode, stdout, _) = ProcessUtils.Exec (XcrunPath, "--find", "xcodebuild");
					if (exitCode == 0) {
						var path = stdout.Trim ();
						if (File.Exists (path))
							return path;
					}
				} catch (System.ComponentModel.Win32Exception) {
					// fall through
				}
			}

			return null;
		}

		/// <summary>
		/// Runs a simctl subcommand and returns stdout, or null on failure.
		/// </summary>
		string? RunSimctl (params string [] args)
		{
			if (!File.Exists (XcrunPath)) {
				log.LogInfo ("xcrun not found at '{0}'.", XcrunPath);
				return null;
			}

			var fullArgs = new string [args.Length + 1];
			fullArgs [0] = "simctl";
			Array.Copy (args, 0, fullArgs, 1, args.Length);

			try {
				var (exitCode, stdout, stderr) = ProcessUtils.Exec (XcrunPath, fullArgs);
				if (exitCode != 0) {
					log.LogInfo ("simctl {0} failed (exit {1}): {2}", args [0], exitCode, stderr.Trim ());
					return null;
				}
				return stdout;
			} catch (System.ComponentModel.Win32Exception ex) {
				log.LogInfo ("Could not run xcrun: {0}", ex.Message);
				return null;
			}
		}
	}
}
