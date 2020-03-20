using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
