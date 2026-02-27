// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;

using Xamarin.MacDev.Models;

#nullable enable

namespace Xamarin.MacDev {

	/// <summary>
	/// High-level simulator operations wrapping <c>xcrun simctl</c>.
	/// Follows the instance-based <see cref="ICustomLogger"/> pattern.
	/// Operation patterns validated against Redth/AppleDev.Tools SimCtl and
	/// ClientTools.Platform RemoteSimulatorValidator.
	/// </summary>
	public class SimulatorService {

		static readonly string XcrunPath = "/usr/bin/xcrun";

		readonly ICustomLogger log;

		public SimulatorService (ICustomLogger log)
		{
			this.log = log ?? throw new ArgumentNullException (nameof (log));
		}

		/// <summary>
		/// Lists all simulator devices. Optionally filters by availability.
		/// </summary>
		public List<SimulatorDeviceInfo> List (bool availableOnly = false)
		{
			var json = RunSimctl ("list", "devices", "--json");
			if (json is null)
				return new List<SimulatorDeviceInfo> ();

			var devices = SimctlOutputParser.ParseDevices (json);

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
				output = RunSimctl ("create", name, deviceTypeIdentifier, runtimeIdentifier!);
			else
				output = RunSimctl ("create", name, deviceTypeIdentifier);

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
		/// Runs a simctl subcommand and returns stdout, or null on failure.
		/// </summary>
		string? RunSimctl (params string [] args)
		{
			if (!File.Exists (XcrunPath)) {
				log.LogInfo ("xcrun not found at '{0}'.", XcrunPath);
				return null;
			}

			var fullArgs = new string [args.Length + 1];
			fullArgs [0] = "simctl";
			Array.Copy (args, 0, fullArgs, 1, args.Length);

			try {
				var (exitCode, stdout, stderr) = ProcessUtils.Exec (XcrunPath, fullArgs);
				if (exitCode != 0) {
					log.LogInfo ("simctl {0} failed (exit {1}): {2}", args [0], exitCode, stderr.Trim ());
					return null;
				}
				return stdout;
			} catch (System.ComponentModel.Win32Exception ex) {
				log.LogInfo ("Could not run xcrun: {0}", ex.Message);
				return null;
			}
		}

		/// <summary>
		/// Runs a simctl subcommand and returns whether it succeeded.
		/// </summary>
		bool RunSimctlBool (string subcommand, string target)
		{
			var result = RunSimctl (subcommand, target);
			var success = result is not null;
			if (success)
				log.LogInfo ("simctl {0} '{1}' succeeded.", subcommand, target);
			return success;
		}
	}
}
