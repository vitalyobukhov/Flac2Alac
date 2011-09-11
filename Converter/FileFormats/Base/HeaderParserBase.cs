using System;
using Converter.BitReader;

namespace Converter.FileFormats.Base
{
    public abstract class HeaderParserBase<THeader> : IHeaderParserBase<THeader>
        where THeader : class, IHeaderBase

    {
        protected Reader reader { get; private set; }



        protected HeaderParserBase(Reader reader)
        {
            if (reader == null)
            {
                throw  new ArgumentNullException("reader");
            }

            this.reader = reader;
        }



        public abstract THeader Parse();

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
