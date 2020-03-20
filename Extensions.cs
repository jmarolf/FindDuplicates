using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FindDuplicates
{
    public static class Extensions
    {
        public static long AsMegaBytes(this long bytes) => bytes.AsKiloBytes() / 1024;
        public static long AsKiloBytes(this long bytes) => bytes / 1024;
        public static string PrintSize(this long bytes)
        {
            if (bytes.AsMegaBytes() > 0)
            {
                return $"{bytes.AsMegaBytes()} MB";
            }

            if (bytes.AsKiloBytes() > 0)
            {
                return $"{bytes.AsKiloBytes()} KB";
            }

            return $"{bytes} bytes";
        }

        public static long TotalWaste(this List<FileInfo> files)
        {
            var bytes = 0L;
            foreach (var file in files.Skip(1))
            {
                bytes += file.Length;
            }

            return bytes;
        }

        public static bool AreAllSameSize(this List<FileInfo> files)
        {
            return files.Select(x => x.Length).Distinct().Count() < 2;
        }
    }
}
