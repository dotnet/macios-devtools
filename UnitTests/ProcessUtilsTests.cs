// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Xamarin.MacDev;

namespace UnitTests {

	[TestFixture]
	public class ProcessUtilsTests {

		static bool IsWindows => RuntimeInformation.IsOSPlatform (OSPlatform.Windows);
		static string ShellExe => IsWindows ? "cmd.exe" : "/bin/sh";

		static string EchoArgs (string text)
		{
			return IsWindows ? $"/c echo {text}" : $"-c \"echo {text}\"";
		}

		static string ExitArgs (int code)
		{
			return IsWindows ? $"/c exit {code}" : $"-c \"exit {code}\"";
		}

		static string StdoutAndStderrArgs ()
		{
			return IsWindows ? "/c echo out& echo err >&2" : "-c \"echo out; echo err >&2\"";
		}

		[Test]
		public async Task RunAsync_ReturnsStdout ()
		{
			var result = await ProcessUtils.RunAsync (ShellExe, EchoArgs ("hello world"));
			Assert.That (result.Trim (), Is.EqualTo ("hello world"));
		}

		[Test]
		public void RunAsync_ThrowsOnNonZeroExitCode ()
		{
			Assert.ThrowsAsync<InvalidOperationException> (async () => {
				await ProcessUtils.RunAsync (ShellExe, ExitArgs (42));
			});
		}

		[Test]
		public async Task TryRunAsync_ReturnsStdoutOnSuccess ()
		{
			var result = await ProcessUtils.TryRunAsync (ShellExe, EchoArgs ("hello"));
			Assert.That (result?.Trim (), Is.EqualTo ("hello"));
		}

		[Test]
		public async Task TryRunAsync_ReturnsNullOnFailure ()
		{
			var result = await ProcessUtils.TryRunAsync (ShellExe, ExitArgs (1));
			Assert.That (result, Is.Null);
		}

		[Test]
		public async Task TryRunAsync_ReturnsNullForMissingExecutable ()
		{
			var missingPath = IsWindows ? @"C:\nonexistent\binary.exe" : "/nonexistent/binary";
			var result = await ProcessUtils.TryRunAsync (missingPath, "");
			Assert.That (result, Is.Null);
		}

		[Test]
		public async Task StartProcess_CapturesStdoutAndStderr ()
		{
			using (var stdout = new StringWriter ())
			using (var stderr = new StringWriter ()) {
				var psi = new System.Diagnostics.ProcessStartInfo (ShellExe, StdoutAndStderrArgs ()) {
					CreateNoWindow = true,
				};

				var exitCode = await ProcessUtils.StartProcess (psi, stdout, stderr);
				Assert.That (exitCode, Is.EqualTo (0));
				Assert.That (stdout.ToString ().Trim (), Is.EqualTo ("out"));
				Assert.That (stderr.ToString ().Trim (), Is.EqualTo ("err"));
			}
		}

		[Test]
		public void StartProcess_RespectsCancellation ()
		{
			using (var cts = new CancellationTokenSource ()) {
				cts.Cancel ();

				var sleepExe = IsWindows ? "timeout" : "/bin/sleep";
				var sleepArgs = IsWindows ? "/t 60" : "60";
				Assert.ThrowsAsync<OperationCanceledException> (async () => {
					await ProcessUtils.RunAsync (sleepExe, sleepArgs, cts.Token);
				});
			}
		}

		[Test]
		public void Exec_ReturnsExitCodeAndOutput ()
		{
			var (exitCode, stdout, stderr) = ProcessUtils.Exec (ShellExe, EchoArgs ("sync test"));
			Assert.That (exitCode, Is.EqualTo (0));
			Assert.That (stdout.Trim (), Is.EqualTo ("sync test"));
			Assert.That (stderr, Is.Empty);
		}

		[Test]
		public void Exec_ReturnsNonZeroExitCode ()
		{
			var (exitCode, _, _) = ProcessUtils.Exec (ShellExe, ExitArgs (7));
			Assert.That (exitCode, Is.EqualTo (7));
		}
	}
}
