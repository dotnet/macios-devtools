// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Xamarin.MacDev {

	/// <summary>
	/// Low-level wrapper for running <c>xcrun devicectl</c> subcommands.
	/// The asynchronous, cancellable counterpart of <see cref="SimCtl"/>.
	/// Logs all subprocess executions and their results through <see cref="ICustomLogger"/>;
	/// nothing is written to stdout.
	/// </summary>
	internal class DeviceCtl {

		readonly ICustomLogger log;
		readonly string xcrunPath;

		public DeviceCtl (ICustomLogger log, string? xcrunPath = null)
		{
			this.log = log ?? throw new ArgumentNullException (nameof (log));
			this.xcrunPath = xcrunPath ?? SimCtl.XcrunPath;
		}

		/// <summary>
		/// Runs <c>xcrun devicectl list devices</c>. devicectl writes its machine-readable JSON to a temporary
		/// file (<c>--json-output</c>) because its stdout is meant for humans and is not stable across releases.
		/// The process is killed if <paramref name="cancellationToken"/> is cancelled (the net10.0 build also kills
		/// any process it started), in which case an <see cref="OperationCanceledException"/> is thrown.
		/// </summary>
		public async Task<DeviceCtlRunResult> ListDevicesAsync (CancellationToken cancellationToken)
		{
			if (!File.Exists (xcrunPath)) {
				log.LogInfo ("xcrun not found at '{0}'.", xcrunPath);
				return DeviceCtlRunResult.NotFound ($"xcrun was not found at '{xcrunPath}'.");
			}

			var jsonPath = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName () + ".json");
			try {
				var arguments = new [] { "devicectl", "list", "devices", "--json-output", jsonPath };
				var psi = ProcessUtils.CreateProcessStartInfo (xcrunPath, arguments);
				log.LogInfo ("Running: {0} {1}", xcrunPath, string.Join (" ", arguments));

				using (var stdout = new StringWriter ())
				using (var stderr = new StringWriter ()) {
					int exitCode;
					try {
						exitCode = await ProcessUtils.StartProcess (psi, stdout, stderr, cancellationToken).ConfigureAwait (false);
					} catch (Win32Exception ex) {
						log.LogInfo ("Could not start '{0}': {1}", xcrunPath, ex.Message);
						return DeviceCtlRunResult.NotFound ($"xcrun could not be started: {ex.Message}");
					}

					log.LogInfo ("devicectl exited with code {0}.", exitCode);
					return DeviceCtlRunResult.Completed (exitCode, stdout.ToString (), stderr.ToString (), ReadJson (jsonPath));
				}
			} finally {
				DeleteFile (jsonPath);
			}
		}

		string? ReadJson (string path)
		{
			try {
				return File.Exists (path) ? File.ReadAllText (path, Encoding.UTF8) : null;
			} catch (IOException ex) {
				log.LogInfo ("Failed to read devicectl JSON output '{0}': {1}", path, ex.Message);
			} catch (UnauthorizedAccessException ex) {
				log.LogInfo ("Failed to read devicectl JSON output '{0}': {1}", path, ex.Message);
			}
			return null;
		}

		void DeleteFile (string path)
		{
			try {
				if (File.Exists (path))
					File.Delete (path);
			} catch (IOException ex) {
				log.LogInfo ("Failed to delete temporary devicectl JSON file '{0}': {1}", path, ex.Message);
			} catch (UnauthorizedAccessException ex) {
				log.LogInfo ("Failed to delete temporary devicectl JSON file '{0}': {1}", path, ex.Message);
			}
		}
	}
}
