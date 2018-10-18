/// <summary>
/// Synthesis.cs
/// Create by lic 2016/11/16
/// 装备强化模块
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class Synthesis : Blacksmith
{
    public override bool DoAction(params object[] args)
    {
        LPCMapping para = args[1] as LPCMapping;
        Operation.CmdBlacksmithAction.Go("synthesis", para);

        return true;
    }

    public override bool DoActionResult(params object[] args)
    {
        // 获取对象
        Property who = args[0] as Property;

        if(who == null)
            return false;

        LPCValue extra_data = args[1] as LPCValue;

        bool result;

        // int值表示操作失败
        if (extra_data.IsInt)
            result = false;
        else
            result = true;

        // 抛出装备强化成功事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_SYNTHESIS, MixedValue.NewMixedValue<bool>(result));

        return true;
    }
}
