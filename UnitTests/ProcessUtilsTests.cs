// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using NUnit.Framework;

using Xamarin.MacDev;

namespace UnitTests {

	[TestFixture]
	public class ProcessUtilsTests {

		[Test]
		public async Task RunAsync_ReturnsStdout ()
		{
			var result = await ProcessUtils.RunAsync ("/bin/echo", "hello world");
			Assert.That (result.Trim (), Is.EqualTo ("hello world"));
		}

		[Test]
		public void RunAsync_ThrowsOnNonZeroExitCode ()
		{
			Assert.ThrowsAsync<InvalidOperationException> (async () => {
				await ProcessUtils.RunAsync ("/bin/sh", "-c \"exit 42\"");
			});
		}

		[Test]
		public async Task TryRunAsync_ReturnsStdoutOnSuccess ()
		{
			var result = await ProcessUtils.TryRunAsync ("/bin/echo", "hello");
			Assert.That (result, Is.EqualTo ("hello"));
		}

		[Test]
		public async Task TryRunAsync_ReturnsNullOnFailure ()
		{
			var result = await ProcessUtils.TryRunAsync ("/bin/sh", "-c \"exit 1\"");
			Assert.That (result, Is.Null);
		}

		[Test]
		public async Task TryRunAsync_ReturnsNullForMissingExecutable ()
		{
			var result = await ProcessUtils.TryRunAsync ("/nonexistent/binary", "");
			Assert.That (result, Is.Null);
		}

		[Test]
		public async Task StartProcess_CapturesStdoutAndStderr ()
		{
			var stdout = new StringWriter ();
			var stderr = new StringWriter ();
			var psi = new System.Diagnostics.ProcessStartInfo ("/bin/sh", "-c \"echo out; echo err >&2\"") {
				CreateNoWindow = true,
			};

			var exitCode = await ProcessUtils.StartProcess (psi, stdout, stderr);
			Assert.That (exitCode, Is.EqualTo (0));
			Assert.That (stdout.ToString ().Trim (), Is.EqualTo ("out"));
			Assert.That (stderr.ToString ().Trim (), Is.EqualTo ("err"));
		}

		[Test]
		public void StartProcess_RespectsCanellation ()
		{
			using (var cts = new CancellationTokenSource ()) {
				cts.Cancel ();

				Assert.ThrowsAsync<OperationCanceledException> (async () => {
					await ProcessUtils.RunAsync ("/bin/sleep", "60", cts.Token);
				});
			}
		}

		[Test]
		public void Exec_ReturnsExitCodeAndOutput ()
		{
			var (exitCode, stdout, stderr) = ProcessUtils.Exec ("/bin/echo", "sync test");
			Assert.That (exitCode, Is.EqualTo (0));
			Assert.That (stdout.Trim (), Is.EqualTo ("sync test"));
			Assert.That (stderr, Is.Empty);
		}

		[Test]
		public void Exec_ReturnsNonZeroExitCode ()
		{
			var (exitCode, _, _) = ProcessUtils.Exec ("/bin/sh", "-c \"exit 7\"");
			Assert.That (exitCode, Is.EqualTo (7));
		}
	}
}
