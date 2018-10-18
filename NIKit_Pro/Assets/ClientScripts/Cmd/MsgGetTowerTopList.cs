/// <summary>
/// MsgGetTowerTopList.cs
/// Created by lic 2017/08/31
/// 获取竞技场排行榜列表
/// </summary>

using LPC;

public class MsgGetTowerTopList : MsgHandler
{
    public string GetName()
    {
        return "msg_get_tower_top_list";
    }

    /// <summary>
    /// 入口
    /// </summary>
    /// <param name="para">Para.</param>
    public void Go(LPCValue para)
    {
        // 通知排行榜管理模块
        TowerMgr.UpdateTopList(para.AsMapping);

        // 抛出获取排行榜数据事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_GET_TOWER_TOP_LIST, null);
    }
}
