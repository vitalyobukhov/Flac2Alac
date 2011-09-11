using Converter.FileFormats;
using System.IO;
using System;
using System.Linq;

namespace Converter.Extensions
{
    static class FileFormatExtensions
    {
        const char ManyWildcard = '*';
        const char ExtensionPrefix = '.';
        const string FlacExtension = "flac";
        const string WaveExtension = "wav";

        public static FileFormat GetFileFormatFromFilename(this string filename)
        {
            var result = FileFormat.Unknown;
            var extension = Path.GetExtension(filename).Trim(new[] { ExtensionPrefix }).ToLower();

            switch (extension)
            {
                case FlacExtension:
                    result = FileFormat.Flac;
                    break;

                case WaveExtension:
                    result = FileFormat.Wave;
                    break;
            }

            return result;
        }

        public static string GetFileExtensionFromFileFormat(this FileFormat fileFormat)
        {
            string result = null;

            switch (fileFormat)
            {
                case FileFormat.Flac:
                    result = FlacExtension;
                    break;

                case FileFormat.Wave:
                    result = WaveExtension;
                    break;
            }

            result = string.Concat(ExtensionPrefix, result);

            return result;
        }

        public static string GetFileFilterFromFileFormat(this FileFormat fileFormat)
        {
            var result = GetFileExtensionFromFileFormat(fileFormat);

            result = result != null ? string.Concat(ManyWildcard, result) : null;

            return result;
        }

        public static string[] GetAllFileFormatFilters()
        {
            return Enum.GetValues(typeof(FileFormat))
                .Cast<int>()
                .Select(ffv => (FileFormat)ffv)
                .Where(ff => ff != FileFormat.Unknown)
                .Select(ff => GetFileFilterFromFileFormat(ff))
                .ToArray();
        }
    }
}
