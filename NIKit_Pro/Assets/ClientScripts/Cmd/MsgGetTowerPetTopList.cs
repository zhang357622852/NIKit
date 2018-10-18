/// <summary>
/// MsgGetTowerPetTopList.cs
/// Created by lic 2017/08/31
/// 获取通天塔使魔排行榜列表
/// </summary>

using LPC;

public class MsgGetTowerPetTopList : MsgHandler
{
    public string GetName()
    {
        return "msg_get_tower_pet_top_list";
    }

    /// <summary>
    /// 入口
    /// </summary>
    /// <param name="para">Para.</param>
    public void Go(LPCValue para)
    {
        // 通知排行榜管理模块
        TowerMgr.UpdatePetTopList(para.AsMapping);

        // 抛出获取排行榜数据事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_GET_TOWER_PET_TOP_LIST, null);
    }
}
