using LPC;

/// <summary>
/// 服务器在处理客户端的登录请求以后返回的消息
/// </summary>
public class MsgRelayToServer : MsgHandler
{
    public string GetName()
    {
        return "msg_relay_to_server";
    }

    /// <summary>
    /// 消息入口
    /// </summary>
    /// <param name="para">Para.</param>
    public void Go(LPCValue para)
    {
        LogMgr.Trace("转接：{0} 提示：{1}", para.AsMapping ["succ"], para.AsMapping ["msg"]);
    }
}