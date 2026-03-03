// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using Xamarin.MacDev.Models;

#nullable enable

namespace Xamarin.MacDev;

/// <summary>
/// Detects and reports on the Xcode Command Line Tools installation.
/// Follows the same instance-based, ICustomLogger pattern as XcodeLocator.
/// </summary>
public class CommandLineTools {

	static readonly string XcodeSelectPath = "/usr/bin/xcode-select";
	static readonly string PkgutilPath = "/usr/bin/pkgutil";
	static readonly string CltPkgId = "com.apple.pkg.CLTools_Executables";
	static readonly string DefaultCltPath = "/Library/Developer/CommandLineTools";

	readonly ICustomLogger log;

	public CommandLineTools (ICustomLogger log)
	{
		this.log = log ?? throw new ArgumentNullException (nameof (log));
	}

	/// <summary>
	/// Checks whether the Xcode Command Line Tools are installed and returns their info.
	/// </summary>
	public CommandLineToolsInfo Check ()
	{
		var info = new CommandLineToolsInfo ();

		var cltPath = GetCommandLineToolsPath ();
		if (cltPath is null) {
			log.LogInfo ("Command Line Tools are not installed (path not found).");
			return info;
		}

		info.Path = cltPath;

		var version = GetVersionFromPkgutil ();
		if (version is not null) {
			info.Version = version;
			info.IsInstalled = true;
			log.LogInfo ("Command Line Tools {0} found at '{1}'.", version, cltPath);
		} else {
			info.IsInstalled = Directory.Exists (Path.Combine (cltPath, "usr", "bin"));
			if (info.IsInstalled)
				log.LogInfo ("Command Line Tools found at '{0}' (version unknown).", cltPath);
			else
				log.LogInfo ("Command Line Tools directory exists at '{0}' but appears incomplete.", cltPath);
		}

		return info;
	}

	/// <summary>
	/// Returns the Command Line Tools install path, or null if not found.
	/// Uses xcode-select -p first, falls back to the well-known default path.
	/// </summary>
	string? GetCommandLineToolsPath ()
	{
		if (File.Exists (XcodeSelectPath)) {
			try {
				var (exitCode, stdout, _) = ProcessUtils.Exec (XcodeSelectPath, "--print-path");
				if (exitCode == 0) {
					var path = stdout.Trim ();
					if (path.Contains ("CommandLineTools") && Directory.Exists (path))
						return path;
				}
			} catch (System.ComponentModel.Win32Exception ex) {
				log.LogInfo ("Could not run xcode-select: {0}", ex.Message);
			} catch (InvalidOperationException ex) {
				log.LogInfo ("Could not run xcode-select: {0}", ex.Message);
			}
		}

		if (Directory.Exists (DefaultCltPath))
			return DefaultCltPath;

		return null;
	}

	/// <summary>
	/// Queries pkgutil for the CLT package version.
	/// Returns the version string or null if not installed.
	/// </summary>
	internal string? GetVersionFromPkgutil ()
	{
		if (!File.Exists (PkgutilPath))
			return null;

		try {
			var (exitCode, stdout, _) = ProcessUtils.Exec (PkgutilPath, "--pkg-info", CltPkgId);
			if (exitCode != 0)
				return null;

			return ParsePkgutilVersion (stdout);
		} catch (System.ComponentModel.Win32Exception ex) {
			log.LogInfo ("Could not run pkgutil: {0}", ex.Message);
			return null;
		} catch (InvalidOperationException ex) {
			log.LogInfo ("Could not run pkgutil: {0}", ex.Message);
			return null;
		}
	}

	/// <summary>
	/// Parses the "version: ..." line from pkgutil --pkg-info output.
	/// </summary>
	public static string? ParsePkgutilVersion (string pkgutilOutput)
	{
		if (string.IsNullOrEmpty (pkgutilOutput))
			return null;

		foreach (var rawLine in pkgutilOutput.Split ('\n')) {
			var line = rawLine.Trim ();
			if (line.StartsWith ("version:", StringComparison.Ordinal)) {
				var version = line.Substring ("version:".Length).Trim ();
				return string.IsNullOrEmpty (version) ? null : version;
			}
		}

		return null;
	}
}
