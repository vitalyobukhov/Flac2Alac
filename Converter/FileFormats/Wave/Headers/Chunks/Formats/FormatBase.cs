namespace Converter.FileFormats.Wave.Headers.Chunks.Formats
{
    public abstract class FormatBase
    {
        public const string ChunkIdConstant = "fmt ";



        public string ChunkId { get; set; }
        public uint ChunkSize { get; set; }

        public FormatCode FormatCode { get; set; }
        public ushort NumberOfChannels { get; set; }
        public uint SampleRate { get; set; }
        public uint DataRate { get; set; }
        public ushort DataBlockSize { get; set; }
        public ushort BitsPerSample { get; set; }
    }
}
