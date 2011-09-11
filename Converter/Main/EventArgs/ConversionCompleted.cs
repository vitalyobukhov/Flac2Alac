namespace Converter.Main.EventArgs
{
    public sealed class ConversionCompleted : System.EventArgs
    {
        public int SuccessFileCount { get; set; }
        public int FailureFileCount { get; set; }
    }
}
