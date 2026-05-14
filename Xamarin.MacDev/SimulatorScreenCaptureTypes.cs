// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable

namespace Xamarin.MacDev;

/// <summary>
/// Screenshot image format for <c>xcrun simctl io screenshot</c>.
/// </summary>
public enum ScreenshotFormat {
	Png,
	Jpeg,
	Tiff,
	Bmp,
}

/// <summary>
/// Video recording format for <c>xcrun simctl io recordVideo</c>.
/// </summary>
public enum VideoRecordingFormat {
	Mp4,
	H264,
	Fmp4,
	Gif,
}

/// <summary>
/// Options for <c>xcrun simctl io recordVideo</c>.
/// </summary>
public class RecordingOptions {

	/// <summary>The output video format. Defaults to <c>mp4</c> when null.</summary>
	public VideoRecordingFormat? Format { get; set; }

	/// <summary>When true, passes <c>--force</c> to overwrite an existing file.</summary>
	public bool Force { get; set; }
}
