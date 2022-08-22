using GameSever.core;
using GameSever.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSever
{
    class MainClass
    {
        /*
        //测试DataMgr类
        public static void Main(string[] args)
        {
            DataMgr dataMgr = new DataMgr();
            //注册
            bool ret = dataMgr.Register("Lwj", "123");
            if (ret)
                Console.WriteLine("注册成功");
            else
                Console.WriteLine("注册失败");
            //创建角色
            ret = dataMgr.CreatePlayer("Lwj");
            if (ret)
                Console.WriteLine("创建角色成功");
            else
                Console.WriteLine("创建角色失败");
            //获取角色数据
            PlayerData pd = dataMgr.GetPlayerData("Lwj");
            if (pd != null)
                Console.WriteLine("获取角色数据成功 分数是 " + pd.score);
            else
                Console.WriteLine("获取角色数据失败");
            //更改玩家数据
            pd.score += 10;
            //保存数据
            Player p = new Player();
            p.id = "Lwj";
            p.data = pd;
            dataMgr.SavePlayer(p);
            //重新读取
            pd = dataMgr.GetPlayerData("Lwj");
            if (pd != null)
                Console.WriteLine("获取角色数据成功 分数是 " + pd.score);
            else
                Console.WriteLine("重新获取角色数据失败");
        }//Main1
        */

        //测试ServNet类
        public static void Main(string[] args)
        {
            DataMgr dataMgr = new DataMgr();
            ServNet servNet = new ServNet();
            servNet.proto = new ProtocolBytes();
            servNet.Start("127.0.0.1", 2222);

            Scene scene = new Scene();

            while(true)
            {
                string str = Console.ReadLine();
                switch(str)
                {
                    case "quit":
                        servNet.Close();
                        return;
                    case "print":
                        servNet.Print();
                        break;
                }//switch
            }//while


        }//Main

    }//class
}//namesapce
