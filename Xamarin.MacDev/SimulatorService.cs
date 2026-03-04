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
		var json = simctl.Run ("list", "devices", "--json");
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

	bool RunSimctlBool (string subcommand, string target)
	{
		var result = simctl.Run (subcommand, target);
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl {0} '{1}' succeeded.", subcommand, target);
		return success;
	}
}
