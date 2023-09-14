using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// author  (hf) time：2023/2/16 16:26:44

namespace F
{
    /// <summary>
    /// 初始化接口
    /// </summary>
    public interface IInitialization
    {
        /// <summary>
        /// 重新初始化
        /// </summary>
        void Initialization();
        /// <summary>
        /// 对象被回收
        /// </summary>
        void Release();
    }
}
