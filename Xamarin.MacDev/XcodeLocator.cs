// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

#nullable enable

namespace Xamarin.MacDev {
	/// <summary>
	/// This is a class that can be used to locate Xcode according to various rules.
	/// An important design goal is that it doesn't store any static state (which has caused numerous problems in the past).
	/// </summary>
	public class XcodeLocator {

		ICustomLogger log;

		/// <summary>
		/// canonicalized Xcode location (no trailing slash, no /Contents/Developer)
		/// </summary>
		public string XcodeLocation { get; private set; } = "";

		/// <summary>
		/// The Developer root path, e.g. /Applications/Xcode.app/Contents/Developer
		/// </summary>
		public string DeveloperRoot { get => Path.Combine (XcodeLocation, "Contents", "Developer"); }

		/// <summary>
		/// The path to the version.plist file inside the Xcode bundle.
		/// </summary>
		public string DeveloperRootVersionPlist { get => Path.Combine (XcodeLocation, "Contents", "version.plist"); }

		/// <summary>
		/// The Xcode version.
		/// </summary>
		public Version XcodeVersion { get; private set; } = new Version (0, 0, 0);

		public string DTXcode { get; private set; } = "";

		/// <summary>If the Xcode location is a symlink or has a parent directory that is a symlink.</summary>
		public bool IsXcodeSymlink => PathUtils.IsSymlinkOrHasParentSymlink (XcodeLocation);

		/// <summary>Look for the Xcode location in the environment variable named <see cref="EnvironmentVariableName" />.</summary>
		public bool SupportEnvironmentVariableLookup { get; set; }

		public bool SystemHasEnvironmentVariable {
			get => !string.IsNullOrEmpty (Environment.GetEnvironmentVariable (EnvironmentVariableName));
		}

		public const string EnvironmentVariableName = "MD_APPLE_SDK_ROOT";

		/// <summary>Look for the Xcode location in ~/Library/Preferences/maui/Settings.plist or ~/Library/Preferences/Xamarin/Settings.plist.</summary>
		public bool SupportSettingsFileLookup { get; set; }

		public bool SystemHasSettingsFile {
			get => SystemExistingSettingsFiles.Any ();
		}

		public IEnumerable<string> SystemExistingSettingsFiles {
			get => SettingsPathCandidates.Where (File.Exists);
		}

		public static IEnumerable<string> SettingsPathCandidates => new string [] {
			Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.UserProfile), "Library", "Preferences", "maui", "Settings.plist"),
			Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.UserProfile), "Library", "Preferences", "Xamarin", "Settings.plist"),
		};

		public XcodeLocator (ICustomLogger logger)
		{
			log = logger;
		}

		public bool TryLocatingXcode (string? xcodeLocationOverride)
		{
			// First try the override.
			if (TryLocatingSpecificXcode (xcodeLocationOverride, out var canonicalizedXcodePath)) {
				log.LogInfo ("Found a valid Xcode using the MSBuild override (the 'XcodeLocation' property).");
				XcodeLocation = canonicalizedXcodePath;
				return true;
			}

			// Historically, this is what we've done: https://github.com/dotnet/macios/issues/11172#issuecomment-1422092279
			//
			// 1. If the MD_APPLE_SDK_ROOT environment variable is set, use that.
			// 2. If the ~/Library/Preferences/maui/Settings.plist or ~/Library/Preferences/Xamarin/Settings.plist file exists, use that.
			// 3. Check xcode-select --print-path.
			// 4. Use /Applications/Xcode.app.
			//
			// A few points:
			//
			// 1. We've recommended using MD_APPLE_SDK_ROOT in our public documentation, so make it possible to opt-in for now.
			// 2. We want to deprecate the settings files, because they just confuse people. Yet we don't want to break people, so make it possible to opt-in for now.
			// 3. This is good.
			// 4. Why check something that probably doesn't even exist (if xcode-select --print-path doesn't know about it, it probably doesn't work).
			//
			// So the current behavior is:
			//
			// 0. Use the override (dotnet/macios sets the override if the "XcodeLocation" MSBuild property is set)
			// 1. Check the MD_APPLE_SDK_ROOT variable, if this is enabled (it's off by default here; dotnet/macios enables it for .NET 10 projects and disables it for .NET 11+ projects)
			// 2. Check the settings files, if this is enabled (it's off by default here; dotnet/macios enables it for .NET 10 projects and disables it for .NET 11+ projects)
			// 3. Check `xcode-select --print-path`
			// 4. Give up.

			// 1. This is opt-out
			if (SupportEnvironmentVariableLookup && TryLocatingSpecificXcode (Environment.GetEnvironmentVariable (EnvironmentVariableName), out var location)) {
				log.LogInfo ($"Found a valid Xcode in the environment variable '{EnvironmentVariableName}'.");
				XcodeLocation = location;
				return true;
			}

			// 2. This is opt-out for the moment, but will likely become opt-in at some point.
			if (SupportSettingsFileLookup) {
				foreach (var candidate in SettingsPathCandidates) {
					if (!TryReadSettingsPath (log, candidate, out var sdkLocation))
						continue;
					if (TryLocatingSpecificXcode (sdkLocation, out location)) {
						log.LogInfo ($"Found a valid Xcode in the settings file '{candidate}'.");
						XcodeLocation = location;
						return true;
					}
				}
			}

			// 3. Not optional
			if (TryGetSystemXcode (log, out location)) {
				log.LogInfo ($"Found a valid Xcode from the system settings ('xcode-select -p').");
				XcodeLocation = location;
				return true;
			}

			log.LogInfo ($"Did not find a valid Xcode.");

			// 4. Nope
			return false;
		}

		bool TryLocatingSpecificXcode (string? xcodePath, [NotNullWhen (true)] out string? canonicalizedXcodePath)
		{
			if (!TryValidateAndCanonicalizeXcodePath (xcodePath, out canonicalizedXcodePath))
				return false;

			var versionPlistPath = Path.Combine (canonicalizedXcodePath, "Contents", "version.plist");
			if (!File.Exists (versionPlistPath)) {
				log.LogInfo ("Discarded the Xcode location '{0}' because it doesn't have the file 'Contents/version.plist'.", canonicalizedXcodePath);
				return false;
			}
			var versionPlist = PDictionary.FromFile (versionPlistPath);
			var cfBundleShortVersion = versionPlist.GetCFBundleShortVersionString ();
			var cfBundleVersion = versionPlist.GetCFBundleVersion ();

			if (!Version.TryParse (cfBundleShortVersion, out var xcodeVersion)) {
				log.LogInfo ("Discarded the Xcode location '{0}' because failure to parse the CFBundleShortVersionString value '{1}' from the file 'Contents/version.plist'.", canonicalizedXcodePath, cfBundleShortVersion);
				return false;
			}

			var infoPlistPath = Path.Combine (canonicalizedXcodePath, "Contents", "Info.plist");
			if (!File.Exists (infoPlistPath)) {
				log.LogInfo ("Discarded the Xcode location '{0}' because it doesn't have the file 'Contents/Info.plist'.", canonicalizedXcodePath);
				return false;
			}
			var infoPlist = PDictionary.FromFile (infoPlistPath);
			if (infoPlist is null) {
				log.LogInfo ("Discarded the Xcode location '{0}' because failure to parse the file 'Contents/Info.plist'.", canonicalizedXcodePath, cfBundleShortVersion);
				return false;
			}

			// Success!
			XcodeVersion = xcodeVersion;
			if (infoPlist.TryGetValue<PString> ("DTXcode", out var value))
				DTXcode = value.Value;

			log.LogInfo ("Found a valid Xcode location (Xcode {1} with CFBundleVersion={2}): {0}", canonicalizedXcodePath, cfBundleShortVersion, cfBundleVersion);

			return true;
		}

		// Accept all of these variations:
		// * /Applications/Xcode.app
		// * /Applications/Xcode.app/
		// * /Applications/Xcode.app/Contents/Developer
		// * /Applications/Xcode.app/Contents/Developer/
		// Also accept Windows-style directory separators (but the output will contain Mac-style directory separators).
		bool TryValidateAndCanonicalizeXcodePath (string? xcodePath, [NotNullWhen (true)] out string? canonicalizedXcodePath)
		{
			canonicalizedXcodePath = null;

#if NET
			if (string.IsNullOrEmpty (xcodePath))
#else
			if (string.IsNullOrEmpty (xcodePath) || xcodePath is null)
#endif
				return false;

			xcodePath = xcodePath.Replace ('\\', '/');
			xcodePath = xcodePath.TrimEnd ('/');

			if (!Directory.Exists (xcodePath)) {
				log.LogInfo ("Discarded the Xcode location '{0}' because it doesn't exist.", xcodePath);
				return false;
			}

			if (xcodePath.EndsWith ("/Contents/Developer", StringComparison.Ordinal))
				xcodePath = xcodePath.Substring (0, xcodePath.Length - "/Contents/Developer".Length);

			canonicalizedXcodePath = xcodePath;
			return true;
		}

		public static bool TryReadSettingsPath (ICustomLogger log, string settingsPath, [NotNullWhen (true)] out string? sdkLocation)
		{
			sdkLocation = null;

			if (!File.Exists (settingsPath)) {
				log.LogInfo ("The settings file {0} doesn't exist.", settingsPath);
				return false;
			}

			var plist = PDictionary.FromFile (settingsPath);
			if (plist is null) {
				log.LogInfo ("The settings file {0} exists, but it couldn't be loaded.", settingsPath);
				return false;
			}

			if (!plist.TryGetValue<PString> ("AppleSdkRoot", out var value)) {
				log.LogInfo ("The settings file {0} exists, but there's no 'AppleSdkRoot' entry in it.", settingsPath);
				return false;
			}

			var location = value?.Value;
#if NET
			if (string.IsNullOrEmpty (location)) {
#else
			if (string.IsNullOrEmpty (location) || location is null) {
#endif
				log.LogInfo ("The settings file {0} exists, but the 'AppleSdkRoot' is empty.", settingsPath);
				return false;
			}

			log.LogInfo ("An Xcode location was found in the file '{0}': {1}", settingsPath, location);
			sdkLocation = location;
			return true;
		}

		public static bool TryGetSystemXcode (ICustomLogger log, [NotNullWhen (true)] out string? path)
		{
			path = null;

			var xcodeSelect = "/usr/bin/xcode-select";
			if (!File.Exists (xcodeSelect)) {
				log.LogInfo ("Could not get the system's Xcode location, because the file '{0}' doesn't exist.", xcodeSelect);
				return false;
			}

			try {
				using var process = new Process ();
				process.StartInfo.FileName = xcodeSelect;
				process.StartInfo.Arguments = "--print-path";
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.UseShellExecute = false;
				process.Start ();
				var stdout = process.StandardOutput.ReadToEnd ();
				process.WaitForExit ();

				stdout = stdout.Trim ();
				if (Directory.Exists (stdout)) {
					if (stdout.EndsWith ("/Contents/Developer", StringComparison.Ordinal))
						stdout = stdout.Substring (0, stdout.Length - "/Contents/Developer".Length);

					path = stdout;
					log.LogInfo ("Detect the Xcode location configured for this system (found using 'xcode-select -p'): '{0}'", path);
					return true;
				}

				log.LogInfo ("The system's Xcode location (found using 'xcode-select -p') does not exist: '{0}'", stdout);

				return false;
			} catch (Exception e) {
				log.LogInfo ("Could not get the system's Xcode location: {0}", e);
				return false;
			}
		}
	}
}
