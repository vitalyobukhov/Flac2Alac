using Converter.FileFormats.Wave.Headers.Chunks;

namespace Converter.FileFormats.Wave.Headers
{
    public sealed class Extensible : HeaderBase
    {
        public new Chunks.Formats.Extensible Format
        {
            get
            {
                return (Chunks.Formats.Extensible)base.Format;
            }
            set
            {
                base.Format = value;
            }
        }

        public Fact Fact { get; set; }
    }
}
