/// <summary>
/// MsgGetArenaTopList.cs
/// Created by cql 2015/03/25
/// 获取竞技场排行榜列表
/// </summary>
using LPC;

public class MsgGetArenaTopList : MsgHandler
{
    public string GetName()
    {
        return "msg_get_arena_top_list";
    }

    /// <summary>
    /// 入口
    /// </summary>
    /// <param name="para">Para.</param>
    public void Go(LPCValue para)
    {
        // 通知排行榜管理模块
        ArenaMgr.UpdateTopList(para.AsMapping);

        // 抛出获取排行榜数据事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_GET_ARENA_TOP_LIST, null);
    }
}
