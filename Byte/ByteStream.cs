using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public ByteStream(int length)
        {
            Position = 0;
            Length = length;
            mBuffer = new Span<byte>();
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
            mBuffer = new Span<byte>(buffer.ToArray());
        }

        public Span<byte> Span
        {
            get { return mBuffer; }
        }

        public byte[] Bytes
        {
            get => mBuffer.ToArray();
        }

        /// <summary>
        /// 基础类型
        /// </summary>
        public unsafe void Push<T>(in T data) where T : unmanaged
        {
            fixed (T* ptr = &data)
            {
                var size = sizeof(T);
                SetBuffSize(size);
                fixed (byte* bptr = mBuffer)
                {
                    Unsafe.CopyBlockUnaligned(bptr + Position, ptr, (uint)size);
                }
                Position += size;

            }
        }
        public unsafe void Push(string v, Encoding encoding = null)
        {
            var strCount = (v?.Length).GetValueOrDefault();
            fixed (char* cPtr = v)
            {
                var byteCount = (encoding ?? Encoding.UTF8).GetByteCount(cPtr, strCount);
                SetBuffSize(byteCount);
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
            byte[] bytArr = new byte[sizeof(T) * length];
            fixed (T* pInt = v)
            {
                byte* pByte = (byte*)pInt;
                for (int i = 0; i < bytArr.Length; i++)
                {
                    bytArr[i] = pByte[i];
                }
            }
            SetBuffSize(bytArr.Length);
            //intToBytes((v as int[]), 0);
            Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref bytArr[0], (uint)bytArr.Length);
            Position += bytArr.Length; 
        }
      

        public unsafe ref T Read<T>() where T : unmanaged
        {
            ref var result = ref Unsafe.As<byte, T>(ref mBuffer[Position]);
            Position += sizeof(T);
            return ref result;
        }

        public unsafe T[] ReadArray<T>() where T : unmanaged
        {
            T[] array = null;
            var size = sizeof(T); ;
            var length = ReadLength(size, ref array);
            for (int i = 0; i < length; i++)
            {
                ref var result = ref Unsafe.As<byte, T>(ref mBuffer[Position]);
                Position += size;
                array[i] = result;
            }
            return array;
        }

        public int ReadLength<T>(int size, ref T[] array)
        {
            var length = 0;
            var offset = Position;
            for (int i = Position; i < mBuffer.Length; i += size)
            {
                offset += size;
                if (offset >= mBuffer.Length)
                {
                    length += 1;
                    break;
                }
                var v = mBuffer[offset] & 0xff;
                if (v != 0)
                {
                    length += 1;
                }
                else
                {
                    break;
                }
            }
            array = length == 0 ? Array.Empty<T>() : new T[length];
            return length;
        }


        public unsafe string Read()
        {
            var count = Length - Position;
            if (count == 0) return string.Empty;
            fixed (byte* bptr = mBuffer)
            {
                var str = Encoding.UTF8.GetString(bptr + Position, count);
                Position += count;
                return str;
            }
        }
        private void SetBuffSize(int newSize)
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
        public static implicit operator byte[](ByteStream Buffer) => Buffer.Span.ToArray();
    }
}
