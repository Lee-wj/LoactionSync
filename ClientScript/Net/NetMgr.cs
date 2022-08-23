using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//网络管理
//没有继承MonoBehaviour，在Root的Update方法中调用
public class NetMgr
{

    public static Connection srvConn = new Connection();

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    public static void Update()
    {

        srvConn.Update();
    }

    //心跳
    public static ProtocolBase GetHeartBeatProtocol()
    {
        //具体的发送内容根据服务端设定进行改动
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("HeartBeat");
        return protocol;
    }
}
