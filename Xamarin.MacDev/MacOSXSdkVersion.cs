// 
// MacOSXSdkVersion.cs
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

// Disable until we get around to enable + fix any issues.
#nullable disable

namespace Xamarin.MacDev {
	[Obsolete ("Use 'AppleSdkVersion' instead.")]
	public struct MacOSXSdkVersion : IComparable<MacOSXSdkVersion>, IEquatable<MacOSXSdkVersion>, IAppleSdkVersion {
		int [] version;

		public MacOSXSdkVersion (params int [] version)
		{
			if (version == null)
				throw new ArgumentNullException ();
			this.version = version;
		}

		void IAppleSdkVersion.SetVersion (int [] version)
		{
			this.version = version;
		}

		public MacOSXSdkVersion (Version version)
		{
			if (version == null)
				throw new ArgumentNullException ();
			this.version = new [] { version.Major, version.Minor };
		}

		public static MacOSXSdkVersion Parse (string s)
		{
			return IAppleSdkVersion_Extensions.Parse<MacOSXSdkVersion> (s);
		}

		public static bool TryParse (string s, out MacOSXSdkVersion result)
		{
			return IAppleSdkVersion_Extensions.TryParse<MacOSXSdkVersion> (s, out result);
		}

		public int [] Version { get { return version; } }

		public override string ToString ()
		{
			return IAppleSdkVersion_Extensions.ToString (this);
		}

		public int CompareTo (MacOSXSdkVersion other)
		{
			return IAppleSdkVersion_Extensions.CompareTo (this, other);
		}

		public bool Equals (MacOSXSdkVersion other)
		{
			return IAppleSdkVersion_Extensions.Equals (this, other);
		}

		public bool Equals (IAppleSdkVersion other)
		{
			return IAppleSdkVersion_Extensions.Equals (this, other);
		}

		public override bool Equals (object obj)
		{
			return IAppleSdkVersion_Extensions.Equals (this, obj);
		}

		public override int GetHashCode ()
		{
			return IAppleSdkVersion_Extensions.GetHashCode (this);
		}

		public static bool operator == (MacOSXSdkVersion a, MacOSXSdkVersion b)
		{
			return a.Equals (b);
		}

		public static bool operator != (MacOSXSdkVersion a, MacOSXSdkVersion b)
		{
			return !a.Equals (b);
		}

		public static bool operator < (MacOSXSdkVersion a, MacOSXSdkVersion b)
		{
			return a.CompareTo (b) < 0;
		}

		public static bool operator > (MacOSXSdkVersion a, MacOSXSdkVersion b)
		{
			return a.CompareTo (b) > 0;
		}

		public static bool operator <= (MacOSXSdkVersion a, MacOSXSdkVersion b)
		{
			return a.CompareTo (b) <= 0;
		}

		public static bool operator >= (MacOSXSdkVersion a, MacOSXSdkVersion b)
		{
			return a.CompareTo (b) >= 0;
		}

		public bool IsUseDefault {
			get {
				return version == null || version.Length == 0;
			}
		}

		IAppleSdkVersion IAppleSdkVersion.GetUseDefault ()
		{
			return UseDefault;
		}

		public static IAppleSdkVersion GetDefault (IAppleSdk sdk)
		{
			return GetDefault ((MacOSXSdk) sdk);
		}

		public static IAppleSdkVersion GetDefault (MacOSXSdk sdk)
		{
			var v = sdk.GetInstalledSdkVersions ();
			return v.Count > 0 ? v [v.Count - 1] : (IAppleSdkVersion) UseDefault;
		}

		public IAppleSdkVersion ResolveIfDefault (MacOSXSdk sdk)
		{
			return IsUseDefault ? GetDefault (sdk) : this;
		}

		public static readonly MacOSXSdkVersion UseDefault = new MacOSXSdkVersion (new int [0]);

		public static readonly MacOSXSdkVersion V10_0 = new MacOSXSdkVersion (10, 0);
		public static readonly MacOSXSdkVersion V10_1 = new MacOSXSdkVersion (10, 1);
		public static readonly MacOSXSdkVersion V10_2 = new MacOSXSdkVersion (10, 2);
		public static readonly MacOSXSdkVersion V10_3 = new MacOSXSdkVersion (10, 3);
		public static readonly MacOSXSdkVersion V10_4 = new MacOSXSdkVersion (10, 4);
		public static readonly MacOSXSdkVersion V10_5 = new MacOSXSdkVersion (10, 5);
		public static readonly MacOSXSdkVersion V10_6 = new MacOSXSdkVersion (10, 6);
		public static readonly MacOSXSdkVersion V10_7 = new MacOSXSdkVersion (10, 7);
		public static readonly MacOSXSdkVersion V10_8 = new MacOSXSdkVersion (10, 8);
		public static readonly MacOSXSdkVersion V10_9 = new MacOSXSdkVersion (10, 9);
		public static readonly MacOSXSdkVersion V10_10 = new MacOSXSdkVersion (10, 10);
		public static readonly MacOSXSdkVersion V10_11 = new MacOSXSdkVersion (10, 11);
		public static readonly MacOSXSdkVersion V10_12 = new MacOSXSdkVersion (10, 12);
		public static readonly MacOSXSdkVersion V10_13 = new MacOSXSdkVersion (10, 13);
		public static readonly MacOSXSdkVersion V10_14 = new MacOSXSdkVersion (10, 14);
		public static readonly MacOSXSdkVersion V10_15 = new MacOSXSdkVersion (10, 15);

		public static readonly MacOSXSdkVersion Cheetah = V10_0;
		public static readonly MacOSXSdkVersion Puma = V10_1;
		public static readonly MacOSXSdkVersion Jaguar = V10_2;
		public static readonly MacOSXSdkVersion Panther = V10_3;
		public static readonly MacOSXSdkVersion Tiger = V10_4;
		public static readonly MacOSXSdkVersion Leopard = V10_5;
		public static readonly MacOSXSdkVersion SnowLeopard = V10_6;
		public static readonly MacOSXSdkVersion Lion = V10_7;
		public static readonly MacOSXSdkVersion MountainLion = V10_8;
		public static readonly MacOSXSdkVersion Mavericks = V10_9;
		public static readonly MacOSXSdkVersion Yosemite = V10_10;
		public static readonly MacOSXSdkVersion ElCapitan = V10_11;
		public static readonly MacOSXSdkVersion Sierra = V10_12;
		public static readonly MacOSXSdkVersion HighSierra = V10_13;
		public static readonly MacOSXSdkVersion Mojave = V10_14;
		public static readonly MacOSXSdkVersion Catalina = V10_15;

		public static string VersionName (MacOSXSdkVersion version)
		{
			string versionName;
			switch (version.ToString ()) {
			case "10.0":
				versionName = "Cheetah";
				break;
			case "10.1":
				versionName = "Puma";
				break;
			case "10.2":
				versionName = "Jaguar";
				break;
			case "10.3":
				versionName = "Panther";
				break;
			case "10.4":
				versionName = "Tiger";
				break;
			case "10.5":
				versionName = "Leopard";
				break;
			case "10.6":
				versionName = "Snow Leopard";
				break;
			case "10.7":
				versionName = "Lion";
				break;
			case "10.8":
				versionName = "Mountain Lion";
				break;
			case "10.9":
				versionName = "Mavericks";
				break;
			case "10.10":
				versionName = "Yosemite";
				break;
			case "10.11":
				versionName = "El Capitan";
				break;
			case "10.12":
				versionName = "Sierra";
				break;
			case "10.13":
				versionName = "High Sierra";
				break;
			case "10.14":
				versionName = "Mojave";
				break;
			case "10.15":
				versionName = "Catalina";
				break;
			default:
				versionName = string.Empty;
				break;
			}
			return versionName;
		}
	}
}
