using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

//消息分发
public class MsgDistribution
{

    //每帧处理消息的数量
    public int num = 15;
    //消息列表
    public List<ProtocolBase> msgList = new List<ProtocolBase>();
    //委托类型
    //委托是一个类，它定义了方法的类型，使得可以将方法当做另一个方法的参数来进行传递
    //声明一个delegate类型，它必须与要传递的方法具有相同的参数和返回值
    //例如：delegate void DelegateStr(string str)创建了一个名为DelegateStr的delegate 类型
    //创建delegate对象，并将要传递的方法作为参数传入
    //例如DelegateStr fun=new DelegateStr(printStr);创建名为fun的delegate对象
    //在适当的地方调用它
    //fun("Hello Lwj")
    public delegate void Delegate(ProtocolBase proto);
    //事件监听表
    private Dictionary<string, Delegate> eventDict = new Dictionary<string, Delegate>();
    private Dictionary<string, Delegate> onceDict = new Dictionary<string, Delegate>();


    //MsgDistribution类没有继承自Monobehaviour,Update方法不会每帧自动执行
    //在Connection类的Updata方法中调用
    public void Update()
    {
        for (int i = 0; i < num; i++)       ///每帧最多处理num条消息
        {
            if (msgList.Count > 0)
            {
                Debug.Log("当前消息列表长度"+msgList.Count);
                DispatchMsgEvent(msgList[0]);
                lock (msgList)
                {
                    msgList.RemoveAt(0);
                    Debug.Log("在消息列表中删除此条消息 ");
                }
            }//if
            else
            {
                break;
            }
        }//for

    }//Update

    //消息分发
    public void DispatchMsgEvent(ProtocolBase protocol)
    {
        string name = protocol.GetName();
        Debug.Log("分发处理消息 " + name);
        if (eventDict.ContainsKey(name))
        {
            Debug.Log("调用" + name+"对应的多次委托方法");
            eventDict[name](protocol);      //用委托调用对应方法
        }
        if (onceDict.ContainsKey(name))
        {
            Debug.Log("调用" + name + "对应的单次委托方法");
            onceDict[name](protocol);       //用委托调用对应方法
            onceDict[name] = null;
            onceDict.Remove(name);
        }
        Debug.Log("此消息分发处理完毕" + name);
    }//DispatchMsgEvent

    //添加监听事件
    public void AddListener(string name, Delegate cb)
    {
        //+=、-=是委托对象的一种运算符
        if (eventDict.ContainsKey(name))
            eventDict[name] += cb;
        else
            eventDict[name] = cb;
    }

    //添加单次监听事件
    public void AddOnceListener(string name, Delegate cb)
    {
        if (onceDict.ContainsKey(name))
            onceDict[name] += cb;
        else
            onceDict[name] = cb;
    }

    //删除监听事件
    public void DelListener(string name, Delegate cb)
    {
        if (eventDict.ContainsKey(name))
        {
            eventDict[name] -= cb;
            if (eventDict[name] == null)
                eventDict.Remove(name);
        }
    }

    //删除单次监听事件
    public void DelOnceListener(string name, Delegate cb)
    {
        if (onceDict.ContainsKey(name))
        {
            onceDict[name] -= cb;
            if (onceDict[name] == null)
                onceDict.Remove(name);
        }
    }
}//class
