using System;
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

        public Span<byte> mBuffer;
        //public byte[] mBuffer;

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
            //get { return mBuffer; }
            get => mBuffer;
        }

        //public byte[] Bytes
        //{
        //    get => mBuffer.ToArray();
        //}

        /// <summary>
        /// 基础类型
        /// </summary>
        public void Push<T>(in T v) where T : unmanaged
        {
            unsafe
            {
                fixed (T* ptr = &v)
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
            PushLength(strCount);
        }

        public unsafe void Push<T>(T[] v) where T : unmanaged
        {
            var length = (v?.Length).GetValueOrDefault();
            byte[] bytArr = new byte[sizeof(T) * length];
            //Span<byte> bytArr = new Span<byte>();
            fixed (T* pInt = v)
            {
                byte* pByte = (byte*)pInt;
                for (int i = 0; i < bytArr.Length; i++)
                {
                    bytArr[i] = pByte[i];
                }
            }
            SetBuffSize(bytArr.Length);
            Unsafe.CopyBlockUnaligned(ref mBuffer[Position], ref bytArr[0], (uint)bytArr.Length);
            Position += bytArr.Length;
            PushLength(length);
        }
        public unsafe void Push(string[] v)
        {
            var length = (v?.Length).GetValueOrDefault();
            if (length == 0)
            {
                Push(0);
            }
            else
            {
                for (int i = 0; i < v.Length; i++)
                {
                    Push(v[i]);
                }
            }
        }

        public unsafe void PushLength(int value)
        {
            byte* buffer = stackalloc byte[8];
            //參考proto WriteUInt32Variant
            int count = 0;
            do
            {
                buffer[count] = (byte)((value & 0x7F) | 0x80);
                count++;
            } while ((value >>= 7) != 0);
            SetBuffSize(count);
            Position += count;
            Unsafe.CopyBlockUnaligned(ref mBuffer[Position - 1], ref buffer[0], (uint)count);
        }
        public unsafe ref T Read<T>() where T : unmanaged
        {
            ref var result = ref Unsafe.As<byte, T>(ref mBuffer[Position]);
            Position += sizeof(T);
            return ref result;
        }
        public unsafe string Read()
        {
            var count = ReadLength(1);
            if (count == 0) return string.Empty;
            fixed (byte* bptr = mBuffer)
            {
                var str = Encoding.UTF8.GetString(bptr + Position, count);
                Position += count + 1;
                return str;
            }
        }

        public unsafe T[] ReadArray<T>() where T : unmanaged
        {
            var size = sizeof(T); ;
            var array = ReadAarray<T>(size);
            var index = 0;
            while (index < array.Length)
            {
                array[index++] = Read<T>();
            }
            Position += 1;
            return array;
        }

        public unsafe string[] ReadArray()
        {
            var length = ReadLength(1);
            var array = length == 0 ? Array.Empty<string>() : new string[1] { Read() };
            return array;
        }


        private T[] ReadAarray<T>(int size)
        {
            var length = ReadLength(size);
            return length == 0 ? Array.Empty<T>() : new T[length];
        }


        public int ReadLength(int size)
        {
            if (Length <= Position)
            {
                return 0;
            }
            var length = 0;
            var offset = Position;
            var v = mBuffer[offset];
            for (int i = Position; i < mBuffer.Length; i += size)
            {
                if ((byte)((v & 0x7F) | 0x80) == v)
                {
                    //length++;
                    break;
                }
                v = mBuffer[offset += size];
                length++;
            }
            return length;
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
