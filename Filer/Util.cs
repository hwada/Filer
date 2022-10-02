using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Filer
{
    internal static class Util
    {
        public static string GetFileSize(long size)
        {
            if (size > 1024 * 1024 * 1024)
            {
                return $"{size / (1024 * 1024 * 1024.0):F2}GB";
            }
            else if (size > 1024 * 1024)
            {
                return $"{size / (1024 * 1024.0):F2}MB";
            }
            else if (size > 1024)
            {
                return $"{size / 1024.0:F2}KB";
            }

            return $"{size}B";
        }
    }
}
