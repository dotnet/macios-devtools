// 
// MonoTouchSdk.cs
//  
// Authors: Michael Hutchinson <mhutch@xamarin.com>
//          Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2011 Xamarin Inc.
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
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Disable until we get around to enable + fix any issues.
#nullable disable

namespace Xamarin.MacDev {
	public class MonoTouchSdk {
		static readonly AppleSdkVersion [] iOSSdkVersions = {
			AppleSdkVersion.V6_0,
			AppleSdkVersion.V6_1,
			AppleSdkVersion.V7_0,
			AppleSdkVersion.V7_1,
			AppleSdkVersion.V8_0,
			AppleSdkVersion.V8_1,
			AppleSdkVersion.V8_2,
			AppleSdkVersion.V8_3,
			AppleSdkVersion.V8_4,
			AppleSdkVersion.V9_0,
			AppleSdkVersion.V9_1,
			AppleSdkVersion.V9_2,
			AppleSdkVersion.V9_3,
			AppleSdkVersion.V10_0,
			AppleSdkVersion.V10_1
		};
		static readonly AppleSdkVersion [] watchOSSdkVersions = {
			AppleSdkVersion.V2_0,
			AppleSdkVersion.V2_1,
			AppleSdkVersion.V2_2,
			AppleSdkVersion.V3_0,
			AppleSdkVersion.V3_1
		};
		static readonly AppleSdkVersion [] tvOSSdkVersions = {
			AppleSdkVersion.V9_0,
			AppleSdkVersion.V9_1,
			AppleSdkVersion.V9_2,
			AppleSdkVersion.V10_0
		};

		public static readonly string [] DefaultLocations;

		// Note: We require Xamarin.iOS 10.4 because it is the first version to bundle mlaunch. Prior to that Xamarin Studio would ship it but that is no longer the case.
		// (We also need 10.4 for the Versions.plist, although we have a handy fallback if that file is missing)
		static AppleSdkVersion requiredXI = AppleSdkVersion.V10_4;

		DateTime lastMTExeWrite = DateTime.MinValue;
		PDictionary versions;
		string mtouchPath;

		static MonoTouchSdk ()
		{
			var locations = new List<string> ();
			var env = Environment.GetEnvironmentVariable ("MD_MTOUCH_SDK_ROOT");
			if (!string.IsNullOrEmpty (env))
				locations.Add (env);
			locations.Add ("/Library/Frameworks/Xamarin.iOS.framework/Versions/Current");
			DefaultLocations = locations.ToArray ();
		}

		public MonoTouchSdk (string sdkDir)
		{
			SdkDir = sdkDir;
			Init ();
		}

		public string SdkDir { get; private set; }

		public string BinDir {
			get {
				// mtouch is in the bin dir
				return Path.GetDirectoryName (mtouchPath);
			}
		}

		public string LibDir {
			get {
				// the lib dir is next to the bin dir
				return Path.Combine (Path.GetDirectoryName (BinDir), "lib");
			}
		}

		public string SdkNotInstalledReason { get; private set; }

		public event EventHandler Changed;

		static PArray CreateKnownSdkVersionsArray (IList<AppleSdkVersion> versions)
		{
			var array = new PArray ();

			for (int i = 0; i < versions.Count; i++)
				array.Add (new PString (versions [i].ToString ()));

			return array;
		}

		static PDictionary CreateKnownVersions ()
		{
			var knownVersions = new PDictionary ();

			knownVersions.Add ("iOS", CreateKnownSdkVersionsArray (iOSSdkVersions));
			knownVersions.Add ("tvOS", CreateKnownSdkVersionsArray (tvOSSdkVersions));
			knownVersions.Add ("watchOS", CreateKnownSdkVersionsArray (watchOSSdkVersions));

			return knownVersions;
		}

		static PDictionary CreateMinExtensionVersions ()
		{
			var minExtensionVersions = new PDictionary ();
			var watchos = new PDictionary ();
			var tvos = new PDictionary ();
			var ios = new PDictionary ();

			ios.Add ("com.apple.ui-services", new PString ("8.0"));
			ios.Add ("com.apple.services", new PString ("8.0"));
			ios.Add ("com.apple.keyboard-service", new PString ("8.0"));
			ios.Add ("com.apple.fileprovider-ui", new PString ("8.0"));
			ios.Add ("com.apple.fileprovider-nonui", new PString ("8.0"));
			ios.Add ("com.apple.photo-editing", new PString ("8.0"));
			ios.Add ("com.apple.share-services", new PString ("8.0"));
			ios.Add ("com.apple.widget-extension", new PString ("8.0"));
			ios.Add ("com.apple.watchkit", new PString ("8.0"));
			ios.Add ("com.apple.AudioUnit-UI", new PString ("8.0"));
			ios.Add ("com.apple.Safari.content-blocker", new PString ("9.0"));
			ios.Add ("com.apple.Safari.sharedlinks-service", new PString ("9.0"));
			ios.Add ("com.apple.spotlight.index", new PString ("9.0"));
			ios.Add ("com.apple.callkit.call-directory", new PString ("10.0"));
			ios.Add ("com.apple.intents-service", new PString ("10.0"));
			ios.Add ("com.apple.intents-ui-service", new PString ("10.0"));
			ios.Add ("com.apple.message-payload-provider", new PString ("10.0"));
			ios.Add ("com.apple.usernotifications.content-extension", new PString ("10.0"));
			ios.Add ("com.apple.usernotifications.service", new PString ("10.0"));
			ios.Add ("com.apple.authentication-services-credential-provider-ui", new PString ("12.0"));

			tvos.Add ("com.apple.broadcast-services", new PString ("10.0"));
			tvos.Add ("com.apple.tv-services", new PString ("9.0"));

			minExtensionVersions.Add ("iOS", ios);
			minExtensionVersions.Add ("tvOS", tvos);
			minExtensionVersions.Add ("watchOS", watchos);

			return minExtensionVersions;
		}

		static PDictionary CreateDefaultVersionsPlist ()
		{
			var versions = new PDictionary ();

			versions.Add ("KnownVersions", CreateKnownVersions ());
			versions.Add ("RecommendedXcodeVersion", new PString ("8.0"));
			versions.Add ("MinExtensionVersion", CreateMinExtensionVersions ());

			return versions;
		}

		void Init ()
		{
			string currentLocation = IsInstalled ? mtouchPath : null;

			IsInstalled = false;
			versions = null;

			if (string.IsNullOrEmpty (SdkDir)) {
				foreach (var loc in DefaultLocations) {
					if (IsInstalled = ValidateSdkLocation (loc, out mtouchPath)) {
						SdkDir = loc;
						break;
					}
				}
			} else {
				IsInstalled = ValidateSdkLocation (SdkDir, out mtouchPath);
			}

			if (IsInstalled) {
				lastMTExeWrite = File.GetLastWriteTimeUtc (mtouchPath);
				Version = ReadVersion ();

				if (Version.CompareTo (requiredXI) >= 0) {
					LoggingService.LogInfo ("Found Xamarin.iOS, version {0}.", Version);

					var path = Path.Combine (SdkDir, "Versions.plist");
					if (File.Exists (path)) {
						try {
							versions = PDictionary.FromFile (path);
						} catch {
							LoggingService.LogWarning ("Xamarin.iOS installation is corrupt: invalid Versions.plist at {0}.", path);
						}
					}

					if (versions == null)
						versions = CreateDefaultVersionsPlist ();
				} else {
					SdkNotInstalledReason = string.Format ("Found unsupported Xamarin.iOS, version {0}.\nYou need Xamarin.iOS {1} or above.", Version, requiredXI.ToString ());
					LoggingService.LogWarning (SdkNotInstalledReason);
					Version = new AppleSdkVersion ();
					versions = new PDictionary ();
					IsInstalled = false;
				}

				AnalyticsService.ReportSdkVersion ("XS.Core.SDK.iOS.Version", Version.ToString ());
			} else {
				lastMTExeWrite = DateTime.MinValue;
				Version = new AppleSdkVersion ();
				versions = new PDictionary ();

				SdkNotInstalledReason = string.Format ("Xamarin.iOS not installed.\nCan't find mtouch or the Version file at {0}.", SdkDir);
				LoggingService.LogInfo (SdkNotInstalledReason);

				AnalyticsService.ReportSdkVersion ("XS.Core.SDK.iOS.Version", string.Empty);
			}

			if (Changed != null && currentLocation != mtouchPath)
				Changed (this, EventArgs.Empty);
		}

		AppleSdkVersion ReadVersion ()
		{
			var versionFile = Path.Combine (SdkDir, "Version");

			if (File.Exists (versionFile)) {
				try {
					return AppleSdkVersion.Parse (File.ReadAllText (versionFile).Trim ());
				} catch (Exception ex) {
					LoggingService.LogError ("Failed to read Xamarin.iOS version", ex);
				}
			}

			return new AppleSdkVersion ();
		}

		public static bool ValidateSdkLocation (string sdkDir)
		{
			return ValidateSdkLocation (sdkDir, out string _);
		}

		public static bool ValidateSdkLocation (string sdkDir, out bool hasUsrSubdir)
		{
			hasUsrSubdir = false;
			return ValidateSdkLocation (sdkDir, out string _);
		}

		public static bool ValidateSdkLocation (string sdkDir, out string mtouchPath)
		{
			mtouchPath = null;

			if (!File.Exists (Path.Combine (sdkDir, "Version")))
				return false;

			var path = Path.Combine (sdkDir, "bin", "mtouch");
			if (File.Exists (path)) {
				mtouchPath = path;
				return true;
			}

			path = Path.Combine (sdkDir, "tools", "bin", "mtouch");
			if (File.Exists (path)) {
				mtouchPath = path;
				return true;
			}

			return false;
		}

		public bool IsInstalled { get; private set; }
		public AppleSdkVersion Version { get; private set; }

		ExtendedVersion extended_version;
		public ExtendedVersion ExtendedVersion {
			get {
				if (extended_version == null) {
					extended_version = ExtendedVersion.Read (Path.Combine (SdkDir, "buildinfo"));
					if (extended_version == null) {
						// 'buildinfo' doesn't work in a nuget package because of https://github.com/NuGet/Home/issues/8810, so use 'tools/buildinfo' instead.
						extended_version = ExtendedVersion.Read (Path.Combine (SdkDir, "tools", "buildinfo"));
					}
				}
				return extended_version;
			}
		}

		static string GetPlatformKey (PlatformName platform)
		{
			switch (platform) {
			case PlatformName.iOS: return "iOS";
			case PlatformName.WatchOS: return "watchOS";
			case PlatformName.TvOS: return "tvOS";
			case PlatformName.MacOSX: return "MacCatalyst";
			default: throw new ArgumentOutOfRangeException (nameof (platform));
			}
		}

		public Version RecommendedXcodeVersion {
			get {
				Version version;
				PString value;

				if (!versions.TryGetValue ("RecommendedXcodeVersion", out value) || !System.Version.TryParse (value.Value, out version))
					return new Version (8, 0);

				return version;
			}
		}

		public IList<AppleSdkVersion> GetKnownSdkVersions (PlatformName platform)
		{
			var list = new List<AppleSdkVersion> ();
			var key = GetPlatformKey (platform);
			PDictionary knownVersions;

			if (versions.TryGetValue ("KnownVersions", out knownVersions)) {
				PArray array;

				if (knownVersions.TryGetValue (key, out array)) {
					versions.TryGetValue ("MacCatalystVersionMap", out PDictionary macCatalystVersionMap);

					foreach (var knownVersion in array.OfType<PString> ()) {
						string versionValue = knownVersion.Value;

						// For MacCatalyst we need to convert the versions to supported versions using the map
						if (platform == PlatformName.MacOSX) {
							if (macCatalystVersionMap != null) {
								if (macCatalystVersionMap.TryGetValue (knownVersion, out PString value))
									versionValue = value.Value;
							}
						}

						if (AppleSdkVersion.TryParse (versionValue, out var version))
							list.Add (version);
					}
				}
			}

			return list;
		}

		public AppleSdkVersion GetMinimumExtensionVersion (PlatformName platform, string extension)
		{
			var key = GetPlatformKey (platform);
			PDictionary minExtensionVersions;

			if (versions.TryGetValue ("MinExtensionVersion", out minExtensionVersions)) {
				PDictionary extensions;

				if (minExtensionVersions.TryGetValue (key, out extensions)) {
					PString value;

					if (extensions.TryGetValue (extension, out value)) {
						if (AppleSdkVersion.TryParse (value.Value, out var version))
							return version;
					}
				}
			}

			return AppleSdkVersion.V8_0;
		}

		public void CheckCaches ()
		{
			if (IsInstalled) {
				try {
					var lastWrite = File.GetLastWriteTimeUtc (mtouchPath);
					if (lastWrite == lastMTExeWrite)
						return;
				} catch (IOException) {
				}
			}

			Init ();
		}

		public bool SupportsFeature (string feature)
		{
			PArray features;

			if (!versions.TryGetValue ("Features", out features))
				return false;

			foreach (var item in features.OfType<PString> ().Select (x => x.Value)) {
				if (feature == item)
					return true;
			}

			return false;
		}

		public bool SupportsSGenConcurrentGC {
			get { return SupportsFeature ("sgen-concurrent-gc"); }
		}

		public bool SupportsLaunchDeviceBundleId {
			get { return SupportsFeature ("mlaunch-launchdevbundleid"); }
		}

		public bool SupportsObserveExtension {
			get { return SupportsFeature ("mlaunch-observe-extension"); }
		}

		public bool SupportsLaunchSimulator {
			get { return SupportsFeature ("mlaunch-launch-simulator"); }
		}

		public bool SupportsInstallProgress {
			get { return SupportsFeature ("mlaunch-install-progress"); }
		}

		public bool SupportsLaunchWatchOSComplications {
			get { return SupportsFeature ("mlaunch-watchos-complications"); }
		}

		public bool SupportsWirelessDevices {
			get { return SupportsFeature ("mlaunch-wireless-devices"); }
		}

		public bool SupportsSiriIntents {
			get { return SupportsFeature ("siri-intents"); }
		}

		public bool SupportsArm64_32 {
			get { return SupportsFeature ("arm64_32"); }
		}

		public bool SupportsAltool {
			get { return SupportsFeature ("altool"); }
		}
	}
}
