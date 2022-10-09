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

        public static void CopyDirectory(string source, string destination)
        {
            var dir = new DirectoryInfo(source);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");
            }

            Directory.CreateDirectory(destination);
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destination, file.Name);
                CopyFile(file.FullName, targetFilePath);
            }

            var dirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in dirs)
            {
                string newDestinationDir = Path.Combine(destination, subDir.Name);
                CopyDirectory(subDir.FullName, newDestinationDir);
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

        public static void MoveDirectory(string source, string destination)
        {
            //TODO: 上書き確認
            if (Directory.Exists(destination))
            {
                return;
            }

            Directory.Move(source, destination);
        }

        public static void MoveFile(string source, string destination)
        {
            //TODO: 上書き確認
            if (File.Exists(destination))
            {
                return;
            }

            File.Move(source, destination, true);
        }
    }
}
