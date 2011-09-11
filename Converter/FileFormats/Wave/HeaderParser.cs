using System;
using System.Text;
using Converter.Extensions;
using Converter.Factories;
using Converter.BitReader;
using System.IO;
using Converter.FileFormats.Base;
using Converter.FileFormats.Wave.Headers.Chunks;
using Converter.FileFormats.Wave.Headers.Chunks.Formats;
using HeaderBase = Converter.FileFormats.Wave.Headers.HeaderBase;

namespace Converter.FileFormats.Wave
{
    public sealed class HeaderParser : HeaderParserBase<HeaderBase>
    {
        private const int MinimalByteStreamLength = 44;



        public HeaderParser(Reader reader)
            : base(reader)
        {
            this.reader.ByteOrder = ByteOrder.LittleEndian;
        }



        private RIFF ReadRiff()
        {
            var result = new RIFF();

            result.ChunkId = reader.ReadString(RIFF.ChunkIdConstant.Length, Encoding.ASCII);
            if (!result.ChunkId.Matches(RIFF.ChunkIdConstant))
            {
                throw new InvalidDataException("Provided stream does not contains RIFF chunk id.");
            }

            result.ChunkSize = reader.ReadUInt(Constants.BitIntSize);

            result.WaveId = reader.ReadString(RIFF.WaveIdConstant.Length, Encoding.ASCII);
            if (!result.WaveId.Matches(RIFF.WaveIdConstant))
            {
                throw new InvalidDataException("Provided stream does not contains WAVE chunk id.");
            }

            return result;
        }

        private FormatBase CreateFormat(FormatCode formatCode)
        {
            FormatBase result = null;

            switch (formatCode)
            {
                case FormatCode.PCM:
                    result = new PCM();
                    break;

                case FormatCode.IEEEFloat:
                case FormatCode.ALaw:
                case FormatCode.MuLaw:
                    result = new NonPCM();
                    break;

                case FormatCode.Extensible:
                    result = new Extensible();
                    break;

                default:
                    throw new InvalidDataException("Provided stream does not contains supported WAVE format.");
            }

            return result;
        }

        private FormatBase ReadFormat()
        {
            var fmtChunkId = reader.ReadString(FormatBase.ChunkIdConstant.Length, Encoding.ASCII);
            if (!fmtChunkId.Matches(FormatBase.ChunkIdConstant))
            {
                throw new InvalidDataException("Provided stream does not contains fmt chunk id.");
            }

            var fmtChuckSize = reader.ReadUInt(Constants.BitIntSize);

            var formatCode = FormatCode.Unknown;
            try
            {
                formatCode = (FormatCode)reader.ReadUShort(Constants.BitShortSize);
            }
            catch (Exception ex)
            {
                throw new InvalidDataException("Provided stream does not contains supported WAVE format.", ex);
            }

            FormatBase result = CreateFormat(formatCode);

            result.ChunkId = fmtChunkId;
            result.ChunkSize = fmtChuckSize;
            result.FormatCode = formatCode;
            result.NumberOfChannels = reader.ReadUShort(Constants.BitShortSize);
            result.SampleRate = reader.ReadUInt(Constants.BitIntSize);
            result.DataRate = reader.ReadUInt(Constants.BitIntSize);
            result.DataBlockSize = reader.ReadUShort(Constants.BitShortSize);
            result.BitsPerSample = reader.ReadUShort(Constants.BitShortSize);

            ushort sizeOfExtension = 0;

            switch (formatCode)
            {
                case FormatCode.IEEEFloat:
                case FormatCode.ALaw:
                case FormatCode.MuLaw:
                case FormatCode.Extensible:
                    sizeOfExtension = reader.ReadUShort(Constants.BitShortSize);
                    break;
            }

            switch (formatCode)
            {
                case FormatCode.IEEEFloat:
                case FormatCode.ALaw:
                case FormatCode.MuLaw:
                    var nonPcmFormat = (NonPCM)result;
                    nonPcmFormat.SizeOfExtension = sizeOfExtension;
                    break;

                case FormatCode.Extensible:
                    var extensibleFormat = (Extensible)result;
                    extensibleFormat.SizeOfExtension = sizeOfExtension;
                    extensibleFormat.ValidBitsPerSecond = reader.ReadUShort(Constants.BitShortSize);
                    extensibleFormat.SpeakerPositionMask = reader.ReadUInt(Constants.BitIntSize);
                    extensibleFormat.SubFormat = reader.ReadGuid(Constants.BitGuidSize);
                    break;
            }

            return result;
        }

        private HeaderBase CreateHeader(FormatCode formatCode)
        {
            HeaderBase result = null;

            switch (formatCode)
            {
                case FormatCode.PCM:
                    result = new Headers.PCM();
                    break;

                case FormatCode.IEEEFloat:
                case FormatCode.ALaw:
                case FormatCode.MuLaw:
                    result = new Headers.NonPCM();
                    break;

                case FormatCode.Extensible:
                    result = new Headers.Extensible();
                    break;

                default:
                    throw new InvalidDataException("Provided stream does not contains supported WAVE format.");
            }

            return result;
        }

        private Fact ReadFact()
        {
            Fact result = null;

            var factChunkId = reader.ReadString(Fact.ChunkIdConstant.Length, Encoding.ASCII);
            if (!factChunkId.Matches(Fact.ChunkIdConstant))
            {
                throw new InvalidDataException("Provided stream does not contains fact chunk id.");
            }

            result = new Fact();

            result.ChunkId = factChunkId;
            result.ChunkSize = reader.ReadUInt(Constants.BitIntSize);
            result.SampleLength = reader.ReadUInt(Constants.BitIntSize);

            return result;
        }

        private Data ReadData(ushort dataBlockSize)
        {
            var result = new Data();

            var dataChunkId = reader.ReadString(Data.ChunkIdConstant.Length, Encoding.ASCII);
            if (!dataChunkId.Matches(Data.ChunkIdConstant))
            {
                throw new InvalidDataException("Provided stream does not contains data chunk id.");
            }

            result.ChunkId = dataChunkId;
            result.ChunkSize = reader.ReadUInt(Constants.BitIntSize);

            reader.Seek(SeekOrigin.Current, result.ChunkSize * Constants.BitByteSize);

            if ((result.ChunkSize / dataBlockSize) % 2 == 1)
            {
                reader.Seek(SeekOrigin.Current, Constants.BitByteSize);
            }

            return result;
        }

        private void ReadDataAndFact(ushort dataBlockSize, out Data data, out Fact fact)
        {
            data = null;
            fact = null;

            var chunkId = reader.ReadString(Data.ChunkIdConstant.Length, Encoding.ASCII);
            if (chunkId.Matches(Data.ChunkIdConstant))
            {
                reader.Seek(SeekOrigin.Current, -Data.ChunkIdConstant.Length * Constants.BitByteSize);
                data = ReadData(dataBlockSize);

                if (reader.ByteStreamPosition + Constants.BitByteSize * Fact.ChunkIdConstant.Length <= reader.ByteStreamLength)
                {
                    chunkId = reader.ReadString(Data.ChunkIdConstant.Length, Encoding.ASCII);
                    if (chunkId.Matches(Fact.ChunkIdConstant))
                    {
                        reader.Seek(SeekOrigin.Current, -Fact.ChunkIdConstant.Length * Constants.BitByteSize);
                        fact = ReadFact();
                    }
                }
            }
            else if (chunkId.Matches(Fact.ChunkIdConstant))
            {
                reader.Seek(SeekOrigin.Current, -Data.ChunkIdConstant.Length * Constants.BitByteSize);
                fact = ReadFact();
                data = ReadData(dataBlockSize);
            }
            else
            {
                throw new InvalidDataException("Provided stream does not contains data chunk id.");
            }
        }

        private void AppendFact(HeaderBase header, Fact fact)
        {
            switch (header.Format.FormatCode)
            {
                case FormatCode.IEEEFloat:
                case FormatCode.ALaw:
                case FormatCode.MuLaw:
                    var nonPcmHeader = (Headers.NonPCM)header;
                    nonPcmHeader.Fact = fact;
                    break;

                case FormatCode.Extensible:
                    var extensibleHeader = (Headers.Extensible)header;
                    extensibleHeader.Fact = fact;
                    break;
            }
        }

        public override HeaderBase Parse()
        {
            reader.Rewind();

            if (reader.ByteStreamLength < MinimalByteStreamLength)
            {
                throw new IOException("Provided stream's length is less than minimal possible value for WAVE format.");
            }

            var riff = ReadRiff();
            var format = ReadFormat();
            Data data = null;
            Fact fact = null;
            ReadDataAndFact(format.DataBlockSize, out data, out fact);

            HeaderBase result = CreateHeader(format.FormatCode);

            result.RIFF = riff;
            result.Format = format;
            result.Data = data;
            AppendFact(result, fact);

            result.IsParsed = true;

            return result;
        }
    }
}
