using System;

namespace Converter.FileFormats.Base
{
    public interface IHeaderParserBase<out THeader> : IDisposable
        where THeader : IHeaderBase
    {
        THeader Parse();
    }
}
