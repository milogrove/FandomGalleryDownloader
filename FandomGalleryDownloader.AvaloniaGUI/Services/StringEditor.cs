using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FandomGalleryDownloader.AvaloniaGUI.Services
{
    internal static class StringEditor
    {
        public static List<string> getPicsList(string sourceCode, bool saveLinks, string path)
        {
            /*
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            */
            string pattern = "data-src=\"https.*?\"";
            MatchCollection matches = Regex.Matches(sourceCode, pattern);
            List<string> pics = new();

            foreach (Match match in matches)
            {
                pics.Add(match.Value);
            }

            for (int i = 0; i < pics.Count; i++)
            {
                pics[i] = pics[i].Substring(10, pics[i].Length - 11);

                pattern = @"(\.png|\.jpg|\.webp|\.jpeg|\.gif)";

                Match match = Regex.Match(pics[i], pattern);
                if (match.Success)
                {
                    int endIndex = match.Index + match.Length;
                    pics[i] = pics[i].Substring(0, endIndex);

                    if (saveLinks)
                    {
                        using (StreamWriter sw = new StreamWriter(Path.Combine(path, "links.txt"), append: true))
                        {
                            sw.WriteLine(pics[i]);
                        }
                    }
                }
            }
            return pics;
        }

        public static string FixInvalidFilename(string fileName)
        {
            char[] invalidChars = Path.GetInvalidFileNameChars();

            foreach (char invalidChar in invalidChars)
            {
                fileName = fileName.Replace(invalidChar, '_');
            }

            return fileName;
        }

        public static bool IsDirectoryPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                return false;
            }

            // Windows
            if (Path.DirectorySeparatorChar == '\\')
            {
                var windowsAbsolutePattern = @"^[a-zA-Z]:\\(?:[^<>:""/\\|?*\r\n]*\\)*[^<>:""/\\|?*\r\n]*$";
                var uncPattern = @"^\\\\[^<>:""/\\|?*\r\n]+(?:\\[^<>:""/\\|?*\r\n]+)*\\?$";

                if (Regex.IsMatch(path, windowsAbsolutePattern) || Regex.IsMatch(path, uncPattern))
                {
                    return true;
                }
            }
            else
            {
                var unixAbsolutePattern = @"^\/(?:[^<>:""/\\|?*\r\n]+\/?)*$";

                if (Regex.IsMatch(path, unixAbsolutePattern))
                {
                    return true;
                }
            }

            var relativePattern = @"^(?:[^<>:""/\\|?*\r\n]+[\/\\]?)*[^<>:""/\\|?*\r\n]*$";

            if (Regex.IsMatch(path, relativePattern))
            {
                return true;
            }

            return false;
        }
    }
}
