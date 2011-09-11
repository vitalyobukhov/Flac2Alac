using System;
using Converter.BitReader;
using Converter.Factories;

namespace Converter.Extensions
{
    static class PrimitiveTypeExtensions
    {
        public static PrimitiveType GetPrimitiveType(this Type type)
        {
            var result = PrimitiveType.Unknown;

            if (type == typeof(sbyte))
            {
                result = PrimitiveType.SByte;
            }
            else if (type == typeof(byte))
            {
                result = PrimitiveType.Byte;
            }
            else if (type == typeof(short))
            {
                result = PrimitiveType.Short;
            }
            else if (type == typeof(ushort))
            {
                result = PrimitiveType.UShort;
            }
            else if (type == typeof(int))
            {
                result = PrimitiveType.Int;
            }
            else if (type == typeof(uint))
            {
                result = PrimitiveType.UInt;
            }
            else if (type == typeof(long))
            {
                result = PrimitiveType.Long;
            }
            else if (type == typeof(ulong))
            {
                result = PrimitiveType.ULong;
            }
            else if (type == typeof(float))
            {
                result = PrimitiveType.Float;
            }
            else if (type == typeof(double))
            {
                result = PrimitiveType.Double;
            }
            else if (type == typeof(decimal))
            {
                result = PrimitiveType.Decimal;
            }
            else if (type == typeof(char))
            {
                result = PrimitiveType.Char;
            }
            else if (type == typeof(bool))
            {
                result = PrimitiveType.Bool;
            }
            else if (type == typeof(Guid))
            {
                result = PrimitiveType.Guid;
            }

            return result;
        }

        //public static Type GetType(this PrimitiveType primitiveType)
        //{
        //    Type result = null;

        //    switch (primitiveType)
        //    {
        //        case PrimitiveType.SByte:
        //            result = typeof(sbyte);
        //            break;

        //        case PrimitiveType.Byte:
        //            result = typeof(byte);
        //            break;

        //        case PrimitiveType.Short:
        //            result = typeof(short);
        //            break;

        //        case PrimitiveType.UShort:
        //            result = typeof(ushort);
        //            break;

        //        case PrimitiveType.Int:
        //            result = typeof(int);
        //            break;

        //        case PrimitiveType.UInt:
        //            result = typeof(uint);
        //            break;

        //        case PrimitiveType.Long:
        //            result = typeof(long);
        //            break;

        //        case PrimitiveType.ULong:
        //            result = typeof(ulong);
        //            break;

        //        case PrimitiveType.Float:
        //            result = typeof(float);
        //            break;

        //        case PrimitiveType.Double:
        //            result = typeof(double);
        //            break;

        //        case PrimitiveType.Decimal:
        //            result = typeof(decimal);
        //            break;

        //        case PrimitiveType.Char:
        //            result = typeof(char);
        //            break;

        //        case PrimitiveType.Bool:
        //            result = typeof(bool);
        //            break;

        //        case PrimitiveType.Guid:
        //            result = typeof(Guid);
        //            break;
        //    }

        //    return result;
        //}

        public static int GetSize(this PrimitiveType primitiveType)
        {
            int result = 0;

            switch (primitiveType)
            {
                case PrimitiveType.SByte:
                case PrimitiveType.Byte:
                    result = Constants.ByteByteSize;
                    break;

                case PrimitiveType.Short:
                case PrimitiveType.UShort:
                    result = Constants.ByteShortSize;
                    break;

                case PrimitiveType.Int:
                case PrimitiveType.UInt:
                    result = Constants.ByteIntSize;
                    break;

                case PrimitiveType.ULong:
                case PrimitiveType.Long:
                    result = Constants.ByteLongSize;
                    break;

                case PrimitiveType.Float:
                    result = Constants.ByteFloatSize;
                    break;

                case PrimitiveType.Double:
                    result = Constants.ByteDoubleSize;
                    break;

                case PrimitiveType.Decimal:
                    result = Constants.ByteDecimalSize;
                    break;

                case PrimitiveType.Bool:
                    result = Constants.ByteBoolSize;
                    break;

                case PrimitiveType.Char:
                    result = Constants.ByteCharSize;
                    break;

                case PrimitiveType.Guid:
                    result = Constants.ByteGuidSize;
                    break;
            }

            return result;
        }
    }
}
