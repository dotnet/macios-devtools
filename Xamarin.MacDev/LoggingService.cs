//
// LoggingService.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc.
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
using System.Diagnostics;

// Disable until we get around to enable + fix any issues.
#nullable disable

namespace Xamarin.MacDev {
	public static class LoggingService {
		static ICustomLogger Logger;

		/// <summary>
		/// Sets the logger backend
		/// </summary>
		public static void SetCustomLogger (ICustomLogger customLogger)
		{
			Logger = customLogger;
		}

		public static void LogError (string messageFormat, params object [] args)
		{
			LogError (string.Format (messageFormat, args), (Exception) null);
		}

		public static void LogError (string message, Exception ex)
		{
			if (Logger != null)
				Logger.LogError (message, ex);
		}

		public static void LogWarning (string messageFormat, params object [] args)
		{
			if (Logger != null)
				Logger.LogWarning (messageFormat, args);
		}

		public static void LogInfo (string messageFormat, params object [] args)
		{
			if (Logger != null)
				Logger.LogInfo (messageFormat, args);
		}

		[Conditional ("DEBUG")]
		public static void LogDebug (string messageFormat, params object [] args)
		{
			if (Logger != null)
				Logger.LogDebug (messageFormat, args);
		}
	}

	public interface ICustomLogger {
		void LogError (string message, Exception ex);
		void LogWarning (string messageFormat, params object [] args);
		void LogInfo (string messageFormat, object [] args);
		void LogDebug (string messageFormat, params object [] args);
	}
}
