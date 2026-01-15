//
// XamMacSdk.cs
//
// Authors:
//   Aaron Bockover <abock@xamarin.com>
//   Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright 2012-2014 Xamarin Inc.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

// Disable until we get around to enable + fix any issues.
#nullable disable

namespace Xamarin.MacDev {
	public sealed class XamMacSdk : IMonoMacSdk {
		static readonly AppleSdkVersion [] MacOSXSdkVersions = {
			AppleSdkVersion.V10_7,
			AppleSdkVersion.V10_8,
			AppleSdkVersion.V10_9,
			AppleSdkVersion.V10_10,
			AppleSdkVersion.V10_11,
			AppleSdkVersion.V10_12
		};

		readonly Dictionary<string, DateTime> lastWriteTimes = new Dictionary<string, DateTime> ();

		string monoMacAppLauncherPath;
		PDictionary versions;
		ExtendedVersion extended_version;

		public bool IsInstalled { get; private set; }

		public AppleSdkVersion Version { get; private set; }
		public string SdkNotInstalledReason { get; private set; }

		public string FrameworkDirectory { get; private set; }
		public string MmpPath { get; private set; }
		public string BmacPath { get; private set; }
		public string UnifiedFullProfileFrameworkAssembly { get; private set; }

		#region Compat paths

		public string LegacyFrameworkAssembly { get; private set; }
		public string LegacyAppLauncherPath { get; private set; }

		#endregion

		public XamMacSdk (string monoMacAppLauncherPath)
		{
			this.monoMacAppLauncherPath = monoMacAppLauncherPath;
			Init ();
		}

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

			knownVersions.Add ("macOS", CreateKnownSdkVersionsArray (MacOSXSdkVersions));

			return knownVersions;
		}

		static PDictionary CreateMinExtensionVersions ()
		{
			var minExtensionVersions = new PDictionary ();
			var mac = new PDictionary ();

			mac.Add ("com.apple.FinderSync", new PString ("10.10"));
			mac.Add ("com.apple.share-services", new PString ("10.10"));
			mac.Add ("com.apple.widget-extension", new PString ("10.10"));
			mac.Add ("com.apple.networkextension.packet-tunnel", new PString ("10.11"));

			minExtensionVersions.Add ("macOS", mac);

			return minExtensionVersions;
		}

		static PArray CreateDefaultFeatures (AppleSdkVersion version)
		{
			var features = new PArray ();

			if (version >= new AppleSdkVersion (1, 9, 0))
				features.Add (new PString ("activation"));
			if (version >= new AppleSdkVersion (1, 11, 0) && version < new AppleSdkVersion (2, 5, 0))
				features.Add (new PString ("ref-counting"));
			if (version >= new AppleSdkVersion (2, 5, 0))
				features.Add (new PString ("modern-http-client"));
			if (version >= new AppleSdkVersion (2, 10, 0))
				features.Add (new PString ("mono-symbol-archive"));

			return features;
		}

		static PDictionary CreateDefaultVersionsPlist (AppleSdkVersion version)
		{
			var versions = new PDictionary ();

			versions.Add ("KnownVersions", CreateKnownVersions ());
			versions.Add ("RecommendedXcodeVersion", new PString ("8.0"));
			versions.Add ("MinExtensionVersion", CreateMinExtensionVersions ());
			versions.Add ("Features", CreateDefaultFeatures (version));

			return versions;
		}

		void Init ()
		{
			string currentLocation = IsInstalled ? MmpPath : null;

			IsInstalled = false;
			versions = null;

			FrameworkDirectory = "/Library/Frameworks/Xamarin.Mac.framework/Versions/Current";
			var envFrameworkDir = Environment.GetEnvironmentVariable ("XAMMAC_FRAMEWORK_PATH");
			if (envFrameworkDir != null && Directory.Exists (envFrameworkDir))
				FrameworkDirectory = envFrameworkDir;

			var versionPath = Path.Combine (FrameworkDirectory, "Version");
			if (File.Exists (versionPath)) {
				Version = ReadVersion (versionPath);
				lastWriteTimes [versionPath] = File.GetLastWriteTimeUtc (versionPath);

				var path = Path.Combine (FrameworkDirectory, "Versions.plist");
				if (File.Exists (path)) {
					try {
						versions = PDictionary.FromFile (path);
					} catch {
						LoggingService.LogWarning ("Xamarin.Mac installation is corrupt: invalid Versions.plist at {0}.", path);
					}
				}

				if (versions == null)
					versions = CreateDefaultVersionsPlist (Version);
			} else {
				NotInstalled (versionPath, error: false);
				AnalyticsService.ReportSdkVersion ("XS.Core.SDK.Mac.Version", string.Empty);
				return;
			}

			var paths = Version >= new AppleSdkVersion (1, 9, 0)
				? Detect2x ()
				: Detect1x ();

			foreach (var path in paths) {
				if (!File.Exists (path)) {
					NotInstalled (path);
					return;
				}

				lastWriteTimes [path] = File.GetLastWriteTimeUtc (path);
			}

			IsInstalled = true;
			LoggingService.LogInfo ("Found Xamarin.Mac, version {0}.", Version);
			AnalyticsService.ReportSdkVersion ("XS.Core.SDK.Mac.Version", Version.ToString ());

			if (Changed != null && currentLocation != MmpPath)
				Changed (this, EventArgs.Empty);
		}

		IEnumerable<string> Detect2x ()
		{
			yield return MmpPath = Path.Combine (FrameworkDirectory, "bin", "mmp");
			yield return BmacPath = Path.Combine (FrameworkDirectory, "bin", "bmac");
			yield return LegacyFrameworkAssembly = Path.Combine (FrameworkDirectory, "lib", "mono", "XamMac.dll");
			yield return LegacyAppLauncherPath = Path.Combine (FrameworkDirectory, "lib", "XamMacLauncher");
			yield return UnifiedFullProfileFrameworkAssembly = Path.Combine (FrameworkDirectory, "lib", "reference", "full", "Xamarin.Mac.dll");

			SupportsFullProfile =
				File.Exists (UnifiedFullProfileFrameworkAssembly) &&
				Version >= new AppleSdkVersion (1, 11, 2, 3);
		}

		IEnumerable<string> Detect1x ()
		{
			var usrPrefix = string.Empty;
			var appDriverPath = monoMacAppLauncherPath;

			// 1.2.13 began bundling its own launcher
			if (Version >= new AppleSdkVersion (1, 2, 13))
				appDriverPath = Path.Combine (FrameworkDirectory, "lib", "mono", "XamMacLauncher");

			if (Version < new AppleSdkVersion (1, 4, 0))
				usrPrefix = "usr";

			yield return MmpPath = Path.Combine (FrameworkDirectory, usrPrefix, "bin", "mmp");
			yield return BmacPath = Path.Combine (FrameworkDirectory, usrPrefix, "bin", "bmac");
			yield return LegacyFrameworkAssembly = Path.Combine (FrameworkDirectory, usrPrefix, "lib", "mono", "XamMac.dll");
			LegacyAppLauncherPath = appDriverPath;

			if (LegacyAppLauncherPath != null)
				yield return LegacyAppLauncherPath;

			yield break;
		}

		void NotInstalled (string pathMissing, bool error = true)
		{
			versions = new PDictionary ();
			lastWriteTimes.Clear ();
			IsInstalled = false;
			Version = new AppleSdkVersion ();

			SdkNotInstalledReason = string.Format ("Xamarin.Mac not installed. Can't find {0}.", pathMissing);

			if (error)
				LoggingService.LogError (SdkNotInstalledReason);
			else
				LoggingService.LogInfo (SdkNotInstalledReason);
		}

		AppleSdkVersion ReadVersion (string versionPath)
		{
			try {
				return AppleSdkVersion.Parse (File.ReadAllText (versionPath).Trim ());
			} catch (Exception ex) {
				LoggingService.LogError ("Failed to read Xamarin.Mac version", ex);
				return new AppleSdkVersion ();
			}
		}

		public void CheckCaches ()
		{
			try {
				foreach (var path in lastWriteTimes) {
					if (File.GetLastWriteTimeUtc (path.Key) != path.Value) {
						Init ();
						return;
					}
				}
			} catch (IOException) {
				Init ();
			}
		}

		bool CheckSupportsFeature (string feature)
		{
			PArray features;

			if (!versions.TryGetValue ("Features", out features)) {
				features = CreateDefaultFeatures (Version);
				versions.Add ("Features", features);
			}

			foreach (var item in features.OfType<PString> ().Select (x => x.Value)) {
				if (feature == item)
					return true;
			}

			return false;
		}

		public bool SupportsMSBuild {
			get { return Version >= new AppleSdkVersion (1, 9, 0); }
		}

		public bool SupportsV2Features {
			get { return Version >= new AppleSdkVersion (1, 9, 0); }
		}

		public bool SupportsNewStyleActivation {
			get { return Version >= new AppleSdkVersion (1, 9, 0); }
		}

		public bool SupportsFullProfile { get; private set; }

		public bool SupportsSGenConcurrentGC {
			get { return CheckSupportsFeature ("sgen-concurrent-gc"); }
		}

		public bool SupportsRefCounting {
			get { return CheckSupportsFeature ("ref-counting"); }
		}

		public bool SupportsHttpClientHandlers {
			get { return CheckSupportsFeature ("http-client-handlers"); }
		}

		public bool SupportsMonoSymbolArchive {
			get { return CheckSupportsFeature ("mono-symbol-archive"); }
		}

		public bool SupportsLinkPlatform {
			get { return CheckSupportsFeature ("link-platform"); }
		}

		public bool SupportsHybridAOT {
			get { return CheckSupportsFeature ("hybrid-aot"); }
		}

		public bool SupportsSiriIntents {
			get { return CheckSupportsFeature ("siri-intents"); }
		}

		public ExtendedVersion ExtendedVersion {
			get {
				if (extended_version == null) {
					extended_version = ExtendedVersion.Read (Path.Combine (FrameworkDirectory, "buildinfo"));
				}
				return extended_version;
			}
		}
	}
}
