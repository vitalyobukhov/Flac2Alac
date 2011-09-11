using System.Collections.Generic;
using System.Linq;
using System;

namespace Converter.Utility
{
    public static class Path
    {
        private const string AllWildcard = "*.*";

        private static string GetCommonDirectory(IEnumerable<string> filenames)
        {
            if (filenames == null)
            {
                throw new ArgumentNullException("filenames");
            }

            string result = null;

            if (filenames.Count() == 0)
            {
                goto GetCommonDirectoryEnd;
            }

            result = System.IO.Path.GetDirectoryName(filenames.First());
            var root = System.IO.Path.GetPathRoot(result);

            for (var i = 1; i < filenames.Count(); i++)
            {
                var filename = filenames.Skip(i).First();

                while (!filename.Contains(result))
                {
                    if (result == root)
                    {
                        result = null;
                        goto GetCommonDirectoryEnd;
                    }

                    result = System.IO.Directory.GetParent(result).FullName;
                }
            }


GetCommonDirectoryEnd: 
            return result;
        }

        private static string GetCommonDirectoryForOutput(IEnumerable<string> filenames)
        {
            if (filenames == null)
            {
                throw new ArgumentNullException("filenames");
            }

            string result = null;

            if (filenames.Count() <= 1)
            {
                goto GetCommonDirectoryForOutputEnd;
            }

            var directory = System.IO.Path.GetDirectoryName(filenames.First());
            for (var i = 1; i < filenames.Count(); i++)
            {
                if (System.IO.Path.GetDirectoryName(filenames.Skip(i).First()) != directory)
                {
                    break;
                }
                else if (i == filenames.Count() - 1)
                {
                    goto GetCommonDirectoryForOutputEnd;
                }
            }

            result = GetCommonDirectory(filenames);

GetCommonDirectoryForOutputEnd:
            return result;
        }

        private static string GetOutputFilename(string inputFilename, string commonDirectory, string outputDirectory)
        {
            if (inputFilename == null)
            {
                throw new ArgumentNullException("inputFilename");
            }

            if (outputDirectory == null)
            {
                throw new ArgumentNullException("outputDirectory");
            }

            string result;

            if (string.IsNullOrEmpty(commonDirectory))
            {
                result = System.IO.Path.Combine(outputDirectory, System.IO.Path.GetFileName(inputFilename));
            }
            else
            {
                result = System.IO.Path.Combine(outputDirectory, 
                    inputFilename.Substring(commonDirectory.Length).
                        TrimStart(new char[] { System.IO.Path.DirectorySeparatorChar}));
            }

            return result;
        }

        public static IEnumerable<string> ExpandAndFilterPaths(IEnumerable<string> paths, IEnumerable<string> filterPatterns)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }

            if (filterPatterns == null)
            {
                filterPatterns = new[] { AllWildcard };
            }

            var result = new List<string>();

            foreach (var path in paths)
            {
                if (System.IO.Directory.Exists(path))
                {
                    foreach(var pattern in filterPatterns)
                    {
                        result.AddRange(System.IO.Directory.EnumerateFiles(path,
                            pattern, System.IO.SearchOption.AllDirectories));
                    }
                }
                else
                {
                    if (filterPatterns.Any(p => System.IO.Directory.EnumerateFiles(System.IO.Path.GetDirectoryName(path),
                            p, System.IO.SearchOption.TopDirectoryOnly).Any(f => f == path)))
                    {
                        result.Add(path);
                    }
                }
            }

            return result;
        }

        public static IEnumerable<KeyValuePair<string, string>> GetInputOutputFilenameMaps(IEnumerable<string> inputFilenames,
            IEnumerable<string> filterPatterns, string outputDirectory)
        {
            if (inputFilenames == null)
            {
                throw new ArgumentNullException("inputFilenames");
            }

            if (outputDirectory == null)
            {
                throw new ArgumentNullException("outputDirectory");
            }

            var result = new List<KeyValuePair<string, string>>();

            var filenames = ExpandAndFilterPaths(inputFilenames, filterPatterns);
            var commonDirectory = GetCommonDirectoryForOutput(filenames);

            foreach (var filename in filenames)
            {
                result.Add(new KeyValuePair<string, string>(filename, 
                        GetOutputFilename(filename, commonDirectory, outputDirectory)));
            }

            return result;
        }

        public static IEnumerable<string> GetValidPaths(IEnumerable<string> paths)
        {
            if (paths == null)
            {
                throw new ArgumentNullException("paths");
            }

            return paths.Where(p => System.IO.Path.GetInvalidPathChars().All(c => !p.Contains(c)) && 
                (System.IO.File.Exists(p) || System.IO.Directory.Exists(p))).ToList();
        }
    }
}
