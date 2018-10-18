using System;

/// <summary>
/// 连接请求者 
/// </summary>
public interface NetConnector
{
    /// <summary>
    /// 连接对象 
    /// </summary>
    NetConnect GetConnect();
    void SetConnect(NetConnect connect);

    /// <summary>
    /// 是否处于连接中 
    /// </summary>
    /// <returns>如果是连接中则返回true</returns>
    bool IsConnected();

    /// <summary>
    /// 连接到服务器 
    /// </summary>
    /// <param name="ip_address">IP地址</param>
    /// <param name="port">端口号</param>
    void ConnectToServer(string ip_address, int port);

    /// <summary>
    /// 从服务器断开连接 
    /// </summary>
    void DisconnectFromServer();

    /// <summary>
    /// 发送原始数据包到服务器 
    /// </summary>
    /// <param name="msgNo"> 消息ID </param>
    /// <param name="data"> 消息体 </param>
    /// <returns>成功发送则返回true</returns>
    bool SendRawToServer(int msgNo, byte[] data);

    /// <summary>
    /// 发生数据到服务器
    /// </summary>
    /// <param name="msgNo"> 消息ID </param>
    /// <param name="param"> 消息体 </param>
    /// <returns>成功发送返回true</returns>
    bool SendToServer(int msgNo, object param);

    /// <summary>
    /// 等待某消息到达 
    /// </summary>
    /// <param name="msg">等待的消息名称</param>
    /// <param name="waitTime">超时时间</param>
    /// <param name="success">成功的回调</param>
    /// <param name="fail">失败的回调</param>
    /// <returns>操作成功则返回true</returns>
    bool WaitMsgArrival(string msg, float waitTime, CallBack success, CallBack fail);

    /// <summary>
    /// 消息到达的处理 
    /// </summary>
    /// <param name="msg">到达的消息</param>
    void MsgArrival(string msg);

    /// <summary>
    /// 取消消息等待 
    /// </summary>
    /// <param name="msg"> 待取消的消息名称</param>
    void RemoveWaitMsg(string msg);

    /// <summary>
    /// 每帧需要调度，以对网络数据进行处理 
    /// </summary>
    void Update();
}
