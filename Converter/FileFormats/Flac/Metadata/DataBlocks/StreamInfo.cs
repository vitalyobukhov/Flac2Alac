namespace Converter.FileFormats.Flac.Metadata.DataBlocks
{
    public sealed class StreamInfo : DataBlockBase
    {
        public ushort MinimumBlockSize { get; set; }
        public ushort MaximumBlockSize { get; set; }
        public uint MinimumFrameSize { get; set; }
        public uint MaximumFrameSize { get; set; }
        public uint SampleRate { get; set; }
        public byte NumberOfChannels { get; set; }
        public byte BitsPerSample { get; set; }
        public ulong TotalSamples { get; set; }
        public byte[] MD5 {get; set;}



        public StreamInfo()
        {
            MD5 = new byte[16];
        }
    }
}
