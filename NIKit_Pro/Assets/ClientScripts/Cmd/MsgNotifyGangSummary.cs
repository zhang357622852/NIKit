using LPC;

/// <summary>
/// 服务器通知公会列表
/// </summary>
public class MsgNotifyGangSummary : MsgHandler
{
    public string GetName()
    {
        return "msg_notify_gang_summary";
    }

    public void Go(LPCValue para)
    {
    }
}