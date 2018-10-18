/// <summary>
/// MsgNotifyActivityList.cs
/// Created by fengsc 2017/03/21
/// 活动列表通知消息
/// </summary>
using LPC;

public class MsgNotifyActivityList : MsgHandler
{
    public string GetName()
    {
        return "msg_notify_activity_list";
    }

    /// <summary>
    /// 消息入口
    /// </summary>
    public void Go(LPCValue para)
    {
        LPCMapping data = para.AsMapping.GetValue<LPCMapping>("activity_list");
        if (data == null)
            return;

        ActivityMgr.ActivityMap = data;

        // 抛出活动列表事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_NOTIFY_ACTIVITY_LIST, null);
    }
}
