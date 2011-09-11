using System;

namespace Converter.FileFormats.Base
{
    public abstract class SampleCounterBase<TParser, THeader> : ISampleCounterBase<TParser, THeader>
        where TParser : class, IHeaderParserBase<THeader>
        where THeader : class, IHeaderBase
    {
        protected ulong? sampleCount;



        protected SampleCounterBase(TParser parser)
        {
            if (parser == null)
            {
                throw new ArgumentNullException("parser");
            }

            this.Parser = parser;
            sampleCount = null;
        }

        protected SampleCounterBase(THeader header)
        {
            if (header == null)
            {
                throw new ArgumentNullException("header");
            }

            if (!header.IsParsed)
            {
                throw new ArgumentException("Header should be in fully parsed state");
            }

            this.Header = header;
            sampleCount = null;
        }



        protected TParser Parser { get; private set; }

        protected THeader Header { get; private set; }



        public abstract void CalculateSampleCount();

        public ulong GetSampleCount()
        {
            if (!sampleCount.HasValue)
            {
                if (Parser != null)
                {
                    this.Header = Parser.Parse();
                }

                CalculateSampleCount();
            }

            return sampleCount.Value;
        }

        public void Dispose()
        {
            if (Parser != null)
            {
                Parser.Dispose();
            }
        }
    }
}
