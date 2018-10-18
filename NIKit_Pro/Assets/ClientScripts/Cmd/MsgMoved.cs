using LPC;

/// <summary>
/// 服务器通知客户端实体移动
/// </summary>
public class MsgMoved : MsgHandler
{
    public string GetName()
    {
        return "msg_moved";
    }

    public void Go(LPCValue para)
    {
    }
}
