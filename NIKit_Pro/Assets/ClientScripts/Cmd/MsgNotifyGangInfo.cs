using LPC;

/// <summary>
/// 帮派相关操作执行完毕
/// </summary>
public class MsgNotifyGangInfo : MsgHandler
{
    public string GetName() { return "msg_notify_gang_info"; }

    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping.GetValue<LPCMapping>("gang_info");

        // 还没有初始化过数据
        if (GangMgr.GangDetail.Count == 0)
            GangMgr.GangDetail = args;
        else
        {
            // relation_tag不一致, 直接替换数据
            if (! string.Equals(args.GetValue<string>("relation_tag"), GangMgr.GangDetail.GetValue<string>("relation_tag")))
                GangMgr.GangDetail = args;
            else
                GangMgr.GangDetail.Append(args);
        }

        // 抛出事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_NOTIFY_GANG_INFO, null);
    }
}