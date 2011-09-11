namespace Converter.FileFormats.Flac.Metadata
{
    public enum BlockType
    {
        StreamInfo = 0,
        Padding = 1,
        Application = 2,
        SeekTable = 3,
        VorbisComment = 4,
        CueSheet = 5,
        Picture = 6,
        Invalid = 127
    }
}
