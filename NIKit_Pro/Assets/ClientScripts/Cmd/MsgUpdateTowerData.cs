/// <summary>
/// MsgUpdateTowerData.cs
/// Created by zhaozy 2017/09/04
/// 通天塔数据更新
/// </summary>

using LPC;

/// <summary>
/// 购买金币
/// </summary>
public class MsgUpdateTowerData : MsgHandler
{
    public string GetName()
    {
        return "msg_update_tower_data";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
        // 缓存数据
        TowerMgr.RunTowerData = para.AsMapping.GetValue<LPCMapping>("tower_data");

        // 刷新排行榜和使魔排行榜
        TowerMgr.RefreshAllData();
    }
}
