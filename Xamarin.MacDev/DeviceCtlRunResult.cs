// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Xamarin.MacDev {

	/// <summary>
	/// The raw outcome of running <c>xcrun devicectl</c>, before it is interpreted.
	/// </summary>
	internal sealed class DeviceCtlRunResult {

		DeviceCtlRunResult (bool toolNotFound, string message, int exitCode, string? stdout, string? stderr, string? json)
		{
			ToolNotFound = toolNotFound;
			Message = message;
			ExitCode = exitCode;
			StdOut = stdout ?? "";
			StdErr = stderr ?? "";
			Json = json;
		}

		/// <summary>True if xcrun does not exist or could not be started, in which case nothing else is set.</summary>
		public bool ToolNotFound { get; }

		/// <summary>Why xcrun could not be started. Empty unless <see cref="ToolNotFound"/> is true.</summary>
		public string Message { get; }

		/// <summary>The exit code of the process.</summary>
		public int ExitCode { get; }

		public string StdOut { get; }

		public string StdErr { get; }

		/// <summary>The contents of the JSON file devicectl wrote, or null if it did not write one.</summary>
		public string? Json { get; }

		public static DeviceCtlRunResult NotFound (string message) => new DeviceCtlRunResult (true, message, -1, null, null, null);

		public static DeviceCtlRunResult Completed (int exitCode, string? stdout, string? stderr, string? json) => new DeviceCtlRunResult (false, "", exitCode, stdout, stderr, json);
	}
}
