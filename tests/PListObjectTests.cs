//
// TestMobileProvisionIndex.cs
//
// Author: Jeffrey Stedfast <jestedfa@microsoft.com>
//
// Copyright (c) 2017 Microsoft Corp.
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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using NUnit.Framework;
using Xamarin.MacDev;

namespace Tests {
	[TestFixture]
	public class PListObjectTests {
		static readonly KeyValuePair<string, long> [] IntegerKeyValuePairs = new KeyValuePair<string, long> [] {
			new KeyValuePair<string, long> ("Negative1", -1),
			new KeyValuePair<string, long> ("SByteMaxValueMinusOne", sbyte.MaxValue - 1),
			new KeyValuePair<string, long> ("SByteMaxValue", sbyte.MaxValue),
			new KeyValuePair<string, long> ("ByteMaxValueMinusOne", byte.MaxValue - 1),
			new KeyValuePair<string, long> ("ByteMaxValue", byte.MaxValue),
			new KeyValuePair<string, long> ("ShortMaxValueMinusOne", short.MaxValue - 1),
			new KeyValuePair<string, long> ("ShortMaxValue", short.MaxValue),
			new KeyValuePair<string, long> ("UShortMaxValueMinusOne", ushort.MaxValue - 1),
			new KeyValuePair<string, long> ("UShortMaxValue", ushort.MaxValue),
			new KeyValuePair<string, long> ("IntMaxValueMinusOne", int.MaxValue - 1),
			new KeyValuePair<string, long> ("IntMaxValue", int.MaxValue),
			new KeyValuePair<string, long> ("IntMaxValuePlusOne", ((long) int.MaxValue) + 1),
			new KeyValuePair<string, long> ("UIntMaxValue", uint.MaxValue),
			new KeyValuePair<string, long> ("UIntMaxValuePlusOne", ((long) uint.MaxValue) + 1),
			new KeyValuePair<string, long> ("LongMaxValue", long.MaxValue),

            // FIXME: Apple supports up to ulong.MaxValue
            // new KeyValuePair<string, long> ("ULongMaxValue", ulong.MaxValue),
        };

		[TestCase ("xml-integers.plist")]
		[TestCase ("binary-integers.plist")]
		public void TestIntegerDeserialization (string fileName)
		{
			PDictionary plist;

			using (var stream = GetType ().Assembly.GetManifestResourceStream ($"tests.TestData.PropertyLists.{fileName}"))
				plist = (PDictionary) PObject.FromStream (stream);

			Assert.That (plist.Count, Is.EqualTo (IntegerKeyValuePairs.Length));

			foreach (var kvp in IntegerKeyValuePairs) {
				Assert.That (plist.TryGetValue (kvp.Key, out PObject value), Is.True);
				Assert.That (value, Is.InstanceOf<PNumber> ());
				var integer = (PNumber) value;
				Assert.That (integer.Value, Is.EqualTo (kvp.Value));
			}
		}

		[Test]
		public void TestIntegerXmlSerialization ()
		{
			var plist = new PDictionary ();

			foreach (var kvp in IntegerKeyValuePairs)
				plist.Add (kvp.Key, new PNumber (kvp.Value));

			var output = plist.ToXml ();
			string expected;

			using (var stream = GetType ().Assembly.GetManifestResourceStream ("tests.TestData.PropertyLists.xml-integers.plist")) {
				var buffer = new byte [stream.Length];
#if NET7_0_OR_GREATER
				stream.ReadExactly (buffer, 0, buffer.Length);
#else
				stream.Read (buffer, 0, buffer.Length);
#endif

				expected = Encoding.UTF8.GetString (buffer);
			}

			output = output.Replace ("\r\n", "\n").Replace ("\r", "\n");
			expected = expected.Replace ("\r\n", "\n").Replace ("\r", "\n");
			Assert.That (output, Is.EqualTo (expected));
		}

		[Test]
		public void TestIntegerBinarySerialization ()
		{
			var plist = new PDictionary ();

			foreach (var kvp in IntegerKeyValuePairs)
				plist.Add (kvp.Key, new PNumber (kvp.Value));

			var output = plist.ToByteArray (PropertyListFormat.Binary);

			plist = (PDictionary) PObject.FromByteArray (output, 0, output.Length, out var isBinary);

			Assert.That (isBinary, Is.True);
			Assert.That (plist.Count, Is.EqualTo (IntegerKeyValuePairs.Length));

			foreach (var kvp in IntegerKeyValuePairs) {
				Assert.That (plist.TryGetValue (kvp.Key, out PObject value), Is.True);
				Assert.That (value, Is.InstanceOf<PNumber> ());
				var integer = (PNumber) value;
				Assert.That (integer.Value, Is.EqualTo (kvp.Value));
			}
		}

		[Test]
		public void TestStrings ()
		{
			PDictionary plist;

			using (var stream = GetType ().Assembly.GetManifestResourceStream ($"tests.TestData.PropertyLists.strings.plist"))
				plist = (PDictionary) PObject.FromStream (stream);

			Assert.That (plist.Count, Is.EqualTo (2));

			Assert.That (plist.TryGetValue<PString> ("KeyA", out var valueA), Is.True);
			Assert.That (valueA.Value, Is.EqualTo ("ValueA"));
			Assert.That (plist.TryGetStringValue ("KeyA", out var valueAString));
			Assert.That (valueAString, Is.EqualTo ("ValueA"));

			Assert.That (plist.TryGetStringValue ("✅", out var emojiValue), Is.True);
			Assert.That (emojiValue, Is.EqualTo ("❌"));
		}

		string CreateTempPlistFile (string content)
		{
			var path = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName () + ".plist");
			File.WriteAllText (path, content);
			return path;
		}

		static readonly string SamplePlistXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<dict>
	<key>Name</key>
	<string>Test</string>
</dict>
</plist>";

		static readonly string ArrayPlistXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<array>
	<string>A</string>
</array>
</plist>";

		static readonly string StringPlistXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<string>hello world</string>
</plist>";

		static readonly string IntegerPlistXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<integer>42</integer>
</plist>";

		static readonly string RealPlistXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<real>3.14</real>
</plist>";

		static readonly string BooleanPlistXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<true/>
</plist>";

		static readonly string DatePlistXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<date>2024-01-15T10:30:00Z</date>
</plist>";

		static readonly string DataPlistXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE plist PUBLIC ""-//Apple//DTD PLIST 1.0//EN"" ""http://www.apple.com/DTDs/PropertyList-1.0.dtd"">
<plist version=""1.0"">
<data>AQID</data>
</plist>";

		[Test]
		public void OpenFile_ValidDictionary ()
		{
			var path = CreateTempPlistFile (SamplePlistXml);
			try {
				var dict = PDictionary.OpenFile (path);
				Assert.That (dict, Is.Not.Null);
				Assert.That (dict.TryGetStringValue ("Name", out var name), Is.True);
				Assert.That (name, Is.EqualTo ("Test"));
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void OpenFile_ValidDictionaryWithIsBinary ()
		{
			var path = CreateTempPlistFile (SamplePlistXml);
			try {
				var dict = PDictionary.OpenFile (path, out bool isBinary);
				Assert.That (isBinary, Is.False);
				Assert.That (dict, Is.Not.Null);
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void OpenFile_FileNotFound ()
		{
			var path = Path.Combine (Path.GetTempPath (), "nonexistent-" + Path.GetRandomFileName () + ".plist");
			Assert.Throws<FileNotFoundException> (() => PDictionary.OpenFile (path));
		}

		[Test]
		public void OpenFile_NotADictionary ()
		{
			var path = CreateTempPlistFile (ArrayPlistXml);
			try {
				var ex = Assert.Throws<FormatException> (() => PDictionary.OpenFile (path));
				Assert.That (ex.Message, Does.Contain ("does not contain a dictionary"));
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void OpenFile_InvalidContent ()
		{
			var path = CreateTempPlistFile ("this is not valid plist content");
			try {
				Assert.Throws<FormatException> (() => PDictionary.OpenFile (path));
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void TryOpenFile_ValidDictionary ()
		{
			var path = CreateTempPlistFile (SamplePlistXml);
			try {
				Assert.That (PDictionary.TryOpenFile (path, out var dict), Is.True);
				Assert.That (dict, Is.Not.Null);
				Assert.That (dict.TryGetStringValue ("Name", out var name), Is.True);
				Assert.That (name, Is.EqualTo ("Test"));
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void TryOpenFile_ValidDictionaryWithIsBinary ()
		{
			var path = CreateTempPlistFile (SamplePlistXml);
			try {
				Assert.That (PDictionary.TryOpenFile (path, out var dict, out bool isBinary), Is.True);
				Assert.That (isBinary, Is.False);
				Assert.That (dict, Is.Not.Null);
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void TryOpenFile_FileNotFound ()
		{
			var path = Path.Combine (Path.GetTempPath (), "nonexistent-" + Path.GetRandomFileName () + ".plist");
			Assert.That (PDictionary.TryOpenFile (path, out var dict), Is.False);
			Assert.That (dict, Is.Null);
		}

		[Test]
		public void TryOpenFile_NotADictionary ()
		{
			var path = CreateTempPlistFile (ArrayPlistXml);
			try {
				Assert.That (PDictionary.TryOpenFile (path, out var dict), Is.False);
				Assert.That (dict, Is.Null);
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void TryOpenFile_InvalidContent ()
		{
			var path = CreateTempPlistFile ("this is not valid plist content");
			try {
				Assert.That (PDictionary.TryOpenFile (path, out var dict), Is.False);
				Assert.That (dict, Is.Null);
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void FromFile_ReturnsArray ()
		{
			var path = CreateTempPlistFile (ArrayPlistXml);
			try {
				var obj = PObject.FromFile (path, out bool isBinary);
				Assert.That (obj, Is.InstanceOf<PArray> ());
				var array = (PArray) obj!;
				Assert.That (array.Count, Is.EqualTo (1));
				Assert.That (((PString) array [0]).Value, Is.EqualTo ("A"));
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void FromFile_ReturnsString ()
		{
			var path = CreateTempPlistFile (StringPlistXml);
			try {
				var obj = PObject.FromFile (path, out bool isBinary);
				Assert.That (obj, Is.InstanceOf<PString> ());
				Assert.That (((PString) obj!).Value, Is.EqualTo ("hello world"));
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void FromFile_ReturnsInteger ()
		{
			var path = CreateTempPlistFile (IntegerPlistXml);
			try {
				var obj = PObject.FromFile (path, out bool isBinary);
				Assert.That (obj, Is.InstanceOf<PNumber> ());
				Assert.That (((PNumber) obj!).Value, Is.EqualTo (42));
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void FromFile_ReturnsReal ()
		{
			var path = CreateTempPlistFile (RealPlistXml);
			try {
				var obj = PObject.FromFile (path, out bool isBinary);
				Assert.That (obj, Is.InstanceOf<PReal> ());
				Assert.That (((PReal) obj!).Value, Is.EqualTo (3.14));
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void FromFile_ReturnsBoolean ()
		{
			var path = CreateTempPlistFile (BooleanPlistXml);
			try {
				var obj = PObject.FromFile (path, out bool isBinary);
				Assert.That (obj, Is.InstanceOf<PBoolean> ());
				Assert.That (((PBoolean) obj!).Value, Is.True);
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void FromFile_ReturnsDate ()
		{
			var path = CreateTempPlistFile (DatePlistXml);
			try {
				var obj = PObject.FromFile (path, out bool isBinary);
				Assert.That (obj, Is.InstanceOf<PDate> ());
				Assert.That (((PDate) obj!).Value, Is.EqualTo (new DateTime (2024, 1, 15, 10, 30, 0, DateTimeKind.Utc)));
			} finally {
				File.Delete (path);
			}
		}

		[Test]
		public void FromFile_ReturnsData ()
		{
			var path = CreateTempPlistFile (DataPlistXml);
			try {
				var obj = PObject.FromFile (path, out bool isBinary);
				Assert.That (obj, Is.InstanceOf<PData> ());
				Assert.That (((PData) obj!).Value, Is.EqualTo (new byte [] { 1, 2, 3 }));
			} finally {
				File.Delete (path);
			}
		}
	}
}

