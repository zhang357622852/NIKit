using LPC;

/// <summary>
/// 服务器消息处理器的接口 
/// </summary>
public interface MsgHandler
{
    /// <summary>
    /// 处理器的名字 
    /// </summary>
    string GetName();
    
    /// <summary>
    /// 入口 
    /// </summary>
    void Go(LPCValue para);
}
