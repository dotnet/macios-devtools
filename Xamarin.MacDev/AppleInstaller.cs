// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Xamarin.MacDev.Models;

#nullable enable

namespace Xamarin.MacDev;

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
	/// </summary>
	public EnvironmentCheckResult Install (IEnumerable<string>? requestedPlatforms = null, bool dryRun = false)
	{
		var checker = new EnvironmentChecker (log);

		log.LogInfo ("Running initial environment check...");
		var result = checker.Check ();

		EnsureCommandLineTools (result.CommandLineTools, dryRun);

		if (result.Xcode is not null)
			EnsureFirstLaunch (checker, dryRun);
		else
			log.LogInfo ("No Xcode found — skipping first-launch check.");

		if (requestedPlatforms is not null)
			EnsureRuntimes (result, requestedPlatforms, dryRun);

		if (!dryRun) {
			log.LogInfo ("Running final environment check...");
			result = checker.Check ();
		}

		log.LogInfo ("Install complete. Status: {0}.", result.Status);
		return result;
	}

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

		const string xcodeSelectPath = "/usr/bin/xcode-select";

		log.LogInfo ("Command Line Tools not found. Triggering installation...");
		try {
			var (exitCode, _, stderr) = ProcessUtils.Exec (xcodeSelectPath, "--install");
			if (exitCode == 0)
				log.LogInfo ("Command Line Tools installer triggered. Complete the dialog to continue.");
			else
				log.LogInfo ("xcode-select --install failed (exit {0}): {1}", exitCode, stderr.Trim ());
		} catch (System.ComponentModel.Win32Exception ex) {
			log.LogInfo ("Could not run xcode-select: {0}", ex.Message);
		} catch (InvalidOperationException ex) {
			log.LogInfo ("Could not run xcode-select: {0}", ex.Message);
		}
	}

	void EnsureFirstLaunch (EnvironmentChecker checker, bool dryRun)
	{
		if (dryRun) {
			log.LogInfo ("[DRY RUN] Would run xcodebuild -runFirstLaunch.");
			return;
		}

		checker.RunFirstLaunch ();
	}

	void EnsureRuntimes (EnvironmentCheckResult result, IEnumerable<string> requestedPlatforms, bool dryRun)
	{
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
