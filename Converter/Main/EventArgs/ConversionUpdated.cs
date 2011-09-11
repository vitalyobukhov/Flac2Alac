namespace Converter.Main.EventArgs
{
    public sealed class ConversionUpdated : System.EventArgs
    {
        public int TotalFileCount { get; set; }
        public int ProcessedFileCount { get; set; }
        public ulong TotalSampleCount { get; set; }
        public ulong ProcessedSampleCount { get; set; }
    }
}
