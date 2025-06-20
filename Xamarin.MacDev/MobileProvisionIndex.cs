//
// MobileProvisionIndex.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2015 Xamarin Inc. (www.xamarin.com)
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
	public class MobileProvisionIndex {
		static readonly string IndexFileName;
		static readonly MobileProvisionCreationDateComparer CreationDateComparer = new MobileProvisionCreationDateComparer ();
		const int IndexVersion = 1;

		public class DeveloperCertificate {
			public string Name { get; private set; }
			public string Thumbprint { get; private set; }

			DeveloperCertificate ()
			{
			}

			internal DeveloperCertificate (X509Certificate2 certificate)
			{
				Name = Keychain.GetCertificateCommonName (certificate);
				Thumbprint = certificate.Thumbprint;
			}

			internal static DeveloperCertificate Load (BinaryReader reader)
			{
				var certificate = new DeveloperCertificate ();
				certificate.Name = reader.ReadString ();
				certificate.Thumbprint = reader.ReadString ();
				return certificate;
			}

			internal void Write (BinaryWriter writer)
			{
				writer.Write (Name);
				writer.Write (Thumbprint);
			}
		}

		public class ProvisioningProfile {
			public string FileName { get; private set; }
			public DateTime LastModified { get; private set; }

			public string Name { get; private set; }
			public string Uuid { get; private set; }
			public MobileProvisionDistributionType Distribution { get; private set; }
			public DateTime CreationDate { get; private set; }
			public DateTime ExpirationDate { get; private set; }

			public List<MobileProvisionPlatform> Platforms { get; private set; }
			public string ApplicationIdentifier { get; private set; }
			public List<DeveloperCertificate> DeveloperCertificates { get; private set; }

			ProvisioningProfile ()
			{
				Platforms = new List<MobileProvisionPlatform> ();
				DeveloperCertificates = new List<DeveloperCertificate> ();
			}

			ProvisioningProfile (string fileName, MobileProvision provision) : this ()
			{
				FileName = fileName;
				LastModified = File.GetLastWriteTimeUtc (fileName);

				Name = provision.Name;
				Uuid = provision.Uuid;
				Distribution = provision.DistributionType;
				CreationDate = provision.CreationDate;
				ExpirationDate = provision.ExpirationDate;

				if (provision.Platforms != null) {
					for (int i = 0; i < provision.Platforms.Length; i++)
						Platforms.Add (provision.Platforms [i]);
				}

				PString identifier;
				if (provision.Entitlements.TryGetValue ("com.apple.application-identifier", out identifier))
					ApplicationIdentifier = identifier.Value;
				else if (provision.Entitlements.TryGetValue ("application-identifier", out identifier))
					ApplicationIdentifier = identifier.Value;
				else
					ApplicationIdentifier = string.Empty;

				for (int i = 0; i < provision.DeveloperCertificates.Count; i++)
					DeveloperCertificates.Add (new DeveloperCertificate (provision.DeveloperCertificates [i]));
			}

			internal static ProvisioningProfile Load (string fileName)
			{
				var provision = MobileProvision.LoadFromFile (fileName);

				return new ProvisioningProfile (fileName, provision);
			}

			internal static ProvisioningProfile Load (BinaryReader reader)
			{
				var profile = new ProvisioningProfile ();
				MobileProvisionDistributionType type;
				int count;

				profile.FileName = reader.ReadString ();
				profile.LastModified = new DateTime (reader.ReadInt64 (), DateTimeKind.Utc);

				profile.Name = reader.ReadString ();
				profile.Uuid = reader.ReadString ();
				Enum.TryParse (reader.ReadString (), out type);
				profile.Distribution = type;
				profile.CreationDate = new DateTime (reader.ReadInt64 (), DateTimeKind.Utc);
				profile.ExpirationDate = new DateTime (reader.ReadInt64 (), DateTimeKind.Utc);

				count = reader.ReadInt32 ();
				for (int i = 0; i < count; i++) {
					MobileProvisionPlatform platform;

					Enum.TryParse (reader.ReadString (), out platform);
					profile.Platforms.Add (platform);
				}

				profile.ApplicationIdentifier = reader.ReadString ();

				count = reader.ReadInt32 ();
				for (int i = 0; i < count; i++)
					profile.DeveloperCertificates.Add (DeveloperCertificate.Load (reader));

				return profile;
			}

			internal void Write (BinaryWriter writer)
			{
				writer.Write (FileName);
				writer.Write (LastModified.Ticks);

				writer.Write (Name);
				writer.Write (Uuid);
				writer.Write (Distribution.ToString ());
				writer.Write (CreationDate.Ticks);
				writer.Write (ExpirationDate.Ticks);

				writer.Write (Platforms.Count);
				for (int i = 0; i < Platforms.Count; i++)
					writer.Write (Platforms [i].ToString ());

				writer.Write (ApplicationIdentifier);

				writer.Write (DeveloperCertificates.Count);
				for (int i = 0; i < DeveloperCertificates.Count; i++)
					DeveloperCertificates [i].Write (writer);
			}
		}

		class MobileProvisionCreationDateComparer : IComparer<ProvisioningProfile> {
			public int Compare (ProvisioningProfile x, ProvisioningProfile y)
			{
				return y.CreationDate.CompareTo (x.CreationDate);
			}
		}

		#region MobileProvisionIndex

		public List<ProvisioningProfile> ProvisioningProfiles { get; private set; }
		public DateTime LastModified { get; private set; }
		public int Version { get; private set; }

		static MobileProvisionIndex ()
		{
			string provisioningFolder;

			if (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX) {
				provisioningFolder = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.UserProfile), "Library", "Caches", "maui");
			} else {
				provisioningFolder = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData), "maui", "iOS", "Provisioning");
			}

			IndexFileName = Path.Combine (provisioningFolder, "Provisioning Profiles.index");
		}

		MobileProvisionIndex ()
		{
			ProvisioningProfiles = new List<ProvisioningProfile> ();
		}

		public static MobileProvisionIndex Load (string fileName)
		{
			var previousCreationDate = DateTime.MaxValue;
			var index = new MobileProvisionIndex ();
			var sorted = true;

			using (var stream = File.OpenRead (fileName)) {
				using (var reader = new BinaryReader (stream)) {
					index.Version = reader.ReadInt32 ();
					index.LastModified = new DateTime (reader.ReadInt64 (), DateTimeKind.Utc);

					int count = reader.ReadInt32 ();
					for (int i = 0; i < count; i++) {
						var profile = ProvisioningProfile.Load (reader);
						index.ProvisioningProfiles.Add (profile);

						if (profile.CreationDate > previousCreationDate)
							sorted = false;

						previousCreationDate = profile.CreationDate;
					}

					if (!sorted)
						index.ProvisioningProfiles.Sort (CreationDateComparer);

					return index;
				}
			}
		}

		public void Save (string fileName)
		{
			var tempFileName = Path.Combine (Path.GetDirectoryName (fileName), ".#" + Path.GetFileNameWithoutExtension (fileName) + ".tmp");

			try {
				Directory.CreateDirectory (Path.GetDirectoryName (fileName));

				using (var stream = File.Create (tempFileName, 4096)) {
					using (var writer = new BinaryWriter (stream)) {
						writer.Write (Version);
						writer.Write (LastModified.Ticks);

						writer.Write (ProvisioningProfiles.Count);
						for (int i = 0; i < ProvisioningProfiles.Count; i++)
							ProvisioningProfiles [i].Write (writer);
					}
				}

				if (File.Exists (fileName))
					File.Replace (tempFileName, fileName, null, true);
				else
					File.Move (tempFileName, fileName);
			} catch (Exception ex) {
				LoggingService.LogWarning ("Failed to save '{0}': {1}", fileName, ex);
			}
		}

		public static MobileProvisionIndex CreateIndex (string [] profileDirs, string indexName)
		{
			var index = new MobileProvisionIndex ();
			var mtime = DateTime.MinValue;

			foreach (var profileDir in profileDirs) {
				if (Directory.Exists (profileDir)) {
					var mtime2 = Directory.GetLastWriteTimeUtc (profileDir);

					if (mtime2 > mtime)
						mtime = mtime2;

					foreach (var fileName in Directory.EnumerateFiles (profileDir)) {
						if (!fileName.EndsWith (".mobileprovision", StringComparison.Ordinal) && !fileName.EndsWith (".provisionprofile", StringComparison.Ordinal))
							continue;

						try {
							var profile = ProvisioningProfile.Load (fileName);
							index.ProvisioningProfiles.Add (profile);
						} catch (Exception ex) {
							LoggingService.LogWarning ("Error reading provisioning profile '{0}': {1}", fileName, ex);
						}
					}
				} else {
					Directory.CreateDirectory (profileDir);
				}
			}

			index.ProvisioningProfiles.Sort (CreationDateComparer);
			index.Version = IndexVersion;
			index.LastModified = mtime;

			index.Save (indexName);

			return index;
		}

		static bool AnyProfileDirModifiedSince (string [] dirs, DateTime mtime)
		{
			foreach (var dir in dirs) {
				if (!Directory.Exists (dir)) {
					// Note: When creating the index, we make sure to create each profile dir which means if any
					// are deleted, then we likely need to reindex.
					return true;
				}

				var mtime2 = Directory.GetLastWriteTimeUtc (dir);

				if (mtime2 > mtime)
					return true;
			}

			return false;
		}

		public static MobileProvisionIndex OpenIndex (string [] profileDirs, string indexName)
		{
			MobileProvisionIndex index;

			try {
				index = Load (indexName);

				if (index.Version != IndexVersion || AnyProfileDirModifiedSince (profileDirs, index.LastModified))
					index = CreateIndex (profileDirs, indexName);

				if (index.ProvisioningProfiles.Count == 0) {
					try {
						File.Delete (indexName);
					} catch (Exception ex) {
						LoggingService.LogWarning ("Failed to delete stale index '{0}': {1}", indexName, ex);
					}

					index.ProvisioningProfiles.Clear ();
				}
			} catch {
				index = CreateIndex (profileDirs, indexName);
			}

			return index;
		}

		public static MobileProvision GetMobileProvision (MobileProvisionPlatform platform, string name, List<string> failures = null)
		{
			var extension = MobileProvision.GetFileExtension (platform);

			foreach (var profileDir in MobileProvision.ProfileDirectories) {
				var path = Path.Combine (profileDir, name + extension);

				if (File.Exists (path))
					return MobileProvision.LoadFromFile (path);
			}

			var index = OpenIndex (MobileProvision.ProfileDirectories, IndexFileName);
			var latestCreationDate = DateTime.MinValue;

			if (index.ProvisioningProfiles.Count == 0) {
				failures?.Add ($"Could not find any provisioning profiles on this machine.");
				return null;
			}

			foreach (var profile in index.ProvisioningProfiles) {
				if (!profile.FileName.EndsWith (extension, StringComparison.Ordinal)) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its FileName ({profile.FileName}) does not end with '{extension}'.");
					continue;
				}

				if (!profile.Platforms.Contains (platform)) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its platforms ({string.Join (", ", profile.Platforms.Select ((v) => v.ToString ()))}) do not match the requested platform ({platform}).");
					continue;
				}

				if (name == profile.Name || name == profile.Uuid)
					return MobileProvision.LoadFromFile (profile.FileName);

				failures?.Add ($"The profile '{profile.Name}' is not applicable because its Name and Uuid ({profile.Uuid}) do not match '{name}'.");
			}

			return null;
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, bool includeExpired = false, bool unique = false, List<string> failures = null)
		{
			var index = OpenIndex (MobileProvision.ProfileDirectories, IndexFileName);
			var extension = MobileProvision.GetFileExtension (platform);
			var dictionary = new Dictionary<string, int> ();
			var list = new List<MobileProvision> ();
			var now = DateTime.UtcNow;

			if (index.ProvisioningProfiles.Count == 0) {
				failures?.Add ($"Could not find any provisioning profiles on this machine.");
				return list;
			}

			// iterate over the profiles in reverse order so that we load newer profiles first (optimization for the 'unique' case)
			for (int i = index.ProvisioningProfiles.Count - 1; i >= 0; i--) {
				var profile = index.ProvisioningProfiles [i];

				if (!profile.FileName.EndsWith (extension, StringComparison.Ordinal)) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its FileName ({profile.FileName}) does not end with '{extension}'.");
					continue;
				}

				if (!profile.Platforms.Contains (platform)) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its platforms ({string.Join (", ", profile.Platforms.Select ((v) => v.ToString ()))}) do not match the requested platform ({platform}).");
					continue;
				}

				if (!includeExpired && profile.ExpirationDate < now) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because it has expired ({profile.ExpirationDate}).");
					continue;
				}

				if (unique) {
					int idx;

					if (dictionary.TryGetValue (profile.Name, out idx)) {
						if (profile.CreationDate > list [idx].CreationDate)
							list [idx] = MobileProvision.LoadFromFile (profile.FileName);
					} else {
						var provision = MobileProvision.LoadFromFile (profile.FileName);
						dictionary.Add (profile.Name, list.Count);
						list.Add (provision);
					}
				} else {
					var provision = MobileProvision.LoadFromFile (profile.FileName);
					list.Add (provision);
				}
			}

			return list;
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, MobileProvisionDistributionType type, bool includeExpired = false, bool unique = false, List<string> failures = null)
		{
			var index = OpenIndex (MobileProvision.ProfileDirectories, IndexFileName);
			var extension = MobileProvision.GetFileExtension (platform);
			var dictionary = new Dictionary<string, int> ();
			var list = new List<MobileProvision> ();
			var now = DateTime.UtcNow;

			if (index.ProvisioningProfiles.Count == 0) {
				failures?.Add ($"Could not find any provisioning profiles on this machine.");
				return list;
			}

			// iterate over the profiles in reverse order so that we load newer profiles first (optimization for the 'unique' case)
			for (int i = index.ProvisioningProfiles.Count - 1; i >= 0; i--) {
				var profile = index.ProvisioningProfiles [i];

				if (!profile.FileName.EndsWith (extension, StringComparison.Ordinal)) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its FileName ({profile.FileName}) does not end with '{extension}'.");
					continue;
				}

				if (!profile.Platforms.Contains (platform)) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its platforms ({string.Join (", ", profile.Platforms.Select ((v) => v.ToString ()))}) do not match the requested platform ({platform}).");
					continue;
				}

				if (!includeExpired && profile.ExpirationDate < now) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because it has expired ({profile.ExpirationDate}).");
					continue;
				}

				if (type != MobileProvisionDistributionType.Any && (profile.Distribution & type) == 0) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its ({profile.Distribution}) does not match the expected type ({type}).");
					continue;
				}

				if (unique) {
					int idx;

					if (dictionary.TryGetValue (profile.Name, out idx)) {
						if (profile.CreationDate > list [idx].CreationDate)
							list [idx] = MobileProvision.LoadFromFile (profile.FileName);
					} else {
						var provision = MobileProvision.LoadFromFile (profile.FileName);
						dictionary.Add (profile.Name, list.Count);
						list.Add (provision);
					}
				} else {
					var provision = MobileProvision.LoadFromFile (profile.FileName);
					list.Add (provision);
				}
			}

			return list;
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, MobileProvisionDistributionType type, IList<X509Certificate2> developerCertificates, bool includeExpired = false, bool unique = false, List<string> failures = null)
		{
			var index = OpenIndex (MobileProvision.ProfileDirectories, IndexFileName);
			var extension = MobileProvision.GetFileExtension (platform);
			var dictionary = new Dictionary<string, int> ();
			var thumbprints = new HashSet<string> ();
			var list = new List<MobileProvision> ();
			var now = DateTime.UtcNow;

			if (developerCertificates == null)
				throw new ArgumentNullException (nameof (developerCertificates));

			if (index.ProvisioningProfiles.Count == 0) {
				failures?.Add ($"Could not find any provisioning profiles on this machine.");
				return list;
			}

			foreach (var certificate in developerCertificates)
				thumbprints.Add (certificate.Thumbprint);

			if (thumbprints.Count == 0) {
				failures?.Add ($"Could not find any provisioning profiles because there are no developer certificates.");
				return list;
			}

			// iterate over the profiles in reverse order so that we load newer profiles first (optimization for the 'unique' case)
			for (int i = index.ProvisioningProfiles.Count - 1; i >= 0; i--) {
				var profile = index.ProvisioningProfiles [i];

				if (!profile.FileName.EndsWith (extension, StringComparison.Ordinal)) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its FileName ({profile.FileName}) does not end with '{extension}'.");
					continue;
				}

				if (!profile.Platforms.Contains (platform)) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its platforms ({string.Join (", ", profile.Platforms.Select ((v) => v.ToString ()))}) do not match the requested platform ({platform}).");
					continue;
				}

				if (!includeExpired && profile.ExpirationDate < now) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because it has expired ({profile.ExpirationDate}).");
					continue;
				}

				if (type != MobileProvisionDistributionType.Any && (profile.Distribution & type) == 0) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its ({profile.Distribution}) does not match the expected type ({type}).");
					continue;
				}

				foreach (var cert in profile.DeveloperCertificates) {
					if (!thumbprints.Contains (cert.Thumbprint)) {
						failures?.Add ($"The profile '{profile.Name}' might not be applicable because its developer certificate (of {profile.DeveloperCertificates.Count} certificates) {cert.Name}'s thumbprint ({cert.Thumbprint}) is not in the list of accepted thumbprints ({string.Join (", ", thumbprints)}).");
						continue;
					}

					if (unique) {
						int idx;

						if (dictionary.TryGetValue (profile.Name, out idx)) {
							if (profile.CreationDate > list [idx].CreationDate)
								list [idx] = MobileProvision.LoadFromFile (profile.FileName);
						} else {
							var provision = MobileProvision.LoadFromFile (profile.FileName);
							dictionary.Add (profile.Name, list.Count);
							list.Add (provision);
						}
					} else {
						var provision = MobileProvision.LoadFromFile (profile.FileName);
						list.Add (provision);
					}
					break;
				}
			}

			return list;
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, string bundleIdentifier, MobileProvisionDistributionType type, bool includeExpired = false, bool unique = false, List<string> failures = null)
		{
			var index = OpenIndex (MobileProvision.ProfileDirectories, IndexFileName);
			var extension = MobileProvision.GetFileExtension (platform);
			var dictionary = new Dictionary<string, int> ();
			var list = new List<MobileProvision> ();
			var now = DateTime.UtcNow;

			if (bundleIdentifier == null)
				throw new ArgumentNullException (nameof (bundleIdentifier));

			if (index.ProvisioningProfiles.Count == 0) {
				failures?.Add ($"Could not find any provisioning profiles on this machine.");
				return list;
			}

			// iterate over the profiles in reverse order so that we load newer profiles first (optimization for the 'unique' case)
			for (int i = index.ProvisioningProfiles.Count - 1; i >= 0; i--) {
				var profile = index.ProvisioningProfiles [i];

				if (!profile.FileName.EndsWith (extension, StringComparison.Ordinal)) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its FileName ({profile.FileName}) does not end with '{extension}'.");
					continue;
				}

				if (!profile.Platforms.Contains (platform)) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its platforms ({string.Join (", ", profile.Platforms.Select ((v) => v.ToString ()))}) do not match the requested platform ({platform}).");
					continue;
				}

				if (!includeExpired && profile.ExpirationDate < now) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because it has expired ({profile.ExpirationDate}).");
					continue;
				}

				if (type != MobileProvisionDistributionType.Any && (profile.Distribution & type) == 0) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its ({profile.Distribution}) does not match the expected type ({type}).");
					continue;
				}

				var id = profile.ApplicationIdentifier;
				int dot;

				// Note: the ApplicationIdentifier will be in the form "7V723M9SQ5.com.xamarin.app-name", so we'll need to trim the leading TeamIdentifierPrefix
				if ((dot = id.IndexOf ('.')) != -1)
					id = id.Substring (dot + 1);

				if (id.Length > 0 && id [id.Length - 1] == '*') {
					// Note: this is a wildcard provisioning profile, which means we need to use a substring match
					id = id.TrimEnd ('*');

					if (!bundleIdentifier.StartsWith (id, StringComparison.Ordinal)) {
						failures?.Add ($"The profile '{profile.Name}' is not applicable because its id ({profile.ApplicationIdentifier}) does not match the bundle identifer {bundleIdentifier}.");
						continue;
					}
				} else if (id != bundleIdentifier) {
					// the CFBundleIdentifier provided by our caller does not match this provisioning profile
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its id ({profile.ApplicationIdentifier}) does not match the bundle identifer {bundleIdentifier}.");
					continue;
				}

				if (unique) {
					int idx;

					if (dictionary.TryGetValue (profile.Name, out idx)) {
						if (profile.CreationDate > list [idx].CreationDate)
							list [idx] = MobileProvision.LoadFromFile (profile.FileName);
					} else {
						var provision = MobileProvision.LoadFromFile (profile.FileName);
						dictionary.Add (profile.Name, list.Count);
						list.Add (provision);
					}
				} else {
					var provision = MobileProvision.LoadFromFile (profile.FileName);
					list.Add (provision);
				}
			}

			return list;
		}

		public static IList<MobileProvision> GetMobileProvisions (MobileProvisionPlatform platform, string bundleIdentifier, MobileProvisionDistributionType type, IList<X509Certificate2> developerCertificates, bool includeExpired = false, bool unique = false, List<string> failures = null)
		{
			var index = OpenIndex (MobileProvision.ProfileDirectories, IndexFileName);
			var extension = MobileProvision.GetFileExtension (platform);
			var dictionary = new Dictionary<string, int> ();
			var thumbprints = new HashSet<string> ();
			var list = new List<MobileProvision> ();
			var now = DateTime.UtcNow;

			if (bundleIdentifier == null)
				throw new ArgumentNullException (nameof (bundleIdentifier));

			if (developerCertificates == null)
				throw new ArgumentNullException (nameof (developerCertificates));

			if (index.ProvisioningProfiles.Count == 0) {
				failures?.Add ($"Could not find any provisioning profiles on this machine.");
				return list;
			}

			foreach (var certificate in developerCertificates)
				thumbprints.Add (certificate.Thumbprint);

			if (thumbprints.Count == 0) {
				failures?.Add ($"Could not find any provisioning profiles because there are no developer certificates.");
				return list;
			}

			// iterate over the profiles in reverse order so that we load newer profiles first (optimization for the 'unique' case)
			for (int i = index.ProvisioningProfiles.Count - 1; i >= 0; i--) {
				var profile = index.ProvisioningProfiles [i];

				if (!profile.FileName.EndsWith (extension, StringComparison.Ordinal)) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its FileName ({profile.FileName}) does not end with '{extension}'.");
					continue;
				}

				if (!profile.Platforms.Contains (platform)) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its platforms ({string.Join (", ", profile.Platforms.Select ((v) => v.ToString ()))}) do not match the requested platform ({platform}).");
					continue;
				}

				if (!includeExpired && profile.ExpirationDate < now) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because it has expired ({profile.ExpirationDate}).");
					continue;
				}

				if (type != MobileProvisionDistributionType.Any && (profile.Distribution & type) == 0) {
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its ({profile.Distribution}) does not match the expected type ({type}).");
					continue;
				}

				var id = profile.ApplicationIdentifier;
				int dot;

				// Note: the ApplicationIdentifier will be in the form "7V723M9SQ5.com.xamarin.app-name", so we'll need to trim the leading TeamIdentifierPrefix
				if ((dot = id.IndexOf ('.')) != -1)
					id = id.Substring (dot + 1);

				if (id.Length > 0 && id [id.Length - 1] == '*') {
					// Note: this is a wildcard provisioning profile, which means we need to use a substring match
					id = id.TrimEnd ('*');

					if (!bundleIdentifier.StartsWith (id, StringComparison.Ordinal)) {
						failures?.Add ($"The profile '{profile.Name}' is not applicable because its id ({profile.ApplicationIdentifier}) does not match the bundle identifer {bundleIdentifier}.");
						continue;
					}
				} else if (id != bundleIdentifier) {
					// the CFBundleIdentifier provided by our caller does not match this provisioning profile
					failures?.Add ($"The profile '{profile.Name}' is not applicable because its id ({profile.ApplicationIdentifier}) does not match the bundle identifer {bundleIdentifier}.");
					continue;
				}

				var matchingCerts = profile.DeveloperCertificates.Where (cert => thumbprints.Contains (cert.Thumbprint)).ToArray ();
				if (!matchingCerts.Any ()) {
					failures?.Add ($"The profile '{profile.Name}' ({profile.ApplicationIdentifier}) is not applicable because none of its developer certificates match the currently available certificates. This provisioning profile has {profile.DeveloperCertificates.Count} certificate(s):");
					foreach (var cert in profile.DeveloperCertificates)
						failures?.Add ($"    {cert.Name} ({cert.Thumbprint})");
				} else {
					foreach (var cert in matchingCerts) {
						if (unique) {
							int idx;

							if (dictionary.TryGetValue (profile.Name, out idx)) {
								if (profile.CreationDate > list [idx].CreationDate)
									list [idx] = MobileProvision.LoadFromFile (profile.FileName);
							} else {
								var provision = MobileProvision.LoadFromFile (profile.FileName);
								dictionary.Add (profile.Name, list.Count);
								list.Add (provision);
							}
						} else {
							var provision = MobileProvision.LoadFromFile (profile.FileName);
							list.Add (provision);
						}
						break;
					}
				}
			}

			return list;
		}
	}

	#endregion MobileProvisionIndex
}
