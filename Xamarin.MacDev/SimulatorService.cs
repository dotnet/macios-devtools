// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using Xamarin.MacDev.Models;

#nullable enable

namespace Xamarin.MacDev;

/// <summary>
/// High-level simulator operations wrapping <c>xcrun simctl</c>.
/// Follows the instance-based <see cref="ICustomLogger"/> pattern.
/// Operation patterns validated against Redth/AppleDev.Tools SimCtl and
/// ClientTools.Platform RemoteSimulatorValidator.
/// </summary>
public class SimulatorService {

	readonly ICustomLogger log;
	readonly SimCtl simctl;

	public SimulatorService (ICustomLogger log)
	{
		this.log = log ?? throw new ArgumentNullException (nameof (log));
		simctl = new SimCtl (log);
	}

	/// <summary>
	/// Lists all simulator devices. Optionally filters by availability.
	/// </summary>
	public List<SimulatorDeviceInfo> List (bool availableOnly = false)
	{
		var json = simctl.RunJson ("list", "devices");
		if (json is null)
			return new List<SimulatorDeviceInfo> ();

		var devices = SimctlOutputParser.ParseDevices (json, log);

		if (availableOnly)
			devices.RemoveAll (d => !d.IsAvailable);

		log.LogInfo ("Found {0} simulator device(s).", devices.Count);
		return devices;
	}

	/// <summary>
	/// Creates a new simulator device. Returns the UDID of the created device, or null on failure.
	/// Pattern from ClientTools.Platform: <c>xcrun simctl create "name" "deviceTypeId"</c>
	/// </summary>
	public string? Create (string name, string deviceTypeIdentifier, string? runtimeIdentifier = null)
	{
		if (string.IsNullOrEmpty (name))
			throw new ArgumentException ("Name must not be null or empty.", nameof (name));
		if (string.IsNullOrEmpty (deviceTypeIdentifier))
			throw new ArgumentException ("Device type identifier must not be null or empty.", nameof (deviceTypeIdentifier));

		string? output;
		if (!string.IsNullOrEmpty (runtimeIdentifier))
			output = simctl.Run ("create", name, deviceTypeIdentifier, runtimeIdentifier!);
		else
			output = simctl.Run ("create", name, deviceTypeIdentifier);

		if (output is null)
			return null;

		var udid = SimctlOutputParser.ParseCreateOutput (output);
		if (udid is not null)
			log.LogInfo ("Created simulator '{0}' with UDID {1}.", name, udid);
		else
			log.LogInfo ("Failed to create simulator '{0}'.", name);

		return udid;
	}

	/// <summary>
	/// Boots a simulator device.
	/// </summary>
	public bool Boot (string udidOrName)
	{
		return RunSimctlBool ("boot", udidOrName);
	}

	/// <summary>
	/// Shuts down a simulator device. Pass "all" to shut down all simulators.
	/// </summary>
	public bool Shutdown (string udidOrName)
	{
		return RunSimctlBool ("shutdown", udidOrName);
	}

	/// <summary>
	/// Erases (factory resets) a simulator device. Pass "all" to erase all.
	/// Pattern from Redth/AppleDev.Tools SimCtl.EraseAsync.
	/// </summary>
	public bool Erase (string udidOrName)
	{
		return RunSimctlBool ("erase", udidOrName);
	}

	/// <summary>
	/// Deletes a simulator device. Pass "unavailable" to delete unavailable sims,
	/// or "all" to delete all.
	/// </summary>
	public bool Delete (string udidOrName)
	{
		return RunSimctlBool ("delete", udidOrName);
	}

	/// <summary>
	/// Installs an app bundle (.app) onto a simulator device.
	/// Pattern: <c>xcrun simctl install &lt;udid&gt; &lt;appBundlePath&gt;</c>
	/// </summary>
	public bool Install (string udid, string appBundlePath)
	{
		if (string.IsNullOrEmpty (udid))
			throw new ArgumentException ("UDID must not be null or empty.", nameof (udid));
		if (string.IsNullOrEmpty (appBundlePath))
			throw new ArgumentException ("App bundle path must not be null or empty.", nameof (appBundlePath));

		var result = simctl.Run ("install", udid, appBundlePath);
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl install '{0}' on '{1}' succeeded.", appBundlePath, udid);
		return success;
	}

	/// <summary>
	/// Uninstalls an app from a simulator device.
	/// Pattern: <c>xcrun simctl uninstall &lt;udid&gt; &lt;bundleIdentifier&gt;</c>
	/// </summary>
	public bool Uninstall (string udid, string bundleIdentifier)
	{
		if (string.IsNullOrEmpty (udid))
			throw new ArgumentException ("UDID must not be null or empty.", nameof (udid));
		if (string.IsNullOrEmpty (bundleIdentifier))
			throw new ArgumentException ("Bundle identifier must not be null or empty.", nameof (bundleIdentifier));

		var result = simctl.Run ("uninstall", udid, bundleIdentifier);
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl uninstall '{0}' on '{1}' succeeded.", bundleIdentifier, udid);
		return success;
	}

	/// <summary>
	/// Launches an app on a booted simulator device.
	/// Optional extra arguments are forwarded to the app process.
	/// Pattern: <c>xcrun simctl launch &lt;udid&gt; &lt;bundleIdentifier&gt; [args…]</c>
	/// </summary>
	public bool Launch (string udid, string bundleIdentifier, params string [] extraArgs)
	{
		if (string.IsNullOrEmpty (udid))
			throw new ArgumentException ("UDID must not be null or empty.", nameof (udid));
		if (string.IsNullOrEmpty (bundleIdentifier))
			throw new ArgumentException ("Bundle identifier must not be null or empty.", nameof (bundleIdentifier));

		var args = new string [3 + extraArgs.Length];
		args [0] = "launch";
		args [1] = udid;
		args [2] = bundleIdentifier;
		Array.Copy (extraArgs, 0, args, 3, extraArgs.Length);

		var result = simctl.Run (args);
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl launch '{0}' on '{1}' succeeded.", bundleIdentifier, udid);
		return success;
	}

	/// <summary>
	/// Terminates a running app on a simulator device.
	/// Pattern: <c>xcrun simctl terminate &lt;udid&gt; &lt;bundleIdentifier&gt;</c>
	/// </summary>
	public bool Terminate (string udid, string bundleIdentifier)
	{
		if (string.IsNullOrEmpty (udid))
			throw new ArgumentException ("UDID must not be null or empty.", nameof (udid));
		if (string.IsNullOrEmpty (bundleIdentifier))
			throw new ArgumentException ("Bundle identifier must not be null or empty.", nameof (bundleIdentifier));

		var result = simctl.Run ("terminate", udid, bundleIdentifier);
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl terminate '{0}' on '{1}' succeeded.", bundleIdentifier, udid);
		return success;
	}

	/// <summary>
	/// Returns the path to an app container directory on a simulator device.
	/// The optional <paramref name="containerType"/> selects which container to
	/// return — typical values are <c>"app"</c>, <c>"data"</c>, and <c>"groups"</c>.
	/// When omitted the default container (equivalent to <c>"app"</c>) is returned.
	/// Pattern: <c>xcrun simctl get_app_container &lt;udid&gt; &lt;bundleIdentifier&gt; [containerType]</c>
	/// </summary>
	public string? GetAppContainer (string udid, string bundleIdentifier, string? containerType = null)
	{
		if (string.IsNullOrEmpty (udid))
			throw new ArgumentException ("UDID must not be null or empty.", nameof (udid));
		if (string.IsNullOrEmpty (bundleIdentifier))
			throw new ArgumentException ("Bundle identifier must not be null or empty.", nameof (bundleIdentifier));

		string? output;
		if (!string.IsNullOrEmpty (containerType))
			output = simctl.Run ("get_app_container", udid, bundleIdentifier, containerType!);
		else
			output = simctl.Run ("get_app_container", udid, bundleIdentifier);

		if (output is null)
			return null;

		var path = output.Trim ();
		if (string.IsNullOrEmpty (path))
			return null;

		log.LogInfo ("simctl get_app_container '{0}' on '{1}': {2}", bundleIdentifier, udid, path);
		return path;
	}

	bool RunSimctlBool (string subcommand, string target)
	{
		var result = simctl.Run (subcommand, target);
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl {0} '{1}' succeeded.", subcommand, target);
		return success;
	}
}
