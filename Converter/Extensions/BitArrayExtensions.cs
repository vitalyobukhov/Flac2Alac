using System;
using System.Collections;

namespace Converter.Extensions
{
    static class BitArrayExtensions
    {
        public static BitArray Reverse(this BitArray bitArray, int blockSize)
        {
            if (bitArray.Length % blockSize != 0)
            {
                throw new ArgumentException("Bit array length should be devide by blockSize.");
            }

            for (var i = 0; i < bitArray.Length / (blockSize * 2); i++)
            {
                for (var j = 0; j < blockSize; j++)
                {
                    var g1 = i * blockSize + j;
                    var g2 = (bitArray.Length / blockSize - 1 - i) * blockSize + j;
                    var temp = bitArray[g1];
                    bitArray[g1] = bitArray[g2];
                    bitArray[g2] = temp;
                }
            }

            return bitArray;
        }

        //public static BitArray ShiftLogicallyRight(this BitArray bitArray, int count)
        //{
        //    if (bitArray == null)
        //    {
        //        throw new ArgumentNullException("bitArray");
        //    }

        //    if (count < 0)
        //    {
        //        throw new ArgumentOutOfRangeException("count");
        //    }

        //    for (var i = 0; i < bitArray.Length - count; i++)
        //    {
        //        bitArray[i] = bitArray[i + count];
        //    }

        //    var lastBit = bitArray[bitArray.Length - 1];
        //    for (var i = bitArray.Length - count; i < bitArray.Length - 1; i++)
        //    {
        //        bitArray[i] = lastBit;
        //    }

        //    return bitArray;
        //}

        //public static BitArray ShiftLogicallyLeft(this BitArray bitArray, int count)
        //{
        //    if (bitArray == null)
        //    {
        //        throw new ArgumentNullException("bitArray");
        //    }

        //    if (count < 0)
        //    {
        //        throw new ArgumentOutOfRangeException("count");
        //    }

        //    for (var i = bitArray.Length - 1; i >= count; i--)
        //    {
        //        bitArray[i] = bitArray[i - count];
        //    }

        //    var firstBit = bitArray[0];
        //    for (var i = count - 1; i > 0; i--)
        //    {
        //        bitArray[i] = firstBit;
        //    }

        //    return bitArray;
        //}

        public static BitArray ShiftArithmeticallyRight(this BitArray bitArray, int count)
        {
            if (bitArray == null)
            {
                throw new ArgumentNullException("bitArray");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            for (var i = 0; i < bitArray.Length - count; i++)
            {
                bitArray[i] = bitArray[i + count];
            }

            for (var i = bitArray.Length - count; i < bitArray.Length; i++)
            {
                bitArray[i] = false;
            }

            return bitArray;
        }

        public static BitArray ShiftArithmeticallyLeft(this BitArray bitArray, int count)
        {
            if (bitArray == null)
            {
                throw new ArgumentNullException("bitArray");
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException("count");
            }

            for (var i = bitArray.Length - 1; i >= count; i--)
            {
                bitArray[i] = bitArray[i - count];
            }

            for (var i = count - 1; i >= 0; i--)
            {
                bitArray[i] = false;
            }

            return bitArray;
        }
    }
}
