namespace Converter.FileFormats.Flac.Metadata
{
    public class HeaderBlock
    {
        public bool IsLastBlock { get; set; }
        public BlockType BlockType { get; set; }
        public uint MetadataLengthToFollow { get; set; }



        public HeaderBlock()
        {
            BlockType = BlockType.Invalid;
        }
    }
}
