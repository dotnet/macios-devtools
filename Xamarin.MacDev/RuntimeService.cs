// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.MacDev.Models;

#nullable enable

namespace Xamarin.MacDev;

/// <summary>
/// Manages simulator runtimes via <c>xcrun simctl</c> and <c>xcrun xcodebuild</c>.
/// </summary>
public class RuntimeService {

	static readonly string XcrunPath = "/usr/bin/xcrun";

	readonly ICustomLogger log;

	public RuntimeService (ICustomLogger log)
	{
		this.log = log ?? throw new ArgumentNullException (nameof (log));
	}

	/// <summary>
	/// Lists installed simulator runtimes. Optionally filters by availability.
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
		if (string.IsNullOrEmpty (platform))
			throw new ArgumentException ("Platform must not be null or empty.", nameof (platform));

		var all = List (availableOnly);
		return all.Where (r => string.Equals (r.Platform, platform, StringComparison.OrdinalIgnoreCase)).ToList ();
	}

	/// <summary>
	/// Downloads a platform runtime using <c>xcrun xcodebuild -downloadPlatform</c>.
	/// </summary>
	/// <param name="platform">The platform to download (e.g. "iOS", "tvOS", "watchOS", "visionOS").</param>
	/// <param name="version">Optional specific version to download (e.g. "17.5").</param>
	/// <returns>True if the download command succeeded.</returns>
	public bool DownloadPlatform (string platform, string? version = null)
	{
		if (string.IsNullOrEmpty (platform))
			throw new ArgumentException ("Platform must not be null or empty.", nameof (platform));

		log.LogInfo ("Downloading {0} platform runtime via xcodebuild...", platform);

		try {
			var args = string.IsNullOrEmpty (version)
				? new [] { "xcodebuild", "-downloadPlatform", platform }
				: new [] { "xcodebuild", "-downloadPlatform", platform, "-buildVersion", version! };

			var (exitCode, _, stderr) = ProcessUtils.Exec (XcrunPath, args);
			if (exitCode != 0) {
				log.LogInfo ("xcodebuild -downloadPlatform {0} failed (exit {1}): {2}", platform, exitCode, stderr.Trim ());
				return false;
			}

			log.LogInfo ("Successfully downloaded {0} platform runtime.", platform);
			return true;
		} catch (System.ComponentModel.Win32Exception ex) {
			log.LogInfo ("Could not run xcodebuild: {0}", ex.Message);
			return false;
		} catch (InvalidOperationException ex) {
			log.LogInfo ("Could not run xcodebuild: {0}", ex.Message);
			return false;
		}
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
		} catch (InvalidOperationException ex) {
			log.LogInfo ("Could not run xcrun: {0}", ex.Message);
			return null;
		}
	}
}
