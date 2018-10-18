using LPC;

/// <summary>
/// 通知客户端状态改变
/// </summary>
public class MsgNotifyStatusUpdated : MsgHandler
{
    public string GetName()
    {
        return "msg_notify_status_updated";
    }

    public void Go(LPCValue para)
    {
    }
}
