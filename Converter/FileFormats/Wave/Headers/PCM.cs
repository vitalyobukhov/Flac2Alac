namespace Converter.FileFormats.Wave.Headers
{
    public sealed class PCM : HeaderBase
    {
        public new Chunks.Formats.PCM Format
        {
            get
            {
                return (Chunks.Formats.PCM)base.Format;
            }
            set
            {
                base.Format = value;
            }
        }
    }
}
