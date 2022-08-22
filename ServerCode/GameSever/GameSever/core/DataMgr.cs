using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MySql.Data;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using GameSever.Logic;

namespace GameSever.core
{
    //单例模式
    public class DataMgr
    {
        MySqlConnection sqlConn;
        public static DataMgr instance;

        public DataMgr()
        {
            instance = this;
            Connect();
        }

        //连接数据库
        public void Connect()
        {
            string connStr = "Database=game;" +
                "DataSource=127.0.0.1;" +
                "User Id=root;" +
                "Password=618907;" +
                "port=3306";
            sqlConn = new MySqlConnection(connStr);
            try
            {
                sqlConn.Open();
            }
            catch(Exception e)
            {
                Console.Write("[DataMgr]Connect " + e.Message);
                return;
            }
        }//Connect

        //防止Sql注入，判断sql语句是否含有恶意字符
        public bool IsSafeStr(string str)
        {
            return !Regex.IsMatch(str, @"[-|;|,|\/|\(|\)|\[|\]|\}|\{|%|@|\*|!|\']");
        }

        //判断数据库中是否已经存在某用，存在则不能注册返回false，否则返回true
        private bool CanRegister(string id)
        {
            //防止SQl注入
            if(!IsSafeStr(id))
            {
                Console.Write("[DataMgr]CanREgister Sql语句不安全");
                return false;
            }

            //查询数据库中是否存在id
            string cmdStr = string.Format("select * from user where id='{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRows = dataReader.HasRows;
                dataReader.Close();
                return !hasRows;
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr]CanREgister Fail " + e.Message);
                return false;
            }
        }//CanRegister

        //注册
        public bool Register(string id,string pw)
        {
            //防止SQL注入
            if(!IsSafeStr(id)||!IsSafeStr(pw))
            {

                Console.WriteLine("[DataMgr]Register 使用非法字符");
                return false;
            }
            //能否注册
            if(!CanRegister(id))
            {
                Console.WriteLine("[DataMgr]Register !CanRegister");
                return false;
            }

            //写入数据库user表
            string cmdStr = string.Format("insert into user set id='{0}',pw='{1}';", id, pw);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr]Register" + e.Message);
                return false;
            }
        }//Register
    
        //创建角色
        public bool CreatePlayer(string id)
        {
            //防止SQL注入
            if(!IsSafeStr(id))
            {
                Console.Write("[DataMgr]CreatePlayer Sql语句不安全");
                return false;
            }

            //序列化
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            PlayerData playerData = new PlayerData();
            try
            {
                //先将playerData对象序列化为内存字节流 MemoryStream
                formatter.Serialize(stream, playerData);
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr]CreatePlayer 序列化" + e.Message);
                return false;
            }
            //再将字节流转换成byte数组
            byte[] byteArr = stream.ToArray();
            //写入数据库
            //@data代表参数名，程序会从cmd的参数表中找到名为@Data的参数并写道SQL语句中
            string cmdStr = string.Format("insert into player set id ='{0}',data=@data;", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            //给cmd添加一个名为@data的参数，类型为二进制数据Blob
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            //给@Data赋值
            cmd.Parameters[0].Value = byteArr;
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr]CreatePlayer 写入 " + e.Message);
                return false;
            }
        }//CreatePlayer

        //检验用户名和密码是否正确
        public bool CheckPassword(string id,string pw)
        {
            //防止SQL注入
            if(!IsSafeStr(id)||!IsSafeStr(pw))
            {
                Console.Write("[DataMgr]CheckPassword Sql语句不安全");
                return false;
            }
            //查询
            string cmdStr = string.Format("select * from user where id='{0}' and pw='{1}';", id, pw);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                bool hasRow = dataReader.HasRows;
                dataReader.Close();
                return hasRow;
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr]CheckPassword " + e.Message);
                return false;
            }
        }//CheckPassword
         
        //获取角色数据
        public PlayerData GetPlayerData(string id)
        {
            PlayerData playerData = null;
            //防止SQL注入
            if(!IsSafeStr(id))
            {
                Console.Write("[DataMgr]GetPlayerData Sql语句不安全");
                return playerData;
            }
            //查询
            string cmdStr = string.Format("select * from player where id='{0}';", id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            byte[] buffer = new byte[1];
            try
            {
                MySqlDataReader dataReader = cmd.ExecuteReader();
                if(!dataReader.HasRows)
                {
                    dataReader.Close();
                    return playerData;
                }
                dataReader.Read();
                //将缓冲区设置为null，只为获取数据长度
                //返回值为读取的实际字节数
                long len = dataReader.GetBytes(1,       //从0开始的列序号
                    0,                                  //字段中的索引，从其开始读取操作
                    null,                               //要将字节流读入的缓冲区buffer
                    0,                                  //buffer中写入操作开始位置索引
                    0);                                 //要复制到缓冲区的最大长度
                buffer = new byte[len];
                //将playerData的二进制数据保存到buffer中
                dataReader.GetBytes(1, 0, buffer, 0, (int)len);
                dataReader.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr]GetPlayerData 查询 " + e.Message);
                return playerData;
            }
            //反序列化
            MemoryStream stream = new MemoryStream(buffer);
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                playerData = (PlayerData)formatter.Deserialize(stream);
                return playerData;
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr]GetPlayerData 反序列化" + e.Message);
                return playerData;
            }
        }//GetPlayerData

        //保存角色数据
        public bool SavePlayer(Player player)
        {
            string id = player.id;
            PlayerData playerData = player.data;
            //序列化
            IFormatter formatter = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            try
            {
                formatter.Serialize(stream, playerData);
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr]SavePlayer 序列化 " + e.Message);
                return false;
            }
            byte[] byteArr = stream.ToArray();
            //写入数据库
            string formatStr = "update player set data=@data where id='{0}';";
            string cmdStr = string.Format(formatStr, player.id);
            MySqlCommand cmd = new MySqlCommand(cmdStr, sqlConn);
            cmd.Parameters.Add("@data", MySqlDbType.Blob);
            cmd.Parameters[0].Value = byteArr;
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("[DataMgr]SavePlayer 写入 " + e.Message);
                return false;
            }
        }//SavePlayer



    }//class
}//namespace
