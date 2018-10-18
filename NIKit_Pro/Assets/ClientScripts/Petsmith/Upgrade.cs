/// <summary>
/// Upgrade.cs
/// Create by lic 2016/09/8
/// 宠物升级模块
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class Upgrade : Petsmith
{
    public override bool DoAction(params object[] args)
    {
        Property who = args [0] as Property;
        LPCMapping para = args[1] as LPCMapping;

        if(who == null)
            return false;

        // 发送消息
        Operation.CmdPetsmithAction.Go("upgrade", para);

        return true;
    }

    public override bool DoActionResult(params object[] args)
    {
        // 获取对象
        Property who = args[0] as Property;

        if(who == null)
            return false;

        // 获取服务器下发的参数
        LPCMapping para = args[1] as LPCMapping;

        if(para == null)
            return false;

        // 获取经验加成系数
        float add_factor = para.GetValue<float>("add_factor");
        string rid = para.GetValue<string>("rid");
        LPCArray skillList = para.GetValue<LPCArray>("level_up_skills");

        // 构建参数
        LPCMapping map = new LPCMapping ();
        map.Add("add_factor", add_factor);
        map.Add("rid", rid);
        map.Add("level_up_skills", skillList);

        // 抛出宠物升级事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_PET_UPGRADE, MixedValue.NewMixedValue<LPCMapping>(map));

        return true;
    }
}
