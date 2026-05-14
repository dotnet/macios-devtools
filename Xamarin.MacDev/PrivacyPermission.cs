// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Xamarin.MacDev;

/// <summary>
/// Privacy service categories for <c>xcrun simctl privacy</c>.
/// </summary>
public enum PrivacyPermission {
	All,
	Calendar,
	ContactsLimited,
	Contacts,
	Location,
	LocationAlways,
	PhotosAdd,
	Photos,
	MediaLibrary,
	Microphone,
	Motion,
	Reminders,
	Siri,
}
