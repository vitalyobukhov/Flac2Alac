namespace Converter.FileFormats.Wave.Headers.Chunks
{
    public class Fact
    {
        public const string ChunkIdConstant = "fact";



        public string ChunkId { get; set; }
        public uint ChunkSize { get; set; }
        public uint SampleLength { get; set; }
    }
}
