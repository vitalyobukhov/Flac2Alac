namespace Converter.Main.EventArgs
{
    public sealed class ConversionCanceled : System.EventArgs
    {
        public int SuccessFileCount { get; set; }
        public int FailureFileCount { get; set; }
    }
}
