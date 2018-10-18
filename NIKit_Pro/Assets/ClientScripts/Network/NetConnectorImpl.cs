using UnityEngine;
using System;
using System.Collections.Generic;
using LPC;
using System.Threading;
using LitJson;
using System.Collections;

/// <summary>
/// 网络连接请求者的实现 
/// </summary>
public class NetConnectorImpl : NetConnector
{
    // ip和域名的映射关系(ipv6)
    public static JsonData IpMapDomain = null;

    // 获取ip对应的域名
    public static string GetIpToDomain(string ip)
    {
        if (IpMapDomain == null)
            return ip;

        if (((IDictionary)IpMapDomain).Contains(ip))
            return IpMapDomain[ip].ToString();
        else
            return ip;
    }

    // 连接实例
    private NetConnect connect = new NetConnect();

    public NetConnectorImpl()
    {
        // 创建个线程不停的处理数据的读取
        Thread thread = new Thread(new ThreadStart(Poll));
        thread.Start();
    }

    /// <summary>
    /// 连接对象 
    /// </summary>
    public NetConnect GetConnect()
    {
        return this.connect;
    }

    public void SetConnect(NetConnect connect)
    {
        this.connect = connect;
    }

    /// <summary>
    /// 是否处于连接中 
    /// </summary>
    /// <returns>如果是连接中则返回true</returns>
    public bool IsConnected()
    {
        return this.connect.IsConnected;
    }

    /// <summary>
    /// 连接到服务器 
    /// </summary>
    /// <param name="ip_address">IP地址</param>
    /// <param name="port">端口号</param>
    public void ConnectToServer(string ip_address, int port)
    {
        this.connect.NonblockConnect(NetConnectorImpl.GetIpToDomain(ip_address), port);
    }

    /// <summary>
    /// 从服务器断开连接 
    /// </summary>
    public void DisconnectFromServer()
    {
        this.connect.Disconnect();
    }

    /// <summary>
    /// 发送原始数据包到服务器 
    /// </summary>
    /// <param name="msgNo"> 消息ID </param>
    /// <param name="data"> 消息体 </param>
    /// <returns>成功发送则返回true</returns>
    public bool SendRawToServer(int msgNo, byte[] data)
    {
        this.connect.SendPkt((ushort)msgNo, data);
        return true;
    }

    /// <summary>
    /// 发生数据到服务器
    /// </summary>
    /// <param name="msgNo"> 消息ID </param>
    /// <param name="param"> 消息体 </param>
    /// <returns>成功发送返回true</returns>
    public bool SendToServer(int msgNo, object param)
    {
        if (!IsConnected())
            return false;

        try
        {
            byte[] data = PktAnalyser.Pack(msgNo, (LPCValue)param);
            this.connect.SendPkt((ushort)msgNo, data);
            LogMgr.Network(msgNo, (LPCValue)param);
            return true;
        }
        catch (Exception e)
        {
            LogMgr.Error("打包数据失败： {0,4:X4}, {1}", msgNo, e.ToString());
            return false;
        }
    }

    /// <summary>
    /// 等待某消息到达 
    /// </summary>
    /// <param name="msg">等待的消息名称</param>
    /// <param name="waitTime">超时时间</param>
    /// <param name="success">成功的回调</param>
    /// <param name="fail">失败的回调</param>
    /// <returns>操作成功则返回true</returns>
    public bool WaitMsgArrival(string msg, float waitTime, CallBack success, CallBack fail)
    {
        // 统一为大写
        msg = msg.ToUpper();

        lock (waitList)
        {
            if (waitList.ContainsKey(msg))
            {
                LogMgr.Trace(string.Format("已经在等待中了，不能重复等待消息{0}", msg));
                return false;
            }

            // 记录起来
            WaitMsgNode node = new WaitMsgNode(waitTime, success, fail);
            waitList.Add(msg, node);
        }

        return true;
    }

    /// <summary>
    /// 消息到达的处理 
    /// </summary>
    /// <param name="msg">到达的消息</param>
    public void MsgArrival(string msg)
    {
        // 统一为大写
        msg = msg.ToUpper();

        lock (waitList)
        {
            // 无人等待此消息
            if (!waitList.ContainsKey(msg))
                return;
        }

        lock (arrivalList)
        {
            // 添加到列表中
            arrivalList.Add(msg);
        }
    }

    /// <summary>
    /// 取消消息等待
    /// </summary>
    /// <param name="msg"> 待取消的消息名称</param>
    public void RemoveWaitMsg(string msg)
    {
        // 统一为大写
        msg = msg.ToUpper();

        lock (waitList)
        {
            // 无人等待此消息
            if (!waitList.ContainsKey(msg))
                return;

            waitList.Remove(msg);
        }
    }

    /// <summary>
    /// 每帧需要调度，以对网络数据进行处理 
    /// </summary>
    private List<string> expireList = new List<string>();

    public void Update()
    {
        lock (this.arrivalList)
        {
            lock (this.waitList)
            {
                // 处理已经到达的消息
                foreach (string msg in this.arrivalList)
                {
                    if (string.IsNullOrEmpty(msg))
                        continue;

                    // 无人等待，直接忽略掉
                    if (!waitList.ContainsKey(msg))
                        continue;

                    WaitMsgNode node = this.waitList[msg];
                    waitList.Remove(msg);
                    node.OnSuccess(null);
                }

                this.arrivalList.Clear();
            }

            // 处理超时时间
            expireList.Clear();

            foreach (string msg in this.waitList.Keys)
            {
                WaitMsgNode node = this.waitList[msg];
                if (node.IsExpired())
                    expireList.Add(msg);
            }

            foreach (string msg in expireList)
            {
                LogMgr.Trace("消息{0}等待超时了。", msg);
                WaitMsgNode node = this.waitList[msg];
                this.waitList.Remove(msg);
                node.OnFail(null);
            }
        }
    }

    // 线程守护，不停的读取数据
    private void Poll()
    {
        while (true)
        {
            // 休眠下
            Thread.Sleep(20);

            // 如果尚未连接上，无视
            if (!IsConnected())
            {
                // 多休眠会儿
                Thread.Sleep(500);
                continue;
            }

            // 读取数据
            try
            {
                this.connect.Poll(0);
            }
            catch
            {
            }
        }
    }

    // 消息等待的结构
    class WaitMsgNode
    {
        private CallBack success;
        private CallBack fail;
        private float timeout;

        public WaitMsgNode(float timeout, CallBack success, CallBack fail)
        {
            this.timeout = timeout + Time.unscaledTime;
            this.success = success;
            this.fail = fail;
        }

        public bool IsExpired()
        {
            return timeout <= Time.unscaledTime;
        }

        public void OnSuccess(object param)
        {
            if (success != null)
                success.Go(param);
        }

        public void OnFail(object param/* = null*/)
        {
            if (fail != null)
                fail.Go(param);
        }
    }

    // 等待消息的列表
    private Dictionary<string, WaitMsgNode> waitList = new Dictionary<string, WaitMsgNode>();

    // 消息到达队列
    private List<string> arrivalList = new List<string>();
}
