﻿using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace DotnetCoreDirectoriesFilesUtility
{
    internal static class SanitizerHelper
    {
        private static TextInfo textInfo = Thread.CurrentThread.CurrentCulture.TextInfo;
        private static Regex WhiteSpaceRegx = new Regex(@"\s+", RegexOptions.Compiled);
        private static Regex AccentRegx = new Regex(@"\p{Mn}", RegexOptions.Compiled);
        private static Regex SanitizeDirRegx = new Regex(@"([\\\/|<>*:“?])", RegexOptions.Compiled);

        public static string SanitizeDir(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            name = SanitizeDirRegx.Replace(name, "");
            name = AccentRegx.Replace(name.Normalize(NormalizationForm.FormD), "");
            name = WhiteSpaceRegx.Replace(name, "-");

            return textInfo.ToTitleCase(name).Trim();
        }
    }

    internal static class FileHelper
    {
        private static Regex FileIndexRegx = new Regex(@"\(\d*\)", RegexOptions.Compiled);

        public static string AutoIndexFile(this string destinationPath)
        {
            if (!File.Exists(destinationPath))
                return destinationPath;

            int index = 0;
            string fileName = destinationPath.GetFileName();
            string filePath = Path.GetDirectoryName(destinationPath);
            string extension = Path.GetExtension(destinationPath);
            string[] files = Directory.GetFiles(filePath);

            for (int i = 0; i < files.Length; i++)
            {
                string _fileName = files[i].GetFileName();
                if (_fileName.IndexOf(fileName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    string strIdx = FileIndexRegx
                        .Match(_fileName).Value
                        .Replace("(", "")
                        .Replace(")", "");
                    if (!string.IsNullOrWhiteSpace(strIdx))
                    {
                        if (int.TryParse(strIdx, out int idx))
                        {
                            index = idx > index ? idx : index;
                        }
                    }
                }
            }

            return destinationPath.Replace($"{extension}", $" ({index + 1}){extension}");
        }

        private static string GetFileName(this string filePath)
        {
            return Path.GetFileNameWithoutExtension(filePath);
        }
    }
}
