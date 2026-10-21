// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.MacDev;
using Xamarin.MacDev.Models;
using static tests.DeviceCtlFixtures;

#nullable enable

namespace tests;

/// <summary>
/// Tests the way code that uses <see cref="PhysicalDeviceService"/> is tested without Xcode or a device: by deriving
/// from it and overriding <see cref="PhysicalDeviceService.ExecuteDeviceCtlAsync"/>, as the dotnet/macios tests do with
/// their own <c>ExecuteCtlAsync</c>. Like such a test, these only use what another assembly can use: the public
/// constructor and the protected method.
/// </summary>
[TestFixture]
public class PhysicalDeviceServiceOverrideTests {

	sealed class OverridingService : PhysicalDeviceService {

		readonly Func<CancellationToken, Task<DeviceCtlRunResult>> run;

		public int Calls { get; private set; }

		public OverridingService (ICustomLogger log, Func<CancellationToken, Task<DeviceCtlRunResult>> run)
			: base (log)
		{
			this.run = run;
		}

		protected override Task<DeviceCtlRunResult> ExecuteDeviceCtlAsync (CancellationToken cancellationToken)
		{
			Calls++;
			return run (cancellationToken);
		}
	}

	RecordingLogger log = new RecordingLogger ();

	[SetUp]
	public void SetUp ()
	{
		log = new RecordingLogger ();
	}

	[Test]
	public async Task ListAsync_OverrideReturningJson_ReturnsTheDevicesOnAnyHost ()
	{
		var json = Document (Device ("COREDEVICE-1", udid: IPhoneUdid, name: "Canned iPhone"));
		var service = new OverridingService (log, _ => Task.FromResult (DeviceCtlRunResult.FromJson (json)));

		var result = await service.ListAsync ();

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.Success));
		Assert.That (result.Devices, Has.Count.EqualTo (1));
		Assert.That (result.Devices [0].Name, Is.EqualTo ("Canned iPhone"));
		Assert.That (service.Calls, Is.EqualTo (1), "devicectl is run once per listing.");
	}

	[Test]
	public async Task ListAsync_OverrideReturningUnsupportedHost_ReturnsUnsupportedHost ()
	{
		var service = new OverridingService (log, _ => Task.FromResult (DeviceCtlRunResult.UnsupportedHost ()));

		var result = await service.ListAsync ();

		Assert.That (result.Status, Is.EqualTo (PhysicalDeviceDiscoveryStatus.UnsupportedHost));
		Assert.That (result.IsSuccess, Is.False);
		Assert.That (result.Devices, Is.Empty);
		Assert.That (result.ErrorMessage, Does.Contain ("macOS"));
	}

	[Test]
	public void ListAsync_OverrideThatThrows_TheExceptionIsNotSwallowed ()
	{
		var service = new OverridingService (log, _ => throw new InvalidOperationException ("canned failure"));

		var exception = Assert.ThrowsAsync<InvalidOperationException> (async () => await service.ListAsync ());

		Assert.That (exception.Message, Is.EqualTo ("canned failure"));
	}

	[Test]
	public void ListAsync_OverrideReturningAFaultedTask_TheExceptionIsNotSwallowed ()
	{
		var service = new OverridingService (log, _ => Task.FromException<DeviceCtlRunResult> (new InvalidOperationException ("canned failure")));

		var exception = Assert.ThrowsAsync<InvalidOperationException> (async () => await service.ListAsync ());

		Assert.That (exception.Message, Is.EqualTo ("canned failure"));
	}
}
