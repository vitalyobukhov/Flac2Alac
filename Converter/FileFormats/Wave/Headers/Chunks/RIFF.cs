namespace Converter.FileFormats.Wave.Headers.Chunks
{
    public class RIFF
    {
        public const string ChunkIdConstant = "RIFF";
        public const string WaveIdConstant = "WAVE";

        public string ChunkId { get; set; }
        public uint ChunkSize { get; set; }

        public string WaveId { get; set; }
    }
}
