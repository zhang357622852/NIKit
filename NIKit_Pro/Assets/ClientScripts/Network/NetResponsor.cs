using System;

/// <summary>
/// 网络响应者
/// </summary>
public interface NetResponsor
{        
    /// <summary>
    /// 连接请求者
    /// </summary>
    NetConnector GetConnector();
    void SetConnector(NetConnector connector);
    
    /// <summary>
    /// 连接成功的处理
    /// </summary>
    void OnConnectSuccess();
    
    /// <summary>
    /// 连接失败的处理
    /// </summary>
    void OnConnectFailure();
    
    /// <summary>
    /// 掉线的处理
    /// </summary>
    void OnDisconnect();

    /// <summary>
    /// 网络出现延迟.
    /// </summary>
    void OnDelay();

    /// <summary>
    /// 网络通畅.
    /// </summary>
    void OnUnobstructed();

    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="cmdNo"> 消息ID </param>
    /// <param name="message"> 消息体内容 </param>
    void OnRead(int cmdNo, byte[] message);
}
