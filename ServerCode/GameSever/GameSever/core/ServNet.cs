using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using MySql.Data;
using MySql.Data.MySqlClient;
using GameSever.Logic;
using System.Reflection;

namespace GameSever.core
{
    public class ServNet
    {

        MySqlConnection sqlConn;
        //监听套接字
        public Socket listenfd;
        //客户端连接
        public Conn[] conns;
        //最大连接数
        public int maxConn = 50;
        //单例 
        public static ServNet instance;

        //主定时器
        System.Timers.Timer timer = new System.Timers.Timer(1000);
        //心跳时间
        public long heartBeatTime = 180;

        //协议
        public ProtocolBase proto;

        //消息分发
        public HandleConnMsg handleConnMsg = new HandleConnMsg();
        public HandlePlayerMsg handlePlayerMsg = new HandlePlayerMsg();
        public HandlePlayerEvent handlePlayerEvent = new HandlePlayerEvent(); 


        public ServNet()
        {
            instance = this;
        }

        //获取连接池索引，返回负数表示获取失败
        public int NewIndex()
        {
            if (conns == null)
                return -1;
            for (int i = 0; i < conns.Length; i++)
            {
                if (conns[i] == null)
                {
                    conns[i] = new Conn();
                    return i;
                }
                else if (conns[i].isUse == false)
                {
                    return i;
                }
            }
            return -1;
        }

        //开启服务器
        public void Start(string host, int port)
        {
            //定时器
            timer.Elapsed += new System.Timers.ElapsedEventHandler(HandleMainTimer);
            timer.AutoReset = false;    //定时器只执行一次，在HandleMainTimer中再次调用timer.Start()使定时器不断执行
            timer.Enabled = true;

            //连接MySQL和选择数据库
            //数据库 
            string connStr = "Database=game;Data Source=127.0.0.1;User Id=root;Password=618907;port=3306";
            try
            {
                sqlConn = new MySqlConnection(connStr);
                sqlConn.Open();
            }
            catch (Exception e)
            {
                Console.Write("[数据库]连接失败" + e.Message);
                return;
            }
            //连接池
            conns = new Conn[maxConn];
            for (int i = 0; i < maxConn; i++)
            {
                conns[i] = new Conn();
            }
            //Socket
            listenfd = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Bind
            IPAddress ipAdr = IPAddress.Parse(host);
            IPEndPoint ipEp = new IPEndPoint(ipAdr, port);
            listenfd.Bind(ipEp);
            //Listen
            listenfd.Listen(maxConn);       //参数指定队列中最多可容纳的连接数，0表示不限制
            //Accept
            listenfd.BeginAccept(AcceptCb, null);
            Console.WriteLine("[服务器]启动成功");

        }

        //主定时器
        public void HandleMainTimer(object sender,System.Timers.ElapsedEventArgs e)
        {
            //处理心跳
            HeartBeat();
            timer.Start();
        }
        //心跳
        public void HeartBeat()
        {                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
            long timeNow = Sys.GetTimeStamp();

            for(int i=0;i<conns.Length;i++)
            {
                Conn conn = conns[i];
                if (conn == null) continue;
                if (!conn.isUse) continue;
                if(conn.lastTickTime<timeNow-heartBeatTime)
                {
                    Console.WriteLine("[心跳引起断开连接]" + conn.GetAdress());
                    lock (conn)
                        conn.Close();
                }
                else
                {
                    Console.WriteLine("[收到来自" + conn.GetAdress()+ "的HeartBeat]");
                }
            }//for
        }//HeartBeat

        //Accept回调
        private void AcceptCb(IAsyncResult ar)
        {
            try
            {
                Socket socket = listenfd.EndAccept(ar); //获取新客户端的套接字
                int index = NewIndex();     //获取可用连接的下标
                if (index < 0)
                {
                    socket.Close();
                    Console.Write("[警告]连接已满");
                }
                else
                {
                    Conn conn = conns[index];
                    conn.Init(socket);
                    string adr = conn.GetAdress();      //获取客户端地址
                    Console.WriteLine("客户端连接[" + adr + "]conn池ID:" + index);
                    //BeginReceive异步数据接收
                    conn.socket.BeginReceive(conn.readBuff,         //buffer：Byte类型的数组，用于存储接收到的数据
                                             conn.buffCount,        //buffer中存储数据的位置，从零开始计数
                                             conn.BuffRemain(),     //最多接收的字节数
                                             SocketFlags.None,      //SocketFlags值的按位组合？
                                             ReceiveCb,             //回调函数，一个AsyncCallback委托
                                             conn);                 //一个用户定义的对象，其中包含接收操作的相关消息,  
                                                                    //当操作完成时，此对象会被传递给EndReceive委托
                }
                listenfd.BeginAccept(AcceptCb, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("AcceptCb失败:" + e.Message);
            }

        }

        //BeginReceive回调
        private void ReceiveCb(IAsyncResult ar)
        {
            Conn conn = (Conn)ar.AsyncState;
            lock(conn)
            {
                try
                {
                    int count = conn.socket.EndReceive(ar);
                    //关闭信号
                    if (count >= 0)
                        conn.buffCount += count;
                    ProcessData(conn);
                    //继续接收
                    conn.socket.BeginReceive(conn.readBuff, conn.buffCount,
                        conn.BuffRemain(), SocketFlags.None, ReceiveCb, conn);
                }
                catch (Exception e)
                {
                    Console.WriteLine("收到[" + conn.GetAdress() + "]断开连接");
                    conn.Close();
                }
            }
        }//ReceiveCb

        //粘包分包处理：处理接受到的消息，如果是一条完整的消息则读取和处理这条消息，否则暂不处理
        private void ProcessData(Conn conn)
        {
            //小于长度字节
            if (conn.buffCount < sizeof(Int32))
                return;
            //消息长度
            Array.Copy(conn.readBuff, conn.lenBytes, sizeof(Int32));
            conn.msgLength = BitConverter.ToInt32(conn.lenBytes, 0);
            if (conn.buffCount < conn.msgLength + sizeof(Int32))
                return;
            //处理消息
            //string str = System.Text.Encoding.UTF8.GetString(conn.readBuff, sizeof(Int32), conn.msgLength);
            //Console.WriteLine("收到消息 [" + conn.GetAdress() + "] " + str);
            //if (str == "HeartBeat")
            //    conn.lastTickTime = Sys.GetTimeStamp();
            //Send(conn, str);
            ProtocolBase protocol = proto.Decode(conn.readBuff, sizeof(Int32), conn.msgLength);
            HandleMsg(conn, protocol);
            //清除已处理的消息
            int count = conn.buffCount - conn.msgLength - sizeof(Int32);
            Array.Copy(conn.readBuff, sizeof(Int32) + conn.msgLength, conn.readBuff, 0, count);
            conn.buffCount = count;
            if(conn.buffCount>0)
            {
                ProcessData(conn);
            }

        }//ProcessData

        private void HandleMsg(Conn conn,ProtocolBase protoBase)
        {
            string name = protoBase.GetName();
            string methodName = "Msg" + name;
            //连接协议分发
            if(conn.player==null||name=="HeartBeat"||name=="Logout")
            {
                MethodInfo mm = handleConnMsg.GetType().GetMethod(methodName);
                if (mm == null)
                {
                    string str = "[警告]HandleMsg没有处理连接的方法";
                    Console.WriteLine(str + methodName);
                    return;
                }//if
                Object[] obj = new Object[] { conn, protoBase };
                Console.WriteLine("[处理连接消息]" + conn.GetAdress() + ":" + name);
                mm.Invoke(handleConnMsg, obj);
            }//if
            //角色协议分发
            else
            {
                MethodInfo mm = handlePlayerMsg.GetType().GetMethod(methodName);
                if(mm==null)
                {
                    string str = "[警告]HandleMsg没有处理角色的方法";
                    Console.WriteLine(str + methodName);
                    return;
                }//if
                Object[] obj = new Object[] { conn.player, protoBase };
                Console.WriteLine("[处理角色的消息]"+conn.player.id+":"+name);
                mm.Invoke(handlePlayerMsg, obj);
            }//else


            //Console.WriteLine("[收到协议]" + name);
            ////处理心跳
            //if (name == "HeartBeat")
            //{
            //    Console.WriteLine("[更新心跳时间]" + conn.GetAdress());
            //    conn.lastTickTime = Sys.GetTimeStamp();
            //}
            ////回复
            //Send(conn, protoBase);
        }//HandleMsg

        //组装消息长度和消息内容一起发送
        public void Send(Conn conn,ProtocolBase protocol)
        {
            //Console.WriteLine("发送给客户端协议 "+protocol.GetDesc());
            byte[] bytes = protocol.Encode();     //消息内容
            byte[] length = BitConverter.GetBytes(bytes.Length);        //消息长度
            byte[] sendbuff = length.Concat(bytes).ToArray();           //组装


            string str = "";
            if(sendbuff!=null)
            for (int i = 0; i < sendbuff.Length; i++)
            {

                int b = (int)sendbuff[i];
                str += b.ToString() + " ";
            }

            try
            {
                Console.WriteLine("发送给用户"+conn.player.id+"消息 " + str);
                conn.socket.BeginSend(sendbuff, 0, sendbuff.Length, SocketFlags.None, null, null);
            }
            catch(Exception e)
            {
                Console.WriteLine("[发送给用户" + conn.player.id + "消息失败]" + conn.GetAdress() + ":" + e.Message);
            }
        }

        //广播
        public void Broadcast(ProtocolBase protocol)
        {
            for (int i = 0; i < conns.Length; i++)
            {
                if (!conns[i].isUse) continue;
                if (conns[i].player == null) continue;
                Send(conns[i], protocol);
            }
        }

        //为了查看当前服务端的玩家数量，遍历连接池，打印连接信息
        public void Print()
        {
            Console.WriteLine("===服务器登录信息===");
            for(int i=0;i<conns.Length;i++)
            {
                if (conns[i] == null) continue;
                if (!conns[i].isUse) continue;
                string str = "连接[" + conns[i].GetAdress() + "] ";
                if (conns[i].player != null)
                    str += "玩家id " + conns[i].player.id;
                Console.WriteLine(str);
            }
        }//Print

        //关闭
        public void Close()
        {
            for(int i=0;i<conns.Length;i++)
            {
                Conn conn = conns[i];
                if (conn == null) continue;
                if (!conn.isUse) continue;
                //使用lock避免线程竞争
                lock(conn)
                {
                    conn.Close();
                }
            }
        }
    }
}
