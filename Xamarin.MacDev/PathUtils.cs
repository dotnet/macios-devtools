using System;
using System.IO;
using System.Runtime.InteropServices;

#nullable enable

namespace Xamarin.MacDev {
	internal static class PathUtils {
		struct Timespec {
			public IntPtr tv_sec;
			public IntPtr tv_nsec;
		}

		struct Stat { /* when _DARWIN_FEATURE_64_BIT_INODE is defined */
			public uint st_dev;
			public ushort st_mode;
			public ushort st_nlink;
			public ulong st_ino;
			public uint st_uid;
			public uint st_gid;
			public uint st_rdev;
			public Timespec st_atimespec;
			public Timespec st_mtimespec;
			public Timespec st_ctimespec;
			public Timespec st_birthtimespec;
			public ulong st_size;
			public ulong st_blocks;
			public uint st_blksize;
			public uint st_flags;
			public uint st_gen;
			public uint st_lspare;
			public ulong st_qspare_1;
			public ulong st_qspare_2;
		}

		[DllImport ("/usr/lib/libc.dylib", EntryPoint = "lstat$INODE64", SetLastError = true)]
		static extern int lstat_x64 (string file_name, out Stat buf);

		[DllImport ("/usr/lib/libc.dylib", EntryPoint = "lstat", SetLastError = true)]
		static extern int lstat_arm64 (string file_name, out Stat buf);

		static int lstat (string path, out Stat buf)
		{
			if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64) {
				return lstat_arm64 (path, out buf);
			} else {
				return lstat_x64 (path, out buf);
			}
		}

		public static bool IsSymlink (string file)
		{
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				var attr = File.GetAttributes (file);
				return attr.HasFlag (FileAttributes.ReparsePoint);
			}
			Stat buf;
			var rv = lstat (file, out buf);
			if (rv != 0)
				throw new Exception (string.Format ("Could not lstat '{0}': {1}", file, Marshal.GetLastWin32Error ()));
			const int S_IFLNK = 40960;
			return (buf.st_mode & S_IFLNK) == S_IFLNK;
		}

		public static bool IsSymlinkOrHasParentSymlink (string directoryOrFile)
		{
			if (IsSymlink (directoryOrFile))
				return true;

			if (!Directory.Exists (directoryOrFile))
				return false;

			var parentDirectory = Path.GetDirectoryName (directoryOrFile);
			if (string.IsNullOrEmpty (parentDirectory) || parentDirectory == directoryOrFile)
				return false;

			return IsSymlinkOrHasParentSymlink (parentDirectory);
		}
	}
}
