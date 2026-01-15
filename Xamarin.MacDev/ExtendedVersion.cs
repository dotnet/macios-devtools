using System;
using System.IO;

// Disable until we get around to enable + fix any issues.
#nullable disable

namespace Xamarin.MacDev {
	public class ExtendedVersion {
		public Version Version { get; set; }
		public string Hash { get; set; }
		public string Branch { get; set; }
		public string BuildDate { get; set; }

		public static ExtendedVersion Read (string file)
		{
			if (!File.Exists (file))
				return null;

			var rv = new ExtendedVersion ();
			foreach (var line in File.ReadAllLines (file)) {
				var colon = line.IndexOf (':');
				if (colon == -1)
					continue;
				var name = line.Substring (0, colon);
				var value = line.Substring (colon + 2);

				switch (name) {
				case "Version":
					Version ev;
					if (Version.TryParse (value, out ev))
						rv.Version = ev;
					break;
				case "Hash":
					rv.Hash = value;
					break;
				case "Branch":
					rv.Branch = value;
					break;
				case "Build date":
					rv.BuildDate = value;
					break;
				}
			}
			return rv;
		}
	}
}

