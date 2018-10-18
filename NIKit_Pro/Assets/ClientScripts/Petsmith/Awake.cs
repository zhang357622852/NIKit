/// <summary>
/// 宠物觉醒
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class Awake : Petsmith
{
    public override bool DoAction(params object[] args)
    {
        Property who = args[0] as Property;
        LPCMapping para = args[1] as LPCMapping;

        if (who == null)
            return false;

        // 发送消息
        Operation.CmdPetsmithAction.Go("awake", para);

        return true;
    }

    /// <summary>
    /// Dos the action result.
    /// </summary>
    public override bool DoActionResult(params object[] args)
    {
        // 获取服务器下发的参数
        LPCMapping para = args[1] as LPCMapping;

        LPCMapping map = new LPCMapping ();
        map.Add("rid", para.GetValue<string>("rid"));

        // 获取宠物对象
        Property ob = Rid.FindObjectByRid(para.GetValue<string>("rid"));
        if (ob != null)
        {
            MonsterMgr.AutoDistributeAttrib(ob);
            PropMgr.RefreshAffect(ob);
        }

        // 抛出宠物升级事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_PET_AWAKE, MixedValue.NewMixedValue<LPCMapping>(map));

        return true;
    }
}
