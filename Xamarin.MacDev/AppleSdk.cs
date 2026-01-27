// 
// AppleSdk.cs
//  
// Authors: Rolf Bjarne Kvinge <rolf@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc.
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
	public abstract class AppleSdk : IAppleSdk {
		public string DeveloperRoot { get; protected set; }
		public string VersionPlist { get; protected set; }

		protected abstract string DevicePlatformName { get; }
		protected abstract string SimulatorPlatformName { get; }

		public string DevicePlatform { get { return Path.Combine (DeveloperRoot, "Platforms/" + DevicePlatformName + ".platform"); } }
		public string SimPlatform { get { return Path.Combine (DeveloperRoot, "Platforms/" + SimulatorPlatformName + ".platform"); } }

		public bool IsInstalled { get; private set; }
		public AppleSdkVersion [] InstalledSdkVersions { get; private set; }
		public AppleSdkVersion [] InstalledSimVersions { get; private set; }

		readonly Dictionary<string, AppleDTSdkSettings> sdkSettingsCache = new Dictionary<string, AppleDTSdkSettings> ();
		readonly Dictionary<string, AppleDTSdkSettings> simSettingsCache = new Dictionary<string, AppleDTSdkSettings> ();
		AppleDTSettings dtSettings;

		const string PLATFORM_VERSION_PLIST = "version.plist";
		const string SYSTEM_VERSION_PLIST = "/System/Library/CoreServices/SystemVersion.plist";

		protected void Init ()
		{
			IsInstalled = File.Exists (Path.Combine (DevicePlatform, "Info.plist"));

			if (IsInstalled) {
				File.GetLastWriteTimeUtc (VersionPlist);
				InstalledSdkVersions = EnumerateSdks (Path.Combine (DevicePlatform, "Developer/SDKs"), DevicePlatformName);
				InstalledSimVersions = EnumerateSdks (Path.Combine (SimPlatform, "Developer/SDKs"), SimulatorPlatformName);
			} else {
				InstalledSdkVersions = new AppleSdkVersion [0];
				InstalledSimVersions = new AppleSdkVersion [0];
			}
		}


		public string GetPlatformPath (bool sim)
		{
			return sim ? SimPlatform : DevicePlatform;
		}

		public string GetSdkPath (IAppleSdkVersion version, bool sim)
		{
			return GetSdkPath (version.ToString (), sim);
		}

		public string GetSdkPath (string version, bool sim)
		{
			if (sim)
				return Path.Combine (SimPlatform, "Developer/SDKs/" + SimulatorPlatformName + version + ".sdk");

			return Path.Combine (DevicePlatform, "Developer/SDKs/" + DevicePlatformName + version + ".sdk");
		}

		public string GetSdkPath (bool isSimulator)
		{
			return GetSdkPath (string.Empty, isSimulator);
		}

		public string GetSdkPath ()
		{
			throw new InvalidOperationException ($"This AppleSdk requires specifying whether we're targeting the simulator or not, so please use the other overload taking a 'bool isSimulator' parameter.");
		}

		public string GetSdkPath (string version)
		{
			throw new InvalidOperationException ($"This AppleSdk requires specifying whether we're targeting the simulator or not, so please use the other overload taking a 'bool isSimulator' parameter.");
		}

		string GetSdkPlistFilename (string version, bool sim)
		{
			return Path.Combine (GetSdkPath (version, sim), "SDKSettings.plist");
		}

		bool IAppleSdk.SdkIsInstalled (IAppleSdkVersion version, bool isSimulator)
		{
			return SdkIsInstalled ((AppleSdkVersion) version, isSimulator);
		}

		public bool SdkIsInstalled (AppleSdkVersion version, bool sim)
		{
			foreach (var v in (sim ? InstalledSimVersions : InstalledSdkVersions))
				if (v.Equals (version))
					return true;
			return false;
		}

		public AppleDTSdkSettings GetSdkSettings (AppleSdkVersion sdk, bool isSim)
		{
			return GetSdkSettings ((IAppleSdkVersion) sdk, isSim);
		}

		public AppleDTSdkSettings GetSdkSettings (IAppleSdkVersion sdk, bool isSim)
		{
			var cache = isSim ? simSettingsCache : sdkSettingsCache;

			AppleDTSdkSettings settings;
			if (cache.TryGetValue (sdk.ToString (), out settings))
				return settings;

			try {
				settings = LoadSdkSettings (sdk, isSim);
			} catch (Exception ex) {
				var sdkName = isSim ? SimulatorPlatformName : DevicePlatformName;
				LoggingService.LogError (string.Format ("Error loading settings for SDK {0} {1}", sdkName, sdk), ex);
			}

			cache [sdk.ToString ()] = settings;
			return settings;
		}

		AppleDTSdkSettings LoadSdkSettings (IAppleSdkVersion sdk, bool isSim)
		{
			var settings = new AppleDTSdkSettings ();

			var plist = PDictionary.FromFile (GetSdkPlistFilename (sdk.ToString (), isSim));
			if (!isSim)
				settings.AlternateSDK = plist.GetString ("AlternateSDK").Value;

			settings.CanonicalName = plist.GetString ("CanonicalName").Value;

			var props = plist.Get<PDictionary> ("DefaultProperties");

			PString gcc;
			if (!props.TryGetValue<PString> ("GCC_VERSION", out gcc))
				settings.DTCompiler = "com.apple.compilers.llvm.clang.1_0";
			else
				settings.DTCompiler = gcc.Value;

			settings.DeviceFamilies = props.GetUIDeviceFamily ("SUPPORTED_DEVICE_FAMILIES");
			var plstPlist = Path.Combine (GetPlatformPath (isSim), PLATFORM_VERSION_PLIST);
			settings.DTSDKBuild = GrabRootString (plstPlist, "ProductBuildVersion");

			return settings;
		}

		public AppleDTSettings GetDTSettings ()
		{
			if (dtSettings != null)
				return dtSettings;

			var dict = PDictionary.FromFile (Path.Combine (DevicePlatform, "Info.plist"));
			var infos = dict.Get<PDictionary> ("AdditionalInfo");

			return (dtSettings = new AppleDTSettings {
				DTPlatformVersion = infos.Get<PString> ("DTPlatformVersion").Value,
				DTPlatformBuild = GrabRootString (Path.Combine (DevicePlatform, "version.plist"), "ProductBuildVersion"),
				DTXcodeBuild = GrabRootString (VersionPlist, "ProductBuildVersion"),
				BuildMachineOSBuild = GrabRootString (SYSTEM_VERSION_PLIST, "ProductBuildVersion"),
			});
		}

		public AppleDTSettings GetAppleDTSettings ()
		{
			return GetDTSettings ();
		}

		IAppleSdkVersion IAppleSdk.GetClosestInstalledSdk (IAppleSdkVersion version, bool isSimulator)
		{
			return GetClosestInstalledSdk ((AppleSdkVersion) version, isSimulator);
		}

		public AppleSdkVersion GetClosestInstalledSdk (AppleSdkVersion v, bool sim)
		{
			//sorted low to high, so get first that's >= requested version
			foreach (var i in GetInstalledSdkVersions (sim)) {
				if (i.CompareTo (v) >= 0)
					return i;
			}
			return AppleSdkVersion.UseDefault;
		}

		IList<IAppleSdkVersion> IAppleSdk.GetInstalledSdkVersions (bool isSimulator)
		{
			return GetInstalledSdkVersions (isSimulator).Cast<IAppleSdkVersion> ().ToArray ();
		}

		public IList<AppleSdkVersion> GetInstalledSdkVersions (bool sim)
		{
			return sim ? InstalledSimVersions : InstalledSdkVersions;
		}

		protected static AppleSdkVersion [] EnumerateSdks (string sdkDir, string name)
		{
			if (!Directory.Exists (sdkDir))
				return new AppleSdkVersion [0];

			var sdks = new List<string> ();

			foreach (var dir in Directory.GetDirectories (sdkDir)) {
				if (!File.Exists (Path.Combine (dir, "SDKSettings.plist")))
					continue;

				string d = Path.GetFileName (dir);
				if (!d.StartsWith (name, StringComparison.Ordinal))
					continue;

				d = d.Substring (name.Length);
				if (!d.EndsWith (".sdk", StringComparison.Ordinal))
					continue;

				d = d.Substring (0, d.Length - ".sdk".Length);
				if (d.Length > 0)
					sdks.Add (d);
			}

			var vs = new List<AppleSdkVersion> ();
			foreach (var s in sdks) {
				try {
					vs.Add (AppleSdkVersion.Parse (s));
				} catch (Exception ex) {
					LoggingService.LogError ("Could not parse {0} SDK version '{1}':\n{2}", name, s, ex.ToString ());
				}
			}

			var versions = vs.ToArray ();
			Array.Sort (versions);
			return versions;
		}

		protected static string GrabRootString (string file, string key)
		{
			if (!File.Exists (file))
				return null;

			var dict = PDictionary.FromFile (file);
			PString value;

			if (dict.TryGetValue<PString> (key, out value))
				return value.Value;

			return null;
		}

		bool IAppleSdk.TryParseSdkVersion (string value, out IAppleSdkVersion version)
		{
			return IAppleSdkVersion_Extensions.TryParse<AppleSdkVersion> (value, out version);
		}
	}
}
