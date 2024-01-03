using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace F
{
    public class RandomUtil
    {
        public static void RandomNext()
        {
        }


        /// <summary>
        /// 随机指定x个值和为targetSum
        /// </summary>
        /// <param name="targetSum">总值</param>
        /// <param name="minValue">最小值</param>
        /// <param name="length">数量</param>
        /// <returns></returns>
        public static int[] GenerateRandomValues(int targetSum, int minValue, int length)
        {
            if (length <= 1)
            {
                throw new ArgumentException("长度最小为2");
            }
            var hash = new HashSet<int>();
            //var startValue = minValue + length - 2;
            var startValue = minValue + length;
            var sum = 0;
            var random = new Random();
            for (int i = length - 1; i >= 1; i--)
            {
                //--剩余值的最大可能值
                var maxPossibleValue = targetSum - sum - i * startValue;
                var value = random.Next(minValue, maxPossibleValue);
                //差值
                var remainV = targetSum - sum - value;
                while (hash.Contains(value) || (i == 1 && (value == remainV || hash.Contains(remainV))))
                {
                    value = random.Next(minValue, maxPossibleValue);
                    //差值
                    remainV = targetSum - sum - value;
                }
                hash.Add(value);
                sum += value;
            }
            var fourthValue = targetSum - sum;
            hash.Add(fourthValue);
            return hash.ToArray();
        }
    }
}
