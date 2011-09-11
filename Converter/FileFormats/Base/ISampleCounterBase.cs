using System;

namespace Converter.FileFormats.Base
{
    public interface ISampleCounterBase<out TParser, out THeader> : IDisposable
        where TParser : IHeaderParserBase<THeader>
        where THeader : IHeaderBase
    {
        void CalculateSampleCount();
        ulong GetSampleCount();
    }
}
