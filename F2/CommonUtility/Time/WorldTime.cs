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

        private float frameTime;

        public float Time
        {
            get;
            set;
        }

        public WorldTime(float frameRate)
        {

            frameTime = 1f / frameRate;
        }

        private List<DateTimeDown> mDateTimes = new List<DateTimeDown>();
        private ObjectPool<DateTimeDown> mTimePool = new ObjectPool<DateTimeDown>();

        public struct DateTime
        {
            /// <summary>
            /// 间隔时间
            /// </summary>
            public float Time;
            /// <summary>
            /// 执行次数
            /// </summary>
            public int Num;
            /// <summary>
            /// 首次执行间隔时间
            /// </summary>
            public float FirstTime;
            public Action<float> Callback;

            //public static bool operator ==(DateTime v, DateTime v2)
            //{
            //    return v.Callback == v2.Callback;
            //}
            //public static bool operator !=(DateTime v, DateTime v2)
            //{
            //    return v.Callback != v2.Callback;
            //}
        }

        public class DateTimeDown : IInitialization
        {

            internal DateTime DateTime;
            /// <summary>
            /// 当前时间
            /// </summary>
            internal float CurrentTime;
            /// <summary>
            /// 已执行的次数
            /// </summary>
            internal int Num;
            /// <summary>
            /// 首次是否执行
            /// </summary>
            internal bool IsFirstExecute;

            /// <summary>
            /// 重新初始化
            /// </summary>
            public void Initialization()
            {

            }
            /// <summary>
            /// 对象被回收
            /// </summary>
            public void Release()
            {
                IsFirstExecute = false;
                Num = 0;
                DateTime = default;
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
                if (time.CurrentTime + time.DateTime.FirstTime <= Time && !time.IsFirstExecute)
                {
                    time.DateTime.Callback?.Invoke(Time - time.CurrentTime);
                    time.IsFirstExecute = true;
                    continue;
                }
                if (time.CurrentTime + time.DateTime.Time <= Time)
                {
                    time.CurrentTime += frameTime;
                    time.Num++;
                    time.DateTime.Callback?.Invoke(Time - time.CurrentTime);
                    if (time.Num >= time.DateTime.Num)
                    {
                        mTimePool.Release(time);
                        mDateTimes.RemoveAt(i);
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
                if (dateTime.DateTime.Callback == callback)
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
        public DateTimeDown AddTime(DateTime dateTime)
        {
            var time = mTimePool.New();
            time.DateTime = dateTime;
            time.CurrentTime = Time;
            mDateTimes.Add(time);
            return time;
        }

        /// <summary>
        /// <param name="time">间隔时间</param>
        /// <param name="num">执行次数,-1无限执行，0执行一次</param>
        /// <param name="callback"></param>
        /// <param name="firstTime">首次间隔时间,如果小于等于0，马上执行一次callback函数</param>
        /// </summary>
        public DateTimeDown AddTime(float time, int num, Action<float> callback, float firstTime = float.MaxValue)
        {
            var dateTime = new DateTime();
            dateTime.Callback = callback;
            dateTime.FirstTime = firstTime;
            if (dateTime.FirstTime <= 0)
            {
                callback(0);
            }
            dateTime.Num = num;
            dateTime.Time = time;
            return AddTime(dateTime);
        }
    }
}
