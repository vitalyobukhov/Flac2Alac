using Converter.FileFormats.Base;
using Converter.FileFormats.Flac.Metadata;

namespace Converter.FileFormats.Flac
{
    public sealed class SampleCounter : SampleCounterBase<HeaderParser, Header>
    {
        public SampleCounter(HeaderParser parser)
            : base(parser)
        { }

        public SampleCounter(Header header)
            : base(header)
        { }



        public override void CalculateSampleCount()
        {
            sampleCount = Header.StreamInfoBlock.Data.TotalSamples;
        }
    }
}
