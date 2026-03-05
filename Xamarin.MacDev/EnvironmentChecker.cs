// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.MacDev.Models;

#nullable enable

namespace Xamarin.MacDev;

/// <summary>
/// Performs a comprehensive check of the Apple development environment.
/// Aggregates results from <see cref="CommandLineTools"/>,
/// <see cref="XcodeManager"/>, and <see cref="RuntimeService"/>.
/// </summary>
public class EnvironmentChecker {

	static readonly string XcrunPath = "/usr/bin/xcrun";

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

		result.Xcode = GetBestXcode ();

		if (result.Xcode is not null) {
			log.LogInfo ("Xcode {0} found at '{1}'.", result.Xcode.Version, result.Xcode.Path);

			if (IsXcodeLicenseAccepted ())
				log.LogInfo ("Xcode license is accepted.");
			else
				log.LogInfo ("Xcode license may not be accepted. Run 'sudo xcodebuild -license accept'.");

			result.Platforms = GetPlatforms (result.Xcode.Path);
		} else {
			log.LogInfo ("No Xcode installation found.");
		}

		try {
			result.CommandLineTools = CheckCommandLineTools ();
		} catch (Exception ex) {
			log.LogInfo ("Could not check Command Line Tools: {0}", ex.Message);
		}

		try {
			result.Runtimes = ListRuntimes ();
		} catch (Exception ex) {
			log.LogInfo ("Could not check runtimes: {0}", ex.Message);
		}

		result.DeriveStatus ();

		log.LogInfo ("Environment check complete. Status: {0}.", result.Status);
		return result;
	}

	/// <summary>
	/// Returns the best available Xcode installation, or null if none found.
	/// </summary>
	protected virtual XcodeInfo? GetBestXcode ()
	{
		var xcodeManager = new XcodeManager (log);
		return xcodeManager.GetBest ();
	}

	/// <summary>
	/// Checks Command Line Tools installation status.
	/// </summary>
	protected virtual CommandLineToolsInfo CheckCommandLineTools ()
	{
		var clt = new CommandLineTools (log);
		return clt.Check ();
	}

	/// <summary>
	/// Lists available simulator runtimes.
	/// </summary>
	protected virtual List<SimulatorRuntimeInfo> ListRuntimes ()
	{
		var runtimeService = new RuntimeService (log);
		return runtimeService.List (availableOnly: true);
	}

	/// <summary>
	/// Checks whether the Xcode license has been accepted by running
	/// <c>xcrun xcodebuild -license check</c>.
	/// </summary>
	public virtual bool IsXcodeLicenseAccepted ()
	{
		try {
			var (exitCode, _, _) = ProcessUtils.Exec (XcrunPath, "xcodebuild", "-license", "check");
			return exitCode == 0;
		} catch (System.ComponentModel.Win32Exception) {
			return false;
		} catch (InvalidOperationException) {
			return false;
		}
	}

	/// <summary>
	/// Runs <c>xcrun xcodebuild -runFirstLaunch</c> to ensure packages are installed.
	/// Returns true if the command succeeded.
	/// </summary>
	public bool RunFirstLaunch ()
	{
		try {
			log.LogInfo ("Running xcodebuild -runFirstLaunch...");
			var (exitCode, _, stderr) = ProcessUtils.Exec (XcrunPath, "xcodebuild", "-runFirstLaunch");
			if (exitCode != 0) {
				log.LogInfo ("xcodebuild -runFirstLaunch failed (exit {0}): {1}", exitCode, stderr.Trim ());
				return false;
			}

			log.LogInfo ("xcodebuild -runFirstLaunch completed successfully.");
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
	/// Gets the list of available platform SDK directories in the Xcode bundle.
	/// </summary>
	protected virtual List<string> GetPlatforms (string xcodePath)
	{
		var platformsDir = Path.Combine (xcodePath, "Contents", "Developer", "Platforms");

		if (!Directory.Exists (platformsDir))
			return new List<string> ();

		try {
			var directoryNames = new List<string> ();
			foreach (var dir in Directory.GetDirectories (platformsDir, "*.platform"))
				directoryNames.Add (Path.GetFileNameWithoutExtension (dir));

			return MapDirectoryNamesToPlatforms (directoryNames);
		} catch (UnauthorizedAccessException ex) {
			log.LogInfo ("Could not read platforms directory: {0}", ex.Message);
			return new List<string> ();
		}
	}

	/// <summary>
	/// Maps a list of Apple platform directory names (e.g. "iPhoneOS", "MacOSX")
	/// to deduplicated friendly names (e.g. "iOS", "macOS").
	/// </summary>
	public static List<string> MapDirectoryNamesToPlatforms (IEnumerable<string> directoryNames)
	{
		var platforms = new List<string> ();
		foreach (var name in directoryNames) {
			var friendly = MapPlatformName (name);
			if (!platforms.Contains (friendly))
				platforms.Add (friendly);
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
