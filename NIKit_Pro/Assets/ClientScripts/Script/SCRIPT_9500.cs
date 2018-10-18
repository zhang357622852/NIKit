/// <summary>
/// SCRIPT_9500.cs
/// Create by wangxw 2015-01-08
/// 战斗表现脚本：收集脚本和其他
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using LPC;
using UnityEngine;

// 通用全体目标收集脚本
public class SCRIPT_9500 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        LPCMapping actionArgs = _params[2] as LPCMapping;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        // 整理筛选结果，将手选结果放在第一个
        List<Property> finalList = RoundCombatMgr.GetPropertyList(pickOb.CampId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" },
            new List<string>(){ actionArgs.GetValue<string>("pick_rid") });

        // 返回目标列表
        return finalList;
    }
}

/// <summary>
/// 自身目标收集脚本（自动选择）
/// </summary>
public class SCRIPT_9501 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg

        // 收集拾取目标
        Property sourceOb = _params[0] as Property;

        // 返回目标列表
        return new List<Property>() { sourceOb };
    }
}

/// <summary>
/// 通用单体目标收集脚本（手动选择的）
/// </summary>
public class SCRIPT_9502 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg

        // 收集拾取目标
        LPCMapping actionArgs = _params[2] as LPCMapping;
        string targetRid = actionArgs.GetValue<string>("pick_rid");
        if (string.IsNullOrEmpty(targetRid))
            return new List<Property>();

        // 查找目标对象
        Property targetOb = Rid.FindObjectByRid(targetRid);
        if (targetOb == null)
            return new List<Property>();

        // 返回目标列表
        return new List<Property>() { targetOb };
    }
}

/// <summary>
/// 通用单体目标收集脚本（随机选择）
/// </summary>
public class SCRIPT_9503 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        LPCMapping actionArgs = _params[2] as LPCMapping;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList(pickOb.CampId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED") && targetOb.QueryTemp<int>("halo/halo_can_not_choose") == 0)
                finalList.Add(targetOb);
        }

        if (finalList.Count == 0)
            return finalList;

        // 按照随机种子随机出目标，返回目标列表
        return new List<Property>(){ finalList[RandomMgr.GetRandom(finalList.Count)] };
    }
}

// 己方单体目标收集脚本（当前生命最低的）
public class SCRIPT_9504 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;

        // 收集本阵营所有对象（不管死活）
        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);
        List<Property> selectOb = new List<Property>();
        int maxHpRate = ConstantValue.MAX_VALUE;

        // 遍历各个角色
        foreach (Property targetOb in targetList)
        {
            // 角色已经死亡
            if (targetOb.CheckStatus("DIED"))
                continue;

            // 如果HpRate较大不处理
            int targetHpRate = Game.Divided(targetOb.Query<int>("hp"), targetOb.QueryAttrib("max_hp"));
            if (targetHpRate > maxHpRate)
                continue;

            // 如果hp rate相同
            if (targetHpRate == maxHpRate)
            {
                selectOb.Add(targetOb);
                continue;
            }

            // targetHpRate较小重新记录数据
            maxHpRate = targetHpRate;
            selectOb.Clear();
            selectOb.Add(targetOb);
        }

        // 没有选择到目标
        if (selectOb.Count == 0)
            return new List<Property>();

        // 返回目标列表
        return new List<Property>() { selectOb[RandomMgr.GetRandom(selectOb.Count)] };
    }
}

// 己方全体目标收集脚本
public class SCRIPT_9505 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;
        //LPCMapping actionArgs = _params[2] as LPCMapping;
        //Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED"))
                finalList.Add(targetOb);
        }

        // 返回目标列表
        return finalList;
    }
}

// 己方所有已死亡单位的收集脚本
public class SCRIPT_9506 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, sourceProfile, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;

        // 收集本阵营所有对象（不管死活）
        List<Property> allList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        List<Property> targetList = new List<Property>();

        foreach (Property targetOb in allList)
        {
            if (targetOb.CheckStatus("DIED") && !string.Equals(sourceOb.GetRid(),targetOb.GetRid()))
                targetList.Add(targetOb);
        }

        // 返回目标列表
        return targetList;
    }
}

/// <summary>
/// 通用己方首领目标收集脚本
/// </summary>
public class SCRIPT_9507 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;

        // 收集本阵营所有对象（不管死活）
        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);
        List<Property> selectOb = new List<Property>();

        // 遍历各个角色
        foreach (Property targetOb in targetList)
        {
            // 角色已经死亡
            if (targetOb.CheckStatus("DIED") || targetOb.Query<int>("is_boss") != 1)
                continue;

            selectOb.Add(targetOb);
        }

        // 没有选择到目标
        if (selectOb.Count == 0)
            return new List<Property>(){ sourceOb };

        // 返回目标列表
        return new List<Property>() { selectOb[RandomMgr.GetRandom(selectOb.Count)] };
    }
}

// 敌方单体目标收集脚本（当前生命比率最高的）
public class SCRIPT_9508 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        LPCMapping actionArgs = _params[2] as LPCMapping;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList(pickOb.CampId);
        List<Property> selectOb = new List<Property>();
        int maxHpRate = -1;

        // 遍历各个角色
        foreach (Property targetOb in targetList)
        {
            // 角色已经死亡
            if (targetOb.CheckStatus("DIED") || targetOb.QueryTemp<int>("halo/halo_can_not_choose") > 0)
                continue;

            // 如果HpRate较大不处理
            int targetHpRate = Game.Divided(targetOb.Query<int>("hp"), targetOb.QueryAttrib("max_hp"));
            if (targetHpRate < maxHpRate)
                continue;

            // 如果hp rate相同
            if (targetHpRate == maxHpRate)
            {
                selectOb.Add(targetOb);
                continue;
            }

            // targetHpRate较小重新记录数据
            maxHpRate = targetHpRate;
            selectOb.Clear();
            selectOb.Add(targetOb);
        }

        // 没有选择到目标
        if (selectOb.Count == 0)
            return new List<Property>();

        // 返回目标列表
        return new List<Property>() { selectOb[RandomMgr.GetRandom(selectOb.Count)] };
    }
}

/// <summary>
/// 通用单体目标收集脚本（存在技能依赖的目标收集）
/// </summary>
public class SCRIPT_9509 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg, hitEntityMap
        // 获取上一个节点的目标
        LPCMapping hitEntityMap = _params[5] as LPCMapping;

        List<Property> entityList = new List<Property>();

        foreach (LPCValue rid in hitEntityMap[0].AsArray.Values)
            entityList.Add(Rid.FindObjectByRid(rid.AsString));

        // 返回目标列表
        return entityList;
    }
}

// 通用敌方全体目标收集脚本
public class SCRIPT_9510 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        //LPCMapping actionArgs = _params[2] as LPCMapping;
        Property sourceOb = _params[0] as Property;

        int campId = (sourceOb.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;

        // 整理筛选结果，将手选结果放在第一个
        List<Property> finalList = RoundCombatMgr.GetPropertyList(campId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" });

        List<Property> petList = new List<Property>();

        foreach (Property checkOb in finalList)
        {
            if (checkOb.QueryTemp<int>("halo/halo_can_not_choose") > 0)
                continue;
            petList.Add(checkOb);
        }

        // 返回目标列表
        return petList;
    }
}

// 通用敌方前排目标收集脚本(前排收集不到收集后排)
public class SCRIPT_9511 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        LPCMapping actionArgs = _params[2] as LPCMapping;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        // 整理筛选结果，将手选结果放在第一个
        List<Property> petList = new List<Property>();
        List<Property> finalList = RoundCombatMgr.GetPropertyList(pickOb.CampId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" },
            new List<string>(){ actionArgs.GetValue<string>("pick_rid") });

        // 筛选前后排信息
        foreach (Property targetOb in finalList)
        {
            if (targetOb.FormationRaw == FormationConst.RAW_FRONT && targetOb.QueryTemp<int>("halo/halo_can_not_choose") == 0)
                petList.Add(targetOb);
        }

        // 返回目标列表
        if (petList.Count == 0)
            return finalList;

        return petList;
    }
}

// 通用敌方后排目标收集脚本（后排收集不到收集前排）
public class SCRIPT_9512 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        LPCMapping actionArgs = _params[2] as LPCMapping;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        // 整理筛选结果，将手选结果放在第一个
        List<Property> petList = new List<Property>();
        List<Property> finalList = RoundCombatMgr.GetPropertyList(pickOb.CampId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" },
            new List<string>(){ actionArgs.GetValue<string>("pick_rid") });

        // 筛选前后排信息
        foreach (Property targetOb in finalList)
        {
            if (targetOb.FormationRaw != FormationConst.RAW_BACK && targetOb.QueryTemp<int>("halo/halo_can_not_choose") == 0)
                petList.Add(targetOb);
        }

        // 返回目标列表
        if (petList.Count == 0)
            return finalList;

        return petList;
    }
}

// 通用敌方前排随机单个目标收集脚本(前排收集不到收集后排)
public class SCRIPT_9513 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;
        LPCMapping actionArgs = _params[2] as LPCMapping;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        if (pickOb != null && pickOb.FormationRaw == FormationConst.RAW_FRONT)
            return new List<Property>(){ pickOb };

        int campId = (sourceOb.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;

        // 整理筛选结果，将手选结果放在第一个
        List<Property> petList = new List<Property>();
        List<Property> finalList = RoundCombatMgr.GetPropertyList(campId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" },
            new List<string>(){ actionArgs.GetValue<string>("pick_rid") });

        // 筛选前后排信息
        foreach (Property targetOb in finalList)
        {
            if (targetOb.FormationRaw == FormationConst.RAW_FRONT && targetOb.QueryTemp<int>("halo/halo_can_not_choose") == 0)
                petList.Add(targetOb);
        }

        // 返回目标列表
        if (petList.Count == 0)
            return new List<Property>(){ finalList[RandomMgr.GetRandom(finalList.Count)] };

        return new List<Property>(){ petList[RandomMgr.GetRandom(petList.Count)] };
    }
}

// 通用敌方后排随机单个目标收集脚本(后排收集不到收集前排)
public class SCRIPT_9514 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;
        LPCMapping actionArgs = _params[2] as LPCMapping;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        if (pickOb != null && pickOb.FormationRaw == FormationConst.RAW_BACK)
            return new List<Property>(){ pickOb };

        int campId = (sourceOb.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;

        // 整理筛选结果，将手选结果放在第一个
        List<Property> petList = new List<Property>();
        List<Property> finalList = RoundCombatMgr.GetPropertyList(campId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" },
            new List<string>(){ actionArgs.GetValue<string>("pick_rid") });

        // 筛选前后排信息
        foreach (Property targetOb in finalList)
        {
            if (targetOb.FormationRaw == FormationConst.RAW_BACK && targetOb.QueryTemp<int>("halo/halo_can_not_choose") == 0)
                petList.Add(targetOb);
        }

        // 返回目标列表
        if (petList.Count == 0)
            return new List<Property>(){ finalList[RandomMgr.GetRandom(finalList.Count)] };

        return new List<Property>(){ petList[RandomMgr.GetRandom(petList.Count)] };
    }
}

/// <summary>
/// 前后排二段技能目标收集脚本（已知前后排情况下搜集已知排的所有单位）
/// </summary>
public class SCRIPT_9515 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg

        // 收集拾取目标
        LPCMapping actionArgs = _params[2] as LPCMapping;
        string targetRid = actionArgs.GetValue<string>("pick_rid");
        if (string.IsNullOrEmpty(targetRid))
            return new List<Property>();

        // 查找目标对象
        Property targetOb = Rid.FindObjectByRid(targetRid);

        string raw = targetOb.FormationRaw;

        List<Property> petList = new List<Property>();

        List<Property> finalList = RoundCombatMgr.GetPropertyList(targetOb.CampId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" },
            new List<string>(){ targetRid });

        // 筛选前后排信息
        foreach (Property ob in finalList)
        {
            if (ob.FormationRaw == raw && ob.QueryTemp<int>("halo/halo_can_not_choose") == 0)
                petList.Add(ob);
        }

        // 返回目标列表
        return petList;
    }
}

// 通用全体目标收集脚本（区分前后排）
public class SCRIPT_9516 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        LPCMapping actionArgs = _params[2] as LPCMapping;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        // 整理筛选结果，将手选结果放在第一个
        List<Property> finalList = RoundCombatMgr.GetPropertyList(pickOb.CampId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" },
            new List<string>(){ actionArgs.GetValue<string>("pick_rid") });

        List<Property> petList1 = new List<Property>();
        List<Property> petList2 = new List<Property>();

        foreach (Property ob in finalList)
        {
            if (ob.FormationRaw == FormationConst.RAW_BACK)
                petList1.Add(ob);
            if (ob.FormationRaw == FormationConst.RAW_FRONT)
                petList2.Add(ob);
        }
        // 只有前排情况
        if (petList1.Count == 0 || petList2.Count == 0)
            actionArgs.Add("one_raw_id", 1);

        // 返回目标列表
        return finalList;
    }
}

// 己方全体目标收集脚本(除自身)
public class SCRIPT_9517 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;
        //LPCMapping actionArgs = _params[2] as LPCMapping;
        //Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED"))
                finalList.Add(targetOb);
        }

        // 排除自身
        finalList.Remove(sourceOb);

        // 返回目标列表
        return finalList;
    }
}

// 通用敌方前排全体目标收集脚本(只收集前排)
public class SCRIPT_9518 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;
        LPCMapping actionArgs = _params[2] as LPCMapping;
        //Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        int campId = (sourceOb.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;

        // 整理筛选结果，将手选结果放在第一个
        List<Property> petList = new List<Property>();
        List<Property> finalList = RoundCombatMgr.GetPropertyList(campId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" },
            new List<string>(){ actionArgs.GetValue<string>("pick_rid") });

        // 筛选前后排信息
        foreach (Property targetOb in finalList)
        {
            if (targetOb.FormationRaw == FormationConst.RAW_FRONT)
                petList.Add(targetOb);
        }

        // 返回目标列表
        if (petList.Count == 0)
            return new List<Property>(){};

        return petList;
    }
}

// 己方对应元素属性全体目标收集脚本
public class SCRIPT_9519 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        Property sourceOb = _params[0] as Property;
        LPCArray propValue = (_params[2] as LPCValue).AsArray;
        //Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位和对应属性单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED") && targetOb.Query<int>("element") == propValue[0].AsArray[0].AsInt)
                finalList.Add(targetOb);
        }

        // 返回目标列表
        return finalList;
    }
}

// 通用敌方速度最快的 3 个目标收集脚本
public class SCRIPT_9520 : Script
{
    /// <summary>
    /// 比较速度
    /// </summary>
    public static int Compara(Property left, Property right)
    {
        int leftSpeed = left.QueryAttrib("speed");
        int rightSpeed = right.QueryAttrib("speed");

        if (leftSpeed > rightSpeed)
            return -1;

        if (leftSpeed < rightSpeed)
            return 1;

        return 0;
    }

    /// <summary>
    /// 筛选对象
    /// </summary>
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;
        LPCMapping actionArgs = _params[2] as LPCMapping;
        //Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        int campId = (sourceOb.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;

        // 整理筛选结果，将手选结果放在第一个
        List<Property> petList = new List<Property>();
        List<Property> finalList = RoundCombatMgr.GetPropertyList(campId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" },
            new List<string>(){ actionArgs.GetValue<string>("pick_rid") });

        // 筛选信息
        finalList.Sort(Compara);

        int index = Math.Min(finalList.Count, 3);

        for (int i = 0; i < index; i++)
            petList.Add(finalList[i]);

        // 返回目标列表
        if (petList.Count == 0)
            return new List<Property>(){};

        return petList;
    }
}

// 战场全体目标收集脚本
public class SCRIPT_9521 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        //Property sourceOb = _params[0] as Property;
        //LPCMapping actionArgs = _params[2] as LPCMapping;
        //Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList();

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED") && !targetOb.CheckStatus("B_CAN_NOT_CHOOSE"))
                finalList.Add(targetOb);
        }

        // 返回目标列表
        return finalList;
    }
}

/// <summary>
/// 兴奋的扫把专用目标选择脚本
/// </summary>
public class SCRIPT_9522 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg

        // 收集拾取目标
        Property sourceOb = _params[0] as Property;
        int skillId = (int)_params[1];
        LPCMapping actionArgs = _params[2] as LPCMapping;
        string targetRid = actionArgs.GetValue<string>("pick_rid");
        if (string.IsNullOrEmpty(targetRid))
            return new List<Property>();

        // 查找目标对象
        Property pickOb = Rid.FindObjectByRid(targetRid);
        if (pickOb == null)
            return new List<Property>();

        // 检查概率抽取全体目标
        int rate = 200;
        int upRate = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), sourceOb.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_RATE);
        rate += upRate;
        if (RandomMgr.GetRandom() < rate)
        {
            // 整理筛选结果，将手选结果放在第一个
            List<Property> finalList = RoundCombatMgr.GetPropertyList(pickOb.CampId,
                new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" },
                new List<string>(){ actionArgs.GetValue<string>("pick_rid") });
            return finalList;
        }

        // 返回目标列表
        return new List<Property>() { pickOb };
    }
}

// 自爆专用全体目标收集脚本
public class SCRIPT_9523 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg, hitEntityMap, cookie
        Property sourceOb = _params[0] as Property;
        int skillId = (int)_params[1];
        LPCMapping actionArgs = _params[2] as LPCMapping;
        LPCArray hitScriptArg = (_params[4] as LPCValue).AsArray;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));
        string cookie = (string)_params[6];

        // 整理筛选结果，将手选结果放在第一个
        List<Property> finalList = RoundCombatMgr.GetPropertyList(pickOb.CampId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" },
            new List<string>(){ actionArgs.GetValue<string>("pick_rid") });

        // 记录攻击信息
        LPCMapping blewData = new LPCMapping ();
        blewData.Add("cookie", cookie);
        blewData.Add("origin_skill_id", SkillMgr.GetOriginalSkillId(skillId));
        blewData.Add("reborn_hp", sourceOb.QueryAttrib("max_hp") * hitScriptArg[2].AsInt / 1000);
        LPCMapping blewMap = new LPCMapping();

        foreach (Property ob in finalList)
        {
            blewMap.Add(ob.GetRid(), 0);
        }

        blewData.Add("blew_map", blewMap);
        sourceOb.SetTemp("blew_data",LPCValue.Create(blewData));

        // 返回目标列表
        return finalList;
    }
}

// 冰雹风暴专用全体目标收集脚本
public class SCRIPT_9524 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg, hitEntityMap, cookie
        Property sourceOb = _params[0] as Property;
        int skillId = (int)_params[1];
        LPCMapping actionArgs = _params[2] as LPCMapping;
        //LPCArray hitScriptArg = (_params[4] as LPCValue).AsArray;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));
        string cookie = (string)_params[6];

        // 整理筛选结果，将手选结果放在第一个
        List<Property> finalList = RoundCombatMgr.GetPropertyList(pickOb.CampId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" },
            new List<string>(){ actionArgs.GetValue<string>("pick_rid") });

        // 记录攻击信息
        LPCMapping blewData = new LPCMapping ();
        blewData.Add("cookie", cookie);
        blewData.Add("origin_skill_id", SkillMgr.GetOriginalSkillId(skillId));
        LPCMapping blewMap = new LPCMapping();

        foreach (Property ob in finalList)
        {
            blewMap.Add(ob.GetRid(), 0);
        }

        blewData.Add("blew_map", blewMap);
        sourceOb.SetTemp("blew_data",LPCValue.Create(blewData));

        // 返回目标列表
        return finalList;
    }
}

// 己方单体目标收集脚本（当前敏捷最低的）
public class SCRIPT_9525 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;

        // 收集本阵营所有对象（不管死活）
        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);
        List<Property> selectOb = new List<Property>();
        int maxAgiRate = ConstantValue.MAX_VALUE;

        // 遍历各个角色
        foreach (Property targetOb in targetList)
        {
            // 角色已经死亡
            if (targetOb.CheckStatus("DIED"))
                continue;

            // 如果敏捷较大不处理
            int targetAgi = targetOb.QueryAttrib("agility");
            if (targetAgi > maxAgiRate)
                continue;

            // 如果敏捷相同
            if (targetAgi == maxAgiRate)
            {
                selectOb.Add(targetOb);
                continue;
            }

            // 敏捷较小重新记录数据
            maxAgiRate = targetAgi;
            selectOb.Clear();
            selectOb.Add(targetOb);
        }

        // 没有选择到目标
        if (selectOb.Count == 0)
            return new List<Property>();

        // 返回目标列表
        return new List<Property>() { selectOb[RandomMgr.GetRandom(selectOb.Count)] };
    }
}

/// <summary>
/// 通用敌方单体目标收集脚本（随机选择，适用于传过来的pick_rid是不确定的情况）
/// </summary>
public class SCRIPT_9526 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        LPCMapping actionArgs = _params[2] as LPCMapping;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        int campId = (pickOb.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;

        List<Property> targetList = RoundCombatMgr.GetPropertyList(campId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED") && targetOb.QueryTemp<int>("halo/halo_can_not_choose") == 0)
                finalList.Add(targetOb);
        }

        if (finalList.Count == 0)
            return finalList;

        // 按照随机种子随机出目标，返回目标列表
        return new List<Property>(){ finalList[RandomMgr.GetRandom(finalList.Count)] };
    }
}

// 战场全体目标收集脚本(除自身外)
public class SCRIPT_9527 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;
        //LPCMapping actionArgs = _params[2] as LPCMapping;
        //Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList();

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED") && !targetOb.CheckStatus("B_CAN_NOT_CHOOSE"))
                finalList.Add(targetOb);
        }

        finalList.Remove(sourceOb);

        // 返回目标列表
        return finalList;
    }
}

// 敌方单体目标收集脚本（当前生命最低的）
public class SCRIPT_9528 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        //Property sourceOb = _params[0] as Property;
        LPCMapping actionArgs = _params[2] as LPCMapping;

        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList(pickOb.CampId);

        List<Property> selectOb = new List<Property>();

        int maxHpRate = ConstantValue.MAX_VALUE;

        // 遍历各个角色
        foreach (Property targetOb in targetList)
        {
            // 角色已经死亡
            if (targetOb.CheckStatus("DIED"))
                continue;

            // 如果HpRate较大不处理
            int targetHpRate = Game.Divided(targetOb.Query<int>("hp"), targetOb.QueryAttrib("max_hp"));
            if (targetHpRate > maxHpRate)
                continue;

            // 如果hp rate相同
            if (targetHpRate == maxHpRate)
            {
                selectOb.Add(targetOb);
                continue;
            }

            // targetHpRate较小重新记录数据
            maxHpRate = targetHpRate;
            selectOb.Clear();
            selectOb.Add(targetOb);
        }

        // 没有选择到目标
        if (selectOb.Count == 0)
            return new List<Property>();

        // 返回目标列表
        return new List<Property>() { selectOb[RandomMgr.GetRandom(selectOb.Count)] };
    }
}

// 己方单体目标收集脚本（当前生命最低的两个单位）
public class SCRIPT_9529 : Script
{
    /// <summary>
    /// 比较生命比率大小
    /// </summary>
    public static int Compara(Property left, Property right)
    {
        int leftHpRate = left.QueryTemp<int>("hp_rate");
        int rightHpRate = right.QueryTemp<int>("hp_rate");

        if (leftHpRate < rightHpRate)
            return -1;

        if (leftHpRate > rightHpRate)
            return 1;

        return 0;
    }

    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;
        LPCMapping actionArgs = _params[2] as LPCMapping;

        // 收集本阵营所有对象（不管死活）
        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" },
            new List<string>(){ actionArgs.GetValue<string>("pick_rid") });

        List<Property> petList = new List<Property>();

        // 筛选信息
        targetList.Sort(Compara);

        int index = Math.Min(targetList.Count, 2);

        for (int i = 0; i < index; i++)
            petList.Add(targetList[i]);

        // 没有选择到目标
        if (petList.Count == 0)
            return new List<Property>();

        // 返回目标列表
        return petList;
    }
}

// 己方全体目标收集脚本(废弃)
public class SCRIPT_9530 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;
        //LPCMapping actionArgs = _params[2] as LPCMapping;
        //Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        // 返回目标列表
        return targetList;
    }
}

// 敌方单体目标收集脚本（当前生命值最高的）
public class SCRIPT_9531 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        LPCMapping actionArgs = _params[2] as LPCMapping;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList(pickOb.CampId);
        List<Property> selectOb = new List<Property>();
        int maxHp = -1;

        // 遍历各个角色
        foreach (Property targetOb in targetList)
        {
            // 角色已经死亡
            if (targetOb.CheckStatus("DIED") || targetOb.QueryTemp<int>("halo/halo_can_not_choose") > 0)
                continue;

            // 如果Hp较大不处理
            int targetHp = targetOb.Query<int>("hp");
            if (targetHp < maxHp)
                continue;

            // 如果hp相同
            if (targetHp == maxHp)
            {
                selectOb.Add(targetOb);
                continue;
            }

            // 重新记录数据
            maxHp = targetHp;
            selectOb.Clear();
            selectOb.Add(targetOb);
        }

        // 没有选择到目标
        if (selectOb.Count == 0)
            return new List<Property>();

        // 返回目标列表
        return new List<Property>() { selectOb[RandomMgr.GetRandom(selectOb.Count)] };
    }
}

// 敌方单体目标收集脚本（当前攻击力最高的）
public class SCRIPT_9532 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        LPCMapping actionArgs = _params[2] as LPCMapping;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList(pickOb.CampId);
        List<Property> selectOb = new List<Property>();
        int maxValue = -1;

        // 遍历各个角色
        foreach (Property targetOb in targetList)
        {
            // 角色已经死亡
            if (targetOb.CheckStatus("DIED") || targetOb.QueryTemp<int>("halo/halo_can_not_choose") > 0)
                continue;

            // 如果攻击力较大不处理
            int targetAtk = targetOb.QueryAttrib("attack");
            if (targetAtk < maxValue)
                continue;

            // 如果攻击力相同
            if (targetAtk == maxValue)
            {
                selectOb.Add(targetOb);
                continue;
            }

            // 重新记录数据
            maxValue = targetAtk;
            selectOb.Clear();
            selectOb.Add(targetOb);
        }

        // 没有选择到目标
        if (selectOb.Count == 0)
            return new List<Property>();

        // 返回目标列表
        return new List<Property>() { selectOb[RandomMgr.GetRandom(selectOb.Count)] };
    }
}

// 通用己方全体目标收集脚本，不管死活（用于后续hit优先收集目标）
public class SCRIPT_9533 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        LPCMapping actionArgs = _params[2] as LPCMapping;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        // 整理筛选结果，将手选结果放在第一个
        List<Property> finalList = RoundCombatMgr.GetPropertyList(pickOb.CampId,
            new List<string>(){ "B_CAN_NOT_CHOOSE" },
            new List<string>(){ actionArgs.GetValue<string>("pick_rid") });

        // 返回目标列表
        return finalList;
    }
}

/// <summary>
/// 随机 3 人目标收集脚本（手动选择第一个目标，剩余随机）
/// </summary>
public class SCRIPT_9600 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        LPCMapping actionArgs = _params[2] as LPCMapping;
        Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> finalList = new List<Property>();
        List<Property> targetList = RoundCombatMgr.GetPropertyList(pickOb.CampId,
            new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" },
            new List<string>(){ actionArgs.GetValue<string>("pick_rid") });

        // 将手选目标放在列表第一个位置
        if (targetList.Count != 0)
        {
            finalList.Add(targetList[0]);
            targetList.Remove(targetList[0]);
        }

        // 随机筛选目标
        while (true)
        {
            // 列表为空跳出循环
            if (targetList.Count == 0)
                break;

            // 最终列表大于随机个数跳出循环
            if (finalList.Count >= 3)
                break;

            // 随机筛选目标
            Property ob = targetList[RandomMgr.GetRandom(targetList.Count)];

            // 加入最终目标列表
            finalList.Add(ob);

            // 从目标列表中移除对象
            targetList.Remove(ob);
        }

        // 返回最终目标列表
        return finalList;
    }
}


/// <summary>
/// 光环作用目标选择脚本(己方除自身外所有队友)（废弃）
/// </summary>
public class SCRIPT_9800 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        //sourceOb, propCsv,propValue, selectArgs
        Property sourceOb = _params[0] as Property;

        List<Property> allList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        List<Property> targetList = new List<Property>();

        foreach (Property targetOb in allList)
        {
            if (targetOb != sourceOb)
                targetList.Add(targetOb);
        }

        // 返回最终目标列表
        return targetList;
    }
}

// 光环己方全体目标收集脚本(除自身)
public class SCRIPT_9801 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;
        //LPCMapping actionArgs = _params[2] as LPCMapping;
        //Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位,排除自身
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.GetRid().Equals(sourceOb.GetRid()))
                finalList.Add(targetOb);
        }

        // 返回目标列表
        return finalList;
    }
}

/// <summary>
/// 光环自身目标收集脚本（自动选择）
/// </summary>
public class SCRIPT_9802 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg

        // 收集拾取目标
        Property sourceOb = _params[0] as Property;

        // 返回目标列表
        return new List<Property>() { sourceOb };
    }
}

// 光环己方全体目标收集脚本
public class SCRIPT_9803 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;
        //LPCMapping actionArgs = _params[2] as LPCMapping;
        //Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        // 返回目标列表
        return targetList;
    }
}

// 光环通用敌方全体目标收集脚本
public class SCRIPT_9804 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        //LPCMapping actionArgs = _params[2] as LPCMapping;
        Property sourceOb = _params[0] as Property;

        int campId = (sourceOb.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;

        // 整理筛选结果，将手选结果放在第一个
        List<Property> finalList = RoundCombatMgr.GetPropertyList(campId,
            new List<string>(){ "B_CAN_NOT_CHOOSE" });

        List<Property> petList = new List<Property>();

        foreach (Property checkOb in finalList)
        {
            if (checkOb.QueryTemp<int>("halo/halo_can_not_choose") > 0)
                continue;
            petList.Add(checkOb);
        }

        // 返回目标列表
        return petList;
    }
}

// 光环战场全体目标收集脚本(除自身外)
public class SCRIPT_9805 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        Property sourceOb = _params[0] as Property;
        //LPCMapping actionArgs = _params[2] as LPCMapping;
        //Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList();

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("B_CAN_NOT_CHOOSE"))
                finalList.Add(targetOb);
        }

        finalList.Remove(sourceOb);

        // 返回目标列表
        return finalList;
    }
}

// 己方对应元素属性全体目标收集脚本
public class SCRIPT_9806 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        Property sourceOb = _params[0] as Property;
        LPCArray propValue = (_params[2] as LPCValue).AsArray;
        //Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位和对应属性单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED") && targetOb.Query<int>("element") == propValue[0].AsArray[0].AsInt)
                finalList.Add(targetOb);
        }

        // 返回目标列表
        return finalList;
    }
}

// 光环己方对应元素属性全体目标收集脚本
public class SCRIPT_9807 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        Property sourceOb = _params[0] as Property;
        LPCArray propValue = (_params[2] as LPCValue).AsArray;
        //Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位和对应属性单位
        foreach (Property targetOb in targetList)
        {
            if (targetOb.Query<int>("element") == propValue[0].AsArray[0].AsInt)
                finalList.Add(targetOb);
        }

        // 返回目标列表
        return finalList;
    }
}

// 光环战场全体目标收集脚本
public class SCRIPT_9808 : Script
{
    public override object Call(params object[] _params)
    {
        // 参数格式
        // sourceOb, skillId, actionArgs, skillInfo, hitScriptArg
        //Property sourceOb = _params[0] as Property;
        //LPCMapping actionArgs = _params[2] as LPCMapping;
        //Property pickOb = Rid.FindObjectByRid(actionArgs.GetValue<string>("pick_rid"));

        List<Property> targetList = RoundCombatMgr.GetPropertyList();

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("B_CAN_NOT_CHOOSE"))
                finalList.Add(targetOb);
        }

        // 返回目标列表
        return finalList;
    }
}