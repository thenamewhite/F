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

        /// <summary>
        /// 消息头
        /// </summary>
        public virtual void ReceiveHead()
        {
        }




        public async void Receive()
        {
            int receiveCount = 0;
            try
            {
                receiveCount = await Target.Socket.ReceiveAsync(new ArraySegment<byte>(ReceiveBufferArray, ReceivedPosition, ReceiveSize - ReceivedPosition), SocketFlags.None);
            }

            catch (SocketException ex)
            {
            }

            while (Target.Socket?.Connected == true)
            {
                //int receiveCount = 0;
                try
                {
                    receiveCount = await Target.Socket.ReceiveAsync(new ArraySegment<byte>(ReceiveBufferArray, ReceivedPosition, ReceiveSize - ReceivedPosition), SocketFlags.None);
                }
                //catch (Exception ex) { }
                catch (SocketException ex)
                {
                }
                ReceivedPosition += receiveCount;
                if (ReceivedPosition < ReceiveSize)
                {
                    continue;
                }
            }
        }
    }
}
