namespace Converter.Factories
{
    static class Constants
    {
        public const byte ByteByteSize = 1;
        public const byte ByteShortSize = 2 * ByteByteSize;
        public const byte ByteIntSize = 4 * ByteByteSize;
        public const byte ByteLongSize = 8 * ByteByteSize;
        public const byte ByteFloatSize = 4 * ByteByteSize;
        public const byte ByteDoubleSize = 8 * ByteByteSize;
        public const byte ByteDecimalSize = 16 * ByteByteSize;
        public const byte ByteCharSize = 2 * ByteByteSize;
        public const byte ByteBoolSize = 1 * ByteByteSize;
        public const byte ByteGuidSize = 16 * ByteByteSize;

        public const byte BitByteSize = 8;
        public const byte BitShortSize = ByteShortSize * BitByteSize;
        public const byte BitIntSize = ByteIntSize * BitByteSize;
        public const byte BitLongSize = ByteLongSize * BitByteSize;
        public const byte BitFloatSize = ByteFloatSize * BitByteSize;
        public const byte BitDoubleSize = ByteDoubleSize * BitByteSize;
        public const byte BitDecimalSize = ByteDecimalSize * BitByteSize;
        public const byte BitCharSize = ByteCharSize * BitByteSize;
        public const byte BitBoolSize = ByteBoolSize * BitByteSize;
        public const byte BitGuidSize = ByteGuidSize * BitByteSize;
    }
}
