// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;

using Xamarin.MacDev.Models;

#nullable enable

namespace Xamarin.MacDev {

	/// <summary>
	/// Lists Xcode installations, reads their metadata, and supports selecting
	/// the active Xcode. Reuses existing <see cref="XcodeLocator"/> for path
	/// validation and plist reading, and <see cref="ProcessUtils"/> for shell commands.
	/// </summary>
	public class XcodeManager {

		static readonly string XcodeSelectPath = "/usr/bin/xcode-select";
		static readonly string MdfindPath = "/usr/bin/mdfind";
		static readonly string ApplicationsDir = "/Applications";

		readonly ICustomLogger log;

		public XcodeManager (ICustomLogger log)
		{
			this.log = log ?? throw new ArgumentNullException (nameof (log));
		}

		/// <summary>
		/// Lists all Xcode installations found on the system.
		/// Searches via Spotlight (mdfind) and the /Applications directory,
		/// deduplicates by resolved path, then reads version metadata from each.
		/// </summary>
		public List<XcodeInfo> List ()
		{
			var selectedPath = GetSelectedPath ();
			var candidates = FindXcodeApps ();
			var results = new List<XcodeInfo> ();
			var seen = new HashSet<string> (StringComparer.Ordinal);

			foreach (var appPath in candidates) {
				if (!seen.Add (appPath))
					continue;

				var info = ReadXcodeInfo (appPath);
				if (info is null)
					continue;

				if (selectedPath is not null && appPath.Equals (selectedPath, StringComparison.Ordinal))
					info.IsSelected = true;

				results.Add (info);
			}

			log.LogInfo ("Found {0} Xcode installation(s).", results.Count);
			return results;
		}

		/// <summary>
		/// Returns information about the currently selected Xcode, or null if none is selected.
		/// </summary>
		public XcodeInfo? GetSelected ()
		{
			var selectedPath = GetSelectedPath ();
			if (selectedPath is null)
				return null;

			var info = ReadXcodeInfo (selectedPath);
			if (info is not null)
				info.IsSelected = true;

			return info;
		}

		/// <summary>
		/// Returns the best available Xcode: the currently selected one, or
		/// the highest-versioned installation if none is selected.
		/// </summary>
		public XcodeInfo? GetBest ()
		{
			var all = List ();
			if (all.Count == 0)
				return null;

			var selected = all.Find (x => x.IsSelected);
			if (selected is not null)
				return selected;

			all.Sort ((a, b) => b.Version.CompareTo (a.Version));
			return all [0];
		}

		/// <summary>
		/// Selects the active Xcode by calling <c>xcode-select -s</c>.
		/// Returns true if the command succeeded.
		/// Note: this typically requires root privileges (sudo).
		/// </summary>
		public bool Select (string path)
		{
			if (string.IsNullOrEmpty (path))
				throw new ArgumentException ("Path must not be null or empty.", nameof (path));

			if (!Directory.Exists (path)) {
				log.LogInfo ("Cannot select Xcode: path '{0}' does not exist.", path);
				return false;
			}

			if (!File.Exists (XcodeSelectPath)) {
				log.LogInfo ("Cannot select Xcode: xcode-select not found.");
				return false;
			}

			try {
				var (exitCode, _, stderr) = ProcessUtils.Exec (XcodeSelectPath, "-s", path);
				if (exitCode != 0) {
					log.LogInfo ("xcode-select -s returned exit code {0}: {1}", exitCode, stderr.Trim ());
					return false;
				}

				log.LogInfo ("Selected Xcode at '{0}'.", path);
				return true;
			} catch (System.ComponentModel.Win32Exception ex) {
				log.LogInfo ("Could not run xcode-select: {0}", ex.Message);
				return false;
			}
		}

		/// <summary>
		/// Returns the canonicalized path of the currently selected Xcode, or null.
		/// Strips the /Contents/Developer suffix that xcode-select -p returns.
		/// </summary>
		string? GetSelectedPath ()
		{
			if (!File.Exists (XcodeSelectPath))
				return null;

			try {
				var (exitCode, stdout, _) = ProcessUtils.Exec (XcodeSelectPath, "--print-path");
				if (exitCode != 0)
					return null;

				var path = stdout.Trim ();
				return CanonicalizeXcodePath (path);
			} catch (System.ComponentModel.Win32Exception) {
				return null;
			}
		}

		/// <summary>
		/// Finds Xcode.app bundles via mdfind and /Applications directory listing.
		/// Returns deduplicated list of canonical Xcode.app paths.
		/// </summary>
		List<string> FindXcodeApps ()
		{
			var paths = new List<string> ();

			// 1. Try Spotlight (mdfind) — fastest way to find all Xcode bundles
			if (File.Exists (MdfindPath)) {
				try {
					var (exitCode, stdout, _) = ProcessUtils.Exec (MdfindPath, "kMDItemCFBundleIdentifier == 'com.apple.dt.Xcode'");
					if (exitCode == 0) {
						foreach (var rawLine in stdout.Split ('\n')) {
							var line = rawLine.Trim ();
							if (line.Length > 0 && Directory.Exists (line))
								paths.Add (line);
						}
					}
				} catch (System.ComponentModel.Win32Exception ex) {
					log.LogInfo ("Could not run mdfind: {0}", ex.Message);
				}
			}

			// 2. Also scan /Applications for Xcode*.app bundles mdfind might miss
			if (Directory.Exists (ApplicationsDir)) {
				try {
					foreach (var dir in Directory.GetDirectories (ApplicationsDir, "Xcode*.app")) {
						if (!paths.Contains (dir))
							paths.Add (dir);
					}
				} catch (UnauthorizedAccessException ex) {
					log.LogInfo ("Could not scan /Applications: {0}", ex.Message);
				}
			}

			return paths;
		}

		/// <summary>
		/// Reads Xcode metadata from a .app bundle path.
		/// Returns null if the path is not a valid Xcode installation.
		/// </summary>
		XcodeInfo? ReadXcodeInfo (string appPath)
		{
			var versionPlistPath = Path.Combine (appPath, "Contents", "version.plist");
			if (!File.Exists (versionPlistPath)) {
				log.LogInfo ("Skipping '{0}': no Contents/version.plist.", appPath);
				return null;
			}

			try {
				var versionPlist = PDictionary.FromFile (versionPlistPath);
				if (versionPlist is null) {
					log.LogInfo ("Skipping '{0}': could not parse version.plist.", appPath);
					return null;
				}

				var versionStr = versionPlist.GetCFBundleShortVersionString ();
				if (!Version.TryParse (versionStr, out var version)) {
					log.LogInfo ("Skipping '{0}': could not parse version '{1}'.", appPath, versionStr);
					return null;
				}

				var info = new XcodeInfo {
					Path = appPath,
					Version = version,
					Build = versionPlist.GetCFBundleVersion () ?? "",
					IsSymlink = PathUtils.IsSymlinkOrHasParentSymlink (appPath),
				};

				// Read DTXcode from Info.plist if available
				var infoPlistPath = Path.Combine (appPath, "Contents", "Info.plist");
				if (File.Exists (infoPlistPath)) {
					try {
						var infoPlist = PDictionary.FromFile (infoPlistPath);
						if (infoPlist is not null && infoPlist.TryGetValue<PString> ("DTXcode", out var dtXcode))
							info.DTXcode = dtXcode.Value;
					} catch (Exception ex) {
						log.LogInfo ("Could not read Info.plist for '{0}': {1}", appPath, ex.Message);
					}
				}

				return info;
			} catch (Exception ex) {
				log.LogInfo ("Could not read Xcode info from '{0}': {1}", appPath, ex.Message);
				return null;
			}
		}

		/// <summary>
		/// Strips /Contents/Developer suffix from an Xcode developer path to get the .app path.
		/// Returns null if the path is empty or does not point to an existing directory.
		/// </summary>
		public static string? CanonicalizeXcodePath (string? path)
		{
			if (string.IsNullOrEmpty (path))
				return null;

			path = path!.TrimEnd ('/');

			if (path.EndsWith ("/Contents/Developer", StringComparison.Ordinal))
				path = path.Substring (0, path.Length - "/Contents/Developer".Length);

			if (!Directory.Exists (path))
				return null;

			return path;
		}

		/// <summary>
		/// Parses the output of mdfind into a list of paths.
		/// Exported for testing.
		/// </summary>
		public static List<string> ParseMdfindOutput (string output)
		{
			var results = new List<string> ();
			if (string.IsNullOrEmpty (output))
				return results;

			foreach (var rawLine in output.Split ('\n')) {
				var line = rawLine.Trim ();
				if (line.Length > 0)
					results.Add (line);
			}

			return results;
		}
	}
}
