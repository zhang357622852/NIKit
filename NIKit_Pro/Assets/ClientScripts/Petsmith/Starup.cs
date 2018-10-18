/// <summary>
/// Upgrade.cs
/// Create by lic 2016/09/8
/// 宠物升级模块
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class Starup : Petsmith
{
    public override bool DoAction(params object[] args)
    {
        Property who = args [0] as Property;
        LPCMapping para = args[1] as LPCMapping;

        if(who == null)
            return false;

        // 发送消息
        Operation.CmdPetsmithAction.Go("starup", para);

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

        LPCArray skillList = para.GetValue<LPCArray>("level_up_skills");
        string rid = para.GetValue<string>("rid");

        // 构建参数
        LPCMapping map = new LPCMapping ();
        map.Add("rid", rid);
        map.Add("level_up_skills", skillList);

        // 抛出宠物升级事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_PET_STARUP, MixedValue.NewMixedValue<LPCMapping>(map));

        if(!string.IsNullOrEmpty(rid))
        {
            Property ob = Rid.FindObjectByRid(rid);
            if(ob != null)
            {
                MonsterMgr.AutoDistributeAttrib(ob);
                PropMgr.RefreshAffect(ob);
            }
        }
        return true;
    }
}
