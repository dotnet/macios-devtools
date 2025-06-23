// 
// MobileProvision.cs
//  
// Author:
//       Michael Hutchinson <mhutchinson@novell.com>
// 
// Copyright (c) 2009 Novell, Inc. (http://www.novell.com)
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
using System.Security.Cryptography.X509Certificates;

namespace Xamarin.MacDev {
	public enum MobileProvisionPlatform {
		MacOS,
		iOS,
		tvOS
	}

	[Flags]
	public enum MobileProvisionDistributionType {
		Any = 0,
		Development = 1 << 0,
		AdHoc = 1 << 1,
		InHouse = 1 << 2,
		AppStore = 1 << 3,
	}

	public class MobileProvision {
		public const string AutomaticAppStore = "Automatic:AppStore";
		public const string AutomaticInHouse = "Automatic:InHouse";
		public const string AutomaticAdHoc = "Automatic:AdHoc";
		public static readonly string [] ProfileDirectories;

		static MobileProvision ()
		{
			if (Environment.OSVersion.Platform == PlatformID.MacOSX
				|| Environment.OSVersion.Platform == PlatformID.Unix) {
				string personal = Environment.GetFolderPath (Environment.SpecialFolder.UserProfile);

				ProfileDirectories = new string [] {
					// Xcode >= 16.x uses ~/Library/Developer/Xcode/UserData/Provisioning Profiles
					Path.Combine (personal, "Library", "Developer", "Xcode", "UserData", "Provisioning Profiles"),

					// Xcode < 16.x uses ~/Library/MobileDevice/Provisioning Profiles
					Path.Combine (personal, "Library", "MobileDevice", "Provisioning Profiles"),
				};
			} else {
				var appDataLocal = Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData);

				ProfileDirectories = new string [] {
					Path.Combine (appDataLocal, "Xamarin", "iOS", "Provisioning", "Profiles"),
					Path.Combine (appDataLocal, "maui", "iOS", "Provisioning", "Profiles")
				};
			}
		}

		public MobileProvisionDistributionType DistributionType {
			get {
				if (ProvisionedDevices != null) {
					PBoolean getTaskAllow;

					// MacOS does not have an AdHoc distribution type
					foreach (var platform in Platforms) {
						if (platform == MobileProvisionPlatform.MacOS)
							return MobileProvisionDistributionType.Development;
					}

					if (Entitlements.TryGetValue ("get-task-allow", out getTaskAllow) && getTaskAllow.Value)
						return MobileProvisionDistributionType.Development;

					return MobileProvisionDistributionType.AdHoc;
				}

				if (ProvisionsAllDevices.HasValue && ProvisionsAllDevices.Value)
					return MobileProvisionDistributionType.InHouse;

				return MobileProvisionDistributionType.AppStore;
			}
		}

		public static string GetFileExtension (MobileProvisionPlatform platform)
		{
			switch (platform) {
			case MobileProvisionPlatform.MacOS:
				return ".provisionprofile";
			case MobileProvisionPlatform.tvOS:
			case MobileProvisionPlatform.iOS:
			default:
				return ".mobileprovision";
			}
		}

		public static MobileProvision LoadFromFile (string fileName)
		{
			var data = File.ReadAllBytes (fileName);
			var m = new MobileProvision ();
			m.Load (PDictionary.FromBinaryXml (data));
			m.Data = data;

			if (m.Platforms == null) {
				switch (Path.GetExtension (fileName).ToLowerInvariant ()) {
				case ".provisionprofile":
					m.Platforms = new MobileProvisionPlatform [1];
					m.Platforms [0] = MobileProvisionPlatform.MacOS;
					break;
				case ".mobileprovision":
					m.Platforms = new MobileProvisionPlatform [1];
					m.Platforms [0] = MobileProvisionPlatform.iOS;
					break;
				}
			}

			LoggingService.LogInfo ($"Loaded provisioning profile from '{fileName}'");

			return m;
		}

		public static MobileProvision LoadFromData (byte [] data)
		{
			var m = new MobileProvision ();
			m.Load (PDictionary.FromBinaryXml (data));
			m.Data = data;
			return m;
		}

		public static IList<MobileProvision> GetAllInstalledProvisions (MobileProvisionPlatform platform)
		{
			return GetAllInstalledProvisions (platform, false);
		}

		/// <summary>
		/// All installed provisioning profiles, sorted by newest first.
		/// </summary>
		public static IList<MobileProvision> GetAllInstalledProvisions (MobileProvisionPlatform platform, bool includeExpired)
		{
			var uuids = new Dictionary<string, MobileProvision> ();
			var list = new List<MobileProvision> ();
			var now = DateTime.Now;
			string pattern;

			switch (platform) {
			case MobileProvisionPlatform.MacOS:
				pattern = "*.provisionprofile";
				break;
			case MobileProvisionPlatform.tvOS:
			case MobileProvisionPlatform.iOS:
				pattern = "*.mobileprovision";
				break;
			default:
				throw new ArgumentOutOfRangeException (nameof (platform));
			}

			foreach (var profileDir in ProfileDirectories) {
				if (!Directory.Exists (profileDir))
					continue;

				foreach (var file in Directory.EnumerateFiles (profileDir, pattern)) {
					try {
						var data = File.ReadAllBytes (file);

						var m = new MobileProvision ();
						m.Load (PDictionary.FromBinaryXml (data));
						m.Data = data;

						if (includeExpired || m.ExpirationDate > now) {
							if (uuids.ContainsKey (m.Uuid)) {
								// we always want the most recently created/updated provision
								if (m.CreationDate > uuids [m.Uuid].CreationDate) {
									int index = list.IndexOf (uuids [m.Uuid]);
									uuids [m.Uuid] = m;
									list [index] = m;
								}
							} else {
								uuids.Add (m.Uuid, m);
								list.Add (m);
							}
						}
					} catch (Exception ex) {
						LoggingService.LogWarning ("Error reading " + platform + " provision file '" + file + "'", ex);
					}
				}
			}

			// newest first
			list.Sort ((x, y) => y.CreationDate.CompareTo (x.CreationDate));

			return list;
		}

		MobileProvision ()
		{
		}

		static IList<X509Certificate2> GetCertificates (PArray array)
		{
			var list = new List<X509Certificate2> ();

			foreach (var item in array) {
				var data = item as PData;

				if (data != null)
					list.Add (new X509Certificate2 (data.Value));
			}

			return list;
		}

		static IList<string> GetStrings (PArray array)
		{
			var list = new List<string> ();

			foreach (var item in array) {
				var str = item as PString;

				if (str != null)
					list.Add (str.Value);
			}

			return list;
		}

		void Load (PDictionary doc)
		{
			PArray prefixes;
			if (doc.TryGetValue ("ApplicationIdentifierPrefix", out prefixes) && prefixes != null)
				ApplicationIdentifierPrefix = GetStrings (prefixes);

			if (doc.TryGetValue ("TeamIdentifier", out prefixes) && prefixes != null)
				TeamIdentifierPrefix = GetStrings (prefixes);

			PDate creationDate;
			if (doc.TryGetValue ("CreationDate", out creationDate) && creationDate != null)
				CreationDate = creationDate.Value;

			PArray devCerts;
			if (doc.TryGetValue ("DeveloperCertificates", out devCerts) && devCerts != null)
				DeveloperCertificates = GetCertificates (devCerts);

			PDictionary entitlements;
			if (doc.TryGetValue ("Entitlements", out entitlements) && entitlements != null) {
				// Note: we clone the entitlements so that the runtime can garbage collect the 'doc' dictionary
				// (which may be rather huge if it contains a massive list of developer certs, for example)
				Entitlements = (PDictionary) entitlements.Clone ();
			}

			PDate expirationDate;
			if (doc.TryGetValue ("ExpirationDate", out expirationDate) && expirationDate != null)
				ExpirationDate = expirationDate.Value;

			PString name;
			if (doc.TryGetValue ("Name", out name) && name != null)
				Name = name.Value;

			PArray array;
			if (doc.TryGetValue ("Platform", out array) && array != null) {
				var platforms = new List<MobileProvisionPlatform> ();

				foreach (var platform in array.OfType<PString> ()) {
					switch (platform) {
					case "OSX": platforms.Add (MobileProvisionPlatform.MacOS); break;
					case "tvOS": platforms.Add (MobileProvisionPlatform.tvOS); break;
					case "iOS": platforms.Add (MobileProvisionPlatform.iOS); break;
					}
				}

				Platforms = platforms.ToArray ();
			}

			PArray provDevs;
			if (doc.TryGetValue ("ProvisionedDevices", out provDevs) && provDevs != null)
				ProvisionedDevices = GetStrings (provDevs);

			PBoolean provisionsAllDevices;
			if (doc.TryGetValue ("ProvisionsAllDevices", out provisionsAllDevices) && provisionsAllDevices != null)
				ProvisionsAllDevices = provisionsAllDevices.Value;

			PNumber ttl;
			if (doc.TryGetValue ("TimeToLive", out ttl) && ttl != null)
				TimeToLive = (int) ttl.Value;

			PString uuid;
			if (doc.TryGetValue ("UUID", out uuid) && uuid != null)
				Uuid = uuid.Value;

			PNumber version;
			if (doc.TryGetValue ("Version", out version) && version != null)
				Version = (int) version.Value;
		}

		public void Save (string path)
		{
			File.WriteAllBytes (path, Data);
		}

		public IList<string> ApplicationIdentifierPrefix { get; private set; }
		public IList<string> TeamIdentifierPrefix { get; private set; }
		public DateTime CreationDate { get; private set; }
		public IList<X509Certificate2> DeveloperCertificates { get; private set; }
		public PDictionary Entitlements { get; private set; }
		public DateTime ExpirationDate { get; private set; }
		public string Name { get; private set; }
		public MobileProvisionPlatform [] Platforms { get; private set; }
		public IList<string> ProvisionedDevices { get; private set; }
		public bool? ProvisionsAllDevices { get; private set; }
		public int TimeToLive { get; private set; }
		public string Uuid { get; private set; }
		public int Version { get; private set; }
		public byte [] Data { get; private set; }

		public bool MatchesBundleIdentifier (string bundleIdentifier)
		{
			PString identifier;
			string id;
			int dot;

			if (bundleIdentifier == null)
				throw new ArgumentNullException (nameof (bundleIdentifier));

			if (Entitlements.TryGetValue ("com.apple.application-identifier", out identifier))
				id = identifier.Value;
			else if (Entitlements.TryGetValue ("application-identifier", out identifier))
				id = identifier.Value;
			else
				return false;

			// Note: the identifier will be in the form "7V723M9SQ5.com.xamarin.app-name", so we'll need to trim the leading TeamIdentifierPrefix
			if ((dot = id.IndexOf ('.')) != -1)
				id = id.Substring (dot + 1);

			if (id.Length > 0 && id [id.Length - 1] == '*') {
				// Note: this is a wildcard provisioning profile, which means we need to use a substring match
				id = id.TrimEnd ('*');

				if (!bundleIdentifier.StartsWith (id, StringComparison.Ordinal))
					return false;
			} else if (id != bundleIdentifier) {
				// the CFBundleIdentifier provided by our caller does not match this provisioning profile
				return false;
			}

			return true;
		}

		public bool MatchesDeveloperCertificate (X509Certificate2 certificate)
		{
			if (certificate == null)
				throw new ArgumentNullException (nameof (certificate));

			for (int i = 0; i < DeveloperCertificates.Count; i++) {
				if (DeveloperCertificates [i].Thumbprint == certificate.Thumbprint)
					return true;
			}

			return false;
		}
	}
}
