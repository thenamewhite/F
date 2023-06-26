using F;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// author  (hf) Date：2023/4/20 14:36:22
namespace F
{
    public class TcpNetReceiver
    {
        public TcpNetClient Target;
        /// <summary>
        /// 消息头长度
        /// </summary>
        public int HeadSize;
        public int ReceivedPosition;
        public int ReceiveSize;
        public byte[] ReceiveBufferArray;

        private bool mIsHead = false;

        /// <summary>
        /// 消息头
        /// </summary>
        public virtual void ReceiveHead()
        {
            mIsHead = true;
        }

        public async void Receive()
        {
            int receiveCount = 0;
            while (Target.Socket?.Connected == true)
            {
                try
                {
                    receiveCount = await Target.Socket.ReceiveAsync(new ArraySegment<byte>(ReceiveBufferArray, ReceivedPosition, ReceiveSize - ReceivedPosition), SocketFlags.None);
                }
                //catch (Exception ex) { }
                catch (SocketException ex)
                {
                }
                if (!mIsHead)
                {
                    ReceiveHead();
                }
                else
                {
                    ReceiveData();
                }
                ReceivedPosition += receiveCount;
            }
        }
        public virtual void ReceiveData()
        {
            ReceivedPosition = 0;
            ReceiveSize = HeadSize;
            Target.Receive(ReceiveBufferArray);
        }
    }
}
