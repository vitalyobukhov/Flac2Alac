using Converter.Factories;

namespace Converter.BitReader
{
    static class DecimalBitConverter
    {
        public static decimal ToDecimal(byte[] bytes)
        {
            var bits = new int[4];
            bits[0] = ((bytes[0] | (bytes[1] << Constants.BitByteSize)) | (bytes[2] << Constants.BitByteSize * 2)) | (bytes[3] << Constants.BitByteSize * 3);
            bits[1] = ((bytes[4] | (bytes[5] << Constants.BitByteSize)) | (bytes[6] << Constants.BitByteSize * 2)) | (bytes[7] << Constants.BitByteSize * 3);
            bits[2] = ((bytes[8] | (bytes[9] << Constants.BitByteSize)) | (bytes[10] << Constants.BitByteSize * 2)) | (bytes[11] << Constants.BitByteSize * 3);
            bits[3] = ((bytes[12] | (bytes[13] << Constants.BitByteSize)) | (bytes[14] << Constants.BitByteSize * 2)) | (bytes[15] << Constants.BitByteSize * 3);

            return new decimal(bits);
        }

        public static byte[] GetBytes(decimal value)
        {
            var bytes = new byte[Constants.ByteDecimalSize];

            int[] bits = decimal.GetBits(value);
            var lo = bits[0];
            var mid = bits[1];
            var hi = bits[2];
            var flags = bits[3];

            bytes[0] = (byte)lo;
            bytes[1] = (byte)(lo >> Constants.BitByteSize);
            bytes[2] = (byte)(lo >> Constants.BitByteSize * 2);
            bytes[3] = (byte)(lo >> Constants.BitByteSize * 3);
            bytes[4] = (byte)mid;
            bytes[5] = (byte)(mid >> Constants.BitByteSize);
            bytes[6] = (byte)(mid >> Constants.BitByteSize * 2);
            bytes[7] = (byte)(mid >> Constants.BitByteSize * 3);
            bytes[8] = (byte)hi;
            bytes[9] = (byte)(hi >> Constants.BitByteSize);
            bytes[10] = (byte)(hi >> Constants.BitByteSize * 2);
            bytes[11] = (byte)(hi >> Constants.BitByteSize * 3);
            bytes[12] = (byte)flags;
            bytes[13] = (byte)(flags >> Constants.BitByteSize);
            bytes[14] = (byte)(flags >> Constants.BitByteSize * 2);
            bytes[15] = (byte)(flags >> Constants.BitByteSize * 3);

            return bytes;
        }
    }
}
