using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSever.core
{
    //字节流协议模型
    public class ProtocolBytes:ProtocolBase
    {
        //传输的字节流
        public byte[] bytes;

        //解码器，将字节流转换为ProtocolBytes对象
        public override ProtocolBase Decode(byte[] readbuff, int start, int length)
        {
            ProtocolBytes protocol = new ProtocolBytes();
            protocol.bytes = new byte[length];
            Array.Copy(readbuff, start, protocol.bytes, 0, length);
            return protocol;
        }

        //编码器
        public override byte[] Encode()
        {
            return bytes;
        }

        //协议名称，获取协议的第一个字符串
        public override string GetName()
        {
            return GetString(0);
        }

        //描述，提取每一个字节并组装成字符串
        public override string GetDesc()
        {
            string str = "";
            if (bytes == null) return str;
            for(int i=0;i<bytes.Length;i++)
            {

                int b = (int)bytes[i];
                str += b.ToString() + " ";
            }
            return str;
        }

        //添加字符串
        public void AddString(string str)
        {
            Int32 len = str.Length;
            byte[] lenBytes = BitConverter.GetBytes(len);
            byte[] strBytes = System.Text.Encoding.UTF8.GetBytes(str);
            if (bytes == null)
                bytes = lenBytes.Concat(strBytes).ToArray();
            else
                bytes = bytes.Concat(lenBytes).Concat(strBytes).ToArray();
        }

        //从字节数组的start处开始读取字符串
        //ref关键字使参数按引用传递
        public string GetString(int start,ref int end)
        {
            if (bytes == null) return "";
            //如果start后的字节数小于4，则不能读取字符串的大小。
            if (bytes.Length < start + sizeof(Int32)) return "";
            Int32 strLen = BitConverter.ToInt32(bytes, start);
            //如果字节数小于字符串长度，那么一定是出错了
            if (bytes.Length < start + sizeof(Int32) + strLen) return "";

            string str = System.Text.Encoding.UTF8.GetString(bytes, start + sizeof(Int32), strLen);
            end = start + sizeof(Int32) + strLen;
            return str;
        }

        public string GetString(int start)
        {
            int end = 0;
            return GetString(start, ref end);
        }

        public void AddInt(int num)
        {
            byte[] numBytes = BitConverter.GetBytes(num);
            if (bytes == null)
                bytes = numBytes;
            else
                bytes = bytes.Concat(numBytes).ToArray();
        }


        public int GetInt(int start,ref int end)
        {
            if (bytes == null)
                return 0;
            if (bytes.Length < start + sizeof(Int32))
                return 0;
            end = start + sizeof(Int32);
            return BitConverter.ToInt32(bytes, start);
        }

        public int GetInt(int start)
        {
            int end = 0;
            return GetInt(start, ref end);
        }

        //添加和获取浮点数
        public void AddFloat(float num)
        {
            byte[] numBytes = BitConverter.GetBytes(num);
            if (bytes == null)
                bytes = numBytes;
            else
                bytes = bytes.Concat(numBytes).ToArray();
        }

        public float GetFloat(int start,ref int end)
        {
            if (bytes == null)
                return 0;
            if (bytes.Length < start + sizeof(float))
                return 0;
            end = start + sizeof(float);
            //将byte数组转换成浮点数
            return BitConverter.ToSingle(bytes, start);
        }

        public float Getfloat(int start)
        {
            int end = 0;
            return GetFloat(start, ref end); 
        }
    }//class
}//namespace
