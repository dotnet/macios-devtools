// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.IO;

using NUnit.Framework;

using Xamarin.MacDev;

namespace Tests {

[TestFixture]
public class ExtendedVersionTests {

[Test]
public void Read_ReturnsNull_WhenFileMissing ()
{
var path = Path.Combine (Path.GetTempPath (), Guid.NewGuid ().ToString ("N"));
Assert.That (ExtendedVersion.Read (path), Is.Null);
}

[Test]
public void Read_ParsesKnownFields ()
{
var path = Path.GetTempFileName ();
try {
File.WriteAllLines (path, new [] {
"Version: 9.8.7",
"Hash: abc123",
"Branch: release/main",
"Build date: 2026-05-29",
});

var result = ExtendedVersion.Read (path);
Assert.That (result, Is.Not.Null);
Assert.That (result.Version, Is.EqualTo (new Version (9, 8, 7)));
Assert.That (result.Hash, Is.EqualTo ("abc123"));
Assert.That (result.Branch, Is.EqualTo ("release/main"));
Assert.That (result.BuildDate, Is.EqualTo ("2026-05-29"));
} finally {
File.Delete (path);
}
}

[Test]
public void Read_IgnoresUnknownFieldsAndInvalidVersion ()
{
var path = Path.GetTempFileName ();
try {
File.WriteAllLines (path, new [] {
"Version: not-a-version",
"Unknown: value",
"NoDelimiter",
"Hash: value123",
});

var result = ExtendedVersion.Read (path);
Assert.That (result, Is.Not.Null);
Assert.That (result.Version, Is.Null);
Assert.That (result.Hash, Is.EqualTo ("value123"));
Assert.That (result.Branch, Is.Null);
} finally {
File.Delete (path);
}
}
}
}
