using LPC;

/// <summary>
/// 帮派成员列表
/// </summary>
public class MsgNotifyGangMemberList : MsgHandler
{
    public string GetName() { return "msg_notify_gang_member_list"; }

    public void Go(LPCValue para)
    {
        GangMgr.GangMemberList = para.AsMapping.GetValue<LPCArray>("member_info");

        // 抛出事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_GET_GANG_MEMBER_LIST, null);
    }
}
