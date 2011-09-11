using Converter.FileFormats.Wave.Headers.Chunks;

namespace Converter.FileFormats.Wave.Headers
{
    public sealed class NonPCM :  HeaderBase
    {
        public new Chunks.Formats.NonPCM Format
        {
            get
            {
                return (Chunks.Formats.NonPCM)base.Format;
            }
            set
            {
                base.Format = value;
            }
        }

        public Fact Fact { get; set; }
    }
}
