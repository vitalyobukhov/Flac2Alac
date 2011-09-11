using Converter.FileFormats.Wave.Headers.Chunks;
using Converter.FileFormats.Wave.Headers.Chunks.Formats;

namespace Converter.FileFormats.Wave.Headers
{
    public abstract class HeaderBase : Base.HeaderBase
    {
        public RIFF RIFF { get; set; }
        public FormatBase Format { get;  set; }
        public Data Data { get; set; }
    }
}
