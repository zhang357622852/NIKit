using LPC;

/// <summary>
/// 服务器消息处理器的接口 
/// </summary>
public interface CmdHandler
{
    /// <summary>
    /// 处理器的名字 
    /// </summary>
    string GetName();
}
