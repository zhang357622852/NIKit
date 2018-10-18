using LPC;

/// <summary>
/// 游戏维护
/// </summary>
public class MsgNotifyMaintain : MsgHandler
{
    public string GetName() { return "msg_notify_maintain"; }

    public void Go(LPCValue para)
    {
        LPCMapping data = para.AsMapping;

        // 开始维护
        MaintainMgr.StartMaintain(data.GetValue<int>("maintain_type"));
    }
}
