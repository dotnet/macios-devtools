// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

#nullable enable

namespace Xamarin.MacDev;

/// <summary>
/// Wraps <c>xcrun simctl privacy</c> operations for granting, revoking,
/// and resetting simulator privacy permissions.
/// </summary>
public class SimulatorPrivacy {

	readonly ICustomLogger log;
	readonly SimCtl simctl;

	internal SimulatorPrivacy (ICustomLogger log, SimCtl simctl)
	{
		this.log = log;
		this.simctl = simctl;
	}

	/// <summary>
	/// Grants a privacy permission for all apps or a specific bundle on the simulator.
	/// Wraps <c>xcrun simctl privacy &lt;udid&gt; grant &lt;service&gt; [bundleId]</c>.
	/// </summary>
	public bool Grant (string udidOrName, PrivacyPermission permission, string? bundleIdentifier = null)
	{
		return RunPrivacy ("grant", udidOrName, permission, bundleIdentifier);
	}

	/// <summary>
	/// Revokes a privacy permission for all apps or a specific bundle on the simulator.
	/// Wraps <c>xcrun simctl privacy &lt;udid&gt; revoke &lt;service&gt; [bundleId]</c>.
	/// </summary>
	public bool Revoke (string udidOrName, PrivacyPermission permission, string? bundleIdentifier = null)
	{
		return RunPrivacy ("revoke", udidOrName, permission, bundleIdentifier);
	}

	/// <summary>
	/// Resets a privacy permission for all apps or a specific bundle on the simulator.
	/// Wraps <c>xcrun simctl privacy &lt;udid&gt; reset &lt;service&gt; [bundleId]</c>.
	/// </summary>
	public bool Reset (string udidOrName, PrivacyPermission permission, string? bundleIdentifier = null)
	{
		return RunPrivacy ("reset", udidOrName, permission, bundleIdentifier);
	}

	bool RunPrivacy (string action, string udidOrName, PrivacyPermission permission, string? bundleIdentifier)
	{
		if (string.IsNullOrWhiteSpace (udidOrName))
			throw new ArgumentException ("Simulator UDID or name must not be null or empty.", nameof (udidOrName));

		var service = ToSimctlServiceName (permission);

		string? result;
		if (!string.IsNullOrEmpty (bundleIdentifier))
			result = simctl.Run ("privacy", udidOrName, action, service, bundleIdentifier!);
		else
			result = simctl.Run ("privacy", udidOrName, action, service);

		var success = result is not null;
		if (success)
			log.LogInfo ("simctl privacy {0} {1} {2} succeeded.", udidOrName, action, service);
		return success;
	}

	public static string ToSimctlServiceName (PrivacyPermission permission)
	{
		return permission switch {
			PrivacyPermission.All => "all",
			PrivacyPermission.Calendar => "calendar",
			PrivacyPermission.ContactsLimited => "contacts-limited",
			PrivacyPermission.Contacts => "contacts",
			PrivacyPermission.Location => "location",
			PrivacyPermission.LocationAlways => "location-always",
			PrivacyPermission.PhotosAdd => "photos-add",
			PrivacyPermission.Photos => "photos",
			PrivacyPermission.MediaLibrary => "media-library",
			PrivacyPermission.Microphone => "microphone",
			PrivacyPermission.Motion => "motion",
			PrivacyPermission.Reminders => "reminders",
			PrivacyPermission.Siri => "siri",
			_ => throw new ArgumentOutOfRangeException (nameof (permission), permission, null),
		};
	}
}
