// 
// IPhoneSdkVersion.cs
//  
// Authors: Michael Hutchinson <mhutchinson@novell.com>
//          Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
// Copyright (c) 2011-2013 Xamarin Inc. (http://www.xamarin.com)
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
	public struct IPhoneSdkVersion : IComparable<IPhoneSdkVersion>, IEquatable<IPhoneSdkVersion>, IAppleSdkVersion {
		int [] version;

		public IPhoneSdkVersion (Version version)
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

		public IPhoneSdkVersion (params int [] version)
		{
			if (version == null)
				throw new ArgumentNullException ();

			this.version = version;
		}

		void IAppleSdkVersion.SetVersion (int [] version)
		{
			this.version = version;
		}

		public static IPhoneSdkVersion Parse (string s)
		{
			return IAppleSdkVersion_Extensions.Parse<IPhoneSdkVersion> (s);
		}

		public static bool TryParse (string s, out IPhoneSdkVersion result)
		{
			return IAppleSdkVersion_Extensions.TryParse<IPhoneSdkVersion> (s, out result);
		}

		public int [] Version { get { return version; } }

		public override string ToString ()
		{
			return IAppleSdkVersion_Extensions.ToString (this);
		}

		public int CompareTo (IPhoneSdkVersion other)
		{
			return IAppleSdkVersion_Extensions.CompareTo (this, other);
		}

		public bool Equals (IAppleSdkVersion other)
		{
			return IAppleSdkVersion_Extensions.Equals (this, other);
		}

		public bool Equals (IPhoneSdkVersion other)
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

		public static bool operator == (IPhoneSdkVersion a, IPhoneSdkVersion b)
		{
			return a.Equals (b);
		}

		public static bool operator != (IPhoneSdkVersion a, IPhoneSdkVersion b)
		{
			return !a.Equals (b);
		}

		public static bool operator < (IPhoneSdkVersion a, IPhoneSdkVersion b)
		{
			return a.CompareTo (b) < 0;
		}

		public static bool operator > (IPhoneSdkVersion a, IPhoneSdkVersion b)
		{
			return a.CompareTo (b) > 0;
		}

		public static bool operator <= (IPhoneSdkVersion a, IPhoneSdkVersion b)
		{
			return a.CompareTo (b) <= 0;
		}

		public static bool operator >= (IPhoneSdkVersion a, IPhoneSdkVersion b)
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
		public IAppleSdkVersion ResolveIfDefault (AppleSdk sdk, bool sim)
		{
			return IsUseDefault ? GetDefault (sdk, sim) : this;
		}

		public static IAppleSdkVersion GetDefault (AppleSdk sdk, bool sim)
		{
			var v = sdk.GetInstalledSdkVersions (sim);
			return v.Count > 0 ? v [v.Count - 1] : (IAppleSdkVersion) UseDefault;
		}
#endif

		public static readonly IPhoneSdkVersion UseDefault = new IPhoneSdkVersion (new int [0]);

		public static readonly IPhoneSdkVersion V1_0 = new IPhoneSdkVersion (1, 0);
		public static readonly IPhoneSdkVersion V2_0 = new IPhoneSdkVersion (2, 0);
		public static readonly IPhoneSdkVersion V2_1 = new IPhoneSdkVersion (2, 1);
		public static readonly IPhoneSdkVersion V2_2 = new IPhoneSdkVersion (2, 2);
		public static readonly IPhoneSdkVersion V3_0 = new IPhoneSdkVersion (3, 0);
		public static readonly IPhoneSdkVersion V3_1 = new IPhoneSdkVersion (3, 1);
		public static readonly IPhoneSdkVersion V3_2 = new IPhoneSdkVersion (3, 2);
		public static readonly IPhoneSdkVersion V3_99 = new IPhoneSdkVersion (3, 99);
		public static readonly IPhoneSdkVersion V4_0 = new IPhoneSdkVersion (4, 0);
		public static readonly IPhoneSdkVersion V4_1 = new IPhoneSdkVersion (4, 1);
		public static readonly IPhoneSdkVersion V4_2 = new IPhoneSdkVersion (4, 2);
		public static readonly IPhoneSdkVersion V4_3 = new IPhoneSdkVersion (4, 3);
		public static readonly IPhoneSdkVersion V5_0 = new IPhoneSdkVersion (5, 0);
		public static readonly IPhoneSdkVersion V5_1 = new IPhoneSdkVersion (5, 1);
		public static readonly IPhoneSdkVersion V5_1_1 = new IPhoneSdkVersion (5, 1, 1);
		public static readonly IPhoneSdkVersion V5_2 = new IPhoneSdkVersion (5, 2);
		public static readonly IPhoneSdkVersion V6_0 = new IPhoneSdkVersion (6, 0);
		public static readonly IPhoneSdkVersion V6_1 = new IPhoneSdkVersion (6, 1);
		public static readonly IPhoneSdkVersion V6_2 = new IPhoneSdkVersion (6, 2);
		public static readonly IPhoneSdkVersion V7_0 = new IPhoneSdkVersion (7, 0);
		public static readonly IPhoneSdkVersion V7_1 = new IPhoneSdkVersion (7, 1);
		public static readonly IPhoneSdkVersion V7_2_1 = new IPhoneSdkVersion (7, 2, 1);
		public static readonly IPhoneSdkVersion V8_0 = new IPhoneSdkVersion (8, 0);
		public static readonly IPhoneSdkVersion V8_1 = new IPhoneSdkVersion (8, 1);
		public static readonly IPhoneSdkVersion V8_2 = new IPhoneSdkVersion (8, 2);
		public static readonly IPhoneSdkVersion V8_3 = new IPhoneSdkVersion (8, 3);
		public static readonly IPhoneSdkVersion V8_4 = new IPhoneSdkVersion (8, 4);
		public static readonly IPhoneSdkVersion V9_0 = new IPhoneSdkVersion (9, 0);
		public static readonly IPhoneSdkVersion V9_1 = new IPhoneSdkVersion (9, 1);
		public static readonly IPhoneSdkVersion V9_2 = new IPhoneSdkVersion (9, 2);
		public static readonly IPhoneSdkVersion V9_3 = new IPhoneSdkVersion (9, 3);
		public static readonly IPhoneSdkVersion V10_0 = new IPhoneSdkVersion (10, 0);
		public static readonly IPhoneSdkVersion V10_1 = new IPhoneSdkVersion (10, 1);
		public static readonly IPhoneSdkVersion V10_2 = new IPhoneSdkVersion (10, 2);
		public static readonly IPhoneSdkVersion V10_3 = new IPhoneSdkVersion (10, 3);
		public static readonly IPhoneSdkVersion V10_4 = new IPhoneSdkVersion (10, 4);
		public static readonly IPhoneSdkVersion V11_0 = new IPhoneSdkVersion (11, 0);
	}
}
