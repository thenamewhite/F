using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
// author  (hf) time：2023/4/20 17:03:08
namespace F
{
    /// <summary>
    /// 字节管理
    /// </summary>
    public struct ByteStream
    {
        public int Position;
        public int Length;

        //private Span<byte> mBuffer;
        public byte[] mBuffer;

        public bool IsEnd
        {
            get => Length == Position;
        }

        public ByteStream(int length)
        {
            Position = 0;
            Length = length;
            mBuffer = new byte[Length];
        }

        public ByteStream(byte[] buffer)
        {
            Position = 0;
            Length = buffer.Length;
            //mBuffer = new Span<byte>(buffer);
            mBuffer = buffer;
        }

        public ByteStream(Span<byte> buffer)
        {
            Position = 0;
            Length = buffer.Length;
            //mBuffer = new Span<byte>(buffer.ToArray());
            mBuffer = buffer.ToArray();
        }

        public Span<byte> Span
        {
            get => mBuffer;
        }

        public byte[] Bytes
        {
            get => mBuffer.ToArray();
        }


        /// <summary>
        /// 基础类型
        /// </summary>
        public void Push<T>(T v) where T : unmanaged
        {
            //TODO没走压缩数据大小的，获取了类型字节长度直接write,后续在看是否压缩字节
            var size = Unsafe.SizeOf<T>();
            TrySetBuffLength(size);
            Span<byte> span = stackalloc byte[size];
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), v);
            Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref span[0], (uint)size);
            Position += size;
        }
        /// <summary>
        /// int 压缩字节
        /// </summary>
        /// <param name="v"></param>
        public void PushInt(int v)
        {
            WriteRawByte32(v);
        }

        public void PushInt(int[] value)
        {
            var size = Unsafe.SizeOf<int>();
            var byteLength = size * value.Length;
            //TrySetBuffLength(byteLength);
            PushLength(value.Length);
            foreach (var v in value)
            {
                WriteRawByte32(v);
            }
        }

        public void PushInt(int[][] value)
        {
            var size = Unsafe.SizeOf<int>();
            var byteLength = size * value.Length;
            //TrySetBuffLength(byteLength);
            PushLength(value.Length);
            foreach (var v in value)
            {
                PushInt(v);
            }
        }
        /// <summary>
        /// uint 压缩字节
        /// </summary>
        /// <param name="v"></param>
        public void PushUInt(uint v)
        {
            WriteRawByte32(v);
        }

        public void PushUInt(uint[] value)
        {
            PushLength(value.Length);
            foreach (var v in value)
            {
                WriteRawByte32(v);
            }
        }
        public void PushUInt(uint[][] value)
        {
            PushLength(value.Length);
            foreach (var v in value)
            {
                PushUInt(v);
            }
        }

        private void WriteRawByte32(long value)
        {
            //while (true)
            //{
            //    if ((value & ~0x7F) == 0)
            //    {//代表只有低7位有值，因此只需1个字节即可完成编码
            //        WriteRawByte(value);
            //        break;
            //    }
            //    else
            //    {
            //        //var c = (value & 0x7F) | 0x80;
            //        WriteRawByte((value & 0x7F) | 0x80);
            //        //writeRawByte((value & 0x7F) | 0x80);//代表编码不止一个字节，value & 0x7f 只取低 7 位，与 0x80 进行按位或（|）运算为了将最高位置位 1 ，代表后续字节也是改数字的一部分
            //        value >>= 7;
            //    }
            //}
            PushLength(value);
        }
        private void WriteRawByte(uint value)
        {
            TrySetBuffLength(1);
            Span<byte> span = stackalloc byte[1];
            span[0] = (byte)value;
            Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref span[0], (uint)1);
            Position += 1;
        }


        public void Push(string v, Encoding encoding = null)
        {
            var bytes = (encoding ?? Encoding.UTF8).GetBytes(v == null ? string.Empty : v);
            int length = bytes.Length;
            PushLength(length);
            TrySetBuffLength(length);
            if (length > 0)
            {
                Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref bytes[0], (uint)length);
                Position += length;
            }
        }

        public void Push<T>(T[] v) where T : unmanaged
        {
            var length = (v?.Length).GetValueOrDefault();
            PushLength(length);
            if (v is byte[] bytes)
            {
                if (length > 0)
                {
                    TrySetBuffLength(length);
                    Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref bytes[0], (uint)length);
                    Position += length;
                }
            }
            else
            {
                if (length > 0)
                {
                    var size = Unsafe.SizeOf<T>();
                    var byteLength = size * length;
                    TrySetBuffLength(byteLength);
                    for (int i = 0; i < v.Length; i++)
                    {
                        Span<byte> span = stackalloc byte[size];
                        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), v[i]);
                        Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref span[0], (uint)span.Length);
                        Position += size;
                    }
                }
            }
        }
        public void Push(string[] v)
        {
            var length = (v?.Length).GetValueOrDefault();
            PushLength(length);
            for (int i = 0; i < v.Length; i++)
            {
                Push(v[i]);
            }
        }
        public void Push<T>(T[][] v) where T : unmanaged
        {
            var length = (v?.Length).GetValueOrDefault();
            PushLength(length);
            for (int i = 0; i < length; i++)
            {
                Push(v[i]);
            }
        }
        public void Push(string[][] v)
        {
            var length = (v?.Length).GetValueOrDefault();
            PushLength(length);
            for (int i = 0; i < length; i++)
            {
                Push(v[i]);
            }
        }

        public void PushLength(long value)
        {
            Span<byte> buffer = stackalloc byte[8];
            // WriteRawVarint64
            int count = 0;
            while (value > 0x7f)
            {
                buffer[count] = (byte)((value & 0x7F | 0x80));
                value >>= 7;
                count++;
            }
            buffer[count] = (byte)value;
            count++;
            TrySetBuffLength(count);
            //byte* buffer = stackalloc byte[8];
            //fixed (byte* bptr = mBuffer)
            //{
            //Unsafe.CopyBlockUnaligned(bptr + Position, buffer, (uint)count);
            //}
            Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref buffer[0], (uint)count);
            Position += count;
            //Push(value);
        }

        public int ReadLength()
        {
            //return Read<int>();
            // ParseRawVarint64
            ulong result = mBuffer[Position++];
            if (result < 0x80)
            {
                return (int)result;
            }
            else
            {
                result &= 0x7f;
                int shift = 7;
                do
                {
                    byte b = mBuffer[Position++];
                    result |= (ulong)(b & 0x7F) << shift;
                    if (b < 0x80)
                    {
                        return (int)result;
                    }
                    shift += 7;
                }
                while (shift < 64);
            }
            return (int)result;
        }
        /// <summary>
        /// 从存入的字节数组中读取指定类型值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Read<T>() where T : unmanaged
        {
            ref var result = ref Unsafe.As<byte, T>(ref mBuffer[Position]);
            //var size = Unsafe.SizeOf<T>();
            //TrySetBuffLength(size);
            //Span<byte> span = stackalloc byte[size];
            //Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), result);
            //Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref span[0], (uint)size);
            //Position += size;
            //var d = new Span<TextInfo>(1,2);


            //var sp = new Span<byte>(result);
            //var dd = MemoryMarshal.Read<T>(span);

            Position += Unsafe.SizeOf<T>();
            return result;
        }


        /// <summary>
        /// 根据传入值返回字节数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        /// <returns></returns>
        public byte[] Read<T>(T v) where T : unmanaged
        {
            var size = Unsafe.SizeOf<T>();
            Span<byte> span = stackalloc byte[size];
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), v);
            return span.ToArray();
        }
        /// <summary>
        /// 根据传入值返回字节数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="v"></param>
        /// <returns></returns>
        public byte[] Read(string v)
        {
            return Encoding.UTF8.GetBytes(v);
        }

        public string Read()
        {
            var length = ReadLength();
            if (length == 0) return string.Empty;
            var str = Encoding.UTF8.GetString(mBuffer, Position, length);
            Position += length;
            return str;
        }
        /// <summary>
        /// 读取压缩后的字节
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public int ReadInt()
        {
            return ReadWa32();
        }

        public int[] ReadIntArray()
        {
            var count = ReadLength();
            var v = new int[count];
            for (int i = 0; i < count; i++)
            {
                v[i] = ReadWa32();
            }
            return v;
        }
        public int[][] ReadIntArray2()
        {
            var count = ReadLength();
            var v = new int[count][];
            for (int i = 0; i < count; i++)
            {
                v[i] = ReadIntArray();
            }
            return v;
        }

        /// <summary>
        /// 读取压缩的字节
        /// </summary>
        /// <returns></returns>
        public uint ReadUint()
        {
            return (uint)ReadWa32();
        }
        /// <summary>
        /// 读取压缩的字节
        /// </summary>
        /// <returns></returns>
        public uint[] ReadUintArray()
        {
            var count = ReadLength();
            var v = new uint[count];
            for (int i = 0; i < count; i++)
            {
                v[i] = (uint)ReadWa32();
            }
            return v;
        }
        /// <summary>
        /// 读取压缩的字节
        /// </summary>
        /// <returns></returns>
        public uint[][] ReadUintArray2()
        {
            var count = ReadLength();
            var v = new uint[count][];
            for (int i = 0; i < count; i++)
            {
                v[i] = ReadUintArray();
            }
            return v;
        }

        private int ReadWa32()
        {
            //int result;
            //var by = Read<byte>();
            //if ((by & 0x80) == 0)
            //{
            //    return by;
            //}
            //result = by & 0x7f;
            //int offset = 7;
            ////读取32位, 64 位不写入
            //for (; offset < 32; offset += 7)
            //{
            //    int b = Read<byte>();
            //    if (b == -1)
            //    {
            //        throw new Exception("byte  decode error");
            //    }
            //    result |= (b & 0x7f) << offset;
            //    if ((b & 0x80) == 0)
            //    {
            //        return result;
            //    }
            //}
            return ReadLength();
            //// Keep reading up to 64 bits.
            //for (; offset < 64; offset += 7)
            //{
            //    int b = Read<byte>();
            //    if (b == -1)
            //    {
            //        throw new Exception("byte  decode error");
            //    }
            //    if ((b & 0x80) == 0)
            //    {
            //        return result;
            //    }
            //}
            //return result;
        }

        public T[] ReadArray<T>() where T : unmanaged
        {
            var array = ReadAarrayLength<T>();
            var index = 0;
            while (index < array.Length)
            {
                array[index++] = Read<T>();
            }
            return array;
        }

        public string[] ReadArray()
        {
            var array = ReadAarrayLength<string>();
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = Read();
            }
            return array;
        }

        public T[][] ReadArray2<T>() where T : unmanaged
        {
            var length = ReadLength();
            var array = new T[length][];
            for (int i = 0; i < length; i++)
            {
                array[i] = ReadArray<T>();
            }
            return array;
        }

        public string[][] ReadArray2()
        {
            var length = ReadLength();
            var array = new string[length][];
            for (int i = 0; i < length; i++)
            {
                array[i] = ReadArray();
            }
            return array;
        }


        private T[] ReadAarrayLength<T>()
        {
            var length = ReadLength();
            return length == 0 ? Array.Empty<T>() : new T[length];
        }

        public void TrySetBuffLength(int newSize)
        {
            if (mBuffer == null || mBuffer.Length == 0)
            {
                mBuffer = new byte[newSize];
            }
            else
            {
                if (Position + newSize > mBuffer.Length)
                {
                    var s = Position + newSize;
                    byte[] bytes = new byte[s];
                    //Span<byte> bytes = stackalloc byte[s];
                    Unsafe.CopyBlock(ref bytes[0], ref mBuffer[0], (uint)mBuffer.Length);
                    mBuffer = bytes;
                }
            }
            Length = mBuffer.Length;
        }

        public void CopyFrom(byte[] from, int fromIndex, int size)
        {
            TrySetBuffLength(size);
            Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref from[fromIndex], (uint)size);
            Position += size;
        }

        public void SetPosition(uint position)
        {
            Position = (int)position;
        }
        //public override string ToString()
        //{
        //    var count = Math.Min(short.MaxValue, Length);
        //    var strbuf = new StringBuilder(count);
        //    for (int i = 0; i < count; i++)
        //    {
        //        strbuf.Append(Bytes[i].ToString("x2", System.Globalization.CultureInfo.InvariantCulture));
        //        //return Number.FormatUInt32(m_value, format, null);
        //    }

        //    return strbuf.ToString();
        //}

        public static implicit operator byte[](ByteStream buffer) => buffer.Span.ToArray();

        public static implicit operator ByteStream(byte[] buffer) => new ByteStream(buffer);

    }
}
