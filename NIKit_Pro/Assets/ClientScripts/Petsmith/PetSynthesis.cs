/// <summary>
/// PetSynthesis.cs
/// Create by lic 2017/02/13
/// 装备强化模块
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class PetSynthesis : Petsmith
{
    public override bool DoAction(params object[] args)
    {
        Property who = args[0] as Property;
        LPCMapping para = args[1] as LPCMapping;

        if (who == null)
            return false;

        // 发送消息
        Operation.CmdPetsmithAction.Go("synthesis", para);

        return true;
    }

    public override bool DoActionResult(params object[] args)
    {
        // 获取服务器下发的参数
        LPCMapping para = args[1] as LPCMapping;

        LPCMapping map = new LPCMapping ();
        map.Add("rid", para.GetValue<string>("rid"));

        // 抛出宠物升级事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_PET_SYNTHESIS, MixedValue.NewMixedValue<LPCMapping>(map));

        return true;
    }
}
