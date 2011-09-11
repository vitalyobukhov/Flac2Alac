namespace Converter.Main.EventArgs
{
    public class PreparationCompleted : System.EventArgs
    {
        public int TotalFileCount { get; set; }
        public ulong TotalSampleCount { get; set; }
    }
}
