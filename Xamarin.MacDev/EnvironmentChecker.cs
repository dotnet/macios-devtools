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

		var xcodeManager = new XcodeManager (log);
		var xcode = xcodeManager.GetBest ();
		result.Xcode = xcode;

		if (xcode is not null) {
			log.LogInfo ("Xcode {0} found at '{1}'.", xcode.Version, xcode.Path);

			if (IsXcodeLicenseAccepted ())
				log.LogInfo ("Xcode license is accepted.");
			else
				log.LogInfo ("Xcode license may not be accepted. Run 'sudo xcodebuild -license accept'.");

			result.Platforms = GetPlatforms (xcode.Path);
		} else {
			log.LogInfo ("No Xcode installation found.");
		}

		var clt = new CommandLineTools (log);
		result.CommandLineTools = clt.Check ();

		var runtimeService = new RuntimeService (log);
		result.Runtimes = runtimeService.List (availableOnly: true);

		result.DeriveStatus ();

		log.LogInfo ("Environment check complete. Status: {0}.", result.Status);
		return result;
	}

	/// <summary>
	/// Checks whether the Xcode license has been accepted by running
	/// <c>xcrun xcodebuild -license check</c>.
	/// </summary>
	public bool IsXcodeLicenseAccepted ()
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
	List<string> GetPlatforms (string xcodePath)
	{
		var platforms = new List<string> ();
		var platformsDir = Path.Combine (xcodePath, "Contents", "Developer", "Platforms");

		if (!Directory.Exists (platformsDir))
			return platforms;

		try {
			foreach (var dir in Directory.GetDirectories (platformsDir, "*.platform")) {
				var name = Path.GetFileNameWithoutExtension (dir);
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
