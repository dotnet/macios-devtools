# Xamarin.MacDev

## Welcome!

This module is a support repository used by both **Xamarin.iOS** and **Xamarin.Mac**.

It contains support libraries used during msbuild builds.

See [dotnet/macios](https://github.com/dotnet/macios) for more information on contributing to and providing feedback on Xamarin.iOS and Xamarin.Mac.

## Physical device discovery

`PhysicalDeviceService` lists the physical Apple devices (iPhone, iPad, iPod touch, Apple Watch, Apple TV, Apple Vision Pro and Mac) known to CoreDevice by running `xcrun devicectl list devices`. It is the physical-device counterpart of `SimulatorService`.

```csharp
using System;
using System.Linq;
using Xamarin.MacDev;
using Xamarin.MacDev.Models;

var service = new PhysicalDeviceService (logger); // logger is an ICustomLogger
var result = await service.ListAsync (cancellationToken);

if (!result.IsSuccess) {
	// result.Status says why, see the table below.
	Console.Error.WriteLine ($"Device discovery failed ({result.Status}): {result.ErrorMessage}");
	return;
}

foreach (var device in result.Devices.Where (d => d.Kind == PhysicalDeviceKind.IPhone && d.IsReachable))
	Console.WriteLine ($"{device.Name} {device.HardwareUdid} ({device.OSVersion}, {device.Availability})");
```

By default devicectl runs with the Xcode that `DEVELOPER_DIR` or `xcode-select` selects. To use a specific Xcode, pass its developer directory, for example `AppleSdkSettings.DeveloperRoot` or the `SdkDevPath` of an MSBuild task. It is set as `DEVELOPER_DIR` for the devicectl process only, so the environment of the current process is not changed and services for different Xcode installations can be used concurrently:

```csharp
var service = new PhysicalDeviceService (logger, "/Applications/Xcode.app/Contents/Developer");
```

`ListAsync` reports failures through `PhysicalDeviceDiscoveryResult.Status` instead of throwing, so an empty device list can be told apart from a failure:

| Status | Meaning |
| --- | --- |
| `Success` | devicectl ran and its output was understood. `Devices` may be empty, which means no physical device is known. |
| `UnsupportedHost` | The host is not macOS. Nothing was run. |
| `ToolUnavailable` | `xcrun` or `devicectl` is missing, for example because Xcode is not installed, is too old, is not selected or its license has not been accepted. |
| `ExecutionFailed` | devicectl ran but failed (a non-zero exit code or an error in its output). `ExitCode` and `ErrorMessage` describe the failure. |
| `TimedOut` | devicectl did not finish in time and was terminated. |
| `InvalidOutput` | devicectl exited successfully, but its output was missing, malformed or in an unexpected format. |

Only invalid arguments (`ArgumentOutOfRangeException` for a non-positive timeout) and cancellation through the `CancellationToken` (`OperationCanceledException`) throw.

### What is returned

- **Remembered devices are included.** CoreDevice also reports paired devices that are not currently reachable. Use `Availability` (`Connected`, `Available`, `Unavailable` or `Unknown`) or `IsReachable` to tell them apart. It is derived only from the connection (tunnel) state devicectl reports, so a paired device is not treated as reachable. A missing or unrecognized state is `Unknown`. The raw values stay available in `ConnectionState`, `PairingState` and `TransportType`.
- **Simulators are excluded.** Newer Xcode versions also list simulators in devicectl. Use `SimulatorService` for those.
- **Hardware UDID and CoreDevice identifier are distinct.** `HardwareUdid` is the hardware UDID devicectl reports, or empty if it reports none (see `HasHardwareUdid`). `Identifier` is the CoreDevice identifier. `Udid` is unchanged for compatibility: it is the hardware UDID, falling back to the CoreDevice identifier when there is none.
- **One entry per device.** If devicectl reports the same hardware more than once (for example once per connection path, such as over USB and over the network), those entries are merged by `HardwareUdid` into one `PhysicalDeviceInfo`, and `Connections` lists every transport with the preferred one first (the most available, then wired over network). Entries without a hardware UDID cannot be correlated and are never merged.
- **Device kind.** `Kind` (`IPhone`, `IPad`, `IPod`, `AppleWatch`, `AppleTV`, `AppleVision`, `Mac` or `Unknown`) is derived from `DeviceClass`, then `ProductType`, then `Platform`. It is never guessed from the device name.
- Name, `Platform`, `DeviceClass`, `ProductType`, `HardwareModel`, `MarketingName`, `OSVersion`, `BuildVersion` and `CpuArchitecture` are preserved. Values devicectl does not report are empty strings.

### Requirements and behavior

- Requires macOS with Xcode 15 or later (selected, or passed as the developer directory), because `devicectl` ships with Xcode 15 and later and not with the Command Line Tools. Xcode 15 itself requires macOS 13.5 or later. devicectl manages devices through CoreDevice, which targets iOS and iPadOS 17, tvOS 17, watchOS 10 and visionOS. Devices CoreDevice cannot manage, such as ones running older operating systems, may be missing from the list or listed with little information. For example, a legacy iPad is listed without a name, OS version or hardware UDID, with a `PairingState` of `unsupported` and an `Availability` of `Unavailable`. That is decided by devicectl, not by this library.
- On any other host `ListAsync` returns `UnsupportedHost` without running anything. `PhysicalDeviceService.IsSupportedHost` tells whether the current host is supported.
- If Xcode is missing, too old, not selected or its license has not been accepted, `ListAsync` returns `ToolUnavailable` with a message describing the problem (xcrun's own message when it reported one). The same status is returned for a developer directory that does not exist.
- Discovery is read-only. It only runs `devicectl list devices`; it never pairs with, trusts, connects to or modifies a device. devicectl writes its JSON output to a temporary file (`--json-output`) that is deleted afterwards.
- `ListAsync` waits at most `PhysicalDeviceService.DefaultTimeout` (30 seconds). Use `ListAsync (TimeSpan, CancellationToken)` for another limit. When the timeout elapses, or the `CancellationToken` is cancelled, devicectl is terminated (the `net10.0` build of the library also terminates the processes devicectl started that are still running). The call never waits longer than the timeout plus a short grace period, even for a runner that ignores cancellation, as long as it returns its task promptly instead of blocking the calling thread.
- Diagnostics are written to the `ICustomLogger` passed to the constructor. The service never writes to stdout. A failed discovery is logged at info level because the result already carries it, and a logger that turns warnings into build warnings (as MSBuild tasks do) should not fail a build over a missing Xcode; only a devicectl runner that does not stop when cancelled is logged as a warning.
- `DeviceCtlOutputParser.ParseDevices` stays lenient: it returns every entry, including simulators and entries without an identity, and keeps the `Udid` fallback. `DeviceCtlOutputParser.TryParseDevices` is its strict counterpart and reports output that is not a valid devicectl device list, including an entry without a text `identifier`.

### Testing code that uses the service

`PhysicalDeviceService.ExecuteDeviceCtlAsync` is the only place the service starts a process, and it is `protected virtual`. Code that uses the service can therefore be tested without Xcode or a device by deriving from it and returning canned devicectl output. It is the same pattern the [dotnet/macios](https://github.com/dotnet/macios) `GetAvailableDevices` tests use with their own `ExecuteCtlAsync`, and the devicectl JSON documents they use as fixtures work unchanged.

```csharp
sealed class FakePhysicalDeviceService : PhysicalDeviceService {

	public string DeviceCtlJson = "";

	public FakePhysicalDeviceService (ICustomLogger log)
		: base (log)
	{
	}

	protected override Task<DeviceCtlRunResult> ExecuteDeviceCtlAsync (CancellationToken cancellationToken)
		=> Task.FromResult (DeviceCtlRunResult.FromJson (DeviceCtlJson));
}
```

The service interprets the canned output exactly as it does real output, so every `Status` above can be tested. `DeviceCtlRunResult` has a factory for each way a run can end:

| Factory | Simulates | Resulting `Status` |
| --- | --- | --- |
| `FromJson (json)` | devicectl exited with code 0 after writing `json` | `Success`; `InvalidOutput` if `json` is not a device list, or `ExecutionFailed` if it is a devicectl error report |
| `Completed (exitCode, stdout, stderr, json)` | devicectl ran to completion with any exit code and output | `Success`, `ExecutionFailed`, `ToolUnavailable` (for xcrun's own failures) or `InvalidOutput` |
| `NotFound (message)` | `xcrun` could not be started | `ToolUnavailable` |
| `UnsupportedHost ()` | the host is not macOS | `UnsupportedHost` |

`Devices` is always empty unless the status is `Success`, so code that maps `result.Devices` treats a failed discovery as no devices without checking the status. `NotFound` requires a message, and a blank one is replaced by a generic message so that a failed discovery always says what went wrong. An override must return its task promptly and do any waiting asynchronously: the timeout applies to the task it returns, not to a call that blocks the calling thread before returning one. An exception thrown by an override, other than `OperationCanceledException`, is not caught by the service. `tests/MaciosParityTests.cs` runs the dotnet/macios devicectl fixtures through the service in this way.

## License

Copyright (c) .NET Foundation Contributors. All rights reserved.
Licensed under the [MIT](https://github.com/xamarin/Xamarin.MacDev/blob/master/LICENSE) License.
