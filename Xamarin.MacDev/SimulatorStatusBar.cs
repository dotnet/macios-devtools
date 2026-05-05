// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

#nullable enable

namespace Xamarin.MacDev;

/// <summary>
/// Battery state values for the simulator status-bar override.
/// Used with <c>xcrun simctl status_bar &lt;udid&gt; override --batteryState</c>.
/// </summary>
public enum SimulatorBatteryState {
	Charging,
	Charged,
	Discharging,
}

/// <summary>
/// Data network type values for the simulator status-bar override.
/// Used with <c>xcrun simctl status_bar &lt;udid&gt; override --dataNetwork</c>.
/// </summary>
public enum SimulatorDataNetwork {
	Wifi,
	ThreeG,
	FourG,
	Lte,
	LteA,
	LtePlus,
	FiveG,
	FiveGPlus,
	FiveGUc,
	FiveGA,
}

/// <summary>
/// Override values for the simulator status bar.
/// All fields are optional; pass only the fields you want to override.
/// </summary>
public record StatusBarOverrides (
	string? Time = null,
	int? BatteryLevel = null,
	SimulatorBatteryState? BatteryState = null,
	SimulatorDataNetwork? DataNetwork = null,
	int? CellularBars = null,
	int? WifiBars = null,
	string? OperatorName = null);

/// <summary>
/// Wraps <c>xcrun simctl status_bar</c> operations for overriding and clearing
/// simulator status bar values.
/// </summary>
public class SimulatorStatusBar {

	readonly ICustomLogger log;
	readonly SimCtl simctl;

	internal SimulatorStatusBar (ICustomLogger log, SimCtl simctl)
	{
		this.log = log;
		this.simctl = simctl;
	}

	/// <summary>
	/// Overrides status-bar values on the simulator.
	/// Wraps <c>xcrun simctl status_bar &lt;udid&gt; override [options]</c>.
	/// </summary>
	public bool Override (string udidOrName, StatusBarOverrides overrides)
	{
		if (string.IsNullOrWhiteSpace (udidOrName))
			throw new ArgumentException ("Simulator UDID or name must not be null or empty.", nameof (udidOrName));
		if (overrides is null)
			throw new ArgumentNullException (nameof (overrides));

		if (overrides.Time is null && !overrides.BatteryLevel.HasValue &&
			!overrides.BatteryState.HasValue && !overrides.DataNetwork.HasValue &&
			!overrides.CellularBars.HasValue && !overrides.WifiBars.HasValue &&
			overrides.OperatorName is null)
			throw new ArgumentException ("At least one StatusBarOverrides field must be set.", nameof (overrides));

		var args = BuildOverrideArgs (udidOrName, overrides);
		var result = simctl.Run (args);
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl status_bar override '{0}' succeeded.", udidOrName);
		return success;
	}

	/// <summary>
	/// Clears all status-bar overrides on the simulator.
	/// Wraps <c>xcrun simctl status_bar &lt;udid&gt; clear</c>.
	/// </summary>
	public bool Clear (string udidOrName)
	{
		if (string.IsNullOrWhiteSpace (udidOrName))
			throw new ArgumentException ("Simulator UDID or name must not be null or empty.", nameof (udidOrName));

		var result = simctl.Run ("status_bar", udidOrName, "clear");
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl status_bar clear '{0}' succeeded.", udidOrName);
		return success;
	}

	static string [] BuildOverrideArgs (string udidOrName, StatusBarOverrides overrides)
	{
		var args = new List<string> {
			"status_bar", udidOrName, "override",
		};

		if (overrides.Time is not null) {
			args.Add ("--time");
			args.Add (overrides.Time);
		}

		if (overrides.BatteryLevel.HasValue) {
			args.Add ("--batteryLevel");
			args.Add (overrides.BatteryLevel.Value.ToString ());
		}

		if (overrides.BatteryState.HasValue) {
			args.Add ("--batteryState");
			args.Add (ToSimctlBatteryState (overrides.BatteryState.Value));
		}

		if (overrides.DataNetwork.HasValue) {
			args.Add ("--dataNetwork");
			args.Add (ToSimctlDataNetwork (overrides.DataNetwork.Value));
		}

		if (overrides.CellularBars.HasValue) {
			args.Add ("--cellularBars");
			args.Add (overrides.CellularBars.Value.ToString ());
		}

		if (overrides.WifiBars.HasValue) {
			args.Add ("--wifiBars");
			args.Add (overrides.WifiBars.Value.ToString ());
		}

		if (overrides.OperatorName is not null) {
			args.Add ("--operatorName");
			args.Add (overrides.OperatorName);
		}

		return args.ToArray ();
	}

	public static string ToSimctlBatteryState (SimulatorBatteryState state)
	{
		return state switch {
			SimulatorBatteryState.Charging => "Charging",
			SimulatorBatteryState.Charged => "Charged",
			SimulatorBatteryState.Discharging => "Discharging",
			_ => throw new ArgumentOutOfRangeException (nameof (state), state, null),
		};
	}

	public static string ToSimctlDataNetwork (SimulatorDataNetwork network)
	{
		return network switch {
			SimulatorDataNetwork.Wifi => "wifi",
			SimulatorDataNetwork.ThreeG => "3g",
			SimulatorDataNetwork.FourG => "4g",
			SimulatorDataNetwork.Lte => "lte",
			SimulatorDataNetwork.LteA => "lte-a",
			SimulatorDataNetwork.LtePlus => "lte+",
			SimulatorDataNetwork.FiveG => "5g",
			SimulatorDataNetwork.FiveGPlus => "5g+",
			SimulatorDataNetwork.FiveGUc => "5g-uc",
			SimulatorDataNetwork.FiveGA => "5g-a",
			_ => throw new ArgumentOutOfRangeException (nameof (network), network, null),
		};
	}
}
