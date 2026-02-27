// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using Xamarin.MacDev.Models;

#nullable enable

namespace Xamarin.MacDev {

	/// <summary>
	/// Orchestrates Apple development environment setup. Checks the current
	/// state via <see cref="EnvironmentChecker"/> and installs missing
	/// components (Command Line Tools, Xcode first-launch packages, and
	/// simulator runtimes).
	/// </summary>
	public class AppleInstaller {

		readonly ICustomLogger log;

		public AppleInstaller (ICustomLogger log)
		{
			this.log = log ?? throw new ArgumentNullException (nameof (log));
		}

		/// <summary>
		/// Ensures the Apple development environment is ready.
		/// When <paramref name="dryRun"/> is true, reports what would be
		/// installed without making any changes.
		/// Returns the final <see cref="EnvironmentCheckResult"/>.
		/// </summary>
		/// <param name="requestedPlatforms">
		/// Platforms to ensure runtimes for (e.g. "iOS", "tvOS").
		/// If null or empty, only existing runtimes are verified.
		/// </param>
		/// <param name="dryRun">
		/// When true, logs planned actions but does not execute them.
		/// </param>
		public EnvironmentCheckResult Install (IEnumerable<string>? requestedPlatforms = null, bool dryRun = false)
		{
			var checker = new EnvironmentChecker (log);

			// 1. Initial check
			log.LogInfo ("Running initial environment check...");
			var result = checker.Check ();

			// 2. Ensure Command Line Tools
			EnsureCommandLineTools (result.CommandLineTools, dryRun);

			// 3. Ensure Xcode first-launch packages
			if (result.Xcode is not null)
				EnsureFirstLaunch (checker, result.Xcode.Path, dryRun);
			else
				log.LogInfo ("No Xcode found — skipping first-launch check.");

			// 4. Ensure requested runtimes
			if (requestedPlatforms is not null)
				EnsureRuntimes (result, requestedPlatforms, dryRun);

			// 5. Re-check and return
			if (!dryRun) {
				log.LogInfo ("Running final environment check...");
				result = checker.Check ();
			}

			log.LogInfo ("Install complete. Status: {0}.", result.Status);
			return result;
		}

		/// <summary>
		/// Triggers CLT installation if not already present.
		/// Uses <c>xcode-select --install</c> to launch the macOS installer UI.
		/// </summary>
		void EnsureCommandLineTools (CommandLineToolsInfo clt, bool dryRun)
		{
			if (clt.IsInstalled) {
				log.LogInfo ("Command Line Tools already installed (v{0}).", clt.Version);
				return;
			}

			if (dryRun) {
				log.LogInfo ("[DRY RUN] Would trigger Command Line Tools installation.");
				return;
			}

			log.LogInfo ("Command Line Tools not found. Triggering installation...");
			try {
				// xcode-select --install triggers the macOS installer dialog
				var (exitCode, _, stderr) = ProcessUtils.Exec ("xcode-select", "--install");
				if (exitCode == 0)
					log.LogInfo ("Command Line Tools installer triggered. Complete the dialog to continue.");
				else
					log.LogInfo ("xcode-select --install failed (exit {0}): {1}", exitCode, stderr.Trim ());
			} catch (System.ComponentModel.Win32Exception ex) {
				log.LogInfo ("Could not run xcode-select: {0}", ex.Message);
			}
		}

		/// <summary>
		/// Ensures Xcode first-launch packages are installed.
		/// </summary>
		void EnsureFirstLaunch (EnvironmentChecker checker, string xcodePath, bool dryRun)
		{
			if (dryRun) {
				log.LogInfo ("[DRY RUN] Would run xcodebuild -runFirstLaunch.");
				return;
			}

			checker.RunFirstLaunch (xcodePath);
		}

		/// <summary>
		/// Ensures simulator runtimes are available for the requested platforms.
		/// Downloads missing runtimes via <see cref="RuntimeService.DownloadPlatform"/>.
		/// </summary>
		void EnsureRuntimes (EnvironmentCheckResult result, IEnumerable<string> requestedPlatforms, bool dryRun)
		{
			// Build a set of available runtime platforms
			var available = new HashSet<string> (StringComparer.OrdinalIgnoreCase);
			foreach (var rt in result.Runtimes) {
				if (!string.IsNullOrEmpty (rt.Platform))
					available.Add (rt.Platform);
			}

			var runtimeService = new RuntimeService (log);

			foreach (var platform in requestedPlatforms) {
				if (available.Contains (platform)) {
					log.LogInfo ("Runtime for '{0}' is already available.", platform);
					continue;
				}

				if (dryRun) {
					log.LogInfo ("[DRY RUN] Would download runtime for '{0}'.", platform);
					continue;
				}

				log.LogInfo ("Downloading runtime for '{0}'...", platform);
				runtimeService.DownloadPlatform (platform);
			}
		}
	}
}
