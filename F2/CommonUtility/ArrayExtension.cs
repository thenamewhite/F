
using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime;
namespace F
{
    public static class ArrayExtension
    {
        /// <summary>
        /// 移除指定index下标，依次向前递增
        /// </summary>
        /// <param name="array"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        public static void RemoveIndex<T>(ref T[] array, int index)
        {
            var count = array.Length;
            for (int i = index + 1; i < count; i++)
            {
                array[i - 1] = array[i];
            }
            Array.Resize(ref array, count - 1);
        }
        public static void RemoveIndex<T>(this Array arr, ref T[] array, int index)
        {
            RemoveIndex(ref array, index);
        }
        /// <summary>
        /// 数组尾部添加值
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public static void Add<T>(ref T[] array, T value)
        {
            Array.Resize(ref array, array.Length + 1);
            array[^1] = value;
        }
        /// <summary>
        /// 数组尾部添加值
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="array"></param>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        public static void Add<T>(this Array arr, ref T[] array, T value)
        {
            Add(ref array, value);
        }

        /// <summary>
        /// 获取数组指定范围内的值
        /// </summary>
        /// <typeparam name = "T" ></ typeparam >
        /// < param name="array"></param>
        /// <param name = "startIndex" ></ param >
        /// < param name="endIndex"></param>
        /// <returns></returns>
        //public static T[] GetStartToEndIndex<T>(ref T[] array, int startIndex, int endIndex)
        //{
        //return array[startIndex..endIndex];
        //}

    }
}