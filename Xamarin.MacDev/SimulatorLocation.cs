// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Globalization;

#nullable enable

namespace Xamarin.MacDev;

/// <summary>
/// Wraps <c>xcrun simctl location</c> operations for setting, clearing,
/// and simulating GPS routes on a simulator.
/// </summary>
public class SimulatorLocation {

	readonly ICustomLogger log;
	readonly SimCtl simctl;

	internal SimulatorLocation (ICustomLogger log, SimCtl simctl)
	{
		this.log = log;
		this.simctl = simctl;
	}

	/// <summary>
	/// Sets the simulated GPS location on the simulator.
	/// Wraps <c>xcrun simctl location &lt;udid&gt; set &lt;lat&gt;,&lt;lng&gt;</c>.
	/// </summary>
	public bool Set (string udidOrName, double latitude, double longitude)
	{
		if (string.IsNullOrWhiteSpace (udidOrName))
			throw new ArgumentException ("Simulator UDID or name must not be null or empty.", nameof (udidOrName));

		var coords = string.Format (CultureInfo.InvariantCulture, "{0},{1}", latitude, longitude);
		var result = simctl.Run ("location", udidOrName, "set", coords);
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl location set '{0}' to {1} succeeded.", udidOrName, coords);
		return success;
	}

	/// <summary>
	/// Clears the simulated GPS location on the simulator.
	/// Wraps <c>xcrun simctl location &lt;udid&gt; clear</c>.
	/// </summary>
	public bool Clear (string udidOrName)
	{
		if (string.IsNullOrWhiteSpace (udidOrName))
			throw new ArgumentException ("Simulator UDID or name must not be null or empty.", nameof (udidOrName));

		var result = simctl.Run ("location", udidOrName, "clear");
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl location clear '{0}' succeeded.", udidOrName);
		return success;
	}

	/// <summary>
	/// Runs a GPX route simulation on the simulator.
	/// Wraps <c>xcrun simctl location &lt;udid&gt; run &lt;gpxPath&gt;</c>.
	/// </summary>
	public bool Run (string udidOrName, string gpxPath)
	{
		if (string.IsNullOrWhiteSpace (udidOrName))
			throw new ArgumentException ("Simulator UDID or name must not be null or empty.", nameof (udidOrName));
		if (string.IsNullOrWhiteSpace (gpxPath))
			throw new ArgumentException ("GPX path must not be null or empty.", nameof (gpxPath));

		var result = simctl.Run ("location", udidOrName, "run", gpxPath);
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl location run '{0}' with '{1}' succeeded.", udidOrName, gpxPath);
		return success;
	}
}
