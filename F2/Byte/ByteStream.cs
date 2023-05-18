﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
// author  (hf) time：2023/4/20 17:03:08
namespace F
{
    /// <summary>
    /// 字节管理
    /// </summary>
    public ref struct ByteStream
    {
        public int Position;
        public int Length;

        private Span<byte> mBuffer;
        //public byte[] mBuffer;

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
            mBuffer = new Span<byte>(buffer);
        }

        public ByteStream(Span<byte> buffer)
        {
            Position = 0;
            Length = buffer.Length;
            //mBuffer = new Span<byte>(buffer.ToArray());
            mBuffer = buffer;
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
        public unsafe void Push<T>(in T v) where T : unmanaged
        {
            //TODO没走压缩数据大小的，获取了类型字节长度直接write,后续在看是否压缩字节
            var length = sizeof(T);
            TrySetBuffLength(length);
            Span<byte> span = stackalloc byte[length];
            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(span), v);
            Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref span[0], (uint)length);
            Position += length;
        }
        public unsafe void Push(string v, Encoding encoding = null)
        {
            var strCount = (v?.Length).GetValueOrDefault();
            fixed (char* cPtr = v)
            {
                PushLength(strCount);
                var byteCount = (encoding ?? Encoding.UTF8).GetByteCount(cPtr, strCount);
                TrySetBuffLength(byteCount);
                fixed (byte* bptr = mBuffer)
                {
                    Encoding.UTF8.GetBytes(cPtr, strCount, bptr + Position, byteCount);
                }
                Position += byteCount;
            }
        }

        public unsafe void Push<T>(T[] v) where T : unmanaged
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
                    byte[] bytArr = new byte[sizeof(T) * length];
                    fixed (T* pInt = v)
                    {
                        byte* pByte = (byte*)pInt;
                        for (int i = 0; i < bytArr.Length; i++)
                        {
                            bytArr[i] = pByte[i];
                        }
                    }
                    TrySetBuffLength(bytArr.Length);
                    Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref bytArr[0], (uint)bytArr.Length);
                    Position += bytArr.Length;
                }
            }
        }
        public unsafe void Push(string[] v)
        {
            var length = (v?.Length).GetValueOrDefault();
            PushLength(length);
            if (length > 0)
            {
                for (int i = 0; i < v.Length; i++)
                {
                    Push(v[i]);
                }
            }
        }
        //public unsafe void Push(List<string> v)
        //{
        //    var length = (v?.Count).GetValueOrDefault();
        //    PushLength(length);
        //    if (length > 0)
        //    {
        //        for (int i = 0; i < v.Count; i++)
        //        {
        //            Push(v[i]);
        //        }
        //    }
        //}
        //public unsafe void Push<T>(List<T> v) where T : unmanaged
        //{
        //    var length = (v?.Count).GetValueOrDefault();
        //    PushLength(length);
        //    if (length > 0)
        //    {
        //        for (int i = 0; i < v.Count; i++)
        //        {
        //            Push(v[i]);
        //        }
        //    }
        //}

        public unsafe void PushLength(long value)
        {
            byte* buffer = stackalloc byte[8];
            //參考proto WriteRawVarint64
            int count = 0;
            while (value > 127)
            {
                buffer[count] = (byte)((value & 0x7F | 0x80));
                value >>= 7;
                count++;
            }
            buffer[count] = (byte)value;
            count++;
            TrySetBuffLength(count);
            fixed (byte* bptr = mBuffer)
            {
                Unsafe.CopyBlockUnaligned(bptr + Position, buffer, (uint)count);
            }
            Position += count;
        }

        public int ReadLength()
        {
            //proto ParseRawVarint64
            ulong result = mBuffer[Position++];
            if (result < 128)
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

        public unsafe ref T Read<T>() where T : unmanaged
        {
            ref var result = ref Unsafe.As<byte, T>(ref mBuffer[Position]);
            Position += sizeof(T);
            return ref result;
        }
        public unsafe string Read()
        {
            var count = ReadLength();
            if (count == 0) return string.Empty;
            fixed (byte* bptr = mBuffer)
            {
                var str = Encoding.UTF8.GetString(bptr + Position, count);
                Position += count;
                return str;
            }
        }

        public unsafe T[] ReadArray<T>() where T : unmanaged
        {
            var array = ReadAarray<T>();
            var index = 0;
            while (index < array.Length)
            {
                array[index++] = Read<T>();
            }
            return array;
        }

        public unsafe string[] ReadArray()
        {
            var length = ReadLength();
            var array = length == 0 ? Array.Empty<string>() : new string[1] { Read() };
            return array;
        }
        //public unsafe List<string> ReadArray<T>()
        //{
        //    var length = ReadLength();
        //    var array = length == 0 ? new List<string>() : new List<string> { Read() };
        //    return array;
        //}


        private T[] ReadAarray<T>()
        {
            var length = ReadLength();
            return length == 0 ? Array.Empty<T>() : new T[length];
        }


        public void TrySetBuffLength(int newSize)
        {
            if (mBuffer.Length == 0)
            {
                mBuffer = new Span<byte>(new byte[newSize]);
            }
            else
            {
                if (Position + newSize > mBuffer.Length)
                {
                    var s = Position + newSize;
                    byte[] bytes = new byte[s];
                    Unsafe.CopyBlock(ref bytes[0], ref mBuffer[0], (uint)mBuffer.Length);
                    mBuffer = bytes;
                }
            }
            Length = mBuffer.Length;
        }

        public void CopyFrom(byte[] from, int fromIndex, int size)
        {
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

        public static implicit operator byte[](ByteStream Buffer) => Buffer.Span.ToArray();
    }
}
