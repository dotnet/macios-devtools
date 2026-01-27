//
// ManifestExtensions.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using System.Linq;

// Disable until we get around to enable + fix any issues.
#nullable disable

namespace Xamarin.MacDev {
	public static class ManifestExtensions {
		#region CoreFoundation Manifest Keys

		public static PArray GetUTExportedTypeDeclaration (this PDictionary dict)
		{
			return dict.Get<PArray> (ManifestKeys.UTExportedTypeDeclaration);
		}

		public static PArray GetUTImportedTypeDeclaration (this PDictionary dict)
		{
			return dict.Get<PArray> (ManifestKeys.UTImportedTypeDeclaration);
		}

		public static string GetCFBundleDisplayName (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.CFBundleDisplayName);
			return str == null ? null : str.Value;
		}

		public static void SetCFBundleDisplayName (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (ManifestKeys.CFBundleDisplayName);
			else
				dict [ManifestKeys.CFBundleDisplayName] = value;
		}

		public static string GetCFBundleName (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.CFBundleName);
			return str == null ? null : str.Value;
		}

		public static void SetCFBundleName (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (ManifestKeys.CFBundleName);
			else
				dict [ManifestKeys.CFBundleName] = value;
		}

		public static string GetCFBundleExecutable (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.CFBundleExecutable);
			return str == null ? null : str.Value;
		}

		public static void SetCFBundleExecutable (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (ManifestKeys.CFBundleExecutable);
			else
				dict [ManifestKeys.CFBundleExecutable] = value;
		}

		public static string GetCFBundleIconFile (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.CFBundleIconFile);
			return str == null ? null : str.Value;
		}

		public static void SetCFBundleIconFile (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (ManifestKeys.CFBundleIconFile);
			else
				dict [ManifestKeys.CFBundleIconFile] = value;
		}

		public static PArray GetCFBundleIconFiles (this PDictionary dict)
		{
			return dict.Get<PArray> (ManifestKeys.CFBundleIconFiles);
		}

		public static void SetCFBundleIconFiles (this PDictionary dict, PArray array)
		{
			if (array == null)
				dict.Remove (ManifestKeys.CFBundleIconFiles);
			else
				dict [ManifestKeys.CFBundleIconFiles] = array;
		}

		public static PDictionary GetCFBundleIcons (this PDictionary dict)
		{
			return dict.Get<PDictionary> (ManifestKeys.CFBundleIcons);
		}

		public static void SetCFBundleIcons (this PDictionary dict, PDictionary icons)
		{
			if (icons == null)
				dict.Remove (ManifestKeys.CFBundleIcons);
			else
				dict [ManifestKeys.CFBundleIcons] = icons;
		}

		public static string GetCFBundleIdentifier (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.CFBundleIdentifier);
			return str == null ? null : str.Value;
		}

		public static void SetCFBundleIdentifier (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (ManifestKeys.CFBundleIdentifier);
			else
				dict [ManifestKeys.CFBundleIdentifier] = value;
		}

		public static string GetCFBundleShortVersionString (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.CFBundleShortVersionString);
			return str == null ? null : str.Value;
		}

		public static void SetCFBundleShortVersionString (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (ManifestKeys.CFBundleShortVersionString);
			else
				dict [ManifestKeys.CFBundleShortVersionString] = value;
		}

		public static string GetCFBundleVersion (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.CFBundleVersion);
			return str == null ? null : str.Value;
		}

		public static void SetCFBundleVersion (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (ManifestKeys.CFBundleVersion);
			else
				dict [ManifestKeys.CFBundleVersion] = value;
		}

		public static string GetNSMainNibFile (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.NSMainNibFile);
			return str == null ? null : str.Value;
		}

		public static void SetNSMainNibFile (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (ManifestKeys.NSMainNibFile);
			else
				dict [ManifestKeys.NSMainNibFile] = value;
		}

		#endregion

		#region iOS-specific Manifest Keys

		public static string GetMinimumOSVersion (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.MinimumOSVersion);
			return str == null ? null : str.Value;
		}

		public static void SetMinimumOSVersion (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (ManifestKeys.MinimumOSVersion);
			else
				dict [ManifestKeys.MinimumOSVersion] = value;
		}

		public static void SetUIDeviceFamily (this PDictionary dict, IPhoneDeviceType deviceTypes)
		{
			if (deviceTypes == IPhoneDeviceType.NotSet) {
				dict.Remove (ManifestKeys.UIDeviceFamily);
				return;
			}

			PArray arr = new PArray ();
			foreach (var family in deviceTypes.ToDeviceFamily ())
				arr.Add (new PNumber ((int) family));

			dict [ManifestKeys.UIDeviceFamily] = arr;
		}

		public static IPhoneDeviceType GetUIDeviceFamily (this PDictionary dict)
		{
			return GetUIDeviceFamily (dict, ManifestKeys.UIDeviceFamily);
		}

		static AppleDeviceFamily ParseDeviceFamilyFromNumber (PNumber number)
		{
			switch (number.Value) {
			case 1:
				return AppleDeviceFamily.IPhone;
			case 2:
				return AppleDeviceFamily.IPad;
			case 3:
				return AppleDeviceFamily.TV;
			case 4:
				return AppleDeviceFamily.Watch;
			case 6:
				return AppleDeviceFamily.MacCatalystOptimizedForMac;
			default:
				throw new ArgumentOutOfRangeException (string.Format ("Unknown device family: {0}", number.Value));
			}
		}

		static IPhoneDeviceType ParseDeviceTypeFromString (PString value)
		{
			// Sometimes this is a string of the form '1,2' as found
			// in the xcode plist files for xcode 4.4.1
			var devices = IPhoneDeviceType.NotSet;
			var str = (string) value;

			if (!string.IsNullOrEmpty (str)) {
				foreach (var v in str.Split (',')) {
					AppleDeviceFamily family;

					if (v == "5") {
						// Xcode 12 beta 1 adds a '5' for SUPPORTED_DEVICE_FAMILIES here:
						// Contents/Developer/Platforms/AppleTVSimulator.platform/Developer/SDKs/AppleTVSimulator.sdk/SDKSettings.plist
						// the value isn't documented anywhere, and it can't be selected in Xcode's UI,
						// which means we have no idea what it means, so just skip it for now.
						LoggingService.LogWarning ($"Skipped unknown device family: {v}");
						continue;
					}

					if (Enum.TryParse<AppleDeviceFamily> (v, out family))
						devices |= family.ToDeviceType ();
				}
			}

			return devices;
		}

		public static IPhoneDeviceType GetUIDeviceFamily (this PDictionary dict, string key)
		{
			PObject value;
			if (!dict.TryGetValue (key, out value))
				return IPhoneDeviceType.NotSet;

			var val = IPhoneDeviceType.NotSet;
			if (value is PArray) {
				foreach (var element in (PArray) value) {
					var p = element as PString;
					if (p != null)
						val |= ParseDeviceTypeFromString (p);

					var number = element as PNumber;
					if (number != null)
						val |= ParseDeviceFamilyFromNumber (number).ToDeviceType ();
				}
			} else if (value is PNumber) {
				val |= ParseDeviceFamilyFromNumber ((PNumber) value).ToDeviceType ();
			} else if (value is PString) {
				val |= ParseDeviceTypeFromString ((PString) value);
			}
			return val;
		}

		public static string GetUILaunchImageFile (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.UILaunchImageFile);
			return str == null ? null : str.Value;
		}

		public static string GetUILaunchImageFileIPad (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.UILaunchImageFileIPad);
			return str == null ? null : str.Value;
		}

		public static string GetUILaunchImageFileIPhone (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.UILaunchImageFileIPhone);
			return str == null ? null : str.Value;
		}

		public static PString GetUIMainStoryboardFile (this PDictionary dict, bool ipad)
		{
			PString str = null;
			if (ipad)
				str = dict.Get<PString> (ManifestKeys.UIMainStoryboardFileIPad);
			return str ?? dict.Get<PString> (ManifestKeys.UIMainStoryboardFile);
		}

		public static IPhoneDeviceCapabilities GetUIRequiredDeviceCapabilities (this PDictionary dict)
		{
			var capabilities = IPhoneDeviceCapabilities.None;
			PDictionary dictionary;
			PObject value;
			PArray array;

			if (!dict.TryGetValue (ManifestKeys.UIRequiredDeviceCapabilities, out value))
				return capabilities;

			if ((dictionary = value as PDictionary) != null) {
				foreach (var kvp in dictionary) {
					var required = kvp.Value as PBoolean;

					if (required == null || !required.Value)
						continue;

					capabilities |= kvp.Key.ToDeviceCapability ();
				}
			} else if ((array = value as PArray) != null) {
				foreach (var capability in array.OfType<PString> ().Select (x => x.Value))
					capabilities |= capability.ToDeviceCapability ();
			}

			return capabilities;
		}

		public static IPhoneOrientation GetUISupportedInterfaceOrientations (this PDictionary dict, string key)
		{
			var orientations = IPhoneOrientation.None;
			var array = dict.Get<PArray> (key);

			if (array == null)
				return orientations;

			foreach (var s in array.OfType<PString> ()) {
				switch (s.Value) {
				case IPhoneOrientationStrings.Up:
					orientations |= IPhoneOrientation.Portrait;
					break;
				case IPhoneOrientationStrings.Down:
					orientations |= IPhoneOrientation.UpsideDown;
					break;
				case IPhoneOrientationStrings.Left:
					orientations |= IPhoneOrientation.LandscapeLeft;
					break;
				case IPhoneOrientationStrings.Right:
					orientations |= IPhoneOrientation.LandscapeRight;
					break;
				}
			}

			return orientations;
		}

		public static IPhoneOrientation GetUISupportedInterfaceOrientations (this PDictionary dict, bool ipad)
		{
			string key;

			if (ipad && dict.ContainsKey (ManifestKeys.UISupportedInterfaceOrientationsIPad))
				key = ManifestKeys.UISupportedInterfaceOrientationsIPad;
			else
				key = ManifestKeys.UISupportedInterfaceOrientations;

			return GetUISupportedInterfaceOrientations (dict, key);
		}

		public static void SetUISupportedInterfaceOrientations (this PDictionary dict, bool ipad, IPhoneOrientation orientations)
		{
			var key = ipad ? ManifestKeys.UISupportedInterfaceOrientationsIPad : ManifestKeys.UISupportedInterfaceOrientations;
			if (orientations == IPhoneOrientation.None) {
				dict.Remove (key);
				return;
			}

			var arr = new PArray ();
			if ((orientations & IPhoneOrientation.Portrait) != 0)
				arr.Add (IPhoneOrientationStrings.Up);
			if ((orientations & IPhoneOrientation.UpsideDown) != 0)
				arr.Add (IPhoneOrientationStrings.Down);
			if ((orientations & IPhoneOrientation.LandscapeLeft) != 0)
				arr.Add (IPhoneOrientationStrings.Left);
			if ((orientations & IPhoneOrientation.LandscapeRight) != 0)
				arr.Add (IPhoneOrientationStrings.Right);

			dict [key] = arr;
		}

		public static bool IsValidPair (this IPhoneOrientation val)
		{
			var landscape = val & IPhoneOrientation.LandscapeMask;

			// If only 1 landscape orientation is set, then it isn't a valid pair
			if (landscape != IPhoneOrientation.None && landscape != IPhoneOrientation.LandscapeMask)
				return false;

			// If UpsideDown is set but not both, then it isn't a valid pair
			if (val.HasFlag (IPhoneOrientation.UpsideDown) && !val.HasFlag (IPhoneOrientation.Portrait))
				return false;

			return true;
		}

		public static string GetNSExtensionPointIdentifier (this PDictionary dict)
		{
			var ext = dict.Get<PDictionary> ("NSExtension");
			if (ext == null)
				return null;

			var id = ext.Get<PString> ("NSExtensionPointIdentifier");
			if (id == null)
				return null;

			return id.Value;
		}

		public static IOSExtensionPoint? GetAppExtensionPoint (this PDictionary dict)
		{
			var ext = dict.Get<PDictionary> ("NSExtension");
			if (ext == null)
				return null;

			var id = ext.Get<PString> ("NSExtensionPointIdentifier");
			if (id == null)
				return IOSExtensionPoint.Unknown;

			switch (id.Value) {
			case "com.apple.widget-extension":
				return IOSExtensionPoint.Widget;
			case "com.apple.watchkit":
				return IOSExtensionPoint.WatchKit;
			case "com.apple.fileprovider-nonui":
				return IOSExtensionPoint.FileProviderNonUI;
			case "com.apple.keyboard-service":
				return IOSExtensionPoint.KeyboardService;
			case "com.apple.ui-services":
				return IOSExtensionPoint.Action;
			case "com.apple.share-services":
				return IOSExtensionPoint.ShareServices;
			case "com.apple.photo-editing":
				return IOSExtensionPoint.PhotoEditing;
			case "com.apple.broadcast-services":
				return IOSExtensionPoint.BroadcastServices;
			case "com.apple.callkit.call-directory":
				return IOSExtensionPoint.CallDirectory;
			case "com.apple.Safari.content-blocker":
				return IOSExtensionPoint.ContentBlocker;
			case "com.apple.intents-service":
				return IOSExtensionPoint.IntentsService;
			case "com.apple.intents-ui-service":
				return IOSExtensionPoint.IntentsUIService;
			case "com.apple.message-payload-provider":
				return IOSExtensionPoint.MessagePayloadProvider;
			case "com.apple.usernotifications.content-extension":
				return IOSExtensionPoint.NotificationContent;
			case "com.apple.usernotifications.service":
				return IOSExtensionPoint.NotificationService;
			case "com.apple.Safari.sharedlinks-service":
				return IOSExtensionPoint.SharedLinks;
			case "com.apple.spotlight.index":
				return IOSExtensionPoint.SpotlightIndex;
			case "com.apple.authentication-services-credential-provider-ui":
				return IOSExtensionPoint.AuthenticationServicesCredentialProviderUI;
			}

			return IOSExtensionPoint.Unknown;
		}

		static PDictionary GetNSExtensionAttributes (this PDictionary dict)
		{
			var ext = dict.Get<PDictionary> ("NSExtension");
			if (ext == null)
				return null;

			var extAtt = ext.Get<PDictionary> ("NSExtensionAttributes");
			if (extAtt == null)
				return null;

			return extAtt;
		}

		#endregion

		#region Watch App Manifest Keys

		public static bool GetWKWatchKitApp (this PDictionary dict)
		{
			return dict.GetBoolean (ManifestKeys.WKWatchKitApp);
		}

		public static void SetWKWatchKitApp (this PDictionary dict, bool value)
		{
			dict.SetBooleanOrRemove (ManifestKeys.WKWatchKitApp, value);
		}

		public static string GetWKAppBundleIdentifier (this PDictionary dict)
		{
			var extAtt = GetNSExtensionAttributes (dict);

			var str = extAtt.Get<PString> (ManifestKeys.WKAppBundleIdentifier);
			return str == null ? null : str.Value;
		}

		public static void SetWKAppBundleIdentifier (this PDictionary dict, string value)
		{
			var extAtt = GetNSExtensionAttributes (dict);

			if (string.IsNullOrEmpty (value))
				extAtt.Remove (ManifestKeys.WKAppBundleIdentifier);
			else
				extAtt [ManifestKeys.WKAppBundleIdentifier] = value;
		}

		public static string GetWKCompanionAppBundleIdentifier (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.WKCompanionAppBundleIdentifier);
			return str == null ? null : str.Value;
		}

		public static void SetWKCompanionAppBundleIdentifier (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (ManifestKeys.WKCompanionAppBundleIdentifier);
			else
				dict [ManifestKeys.WKCompanionAppBundleIdentifier] = value;
		}

		public static string GetCLKComplicationPrincipalClass (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.CLKComplicationPrincipalClass);
			return str == null ? null : str.Value;
		}

		public static void SetCLKComplicationPrincipalClass (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (ManifestKeys.CLKComplicationPrincipalClass);
			else
				dict [ManifestKeys.CLKComplicationPrincipalClass] = value;
		}

		public static WatchComplicationSupportedFamilies GetCLKComplicationSupportedFamilies (this PDictionary dict, string key)
		{
			var supportedFamilies = WatchComplicationSupportedFamilies.None;
			var array = dict.Get<PArray> (key);

			if (array == null)
				return supportedFamilies;

			foreach (var s in array.OfType<PString> ()) {
				switch (s.Value) {
				case WatchComplicationSupportedFamiliesStrings.ModularSmall:
					supportedFamilies |= WatchComplicationSupportedFamilies.ModularSmall;
					break;
				case WatchComplicationSupportedFamiliesStrings.ModularLarge:
					supportedFamilies |= WatchComplicationSupportedFamilies.ModularLarge;
					break;
				case WatchComplicationSupportedFamiliesStrings.UtilitarianSmall:
					supportedFamilies |= WatchComplicationSupportedFamilies.UtilitarianSmall;
					break;
				case WatchComplicationSupportedFamiliesStrings.UtilitarianSmallFlat:
					supportedFamilies |= WatchComplicationSupportedFamilies.UtilitarianSmallFlat;
					break;
				case WatchComplicationSupportedFamiliesStrings.UtilitarianLarge:
					supportedFamilies |= WatchComplicationSupportedFamilies.UtilitarianLarge;
					break;
				case WatchComplicationSupportedFamiliesStrings.CircularSmall:
					supportedFamilies |= WatchComplicationSupportedFamilies.CircularSmall;
					break;
				case WatchComplicationSupportedFamiliesStrings.ExtraLarge:
					supportedFamilies |= WatchComplicationSupportedFamilies.ExtraLarge;
					break;
				case WatchComplicationSupportedFamiliesStrings.GraphicCorner:
					supportedFamilies |= WatchComplicationSupportedFamilies.GraphicCorner;
					break;
				case WatchComplicationSupportedFamiliesStrings.GraphicBezel:
					supportedFamilies |= WatchComplicationSupportedFamilies.GraphicBezel;
					break;
				case WatchComplicationSupportedFamiliesStrings.GraphicCircular:
					supportedFamilies |= WatchComplicationSupportedFamilies.GraphicCircular;
					break;
				case WatchComplicationSupportedFamiliesStrings.GraphicRectangular:
					supportedFamilies |= WatchComplicationSupportedFamilies.GraphicRectangular;
					break;
				}
			}

			return supportedFamilies;
		}

		public static string GetComplicationGroup (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.CLKComplicationGroup);
			return str == null ? null : str.Value;
		}

		public static void SetComplicationGroup (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (ManifestKeys.CLKComplicationGroup);
			else
				dict [ManifestKeys.CLKComplicationGroup] = value;
		}

		#endregion

		#region Mac-specific Manifest Keys

		public static string GetMinimumSystemVersion (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.LSMinimumSystemVersion);
			return str == null ? null : str.Value;
		}

		public static void SetMinimumSystemVersion (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (ManifestKeys.LSMinimumSystemVersion);
			else
				dict [ManifestKeys.LSMinimumSystemVersion] = value;
		}

		#endregion

		#region Xamarin Studio Manifest Keys

		public static string GetAppIconAssets (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.XSAppIconAssets);
			return str == null ? null : str.Value;
		}

		public static void SetAppIconAssets (this PDictionary dict, string vpath)
		{
			if (string.IsNullOrEmpty (vpath))
				dict.Remove (ManifestKeys.XSAppIconAssets);
			else
				dict [ManifestKeys.XSAppIconAssets] = vpath;
		}

		public static string GetLaunchImageAssets (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.XSLaunchImageAssets);
			return str == null ? null : str.Value;
		}

		public static void SetLaunchImageAssets (this PDictionary dict, string vpath)
		{
			if (string.IsNullOrEmpty (vpath))
				dict.Remove (ManifestKeys.XSLaunchImageAssets);
			else
				dict [ManifestKeys.XSLaunchImageAssets] = vpath;
		}

		public static string GetAccentColorAssets (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.XSAccentColorAssets);
			return str == null ? null : str.Value;
		}

		public static void SetAccentColorAssets (this PDictionary dict, string vpath)
		{
			if (string.IsNullOrEmpty (vpath))
				dict.Remove (ManifestKeys.XSAccentColorAssets);
			else
				dict [ManifestKeys.XSAccentColorAssets] = vpath;
		}

		public static string GetAccentColorName (this PDictionary dict)
		{
			var str = dict.Get<PString> (ManifestKeys.NSAccentColorName);
			return str == null ? null : str.Value;
		}

		public static void SetAccentColorName (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (ManifestKeys.NSAccentColorName);
			else
				dict [ManifestKeys.NSAccentColorName] = value;
		}

		#endregion

		public static bool GetBoolean (this PDictionary dict, string key)
		{
			PObject value;

			if (!dict.TryGetValue<PObject> (key, out value))
				return false;

			var boolean = value as PBoolean;

			if (boolean != null)
				return boolean.Value;

			var str = value as PString;

			if (str != null)
				return str.Value != null && string.Compare (str.Value, "yes", StringComparison.OrdinalIgnoreCase) == 0;

			return false;
		}

		public static void SetBooleanOrRemove (this PDictionary dict, string key, bool value)
		{
			if (value)
				dict [key] = new PBoolean (true);
			else
				dict.Remove (key);
		}

		public static void SetIfNotPresent (this PDictionary dict, string key, string value)
		{
			var str = dict.Get<PString> (key);
			if (str == null || string.IsNullOrEmpty (str.Value))
				dict [key] = new PString (value);
		}

		public static void SetIfNotPresent (this PDictionary dict, string key, bool value)
		{
			var b = dict.Get<PBoolean> (key);
			if (b == null)
				dict [key] = new PBoolean (value);
		}

		/// <summary>
		/// Sets value for key. If item exists for alt key, replace it.
		/// </summary>
		public static void SetOrChange (this PDictionary dict, string key, string altKey, string value)
		{
			var keyItem = dict.Get<PString> (key);
			var altItem = dict.Get<PString> (altKey);
			if (keyItem == null) {
				if (altItem != null) {
					dict.ChangeKey (altItem, key);
					altItem.Value = value;
				} else {
					dict [key] = new PString (value);
				}
			} else {
				if (altItem != null)
					dict.Remove (altKey);
				keyItem.Value = value;
			}
		}
	}

	public static class ManifestKeys {
		public const string BuildMachineOSBuild = "BuildMachineOSBuild";

		public const string CFBundleDevelopmentRegion = "CFBundleDevelopmentRegion";
		public const string CFBundleDisplayName = "CFBundleDisplayName";
		public const string CFBundleDocumentTypes = "CFBundleDocumentTypes";
		public const string CFBundleExecutable = "CFBundleExecutable";
		public const string CFBundleIconFile = "CFBundleIconFile";
		public const string CFBundleIconFiles = "CFBundleIconFiles";
		public const string CFBundleIcons = "CFBundleIcons";
		public const string CFBundleIdentifier = "CFBundleIdentifier";
		public const string CFBundleInfoDictionaryVersion = "CFBundleInfoDictionaryVersion";
		public const string CFBundleName = "CFBundleName";
		public const string CFBundlePackageType = "CFBundlePackageType";
		public const string CFBundlePrimaryIcon = "CFBundlePrimaryIcon";
		public const string CFBundleResourceSpecification = "CFBundleResourceSpecification";
		public const string CFBundleShortVersionString = "CFBundleShortVersionString";
		public const string CFBundleSignature = "CFBundleSignature";
		public const string CFBundleSupportedPlatforms = "CFBundleSupportedPlatforms";
		public const string CFBundleVersion = "CFBundleVersion";

		public const string CFBundleURLIconFile = "CFBundleURLIconFile";
		public const string CFBundleURLName = "CFBundleURLName";
		public const string CFBundleURLSchemes = "CFBundleURLSchemes";
		public const string CFBundleURLTypes = "CFBundleURLTypes";

		public const string CFBundleTypeIconFiles = "CFBundleTypeIconFiles";
		public const string CFBundleTypeName = "CFBundleTypeName";
		public const string CFBundleTypeRole = "CFBundleTypeRole";

		public const string LSItemContentTypes = "LSItemContentTypes";
		public const string LSMinimumSystemVersion = "LSMinimumSystemVersion";
		public const string LSRequiresIPhoneOS = "LSRequiresIPhoneOS";

		public const string MinimumOSVersion = "MinimumOSVersion";

		public const string NSMainNibFile = "NSMainNibFile";
		public const string NSMainNibFileIPad = "NSMainNibFile~ipad";

		public const string UIDeviceFamily = "UIDeviceFamily";
		public const string UILaunchImageFile = "UILaunchImageFile";
		public const string UILaunchImageFileIPad = "UILaunchImageFile~ipad";
		public const string UILaunchImageFileIPhone = "UILaunchImageFile~iphone";
		public const string UILaunchStoryboardName = "UILaunchStoryboardName";
		public const string UIMainNibFile = "UIMainNibFile";
		public const string UIMainStoryboardFile = "UIMainStoryboardFile";
		public const string UIMainStoryboardFileIPad = "UIMainStoryboardFile~ipad";
		public const string UIPrerenderedIcon = "UIPrerenderedIcon";
		public const string UIRequiredDeviceCapabilities = "UIRequiredDeviceCapabilities";
		public const string UIRequiresFullScreen = "UIRequiresFullScreen";
		public const string UIStatusBarHidden = "UIStatusBarHidden";
		public const string UIStatusBarHiddenIPad = "UIStatusBarHidden~ipad";
		public const string UIStatusBarStyle = "UIStatusBarStyle";
		public const string UIApplicationSceneManifest = "UIApplicationSceneManifest";
		public const string UIStatusBarTintParameters = "UIStatusBarTintParameters";
		public const string UISupportedInterfaceOrientations = "UISupportedInterfaceOrientations";
		public const string UISupportedInterfaceOrientationsIPad = "UISupportedInterfaceOrientations~ipad";

		public const string UTExportedTypeDeclarations = "UTExportedTypeDeclarations";
		public const string UTImportedTypeDeclarations = "UTImportedTypeDeclarations";
		public const string UTTypeDescription = "UTTypeDescription";
		public const string UTTypeIdentifier = "UTTypeIdentifier";
		public const string UTTypeSize320IconFile = "UTTypeSize320IconFile";
		public const string UTTypeSize64IconFile = "UTTypeSize64IconFile";
		public const string UTTypeConformsTo = "UTTypeConformsTo";

		public const string WKCompanionAppBundleIdentifier = "WKCompanionAppBundleIdentifier";
		public const string WKAppBundleIdentifier = "WKAppBundleIdentifier";
		public const string WKWatchKitApp = "WKWatchKitApp";

		public const string CLKComplicationPrincipalClass = "CLKComplicationPrincipalClass";
		public const string CLKComplicationSupportedFamilies = "CLKComplicationSupportedFamilies";
		public const string CLKComplicationGroup = "CLKComplicationGroup";

		public const string XSAppIconAssets = "XSAppIconAssets";
		public const string XSLaunchImageAssets = "XSLaunchImageAssets";
		public const string XSAccentColorAssets = "XSAccentColorAssets";
		public const string NSAccentColorName = "NSAccentColorName";

		public const string MapKitDirections = "MKDirectionsApplicationSupportedModes";

		public const string NSAppTransportSecurity = "NSAppTransportSecurity";
		public const string NSExceptionDomains = "NSExceptionDomains";
		public const string NSAllowsArbitraryLoads = "NSAllowsArbitraryLoads";
		public const string NSTemporaryExceptionAllowsInsecureHTTPLoads = "NSTemporaryExceptionAllowsInsecureHTTPLoads";

		public const string UTExportedTypeDeclaration = nameof (UTExportedTypeDeclaration);
		public const string UTImportedTypeDeclaration = nameof (UTImportedTypeDeclaration);
	}

	public static class WatchComplicationSupportedFamiliesStrings {
		public const string ModularSmall = "CLKComplicationFamilyModularSmall";
		public const string ModularLarge = "CLKComplicationFamilyModularLarge";
		public const string UtilitarianSmall = "CLKComplicationFamilyUtilitarianSmall";
		public const string UtilitarianSmallFlat = "CLKComplicationFamilyUtilitarianSmallFlat";
		public const string UtilitarianLarge = "CLKComplicationFamilyUtilitarianLarge";
		public const string CircularSmall = "CLKComplicationFamilyCircularSmall";
		public const string ExtraLarge = "CLKComplicationFamilyExtraLarge";
		public const string GraphicCorner = "CLKComplicationFamilyGraphicCorner";
		public const string GraphicBezel = "CLKComplicationFamilyGraphicBezel";
		public const string GraphicCircular = "CLKComplicationFamilyGraphicCircular";
		public const string GraphicRectangular = "CLKComplicationFamilyGraphicRectangular";
	}

	[Flags]
	public enum WatchComplicationSupportedFamilies {
		None = 0,
		ModularSmall = 1 << 0,  // CLKComplicationFamilyModularSmall
		ModularLarge = 1 << 1,  // CLKComplicationFamilyModularLarge
		UtilitarianSmall = 1 << 2,  // CLKComplicationFamilyUtilitarianSmall
		UtilitarianLarge = 1 << 3,  // CLKComplicationFamilyUtilitarianLarge
		CircularSmall = 1 << 4,  // CLKComplicationFamilyCircularSmall
		UtilitarianSmallFlat = 1 << 5,  // CLKComplicationFamilyUtilitarianSmallFlat
		ExtraLarge = 1 << 6,  // CLKComplicationFamilyExtraLarge
		GraphicCorner = 1 << 7,  // CLKComplicationFamilyGraphicCorner
		GraphicBezel = 1 << 8,  // CLKComplicationFamilyGraphicBezel
		GraphicCircular = 1 << 9,  // CLKComplicationFamilyGraphicCircular
		GraphicRectangular = 1 << 10, // CLKComplicationFamilyGraphicRectangular
	}

	public static class IPhoneOrientationStrings {
		public const string Up = "UIInterfaceOrientationPortrait";
		public const string Down = "UIInterfaceOrientationPortraitUpsideDown";
		public const string Left = "UIInterfaceOrientationLandscapeLeft";
		public const string Right = "UIInterfaceOrientationLandscapeRight";
	}

	[Flags]
	public enum IPhoneOrientation {
		None = 0,
		Portrait = 1 << 0,  // UIInterfaceOrientationPortrait
		UpsideDown = 1 << 1,  // UIInterfaceOrientationPortraitUpsideDown
		LandscapeLeft = 1 << 2,  // UIInterfaceOrientationLandscapeLeft
		LandscapeRight = 1 << 3,  // UIInterfaceOrientationLandscapeRight

		PortraitMask = Portrait | UpsideDown,
		LandscapeMask = LandscapeRight | LandscapeLeft,
	}

	public enum IOSExtensionPoint {
		Unknown,
		//com.apple.widget-extension
		Widget,
		//com.apple.watchkit
		WatchKit,
		//com.apple.fileprovider-nonui
		FileProviderNonUI,
		//com.apple.keyboard-service
		KeyboardService,
		//com.apple.ui-services
		Action,
		//com.apple.share-services
		ShareServices,
		//com.apple.photo-editing
		PhotoEditing,
		//com.apple.broadcast-services
		BroadcastServices,
		//com.apple.callkit.call-directory
		CallDirectory,
		//com.apple.Safari.content-blocker
		ContentBlocker,
		//com.apple.intents-service
		IntentsService,
		//com.apple.intents-ui-service
		IntentsUIService,
		//com.apple.message-payload-provider
		MessagePayloadProvider,
		//com.apple.usernotifications.content-extension
		NotificationContent,
		//com.apple.usernotifications.service
		NotificationService,
		//com.apple.Safari.sharedlinks-service
		SharedLinks,
		//com.apple.spotlight.index
		SpotlightIndex,
		// com.apple.authentication-services-credential-provider-ui
		AuthenticationServicesCredentialProviderUI,
	}
}
