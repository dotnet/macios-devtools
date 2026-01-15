//
// AppleSdkVersion.cs
//
// Authors: Michael Hutchinson <mhutchinson@novell.com>
//          Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2011-2013 Xamarin Inc. (http://www.xamarin.com)
// Copyright (c) 2021 Microsoft Corp. (http://www.microsoft.com)
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
	public struct AppleSdkVersion : IComparable<AppleSdkVersion>, IEquatable<AppleSdkVersion>, IAppleSdkVersion {
		int [] version;

		public AppleSdkVersion (Version version)
		{
			if (version == null)
				throw new ArgumentNullException ();

			if (version.Build != -1) {
				this.version = new int [3];
				this.version [2] = version.Build;
			} else {
				this.version = new int [2];
			}

			this.version [0] = version.Major;
			this.version [1] = version.Minor;
		}

		public AppleSdkVersion (params int [] version)
		{
			if (version == null)
				throw new ArgumentNullException (nameof (version));

			this.version = version;
		}

		void IAppleSdkVersion.SetVersion (int [] version)
		{
			this.version = version;
		}

		public static AppleSdkVersion Parse (string s)
		{
			return IAppleSdkVersion_Extensions.Parse<AppleSdkVersion> (s);
		}

		public static bool TryParse (string s, out AppleSdkVersion result)
		{
			return IAppleSdkVersion_Extensions.TryParse (s, out result);
		}

		public int [] Version { get { return version; } }

		public override string ToString ()
		{
			return IAppleSdkVersion_Extensions.ToString (this);
		}

		public int CompareTo (AppleSdkVersion other)
		{
			return IAppleSdkVersion_Extensions.CompareTo (this, other);
		}

		public int CompareTo (IAppleSdkVersion other)
		{
			return IAppleSdkVersion_Extensions.CompareTo (this, other);
		}

		public bool Equals (IAppleSdkVersion other)
		{
			return IAppleSdkVersion_Extensions.Equals (this, other);
		}

		public bool Equals (AppleSdkVersion other)
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

		public static bool operator == (AppleSdkVersion a, IAppleSdkVersion b)
		{
			return a.Equals (b);
		}

		public static bool operator != (AppleSdkVersion a, IAppleSdkVersion b)
		{
			return !a.Equals (b);
		}

		public static bool operator < (AppleSdkVersion a, IAppleSdkVersion b)
		{
			return a.CompareTo (b) < 0;
		}

		public static bool operator > (AppleSdkVersion a, IAppleSdkVersion b)
		{
			return a.CompareTo (b) > 0;
		}

		public static bool operator <= (AppleSdkVersion a, IAppleSdkVersion b)
		{
			return a.CompareTo (b) <= 0;
		}

		public static bool operator >= (AppleSdkVersion a, IAppleSdkVersion b)
		{
			return a.CompareTo (b) >= 0;
		}

		public bool IsUseDefault {
			get { return version == null || version.Length == 0; }
		}

		IAppleSdkVersion IAppleSdkVersion.GetUseDefault ()
		{
			return UseDefault;
		}

#if !WINDOWS
		public AppleSdkVersion ResolveIfDefault (AppleSdk sdk, bool sim)
		{
			return IsUseDefault ? GetDefault (sdk, sim) : this;
		}

		public static AppleSdkVersion GetDefault (AppleSdk sdk, bool sim)
		{
			var v = sdk.GetInstalledSdkVersions (sim);
			return v.Count > 0 ? v [v.Count - 1] : UseDefault;
		}
#endif

		public static readonly AppleSdkVersion UseDefault = new AppleSdkVersion (new int [0]);

		public static readonly AppleSdkVersion V1_0 = new AppleSdkVersion (1, 0);
		public static readonly AppleSdkVersion V2_0 = new AppleSdkVersion (2, 0);
		public static readonly AppleSdkVersion V2_1 = new AppleSdkVersion (2, 1);
		public static readonly AppleSdkVersion V2_2 = new AppleSdkVersion (2, 2);
		public static readonly AppleSdkVersion V3_0 = new AppleSdkVersion (3, 0);
		public static readonly AppleSdkVersion V3_1 = new AppleSdkVersion (3, 1);
		public static readonly AppleSdkVersion V3_2 = new AppleSdkVersion (3, 2);
		public static readonly AppleSdkVersion V3_99 = new AppleSdkVersion (3, 99);
		public static readonly AppleSdkVersion V4_0 = new AppleSdkVersion (4, 0);
		public static readonly AppleSdkVersion V4_1 = new AppleSdkVersion (4, 1);
		public static readonly AppleSdkVersion V4_2 = new AppleSdkVersion (4, 2);
		public static readonly AppleSdkVersion V4_3 = new AppleSdkVersion (4, 3);
		public static readonly AppleSdkVersion V5_0 = new AppleSdkVersion (5, 0);
		public static readonly AppleSdkVersion V5_1 = new AppleSdkVersion (5, 1);
		public static readonly AppleSdkVersion V5_1_1 = new AppleSdkVersion (5, 1, 1);
		public static readonly AppleSdkVersion V5_2 = new AppleSdkVersion (5, 2);
		public static readonly AppleSdkVersion V6_0 = new AppleSdkVersion (6, 0);
		public static readonly AppleSdkVersion V6_1 = new AppleSdkVersion (6, 1);
		public static readonly AppleSdkVersion V6_2 = new AppleSdkVersion (6, 2);
		public static readonly AppleSdkVersion V7_0 = new AppleSdkVersion (7, 0);
		public static readonly AppleSdkVersion V7_1 = new AppleSdkVersion (7, 1);
		public static readonly AppleSdkVersion V7_2_1 = new AppleSdkVersion (7, 2, 1);
		public static readonly AppleSdkVersion V8_0 = new AppleSdkVersion (8, 0);
		public static readonly AppleSdkVersion V8_1 = new AppleSdkVersion (8, 1);
		public static readonly AppleSdkVersion V8_2 = new AppleSdkVersion (8, 2);
		public static readonly AppleSdkVersion V8_3 = new AppleSdkVersion (8, 3);
		public static readonly AppleSdkVersion V8_4 = new AppleSdkVersion (8, 4);
		public static readonly AppleSdkVersion V9_0 = new AppleSdkVersion (9, 0);
		public static readonly AppleSdkVersion V9_1 = new AppleSdkVersion (9, 1);
		public static readonly AppleSdkVersion V9_2 = new AppleSdkVersion (9, 2);
		public static readonly AppleSdkVersion V9_3 = new AppleSdkVersion (9, 3);
		public static readonly AppleSdkVersion V10_0 = new AppleSdkVersion (10, 0);
		public static readonly AppleSdkVersion V10_1 = new AppleSdkVersion (10, 1);
		public static readonly AppleSdkVersion V10_2 = new AppleSdkVersion (10, 2);
		public static readonly AppleSdkVersion V10_3 = new AppleSdkVersion (10, 3);
		public static readonly AppleSdkVersion V10_4 = new AppleSdkVersion (10, 4);
		public static readonly AppleSdkVersion V10_5 = new AppleSdkVersion (10, 5);
		public static readonly AppleSdkVersion V10_6 = new AppleSdkVersion (10, 6);
		public static readonly AppleSdkVersion V10_7 = new AppleSdkVersion (10, 7);
		public static readonly AppleSdkVersion V10_8 = new AppleSdkVersion (10, 8);
		public static readonly AppleSdkVersion V10_9 = new AppleSdkVersion (10, 9);
		public static readonly AppleSdkVersion V10_10 = new AppleSdkVersion (10, 10);
		public static readonly AppleSdkVersion V10_11 = new AppleSdkVersion (10, 11);
		public static readonly AppleSdkVersion V10_12 = new AppleSdkVersion (10, 12);
		public static readonly AppleSdkVersion V10_13 = new AppleSdkVersion (10, 13);
		public static readonly AppleSdkVersion V10_14 = new AppleSdkVersion (10, 14);
		public static readonly AppleSdkVersion V10_15 = new AppleSdkVersion (10, 15);
		public static readonly AppleSdkVersion V11_0 = new AppleSdkVersion (11, 0);
	}
}
