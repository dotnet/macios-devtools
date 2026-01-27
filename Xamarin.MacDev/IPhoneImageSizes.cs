// 
// IPhoneImageSizes.cs
//  
// Authors: Michael Hutchinson <mhutch@xamarin.com>
//          Jeffrey Stedfast <jeff@xamarin.com>
// 
// Copyright (c) 2012 Xamarin Inc. (http://xamarin.com)
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

// Disable until we get around to enable + fix any issues.
#nullable disable

namespace Xamarin.MacDev {
	public struct ImageSize {
		public static readonly ImageSize Empty = new ImageSize (0, 0);
		public readonly int Width, Height;

		public ImageSize (int width, int height)
		{
			Width = width;
			Height = height;
		}

		public bool IsEmpty {
			get { return ((Width == 0) && (Height == 0)); }
		}

		public static bool operator == (ImageSize sz1, ImageSize sz2)
		{
			return ((sz1.Width == sz2.Width) && (sz1.Height == sz2.Height));
		}

		public static bool operator != (ImageSize sz1, ImageSize sz2)
		{
			return ((sz1.Width != sz2.Width) || (sz1.Height != sz2.Height));
		}

		public override bool Equals (object obj)
		{
			if (!(obj is ImageSize))
				return false;

			return (this == (ImageSize) obj);
		}

		public override int GetHashCode ()
		{
			return Width ^ Height;
		}

		public override string ToString ()
		{
			return string.Format ("{{Width={0}, Height={1}}}", Width, Height);
		}
	}

	// http://developer.apple.com/library/ios/#qa/qa1686/_index.html
	public static class IPhoneImageSizes {
		public static readonly ImageSize Icon = new ImageSize (57, 57);
		public static readonly ImageSize RetinaIcon = new ImageSize (114, 114);

		public static readonly ImageSize Settings = new ImageSize (29, 29);
		public static readonly ImageSize RetinaSettings = new ImageSize (58, 58);

		public static readonly ImageSize Spotlight = new ImageSize (29, 29);
		public static readonly ImageSize RetinaSpotlight = new ImageSize (58, 58);

		public static readonly ImageSize Launch = new ImageSize (320, 480);
		public static readonly ImageSize LaunchRetina = new ImageSize (640, 960);
		public static readonly ImageSize LaunchRetinaTall = new ImageSize (640, 1136);

		public static readonly ImageSize LaunchRetinaHD47 = new ImageSize (750, 1334);
		public static readonly ImageSize LaunchRetinaHD55 = new ImageSize (1242, 2208);
		public static readonly ImageSize LaunchRetinaHD55Landscape = new ImageSize (2208, 1242);

		public static readonly ImageSize LaunchRetinaX = new ImageSize (1125, 2436);
		public static readonly ImageSize LaunchRetinaXLandscape = new ImageSize (2436, 1125);

		public static readonly ImageSize LaunchRetinaXR = new ImageSize (828, 1792);
		public static readonly ImageSize LaunchRetinaXRLandscape = new ImageSize (1792, 828);

		public static readonly ImageSize LaunchRetinaXSMax = new ImageSize (1242, 2688);
		public static readonly ImageSize LaunchRetinaXSMaxLandscape = new ImageSize (2688, 1242);
	}

	public static class IPadImageSizes {
		public static readonly ImageSize Icon = new ImageSize (72, 72);
		public static readonly ImageSize RetinaIcon = new ImageSize (144, 144);
		public static readonly ImageSize Spotlight = new ImageSize (50, 50);
		public static readonly ImageSize RetinaSpotlight = new ImageSize (100, 100);
		public static readonly ImageSize Settings = new ImageSize (29, 29);
		public static readonly ImageSize RetinaSettings = new ImageSize (58, 58);

		public static readonly ImageSize LaunchPortrait = new ImageSize (768, 1004);
		public static readonly ImageSize LaunchLandscape = new ImageSize (1024, 748);
		public static readonly ImageSize LaunchPortraitFull = new ImageSize (768, 1024);
		public static readonly ImageSize LaunchLandscapeFull = new ImageSize (1024, 768);

		public static readonly ImageSize LaunchRetinaPortrait = new ImageSize (1536, 2008);
		public static readonly ImageSize LaunchRetinaLandscape = new ImageSize (2048, 1496);
		public static readonly ImageSize LaunchRetinaPortraitFull = new ImageSize (1536, 2048);
		public static readonly ImageSize LaunchRetinaLandscapeFull = new ImageSize (2048, 1536);

		public static readonly ImageSize ProLaunchRetinaPortraitFull = new ImageSize (2048, 2732);
		public static readonly ImageSize ProLaunchRetinaLandscapeFull = new ImageSize (2732, 2048);
	}

	public static class IOS7ImageSizes {
		public static readonly ImageSize IPhoneRetinaIcon = new ImageSize (120, 120);
		public static readonly ImageSize IPadProRetinaIcon = new ImageSize (167, 167);
		public static readonly ImageSize IPadRetinaIcon = new ImageSize (152, 152);
		public static readonly ImageSize IPadIcon = new ImageSize (76, 76);

		public static readonly ImageSize RetinaSettings = new ImageSize (58, 58);
		public static readonly ImageSize Settings = new ImageSize (29, 29);

		public static readonly ImageSize RetinaSpotlight = new ImageSize (80, 80);
		public static readonly ImageSize Spotlight = new ImageSize (40, 40);

		public static readonly ImageSize IPhoneRetinaLaunchImage = new ImageSize (640, 960);
		public static readonly ImageSize IPhoneRetinaLaunchImageTall = new ImageSize (640, 1136);
		public static readonly ImageSize IPhoneRetinaHD47LaunchImage = new ImageSize (750, 1334);
		public static readonly ImageSize IPhoneRetinaHD55LaunchImage = new ImageSize (1242, 2208);
		public static readonly ImageSize IPhoneRetinaHD55LaunchImageLandscape = new ImageSize (2208, 1242);

		public static readonly ImageSize IPadLaunchImagePortrait = new ImageSize (768, 1024);
		public static readonly ImageSize IPadLaunchImageLandscape = new ImageSize (1024, 768);

		public static readonly ImageSize IPadRetinaLaunchImagePortrait = new ImageSize (1536, 2048);
		public static readonly ImageSize IPadRetinaLaunchImageLandscape = new ImageSize (2048, 1536);

		public static readonly ImageSize IPadProRetinaLaunchImagePortrait = new ImageSize (2048, 2732);
		public static readonly ImageSize IPadProRetinaLaunchImageLandscape = new ImageSize (2732, 2048);
	}

	public static class AppleWatchImageSizes {
		public static readonly ImageSize NotificationCenter38mm = new ImageSize (44, 44);
		public static readonly ImageSize NotificationCenter42mm = new ImageSize (55, 55);

		public static readonly ImageSize CompanionSettings2x = new ImageSize (58, 58);
		public static readonly ImageSize CompanionSettings3x = new ImageSize (88, 88);

		public static readonly ImageSize AppLauncher38mm = new ImageSize (80, 80);
		public static readonly ImageSize AppLauncher42mm = new ImageSize (88, 88);

		public static readonly ImageSize QuickLook38mm = new ImageSize (172, 172);
		public static readonly ImageSize QuickLook42mm = new ImageSize (196, 196);

		public static readonly ImageSize LaunchImage38mm = new ImageSize (272, 340);
		public static readonly ImageSize LaunchImage42mm = new ImageSize (312, 390);
	}

	public static class AppleTVSizes {
		public static readonly ImageSize FullScreenHD = new ImageSize (1920, 1080);
		public static readonly ImageSize FullScreen4K = new ImageSize (3840, 2160);
	}

	public static class ITunesArtworkSizes {
		public static readonly ImageSize Standard = new ImageSize (512, 512);
		public static readonly ImageSize Retina = new ImageSize (1024, 1024);
	}
}
