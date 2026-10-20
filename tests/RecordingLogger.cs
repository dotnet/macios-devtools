// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.MacDev;

#nullable enable

namespace tests;

/// <summary>An <see cref="ICustomLogger"/> that records every message, whatever its level.</summary>
sealed class RecordingLogger : ICustomLogger {

	readonly object sync = new object ();
	readonly List<string> messages = new List<string> ();
	readonly List<string> warnings = new List<string> ();

	/// <summary>Every message that was logged, in order.</summary>
	public IReadOnlyList<string> Messages {
		get {
			lock (sync)
				return messages.ToArray ();
		}
	}

	/// <summary>The messages that were logged as warnings or errors, in order.</summary>
	public IReadOnlyList<string> Warnings {
		get {
			lock (sync)
				return warnings.ToArray ();
		}
	}

	public void LogError (string message, Exception? ex)
	{
		var text = ex is null ? message : $"{message}: {ex.Message}";
		Record (text, isWarning: true);
	}

	public void LogWarning (string messageFormat, params object? [] args) => Record (Format (messageFormat, args), isWarning: true);

	public void LogInfo (string messageFormat, params object? [] args) => Record (Format (messageFormat, args), isWarning: false);

	public void LogDebug (string messageFormat, params object? [] args) => Record (Format (messageFormat, args), isWarning: false);

	static string Format (string messageFormat, object? [] args) => string.Format (CultureInfo.InvariantCulture, messageFormat, args);

	void Record (string text, bool isWarning)
	{
		lock (sync) {
			messages.Add (text);
			if (isWarning)
				warnings.Add (text);
		}
	}
}
