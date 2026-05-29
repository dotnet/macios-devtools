// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using NUnit.Framework;

using Xamarin.MacDev;

namespace Tests {

[TestFixture]
public class PlatformAvailabilityTests {

[Test]
public void Platform_FullName_UsesVersionWhenSpecified ()
{
var platform = new Platform (PlatformName.iOS, major: 18, minor: 2);
Assert.That (platform.FullName, Is.EqualTo ("iOS 18.2"));
}

[Test]
public void Platform_FullName_UsesArchitectureWhenNoVersion ()
{
var arch32 = new Platform (PlatformName.MacOSX, PlatformArchitecture.Arch32);
Assert.That (arch32.FullName, Is.EqualTo ("Mac OS X 32-bit"));

var arch64 = new Platform (PlatformName.TvOS, PlatformArchitecture.Arch64);
Assert.That (arch64.FullName, Is.EqualTo ("tvOS 64-bit"));
}

[Test]
public void Platform_VersionCompare_OrdersVersions ()
{
var newer = new Platform (PlatformName.iOS, major: 18, minor: 2, subminor: 1);
var older = new Platform (PlatformName.iOS, major: 18, minor: 2, subminor: 0);

Assert.That (newer.VersionCompare (older), Is.EqualTo (1));
Assert.That (older.VersionCompare (newer), Is.EqualTo (-1));
Assert.That (newer.VersionCompare (null), Is.EqualTo (1));
}

[Test]
public void Platform_ToString_ReturnsEmptyWhenUnspecified ()
{
var platform = new Platform (PlatformName.iOS, PlatformArchitecture.None);
Assert.That (platform.ToString (), Is.Empty);
}

[Test]
public void PlatformSet_OrOperator_HandlesNullOperands ()
{
var value = new PlatformSet ((ulong) 0x000A000100090002);

var leftNull = null as PlatformSet;
var rightNull = null as PlatformSet;

Assert.That ((leftNull | rightNull), Is.Null);
Assert.That ((leftNull | value).iOS.Major, Is.EqualTo (9));
Assert.That ((value | rightNull).MacOSX.Major, Is.EqualTo (10));
}

[Test]
public void PlatformSet_ToString_CombinesSpecifiedPlatforms ()
{
var set = new PlatformSet ();
set.iOS.Major = 18;
set.iOS.Minor = 0;
set.iOS.Architecture = PlatformArchitecture.All;
set.MacOSX.Architecture = PlatformArchitecture.Arch64;

var value = set.ToString ();
Assert.That (value, Does.Contain ("Platform.iOS_18_0"));
Assert.That (value, Does.Contain ("Arch64"));
}

[Test]
public void PlatformAvailability_Getters_SelectExpectedPlatform ()
{
var availability = new PlatformAvailability {
Introduced = new PlatformSet (),
Deprecated = new PlatformSet (),
Obsoleted = new PlatformSet (),
Unavailable = new PlatformSet (),
};

availability.Introduced.iOS.Major = 16;
availability.Deprecated.WatchOS.Major = 9;
availability.Obsoleted.TvOS.Major = 18;
availability.Unavailable.MacOSX.Major = 10;

Assert.That (availability.GetIntroduced (PlatformName.iOS).Major, Is.EqualTo (16));
Assert.That (availability.GetDeprecated (PlatformName.WatchOS).Major, Is.EqualTo (9));
Assert.That (availability.GetObsoleted (PlatformName.TvOS).Major, Is.EqualTo (18));
Assert.That (availability.GetUnavailable (PlatformName.None).Major, Is.EqualTo (10));
}

[Test]
public void PlatformAvailability_IsSpecified_TracksState ()
{
var availability = new PlatformAvailability ();
Assert.That (availability.IsSpecified, Is.False);

availability.Message = "Needs update";
Assert.That (availability.IsSpecified, Is.True);
}

[Test]
public void PlatformAvailability_ToString_FormatsAndEscapesMessage ()
{
var availability = new PlatformAvailability {
Introduced = new PlatformSet (),
Message = "Say \"hello\"",
};
availability.Introduced.iOS.Major = 18;
availability.Introduced.iOS.Minor = 1;

var value = availability.ToString ();
Assert.That (value, Does.StartWith ("[Availability ("));
Assert.That (value, Does.Contain ("Introduced ="));
Assert.That (value, Does.Contain ("Message = \"Say \\\"hello\\\"\""));
}
}
}
