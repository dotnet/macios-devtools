// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;

#nullable enable

namespace Xamarin.MacDev;

/// <summary>
/// Low-level wrapper for running <c>xcrun simctl</c> subcommands.
/// Shared by <see cref="SimulatorService"/> and <c>RuntimeService</c>
/// to avoid duplicated subprocess execution logic.
/// Logs all subprocess executions and their results.
/// </summary>
public class SimCtl {

	static readonly string XcrunPath = "/usr/bin/xcrun";

	readonly ICustomLogger log;

	public SimCtl (ICustomLogger log)
	{
		this.log = log ?? throw new ArgumentNullException (nameof (log));
	}

	/// <summary>
	/// Runs <c>xcrun simctl {args}</c> and returns stdout, or null on failure.
	/// All subprocess executions and errors are logged.
	/// </summary>
	public string? Run (params string [] args)
	{
		if (!File.Exists (XcrunPath)) {
			log.LogInfo ("xcrun not found at '{0}'.", XcrunPath);
			return null;
		}

		var fullArgs = new string [args.Length + 1];
		fullArgs [0] = "simctl";
		Array.Copy (args, 0, fullArgs, 1, args.Length);

		log.LogInfo ("Executing: {0} {1}", XcrunPath, string.Join (" ", fullArgs));

		try {
			var (exitCode, stdout, stderr) = ProcessUtils.Exec (XcrunPath, fullArgs);
			if (exitCode != 0) {
				log.LogInfo ("simctl {0} failed (exit {1}): {2}", args.Length > 0 ? args [0] : "", exitCode, stderr.Trim ());
				return null;
			}
			return stdout;
		} catch (System.ComponentModel.Win32Exception ex) {
			log.LogInfo ("Could not run xcrun simctl: {0}", ex.Message);
			return null;
		} catch (InvalidOperationException ex) {
			log.LogInfo ("Could not run xcrun simctl: {0}", ex.Message);
			return null;
		}
	}
}
