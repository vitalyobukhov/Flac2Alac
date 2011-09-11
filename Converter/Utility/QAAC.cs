using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;

namespace Converter.Utility
{
    static class QAAC
    {
        public const string ArgumentsTemplate = " \"<input>\" -A -o \"<output>\"";
        public const string InputArgument = "<input>";
        public const string OutputArgument = "<output>";
        public const string OutputFileExtension = ".m4a";
        public const string TemporaryFileExtension = ".tmp";
        public const string ConsoleRegexTotalGroup = "total";
        public const string ConsoleRegexProcessedGroup = "processed";
        public const string ConsoleRegexText = "(?<" + ConsoleRegexProcessedGroup + ">\\d+)/(?<" + ConsoleRegexTotalGroup + ">\\d+) ";
        public const int ConsoleBufferSize = Int16.MaxValue;

        private static Regex consoleRegex;
        private static string moduleFilename;
        private static readonly IEnumerable<string> dependecyFilenames;

        static QAAC()
        {
            dependecyFilenames = new[]
            {
                @"qaac.exe",
                @"msvcp100.dll",
                @"msvcr100.dll",
                @"libFLAC.dll"
            };

            moduleFilename = null;
        }



        public static string ModuleFilename
        {
            get
            {
                if (moduleFilename == null)
                {
                    const string qaacShortFilename = "qaac.exe";

                    moduleFilename = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        qaacShortFilename);
                }

                return moduleFilename;
            }
        }

        public static Regex ConsoleRegex
        {
            get
            {
                if (consoleRegex == null)
                {
                    consoleRegex = new Regex(ConsoleRegexText,
                        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);
                }

                return consoleRegex;
            }
        }



        private static bool CheckDependencyExists(string filename)
        {
            return File.Exists(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), filename));
        }

        public static bool CheckDependencies()
        {
            return dependecyFilenames.All(d => CheckDependencyExists(d));
        }
    }
}
