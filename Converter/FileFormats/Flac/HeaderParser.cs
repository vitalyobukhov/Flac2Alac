using System.Linq;
using System.Text;
using System.IO;
using Converter.Extensions;
using Converter.Factories;
using Converter.FileFormats.Flac.Metadata.DataBlocks;
using Converter.BitReader;
using Converter.FileFormats.Flac.Metadata;

namespace Converter.FileFormats.Flac
{
    public sealed class HeaderParser : Base.HeaderParserBase<Header>
    {
        private const int MinimalByteStreamLength = 42;



        public HeaderParser(Reader reader)
            : base(reader)
        {
            this.reader.ByteOrder = ByteOrder.BigEndian;
        }



        private Block<StreamInfo> ReadStreamInfoBlock()
        {
            var result = new Block<StreamInfo>();

            var header = new HeaderBlock();
            header.IsLastBlock = reader.ReadBool(1);
            header.BlockType = (BlockType)reader.ReadByte(7);

            if (header.BlockType != BlockType.StreamInfo)
            {
                throw new InvalidDataException("Provided stream does not contains stream info metadata as first block.");
            }

            header.MetadataLengthToFollow = reader.ReadUInt(24);

            var data = new StreamInfo();
            data.MinimumBlockSize = reader.ReadUShort(16);
            data.MaximumBlockSize = reader.ReadUShort(16);
            data.MinimumFrameSize = reader.ReadUInt(24);
            data.MaximumFrameSize = reader.ReadUInt(24);
            data.SampleRate = reader.ReadUInt(20);
            data.NumberOfChannels = (byte)(reader.ReadByte(3) + 1);
            data.BitsPerSample = (byte)(reader.ReadByte(5) + 1);
            data.TotalSamples = reader.ReadULong(36);
            Enumerable.Range(0, data.MD5.Length)
                .Select(i => data.MD5[i] = reader.ReadByte(Constants.BitByteSize))
                .ToList();

            result.Header = header;
            result.Data = data;

            return result;
        }

        public override Header Parse()
        {
            reader.Rewind();

            if (reader.ByteStreamLength < MinimalByteStreamLength)
            {
                throw new InvalidDataException("Provided stream's length is less than minimal possible value for FLAC format.");
            }

            var streamMarker = reader.ReadString(Header.FlacStreamMarkerConstant.Length, Encoding.ASCII);
            if (!streamMarker.Matches(Header.FlacStreamMarkerConstant))
            {
                throw new InvalidDataException("Provided stream does not contains FLAC format marker.");
            }

            var result = new Header();

            result.FlacStreamMarker = streamMarker;
            result.StreamInfoBlock = ReadStreamInfoBlock();

            result.IsParsed = true;

            return result;
        }
    }
}
