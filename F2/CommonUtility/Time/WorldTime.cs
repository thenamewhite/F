using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// author  (hf) time：2023/3/9 17:02:34

namespace F
{
    public class WorldTime
    {

        private readonly float frameTime;
        private readonly List<DateTimeDown> mDateTimes;
        private readonly ObjectPool<DateTimeDown> mTimePool;
        public float Time
        {
            get;
            set;
        }

        public WorldTime(float frameRate)
        {
            frameTime = 1f / frameRate;
            //微信环境下 直接在字段上实例化会报错找不到类型
            mDateTimes = new List<DateTimeDown>(8);
            mTimePool = new ObjectPool<DateTimeDown>();
        }

        public class DateTimeDown : IInitialization
        {
            /// <summary>
            /// 间隔时间
            /// </summary>
            internal float Time;
            /// <summary>
            /// 执行次数
            /// </summary>
            internal int Num;
            /// <summary>
            /// 首次执行间隔时间
            /// </summary>
            internal float FirstTime;
            internal Action<float> Callback;
            /// <summary>
            /// 当前时间
            /// </summary>
            internal float CurrentTime;
            /// <summary>
            /// 重新初始化
            /// </summary>
            void IInitialization.Initialization()
            {

            }
            /// <summary>
            /// 对象被回收
            /// </summary>
            void IInitialization.Release()
            {
                Num = default;
                Callback = default;
                Time = default;
                CurrentTime = default;
                FirstTime = default;
            }
            public DateTimeDown()
            {
            }
        }

        public void Update()
        {
            var count = mDateTimes.Count;
            for (int i = mDateTimes.Count - 1; i >= 0; i--)
            {
                if (i >= mDateTimes.Count)
                {
                    continue;
                }
                var time = mDateTimes[i];
                if (time.FirstTime != float.MaxValue && time.CurrentTime + time.FirstTime <= Time)
                {
                    try
                    {
                        time.Callback?.Invoke(Time - time.CurrentTime);
                    }
                    finally
                    {
                        time.FirstTime = float.MaxValue;
                        time.CurrentTime += time.FirstTime;
                    }
                    continue;
                }
                if (time.CurrentTime + time.Time <= Time)
                {
                    time.CurrentTime += frameTime;
                    try
                    {
                        time.Callback?.Invoke(Time - time.CurrentTime);
                    }
                    finally
                    {
                        if (--time.Num <= 0)
                        {
                            mTimePool.Release(time);
                            mDateTimes.Remove(time);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 移除定时器
        /// </summary>
        /// <returns></returns>
        public void RemoveTime(Action<float> callback)
        {
            for (int i = 0; i < mDateTimes.Count; i++)
            {
                var dateTime = mDateTimes[i];
                if (dateTime.Callback == callback)
                {
                    mTimePool.Release(dateTime);
                    mDateTimes.RemoveAt(i);
                    break;
                }
            }
        }
        public void RemoveTime(DateTimeDown v)
        {
            for (int i = 0; i < mDateTimes.Count; i++)
            {
                var dateTime = mDateTimes[i];
                if (dateTime == v)
                {
                    mTimePool.Release(dateTime);
                    mDateTimes.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="time">间隔时间</param>
        /// <param name="num">执行次数,-1无限执行，0执行一次</param>
        /// <param name="callback"></param>
        /// <param name="firstTime">首次间隔时间,如果小于等于0，马上执行一次callback函数</param>
        /// <returns></returns>
        public DateTimeDown AddTime(float time, int num, Action<float> callback, float firstTime = float.MaxValue)
        {
            var obj = mTimePool.New();
            obj.CurrentTime = Time;
            obj.FirstTime = firstTime == float.MaxValue ? float.MaxValue : firstTime;
            mDateTimes.Add(obj);
            return obj;
        }
    }
}
