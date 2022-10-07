using System;
using System.Collections.Generic;
using System.IO;
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

        public static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            }

            Directory.CreateDirectory(destinationDir);
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                CopyFile(file.FullName, targetFilePath);
            }

            if (recursive)
            {
                var dirs = dir.GetDirectories();
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        public static void CopyFile(string source, string destination)
        {
            //TODO: 上書き確認
            if (File.Exists(destination))
            {
                return;
            }

            File.Copy(source, destination, true);
        }
    }
}
