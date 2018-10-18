using LPC;
using System.Diagnostics;

/// <summary>
/// 服务器通知排行榜更新
/// </summary>
public class MsgTowerPetTopListUpdated : MsgHandler
{
    public string GetName()
    {
        return "msg_tower_pet_top_list_updated";
    }

    public void Go(LPCValue para)
    {
        // 通知通天塔管理模块使魔排行榜更新了
        TowerMgr.SetPetTopListDirty(para.AsMapping);
    }
}
