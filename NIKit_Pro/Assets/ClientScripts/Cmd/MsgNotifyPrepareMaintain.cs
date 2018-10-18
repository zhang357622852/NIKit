using LPC;

/// <summary>
/// 游戏即将维护
/// </summary>
public class MsgNotifyPrepareMaintain : MsgHandler
{
    public string GetName()
    {
        return "msg_notify_prepare_maintain";
    }

    public void Go(LPCValue para)
    {
        LPCMapping data = para.AsMapping;
        MaintainMgr.NoticePrepareMaintain(data.GetValue<int>("left_time"));
    }
}
