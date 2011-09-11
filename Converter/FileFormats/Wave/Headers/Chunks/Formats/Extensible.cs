using System;

namespace Converter.FileFormats.Wave.Headers.Chunks.Formats
{
    public sealed class Extensible : FormatBase
    {
        public ushort SizeOfExtension { get; set; }
        public ushort ValidBitsPerSecond { get; set; }
        public uint SpeakerPositionMask { get; set; }
        public Guid SubFormat { get; set; }
    }
}
