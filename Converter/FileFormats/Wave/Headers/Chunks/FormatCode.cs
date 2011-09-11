namespace Converter.FileFormats.Wave.Headers.Chunks
{
    public enum FormatCode
    {
        Unknown = 0x0,
        PCM = 0x1,
        IEEEFloat = 0x3,
        ALaw = 0x6,
        MuLaw = 0x7,
        Extensible = 0xFFFE
    }
}
