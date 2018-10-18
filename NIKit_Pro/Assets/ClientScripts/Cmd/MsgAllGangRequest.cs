using LPC;
using System.Collections.Generic;

/// <summary>
/// 服务器通知公会请求列表
/// </summary>
public class MsgAllGangRequest : MsgHandler
{
    public string GetName()
    {
        return "msg_all_gang_request";
    }

    public void Go(LPCValue para)
    {
        // 更新请求列表
        GangMgr.AllRequestList = para.AsMapping.GetValue<LPCArray>("request_list");

        // 抛出获取所有请求数据事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_ALL_GANG_REQUEST, null);
    }
}