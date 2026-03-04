// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using NUnit.Framework;
using Xamarin.MacDev;

#nullable enable

namespace tests;

[TestFixture]
public class RuntimeServiceTests {

	[Test]
	public void Constructor_ThrowsOnNullLogger ()
	{
		Assert.Throws<ArgumentNullException> (() => new RuntimeService (null!));
	}

	[Test]
	public void ListByPlatform_ThrowsOnNullPlatform ()
	{
		var svc = new RuntimeService (ConsoleLogger.Instance);
		Assert.Throws<ArgumentException> (() => svc.ListByPlatform (null!));
		Assert.Throws<ArgumentException> (() => svc.ListByPlatform (""));
	}

	[Test]
	public void DownloadPlatform_ThrowsOnNullPlatform ()
	{
		var svc = new RuntimeService (ConsoleLogger.Instance);
		Assert.Throws<ArgumentException> (() => svc.DownloadPlatform (null!));
		Assert.Throws<ArgumentException> (() => svc.DownloadPlatform (""));
	}

	[Test]
	[Platform ("MacOsX")]
	public void List_DoesNotThrow ()
	{
		var svc = new RuntimeService (ConsoleLogger.Instance);
		Assert.DoesNotThrow (() => svc.List ());
	}

	[Test]
	[Platform ("MacOsX")]
	public void ListByPlatform_DoesNotThrow ()
	{
		var svc = new RuntimeService (ConsoleLogger.Instance);
		Assert.DoesNotThrow (() => svc.ListByPlatform ("iOS"));
	}
}
