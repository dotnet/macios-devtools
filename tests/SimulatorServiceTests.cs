// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using NUnit.Framework;
using Xamarin.MacDev;

#nullable enable

namespace tests;

[TestFixture]
public class SimulatorServiceTests {

	readonly SimulatorService svc = new SimulatorService (ConsoleLogger.Instance);

	[Test]
	public void Constructor_ThrowsOnNullLogger ()
	{
		Assert.Throws<ArgumentNullException> (() => new SimulatorService (null!));
	}

	// Install

	[Test]
	public void Install_ThrowsOnNullOrEmptyUdid ()
	{
		Assert.Throws<ArgumentException> (() => svc.Install (null!, "/path/to/App.app"));
		Assert.Throws<ArgumentException> (() => svc.Install ("", "/path/to/App.app"));
	}

	[Test]
	public void Install_ThrowsOnNullOrEmptyAppBundlePath ()
	{
		Assert.Throws<ArgumentException> (() => svc.Install ("SOME-UDID", null!));
		Assert.Throws<ArgumentException> (() => svc.Install ("SOME-UDID", ""));
	}

	// Uninstall

	[Test]
	public void Uninstall_ThrowsOnNullOrEmptyUdid ()
	{
		Assert.Throws<ArgumentException> (() => svc.Uninstall (null!, "com.example.App"));
		Assert.Throws<ArgumentException> (() => svc.Uninstall ("", "com.example.App"));
	}

	[Test]
	public void Uninstall_ThrowsOnNullOrEmptyBundleIdentifier ()
	{
		Assert.Throws<ArgumentException> (() => svc.Uninstall ("SOME-UDID", null!));
		Assert.Throws<ArgumentException> (() => svc.Uninstall ("SOME-UDID", ""));
	}

	// Launch

	[Test]
	public void Launch_ThrowsOnNullOrEmptyUdid ()
	{
		Assert.Throws<ArgumentException> (() => svc.Launch (null!, "com.example.App"));
		Assert.Throws<ArgumentException> (() => svc.Launch ("", "com.example.App"));
	}

	[Test]
	public void Launch_ThrowsOnNullOrEmptyBundleIdentifier ()
	{
		Assert.Throws<ArgumentException> (() => svc.Launch ("SOME-UDID", null!));
		Assert.Throws<ArgumentException> (() => svc.Launch ("SOME-UDID", ""));
	}

	// Terminate

	[Test]
	public void Terminate_ThrowsOnNullOrEmptyUdid ()
	{
		Assert.Throws<ArgumentException> (() => svc.Terminate (null!, "com.example.App"));
		Assert.Throws<ArgumentException> (() => svc.Terminate ("", "com.example.App"));
	}

	[Test]
	public void Terminate_ThrowsOnNullOrEmptyBundleIdentifier ()
	{
		Assert.Throws<ArgumentException> (() => svc.Terminate ("SOME-UDID", null!));
		Assert.Throws<ArgumentException> (() => svc.Terminate ("SOME-UDID", ""));
	}

	// GetAppContainer

	[Test]
	public void GetAppContainer_ThrowsOnNullOrEmptyUdid ()
	{
		Assert.Throws<ArgumentException> (() => svc.GetAppContainer (null!, "com.example.App"));
		Assert.Throws<ArgumentException> (() => svc.GetAppContainer ("", "com.example.App"));
	}

	[Test]
	public void GetAppContainer_ThrowsOnNullOrEmptyBundleIdentifier ()
	{
		Assert.Throws<ArgumentException> (() => svc.GetAppContainer ("SOME-UDID", null!));
		Assert.Throws<ArgumentException> (() => svc.GetAppContainer ("SOME-UDID", ""));
	}

	[Test]
	[Platform ("MacOsX")]
	public void GetAppContainer_ReturnsNullForNonExistentApp ()
	{
		// Using a non-existent bundle identifier should return null (simctl exits non-zero)
		var result = svc.GetAppContainer ("booted", "com.example.NoSuchApp.DoesNotExist");
		Assert.That (result, Is.Null);
	}
}
