// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;

using NUnit.Framework;

using Xamarin.MacDev;

#nullable enable

namespace tests;

[TestFixture]
public class SimulatorServiceExtrasTests {

	// ── SimulatorService construction ────────────────────────────────────────

	[Test]
	public void Constructor_ThrowsOnNullLogger ()
	{
		Assert.Throws<ArgumentNullException> (() => new SimulatorService (null!));
	}

	// ── Privacy service lazy-initialisation ─────────────────────────────────

	[Test]
	public void Privacy_PropertyIsNotNull ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		Assert.That (svc.Privacy, Is.Not.Null);
	}

	[Test]
	public void Privacy_ReturnsSameInstance ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		var first = svc.Privacy;
		Assert.That (first, Is.SameAs (svc.Privacy));
	}

	[Test]
	public void Privacy_Grant_ThrowsOnNullOrEmptyUdid ()
	{
		var p = new SimulatorService (ConsoleLogger.Instance).Privacy;
		Assert.Throws<ArgumentException> (() => p.Grant (null!, PrivacyPermission.Calendar));
		Assert.Throws<ArgumentException> (() => p.Grant ("", PrivacyPermission.Calendar));
		Assert.Throws<ArgumentException> (() => p.Grant ("  ", PrivacyPermission.Calendar));
	}

	[Test]
	public void Privacy_Revoke_ThrowsOnNullOrEmptyUdid ()
	{
		var p = new SimulatorService (ConsoleLogger.Instance).Privacy;
		Assert.Throws<ArgumentException> (() => p.Revoke (null!, PrivacyPermission.Photos));
		Assert.Throws<ArgumentException> (() => p.Revoke ("", PrivacyPermission.Photos));
	}

	[Test]
	public void Privacy_Reset_ThrowsOnNullOrEmptyUdid ()
	{
		var p = new SimulatorService (ConsoleLogger.Instance).Privacy;
		Assert.Throws<ArgumentException> (() => p.Reset (null!, PrivacyPermission.All));
		Assert.Throws<ArgumentException> (() => p.Reset ("", PrivacyPermission.All));
	}

	[Test]
	public void PrivacyPermission_ToSimctlServiceName_ConvertsAllValues ()
	{
		Assert.That (SimulatorPrivacy.ToSimctlServiceName (PrivacyPermission.All), Is.EqualTo ("all"));
		Assert.That (SimulatorPrivacy.ToSimctlServiceName (PrivacyPermission.Calendar), Is.EqualTo ("calendar"));
		Assert.That (SimulatorPrivacy.ToSimctlServiceName (PrivacyPermission.ContactsLimited), Is.EqualTo ("contacts-limited"));
		Assert.That (SimulatorPrivacy.ToSimctlServiceName (PrivacyPermission.Contacts), Is.EqualTo ("contacts"));
		Assert.That (SimulatorPrivacy.ToSimctlServiceName (PrivacyPermission.Location), Is.EqualTo ("location"));
		Assert.That (SimulatorPrivacy.ToSimctlServiceName (PrivacyPermission.LocationAlways), Is.EqualTo ("location-always"));
		Assert.That (SimulatorPrivacy.ToSimctlServiceName (PrivacyPermission.PhotosAdd), Is.EqualTo ("photos-add"));
		Assert.That (SimulatorPrivacy.ToSimctlServiceName (PrivacyPermission.Photos), Is.EqualTo ("photos"));
		Assert.That (SimulatorPrivacy.ToSimctlServiceName (PrivacyPermission.MediaLibrary), Is.EqualTo ("media-library"));
		Assert.That (SimulatorPrivacy.ToSimctlServiceName (PrivacyPermission.Microphone), Is.EqualTo ("microphone"));
		Assert.That (SimulatorPrivacy.ToSimctlServiceName (PrivacyPermission.Motion), Is.EqualTo ("motion"));
		Assert.That (SimulatorPrivacy.ToSimctlServiceName (PrivacyPermission.Reminders), Is.EqualTo ("reminders"));
		Assert.That (SimulatorPrivacy.ToSimctlServiceName (PrivacyPermission.Siri), Is.EqualTo ("siri"));
	}

	// ── StatusBar service lazy-initialisation ────────────────────────────────

	[Test]
	public void StatusBar_PropertyIsNotNull ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		Assert.That (svc.StatusBar, Is.Not.Null);
	}

	[Test]
	public void StatusBar_ReturnsSameInstance ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		var first = svc.StatusBar;
		Assert.That (first, Is.SameAs (svc.StatusBar));
	}

	[Test]
	public void StatusBar_Override_ThrowsOnNullOrEmptyUdid ()
	{
		var sb = new SimulatorService (ConsoleLogger.Instance).StatusBar;
		var overrides = new StatusBarOverrides (Time: "09:41");
		Assert.Throws<ArgumentException> (() => sb.Override (null!, overrides));
		Assert.Throws<ArgumentException> (() => sb.Override ("", overrides));
	}

	[Test]
	public void StatusBar_Override_ThrowsOnNullOverrides ()
	{
		var sb = new SimulatorService (ConsoleLogger.Instance).StatusBar;
		Assert.Throws<ArgumentNullException> (() => sb.Override ("booted", null!));
	}

	[Test]
	public void StatusBar_Override_ThrowsOnEmptyOverrides ()
	{
		var sb = new SimulatorService (ConsoleLogger.Instance).StatusBar;
		Assert.Throws<ArgumentException> (() => sb.Override ("booted", new StatusBarOverrides ()));
	}

	[Test]
	public void StatusBar_Clear_ThrowsOnNullOrEmptyUdid ()
	{
		var sb = new SimulatorService (ConsoleLogger.Instance).StatusBar;
		Assert.Throws<ArgumentException> (() => sb.Clear (null!));
		Assert.Throws<ArgumentException> (() => sb.Clear (""));
	}

	[Test]
	public void StatusBar_BatteryState_ConvertsAllValues ()
	{
		Assert.That (SimulatorStatusBar.ToSimctlBatteryState (SimulatorBatteryState.Charging), Is.EqualTo ("charging"));
		Assert.That (SimulatorStatusBar.ToSimctlBatteryState (SimulatorBatteryState.Charged), Is.EqualTo ("charged"));
		Assert.That (SimulatorStatusBar.ToSimctlBatteryState (SimulatorBatteryState.Discharging), Is.EqualTo ("discharging"));
	}

	[Test]
	public void StatusBar_DataNetwork_ConvertsAllValues ()
	{
		Assert.That (SimulatorStatusBar.ToSimctlDataNetwork (SimulatorDataNetwork.Wifi), Is.EqualTo ("wifi"));
		Assert.That (SimulatorStatusBar.ToSimctlDataNetwork (SimulatorDataNetwork.ThreeG), Is.EqualTo ("3g"));
		Assert.That (SimulatorStatusBar.ToSimctlDataNetwork (SimulatorDataNetwork.FourG), Is.EqualTo ("4g"));
		Assert.That (SimulatorStatusBar.ToSimctlDataNetwork (SimulatorDataNetwork.Lte), Is.EqualTo ("lte"));
		Assert.That (SimulatorStatusBar.ToSimctlDataNetwork (SimulatorDataNetwork.LteA), Is.EqualTo ("lte-a"));
		Assert.That (SimulatorStatusBar.ToSimctlDataNetwork (SimulatorDataNetwork.LtePlus), Is.EqualTo ("lte+"));
		Assert.That (SimulatorStatusBar.ToSimctlDataNetwork (SimulatorDataNetwork.FiveG), Is.EqualTo ("5g"));
		Assert.That (SimulatorStatusBar.ToSimctlDataNetwork (SimulatorDataNetwork.FiveGPlus), Is.EqualTo ("5g+"));
		Assert.That (SimulatorStatusBar.ToSimctlDataNetwork (SimulatorDataNetwork.FiveGUc), Is.EqualTo ("5g-uc"));
		Assert.That (SimulatorStatusBar.ToSimctlDataNetwork (SimulatorDataNetwork.FiveGA), Is.EqualTo ("5g-a"));
	}

	// ── Location service lazy-initialisation ─────────────────────────────────

	[Test]
	public void Location_PropertyIsNotNull ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		Assert.That (svc.Location, Is.Not.Null);
	}

	[Test]
	public void Location_ReturnsSameInstance ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		var first = svc.Location;
		Assert.That (first, Is.SameAs (svc.Location));
	}

	[Test]
	public void Location_Set_ThrowsOnNullOrEmptyUdid ()
	{
		var loc = new SimulatorService (ConsoleLogger.Instance).Location;
		Assert.Throws<ArgumentException> (() => loc.Set (null!, 37.33, -122.03));
		Assert.Throws<ArgumentException> (() => loc.Set ("", 37.33, -122.03));
	}

	[Test]
	public void Location_Clear_ThrowsOnNullOrEmptyUdid ()
	{
		var loc = new SimulatorService (ConsoleLogger.Instance).Location;
		Assert.Throws<ArgumentException> (() => loc.Clear (null!));
		Assert.Throws<ArgumentException> (() => loc.Clear (""));
	}

	[Test]
	public void Location_Run_ThrowsOnNullOrEmptyUdidOrPath ()
	{
		var loc = new SimulatorService (ConsoleLogger.Instance).Location;
		Assert.Throws<ArgumentException> (() => loc.Run (null!, "/tmp/route.gpx"));
		Assert.Throws<ArgumentException> (() => loc.Run ("", "/tmp/route.gpx"));
		Assert.Throws<ArgumentException> (() => loc.Run ("booted", null!));
		Assert.Throws<ArgumentException> (() => loc.Run ("booted", ""));
	}

	// ── ScreenCapture service lazy-initialisation ────────────────────────────

	[Test]
	public void ScreenCapture_PropertyIsNotNull ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		Assert.That (svc.ScreenCapture, Is.Not.Null);
	}

	[Test]
	public void ScreenCapture_ReturnsSameInstance ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		var first = svc.ScreenCapture;
		Assert.That (first, Is.SameAs (svc.ScreenCapture));
	}

	[Test]
	public void ScreenCapture_Screenshot_ThrowsOnNullOrEmptyUdid ()
	{
		var sc = new SimulatorService (ConsoleLogger.Instance).ScreenCapture;
		Assert.Throws<ArgumentException> (() => sc.Screenshot (null!, "/tmp/out.png"));
		Assert.Throws<ArgumentException> (() => sc.Screenshot ("", "/tmp/out.png"));
	}

	[Test]
	public void ScreenCapture_Screenshot_ThrowsOnNullOrEmptyPath ()
	{
		var sc = new SimulatorService (ConsoleLogger.Instance).ScreenCapture;
		Assert.Throws<ArgumentException> (() => sc.Screenshot ("booted", null!));
		Assert.Throws<ArgumentException> (() => sc.Screenshot ("booted", ""));
	}

	[Test]
	public void ScreenCapture_StartRecording_ThrowsOnNullOrEmptyUdid ()
	{
		var sc = new SimulatorService (ConsoleLogger.Instance).ScreenCapture;
		Assert.Throws<ArgumentException> (() => sc.StartRecording (null!, "/tmp/out.mp4"));
		Assert.Throws<ArgumentException> (() => sc.StartRecording ("", "/tmp/out.mp4"));
	}

	[Test]
	public void ScreenCapture_StartRecording_ThrowsOnNullOrEmptyPath ()
	{
		var sc = new SimulatorService (ConsoleLogger.Instance).ScreenCapture;
		Assert.Throws<ArgumentException> (() => sc.StartRecording ("booted", null!));
		Assert.Throws<ArgumentException> (() => sc.StartRecording ("booted", ""));
	}

	[Test]
	public void ScreenCapture_ScreenshotFormat_ConvertsAllValues ()
	{
		Assert.That (SimulatorScreenCapture.ToSimctlFormatName (ScreenshotFormat.Png), Is.EqualTo ("png"));
		Assert.That (SimulatorScreenCapture.ToSimctlFormatName (ScreenshotFormat.Jpeg), Is.EqualTo ("jpeg"));
		Assert.That (SimulatorScreenCapture.ToSimctlFormatName (ScreenshotFormat.Tiff), Is.EqualTo ("tiff"));
		Assert.That (SimulatorScreenCapture.ToSimctlFormatName (ScreenshotFormat.Bmp), Is.EqualTo ("bmp"));
	}

	[Test]
	public void ScreenCapture_VideoRecordingFormat_ConvertsAllValues ()
	{
		Assert.That (SimulatorScreenCapture.ToSimctlVideoFormatName (VideoRecordingFormat.Mp4), Is.EqualTo ("mp4"));
		Assert.That (SimulatorScreenCapture.ToSimctlVideoFormatName (VideoRecordingFormat.H264), Is.EqualTo ("h264"));
		Assert.That (SimulatorScreenCapture.ToSimctlVideoFormatName (VideoRecordingFormat.Fmp4), Is.EqualTo ("fmp4"));
		Assert.That (SimulatorScreenCapture.ToSimctlVideoFormatName (VideoRecordingFormat.Gif), Is.EqualTo ("gif"));
	}

	// ── Direct methods: SetAppearance / GetAppearance ────────────────────────

	[Test]
	public void SetAppearance_ThrowsOnNullOrEmptyUdid ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		Assert.Throws<ArgumentException> (() => svc.SetAppearance (null!, SimulatorAppearance.Dark));
		Assert.Throws<ArgumentException> (() => svc.SetAppearance ("", SimulatorAppearance.Dark));
	}

	[Test]
	public void GetAppearance_ThrowsOnNullOrEmptyUdid ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		Assert.Throws<ArgumentException> (() => svc.GetAppearance (null!));
		Assert.Throws<ArgumentException> (() => svc.GetAppearance (""));
	}

	// ── Direct method: OpenUrl ───────────────────────────────────────────────

	[Test]
	public void OpenUrl_ThrowsOnNullOrEmptyUdid ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		Assert.Throws<ArgumentException> (() => svc.OpenUrl (null!, "https://example.com"));
		Assert.Throws<ArgumentException> (() => svc.OpenUrl ("", "https://example.com"));
	}

	[Test]
	public void OpenUrl_ThrowsOnNullOrEmptyUrl ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		Assert.Throws<ArgumentException> (() => svc.OpenUrl ("booted", null!));
		Assert.Throws<ArgumentException> (() => svc.OpenUrl ("booted", ""));
	}

	// ── Direct method: Push ──────────────────────────────────────────────────

	[Test]
	public void Push_ThrowsOnNullOrEmptyUdid ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		Assert.Throws<ArgumentException> (() => svc.Push (null!, "com.example.app", "{}"));
		Assert.Throws<ArgumentException> (() => svc.Push ("", "com.example.app", "{}"));
	}

	[Test]
	public void Push_ThrowsOnNullOrEmptyBundleId ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		Assert.Throws<ArgumentException> (() => svc.Push ("booted", null!, "{}"));
		Assert.Throws<ArgumentException> (() => svc.Push ("booted", "", "{}"));
	}

	[Test]
	public void Push_ThrowsOnNullOrEmptyPayload ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		Assert.Throws<ArgumentException> (() => svc.Push ("booted", "com.example.app", null!));
		Assert.Throws<ArgumentException> (() => svc.Push ("booted", "com.example.app", ""));
	}

	// ── Direct method: AddMedia ──────────────────────────────────────────────

	[Test]
	public void AddMedia_ThrowsOnNullOrEmptyUdid ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		Assert.Throws<ArgumentException> (() => svc.AddMedia (null!, new [] { "/tmp/photo.png" }));
		Assert.Throws<ArgumentException> (() => svc.AddMedia ("", new [] { "/tmp/photo.png" }));
	}

	[Test]
	public void AddMedia_ThrowsOnNullPaths ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		Assert.Throws<ArgumentNullException> (() => svc.AddMedia ("booted", null!));
	}

	[Test]
	public void AddMedia_ThrowsOnEmptyPathsList ()
	{
		var svc = new SimulatorService (ConsoleLogger.Instance);
		Assert.Throws<ArgumentException> (() => svc.AddMedia ("booted", new List<string> ()));
	}

	// ── StatusBarOverrides record ─────────────────────────────────────────────

	[Test]
	public void StatusBarOverrides_DefaultsToAllNull ()
	{
		var o = new StatusBarOverrides ();
		Assert.That (o.Time, Is.Null);
		Assert.That (o.BatteryLevel, Is.Null);
		Assert.That (o.BatteryState, Is.Null);
		Assert.That (o.DataNetwork, Is.Null);
		Assert.That (o.CellularBars, Is.Null);
		Assert.That (o.WifiBars, Is.Null);
		Assert.That (o.OperatorName, Is.Null);
	}

	[Test]
	public void StatusBarOverrides_CanSetAllFields ()
	{
		var o = new StatusBarOverrides (
			Time: "09:41",
			BatteryLevel: 100,
			BatteryState: SimulatorBatteryState.Charging,
			DataNetwork: SimulatorDataNetwork.Wifi,
			CellularBars: 4,
			WifiBars: 3,
			OperatorName: "MAUI Mobile");

		Assert.That (o.Time, Is.EqualTo ("09:41"));
		Assert.That (o.BatteryLevel, Is.EqualTo (100));
		Assert.That (o.BatteryState, Is.EqualTo (SimulatorBatteryState.Charging));
		Assert.That (o.DataNetwork, Is.EqualTo (SimulatorDataNetwork.Wifi));
		Assert.That (o.CellularBars, Is.EqualTo (4));
		Assert.That (o.WifiBars, Is.EqualTo (3));
		Assert.That (o.OperatorName, Is.EqualTo ("MAUI Mobile"));
	}
}
