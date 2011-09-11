namespace Converter.FileFormats.Wave.Headers.Chunks
{
    public class Data
    {
        public const string ChunkIdConstant = "data";



        public string ChunkId { get; set; }
        public uint ChunkSize { get; set; }
    }
}
