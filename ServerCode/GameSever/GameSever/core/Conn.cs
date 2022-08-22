using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace GameSever.core
{
    public class Conn
    {
        //常量缓冲区大小
        public const int BUFFER_SIZE = 1024;
        //与客户端连接的套接字
        public Socket socket;
        //标记对象是否被使用
        public bool isUse = false;
        //读缓冲区
        public byte[] readBuff = new byte[BUFFER_SIZE];
        //当前读缓冲区的长度
        public int buffCount = 0;
        //粘包分包
        //转换成byte[]类型的消息长度
        public byte[] lenBytes = new byte[sizeof(UInt32)];
        //消息长度
        public Int32 msgLength = 0;
        //心跳时间
        public long lastTickTime = long.MinValue;
        //对应的player
        public Player player;

        //构造函数
        public Conn()
        {
            readBuff = new byte[BUFFER_SIZE];
        }

        //初始化
        public void Init(Socket socket)
        {
            this.socket = socket;
            isUse = true;
            buffCount = 0;
            //心跳处理，稍后实现GetTimeStamp方法
            lastTickTime=Sys.GetTimeStamp();
        }

        //剩余的Buff空间长度
        public int BuffRemain()
        {
            return BUFFER_SIZE - buffCount;
        }

        //获取客户端地址
        public string GetAdress()
        {
            if (!isUse)
                return "无法获取地址";
            return socket.RemoteEndPoint.ToString(); 
        }

        //关闭
        public void Close()
        {
            if (!isUse)
                return;
            if (player != null)
            {
                //玩家退出时保存角色数据
                player.Logout();
                return;
            }
            Console.WriteLine("[断开连接]" + GetAdress());
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            isUse = false;
        }

        //发送协议
        public void Send(ProtocolBase protocol)
        {
            ServNet.instance.Send(this, protocol);
        }

    }//class
}//namespace
