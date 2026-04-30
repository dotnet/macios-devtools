// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

#nullable enable

namespace Xamarin.MacDev;

/// <summary>
/// Wraps <c>xcrun simctl io</c> operations for taking screenshots and recording video
/// from a simulator.
/// </summary>
public class SimulatorScreenCapture {

	readonly ICustomLogger log;
	readonly SimCtl simctl;

	internal SimulatorScreenCapture (ICustomLogger log, SimCtl simctl)
	{
		this.log = log;
		this.simctl = simctl;
	}

	/// <summary>
	/// Takes a screenshot of the simulator and saves it to <paramref name="outputPath"/>.
	/// Wraps <c>xcrun simctl io &lt;udid&gt; screenshot [--type=png|jpeg|tiff|bmp] &lt;path&gt;</c>.
	/// </summary>
	public bool Screenshot (string udidOrName, string outputPath, ScreenshotFormat format = ScreenshotFormat.Png)
	{
		if (string.IsNullOrWhiteSpace (udidOrName))
			throw new ArgumentException ("Simulator UDID or name must not be null or empty.", nameof (udidOrName));
		if (string.IsNullOrWhiteSpace (outputPath))
			throw new ArgumentException ("Output path must not be null or empty.", nameof (outputPath));

		var formatArg = "--type=" + ToSimctlFormatName (format);
		var result = simctl.Run ("io", udidOrName, "screenshot", formatArg, outputPath);
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl io screenshot '{0}' to '{1}' succeeded.", udidOrName, outputPath);
		return success;
	}

	/// <summary>
	/// Starts a video recording of the simulator. Dispose the returned handle to stop recording.
	/// Wraps <c>xcrun simctl io &lt;udid&gt; recordVideo [options] &lt;path&gt;</c>.
	/// </summary>
	/// <returns>
	/// A disposable handle; call <see cref="IDisposable.Dispose"/> to stop the recording.
	/// Returns null if xcrun cannot be started.
	/// </returns>
	public IDisposable? StartRecording (string udidOrName, string outputPath, RecordingOptions? options = null)
	{
		if (string.IsNullOrWhiteSpace (udidOrName))
			throw new ArgumentException ("Simulator UDID or name must not be null or empty.", nameof (udidOrName));
		if (string.IsNullOrWhiteSpace (outputPath))
			throw new ArgumentException ("Output path must not be null or empty.", nameof (outputPath));

		if (!File.Exists (SimCtl.XcrunPath)) {
			log.LogInfo ("xcrun not found at '{0}'.", SimCtl.XcrunPath);
			return null;
		}

		var simctlArgs = BuildRecordArgs (udidOrName, outputPath, options);

		// Build the full argument list: "simctl io <udid> recordVideo [opts] <file>"
		var allArgs = new string [simctlArgs.Length + 1];
		allArgs [0] = "simctl";
		Array.Copy (simctlArgs, 0, allArgs, 1, simctlArgs.Length);

		log.LogInfo ("Executing: {0} {1}", SimCtl.XcrunPath, string.Join (" ", allArgs));

		var psi = new ProcessStartInfo (SimCtl.XcrunPath) {
			CreateNoWindow = true,
			UseShellExecute = false,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};

#if NETSTANDARD2_0
		psi.Arguments = QuoteArguments (allArgs);
#else
		foreach (var arg in allArgs)
			psi.ArgumentList.Add (arg);
#endif

		try {
			var process = new Process { StartInfo = psi };
			process.Start ();
			log.LogInfo ("simctl io recordVideo started for '{0}'.", udidOrName);
			return new VideoRecordingSession (process, log);
		} catch (System.ComponentModel.Win32Exception ex) {
			log.LogInfo ("Could not start xcrun simctl io recordVideo: {0}", ex.Message);
			return null;
		} catch (InvalidOperationException ex) {
			log.LogInfo ("Could not start xcrun simctl io recordVideo: {0}", ex.Message);
			return null;
		}
	}

	static string [] BuildRecordArgs (string udidOrName, string outputPath, RecordingOptions? options)
	{
		var args = new List<string> { "io", udidOrName, "recordVideo" };

		if (options?.Format is { } fmt)
			args.Add ("--type=" + ToSimctlVideoFormatName (fmt));

		if (options?.Force == true)
			args.Add ("--force");

		args.Add (outputPath);
		return args.ToArray ();
	}

	public static string ToSimctlFormatName (ScreenshotFormat format)
	{
		return format switch {
			ScreenshotFormat.Png => "png",
			ScreenshotFormat.Jpeg => "jpeg",
			ScreenshotFormat.Tiff => "tiff",
			ScreenshotFormat.Bmp => "bmp",
			_ => throw new ArgumentOutOfRangeException (nameof (format), format, null),
		};
	}

	public static string ToSimctlVideoFormatName (VideoRecordingFormat format)
	{
		return format switch {
			VideoRecordingFormat.Mp4 => "mp4",
			VideoRecordingFormat.H264 => "h264",
			VideoRecordingFormat.Fmp4 => "fmp4",
			VideoRecordingFormat.Gif => "gif",
			_ => throw new ArgumentOutOfRangeException (nameof (format), format, null),
		};
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

	/// <summary>
	/// Disposable handle that terminates a running <c>simctl io recordVideo</c> process
	/// when disposed.
	/// </summary>
	sealed class VideoRecordingSession : IDisposable {

		readonly Process process;
		readonly ICustomLogger log;
		bool disposed;

		public VideoRecordingSession (Process process, ICustomLogger log)
		{
			this.process = process;
			this.log = log;
		}

		public void Dispose ()
		{
			if (disposed)
				return;

			disposed = true;

			try {
				if (!process.HasExited) {
					process.Kill ();
					process.WaitForExit (5000);
					log.LogInfo ("simctl io recordVideo process stopped.");
				}
			} catch (InvalidOperationException) {
				// Process already exited
			} catch (System.ComponentModel.Win32Exception) {
				// Cannot kill process (e.g. access denied)
			} finally {
				process.Dispose ();
			}
		}
	}
}
