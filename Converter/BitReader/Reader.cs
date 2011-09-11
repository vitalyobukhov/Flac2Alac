using System;
using System.IO;
using System.Collections;
using Converter.Extensions;
using System.Text;
using Converter.Factories;
using System.Linq;

namespace Converter.BitReader
{
    public class Reader : IDisposable
    {
        private readonly Stream stream;

        private long offset;



        public bool AutoAdvance { get; set; }

        public ByteOrder ByteOrder { get; set; }



        public Reader(Stream stream)
        {
            ValidateInit(stream);

            this.stream = stream;
            offset = 0;
            AutoAdvance = true;
            ByteOrder = BitConverter.IsLittleEndian ? ByteOrder.LittleEndian : ByteOrder.BigEndian;
        }



        public long ByteStreamLength
        {
            get
            {
                return stream.Length;
            }
        }

        public long BitStreamLength
        {
            get
            {
                return stream.Length * Constants.BitByteSize;
            }
        }

        public long ByteStreamPosition
        {
            get
            {
                return offset / Constants.BitByteSize;
            }
        }

        public long BitStreamPosition
        {
            get
            {
                return offset;
            }
        }


        public void Dispose()
        {
            try
            {
                stream.Dispose();
            }
            catch
            { }
        }

        private void ValidateInit(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (!stream.CanRead)
            {
                throw new ArgumentException("You should provide readable stream object.");
            }

            if (!stream.CanSeek)
            {
                throw new ArgumentException("You should  provide seekable stream object.");
            }
        }

        private void ValidateRead(SeekOrigin seekOrigin, long offset, int bitValueSize, 
            int byteValueSize, PrimitiveType primitiveType)
        {
            if (primitiveType == PrimitiveType.Unknown)
            {
                throw new ArgumentException("Provided value type is not primitive.");
            }

            if (bitValueSize <= 0)
            {
                throw new ArgumentOutOfRangeException("bitValueSize");
            }

            if (byteValueSize * Constants.BitByteSize < bitValueSize)
            {
                throw new ArgumentException("Provided valueSize is greater than size of value type.");
            }

            var isErrorOccured = false;

            switch (seekOrigin)
            {
                case SeekOrigin.Begin:
                    isErrorOccured = offset < 0 || offset + bitValueSize > stream.Length * Constants.BitByteSize;
                    break;

                case SeekOrigin.Current:
                    isErrorOccured = this.offset + offset < 0 ||
                        this.offset + bitValueSize + offset > stream.Length * Constants.BitByteSize;
                    break;

                case SeekOrigin.End:
                    isErrorOccured = offset < 0 || -offset + bitValueSize > 0;
                    break;
            }

            if (isErrorOccured)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
        }

        private void SetOffset(SeekOrigin seekOrigin, long offset)
        {
            switch (seekOrigin)
            {
                case SeekOrigin.Begin:
                    this.offset = offset;
                    break;

                case SeekOrigin.Current:
                    this.offset += offset;
                    break;

                case SeekOrigin.End:
                    this.offset = stream.Length * Constants.BitByteSize - offset;
                    break;
            }
        }

        private ValueType ConvertBytes(PrimitiveType type, byte[] bytes)
        {
            ValueType result = null;

            switch (type)
            {
                case PrimitiveType.SByte:
                    result = Convert.ToSByte(bytes[0]);
                    break;

                case PrimitiveType.Byte:
                    result = Convert.ToByte(bytes[0]);
                    break;

                case PrimitiveType.Short:
                    result = BitConverter.ToInt16(bytes, 0);
                    break;

                case PrimitiveType.UShort:
                    result = BitConverter.ToUInt16(bytes, 0);
                    break;

                case PrimitiveType.Int:
                    result = BitConverter.ToInt32(bytes, 0);
                    break;

                case PrimitiveType.UInt:
                    result = BitConverter.ToUInt32(bytes, 0);
                    break;

                case PrimitiveType.Long:
                    result = BitConverter.ToInt64(bytes, 0);
                    break;

                case PrimitiveType.ULong:
                    result = BitConverter.ToUInt64(bytes, 0);
                    break;

                case PrimitiveType.Float:
                    result = BitConverter.ToSingle(bytes, 0);
                    break;

                case PrimitiveType.Double:
                    result = BitConverter.ToDouble(bytes, 0);
                    break;

                case PrimitiveType.Decimal:
                    result = DecimalBitConverter.ToDecimal(bytes);
                    break;

                case PrimitiveType.Char:
                    result = BitConverter.ToChar(bytes, 0);
                    break;

                case PrimitiveType.Bool:
                    result = BitConverter.ToBoolean(bytes, 0);
                    break;

                case PrimitiveType.Guid:
                    result = new Guid(bytes);
                    break;
            }

            return result;
        }

        private ValueType ReadValue<TValue>(SeekOrigin seekOrigin, long offset, int valueSize)
            where TValue : struct
        {
            var valueType = typeof(TValue);
            var primitiveType = valueType.GetPrimitiveType();
            var byteValueSize = primitiveType.GetSize();

            ValidateRead(seekOrigin, offset, valueSize, byteValueSize, primitiveType);

            SetOffset(seekOrigin, offset);

            var byteBufferSize = ((this.offset + valueSize) % Constants.BitByteSize != 0) && 
                (this.offset % Constants.BitByteSize + valueSize > Constants.BitByteSize * byteValueSize) ?
                byteValueSize + 1 : byteValueSize;
            var valueBytes = new byte[byteBufferSize];
            var byteOffset = this.offset / Constants.BitByteSize;

            stream.Seek(byteOffset, SeekOrigin.Begin);
            stream.Read(valueBytes, 0, byteBufferSize);

            var valueBits = new BitArray(valueBytes);

            if (ByteOrder == ByteOrder.BigEndian)
            {
                valueBits.Reverse(Constants.BitByteSize);
            }

            valueBits.ShiftArithmeticallyLeft((int)(this.offset - (this.offset / Constants.BitByteSize) * Constants.BitByteSize));
            valueBits.ShiftArithmeticallyRight(byteBufferSize * Constants.BitByteSize - valueSize);

            valueBits.CopyTo(valueBytes, 0);

            var result = ConvertBytes(primitiveType, valueBytes);

            if (AutoAdvance)
            {
                this.offset += valueSize;
            }

            return result;
        }

        public byte ReadByte(SeekOrigin seekOrigin, long offset, int valueSize)
        {
            return (byte)ReadValue<byte>(seekOrigin, offset, valueSize);
        }

        public byte ReadByte(int valueSize)
        {
            return (byte)ReadValue<byte>(SeekOrigin.Current, 0, valueSize);
        }

        public sbyte ReadSByte(SeekOrigin seekOrigin, long offset, int valueSize)
        {
            return (sbyte)ReadValue<sbyte>(seekOrigin, offset, valueSize);
        }

        public sbyte ReadSByte(int valueSize)
        {
            return (sbyte)ReadValue<sbyte>(SeekOrigin.Current, 0, valueSize);
        }

        public short ReadShort(SeekOrigin seekOrigin, long offset, int valueSize)
        {
            return (short)ReadValue<short>(seekOrigin, offset, valueSize);
        }

        public short ReadShort(int valueSize)
        {
            return (short)ReadValue<short>(SeekOrigin.Current, 0, valueSize);
        }

        public ushort ReadUShort(SeekOrigin seekOrigin, long offset, int valueSize)
        {
            return (ushort)ReadValue<ushort>(seekOrigin, offset, valueSize);
        }

        public ushort ReadUShort(int valueSize)
        {
            return (ushort)ReadValue<ushort>(SeekOrigin.Current, 0, valueSize);
        }

        public int ReadInt(SeekOrigin seekOrigin, long offset, int valueSize)
        {
            return (int)ReadValue<int>(seekOrigin, offset, valueSize);
        }

        public int ReadInt(int valueSize)
        {
            return (int)ReadValue<int>(SeekOrigin.Current, 0, valueSize);
        }

        public uint ReadUInt(SeekOrigin seekOrigin, long offset, int valueSize)
        {
            return (uint)ReadValue<uint>(seekOrigin, offset, valueSize);
        }

        public uint ReadUInt(int valueSize)
        {
            return (uint)ReadValue<uint>(SeekOrigin.Current, 0, valueSize);
        }

        public long ReadLong(SeekOrigin seekOrigin, long offset, int valueSize)
        {
            return (long)ReadValue<long>(seekOrigin, offset, valueSize);
        }

        public long ReadLong(int valueSize)
        {
            return (long)ReadValue<long>(SeekOrigin.Current, 0, valueSize);
        }

        public ulong ReadULong(SeekOrigin seekOrigin, long offset, int valueSize)
        {
            return (ulong)ReadValue<ulong>(seekOrigin, offset, valueSize);
        }

        public ulong ReadULong(int valueSize)
        {
            return (ulong)ReadValue<ulong>(SeekOrigin.Current, 0, valueSize);
        }

        public float ReadFloat(SeekOrigin seekOrigin, long offset, int valueSize)
        {
            return (float)ReadValue<float>(seekOrigin, offset, valueSize);
        }

        public float ReadFloat(int valueSize)
        {
            return (float)ReadValue<float>(SeekOrigin.Current, 0, valueSize);
        }

        public double ReadDouble(SeekOrigin seekOrigin, long offset, int valueSize)
        {
            return (double)ReadValue<double>(seekOrigin, offset, valueSize);
        }

        public double ReadDouble(int valueSize)
        {
            return (double)ReadValue<double>(SeekOrigin.Current, 0, valueSize);
        }

        public decimal ReadDecimal(SeekOrigin seekOrigin, long offset, int valueSize)
        {
            return (decimal)ReadValue<decimal>(seekOrigin, offset, valueSize);
        }

        public decimal ReadDecimal(int valueSize)
        {
            return (decimal)ReadValue<decimal>(SeekOrigin.Current, 0, valueSize);
        }

        public char ReadChar(SeekOrigin seekOrigin, long offset, int valueSize)
        {
            return (char)ReadValue<char>(seekOrigin, offset, valueSize);
        }

        public char ReadChar(int valueSize)
        {
            return (char)ReadValue<char>(SeekOrigin.Current, 0, valueSize);
        }

        public bool ReadBool(SeekOrigin seekOrigin, long offset, int valueSize)
        {
            return (bool)ReadValue<bool>(seekOrigin, offset, valueSize);
        }

        public bool ReadBool(int valueSize)
        {
            return (bool)ReadValue<bool>(SeekOrigin.Current, 0, valueSize);
        }

        public Guid ReadGuid(SeekOrigin seekOrigin, long offset, int valueSize)
        {
            return (Guid)ReadValue<Guid>(seekOrigin, offset, valueSize);
        }

        public Guid ReadGuid(int valueSize)
        {
            return (Guid)ReadValue<Guid>(SeekOrigin.Current, 0, valueSize);
        }

        public string ReadString(SeekOrigin seekOrigin, long offset, int length, Encoding encoding)
        {
            if (encoding == null)
            {
                throw new ArgumentNullException("encoding");
            }

            Seek(seekOrigin, offset);

            var binaryString = new byte[length];
            for (var i = 0; i < length; i++)
            {
                var newOffset = AutoAdvance ? offset : offset + i * Constants.BitByteSize;
                binaryString[i] = ReadByte(seekOrigin, newOffset, Constants.BitByteSize);
            }

            var result = encoding.GetString(binaryString);
            return result;
        }

        public string ReadString(int length, Encoding encoding)
        {
            return ReadString(SeekOrigin.Current, 0, length, encoding);
        }

        private void ValidateSeek(SeekOrigin seekOrigin, long offset)
        {
            var isErrorOccured = false;

            switch (seekOrigin)
            {
                case SeekOrigin.Begin:
                case SeekOrigin.End:
                    isErrorOccured = offset > stream.Length * Constants.BitByteSize || offset < 0;
                    break;

                case SeekOrigin.Current:
                    isErrorOccured = offset + this.offset > stream.Length * Constants.BitByteSize ||
                        offset + this.offset < 0;
                    break;
            }

            if (isErrorOccured)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
        }

        public void Seek(SeekOrigin seekOrigin, long offset)
        {
            ValidateSeek(seekOrigin, offset);

            SetOffset(seekOrigin, offset);
        }

        public void Rewind()
        {
            Seek(SeekOrigin.Begin, 0);
        }
    }
}
