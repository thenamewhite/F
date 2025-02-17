﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
// author  (hf) time：2023/4/20 17:03:08
namespace F
{
    /// <summary>
    ///     字节管理
    /// </summary>
    public struct ByteStream : IDisposable
    {
        public int Position { private set; get; }
        public int Length { private set; get; }
        public bool IsNotWriteLength { get; }
        public bool IsEnd => Length == Position;
        public Span<byte> Span => mBuffer;

        public byte[] Bytes
        {
            get => mBuffer;
            set
            {
                mBuffer = value;
                Length = value.Length;
                Position = 0;
            }
        }


        private byte[] mBuffer;

        public ByteStream(int length, bool isNotWriteLength = false)
        {
            Position = 0;
            Length = length;
            mBuffer = new byte[Length];
            IsNotWriteLength = isNotWriteLength;
        }

        public ByteStream(byte[] buffer, bool isNotWriteLength = false)
        {
            Position = 0;
            Length = buffer.Length;
            //mBuffer = new Span<byte>(buffer);
            mBuffer = buffer;
            IsNotWriteLength = isNotWriteLength;
        }

        public ByteStream(Span<byte> buffer, bool isNotWriteLength = false)
        {
            Position = 0;
            Length = buffer.Length;
            //mBuffer = new Span<byte>(buffer.ToArray());
            mBuffer = buffer.ToArray();
            IsNotWriteLength = isNotWriteLength;
        }


        /// <summary>
        ///     基础类型
        /// </summary>
        public void Push<T>(T v) where T : unmanaged
        {
            var size = Unsafe.SizeOf<T>();
            TrySetBuffLength(size);
            Unsafe.WriteUnaligned(ref mBuffer[Position], v);
            Position += size;
        }

        /// <summary>
        ///     int 压缩字节
        /// </summary>
        /// <param name="v"></param>
        public void PushInt(int v)
        {
            //var encoded_value = (v << 1) ^ (v >> 31);
            //WriteRaw64(encoded_value);
            ZigzagEncode(v);
        }

        /// <summary>
        ///     处理为一个正整数
        /// </summary>
        /// <param name="value"></param>
        private void ZigzagEncode(int value)
        {
            // 将value左移一位，再与value右移31位进行异或运算
            var encoded_value = (value << 1) ^ (value >> 31);
            // 将encoded_value写入输出流
            WriteRawVarint64(encoded_value);
        }

        /// <summary>
        ///     解码
        /// </summary>
        /// <returns></returns>
        private int ZigzagDecode()
        {
            // 解析原始64位整数
            var encodedValue = ParseRawVarint64();
            // 将编码值右移一位，再与-1进行位与运算，最后再进行位异或运算
            var decodedValue = (encodedValue >> 1) ^ -(encodedValue & 1);
            // 返回解码后的值
            return decodedValue;
        }

        public void PushInt(int[] value)
        {
            //TrySetBuffLength(byteLength);
            PushLength(value.Length);
            foreach (var v in value) PushLength(v);
        }

        public void PushInt(int[][] value)
        {
            PushLength(value.Length);
            foreach (var v in value) PushInt(v);
        }

        /// <summary>
        ///     uint 压缩字节
        /// </summary>
        /// <param name="v"></param>
        public void PushUInt(uint v)
        {
            PushLength(v);
        }

        public void PushUInt(uint[] value)
        {
            PushLength(value.Length);
            foreach (var v in value) PushLength(v);
        }

        public void PushUInt(uint[][] value)
        {
            PushLength(value.Length);
            foreach (var v in value) PushUInt(v);
        }

        public void Push(string v, Encoding encoding = null)
        {
            var bytes = (encoding ?? Encoding.UTF8).GetBytes(v == null ? string.Empty : v);
            var length = bytes.Length;
            PushLength(length);
            TrySetBuffLength(length);
            if (length > 0)
            {
                Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref bytes[0], (uint)length);
                Position += length;
            }
        }

        //public void Push<T>(T[] v) where T : unmanaged
        //{
        //    var length = (v?.Length).GetValueOrDefault();
        //    PushLength(length);
        //    if (length > 0)
        //    {
        //        var size = Unsafe.SizeOf<T>();
        //        var byteLength = size * length;
        //        TrySetBuffLength(byteLength);
        //        Unsafe.WriteUnaligned(ref mBuffer[Position], v);
        //        Position += byteLength;
        //    }
        //}

        public void Push<T>(T[] v) where T : unmanaged
        {
            var length = (v?.Length).GetValueOrDefault();
            PushLength(length);
            if (length > 0)
            {
                if (v is byte[] bytes)
                {
                    TrySetBuffLength(length);
                    Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref bytes[0], (uint)length);
                    Position += length;
                }
                else
                {
                    var size = Unsafe.SizeOf<T>();
                    var byteLength = size * length;
                    TrySetBuffLength(byteLength);
                    for (var i = 0; i < v.Length; i++)
                    {
                        Unsafe.WriteUnaligned(ref mBuffer[Position], v[i]);
                        Position += size;
                    }
                }
            }
        }

        public void Push(string[] v)
        {
            var length = (v?.Length).GetValueOrDefault();
            PushLength(length);
            for (var i = 0; i < v.Length; i++) Push(v[i]);
        }

        public void Push<T>(T[][] v) where T : unmanaged
        {
            var length = (v?.Length).GetValueOrDefault();
            PushLength(length);
            for (var i = 0; i < length; i++) Push(v[i]);
        }

        public void Push(string[][] v)
        {
            var length = (v?.Length).GetValueOrDefault();
            PushLength(length);
            for (var i = 0; i < length; i++) Push(v[i]);
        }


        public void PushLength(long value)
        {
            if (IsNotWriteLength) return;
            WriteRawVarint64(value);
        }

        private void WriteRawVarint64(long value)
        {
            //当vlaue 等于long.maxValue是 需要9个字节
            Span<byte> buffer = stackalloc byte[8];
            var count = 0;
            while (value > 0x7f)
            {
                buffer[count] = (byte)((value & 0x7F) | 0x80);
                value >>= 7;
                count++;
            }

            buffer[count] = (byte)value;
            count++;
            TrySetBuffLength(count);
            Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref buffer[0], (uint)count);
            Position += count;
        }

        private int ParseRawVarint64()
        {
            ulong result = mBuffer[Position++];
            if (result < 0x80) return (int)result;

            result &= 0x7f;
            var shift = 7;
            do
            {
                var b = mBuffer[Position++];
                result |= (ulong)(b & 0x7F) << shift;
                if (b < 0x80) return (int)result;
                shift += 7;
            } while (shift < 64);

            return (int)result;
        }

        public int ReadLength()
        {
            return ParseRawVarint64();
        }

        /// <summary>
        ///     从存入的字节数组中读取指定类型值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Read<T>() where T : unmanaged
        {
            ref var result = ref Unsafe.As<byte, T>(ref mBuffer[Position]);
            Position += Unsafe.SizeOf<T>();
            return result;
        }


        /// <summary>
        ///// 根据传入值返回字节数组
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="v"></param>
        ///// <returns></returns>
        public byte[] Read<T>(T v) where T : unmanaged
        {
            int size = Unsafe.SizeOf<T>();
            var result = new byte[size];
            var span = result.AsSpan();
            Unsafe.WriteUnaligned(ref span[0], v);
            return result;
        }

        /// <summary>
        ///     根据传入值返回字节数组
        /// </summary>
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
        ///     根据传入的长度读取字符
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string Read(int length)
        {
            if (length <= 0) return string.Empty;
            var str = Encoding.UTF8.GetString(mBuffer, Position, length);
            Position += length;
            return str;
        }

        /// <summary>
        ///     读取压缩后的字节
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public int ReadInt()
        {
            return ZigzagDecode();
        }

        public int[] ReadIntArray()
        {
            var count = ReadLength();
            var v = new int[count];
            for (var i = 0; i < count; i++) v[i] = ReadLength();
            return v;
        }

        public int[][] ReadIntArray2()
        {
            var count = ReadLength();
            var v = new int[count][];
            for (var i = 0; i < count; i++) v[i] = ReadIntArray();
            return v;
        }

        /// <summary>
        ///     读取压缩的字节
        /// </summary>
        /// <returns></returns>
        public uint ReadUint()
        {
            return (uint)ReadLength();
        }

        /// <summary>
        ///     读取压缩的字节
        /// </summary>
        /// <returns></returns>
        public uint[] ReadUintArray()
        {
            var count = ReadLength();
            var v = new uint[count];
            for (var i = 0; i < count; i++) v[i] = (uint)ParseRawVarint64();
            return v;
        }

        /// <summary>
        ///     读取压缩的字节
        /// </summary>
        /// <returns></returns>
        public uint[][] ReadUintArray2()
        {
            var count = ReadLength();
            var v = new uint[count][];
            for (var i = 0; i < count; i++) v[i] = ReadUintArray();
            return v;
        }


        public T[] ReadArray<T>() where T : unmanaged
        {
            var array = ReadAarrayLength<T>();
            var index = 0;
            while (index < array.Length) array[index++] = Read<T>();
            return array;
        }

        public string[] ReadArray()
        {
            var array = ReadAarrayLength<string>();
            for (var i = 0; i < array.Length; i++) array[i] = Read();
            return array;
        }

        public T[][] ReadArray2<T>() where T : unmanaged
        {
            var length = ReadLength();
            var array = new T[length][];
            for (var i = 0; i < length; i++) array[i] = ReadArray<T>();
            return array;
        }

        public string[][] ReadArray2()
        {
            var length = ReadLength();
            var array = new string[length][];
            for (var i = 0; i < length; i++) array[i] = ReadArray();
            return array;
        }


        private T[] ReadAarrayLength<T>()
        {
            var length = ReadLength();
            return length == 0 ? Array.Empty<T>() : new T[length];
        }

        private void TrySetBuffLength(int newSize)
        {
            if (mBuffer == null || mBuffer.Length == 0)
            {
                mBuffer = new byte[newSize];
            }
            else
            {
                if (Position + newSize > mBuffer.Length)
                {
                    var newLength = Position + newSize;
                    //byte[] newBuffer = new byte[newLength];
                    ////// 使用 Buffer.BlockCopy 进行内存拷贝
                    //Buffer.BlockCopy(mBuffer, 0, newBuffer, 0, mBuffer.Length);
                    //mBuffer = newBuffer;
                    Array.Resize(ref mBuffer, newLength);
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

        public void Dispose()
        {
            Array.Clear(mBuffer, 0, mBuffer.Length);
            mBuffer = null;
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

        public static implicit operator byte[](ByteStream buffer)
        {
            return buffer.mBuffer;
        }

        public static implicit operator Span<byte>(ByteStream buffer)
        {
            return buffer.mBuffer;
        }

        public static implicit operator ByteStream(byte[] buffer)
        {
            return new ByteStream(buffer);
        }
    }
}