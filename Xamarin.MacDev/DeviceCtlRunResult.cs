// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Xamarin.MacDev {

	/// <summary>
	/// The raw outcome of running <c>xcrun devicectl list devices</c>, before <see cref="PhysicalDeviceService"/> interprets it.
	/// It is what <see cref="PhysicalDeviceService.ExecuteDeviceCtlAsync"/> returns, so code that uses the service
	/// can be tested by returning canned output, without Xcode or a device.
	/// </summary>
	public sealed class DeviceCtlRunResult {

		DeviceCtlRunResult (bool toolNotFound, bool hostNotSupported, string message, int exitCode, string? stdout, string? stderr, string? json)
		{
			ToolNotFound = toolNotFound;
			HostNotSupported = hostNotSupported;
			Message = message;
			ExitCode = exitCode;
			StdOut = stdout ?? "";
			StdErr = stderr ?? "";
			Json = json;
		}

		/// <summary>True if xcrun does not exist or could not be started, in which case nothing else is set.</summary>
		public bool ToolNotFound { get; }

		/// <summary>True if the host cannot run devicectl because it is not macOS, in which case nothing else is set.</summary>
		public bool HostNotSupported { get; }

		/// <summary>Why xcrun could not be started. Empty unless <see cref="ToolNotFound"/> is true.</summary>
		public string Message { get; }

		/// <summary>The exit code of the process.</summary>
		public int ExitCode { get; }

		/// <summary>What the process wrote to its standard output. Empty if it wrote nothing.</summary>
		public string StdOut { get; }

		/// <summary>What the process wrote to its standard error. Empty if it wrote nothing.</summary>
		public string StdErr { get; }

		/// <summary>The contents of the JSON file devicectl wrote, or null if it did not write one.</summary>
		public string? Json { get; }

		/// <summary>The outcome when xcrun does not exist or could not be started.</summary>
		/// <param name="message">Why xcrun could not be started.</param>
		public static DeviceCtlRunResult NotFound (string message) => new DeviceCtlRunResult (true, false, message, -1, null, null, null);

		/// <summary>The outcome when the host is not macOS, so devicectl cannot run.</summary>
		public static DeviceCtlRunResult UnsupportedHost () => new DeviceCtlRunResult (false, true, "", -1, null, null, null);

		/// <summary>The outcome when the process ran to completion, whether it succeeded or not.</summary>
		/// <param name="exitCode">The exit code of the process.</param>
		/// <param name="stdout">What the process wrote to its standard output.</param>
		/// <param name="stderr">What the process wrote to its standard error.</param>
		/// <param name="json">The contents of the JSON file devicectl wrote, or null if it did not write one.</param>
		public static DeviceCtlRunResult Completed (int exitCode, string? stdout, string? stderr, string? json) => new DeviceCtlRunResult (false, false, "", exitCode, stdout, stderr, json);

		/// <summary>The outcome of a devicectl that exited with code 0 after writing <paramref name="json"/>. Meant for supplying canned output.</summary>
		/// <param name="json">The contents of the JSON file devicectl wrote, or null if it did not write one.</param>
		public static DeviceCtlRunResult FromJson (string? json) => Completed (0, null, null, json);
	}
}
