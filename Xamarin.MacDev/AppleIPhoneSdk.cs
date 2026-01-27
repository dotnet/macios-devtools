// 
// AppleIPhoneSdk.cs
//  
// Authors: Michael Hutchinson <mhutch@xamarin.com>
//          Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2011-2013 Xamarin Inc.
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

using System.Collections.Generic;

// Disable until we get around to enable + fix any issues.
#nullable disable

namespace Xamarin.MacDev {
	public class AppleIPhoneSdk : AppleSdk {
		protected override string SimulatorPlatformName {
			get {
				return "iPhoneSimulator";
			}
		}

		protected override string DevicePlatformName {
			get {
				return "iPhoneOS";
			}
		}

		public AppleIPhoneSdk (string sdkRoot, string versionPlist)
		{
			DeveloperRoot = sdkRoot;
			VersionPlist = versionPlist;
			Init ();
		}
	}

	public class AppleDTSettings {
		public string DTXcodeBuild { get; set; }
		public string DTPlatformVersion { get; set; }
		public string DTPlatformBuild { get; set; }
		public string BuildMachineOSBuild { get; set; }
	}

	public class AppleDTSdkSettings {
		public string CanonicalName { get; set; }
		public string AlternateSDK { get; set; }
		public string DTCompiler { get; set; }
		public string DTSDKBuild { get; set; }
		public IPhoneDeviceType DeviceFamilies { get; set; }
	}
}
