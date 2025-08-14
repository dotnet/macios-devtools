//
// EntitlementExtensions.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2016 Xamarin Inc. (www.xamarin.com)
//

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Xamarin.MacDev {
	public static class EntitlementKeys {
		public const string SystemExtension = "com.apple.developer.system-extension.install";
		public const string UserManagement = "com.apple.developer.user-management";
		public const string Fonts = "com.apple.developer.user-fonts";
		public const string AccessWiFiInfo = "com.apple.developer.networking.wifi-info";
		public const string SignInWithApple = "com.apple.developer.applesignin";
		public const string ClassKit = "com.apple.developer.ClassKit-environment";
		public const string DataProtection = "com.apple.developer.default-data-protection";
		public const string WirelessAccessoryConfiguration = "com.apple.external-accessory.wireless-configuration";
		public const string UbiquityKeyValueStore = "com.apple.developer.ubiquity-kvstore-identifier";
		public const string UbiquityContainers = "com.apple.developer.ubiquity-container-identifiers";
		public const string iCloudContainers = "com.apple.developer.icloud-container-identifiers";
		public const string iCloudServices = "com.apple.developer.icloud-services";
		public const string PassBookIdentifiers = "com.apple.developer.pass-type-identifiers";
		public const string AssociatedDomains = "com.apple.developer.associated-domains";
		public const string ApplicationGroups = "com.apple.security.application-groups";
		public const string NetworkingVpnApi = "com.apple.developer.networking.vpn.api";
		public const string NetworkExtensions = "com.apple.developer.networking.networkextension";
		public const string NFC = "com.apple.developer.nfc.readersession.formats";
		public const string HotspotConfiguration = "com.apple.developer.networking.HotspotConfiguration";
		public const string Multipath = "com.apple.developer.networking.multipath";
		public const string InAppPayments = "com.apple.developer.in-app-payments";
		public const string KeychainAccessGroups = "keychain-access-groups";
		public const string HealthKit = "com.apple.developer.healthkit";
		public const string HomeKit = "com.apple.developer.homekit";
		public const string InterAppAudio = "inter-app-audio";
		public const string AutoFillCredentialProvider = "com.apple.developer.authentication-services.autofill-credential-provider";
		public const string GetTaskAllow = "get-task-allow";
		public const string Siri = "com.apple.developer.siri";
		public const string APS = "aps-environment";

		public const string AllowExecutionOfJitCode = "com.apple.security.cs.allow-jit";
		public const string AllowUnsignedExecutableMemory = "com.apple.security.cs.allow-unsigned-executable-memory";
		public const string AllowDYLDEnvironmentVariable = "com.apple.security.cs.allow-dyld-environment-variables";
		public const string DisableLibraryValidation = "com.apple.security.cs.disable-library-validation";
		public const string DisableExecutableMemoryProtection = "com.apple.security.cs.disable-executable-page-protection";
		public const string DebuggingTool = "com.apple.security.cs.debugger";

		public const string AudioInput = "com.apple.security.device.audio-input";
		public const string Camera = "com.apple.security.device.camera";
		public const string Location = "com.apple.security.personal-information.location";
		public const string AddressBook = "com.apple.security.personal-information.addressbook";
		public const string Calendar = "com.apple.security.personal-information.calendars";
		public const string PhotosLibrary = "com.apple.security.personal-information.photos-library";
		public const string AppleEvents = "com.apple.security.automation.apple-events";

		public const string AppAttest = "com.apple.developer.devicecheck.appattest-environment";
		public const string CommunicatesWithDrivers = "com.apple.developer.driverkit.communicates-with-drivers";
		public const string CommunicationNotifications = "com.apple.developer.usernotifications.communication";
		public const string CustomNetworkProtocol = "com.apple.developer.networking.custom-protocol";
		public const string ExtendedVirtualAddressing = "com.apple.developer.kernel.extended-virtual-addressing";
		public const string FamilyControls = "com.apple.developer.family-controls";
		public const string FileProviderTestingMode = "com.apple.developer.fileprovider.testing-mode";
		public const string GameCenter = "com.apple.developer.game-center";
		public const string GameControllers = "GCSupportedGameControllers";
		public const string GroupActivities = "com.apple.developer.group-session";
		public const string IncreasedMemoryLimit = "com.apple.developer.kernel.increased-memory-limit";
		public const string MDMManagedAssociatedDomains = "com.apple.developer.associated-domains.mdm-managed";
		public const string PushToTalk = "com.apple.developer.push-to-talk";
		public const string SharedWithYou = "com.apple.developer.shared-with-you";
		public const string TimeSensitiveNotifications = "com.apple.developer.usernotifications.time-sensitive";
		public const string WeatherKit = "com.apple.developer.weatherkit";

		static string [] allKeys;

		public static string [] AllKeys {
			get {
				if (allKeys == null) {
					allKeys = typeof (EntitlementKeys).GetFields (BindingFlags.Public | BindingFlags.Static).
						Where (f => f.FieldType == typeof (string)).
						Select (f => (string) f.GetValue (null)).
						ToArray ();
				}

				return allKeys;
			}
		}
	}

	public static class EntitlementExtensions {
		public static PArray GetApplePayMerchants (this PDictionary dict)
		{
			return dict.Get<PArray> (EntitlementKeys.InAppPayments);
		}

		public static void SetApplePayMerchants (this PDictionary dict, PArray value)
		{
			if (value == null)
				dict.Remove (EntitlementKeys.InAppPayments);
			else
				dict [EntitlementKeys.InAppPayments] = value;
		}

		public static PArray GetApplicationGroups (this PDictionary dict)
		{
			return dict.Get<PArray> (EntitlementKeys.ApplicationGroups);
		}

		public static void SetApplicationGroups (this PDictionary dict, PArray value)
		{
			if (value == null)
				dict.Remove (EntitlementKeys.ApplicationGroups);
			else
				dict [EntitlementKeys.ApplicationGroups] = value;
		}

		public static PArray GetAssociatedDomains (this PDictionary dict)
		{
			return dict.Get<PArray> (EntitlementKeys.AssociatedDomains);
		}

		public static void SetAssociatedDomains (this PDictionary dict, PArray value)
		{
			if (value == null)
				dict.Remove (EntitlementKeys.AssociatedDomains);
			else
				dict [EntitlementKeys.AssociatedDomains] = value;
		}

		public static PArray GetiCloudContainers (this PDictionary dict)
		{
			return dict.Get<PArray> (EntitlementKeys.iCloudContainers);
		}

		public static void SetiCloudContainers (this PDictionary dict, PArray value)
		{
			if (value == null)
				dict.Remove (EntitlementKeys.iCloudContainers);
			else
				dict [EntitlementKeys.iCloudContainers] = value;
		}

		public static string GetUbiquityKeyValueStore (this PDictionary dict)
		{
			var str = dict.Get<PString> (EntitlementKeys.UbiquityKeyValueStore);
			return str == null ? null : str.Value;
		}

		public static void SetUbiquityKeyValueStore (this PDictionary dict, string value)
		{
			if (string.IsNullOrEmpty (value))
				dict.Remove (EntitlementKeys.UbiquityKeyValueStore);
			else
				dict [EntitlementKeys.UbiquityKeyValueStore] = value;
		}

		public static PArray GetiCloudServices (this PDictionary dict)
		{
			return dict.Get<PArray> (EntitlementKeys.iCloudServices);
		}

		public static void SetiCloudServices (this PDictionary dict, PArray value)
		{
			if (value == null)
				dict.Remove (EntitlementKeys.iCloudServices);
			else
				dict [EntitlementKeys.iCloudServices] = value;
		}

		public static PArray GetKeychainAccessGroups (this PDictionary dict)
		{
			return dict.Get<PArray> (EntitlementKeys.KeychainAccessGroups);
		}

		public static void SetKeychainAccessGroups (this PDictionary dict, PArray value)
		{
			if (value == null)
				dict.Remove (EntitlementKeys.KeychainAccessGroups);
			else
				dict [EntitlementKeys.KeychainAccessGroups] = value;
		}

		public static PArray GetPassBookIdentifiers (this PDictionary dict)
		{
			return dict.Get<PArray> (EntitlementKeys.PassBookIdentifiers);
		}

		public static void SetPassBookIdentifiers (this PDictionary dict, PArray value)
		{
			if (value == null)
				dict.Remove (EntitlementKeys.PassBookIdentifiers);
			else
				dict [EntitlementKeys.PassBookIdentifiers] = value;
		}

		public static List<string> GetEntitlementKeys (this PDictionary dict)
		{
			var enabledEntitlements = new List<string> ();
			var keys = EntitlementKeys.AllKeys;

			for (int i = 0; i < keys.Length; i++) {
				if (dict.ContainsKey (keys [i]))
					enabledEntitlements.Add (keys [i]);
			}

			return enabledEntitlements;
		}
	}
}
