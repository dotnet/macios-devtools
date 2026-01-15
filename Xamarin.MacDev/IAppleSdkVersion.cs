using System;
using System.Diagnostics.CodeAnalysis;

#nullable enable

namespace Xamarin.MacDev {
	public interface IAppleSdkVersion : IEquatable<IAppleSdkVersion> {
		bool IsUseDefault { get; }
		void SetVersion (int [] version);
		IAppleSdkVersion GetUseDefault ();
		int [] Version { get; }
	}

	public static class IAppleSdkVersion_Extensions {
		public static IAppleSdkVersion ResolveIfDefault (this IAppleSdkVersion @this, IAppleSdk sdk, bool sim)
		{
			return @this.IsUseDefault ? @this.GetDefault (sdk, sim) : @this;
		}

		public static IAppleSdkVersion GetDefault (this IAppleSdkVersion @this, IAppleSdk sdk, bool sim)
		{
			var v = sdk.GetInstalledSdkVersions (sim);
			return v.Count > 0 ? v [v.Count - 1] : @this.GetUseDefault ();
		}

#if NET
		public static bool TryParse (string? s, [NotNullWhen (true)] out int []? result)
#else
		public static bool TryParse (string? s, out int []? result)
#endif
		{
			if (s == null) {
				result = null;
				return false;
			}

			var vstr = s.Split ('.');
			result = new int [vstr.Length];

			for (int j = 0; j < vstr.Length; j++) {
				int component;
				if (!int.TryParse (vstr [j], out component))
					return false;

				result [j] = component;
			}

			return true;
		}

		public static bool TryParse<T> (string? s, out T result) where T : IAppleSdkVersion, new()
		{
			result = new T ();
			if (s == null)
				return false;

			if (!TryParse (s, out var vint))
				return false;

#if NET
			result.SetVersion (vint);
#else
			result.SetVersion (vint!);
#endif
			return true;
		}

		public static bool TryParse<T> (string s, out IAppleSdkVersion result) where T : IAppleSdkVersion, new()
		{
			var rv = TryParse<T> (s, out T tmp);
			result = tmp;
			return rv;
		}

		public static T Parse<T> (string s) where T : IAppleSdkVersion, new()
		{
			var vstr = s.Split ('.');
			var vint = new int [vstr.Length];
			for (int j = 0; j < vstr.Length; j++)
				vint [j] = int.Parse (vstr [j]);
			var rv = new T ();
			rv.SetVersion (vint);
			return rv;
		}

		public static bool Equals (IAppleSdkVersion a, IAppleSdkVersion b)
		{
			if (a is null && b is null)
				return true;

			if (a is null || b is null)
				return false;

			if ((object) a == (object) b)
				return true;

			var x = a.Version;
			var y = b.Version;
			if (x == null || y == null || x.Length != y.Length)
				return false;

			for (int i = 0; i < x.Length; i++)
				if (x [i] != y [i])
					return false;

			return true;
		}

		public static bool Equals (IAppleSdkVersion @this, object other)
		{
			if (other is IAppleSdkVersion obj)
				return Equals (@this, obj);
			return false;
		}

		public static int CompareTo (IAppleSdkVersion @this, IAppleSdkVersion other)
		{
			var x = @this.Version;
			var y = other.Version;
			if (ReferenceEquals (x, y))
				return 0;

			if (x == null)
				return -1;
			if (y == null)
				return 1;

			for (int i = 0; i < Math.Min (x.Length, y.Length); i++) {
				int res = x [i] - y [i];
				if (res != 0)
					return res;
			}
			return x.Length - y.Length;
		}

		public static string ToString (IAppleSdkVersion @this)
		{
			if (@this.IsUseDefault)
				return "";

			var version = @this.Version;
			var v = new string [version.Length];
			for (int i = 0; i < v.Length; i++)
				v [i] = version [i].ToString ();

			return string.Join (".", v);
		}

		public static int GetHashCode (IAppleSdkVersion @this)
		{
			unchecked {
				var x = @this.Version;
				int acc = 0;
				for (int i = 0; i < x.Length; i++)
					acc ^= x [i] << i;
				return acc;
			}
		}
	}
}
