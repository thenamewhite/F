using System;
using System.Collections.Generic;

// author  (hf) time：2023/3/9 17:02:34

namespace F
{
    public class WorldTime
    {
        public class DateTimeDown : IInitialization
        {
            internal Action<float> Callback;

            /// <summary>
            ///当前时间
            /// </summary>
            internal float CurrentTime;

            /// <summary>
            ///首次执行间隔时间
            /// </summary>
            internal float FirstTime;

            /// <summary>
            ///间隔时间
            /// </summary>
            internal float IntervalTime;

            /// <summary>
            ///执行次数
            /// </summary>
            internal int Num;

            /// <summary>
            /// 重新初始化
            /// </summary>
            void IInitialization.Initialization()
            {
            }

            /// <summary>
            ///     对象被回收
            /// </summary>
            void IInitialization.Release()
            {
                Num = default;
                Callback = default;
                IntervalTime = default;
                CurrentTime = default;
                FirstTime = default;
            }
        }

        /// <summary>
        /// 受后台切换影响
        /// </summary>
        private readonly HashSet<DateTimeDown> mDateTimes;

        /// <summary>
        ///不被后台切换受影响,每次调用都是固定时间
        /// </summary>
        private readonly HashSet<DateTimeDown> mFixedDateTimes;

        /// <summary>
        ///每帧固定时间
        /// </summary>
        private readonly float mframeTime;

        private readonly ObjectPool<DateTimeDown> mTimePool;

        private float mOldTime;

        public WorldTime(float frameRate)
        {
            mframeTime = 1f / frameRate;
            //微信环境下 直接在字段上实例化会报错找不到类型
            mFixedDateTimes = mDateTimes = new HashSet<DateTimeDown>(8);
            mTimePool = new ObjectPool<DateTimeDown>();
        }

        private float mCurrentTime { get; set; }

        public void Update(float currentTime)
        {
            var count = mDateTimes.Count;
            var ass = mDateTimes.GetEnumerator();
            while (ass.MoveNext())
            {
                var timeData = ass.Current;
                if (timeData.FirstTime != float.MaxValue && timeData.CurrentTime + timeData.FirstTime <= currentTime)
                {
                    try
                    {
                        timeData.Callback?.Invoke(currentTime - timeData.CurrentTime);
                    }
                    finally
                    {
                        timeData.FirstTime = float.MaxValue;
                        timeData.CurrentTime += timeData.FirstTime;
                    }

                    continue;
                }

                if (timeData.CurrentTime + timeData.IntervalTime <= currentTime)
                {
                    var time = currentTime - timeData.CurrentTime;
                    timeData.CurrentTime += time;
                    try
                    {
                        timeData.Callback?.Invoke(time);
                    }
                    finally
                    {
                        if (timeData.Num != -1)
                            if (--timeData.Num <= 0)
                            {
                                mTimePool.Release(timeData);
                                mDateTimes.Remove(timeData);
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
            var ass = mDateTimes.GetEnumerator();
            while (ass.MoveNext())
            {
                var time = ass.Current;
                if (time.Callback == callback)
                {
                    mDateTimes.Remove(time);
                    break;
                }
            }
        }

        public void RemoveTime(DateTimeDown v)
        {
            if (mDateTimes.Remove(v)) mTimePool.Release(v);
        }

        /// <summary>
        /// </summary>
        /// <param name="time">间隔时间</param>
        /// <param name="callback">上次调用和这次时间间隔</param>
        /// <param name="num">执行次数,-1无限执行，默认0执行一次</param>
        /// <param name="firstTime">首次间隔时间,如果小于等于0，马上执行一次callback函数</param>
        /// <param name="isFixed">是否是固定时间，如果是ture 每次callback<v> v 都是time</param>
        /// <returns></returns>
        public DateTimeDown AddTime(float time, Action<float> callback, int num, float firstTime = float.MaxValue, bool isFixed = true)
        {
            foreach (var timeData in mDateTimes)
            {
                if (timeData.Callback == callback)
                    return timeData;
            }

            var obj = mTimePool.New();
            obj.CurrentTime = mCurrentTime;
            obj.IntervalTime = time;
            obj.Num = num;
            if (obj.FirstTime <= 0f) callback(0f);
            obj.FirstTime = firstTime == float.MaxValue ? float.MaxValue : firstTime;
            mDateTimes.Add(obj);
            return obj;
        }
    }
}