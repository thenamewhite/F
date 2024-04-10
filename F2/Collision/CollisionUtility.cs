using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

// author  (hf) time：2023/4/14 16:09:55

namespace F
{

    /// <summary>
    /// 用于一些碰撞检测
    /// </summary>
    public static class CollisionUtility
    {
        /// <summary>
        /// 圆和扇形相交, https://zhuanlan.zhihu.com/p/607728812 参考实现
        /// </summary>
        /// <param name="sectorPosition">扇形中心点坐标</param>
        /// <param name="sectorForward">扇形自身朝向</param>
        /// <param name="sectorAngle">扇形角度</param>
        /// <param name="sectorRadius">扇形半径</param>
        /// <param name="circlePosition">目标</param>
        /// <param name="circleRadius">目标半径</param>
        public static bool IsSectorDiskIntersect(Ve2 sectorPosition, Ve2 sectorForward, float sectorAngle, float sectorRadius, Ve2 circlePosition, float circleRadius)
        {
            // 1. 如果扇形圆心和圆盘圆心的方向能分离，两形状不相交
            var d = circlePosition - sectorPosition;
            float rsum = sectorRadius + circleRadius;
            if (d.SqrMagnitude > rsum * rsum)
                return false;
            //radian=sectorAngle* (180 / Math.PI)
            // sectorForward.X = Math.Cos(radian) ;
            //sectorForward.Z = Math.Sin(radian);


            // 2. 计算出扇形局部空间的 p
            float px = Ve2.Dot(d, sectorForward);
            float py = Math.Abs(Ve2.Dot(d, new Ve2(-sectorForward.y, sectorForward.x)));

            float theta = (float)(sectorAngle / 2f * (Math.PI / 180f));
            // 3. 如果 p_x > ||p|| cos theta，两形状相交
            if (px > d.Magnitude * Math.Cos(theta))
                return true;

            // 4. 求左边线段与圆盘是否相交 ，计算扇形角度增量位置
            var q = new Ve2((float)Math.Cos(theta), (float)Math.Sin(theta)) * sectorRadius;
            var p = new Ve2(px, py);
            return SegmentPointSqrDistance(Ve2.Zero, q, p) <= circleRadius * circleRadius;
        }

        // 计算线段与点的最短平方距离
        // x0 线段起点
        // u  线段方向至末端点
        // x  任意点
        static float SegmentPointSqrDistance(Ve2 x0, Ve2 u, Ve2 x)
        {
            float t = Ve2.Dot(x - x0, u) / u.SqrMagnitude;
            return (x - (x0 + u * Ve2.Clamp(t, 0, 1))).SqrMagnitude;
        }

        /// <summary>
        /// 2圆是否相交
        /// </summary>
        public static bool IsCircleIntersect(Ve2 v1, Ve2 v2, float v1Radius, float v2Raius)
        {
            //不使用平方更计算
            //var p = (v2 - v1).Magnitude;
            //return v1Radius - v1Radius < p && p < v1Radius + v2Raius;
            var d = v1 - v2;
            float rsum = v1Radius + v2Raius;
            return d.SqrMagnitude < rsum * rsum;
        }

        /// <summary>
        /// 判断一个点的否在园中
        /// </summary>
        /// <returns></returns>
        public static bool IsPointInCircle(Ve2 v1, Ve2 ve2, float radius)
        {
            double dx = v1.x - ve2.x;
            double dy = v1.y - ve2.y;
            double distanceSquared = dx + dy * dy;
            return distanceSquared <= radius * radius;

        }
        /// <summary>
        /// 射线否和球相交
        /// </summary>
        /// <param name="origin">射线起点</param>
        /// <param name="direction">射线方向</param>
        /// <param name="center">球中心</param>
        /// <param name="radius">球半径</param>
        /// <returns></returns>
        public static bool IsRayIntersectingSphere(Vector3 origin, Vector3 direction, Vector3 center, float radius)
        {
            // 球心到射线起点向量
            Vector3 Voc = center - origin;

            // 射线起点到球心向量在射线方向上的投影
            float Poc = Vector3.Dot(Voc, direction);

            // 球心到射线的最短距离平方
            float dSquared = Vector3.Dot(Voc, Voc) - Poc * Poc;

            // 最短距离
            float d = MathF.Sqrt(dSquared);

            // 判断相交情况
            return d <= radius;
        }
    }

}
