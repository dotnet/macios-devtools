// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.MacDev;

#nullable enable

namespace tests;

/// <summary>Builds <see cref="PhysicalDeviceService"/> instances backed by in-process fake devicectl runners.</summary>
static class FakeDeviceCtl {

	/// <summary>A short grace period, so tests with a runner that ignores cancellation stay fast.</summary>
	public static readonly TimeSpan ShortGracePeriod = TimeSpan.FromMilliseconds (100);

	public static PhysicalDeviceService CreateService (ICustomLogger log, Func<CancellationToken, Task<DeviceCtlRunResult>> runner, bool supportedHost = true, TimeSpan? gracePeriod = null)
		=> new PhysicalDeviceService (log, runner, () => supportedHost, gracePeriod ?? ShortGracePeriod);

	public static Func<CancellationToken, Task<DeviceCtlRunResult>> Returns (DeviceCtlRunResult result) => _ => Task.FromResult (result);

	/// <summary>A runner whose devicectl process completed with the given exit code, output and JSON file.</summary>
	public static Func<CancellationToken, Task<DeviceCtlRunResult>> ReturnsJson (string? json, int exitCode = 0, string stdout = "", string stderr = "")
		=> Returns (DeviceCtlRunResult.Completed (exitCode, stdout, stderr, json));

	/// <summary>A runner that never finishes on its own, but stops when it is cancelled, like the real one.</summary>
	public static async Task<DeviceCtlRunResult> WaitUntilCancelled (CancellationToken token)
	{
		await Task.Delay (Timeout.Infinite, token).ConfigureAwait (false);
		return DeviceCtlRunResult.NotFound ("The runner was not cancelled.");
	}
}
