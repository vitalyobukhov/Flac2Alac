using Converter.FileFormats.Flac.Metadata.DataBlocks;

namespace Converter.FileFormats.Flac.Metadata
{
    public class Block<TDataBlock>
        where TDataBlock : DataBlockBase
    {
        public HeaderBlock Header { get; set; }
        public TDataBlock Data { get; set; }
    }
}
