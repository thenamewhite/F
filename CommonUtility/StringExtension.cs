using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Schema;

// author  (hf) Date：2023/5/4 16:07:38
namespace F
{
    public static class StringExtension
    {

        /// <summary>
        /// 高性能的切割字符串
        /// </summary>
        /// <param name="source"></param>
        /// <param name="source"></param>
        /// <param name="slice"></param>
        /// <returns></returns>

        //public static string Split(this string t, string slice)
        //{
        //    ReadOnlySpan<char> s = t.AsSpan();
        //    string newString = default;
        //    var index = t.IndexOf(slice);
        //    if (index > -1)
        //    {
        //        newString = s.Slice(0, index).ToString();
        //    }
        //    return newString;
        //}
        public static string Split(string source, string slice)
        {
            var index = source.IndexOf(slice);
            string newString = source;
            if (index > -1)
            {
                ReadOnlySpan<char> s = source.AsSpan();
                newString = s.Slice(0, index).ToString();
            }
            return newString;
        }

        //public static int[] Span(int[] a)
        //{
        //    //var spa = new Span<int>(a).CopyTo;
        //    return spa.to();
        //}
    }
}
