using Converter.FileFormats.Flac.Metadata.DataBlocks;
using Converter.FileFormats.Base;

namespace Converter.FileFormats.Flac.Metadata
{
    public sealed class Header : HeaderBase
    {
        public const string FlacStreamMarkerConstant = "fLaC";



        public string FlacStreamMarker { get; set; }
        public Block<StreamInfo> StreamInfoBlock { get; set; }
    }
}
