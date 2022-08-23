using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : PanelBase
{

    private InputField idInput;
    private InputField pwInput;
    private Button loginBtn;
    private Button regBtn;

    #region 生命周期
    //初始化
    public override void Init(params object[] args)
    {
        base.Init(args);
        skinPath = "LoginPanel";
        layer = PanelLayer.Panel;
    }

    public override void OnShowing()
    {
        base.OnShowing();
        Transform skinTrans = skin.transform;
        idInput = skinTrans.Find("IDInput").GetComponent<InputField>();
        pwInput = skinTrans.Find("PWInput").GetComponent<InputField>();
        loginBtn = skinTrans.Find("LoginBtn").GetComponent<Button>();
        regBtn = skinTrans.Find("RegBtn").GetComponent<Button>();

        loginBtn.onClick.AddListener(OnLoginClick);
        regBtn.onClick.AddListener(OnRegClick);
    }

    public void OnRegClick()
    {
        PanelMgr.instance.OpenPanel<RegPanel>("");
        Close();
    }

    public void OnLoginClick()
    {
        //用户名密码为空
        if (idInput.text == "" || pwInput.text == "")
        {
            Debug.Log("用户名密码不能为空！");
            return;
        }

        //如果尚未连接则发起连接
        if (NetMgr.srvConn.status != Connection.Status.Connected)
        {
            string host = "127.0.0.1";
            int port = 2222;
            NetMgr.srvConn.proto = new ProtocolBytes();
            NetMgr.srvConn.Connect(host, port);
        }
        //发送
        ProtocolBytes protocol = new ProtocolBytes();
        protocol.AddString("Login");
        protocol.AddString(idInput.text);
        protocol.AddString(pwInput.text);
        //发送Login协议，并注册OnLoginBack
        NetMgr.srvConn.Send(protocol, OnLoginBack);
    }

    public void OnLoginBack(ProtocolBase protocol)
    {
        ProtocolBytes proto = (ProtocolBytes)protocol;
        int start = 0;
        string protoName = proto.GetString(start, ref start);
        int ret = proto.GetInt(start, ref start);
        if (ret == 0)
        {
            Debug.Log("登录成功！");
            //开始游戏
            Walk.instance.StartGame(idInput.text);
            Close();
            PanelMgr.instance.canvas.SetActive(false);
        }//if
        else
        {
            Debug.Log("登录失败！");
        }
    }
    #endregion
    protected override void Close()
    {
        string name = this.GetType().ToString();
        PanelMgr.instance.ClosePanel(name);
    }
}