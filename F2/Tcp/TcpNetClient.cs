using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
namespace F
{

    public class TcpNetClient
    {

        private static readonly object locker = new object();

        public Socket Socket
        {
            get;
            private set;
        }

        /// <summary>
        /// 发起连接请求
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="timeOut"></param>
        public async void Connect(string address, int port, int timeOut = 5000)
        {
            Socket = new Socket(Dns.GetHostAddresses(address)[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            await Task.WhenAll(Socket.ConnectAsync(address, port), Task.Delay(timeOut));
            if (Socket.Connected)
            {
                OnConnectSuccess();
            }
            else
            {
                Socket = null;
            }
            //Socket.Connected? Close();
        }
        public void Close() { }

        public void Receive()
        {
        }
        /// <summary>
        /// 连接成功
        /// </summary>
        public virtual void OnConnectSuccess()
        {
        }

        public void Send(byte[] bytes, int offect, int length)
        {
            lock (locker)
            {
                if (Socket.Connected)
                {
                    Socket.Send(bytes, offect, length, SocketFlags.None);
                }
                else
                {
                    //连接断开
                }
            }
        }
    }
}
