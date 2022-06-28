﻿//
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
using System.IO;
using System.Text;
using System.Collections.Generic;

using NUnit.Framework;

using Xamarin.MacDev;
using System.Runtime.InteropServices.ComTypes;

namespace UnitTests
{
    [TestFixture]
    public class PListObjectTests
    {
        static readonly KeyValuePair<string, long>[] IntegerKeyValuePairs = new KeyValuePair<string, long>[] {
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
        };

        [TestCase ("xml-integers.plist")]
        [TestCase ("binary-integers.plist")]
        public void TestIntegerDeserialization (string fileName)
        {
            PDictionary plist;

            using (var stream = GetType ().Assembly.GetManifestResourceStream ($"UnitTests.TestData.PropertyLists.{fileName}"))
                plist = (PDictionary) PObject.FromStream (stream);

            Assert.AreEqual (IntegerKeyValuePairs.Length, plist.Count);

            foreach (var kvp in IntegerKeyValuePairs) {
                Assert.IsTrue (plist.TryGetValue (kvp.Key, out PObject value));
                Assert.IsInstanceOf<PNumber> (value);
                var integer = (PNumber) value;
                Assert.AreEqual (kvp.Value, integer.Value);
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

            using (var stream = GetType ().Assembly.GetManifestResourceStream ("UnitTests.TestData.PropertyLists.xml-integers.plist")) {
                var buffer = new byte[stream.Length];
                stream.Read (buffer, 0, buffer.Length);

                expected = Encoding.UTF8.GetString(buffer);
            }

            Assert.AreEqual (expected, output);
        }

        [Test]
        public void TestIntegerBinarySerialization ()
        {
            var plist = new PDictionary ();

            foreach (var kvp in IntegerKeyValuePairs)
                plist.Add (kvp.Key, new PNumber (kvp.Value));

            var output = plist.ToByteArray (PropertyListFormat.Binary);

            plist = (PDictionary) PObject.FromByteArray (output, 0, output.Length, out var isBinary);

            Assert.IsTrue (isBinary);
            Assert.AreEqual (IntegerKeyValuePairs.Length, plist.Count);

            foreach (var kvp in IntegerKeyValuePairs) {
                Assert.IsTrue (plist.TryGetValue (kvp.Key, out PObject value));
                Assert.IsInstanceOf<PNumber> (value);
                var integer = (PNumber) value;
                Assert.AreEqual (kvp.Value, integer.Value);
            }
        }
    }
}

