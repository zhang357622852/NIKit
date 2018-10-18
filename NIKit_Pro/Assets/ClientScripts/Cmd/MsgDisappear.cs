using LPC;

/// <summary>
/// 服务器通知客户端实体消失
/// </summary>
public class MsgDisappear : MsgHandler
{
    public string GetName()
    {
        return "msg_disappear";
    }

    public void Go(LPCValue para)
    {
    }
}
