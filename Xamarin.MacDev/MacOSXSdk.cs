// 
// MacOSXSdk.cs
//  
// Author: Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc.
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
	public class MacOSXSdk : IAppleSdk {
		List<AppleSdkVersion> knownOSVersions = new List<AppleSdkVersion> {
			AppleSdkVersion.V10_7,
			AppleSdkVersion.V10_8,
			AppleSdkVersion.V10_9,
			AppleSdkVersion.V10_10,
			AppleSdkVersion.V10_11,
			AppleSdkVersion.V10_12,
			AppleSdkVersion.V10_13,
			AppleSdkVersion.V10_14,
			};

		static readonly Dictionary<string, AppleDTSdkSettings> sdkSettingsCache = new Dictionary<string, AppleDTSdkSettings> ();
		static DTSettings dtSettings;

		public string DeveloperRoot { get; private set; }
		public string VersionPlist { get; private set; }
		public string DesktopPlatform { get; private set; }
		public string SdkDeveloperRoot { get; private set; }

		const string PLATFORM_VERSION_PLIST = "version.plist";
		const string SYSTEM_VERSION_PLIST = "/System/Library/CoreServices/SystemVersion.plist";

		public MacOSXSdk (string developerRoot, string versionPlist)
		{
			var platformDir = Path.Combine (developerRoot, "Platforms", "MacOSX.platform");
			var sdkDir = Path.Combine (platformDir, "Developer", "SDKs");

			if (Directory.Exists (sdkDir))
				SdkDeveloperRoot = Path.Combine (platformDir, "Developer");
			else
				SdkDeveloperRoot = developerRoot;

			DesktopPlatform = platformDir;
			DeveloperRoot = developerRoot;
			VersionPlist = versionPlist;

			Init ();

			if (InstalledSdkVersions.Length > 0) {
				// Enumerable.Union is there to ensure we add the latest installed sdk to the list (even if it's not in knownOSVersions)
				knownOSVersions = knownOSVersions.Where (x => x < InstalledSdkVersions [0]).Union (InstalledSdkVersions).ToList ();
			}
		}

		void Init ()
		{
			IsInstalled = File.Exists (Path.Combine (DesktopPlatform, "Info.plist"));
			if (IsInstalled) {
				InstalledSdkVersions = EnumerateSdks (Path.Combine (SdkDeveloperRoot, "SDKs"), "MacOSX");
			} else {
				InstalledSdkVersions = new AppleSdkVersion [0];
			}
		}

		public bool IsInstalled { get; private set; }
		public AppleSdkVersion [] InstalledSdkVersions { get; private set; }

		public IList<AppleSdkVersion> KnownOSVersions { get { return knownOSVersions; } }

		static AppleSdkVersion [] EnumerateSdks (string sdkDir, string name)
		{
			if (!Directory.Exists (sdkDir))
				return new AppleSdkVersion [0];

			var sdks = new List<string> ();

			foreach (var dir in Directory.GetDirectories (sdkDir)) {
				string dirName = Path.GetFileName (dir);
				if (!dirName.StartsWith (name, StringComparison.Ordinal) || !dirName.EndsWith (".sdk", StringComparison.Ordinal))
					continue;

				if (!File.Exists (Path.Combine (dir, "SDKSettings.plist")))
					continue;

				int verLength = dirName.Length - (name.Length + ".sdk".Length);
				if (verLength == 0)
					continue;

				dirName = dirName.Substring (name.Length, verLength);
				sdks.Add (dirName);
			}

			var vs = new List<AppleSdkVersion> ();
			foreach (var s in sdks) {
				try {
					vs.Add (AppleSdkVersion.Parse (s));
				} catch (Exception ex) {
					LoggingService.LogError ("Could not parse {0} SDK version '{1}':\n{2}", name, s, ex);
				}
			}

			var versions = vs.ToArray ();
			Array.Sort (versions);

			return versions;
		}

		public string GetPlatformPath ()
		{
			return DesktopPlatform;
		}

		string IAppleSdk.GetPlatformPath (bool isSimulator)
		{
			return GetPlatformPath ();
		}

		public string GetSdkPath (AppleSdkVersion version)
		{
			return GetSdkPath (version.ToString ());
		}

		public string GetSdkPath (string version)
		{
			return Path.Combine (SdkDeveloperRoot, "SDKs", "MacOSX" + version + ".sdk");
		}

		string IAppleSdk.GetSdkPath (string version, bool isSimulator)
		{
			return GetSdkPath (version);
		}

		public string GetSdkPath (bool isSimulator)
		{
			return GetSdkPath (string.Empty);
		}

		public string GetSdkPath ()
		{
			return GetSdkPath (string.Empty);
		}

		string GetSdkPlistFilename (string version)
		{
			return Path.Combine (GetSdkPath (version), "SDKSettings.plist");
		}

		Dictionary<string, string> catalyst_version_map_ios_to_macos;
		Dictionary<string, string> catalyst_version_map_macos_to_ios;

		void LoadCatalystVersionMaps (string version)
		{
			if (catalyst_version_map_ios_to_macos != null && catalyst_version_map_macos_to_ios != null)
				return;

			MacCatalystSupport.LoadVersionMaps (GetSdkPath (version), out catalyst_version_map_ios_to_macos, out catalyst_version_map_macos_to_ios);
		}

		public Dictionary<string, string> GetCatalystVersionMap_iOS_to_Mac (string version)
		{
			LoadCatalystVersionMaps (version);
			return catalyst_version_map_ios_to_macos;
		}

		public Dictionary<string, string> GetCatalystVersionMap_Mac_to_iOS (string version)
		{
			LoadCatalystVersionMaps (version);
			return catalyst_version_map_macos_to_ios;
		}

		bool IAppleSdk.SdkIsInstalled (IAppleSdkVersion version, bool isSimulator)
		{
			return SdkIsInstalled ((AppleSdkVersion) version);
		}

		public bool SdkIsInstalled (AppleSdkVersion version)
		{
			foreach (var v in InstalledSdkVersions) {
				if (v.Equals (version))
					return true;
			}

			return false;
		}

		[Obsolete ("Use the 'GetSdkSettings (IAppleSdkVersion)' overload instead.")]
		public DTSdkSettings GetSdkSettings (MacOSXSdkVersion sdk)
		{
			var settings = GetSdkSettings ((IAppleSdkVersion) sdk);
			return new DTSdkSettings {
				AlternateSDK = settings.AlternateSDK,
				CanonicalName = settings.CanonicalName,
				DTCompiler = settings.DTCompiler,
				DTSDKBuild = settings.DTSDKBuild,
			};
		}

		public AppleDTSdkSettings GetSdkSettings (IAppleSdkVersion sdk, bool isSimulator)
		{
			return GetSdkSettings (sdk);
		}

		public AppleDTSdkSettings GetSdkSettings (IAppleSdkVersion sdk)
		{
			Dictionary<string, AppleDTSdkSettings> cache = sdkSettingsCache;

			if (cache.TryGetValue (sdk.ToString (), out var settings))
				return settings;

			try {
				settings = LoadSdkSettings (sdk);
			} catch (Exception ex) {
				LoggingService.LogError (string.Format ("Error loading settings for SDK MacOSX {0}", sdk), ex);
			}

			cache [sdk.ToString ()] = settings;

			return settings;
		}

		AppleDTSdkSettings LoadSdkSettings (IAppleSdkVersion sdk)
		{
			string filename = GetSdkPlistFilename (sdk.ToString ());

			if (!File.Exists (filename))
				return null;

			var plist = PDictionary.FromFile (filename);
			var settings = new AppleDTSdkSettings ();

			settings.CanonicalName = plist.GetString ("CanonicalName").Value;

			var props = plist.Get<PDictionary> ("DefaultProperties");

			PString gcc;
			if (!props.TryGetValue<PString> ("GCC_VERSION", out gcc))
				settings.DTCompiler = "com.apple.compilers.llvm.clang.1_0";
			else
				settings.DTCompiler = gcc.Value;

			var sdkPath = GetSdkPath (sdk.ToString ());
			// Do not do 'Path.Combine (sdkPath, SYSTEM_VERSION_PLIST)', because SYSTEM_VERSION_PLIST starts with a slash,
			// and thus Path.Combine wouldn't combine, just return SYSTEM_VERSION_PLIST.
			settings.DTSDKBuild = GrabRootString (sdkPath + SYSTEM_VERSION_PLIST, "ProductBuildVersion");

			return settings;
		}

		[Obsolete ("Use GetAppleDTSettings instead")]
		public DTSettings GetDTSettings ()
		{
			return GetSettings ();
		}

		DTSettings GetSettings ()
		{
			if (dtSettings != null)
				return dtSettings;

			var dict = PDictionary.FromFile (Path.Combine (DesktopPlatform, "Info.plist"));
			var infos = dict.Get<PDictionary> ("AdditionalInfo");

			return (dtSettings = new DTSettings {
				DTPlatformVersion = infos.Get<PString> ("DTPlatformVersion").Value,
				DTPlatformBuild = GrabRootString (Path.Combine (DesktopPlatform, "version.plist"), "ProductBuildVersion") ?? GrabRootString (VersionPlist, "ProductBuildVersion"),
				DTXcodeBuild = GrabRootString (VersionPlist, "ProductBuildVersion"),
				BuildMachineOSBuild = GrabRootString (SYSTEM_VERSION_PLIST, "ProductBuildVersion"),
			});
		}

		public AppleDTSettings GetAppleDTSettings ()
		{
			var settings = GetSettings ();
			return new AppleDTSettings {
				DTPlatformBuild = settings.DTPlatformBuild,
				DTPlatformVersion = settings.DTPlatformVersion,
				BuildMachineOSBuild = settings.BuildMachineOSBuild,
				DTXcodeBuild = settings.DTXcodeBuild,
			};
		}

		static string GrabRootString (string file, string key)
		{
			if (!File.Exists (file))
				return null;

			var dict = PDictionary.FromFile (file);
			PString value;

			if (dict.TryGetValue<PString> (key, out value))
				return value.Value;

			return null;
		}

		IAppleSdkVersion IAppleSdk.GetClosestInstalledSdk (IAppleSdkVersion version, bool isSimulator)
		{
			return GetClosestInstalledSdk ((AppleSdkVersion) version);
		}

		public AppleSdkVersion GetClosestInstalledSdk (AppleSdkVersion v)
		{
			// sorted low to high, so get first that's >= requested version
			foreach (var i in GetInstalledSdkVersions ()) {
				if (i.CompareTo (v) >= 0)
					return i;
			}
			return AppleSdkVersion.UseDefault;
		}

		IList<IAppleSdkVersion> IAppleSdk.GetInstalledSdkVersions (bool isSimulator)
		{
			return GetInstalledSdkVersions ().Cast<IAppleSdkVersion> ().ToArray ();
		}

		public IList<AppleSdkVersion> GetInstalledSdkVersions ()
		{
			return InstalledSdkVersions;
		}

		bool IAppleSdk.TryParseSdkVersion (string value, out IAppleSdkVersion version)
		{
			return IAppleSdkVersion_Extensions.TryParse<AppleSdkVersion> (value, out version);
		}

		public class DTSettings {
			public string DTXcodeBuild { get; set; }
			public string DTPlatformVersion { get; set; }
			public string DTPlatformBuild { get; set; }
			public string BuildMachineOSBuild { get; set; }
		}

		public class DTSdkSettings {
			public string CanonicalName { get; set; }
			public string AlternateSDK { get; set; }
			public string DTCompiler { get; set; }
			public string DTSDKBuild { get; set; }
		}
	}
}
