using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// author  (hf) time：2023/3/28 11:28:00

namespace CommonUtility
{
    public static class DictionaryExtend
    {
        public static TSource GetIndex<TSource>(this IEnumerable<TSource> source, int index)
        {
            IList<TSource> list = source as IList<TSource>;
            if (list != null)
            {
                return list[index];
            }
            if (index >= 0)
            {
                using (IEnumerator<TSource> enumerator = source.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (index == 0)
                        {
                            return enumerator.Current;
                        }
                        index--;
                    }
                }
            }
            return default;
        }
    }
}
