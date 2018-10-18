/// <summary>
/// Created bu fengsc 2016/08/05
/// 副本复活
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class MsgRevivePet : MsgHandler
{
    public string GetName()
    {
        return "msg_revive_pet";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
        // 服务器下发复活消息后将宠物复活;
        InstanceMgr.RevivePet(ME.user, CampConst.CAMP_TYPE_ATTACK);

        // 累计该副本玩家复活次数
        InstanceMgr.AddReviveTimes(ME.user);
    }
}
