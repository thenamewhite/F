using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// author  (hf) time：2023/4/14 16:11:28

namespace CommonUtility
{
    public struct Ve2
    {
        public float x;
        public float y;
        public static Ve2 Zero = new Ve2();
        public static Ve2 Forward = new Ve2(0, 0);
        public Ve2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public static Ve2 operator +(Ve2 v1, Ve2 v2) => new Ve2(v1.x + v2.x, v1.y + v2.y);
        public static Ve2 operator -(Ve2 v1, Ve2 v2) => new Ve2(v1.x - v2.x, v1.y - v2.y);
        public static Ve2 operator /(Ve2 v1, float v2) => new Ve2(v1.x / v2, v1.y / v2);
        public static Ve2 operator *(Ve2 v1, float v2) => new Ve2(v1.x * v2, v1.y * v2);


        /// <summary>
        /// 获取2个单位的距离
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Distance(Ve2 a, Ve2 b)
        {
            float num = a.x - b.x;
            float num2 = a.y - b.y;
            return (float)Math.Sqrt(num * num + num2 * num2);
        }
        /// <summary>
        /// 点乘
        /// </summary>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static float Dot(Ve2 lhs, Ve2 rhs)
        {
            return lhs.x * rhs.x + lhs.y * rhs.y;
        }

        /// <summary>
        /// 反余弦 角度
        /// </summary>
        /// <param name="dot"></param>
        /// <returns></returns>
        public static float AcosAngle(float dot)
        {
            //反余弦计算角度
            var offsetAngle = Math.Acos(dot) * (180 / Math.PI);
            return (float)offsetAngle;
        }

        public static Ve2 Cross(Ve2 lhs, Ve2 rhs)
        {
            return new Ve2(lhs.y * rhs.x - lhs.x * rhs.y, lhs.x * rhs.y - lhs.y * rhs.x);
        }
        /// <summary>
        /// 
        /// </summary>
        public float Magnitude
        {
            get => (float)Math.Sqrt(x * x + y * y);
        }
        //
        // 摘要:
        //     Returns the squared length of this vector (Read Only).
        public float SqrMagnitude => x * x + y * y;
        /// <summary>
        /// 归一化
        /// </summary>
        public Ve2 Normalize
        {
            get
            {
                float num = Magnitude;
                //folat 精度不判断0 
                //decimal.Parse(1E-05.ToString(), System.Globalization.NumberStyles.Float)= 0.00001
                if (num > 1E-05f)
                {
                    return this / num;
                }
                return Ve2.Zero;
            }
        }

        public static float Clamp(float value, float min, float max)
        {
            if (value < min)
            {
                value = min;
            }
            else if (value > max)
            {
                value = max;
            }

            return value;
        }
    }
}
