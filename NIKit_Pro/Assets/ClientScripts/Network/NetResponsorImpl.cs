using System;
using UnityEngine;
using LPC;

/// <summary>
/// 实现网络响应者接口
/// </summary>
public class NetResponsorImpl : NetResponsor
{
    // 连接请求者
    private NetConnector connector = null;

    // 连接的目标是aaa还是gs
    private bool isAAA = false;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="isAAA">指明连接的目标是GS还是AAA</param>
    public NetResponsorImpl(bool isAAA)
    {
        this.isAAA = isAAA;
    }

    /// <summary>
    /// 连接请求者
    /// </summary>
    public NetConnector GetConnector()
    {
        return this.connector;
    }
    public void SetConnector(NetConnector connector)
    {
        this.connector = connector;
    }

    /// <summary>
    /// 连接成功的处理
    /// </summary>
    public void OnConnectSuccess()
    {
        // 派发消息到达的消息
        if (isAAA)
            this.connector.MsgArrival("connect_to_aaa");
        else
            this.connector.MsgArrival("connect_to_gs");
    }

    /// <summary>
    /// 连接失败的处理
    /// </summary>
    public void OnConnectFailure()
    {
        // 派发消息到达的消息
        if (isAAA)
            this.connector.MsgArrival("connect_to_aaa");
        else
            this.connector.MsgArrival("connect_to_gs");
    }

    /// <summary>
    /// 掉线的处理
    /// </summary>
    public void OnDisconnect()
    {
        if (! isAAA)
            MsgMgr.AddMsg("DISCONNECT", null, null);
    }

    /// <summary>
    /// 当前网络状态：延时
    /// </summary>
    public void OnDelay()
    {
        if (! isAAA)
            MsgMgr.AddMsg("NETDELAY", null, null);
    }

    /// <summary>
    /// 当前网络状态：可以发包
    /// </summary>
    public void OnUnobstructed()
    {
        if (! isAAA)
            MsgMgr.AddMsg("NETUNOBSTRUCTED", null, null);
    }

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="cmdNo"> 消息ID </param>
    /// <param name="message"> 消息体内容 </param>
    private byte[] buf, rawBuf, rawMessage;
    public void OnRead(int cmdNo, byte[] message)
    {
        if (cmdNo == 0xff01)
        {
            // echo指令，直接处理掉
            this.connector.SendRawToServer(0xff02, message);
            return;
        }

        if (MsgMgr.IsCompressedMsgNo(cmdNo))
        {
            // 这是经过压缩的指令，需要先解压缩
            LPCValue v = PktAnalyser.Unpack(cmdNo, message);

            // 解压缩还原
            buf = v.AsMapping ["buf"].AsBuffer;

            int offset = 0;
            rawBuf = buf;
            if (buf[0] == 0 && buf[1] == 0 && buf[2] == 0 && buf[3] == 0)
            {
                // 服务器压缩失败了，直接复制数据，忽略4个字节的长度
                offset = 4;
            }
            else
            {
                // 解压缩消息
                rawBuf = Zlib.Decompress(buf);

                // 消息解压缩失败
                if (rawBuf == null)
                    return;
            }

            // 拆消息
            int rawCmdNo = (int)LPCUtil._GET_U16(rawBuf, ref offset);

            rawMessage = NetUtil.ByteArraySkip(rawBuf, offset);

            // 处理消息
            ProcessPacket(rawCmdNo, rawMessage);
            return;
        }

        // 处理消息
        ProcessPacket(cmdNo, message);
    }

    // 处理数据包
    private void ProcessPacket(int cmdNo, byte[] message)
    {
        LPCValue v = null;

        try
        {
            // 数据解包
            v = PktAnalyser.Unpack(cmdNo, message);
        } catch (Exception)
        {
            // 解压数据失败了
            Debug.Log(string.Format("消息数据解包异常{0}", MsgMgr.MsgName(cmdNo)));
            return;
        }

        // 交给Unity主线程处理
        MsgMgr.AddMsg(cmdNo, v, OnMsgExecuted);
    }

    // 消息被unity3d处理后需要做的事情
    private void OnMsgExecuted(string cmdName)
    {
        if (this.connector != null)
        {
            this.connector.MsgArrival(cmdName);
        }
    }
}
