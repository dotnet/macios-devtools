// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Xamarin.MacDev;

#nullable enable

namespace tests;

/// <summary>Helpers for tests that start real child processes and have to prove they are cleaned up (Unix only).</summary>
static class ProcessTestHelper {

	/// <summary>Reads a process id that a script wrote to <paramref name="path"/>, or null if it is missing or incomplete.</summary>
	public static int? TryReadPid (string path)
	{
		try {
			if (File.Exists (path) && int.TryParse (File.ReadAllText (path).Trim (), NumberStyles.None, CultureInfo.InvariantCulture, out var pid))
				return pid;
		} catch (IOException) {
			// The script may still be writing the file.
		}
		return null;
	}

	/// <summary>Waits until a script has written a process id to <paramref name="path"/>.</summary>
	public static async Task<int> WaitForPidAsync (string path, TimeSpan timeout)
	{
		var stopwatch = Stopwatch.StartNew ();
		while (true) {
			var pid = TryReadPid (path);
			if (pid.HasValue)
				return pid.Value;
			if (stopwatch.Elapsed > timeout)
				throw new TimeoutException ($"No process id was written to '{path}' within {timeout}.");
			await Task.Delay (25).ConfigureAwait (false);
		}
	}

	/// <summary>True if a process with the given id exists and is still running. A zombie counts as dead.</summary>
	public static bool IsAlive (int pid)
	{
		var (exitCode, stdout, _) = ProcessUtils.Exec ("ps", "-o", "stat=", "-p", pid.ToString (CultureInfo.InvariantCulture));
		var state = stdout.Trim ();
		return exitCode == 0 && state.Length > 0 && state [0] != 'Z';
	}

	/// <summary>Waits until the process is gone. Returns false if it is still running after <paramref name="timeout"/>.</summary>
	public static async Task<bool> WaitUntilGoneAsync (int pid, TimeSpan timeout)
	{
		var stopwatch = Stopwatch.StartNew ();
		while (IsAlive (pid)) {
			if (stopwatch.Elapsed > timeout)
				return false;
			await Task.Delay (50).ConfigureAwait (false);
		}
		return true;
	}

	/// <summary>Kills the process if it is still running, so that a test that fails early does not leave it behind.</summary>
	public static void Kill (int pid)
	{
		if (IsAlive (pid))
			ProcessUtils.Exec ("kill", "-9", pid.ToString (CultureInfo.InvariantCulture));
	}

	/// <summary>Makes a file executable.</summary>
	public static void MakeExecutable (string path)
	{
		var (exitCode, _, stderr) = ProcessUtils.Exec ("chmod", "+x", path);
		if (exitCode != 0)
			throw new InvalidOperationException ($"chmod +x '{path}' failed with exit code {exitCode}: {stderr}");
	}
}
