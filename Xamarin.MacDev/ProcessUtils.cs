// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Xamarin.MacDev {

	/// <summary>
	/// Static helper for running external processes with async stdout/stderr capture
	/// and cancellation support. Inspired by dotnet/android-tools ProcessUtils.
	/// </summary>
	public static class ProcessUtils {

		/// <summary>
		/// Starts a process and asynchronously streams its stdout/stderr to the provided writers.
		/// Returns the process exit code.
		/// </summary>
		public static async Task<int> StartProcess (ProcessStartInfo psi, TextWriter? stdout, TextWriter? stderr, CancellationToken cancellationToken = default)
		{
			cancellationToken.ThrowIfCancellationRequested ();
			psi.UseShellExecute = false;
			psi.RedirectStandardOutput |= stdout is not null;
			psi.RedirectStandardError |= stderr is not null;

			var process = new Process {
				StartInfo = psi,
				EnableRaisingEvents = true,
			};

			Task output = Task.FromResult (true);
			Task error = Task.FromResult (true);
			Task exit = WaitForExitAsync (process);

			using (process) {
				process.Start ();

				using (cancellationToken.Register (() => KillProcess (process))) {
					if (psi.RedirectStandardOutput)
						output = ReadStreamAsync (process.StandardOutput, TextWriter.Synchronized (stdout!));

					if (psi.RedirectStandardError)
						error = ReadStreamAsync (process.StandardError, TextWriter.Synchronized (stderr!));

					await Task.WhenAll (output, error, exit).ConfigureAwait (false);
				}

				cancellationToken.ThrowIfCancellationRequested ();
				return process.ExitCode;
			}
		}

		/// <summary>
		/// Runs an executable and returns its stdout as a string.
		/// Throws <see cref="InvalidOperationException"/> if the process returns a non-zero exit code.
		/// </summary>
		public static async Task<string> RunAsync (string executable, string arguments, CancellationToken cancellationToken = default)
		{
			using var stdout = new StringWriter ();
			using var stderr = new StringWriter ();

			var psi = new ProcessStartInfo (executable, arguments) {
				CreateNoWindow = true,
			};

			var exitCode = await StartProcess (psi, stdout, stderr, cancellationToken).ConfigureAwait (false);

			if (exitCode != 0) {
				var errorOutput = stderr.ToString ().Trim ();
				var stdoutOutput = stdout.ToString ().Trim ();
				var message = !string.IsNullOrEmpty (errorOutput) ? errorOutput : stdoutOutput;
				if (string.IsNullOrEmpty (message))
					message = $"'{Path.GetFileName (executable)}' returned exit code {exitCode}";

				throw new InvalidOperationException (message);
			}

			return stdout.ToString ();
		}

		/// <summary>
		/// Runs an executable and returns its stdout as a trimmed string, or null if the process fails.
		/// Does not throw on non-zero exit codes.
		/// </summary>
		public static async Task<string?> TryRunAsync (string executable, string arguments, CancellationToken cancellationToken = default)
		{
			using var stdout = new StringWriter ();
			using var stderr = new StringWriter ();

			var psi = new ProcessStartInfo (executable, arguments) {
				CreateNoWindow = true,
			};

			try {
				var exitCode = await StartProcess (psi, stdout, stderr, cancellationToken).ConfigureAwait (false);
				if (exitCode != 0)
					return null;

				return stdout.ToString ().Trim ();
			} catch (OperationCanceledException) {
				throw;
			} catch {
				return null;
			}
		}

		/// <summary>
		/// Synchronous convenience wrapper around <see cref="StartProcess"/>.
		/// Runs an executable and returns stdout/stderr and the exit code.
		/// </summary>
		public static (int exitCode, string stdout, string stderr) Exec (string executable, string arguments)
		{
			var psi = new ProcessStartInfo (executable, arguments) {
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				CreateNoWindow = true,
			};

			using var process = new Process { StartInfo = psi };
			process.Start ();
			var stdout = process.StandardOutput.ReadToEnd ();
			var stderr = process.StandardError.ReadToEnd ();
			process.WaitForExit ();

			return (process.ExitCode, stdout, stderr);
		}

		static void KillProcess (Process p)
		{
			try {
				p.Kill ();
			} catch (InvalidOperationException) {
				// Process may have already exited
			}
		}

		static Task WaitForExitAsync (Process process)
		{
			var exitDone = new TaskCompletionSource<bool> ();
			process.Exited += (o, e) => exitDone.TrySetResult (true);
			return exitDone.Task;
		}

		static async Task ReadStreamAsync (StreamReader stream, TextWriter destination)
		{
			int read;
			var buffer = new char [4096];
			while ((read = await stream.ReadAsync (buffer, 0, buffer.Length).ConfigureAwait (false)) > 0)
				destination.Write (buffer, 0, read);
		}
	}
}
