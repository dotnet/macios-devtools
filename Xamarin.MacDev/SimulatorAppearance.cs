// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Xamarin.MacDev;

/// <summary>
/// The UI appearance (theme) of a simulator.
/// Used with <c>xcrun simctl ui &lt;udid&gt; appearance</c>.
/// </summary>
public enum SimulatorAppearance {
	Light,
	Dark,
}
