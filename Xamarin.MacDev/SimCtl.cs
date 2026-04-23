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
		return RunCore (null, args);
	}

	/// <summary>
	/// Runs <c>xcrun simctl {args}</c>, writes stdout to <paramref name="outputPath"/>,
	/// and returns the output. Returns null on failure.
	/// Use this instead of <see cref="Run"/> when the output is structured (e.g. JSON)
	/// and you need to isolate it from any stdout noise.
	/// </summary>
	public string? RunToFile (string outputPath, params string [] args)
	{
		if (string.IsNullOrWhiteSpace (outputPath))
			throw new ArgumentException ("Output path must not be null or empty.", nameof (outputPath));

		return RunCore (outputPath, args);
	}

	string? RunCore (string? outputPath, string [] args)
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

			if (outputPath is not null) {
				try {
					File.WriteAllText (outputPath, stdout);
				} catch (IOException ex) {
					log.LogWarning ("Failed to write output to '{0}': {1}", outputPath, ex.Message);
				} catch (UnauthorizedAccessException ex) {
					log.LogWarning ("Failed to write output to '{0}': {1}", outputPath, ex.Message);
				}
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
