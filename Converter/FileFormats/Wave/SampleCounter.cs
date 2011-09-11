using Converter.FileFormats.Base;

namespace Converter.FileFormats.Wave
{
    public sealed class SampleCounter : SampleCounterBase<HeaderParser, Wave.Headers.HeaderBase>
    {
        public SampleCounter(HeaderParser parser)
            : base(parser)
        { }

        public SampleCounter(Headers.HeaderBase header)
            : base(header)
        { }



        public override void CalculateSampleCount()
        {
            sampleCount = Header.Data.ChunkSize / Header.Format.DataBlockSize;
        }
    }
}
