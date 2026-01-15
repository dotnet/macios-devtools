//
// PlatformAvailability.cs
//
// Data structures for mapping Xamarin.iOS and Xamarin.Mac's
// AvailabilityAttribute and its subclasses.
//
// Keep in sync with src/ObjCRuntime/PlatformAvailability.cs
// in xamarin/maccore.
//
// Author:
//   Aaron Bockover <abock@xamarin.com>
//
// Copyright 2014 Xamarin Inc.
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Disable until we get around to enable + fix any issues.
#nullable disable

namespace Xamarin.MacDev {
	[Flags]
	public enum PlatformArchitecture : byte {
		None = 0x00,
		Arch32 = 0x01,
		Arch64 = 0x02,
		All = 0xff
	}

	public enum PlatformName : byte {
		None,
		MacOSX,
		iOS,
		WatchOS,
		TvOS
	}

	public enum AvailabilityKind {
		Introduced,
		Deprecated,
		Obsoleted,
		Unavailable
	}

	public class Platform {
		internal uint value;

		public PlatformName Name { get; set; }

		public PlatformArchitecture Architecture {
			get {
				var arch = (byte) (value >> 24);
				if (arch == 0)
					return PlatformArchitecture.All;
				return (PlatformArchitecture) arch;
			}

			set { this.value = (this.value & 0x00ffffff) | (uint) (byte) value << 24; }
		}

		public byte Major {
			get { return (byte) (value >> 16); }
			set { this.value = (this.value & 0xff00ffff) | (uint) (byte) value << 16; }
		}

		public byte Minor {
			get { return (byte) (value >> 8); }
			set { this.value = (this.value & 0xffff00ff) | (uint) (byte) value << 8; }
		}

		public byte Subminor {
			get { return (byte) value; }
			set { this.value = (this.value & 0xffffff00) | (uint) (byte) value; }
		}

		static string ToNiceName (PlatformName name)
		{
			switch (name) {
			case PlatformName.None:
				return "None";
			case PlatformName.MacOSX:
				return "Mac OS X";
			case PlatformName.iOS:
				return "iOS";
			case PlatformName.WatchOS:
				return "watchOS";
			case PlatformName.TvOS:
				return "tvOS";
			default:
				throw new ArgumentOutOfRangeException ();
			}
		}

		public string FullName {
			get {
				var name = ToNiceName (Name);

				// when a version is specified, use that; all bits set mean
				// the attribute applies to all versions, so ignore that
				if (Major > 0 && Major != 0xff)
					return String.Format ("{0} {1}.{2}", name, Major, Minor);

				// otherwise note the architecture
				if (Architecture == PlatformArchitecture.Arch32)
					return String.Format ("{0} 32-bit", name);

				if (Architecture == PlatformArchitecture.Arch64)
					return String.Format ("{0} 64-bit", name);

				// unless it applies to a combination or lack
				// of architectures, then just use the name
				return name;
			}
		}

		public bool HasValue { get; set; }

		public bool IsSpecified {
			get { return value != 0; }
		}

		internal Platform (PlatformName name, uint value)
		{
			Name = name;
			this.value = value;
		}

		public Platform (PlatformName name,
			PlatformArchitecture architecture = PlatformArchitecture.All,
			byte major = 0, byte minor = 0, byte subminor = 0)
		{
			Name = name;
			Architecture = architecture;
			Major = major;
			Minor = minor;
			Subminor = subminor;
		}

		public int VersionCompare (Platform value)
		{
			if (value == null)
				return 1;

			if (Major > value.Major)
				return 1;
			if (Major < value.Major)
				return -1;

			if (Minor > value.Minor)
				return 1;
			if (Minor < value.Minor)
				return -1;

			if (Subminor > value.Subminor)
				return 1;
			if (Subminor < value.Subminor)
				return -1;

			return 0;
		}

		public override string ToString ()
		{
			if (!IsSpecified)
				return String.Empty;

			var name = ToNiceName (Name);
			string s = null;

			if (Major > 0) {
				s = String.Format ("Platform.{0}_{1}_{2}", name, Major, Minor);
				if (Subminor > 0)
					s += String.Format ("_{0}", Subminor);
			}

			if (Architecture != PlatformArchitecture.All) {
				if (!String.IsNullOrEmpty (s))
					s += " | ";
				s += Architecture.ToString ().Replace ("Any", "Platform." + name + "_Arch");
			}

			return s;
		}
	}

	public class PlatformSet : IEnumerable<Platform> {
		// Analysis disable once InconsistentNaming
		public Platform iOS { get; private set; }
		public Platform MacOSX { get; private set; }
		public Platform WatchOS { get; private set; }
		public Platform TvOS { get; private set; }

		public bool IsSpecified {
			get { return this.Any (p => p.IsSpecified); }
		}

		public PlatformSet ()
		{
			iOS = new Platform (PlatformName.iOS, PlatformArchitecture.None);
			MacOSX = new Platform (PlatformName.MacOSX, PlatformArchitecture.None);
			WatchOS = new Platform (PlatformName.WatchOS, PlatformArchitecture.None);
			TvOS = new Platform (PlatformName.TvOS, PlatformArchitecture.None);
		}

		/// <summary>
		/// Initialize a <see cref="MonoDevelop.MacDev.PlatformVersion"/> struct with
		/// version information encoded as a value of <see cref="ObjCRuntime.Platform"/>
		/// enum. For example (ulong)(Platform.iOS_8_0 | Platform.Mac_10_10).
		/// </summary>
		/// <param name="platformEncoding">Should have the bit format AAJJNNSSAAJJNNSS, where
		/// AA are the supported architecture flags, JJ is the maJor version, NN
		/// is the miNor version, and SS is the Subminor version. The high AAJJNNSS
		/// bytes indicate Mac version information and the low AAJJNNSS bytes
		/// indicate iOS version information. Only Major and Minor version components
		/// are parsed from the version.</param>
		public PlatformSet (ulong platformEncoding)
		{
			//This constructor is only useful in XAMCORE_2 or older, since it only supports iOS and OSX, keep it for backward compatibility
			iOS = new Platform (PlatformName.iOS, (uint) platformEncoding);
			MacOSX = new Platform (PlatformName.MacOSX, (uint) (platformEncoding >> 32));
			WatchOS = new Platform (PlatformName.WatchOS, PlatformArchitecture.None);
			TvOS = new Platform (PlatformName.TvOS, PlatformArchitecture.None);
		}

		public static PlatformSet operator | (PlatformSet a, PlatformSet b)
		{
			if (a == null && b == null)
				return null;

			var result = new PlatformSet ();

			if (a == null) {
				result.MacOSX.value = b.MacOSX.value;
				result.iOS.value = b.iOS.value;
				result.WatchOS.value = b.WatchOS.value;
				result.TvOS.value = b.TvOS.value;
			} else if (b == null) {
				result.MacOSX.value = a.MacOSX.value;
				result.iOS.value = a.iOS.value;
				result.WatchOS.value = a.WatchOS.value;
				result.TvOS.value = a.TvOS.value;
			} else {
				result.MacOSX.value = a.MacOSX.value | b.MacOSX.value;
				result.iOS.value = a.iOS.value | b.iOS.value;
				result.WatchOS.value = a.WatchOS.value | b.WatchOS.value;
				result.TvOS.value = a.TvOS.value | b.TvOS.value;
			}

			return result;
		}

		public IEnumerator<Platform> GetEnumerator ()
		{
			yield return iOS;
			yield return MacOSX;
			yield return WatchOS;
			yield return TvOS;
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		public override string ToString ()
		{
			string s = null;

			foreach (var platform in this) {
				var platformString = platform.ToString ();
				if (!String.IsNullOrEmpty (platformString)) {
					if (!String.IsNullOrEmpty (s))
						s += " | ";
					s += platformString;
				}
			}

			return s;
		}
	}

	public class PlatformAvailability {
		public PlatformSet Introduced { get; set; }
		public PlatformSet Deprecated { get; set; }
		public PlatformSet Obsoleted { get; set; }
		public PlatformSet Unavailable { get; set; }
		public string Message { get; set; }

		public bool IsSpecified {
			get {
				return
					(Introduced != null && Introduced.IsSpecified) ||
					(Deprecated != null && Deprecated.IsSpecified) ||
					(Obsoleted != null && Obsoleted.IsSpecified) ||
					(Unavailable != null && Unavailable.IsSpecified) ||
					Message != null;
			}
		}

		public Platform GetIntroduced (Platform platform)
		{
			return GetIntroduced (platform.Name);
		}

		public Platform GetIntroduced (PlatformName name)
		{
			if (Introduced == null)
				return null;
			switch (name) {
			case PlatformName.iOS:
				return Introduced.iOS;
			case PlatformName.WatchOS:
				return Introduced.WatchOS;
			case PlatformName.TvOS:
				return Introduced.TvOS;
			}
			return Introduced.MacOSX;
		}

		public Platform GetDeprecated (Platform platform)
		{
			return GetDeprecated (platform.Name);
		}

		public Platform GetDeprecated (PlatformName name)
		{
			if (Deprecated == null)
				return null;
			switch (name) {
			case PlatformName.iOS:
				return Deprecated.iOS;
			case PlatformName.WatchOS:
				return Deprecated.WatchOS;
			case PlatformName.TvOS:
				return Deprecated.TvOS;
			}
			return Deprecated.MacOSX;
		}

		public Platform GetObsoleted (Platform platform)
		{
			return GetObsoleted (platform.Name);
		}

		public Platform GetObsoleted (PlatformName name)
		{
			if (Obsoleted == null)
				return null;
			switch (name) {
			case PlatformName.iOS:
				return Obsoleted.iOS;
			case PlatformName.WatchOS:
				return Obsoleted.WatchOS;
			case PlatformName.TvOS:
				return Obsoleted.TvOS;
			}
			return Obsoleted.MacOSX;
		}

		public Platform GetUnavailable (Platform platform)
		{
			return GetUnavailable (platform.Name);
		}

		public Platform GetUnavailable (PlatformName name)
		{
			if (Unavailable == null)
				return null;
			switch (name) {
			case PlatformName.iOS:
				return Unavailable.iOS;
			case PlatformName.WatchOS:
				return Unavailable.WatchOS;
			case PlatformName.TvOS:
				return Unavailable.TvOS;
			}
			return Unavailable.MacOSX;
		}

		public override string ToString ()
		{
			var s = new StringBuilder ();

			if (Introduced != null && Introduced.IsSpecified)
				s.AppendFormat ("Introduced = {0}, ", Introduced);

			if (Deprecated != null && Deprecated.IsSpecified)
				s.AppendFormat ("Deprecated = {0}, ", Deprecated);

			if (Obsoleted != null && Obsoleted.IsSpecified)
				s.AppendFormat ("Obsoleted = {0}, ", Obsoleted);

			if (Unavailable != null && Unavailable.IsSpecified)
				s.AppendFormat ("Unavailable = {0}, ", Unavailable);

			if (!String.IsNullOrEmpty (Message))
				s.AppendFormat ("Message = \"{0}\", ", Message.Replace ("\"", "\\\""));

			if (s.Length < 2)
				return "[Availability]";

			s.Length -= 2;

			return String.Format ("[Availability ({0})]", s);
		}
	}
}
