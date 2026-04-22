// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

using System;
using System.IO;

using NUnit.Framework;

using Xamarin.MacDev;

namespace tests {

	[TestFixture]
	public class ConsoleLoggerTests {

		[Test]
		public void LogInfo_WritesToStdErr ()
		{
			var logger = ConsoleLogger.Instance;
			var originalErr = Console.Error;
			try {
				using var writer = new StringWriter ();
				Console.SetError (writer);
				logger.LogInfo ("test message {0}", "arg1");
				Assert.That (writer.ToString ().TrimEnd (), Is.EqualTo ("Info: test message arg1"));
			} finally {
				Console.SetError (originalErr);
			}
		}

		[Test]
		public void LogWarning_WritesToStdErr ()
		{
			var logger = ConsoleLogger.Instance;
			var originalErr = Console.Error;
			try {
				using var writer = new StringWriter ();
				Console.SetError (writer);
				logger.LogWarning ("warning {0}", "arg1");
				Assert.That (writer.ToString ().TrimEnd (), Is.EqualTo ("Warning: warning arg1"));
			} finally {
				Console.SetError (originalErr);
			}
		}

		[Test]
		public void LogDebug_WritesToStdErr ()
		{
			var logger = ConsoleLogger.Instance;
			var originalErr = Console.Error;
			try {
				using var writer = new StringWriter ();
				Console.SetError (writer);
				logger.LogDebug ("debug {0}", "arg1");
				Assert.That (writer.ToString ().TrimEnd (), Is.EqualTo ("Debug: debug arg1"));
			} finally {
				Console.SetError (originalErr);
			}
		}

		[Test]
		public void LogError_WritesToStdErr ()
		{
			var logger = ConsoleLogger.Instance;
			var originalErr = Console.Error;
			try {
				using var writer = new StringWriter ();
				Console.SetError (writer);
				logger.LogError ("error message", null);
				Assert.That (writer.ToString ().TrimEnd (), Is.EqualTo ("Error: error message"));
			} finally {
				Console.SetError (originalErr);
			}
		}

		[Test]
		public void LogInfo_DoesNotWriteToStdOut ()
		{
			var logger = ConsoleLogger.Instance;
			var originalOut = Console.Out;
			try {
				using var writer = new StringWriter ();
				Console.SetOut (writer);
				logger.LogInfo ("should not appear");
				Assert.That (writer.ToString (), Is.Empty);
			} finally {
				Console.SetOut (originalOut);
			}
		}

		[Test]
		public void LogWarning_DoesNotWriteToStdOut ()
		{
			var logger = ConsoleLogger.Instance;
			var originalOut = Console.Out;
			try {
				using var writer = new StringWriter ();
				Console.SetOut (writer);
				logger.LogWarning ("should not appear");
				Assert.That (writer.ToString (), Is.Empty);
			} finally {
				Console.SetOut (originalOut);
			}
		}

		[Test]
		public void LogDebug_DoesNotWriteToStdOut ()
		{
			var logger = ConsoleLogger.Instance;
			var originalOut = Console.Out;
			try {
				using var writer = new StringWriter ();
				Console.SetOut (writer);
				logger.LogDebug ("should not appear");
				Assert.That (writer.ToString (), Is.Empty);
			} finally {
				Console.SetOut (originalOut);
			}
		}
	}
}
