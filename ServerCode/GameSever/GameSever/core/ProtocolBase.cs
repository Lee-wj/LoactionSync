using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameSever.core
{
    public class ProtocolBase
    {
        //解码器，解码readbuff中从start开始的length字节
        public virtual ProtocolBase Decode(byte[] readbuff,int start,int length)
        {
            return new ProtocolBase();
        }

        //编码器
        public virtual byte[] Encode()
        {
            return new byte[] { };
        }

        //协议名称，用于消息分发，把不同协议名称的协议交给不同的函数处理
        public virtual string GetName()
        {
            return "";
        }

        //描述,用于印输出
        public virtual string GetDesc()
        {
            return "";
        }
    }//class
}//namespace
