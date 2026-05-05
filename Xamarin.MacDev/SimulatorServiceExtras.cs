// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#nullable enable

namespace Xamarin.MacDev;

public partial class SimulatorService {

	SimulatorPrivacy? privacyService;
	SimulatorStatusBar? statusBarService;
	SimulatorLocation? locationService;
	SimulatorScreenCapture? screenCaptureService;

	/// <summary>
	/// Provides <c>xcrun simctl privacy</c> operations (grant, revoke, reset).
	/// </summary>
	public SimulatorPrivacy Privacy => privacyService ??= new SimulatorPrivacy (log, simctl);

	/// <summary>
	/// Provides <c>xcrun simctl status_bar</c> operations (override, clear).
	/// </summary>
	public SimulatorStatusBar StatusBar => statusBarService ??= new SimulatorStatusBar (log, simctl);

	/// <summary>
	/// Provides <c>xcrun simctl location</c> operations (set, clear, run).
	/// </summary>
	public SimulatorLocation Location => locationService ??= new SimulatorLocation (log, simctl);

	/// <summary>
	/// Provides <c>xcrun simctl io</c> operations (screenshot, recordVideo).
	/// </summary>
	public SimulatorScreenCapture ScreenCapture => screenCaptureService ??= new SimulatorScreenCapture (log, simctl);

	/// <summary>
	/// Sets the UI appearance (light/dark) of the simulator.
	/// Wraps <c>xcrun simctl ui &lt;udid&gt; appearance light|dark</c>.
	/// </summary>
	public bool SetAppearance (string udidOrName, SimulatorAppearance appearance)
	{
		if (string.IsNullOrWhiteSpace (udidOrName))
			throw new ArgumentException ("Simulator UDID or name must not be null or empty.", nameof (udidOrName));

		var value = appearance == SimulatorAppearance.Dark ? "dark" : "light";
		var result = simctl.Run ("ui", udidOrName, "appearance", value);
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl ui appearance '{0}' set to {1}.", udidOrName, value);
		return success;
	}

	/// <summary>
	/// Gets the current UI appearance of the simulator.
	/// Wraps <c>xcrun simctl ui &lt;udid&gt; appearance</c>.
	/// Returns null if the query fails or the output is unrecognised.
	/// </summary>
	public SimulatorAppearance? GetAppearance (string udidOrName)
	{
		if (string.IsNullOrWhiteSpace (udidOrName))
			throw new ArgumentException ("Simulator UDID or name must not be null or empty.", nameof (udidOrName));

		var output = simctl.Run ("ui", udidOrName, "appearance");
		if (output is null)
			return null;

		var trimmed = output.Trim ();
		if (string.Equals (trimmed, "dark", StringComparison.OrdinalIgnoreCase))
			return SimulatorAppearance.Dark;
		if (string.Equals (trimmed, "light", StringComparison.OrdinalIgnoreCase))
			return SimulatorAppearance.Light;

		log.LogInfo ("Unrecognised appearance value from simctl: '{0}'.", trimmed);
		return null;
	}

	/// <summary>
	/// Opens a URL on the simulator, triggering the registered URL handler or browser.
	/// Wraps <c>xcrun simctl openurl &lt;udid&gt; &lt;url&gt;</c>.
	/// </summary>
	public bool OpenUrl (string udidOrName, string url)
	{
		if (string.IsNullOrWhiteSpace (udidOrName))
			throw new ArgumentException ("Simulator UDID or name must not be null or empty.", nameof (udidOrName));
		if (string.IsNullOrWhiteSpace (url))
			throw new ArgumentException ("URL must not be null or empty.", nameof (url));

		var result = simctl.Run ("openurl", udidOrName, url);
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl openurl '{0}' succeeded.", udidOrName);
		return success;
	}

	/// <summary>
	/// Sends a push notification to the simulator.
	/// <paramref name="payloadJsonOrPath"/> may be a file path to an APNS JSON payload file
	/// or a raw JSON string (starting with <c>{</c>), which is written to a temporary file.
	/// Wraps <c>xcrun simctl push &lt;udid&gt; &lt;bundleId&gt; &lt;file&gt;</c>.
	/// </summary>
	public bool Push (string udidOrName, string bundleIdentifier, string payloadJsonOrPath)
	{
		if (string.IsNullOrWhiteSpace (udidOrName))
			throw new ArgumentException ("Simulator UDID or name must not be null or empty.", nameof (udidOrName));
		if (string.IsNullOrWhiteSpace (bundleIdentifier))
			throw new ArgumentException ("Bundle identifier must not be null or empty.", nameof (bundleIdentifier));
		if (string.IsNullOrWhiteSpace (payloadJsonOrPath))
			throw new ArgumentException ("Payload JSON or path must not be null or empty.", nameof (payloadJsonOrPath));

		var isInlineJson = payloadJsonOrPath.TrimStart ().StartsWith ("{", StringComparison.Ordinal);
		if (isInlineJson) {
			var tempPath = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName () + ".json");
			try {
				File.WriteAllText (tempPath, payloadJsonOrPath, Encoding.UTF8);
				return RunPush (udidOrName, bundleIdentifier, tempPath);
			} finally {
				try { if (File.Exists (tempPath)) File.Delete (tempPath); } catch (IOException ex) {
					log.LogInfo ("Failed to delete temporary push payload file '{0}': {1}", tempPath, ex.Message);
				} catch (UnauthorizedAccessException ex) {
					log.LogInfo ("Failed to delete temporary push payload file '{0}': {1}", tempPath, ex.Message);
				}
			}
		}

		return RunPush (udidOrName, bundleIdentifier, payloadJsonOrPath);
	}

	bool RunPush (string udidOrName, string bundleIdentifier, string payloadPath)
	{
		var result = simctl.Run ("push", udidOrName, bundleIdentifier, payloadPath);
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl push to '{0}' ({1}) succeeded.", udidOrName, bundleIdentifier);
		return success;
	}

	/// <summary>
	/// Adds media files (photos, videos) to the simulator's media library.
	/// Wraps <c>xcrun simctl addmedia &lt;udid&gt; &lt;file&gt; ...</c>.
	/// </summary>
	public bool AddMedia (string udidOrName, IEnumerable<string> paths)
	{
		if (string.IsNullOrWhiteSpace (udidOrName))
			throw new ArgumentException ("Simulator UDID or name must not be null or empty.", nameof (udidOrName));
		if (paths is null)
			throw new ArgumentNullException (nameof (paths));

		var pathList = new List<string> (paths);
		if (pathList.Count == 0)
			throw new ArgumentException ("At least one media path must be provided.", nameof (paths));

		var args = new string [pathList.Count + 2];
		args [0] = "addmedia";
		args [1] = udidOrName;
		for (int i = 0; i < pathList.Count; i++)
			args [i + 2] = pathList [i];

		var result = simctl.Run (args);
		var success = result is not null;
		if (success)
			log.LogInfo ("simctl addmedia '{0}' ({1} file(s)) succeeded.", udidOrName, pathList.Count);
		return success;
	}
}
