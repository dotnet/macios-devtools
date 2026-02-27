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

			// Provide sinks when redirection is on but no writer was supplied
			if (psi.RedirectStandardOutput && stdout is null)
				stdout = TextWriter.Null;
			if (psi.RedirectStandardError && stderr is null)
				stderr = TextWriter.Null;

			var process = new Process {
				StartInfo = psi,
				EnableRaisingEvents = true,
			};

			Task output = Task.FromResult (true);
			Task error = Task.FromResult (true);
			var exitDone = new TaskCompletionSource<bool> ();
			process.Exited += (o, e) => exitDone.TrySetResult (true);

			using (process) {
				process.Start ();

				// Guard against race where process exits before Exited handler fires
				if (process.HasExited)
					exitDone.TrySetResult (true);

				using (cancellationToken.Register (() => KillProcess (process))) {
					if (psi.RedirectStandardOutput)
						output = ReadStreamAsync (process.StandardOutput, TextWriter.Synchronized (stdout!));

					if (psi.RedirectStandardError)
						error = ReadStreamAsync (process.StandardError, TextWriter.Synchronized (stderr!));

					await Task.WhenAll (output, error, exitDone.Task).ConfigureAwait (false);
				}

				cancellationToken.ThrowIfCancellationRequested ();
				return process.ExitCode;
			}
		}

		/// <summary>
		/// Runs an executable and returns its stdout as a string.
		/// Throws <see cref="InvalidOperationException"/> if the process returns a non-zero exit code.
		/// </summary>
		public static async Task<string> RunAsync (string executable, CancellationToken cancellationToken, params string [] arguments)
		{
			using (var stdout = new StringWriter ())
			using (var stderr = new StringWriter ()) {
				var psi = CreateProcessStartInfo (executable, arguments);

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
		}

		/// <summary>
		/// Runs an executable and returns its stdout as a string.
		/// Throws <see cref="InvalidOperationException"/> if the process returns a non-zero exit code.
		/// </summary>
		public static Task<string> RunAsync (string executable, params string [] arguments)
		{
			return RunAsync (executable, CancellationToken.None, arguments);
		}

		/// <summary>
		/// Runs an executable and returns its stdout as a trimmed string, or null if the process fails.
		/// Does not throw on non-zero exit codes.
		/// </summary>
		public static async Task<string?> TryRunAsync (string executable, CancellationToken cancellationToken, params string [] arguments)
		{
			using (var stdout = new StringWriter ())
			using (var stderr = new StringWriter ()) {
				var psi = CreateProcessStartInfo (executable, arguments);

				try {
					var exitCode = await StartProcess (psi, stdout, stderr, cancellationToken).ConfigureAwait (false);
					if (exitCode != 0)
						return null;

					return stdout.ToString ().Trim ();
				} catch (OperationCanceledException) {
					throw;
				} catch (System.ComponentModel.Win32Exception) {
					return null;
				} catch (InvalidOperationException) {
					return null;
				}
			}
		}

		/// <summary>
		/// Runs an executable and returns its stdout as a trimmed string, or null if the process fails.
		/// Does not throw on non-zero exit codes.
		/// </summary>
		public static Task<string?> TryRunAsync (string executable, params string [] arguments)
		{
			return TryRunAsync (executable, CancellationToken.None, arguments);
		}

		/// <summary>
		/// Synchronous convenience wrapper.
		/// Runs an executable and returns stdout/stderr and the exit code.
		/// </summary>
		public static (int exitCode, string stdout, string stderr) Exec (string executable, params string [] arguments)
		{
			var psi = CreateProcessStartInfo (executable, arguments);

			using (var stdout = new StringWriter ())
			using (var stderr = new StringWriter ()) {
				var exitCode = StartProcess (psi, stdout, stderr).GetAwaiter ().GetResult ();

				return (exitCode, stdout.ToString (), stderr.ToString ());
			}
		}

		static ProcessStartInfo CreateProcessStartInfo (string executable, string [] arguments)
		{
			var psi = new ProcessStartInfo (executable) {
				CreateNoWindow = true,
			};

#if NETSTANDARD2_0
			psi.Arguments = QuoteArguments (arguments);
#else
			foreach (var arg in arguments)
				psi.ArgumentList.Add (arg);
#endif

			return psi;
		}

#if NETSTANDARD2_0
		static string QuoteArguments (string [] arguments)
		{
			if (arguments.Length == 0)
				return string.Empty;

			var sb = new System.Text.StringBuilder ();
			for (int i = 0; i < arguments.Length; i++) {
				if (i > 0)
					sb.Append (' ');

				var arg = arguments [i];
				if (arg.Length > 0 && arg.IndexOfAny (new [] { ' ', '\t', '"' }) < 0) {
					sb.Append (arg);
				} else {
					sb.Append ('"');
					sb.Append (arg.Replace ("\"", "\\\""));
					sb.Append ('"');
				}
			}
			return sb.ToString ();
		}
#endif

		static void KillProcess (Process p)
		{
			try {
				p.Kill ();
			} catch (InvalidOperationException) {
				// Process may have already exited
			} catch (System.ComponentModel.Win32Exception) {
				// Process cannot be terminated (e.g. access denied)
			}
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
