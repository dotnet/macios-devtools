// 
// AppleSdkSettings.cs
//  
// Authors: Michael Hutchinson <mhutch@xamarin.com>
//          Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc. (http://xamarin.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Xamarin.MacDev {
	[Obsolete ("Use 'XcodeLocator' instead, this class keeps static state, which causes various problems when the state isn't updated.")]
	public static class AppleSdkSettings {
		static readonly string SettingsPath;

		// Put newer SDKs at the top as we scan from 0 -> List.Count
		public static readonly string [] DefaultRoots = new string [] {
			"/Applications/Xcode.app",
		};
		static DateTime lastWritten;

		public static string SdkNotInstalledReason { get; private set; }

		static void GetNewPaths (string root, out string xcode, out string vplist, out string devroot)
		{
			xcode = root;
			vplist = Path.Combine (root, "Contents", "version.plist");
			devroot = Path.Combine (root, "Contents", "Developer");
		}

		static void GetOldPaths (string root, out string xcode, out string vplist, out string devroot)
		{
			xcode = Path.Combine (root, "Applications", "Xcode.app");
			vplist = Path.Combine (root, "Library", "version.plist");
			devroot = root;
		}

		static bool ValidatePaths (string xcode, string vplist, string devroot)
		{
			return Directory.Exists (xcode)
				&& Directory.Exists (devroot)
				&& File.Exists (vplist)
				&& File.Exists (Path.Combine (xcode, "Contents", "Info.plist"));
		}

		public static bool ValidateSdkLocation (string location, out string xcode, out string vplist, out string devroot)
		{
			GetNewPaths (location, out xcode, out vplist, out devroot);
			if (ValidatePaths (xcode, vplist, devroot))
				return true;

			GetOldPaths (location, out xcode, out vplist, out devroot);
			if (ValidatePaths (xcode, vplist, devroot))
				return true;

			return false;
		}

		public static void SetConfiguredSdkLocation (string location)
		{
			PDictionary plist;
			bool binary;

			try {
				plist = PDictionary.FromFile (SettingsPath, out binary);
			} catch (FileNotFoundException) {
				plist = new PDictionary ();
				binary = false;
			}

			if (!string.IsNullOrEmpty (location)) {
				plist.SetString ("AppleSdkRoot", location);
			} else {
				plist.Remove ("AppleSdkRoot");
			}

			plist.Save (SettingsPath, true, binary);

			//Init ();
			//var changed = Changed;
			//if (changed != null)
			//	changed ();
		}

		public static string GetConfiguredSdkLocation ()
		{
			if (XcodeLocator.TryReadSettingsPath (LoggingServiceLogger.Instance, SettingsPath, out var path))
				return path;

			// Then check the system's default Xcode
			if (TryGetSystemXcode (out path))
				return path;

			// Finally return the hardcoded default
			LoggingService.LogInfo (string.Format ("Using the default Xcode location '{0}'", DefaultRoots [0]));
			return DefaultRoots [0];
		}

		static void SetInvalid ()
		{
			XcodePath = string.Empty;
			InvalidDeveloperRoot = DeveloperRoot;
			DeveloperRoot = string.Empty;
			DeveloperRootVersionPlist = string.Empty;
			IsValid = false;
			DTXcode = null;
			XcodeVersion = new Version (0, 0, 0);
			XcodeRevision = string.Empty;
			lastWritten = DateTime.MinValue;
		}

		static AppleSdkSettings ()
		{
			foreach (var path in XcodeLocator.SettingsPathCandidates) {
				if (!File.Exists (path))
					continue;
				SettingsPath = path;
				break;
			}

			Directory.CreateDirectory (Path.GetDirectoryName (SettingsPath));

			Init ();
		}

		public static bool TryGetSystemXcode (out string path)
		{
			return XcodeLocator.TryGetSystemXcode (LoggingServiceLogger.Instance, out path);
		}

		public static void Init ()
		{
			string devroot = null, vplist = null, xcode = null;
			bool foundSdk = false;

			SetInvalid ();

			DeveloperRoot = Environment.GetEnvironmentVariable ("MD_APPLE_SDK_ROOT");
			if (!string.IsNullOrEmpty (DeveloperRoot))
				LoggingService.LogInfo (string.Format ("An Xcode location was specified in the environment variable 'MD_APPLE_SDK_ROOT': {0}", DeveloperRoot));

			if (string.IsNullOrEmpty (DeveloperRoot))
				DeveloperRoot = GetConfiguredSdkLocation ();

			if (string.IsNullOrEmpty (DeveloperRoot)) {
				foreach (var v in DefaultRoots) {
					if (ValidateSdkLocation (v, out xcode, out vplist, out devroot)) {
						foundSdk = true;
						break;
					}

					SdkNotInstalledReason += string.Format ("A valid Xcode installation was not found at '{0}'\n", v);
					LoggingService.LogInfo (SdkNotInstalledReason);
				}
			} else if (!ValidateSdkLocation (DeveloperRoot, out xcode, out vplist, out devroot)) {
				SdkNotInstalledReason = string.Format ("A valid Xcode installation was not found at the configured location: '{0}'", DeveloperRoot);
				LoggingService.LogError (SdkNotInstalledReason);
				SetInvalid ();
				return;
			} else {
				foundSdk = true;
			}

			if (foundSdk) {
				XcodePath = xcode;
				DeveloperRoot = devroot;
				DeveloperRootVersionPlist = vplist;
				Environment.SetEnvironmentVariable ("XCODE_DEVELOPER_DIR_PATH", DeveloperRoot);
			} else {
				SetInvalid ();
				return;
			}

			try {
				var plist = Path.Combine (XcodePath, "Contents", "Info.plist");

				if (!File.Exists (plist)) {
					SetInvalid ();
					return;
				}

				lastWritten = File.GetLastWriteTimeUtc (plist);

				XcodeVersion = new Version (3, 2, 6);
				XcodeRevision = "0";

				// DTXCode was introduced after xcode 3.2.6 so it may not exist
				var dict = PDictionary.FromFile (plist);

				PString value;
				if (dict.TryGetValue ("DTXcode", out value))
					DTXcode = value.Value;

				if (dict.TryGetValue ("CFBundleShortVersionString", out value))
					XcodeVersion = Version.Parse (value.Value);

				if (dict.TryGetValue ("CFBundleVersion", out value))
					XcodeRevision = value.Value;

				LoggingService.LogInfo ("Found Xcode, version {0} ({1}).", XcodeVersion, XcodeRevision);
				AnalyticsService.ReportSdkVersion ("XS.Core.SDK.Xcode.Version", XcodeVersion.ToString ());
				IsValid = true;
			} catch (Exception ex) {
				SdkNotInstalledReason = string.Format ("Error loading Xcode information for prefix '" + DeveloperRoot + "'");
				LoggingService.LogError (SdkNotInstalledReason, ex);
				SetInvalid ();
			}
		}

		public static string DeveloperRoot { get; private set; }

		public static string InvalidDeveloperRoot { get; private set; }

		public static string DeveloperRootVersionPlist {
			get; private set;
		}

		public static string XcodePath {
			get; private set;
		}

		[Obsolete ("This method does nothing")]
		public static void CheckChanged ()
		{
			//var plist = Path.Combine (XcodePath, "Contents", "Info.plist");
			//var mtime = DateTime.MinValue;

			//if (File.Exists (plist))
			//	mtime = File.GetLastWriteTimeUtc (plist);

			//if (mtime != lastWritten) {
			//	Init ();
			//	Changed ();
			//}
		}

		public static bool IsValid { get; private set; }
		public static string DTXcode { get; private set; }

		public static Version XcodeVersion { get; private set; }
		public static string XcodeRevision { get; private set; }

#pragma warning disable 0067
		[Obsolete ("This event is never raised")]
		public static event Action Changed;
#pragma warning restore 0067
	}
}
