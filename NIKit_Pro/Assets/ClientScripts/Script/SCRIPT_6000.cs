/// <summary>
/// SCRIPT_6000.cs
/// Create by fengkk 2014-11-25
/// 技能脚本
/// </summary>

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using LPC;

/// <summary>
/// 技能选择对象脚本，选择敌方全体对象
/// </summary>
public class SCRIPT_6001 : Script
{
    public override object Call(params object[] _param)
    {
        Property ob = _param[1] as Property;

        int camp = CampConst.CAMP_TYPE_ATTACK;
        if (ob.CampId == CampConst.CAMP_TYPE_ATTACK)
            camp = CampConst.CAMP_TYPE_DEFENCE;

        List<Property> petList = new List<Property>();
        // 测试用选择为守方的阵营
        List<Property> allPetList = RoundCombatMgr.GetPropertyList(camp);

        // 若已阵亡，不作选择
        foreach (Property item in allPetList)
        {
            if (item.CheckStatus("DIED") || item.CheckStatus("B_CAN_NOT_CHOOSE"))
                continue;

                petList.Add(item);
        }

        return petList;
    }
}

/// <summary>
/// 技能选择对象脚本，选择己方全体对象
/// </summary>
public class SCRIPT_6002 : Script
{
    public override object Call(params object[] _param)
    {
        Property ob = _param[1] as Property;

        List<Property> petList = new List<Property>();
        // int skill_id = (int) _param [0];

        // 选择为攻方的阵营
        List<Property> allPetList = RoundCombatMgr.GetPropertyList(ob.CampId);

        // 若已阵亡，不作选择
        foreach (Property item in allPetList)
        {
            if (!item.CheckStatus("DIED") && !item.CheckStatus("B_CAN_NOT_CHOOSE"))
                petList.Add(item);
        }

        return petList;
    }
}

/// <summary>
/// 技能选择对象脚本，选择己方全体对象(排除自身)
/// </summary>
public class SCRIPT_6003 : Script
{
    public override object Call(params object[] _param)
    {
        Property ob = _param[1] as Property;

        // skillId, sourceOb
        List<Property> petList = new List<Property>();
        // int skill_id = (int) _param [0];

        // 获取释放者
        Property sourceOb = _param[1] as Property;

        // 选择为攻方的阵营
        List<Property> allPetList = RoundCombatMgr.GetPropertyList(ob.CampId);

        foreach (Property item in allPetList)
        {
            // 死亡或者自身排除
            if (!item.CheckStatus("DIED") && (item != sourceOb) && !item.CheckStatus("B_CAN_NOT_CHOOSE"))
                petList.Add(item);
        }

        return petList;
    }
}

/// <summary>
/// 技能选择对象脚本，选择自身
/// </summary>
public class SCRIPT_6004 : Script
{
    public override object Call(params object[] _param)
    {
        // skillId, sourceOb
        List<Property> petList = new List<Property>();
        // int skill_id = (int) _param [0];

        // 获取释放者
        Property sourceOb = _param[1] as Property;

        petList.Add(sourceOb);

        return petList;
    }
}

/// <summary>
/// 技能选择对象脚本，选择己方已死亡的对象
/// </summary>
public class SCRIPT_6005 : Script
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY", "B_CAN_NOT_CHOOSE", "D_NO_REBORN"};

    public override object Call(params object[] _param)
    {
        Property ob = _param[1] as Property;

        List<Property> petList = new List<Property>();
        // int skill_id = (int) _param [0];

        // 选择为攻方的阵营
        List<Property> allPetList = RoundCombatMgr.GetPropertyList(ob.CampId);

        // 筛选出死亡对象
        foreach (Property item in allPetList)
        {
            //if (item.CheckStatus("DIED") && !item.CheckStatus(statusList) && !item.IsOccupied())
            if (item.CheckStatus("DIED") && !item.CheckStatus(statusList))
            {
                // 被占用且没有被替身状态
                if (item.IsOccupied() && !item.CheckStatus("B_STANDED"))
                    continue;
                petList.Add(item);
            }
        }

        return petList;
    }
}

/// <summary>
/// 计算技能的能量开销脚本
/// </summary>
public class SCRIPT_6006 : Script
{
    public override object Call(params object[] _param)
    {
        Property who = _param[0] as Property;
        int skillId = (int)_param[1];
        LPCMapping cost_arg = _param[2] as LPCMapping;
        //CsvRow skillCsv = _param[3] as CsvRow;
        LPCMapping costMap = new LPCMapping();

        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), who.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_MP_COST);

        costMap.Add("mp", cost_arg.GetValue<int>("mp") - skillEffect);

        return costMap;
    }
}

/// <summary>
/// 特殊能量开销计算脚本，消耗剩余全部能量，最低消耗量为配置量
/// </summary>
public class SCRIPT_6007 : Script
{
    public override object Call(params object[] _param)
    {
        Property who = _param[0] as Property;
        //int skill_id = (int)_param[1];
        LPCMapping cost_arg = _param[2] as LPCMapping;
        //CsvRow skillCsv = _param[3] as CsvRow;

        LPCMapping costMap = new LPCMapping();

        costMap.Add("mp", Math.Max(who.Query<int>("mp"), cost_arg.GetValue<int>("mp")));

        return costMap;
    }
}

///<summary>
/// 技能冷却时间
///</summary>
public class SCRIPT_6008 : Script
{
    public override object Call(params object[] _param)
    {
        Property who = _param[0] as Property;
        int skillId = (int)_param[1];
        LPCArray arg = _param[2] as LPCArray;

        // 没有CD配置时 返回0
        if (arg.Count < 1)
            return 0;

        // 返回cd时间
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), who.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_CD);
        // 获取CD减少属性
        int cdPropReduce = who.QueryAttrib("cd_reduce");
        int cd = arg[0].AsInt - skillEffect - cdPropReduce;
        int cdReduce = 0;
        if (who.CheckStatus("B_MAX_CD_REDUCE"))
        {
            List<LPCMapping> allStatus = who.GetStatusCondition("B_MAX_CD_REDUCE");
            foreach (LPCMapping statusMap in allStatus)
            {
                if (statusMap.GetValue<int>("max_reduce_cd") >= cdReduce)
                    cdReduce = statusMap.GetValue<int>("max_reduce_cd");
            }
        }

        return Math.Max(cd - cdReduce, 1);
    }
}

/// <summary>
/// 技能选择对象脚本，选择己方全体对象（包括已死亡的，暂时废弃）
/// </summary>
public class SCRIPT_6009 : Script
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY",};

    public override object Call(params object[] _param)
    {
        Property ob = _param[1] as Property;

        //List<Property> petList = new List<Property>();
        // int skill_id = (int) _param [0];

        // 选择为攻方的阵营
        List<Property> allPetList = RoundCombatMgr.GetPropertyList(ob.CampId);
        List<Property> petList = new List<Property>();

        // 筛选出对象
        foreach (Property item in allPetList)
        {
            if (!item.CheckStatus("B_CAN_NOT_CHOOSE") && !item.CheckStatus(statusList) && !item.IsOccupied())
                petList.Add(item);
        }

        return petList;
    }
}

/// <summary>
/// 技能选择对象脚本，选择首领目标
/// </summary>
public class SCRIPT_6010 : Script
{
    public override object Call(params object[] _param)
    {
        // int skill_id = (int) _param [0];
        Property ob = _param[1] as Property;

        List<Property> petList = new List<Property>();

        // 选择为己方的阵营
        List<Property> allPetList = RoundCombatMgr.GetPropertyList(ob.CampId);

        // 若已阵亡，不作选择
        foreach (Property target in allPetList)
        {
            if (target.CheckStatus("DIED") || target.Query<int>("is_boss") != 1 || target.CheckStatus("B_CAN_NOT_CHOOSE"))
                continue;

            petList.Add(target);
        }

        if (petList.Count == 0)
            petList.Add(ob);

        return petList;
    }
}

/// <summary>
/// 技能选择对象脚本，优先选择敌方前排全体对象
/// </summary>
public class SCRIPT_6011 : Script
{
    public override object Call(params object[] _param)
    {
        Property ob = _param[1] as Property;

        int camp = CampConst.CAMP_TYPE_ATTACK;
        if (ob.CampId == CampConst.CAMP_TYPE_ATTACK)
            camp = CampConst.CAMP_TYPE_DEFENCE;

        List<Property> petList = new List<Property>();

        // 测试用选择为守方的阵营
        List<Property> finalList = RoundCombatMgr.GetPropertyList(camp,
            new List<string>(){ "DIED" });

        foreach (Property targetOb in finalList)
        {
            if (targetOb.FormationRaw == FormationConst.RAW_FRONT && !targetOb.CheckStatus("B_CAN_NOT_CHOOSE"))
                petList.Add(targetOb);
        }

        if (petList.Count == 0)
            return finalList;

        return petList;
    }
}

///<summary>
/// 技能基础冷却时间
///</summary>
public class SCRIPT_6012 : Script
{
    public override object Call(params object[] _param)
    {
        int skillId = (int)_param[0];
        int curLv = (int)_param[1];
        LPCArray arg = _param[2] as LPCArray;

        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), curLv, SkillType.SE_CD);

        // 没有CD配置时 返回0
        if (arg[0].AsInt - skillEffect < 1)
            return 0;

        // 返回cd时间
        return arg[0].AsInt - skillEffect;
    }
}

/// <summary>
/// 技能选择对象脚本，选择全场全体对象
/// </summary>
public class SCRIPT_6013 : Script
{
    public override object Call(params object[] _param)
    {
        //Property ob = _param[1] as Property;

        List<Property> petList = new List<Property>();
        // int skill_id = (int) _param [0];

        // 选择全体对象
        List<Property> allPetList = RoundCombatMgr.GetPropertyList();

        // 筛选出死亡对象
        foreach (Property item in allPetList)
        {
            if (item.CheckStatus("DIED") && !item.CheckStatus("B_CAN_NOT_CHOOSE"))
                petList.Add(item);
        }

        return petList;
    }
}

/// <summary>
/// 技能选择对象脚本，选择己方已行动的对象
/// </summary>
public class SCRIPT_6014 : Script
{
    public override object Call(params object[] _param)
    {
        Property ob = _param[1] as Property;

        List<Property> petList = new List<Property>();
        // int skill_id = (int) _param [0];

        // 选择为攻方的阵营
        List<Property> allPetList = RoundCombatMgr.GetPropertyList(ob.CampId);

        foreach (Property item in allPetList)
        {
            // 如果已行动，则跳出循环
            if (!RoundCombatMgr.IsRoundDone(item))
                continue;

            // 若已阵亡或者未行动则不选择
            if (!item.CheckStatus("DIED") && !item.CheckStatus("B_CAN_NOT_CHOOSE"))
                petList.Add(item);
        }

        // 排除自身
        petList.Remove(ob);

        return petList;
    }
}

/// <summary>
/// 技能作用脚本
/// </summary>
public class SCRIPT_6018 : Script
{
    public override object Call(params object[] _param)
    {
        // 获取受创者对象
        Property target = _param[0] as Property;
        if (target == null)
            return 0;

        // 获取sourceInfo
        LPCMapping sourceInfo = _param[1] as LPCMapping;

        // 计算攻击力(初始伤害 * 攻方的百分比加成)
        LPCArray apply_args = _param[2] as LPCArray;

        int attack = (int)(apply_args[0].AsFloat / 1000 * sourceInfo.GetValue<int>("attack"));
        int defense = target.QueryAttrib("defense");

        LPCMapping damageInfo = new LPCMapping();
        LPCMapping points = new LPCMapping();
        points.Add("hp", (attack - defense < 0) ? 0 : attack - defense);
        damageInfo.Add("points", points);

        //LPCMapping damageInfo = CALC_SKILL_ALL_DAMAGE.Call(attack, finalAttribMap, improvementMap, sourceInfo, extraPara);

        // 返回伤害结构
        return damageInfo;
    }
}

/// <summary>
/// 技能选择对象脚本，选择己方全体对象，除自身
/// </summary>
public class SCRIPT_6019 : Script
{
    public override object Call(params object[] _param)
    {
        Property ob = _param[1] as Property;

        List<Property> petList = new List<Property>();
        // int skill_id = (int) _param [0];

        // 选择为攻方的阵营
        List<Property> allPetList = RoundCombatMgr.GetPropertyList(ob.CampId);

        // 若已阵亡，不作选择
        foreach (Property item in allPetList)
        {
            if (!item.CheckStatus("DIED") && !item.CheckStatus("B_CAN_NOT_CHOOSE") && !Property.Equals(item,ob))
                petList.Add(item);
        }

        return petList;
    }
}

/// <summary>
/// 技能选择对象脚本，选择己方已死亡的对象，若没有死亡对象，则选择全体队友
/// </summary>
public class SCRIPT_6020 : Script
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY", "B_CAN_NOT_CHOOSE", "D_NO_REBORN"};

    public override object Call(params object[] _param)
    {
        Property ob = _param[1] as Property;

        List<Property> petList = new List<Property>();
        // int skill_id = (int) _param [0];

        // 选择为攻方的阵营
        List<Property> allPetList = RoundCombatMgr.GetPropertyList(ob.CampId);

        // 筛选出死亡对象
        foreach (Property item in allPetList)
        {
            if (item.CheckStatus("DIED") && !item.CheckStatus(statusList))
            {
                // 被占用且没有被替身状态
                if (item.IsOccupied() && !item.CheckStatus("B_STANDED"))
                    continue;
                petList.Add(item);
            }
        }

        List<Property> newAllPetList = new List<Property>();
        if (petList.Count == 0)
        {
            // 已被占用位置的单位不参与选择、替身除外
            foreach (Property item in allPetList)
            {
                if (!item.IsOccupied() && !item.CheckStatus(statusList))
                    newAllPetList.Add(item);
            }

            return newAllPetList;
        }

        return petList;
    }
}

///<summary>
/// 特殊技能冷却时间计算脚本，水恶魔之书，魔界迷雾专用
///</summary>
public class SCRIPT_6021 : Script
{
    private static List<string> statusList = new List<string>(){ "B_REBORN_PASSIVE", "B_REBORN_PASSIVE_APPLY", "B_CAN_NOT_CHOOSE", "D_NO_REBORN"};

    public override object Call(params object[] _param)
    {
        Property who = _param[0] as Property;
        int skillId = (int)_param[1];
        LPCArray arg = _param[2] as LPCArray;

        // 没有CD配置时 返回0
        if (arg.Count < 1)
            return 0;

        // 返回cd时间
        int skillEffect = CALC_SKILL_UPGRADE_EFFECT.Call(SkillMgr.GetOriginalSkillId(skillId), who.GetSkillLevel(SkillMgr.GetOriginalSkillId(skillId)), SkillType.SE_CD);
        // 获取CD减少属性
        int cdPropReduce = who.QueryAttrib("cd_reduce");
        int cd = arg[0].AsInt - skillEffect - cdPropReduce;

        // 选择为攻方的阵营
        List<Property> allPetList = RoundCombatMgr.GetPropertyList(who.CampId);

        if (allPetList.Count == 0)
            return cd;

        int cdReduce = 0;
        if (who.CheckStatus("B_MAX_CD_REDUCE"))
        {
            List<LPCMapping> allStatus = who.GetStatusCondition("B_MAX_CD_REDUCE");
            foreach (LPCMapping statusMap in allStatus)
            {
                if (statusMap.GetValue<int>("max_reduce_cd") >= cdReduce)
                    cdReduce = statusMap.GetValue<int>("max_reduce_cd");
            }
        }

        List<Property> dieList = new List<Property>();

        // 筛选出死亡对象
        foreach (Property item in allPetList)
        {
            if (item.CheckStatus("DIED") && !item.CheckStatus(statusList))
                dieList.Add(item);
        }

        if (dieList.Count == 0)
            return Math.Max(cd - cdReduce - arg[1].AsInt, 1);

        return Math.Max(cd - cdReduce, 1);
    }
}

/// <summary>
/// 技能基础效果描述脚本(测试)
/// </summary>
public class SCRIPT_6050 : Script
{
    public override object Call(params object[] _param)
    {
        //int level = _param[0] as int;
        string desc = _param[1] as string;
        //LPCValue arg = _param[2] as LPCValue;

        return string.Format("[FDE3C8FF]{0}", desc);
    }
}

/// <summary>
/// 技能等级效果描述脚本(测试)
/// </summary>
public class SCRIPT_6200 : Script
{
    public override object Call(params object[] _param)
    {
        int level = (int)_param[0];
        LPCMapping arg = (_param[1] as LPCValue).AsMapping;
        bool isSingleDesc = (bool)_param[2];
        string desc = string.Empty;

        if (arg == null || arg.Count == 0)
            return desc;

        // 只抽取单条效果描述
        if (isSingleDesc)
            return CALC_SKILL_UPGRADE_EFFECT_DESC.Call(level, arg);

        foreach (int lv in arg.Keys)
        {
            string SfinalString = CALC_SKILL_UPGRADE_EFFECT_DESC.Call(lv, arg);

            if (level < lv)
                desc += string.Format("[FDE3C864]{0}[-]\n", SfinalString);
            else
                desc += string.Format("[FDE3C8FF]{0}[-]\n", SfinalString);
        }

        return desc;
    }
}

/// <summary>
/// 技能抽取权重计算脚本（通用）
/// </summary>
public class SCRIPT_6300 : Script
{
    public override object Call(params object[] _param)
    {
        // Property pet = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;

        LPCMapping data = new LPCMapping ();
        data.Add("fetch_level", 1);
        data.Add("weight", args.AsInt);

        return data;
    }
}

/// <summary>
/// 技能目标权重脚本（通用普通攻击）
/// </summary>
public class SCRIPT_6301 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int weight = 0;
        int hpRate = Game.Divided(targetOb.Query<int>("hp"), targetOb.QueryAttrib("max_hp"));
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb); 

        //克制目标+10
        if (restrain == ElementConst.ELEMENT_ADVANTAGE)
            weight += 10;

        //无克制+5
        if (restrain == ElementConst.ELEMENT_NEUTRAL)
            weight += 5;

        //破甲效果+15
        if (targetOb.CheckStatus("D_DEF_DOWN"))
            weight += 15;

        //伤害加深+10
        if (targetOb.CheckStatus("D_MARK"))
            weight += 10;

        //生命少于40%+5
        if (hpRate < 400)
            weight += 5;

        //生命少于20%+5
        if (hpRate < 200)
            weight += 5;

        // 检查昏睡目标
        if (targetOb.CampId != sourceOb.CampId && targetOb.CheckStatus("D_SLEEP"))
            weight = 0;

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 技能目标权重脚本（通用攻击技能）
/// </summary>
public class SCRIPT_6302 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int weight = 0;
        int hpRate = Game.Divided(targetOb.Query<int>("hp"), targetOb.QueryAttrib("max_hp"));
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb); 

        // 克制目标+10
        if (restrain == ElementConst.ELEMENT_ADVANTAGE)
            weight += 10;

        // 无克制+5
        if (restrain == ElementConst.ELEMENT_NEUTRAL)
            weight += 5;

        // 破甲效果+15
        if (targetOb.CheckStatus("D_DEF_DOWN"))
            weight += 15;

        // 伤害加深+10
        if (targetOb.CheckStatus("D_MARK"))
            weight += 10;

        // 生命少于40%+5
        if (hpRate < 400)
            weight += 5;

        //生命少于20%+5
        if (hpRate < 200)
            weight += 5;

        // 检查昏睡目标
        if (targetOb.CampId != sourceOb.CampId && targetOb.CheckStatus("D_SLEEP"))
            weight = 0;

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 技能目标权重脚本（通用驱散技能）
/// </summary>
public class SCRIPT_6303 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int weight = 0;
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb); 
        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        CsvRow statusInfo;
        int buffTypeId = 0;

        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            statusInfo = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = statusInfo.Query<int>("status_type");
            if (buff_type == StatusConst.TYPE_BUFF)
                buffTypeId += 1;
            else
                continue;
        }

        //带增益效果+20
        if (buffTypeId > 0)
            weight += 20;

        //克制目标+10
        if (restrain == ElementConst.ELEMENT_ADVANTAGE)
            weight += 10;

        //无克制+5
        if (restrain == ElementConst.ELEMENT_NEUTRAL)
            weight += 5;

        // 检查昏睡目标
        if (targetOb.CampId != sourceOb.CampId && targetOb.CheckStatus("D_SLEEP"))
            weight = 0;

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 技能目标权重脚本（通用净化技能）
/// </summary>
public class SCRIPT_6304 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        int weight = 0;
        int hpRate = Game.Divided(targetOb.Query<int>("hp"), targetOb.QueryAttrib("max_hp"));
        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        CsvRow statusInfo;
        int buffTypeId = 0;

        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            statusInfo = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = statusInfo.Query<int>("status_type");
            if (buff_type == StatusConst.TYPE_DEBUFF)
                buffTypeId += 1;
            else
                continue;
        }

        // 带减益效果+20
        if (buffTypeId > 0)
            weight += 20;

        // 生命值低于50%+5
        if (hpRate < 500)
            weight += 5;

        // 敌方单位
        if (sourceOb.CampId != targetOb.CampId)
        {
            //克制目标+10
            if (restrain == ElementConst.ELEMENT_ADVANTAGE)
                weight += 10;

            //无克制+5
            if (restrain == ElementConst.ELEMENT_NEUTRAL)
                weight += 5;

            //破甲效果+15
            if (targetOb.CheckStatus("D_DEF_DOWN"))
                weight += 10;

            //生命值低于20%+5
            if (hpRate < 200)
                weight += 5;
        }

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 技能目标权重脚本（通用辅助技能友军目标）
/// </summary>
public class SCRIPT_6305 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int weight = 0;
        int hpRate = targetOb.QueryTemp<int>("hp_rate");

        // 友军目标
        if (sourceOb.CampId == targetOb.CampId)
        {
            List<LPCMapping> allStatus = targetOb.GetAllStatus();
            CsvRow statusInfo;
            int buffTypeId = 0;

            for (int i = 0; i < allStatus.Count; i++)
            {
                // 获取状态的信息
                statusInfo = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

                // 获取状态的类型：debuff / buff
                int buff_type = statusInfo.Query<int>("status_type");
                if (buff_type == StatusConst.TYPE_BUFF)
                    buffTypeId += 1;
                else
                    continue;
            }

            // 无增益效果+20
            if (buffTypeId == 0)
                weight += 20;

            //生命值低于50%+5
            if (hpRate < 500)
                weight += 5;

            // 返回权重
            return weight;
        }

        // 敌军目标
        // 克制目标+10
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        if (restrain == ElementConst.ELEMENT_ADVANTAGE)
            weight += 10;

        // 无克制+5
        if (restrain == ElementConst.ELEMENT_NEUTRAL)
            weight += 5;

        // 破甲效果+10
        if (targetOb.CheckStatus("D_DEF_DOWN"))
            weight += 10;

        // 伤害加深+10
        if (targetOb.CheckStatus("D_MARK"))
            weight += 10;

        //生命少于40%+5
        if (hpRate < 400)
            weight += 5;

        //生命少于20%+5
        if (hpRate < 200)
            weight += 5;

        // 检查昏睡目标
        if (targetOb.CampId != sourceOb.CampId && targetOb.CheckStatus("D_SLEEP"))
            weight = 0;

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 技能目标权重脚本（通用辅助技能敌军目标）
/// </summary>
public class SCRIPT_6306 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int weight = 0;
        int hpRate = Game.Divided(targetOb.Query<int>("hp"), targetOb.QueryAttrib("max_hp"));
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);

        //克制目标+10
        if (restrain == ElementConst.ELEMENT_ADVANTAGE)
            weight += 10;

        //无克制+5
        if (restrain == ElementConst.ELEMENT_NEUTRAL)
            weight += 5;

        //破甲效果+15
        if (targetOb.CheckStatus("D_DEF_DOWN"))
            weight += 15;

        //伤害加深+10
        if (targetOb.CheckStatus("D_MARK"))
            weight += 10;

        //生命少于40%+5
        if (hpRate < 400)
            weight += 5;

        //生命少于20%+5
        if (hpRate < 200)
            weight += 5;

        // 检查昏睡目标
        if (targetOb.CampId != sourceOb.CampId && targetOb.CheckStatus("D_SLEEP"))
            weight = 0;

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 技能目标权重脚本（通用治疗技能）
/// </summary>
public class SCRIPT_6307 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int weight = 0;
        int hpRate = Game.Divided(targetOb.Query<int>("hp"), targetOb.QueryAttrib("max_hp"));

        // 如果是己方成员，则直接按照血量比选择目标
        if (sourceOb.CampId == targetOb.CampId && !targetOb.CheckStatus("B_FURY"))
        {
            // 生命值低于50%+5
            if (hpRate < 500)
                weight += 5;

            // 生命值低于40%+5
            if (hpRate < 400)
                weight += 5;

            // 生命值低于30%+5
            if (hpRate < 300)
                weight += 5;

            // 生命值低于20%+5
            if (hpRate < 200)
                weight += 5;

            // 返回权重数值
            return weight;
        }

        // 获取克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);

        // 克制目标+10
        if (restrain == ElementConst.ELEMENT_ADVANTAGE)
            weight += 10;

        // 无克制+5
        if (restrain == ElementConst.ELEMENT_NEUTRAL)
            weight += 5;

        // 破甲效果+15
        if (targetOb.CheckStatus("D_DEF_DOWN"))
            weight += 15;

        // 伤害加深+10
        if (targetOb.CheckStatus("D_MARK"))
            weight += 10;

        // 生命少于40%+5
        if (hpRate < 400)
            weight += 5;

        // 生命少于20%+5
        if (hpRate < 200)
            weight += 5;

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 技能目标权重脚本（通用复活技能）
/// </summary>
public class SCRIPT_6308 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int weight = 0;
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);
        int hpRate = Game.Divided(targetOb.Query<int>("hp"), targetOb.QueryAttrib("max_hp"));

        //己方单位有死亡
        if (targetOb.CheckStatus("DIED") && sourceOb.CampId == targetOb.CampId)
            weight += 5;

        //克制目标+10
        if (restrain == ElementConst.ELEMENT_ADVANTAGE)
            weight += 10;

        //无克制+5
        if (restrain == ElementConst.ELEMENT_NEUTRAL)
            weight += 5;

        //破甲效果+15
        if (targetOb.CheckStatus("D_DEF_DOWN"))
            weight += 15;

        //伤害加深+10
        if (targetOb.CheckStatus("D_MARK"))
            weight += 10;

        //生命少于40%+5
        if (hpRate < 400)
            weight += 5;

        //生命少于20%+5
        if (hpRate < 200)
            weight += 5;

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 计算技能连击次数（急速射击）
/// </summary>
public class SCRIPT_6309 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        // LPCValue comboArgs = _param[1] as LPCValue;
        LPCMapping extraPara = _param[2] as LPCMapping;

        // 获取攻击目标
        Property targetOb = Rid.FindObjectByRid(extraPara["pick_rid"].AsString);

        // 获取克制关系
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb);

        // 如果克制，暴击几率提升15%
        // 如果被克制，则暴击几率降低15%
        if (restrain == ElementConst.ELEMENT_ADVANTAGE)
            restrain = CombatConst.RESTRAIN_DEADLY_VALUE;
        else if (restrain == ElementConst.ELEMENT_DISADVANTAGE)
            restrain = -CombatConst.RESTRAIN_DEADLY_VALUE;

        // 计算概率
        int calcCrtRate = Math.Min(sourceOb.QueryAttrib("crt_rate") + restrain, 1000);

        // 根据暴击率计算连击次数
        return Math.Max((calcCrtRate / 200), 2);
    }
}

/// <summary>
/// 计算技能连击次数（根据自身敏捷计算）
/// </summary>
public class SCRIPT_6310 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        // LPCValue comboArgs = _param[1] as LPCValue;
        //LPCMapping extraPara = _param[2] as LPCMapping;

        // 获取自身敏捷
        int castNum = 0;
        int atkSpd = sourceOb.QueryAttrib("agility");

        if (atkSpd > 239)
            castNum = 4;
        if (atkSpd <= 239)
            castNum = 3;
        if (atkSpd <= 189)
            castNum = 2;
        if (atkSpd < 140)
            castNum = 1;

        // 根据暴击率计算连击次数
        return castNum;
    }
}

/// <summary>
/// 技能抽取权重计算脚本（血量判断）
/// </summary>
public class SCRIPT_6311 : Script
{
    public override object Call(params object[] _param)
    {
        Property petOb = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;
        //LPCValue fetchType = _param[2] as LPCValue;

        // 获取角色列表
        List<Property> targetList = RoundCombatMgr.GetPropertyList(petOb.CampId);
        int minRate = 1000;

        // 遍历角色
        foreach (Property checkOb in targetList)
        {
            // 如果角色已经死亡不处理
            if (checkOb.CheckStatus("DIED"))
                continue;

            // 获取角色当前血量情况
            int maxHp = checkOb.QueryAttrib("max_hp");
            int hp = checkOb.Query<int>("hp");

            // 血量低于20%直接释放无视抽取权重
            minRate = Math.Min(minRate, Game.Divided(hp, maxHp));
        }

        // 抽取结果
        LPCMapping fetchMap = new LPCMapping();

        // 根据minRate选择权重
        if (minRate <= 200)
        {
            // 血量低于20%直接释放无视抽取权重
            fetchMap.Add("fetch_level", 2);
            fetchMap.Add("weight", args.AsInt);
        }
        else if (minRate <= 500)
        {
            // 血量低于50%才起效
            fetchMap.Add("fetch_level", 1);
            fetchMap.Add("weight", args.AsInt);
        }
        else
        {
            // 其他情况不能抽取
            fetchMap.Add("fetch_level", 0);
            fetchMap.Add("weight", 0);
        }

        // 返回数据
        return fetchMap;
    }
}

/// <summary>
/// 技能抽取权重计算脚本（能量判断）
/// </summary>
public class SCRIPT_6312 : Script
{
    public override object Call(params object[] _param)
    {
        Property petOb = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;

        List<Property> targetList = RoundCombatMgr.GetPropertyList(petOb.CampId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED"))
                finalList.Add(targetOb);
        }

        foreach (Property checkOb in finalList)
        {
            if (checkOb.Query<int>("mp") <= 2)
            {
                LPCMapping fetchData = new LPCMapping ();
                fetchData.Add("fetch_level", 1);
                fetchData.Add("weight", args.AsInt);
                return fetchData;
            }
        }

        LPCMapping data = new LPCMapping ();
        data.Add("fetch_level", 0);
        data.Add("weight", 0);

        return data;
    }
}

/// <summary>
/// 技能抽取权重计算脚本（减益状态判断）
/// </summary>
public class SCRIPT_6313 : Script
{
    public override object Call(params object[] _param)
    {
        Property petOb = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;

        List<Property> targetList = RoundCombatMgr.GetPropertyList(petOb.CampId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED"))
                finalList.Add(targetOb);
        }

        CsvRow info;

        int typeId = 0;

        foreach (Property checkOb in finalList)
        {
            List<LPCMapping> allStatus = checkOb.GetAllStatus();
            for (int i = 0; i < allStatus.Count; i++)
            {
                info = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));
                if (info.Query<int>("status_type") == StatusConst.TYPE_DEBUFF)
                    typeId += 1;
            }
        }

        // 检查减益状态存在与否没有则返回0权重
        if (typeId == 0)
        {
            LPCMapping data = new LPCMapping ();
            data.Add("fetch_level", 0);
            data.Add("weight", 0);
            return data;
        }

        LPCMapping fetchData = new LPCMapping ();
        fetchData.Add("fetch_level", 1);
        fetchData.Add("weight", args.AsInt);

        return fetchData;
    }
}

/// <summary>
/// 技能抽取权重计算脚本（死亡单位判断）
/// </summary>
public class SCRIPT_6314 : Script
{
    public override object Call(params object[] _param)
    {
        Property petOb = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;

        List<Property> targetList = RoundCombatMgr.GetPropertyList(petOb.CampId);

        List<Property> dieList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (targetOb.CheckStatus("DIED"))
                dieList.Add(targetOb);
        }

        // 如果没有死亡单位，则返回0权重
        if (dieList.Count == 0)
        {
            LPCMapping data = new LPCMapping ();
            data.Add("fetch_level", 0);
            data.Add("weight", 0);
            return data;
        }

        LPCMapping fetchMap = new LPCMapping();
        fetchMap.Add("fetch_level", 2);
        fetchMap.Add("weight", args.AsInt);

        return fetchMap;
    }
}

/// <summary>
/// 技能目标权重脚本（通用加能量技能）
/// </summary>
public class SCRIPT_6315 : Script
{
    public override object Call(params object[] _param)
    {
        //Property sourceOb = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int weight = 0;
        int curMp = targetOb.Query<int>("mp");
        List<LPCMapping> allStatus = targetOb.GetAllStatus();
        CsvRow statusInfo;
        int buffTypeId = 0;

        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            statusInfo = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = statusInfo.Query<int>("status_type");
            if (buff_type == StatusConst.TYPE_BUFF)
                buffTypeId += 1;
            else
                continue;
        }

        //无增益效果+5
        if (buffTypeId == 0)
            weight += 5;
        //能量低于5+5
        if (curMp < 5)
            weight += 5;
        //能量低于4+5
        if (curMp < 4)
            weight += 5;
        //能量低于3+5
        if (curMp < 3)
            weight += 5;
        //能量低于2+5
        if (curMp < 2)
            weight += 5;
        //能量低于1+5
        if (curMp < 1)
            weight += 5;

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 技能冷却类抽取权重计算脚本
/// </summary>
public class SCRIPT_6316 : Script
{
    public override object Call(params object[] _param)
    {
        Property targetOb = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;

        LPCArray skills = targetOb.GetAllSkills();
        foreach (LPCValue mks in skills.Values)
        {
            int skillId = mks.AsArray[0].AsInt;
            if (CdMgr.SkillIsCooldown(targetOb, skillId))
            {
                LPCMapping fetchData = new LPCMapping ();
                fetchData.Add("fetch_level", 1);
                fetchData.Add("weight", args.AsInt);
                return fetchData;
            }
        }

        LPCMapping data = new LPCMapping ();
        data.Add("fetch_level", 0);
        data.Add("weight", 0);

        return data;
    }
}

/// <summary>
/// 技能目标权重脚本（己方冷却减少类专用，选取技能冷却值最大的目标）
/// </summary>
public class SCRIPT_6317 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        Property petOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int weight = 0;

        // 遍历所有人的最大CD，得出一个最大值
        List<Property> targetList = RoundCombatMgr.GetPropertyList(petOb.CampId);
        int targetRemainCd = 0;
        foreach (Property targetOb in targetList)
        {
            LPCArray targetSkills = targetOb.GetAllSkills();

            foreach (LPCValue targetMks in targetSkills.Values)
            {
                int skillId = targetMks.AsArray[0].AsInt;

                // 如果有技能在冷却
                if (!CdMgr.SkillIsCooldown(targetOb, skillId))
                    continue;

                // 获取技能最大cd
                int cd = CdMgr.GetSkillCdRemainRounds(targetOb, skillId);
                if (cd > targetRemainCd)
                    targetRemainCd = cd;
            }
        }

        // 检查自身技能CD，得出一个最大值
        int petRemainCd = 0;
        LPCArray petSkills = petOb.GetAllSkills();
        foreach (LPCValue mks in petSkills.Values)
        {
            int skillId = mks.AsArray[0].AsInt;
            if (!CdMgr.SkillIsCooldown(petOb, skillId))
                continue;

            int cd = CdMgr.GetSkillCdRemainRounds(petOb, skillId);
            if (cd > petRemainCd)
                petRemainCd = cd;
        }

        // 确定权重
        if (petRemainCd >= targetRemainCd)
            weight += 10;

        // 检查昏睡目标
        if (petOb.CampId != sourceOb.CampId && petOb.CheckStatus("D_SLEEP"))
            weight = 0;

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 技能目标权重脚本（优先出手）
/// </summary>
public class SCRIPT_6318 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int weight = 0;

        if (sourceOb.CampId != targetOb.CampId)
        {
            int hpRate = Game.Divided(targetOb.Query<int>("hp"), targetOb.QueryAttrib("max_hp"));
            int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb); 

            // 克制目标+10
            if (restrain == ElementConst.ELEMENT_ADVANTAGE)
                weight += 10;

            // 无克制+5
            if (restrain == ElementConst.ELEMENT_NEUTRAL)
                weight += 5;

            // 破甲效果+10
            if (targetOb.CheckStatus("D_DEF_DOWN"))
                weight += 10;

            // 伤害加深+10
            if (targetOb.CheckStatus("D_MARK"))
                weight += 10;

            // 生命少于40%+5
            if (hpRate < 400)
                weight += 5;

            // 生命少于20%+5
            if (hpRate < 200)
                weight += 5;
        }

        // 遍历所有人的速度，得出一个最小值
        List<Property> targetList = RoundCombatMgr.GetPropertyList(targetOb.CampId);
        int minSpeed = targetOb.QueryAttrib("speed");
        foreach (Property petOb in targetList)
        {
            if (petOb.QueryAttrib("speed") <= minSpeed)
                minSpeed = petOb.QueryAttrib("speed");
        }

        if (targetOb.QueryAttrib("speed") <= minSpeed)
            weight += 10;

        // 检查昏睡目标
        if (targetOb.CampId != sourceOb.CampId && targetOb.CheckStatus("D_SLEEP"))
            weight = 0;

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 技能抽取权重脚本（全场debuff/debuff存在判断）
/// </summary>
public class SCRIPT_6319 : Script
{
    public override object Call(params object[] _param)
    {
        //Property petOb = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;

        List<Property> targetList = RoundCombatMgr.GetPropertyList();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (targetOb.CheckStatus("DIED"))
                continue;
            List<LPCMapping> allStatus = targetOb.GetAllStatus();
            foreach (LPCMapping statusData in allStatus)
            {
                CsvRow info;
                // 获取状态的信息
                info = StatusMgr.GetStatusInfo(statusData.GetValue<int>("status_id"));
                int statusType = info.Query<int>("status_type");
                if (statusType == StatusConst.TYPE_BUFF || statusType == StatusConst.TYPE_DEBUFF)
                    return args.AsInt;
            }
        }

        return args.AsInt;
    }
}

/// <summary>
/// 普通攻击技能抽取权重计算脚本（通用）
/// </summary>
public class SCRIPT_6320 : Script
{
    public override object Call(params object[] _param)
    {
        LPCValue args = _param[1] as LPCValue;
        return args.AsInt;
    }
}

/// <summary>
/// 技能目标权重脚本（无克制排序）
/// </summary>
public class SCRIPT_6321 : Script
{
    public override object Call(params object[] _param)
    {
        //Property sourceOb = _param[0] as Property;
        //Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;

        // 返回权重数值
        return 100;
    }
}

/// <summary>
/// 技能抽取权重计算脚本（血量百分比绝对差值判断，周天法）
/// </summary>
public class SCRIPT_6322 : Script
{
    public override object Call(params object[] _param)
    {
        Property petOb = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;
        //LPCValue fetchType = _param[2] as LPCValue;

        int selfHpRate = Game.Divided(petOb.Query<int>("hp"), petOb.QueryAttrib("max_hp"), 1000);

        int selfMp = petOb.Query<int>("mp");

        LPCMapping fetchMap = new LPCMapping();
        int maxHpValue = 0;
        int maxMpValue = 0;
        int mpSign = 0;
        // 获取角色列表
        List<Property> targetList = RoundCombatMgr.GetPropertyList(petOb.CampId);

        // 遍历角色
        foreach (Property checkOb in targetList)
        {
            // 如果角色已经死亡不处理
            if (checkOb.CheckStatus("DIED"))
                continue;
            // 如果是自己不处理
            if (Property.Equals(checkOb, petOb))
                continue;

            int tarHpRate = Game.Divided(checkOb.Query<int>("hp"), checkOb.QueryAttrib("max_hp"), 1000);
            int tarMp = checkOb.Query<int>("mp");
            if (tarMp > selfMp)
                mpSign += 1;
            // 对比绝对差值
            maxHpValue = Math.Max(maxHpValue,Math.Abs(selfHpRate-tarHpRate));
            maxMpValue = Math.Max(maxMpValue, Math.Abs(selfMp - tarMp));
        }

        // 如果最大绝对差值小于等于阈值则进行能量判断
        if (maxHpValue <= (selfHpRate / 4))
        {
            if (mpSign > 0)
            {
                fetchMap.Add("fetch_level", 2);
                fetchMap.Add("weight", args.AsInt);
                return fetchMap;
            }

            fetchMap.Add("fetch_level", 0);
            fetchMap.Add("weight", 0);
            return fetchMap;
        }

        fetchMap.Add("fetch_level", 2);
        fetchMap.Add("weight", args.AsInt);
        return fetchMap;
    }
}

/// <summary>
/// 技能目标权重脚本（周天法）
/// </summary>
public class SCRIPT_6323 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int weight = 0;
        int selfHpRate = Game.Divided(sourceOb.Query<int>("hp"), sourceOb.QueryAttrib("max_hp"), 1000);

        int hpValue = Math.Abs(Game.Divided(targetOb.Query<int>("hp"), targetOb.QueryAttrib("max_hp"), 1000) - selfHpRate);
        weight += hpValue + targetOb.Query<int>("mp") * 10;

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 计算技能连击次数（冰雹风暴）
/// </summary>
public class SCRIPT_6324 : Script
{
    public override object Call(params object[] _param)
    {
        //Property sourceOb = _param[0] as Property;
        // LPCValue comboArgs = _param[1] as LPCValue;
        LPCMapping extraPara = _param[2] as LPCMapping;

        // 获取冰冻个数
        LPCArray freezeList = extraPara.GetValue<LPCArray>("freeze_list");
        if (freezeList == null)
            return 0;

        // 根据暴击率计算连击次数
        return freezeList.Count;
    }
}

/// <summary>
/// 技能抽取权重计算脚本（生命、debuff判断）
/// </summary>
public class SCRIPT_6325 : Script
{
    public override object Call(params object[] _param)
    {
        Property petOb = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;

        List<Property> targetList = RoundCombatMgr.GetPropertyList(petOb.CampId);

        LPCArray hpLvList = new  LPCArray();
        LPCArray defDownList = new  LPCArray();
        LPCArray stunList = new  LPCArray();
        LPCArray freezeList = new  LPCArray();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (targetOb.CheckStatus("DIED"))
                continue;
            string targetRid = targetOb.GetRid();
            if (targetOb.QueryTemp<int>("hp_rate") <= 500)
                hpLvList.Add(targetRid);
            if (targetOb.CheckStatus("D_DEF_DOWN"))
                defDownList.Add(targetRid);
            if (targetOb.CheckStatus("D_STUN"))
                stunList.Add(targetRid);
            if (targetOb.CheckStatus("D_FREEZE") || targetOb.CheckStatus("D_FREEZE_START"))
                freezeList.Add(targetRid);
        }

        // 优先判断己方单位生命低于 50% 时是否需要按照权重进行抽取
        // 遍历己方所有单位，获取生命低于 50% 的单位列表
        if (hpLvList.Count > 0 || defDownList.Count > 0 || stunList.Count > 0 || freezeList.Count > 0)
        {
                LPCMapping fetchMap = new LPCMapping();
                fetchMap.Add("fetch_level", 1);
                fetchMap.Add("weight", args.AsInt);
                return fetchMap;
        }

        LPCMapping data = new LPCMapping ();
        data.Add("fetch_level", 0);
        data.Add("weight", 0);
        return data;
    }
}

/// <summary>
/// 技能目标权重脚本（负面生命影响）
/// </summary>
public class SCRIPT_6326 : Script
{
    public override object Call(params object[] _param)
    {
        //Property sourceOb = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int weight = 0;
        int hpRate = targetOb.QueryTemp<int>("hp_rate");

        // 遍历负面状态，每个加 1 
        List<LPCMapping> allStatus = targetOb.GetAllStatus();

        for (int i = 0; i < allStatus.Count; i++)
        {
            // 获取状态的信息
            CsvRow statusInfo = StatusMgr.GetStatusInfo(allStatus[i].GetValue<int>("status_id"));

            // 获取状态的类型：debuff / buff
            int buff_type = statusInfo.Query<int>("status_type");
            if (buff_type ==StatusConst.TYPE_DEBUFF)
                weight += 1;
        }

        // 破甲+5
        if (targetOb.CheckStatus("D_DEF_DOWN"))
            weight += 5;
        // 眩晕+5
        if (targetOb.CheckStatus("D_STUN"))
            weight += 5;
        // 冰冻+5
        if (targetOb.CheckStatus("D_FREEZE") || targetOb.CheckStatus("D_FREEZE_START"))
            weight += 5;

        //生命少于60%+2
        if (hpRate < 600)
            weight += 2;

        //生命少于50%+3
        if (hpRate < 500)
            weight += 3;

        //生命少于40%+10
        if (hpRate < 400)
            weight += 10;

        //生命少于30%+10
        if (hpRate < 300)
            weight += 10;

        //生命少于20%+10
        if (hpRate < 200)
            weight += 10;

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 技能抽取权重计算脚本（血量判断，自身判断）
/// </summary>
public class SCRIPT_6327 : Script
{
    public override object Call(params object[] _param)
    {
        Property petOb = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;
        //LPCValue fetchType = _param[2] as LPCValue;

        int minRate = 1000;

        // 获取角色当前血量情况
        int maxHp = petOb.QueryAttrib("max_hp");
        int hp = petOb.Query<int>("hp");

        // 血量低于20%直接释放无视抽取权重
        minRate = Math.Min(minRate, Game.Divided(hp, maxHp));

        // 抽取结果
        LPCMapping fetchMap = new LPCMapping();

        // 根据minRate选择权重
        if (minRate <= 500)
        {
            // 血量低于50%才起效
            fetchMap.Add("fetch_level", 1);
            fetchMap.Add("weight", args.AsInt);
        }
        else if (minRate <= 200)
        {
            // 血量低于20%直接释放无视抽取权重
            fetchMap.Add("fetch_level", 2);
            fetchMap.Add("weight", args.AsInt);
        }
        else
        {
            // 其他情况不能抽取
            fetchMap.Add("fetch_level", 0);
            fetchMap.Add("weight", 0);
        }

        // 返回数据
        return fetchMap;
    }
}

/// <summary>
/// 技能抽取权重计算脚本（输血治疗专用）
/// </summary>
public class SCRIPT_6328 : Script
{
    public override object Call(params object[] _param)
    {
        Property petOb = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;
        //LPCValue fetchType = _param[2] as LPCValue;

        LPCArray checkList = new LPCArray();
        List<Property> targetList = RoundCombatMgr.GetPropertyList(petOb.CampId);

        // 遍历角色
        foreach (Property checkOb in targetList)
        {
            // 如果角色已经死亡不处理
            if (checkOb.CheckStatus("DIED"))
                continue;
            // 排除自身目标
            if (checkOb.Equals(petOb))
                continue;
            if (checkOb.QueryTemp<int>("hp_rate") <= 400)
                checkList.Add(checkOb.GetRid());
        }

        // 抽取结果
        LPCMapping fetchMap = new LPCMapping();

        if (petOb.QueryTemp<int>("hp_rate") > 250 && checkList.Count != 0)
        {
            fetchMap.Add("fetch_level", 2);
            fetchMap.Add("weight", args.AsInt);
        } else
        {
            // 其他情况不能抽取
            fetchMap.Add("fetch_level", 0);
            fetchMap.Add("weight", 0);
        }

        // 返回数据
        return fetchMap;
    }
}

/// <summary>
/// 技能目标权重脚本（输血治疗专用）
/// </summary>
public class SCRIPT_6329 : Script
{
    public override object Call(params object[] _param)
    {
        //Property sourceOb = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int weight = 0;
        int hpRate = targetOb.QueryTemp<int>("hp_rate");

        //生命少于40%+5
        if (hpRate < 400)
            weight += 5;
        //生命少于30%+5
        if (hpRate < 300)
            weight += 5;
        //生命少于20%+5
        if (hpRate < 200)
            weight += 5;
        //生命少于10%+5
        if (hpRate < 100)
            weight += 5;

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 普通攻击技能抽取权重计算脚本（黑血吊针状态下的普攻专用）
/// </summary>
public class SCRIPT_6330 : Script
{
    public override object Call(params object[] _param)
    {
        Property petOb = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;

        LPCMapping fetchMap = new LPCMapping();

        if (petOb.CheckStatus("B_FURY"))
        {
            fetchMap.Add("fetch_level", 3);
            fetchMap.Add("weight", args.AsInt);
        } else
        {
            // 其他情况不能抽取
            fetchMap.Add("fetch_level", 0);
            fetchMap.Add("weight", args.AsInt);
        }

        return fetchMap;
    }
}

/// <summary>
/// 特殊辅助技能抽取权重计算脚本（上紧发条专用）
/// </summary>
public class SCRIPT_6331 : Script
{
    public override object Call(params object[] _param)
    {
        Property petOb = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;

        LPCMapping fetchMap = new LPCMapping();

        List<LPCMapping> statuList = petOb.GetStatusCondition("B_DMG_UP");
        int hpRate = petOb.QueryTemp<int>("hp_rate");

        if (statuList.Count < 3 && hpRate > 200)
        {
            fetchMap.Add("fetch_level", 2);
            fetchMap.Add("weight", args.AsInt);
        } else
        {
            // 其他情况不能抽取
            fetchMap.Add("fetch_level", 0);
            fetchMap.Add("weight", 0);
        }

        return fetchMap;
    }
}

/// <summary>
/// 特殊辅助技能抽取权重计算脚本（死亡伪装专用）
/// </summary>
public class SCRIPT_6332 : Script
{
    public override object Call(params object[] _param)
    {
        Property petOb = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;

        List<Property> targetList = RoundCombatMgr.GetPropertyList(petOb.CampId);

        List<Property> finalList = new List<Property>();

        // 筛选非死亡单位
        foreach (Property targetOb in targetList)
        {
            if (!targetOb.CheckStatus("DIED"))
                finalList.Add(targetOb);
        }

        LPCMapping fetchMap = new LPCMapping();

        if (targetList.Count >= 2)
        {
            fetchMap.Add("fetch_level", 2);
            fetchMap.Add("weight", args.AsInt);
        } else
        {
            // 其他情况不能抽取
            fetchMap.Add("fetch_level", 0);
            fetchMap.Add("weight", 0);
        }

        return fetchMap;
    }
}

/// <summary>
/// 技能目标权重脚本（暂时梦魇降临特殊）
/// </summary>
public class SCRIPT_6333 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        Property targetOb = _param[1] as Property;
        // LPCValue args = _param[2] as LPCValue;
        int weight = 0;
        int hpRate = Game.Divided(targetOb.Query<int>("hp"), targetOb.QueryAttrib("max_hp"));
        int restrain = ElementMgr.GetMonsterCounter(sourceOb, targetOb); 

        // 克制目标+10
        if (restrain == ElementConst.ELEMENT_ADVANTAGE)
            weight += 10;

        // 无克制+5
        if (restrain == ElementConst.ELEMENT_NEUTRAL)
            weight += 5;

        // 破甲效果+15
        if (targetOb.CheckStatus("D_DEF_DOWN"))
            weight += 15;

        // 伤害加深+10
        if (targetOb.CheckStatus("D_MARK"))
            weight += 10;

        // 生命少于40%+5
        if (hpRate < 400)
            weight += 5;

        //生命少于20%+5
        if (hpRate < 200)
            weight += 5;

        // 检查昏睡目标
        if (targetOb.CampId != sourceOb.CampId && targetOb.CheckStatus("D_SLEEP"))
            weight += 100;

        // 返回权重数值
        return weight;
    }
}

/// <summary>
/// 计算技能连击次数（概率连击）
/// </summary>
public class SCRIPT_6334 : Script
{
    public override object Call(params object[] _param)
    {
        //Property sourceOb = _param[0] as Property;
        LPCMapping comboArgs = _param[1] as LPCMapping;
        //LPCMapping extraPara = _param[2] as LPCMapping;
        int times = 1;

        do{
            // 概率不满足需求
            if (RandomMgr.GetRandom() > comboArgs.GetValue<int>("rate"))
                break;

            // 累积次数
            times++;

        } while(true);

        times = Math.Min(times, comboArgs.GetValue<int>("max_combo"));

        // 根据暴击率计算连击次数
        return times;
    }
}

/// <summary>
/// 计算技能连击次数（审判之击，平均随机N个连击次数）
/// </summary>
public class SCRIPT_6335 : Script
{
    public override object Call(params object[] _param)
    {
        //Property sourceOb = _param[0] as Property;
        LPCMapping comboArgs = _param[1] as LPCMapping;
        //LPCMapping extraPara = _param[2] as LPCMapping;

        LPCArray rate_list = comboArgs.GetValue<LPCArray>("rate_list");

        int rate = RandomMgr.GetRandom();

        int comboTimes = 0;

        for(int i = 0; i < rate_list.Count; i++)
        {
            LPCMapping rateMap = rate_list[i].AsMapping;
            if (rate < rateMap.GetValue<int>("c_rate"))
                comboTimes = rateMap.GetValue<int>("c_times");
        }

        return comboTimes;
    }
}

/// <summary>
/// 深渊追击连击检查脚本(检查每下连击是否该执行)
/// </summary>
public class SCRIPT_6336 : Script
{
    public override object Call(params object[] _param)
    {
        Property sourceOb = _param[0] as Property;
        int skillId = (int)_param[1];
        LPCMapping comboArgs = (_param[2] as LPCValue).AsMapping;
        LPCMapping extraPara = _param[3] as LPCMapping;

        int comboTimes = extraPara.GetValue<int>("combo_times");

        // pickOb对象不存在
        Property pickOb = Rid.FindObjectByRid(extraPara.GetValue<string>("pick_rid"));
        if (pickOb == null)
            return false;

        // 如果pickOb对象当前血量小于0或者有不能选择属性
        if (pickOb.Query<int>("hp") <= 0 ||
            pickOb.QueryTemp<int>("halo/halo_can_not_choose") != 0)
            return false;

        // 概率和能量检查
        switch (comboTimes)
        {
            case 0:
                return true;
            case 1:
                {
                    //如果有深渊追击加成属性，需要附加额外概率
                    int specAddRate = 0;
                    if (sourceOb.QueryAttrib("add_min_rate") > 0)
                    {
                        // 获取当前血量情况
                        int hpRate = pickOb.QueryTemp<int>("hp_rate");

                        // 根据当前血量计算概率加成
                        if (hpRate <= sourceOb.QueryAttrib("add_min_rate"))
                            specAddRate = 1000;
                        else
                            specAddRate = (1000 - hpRate) * sourceOb.QueryAttrib("lost_add_rate") / 100;
                    }

                    // 获取原始技能id
                    int originalSkillId = SkillMgr.GetOriginalSkillId(skillId);

                    // 取概率检查值
                    // 计算技能升级效果
                    int checkRate = specAddRate +
                        comboArgs.GetValue<int>("rate_check") +
                        CALC_SKILL_UPGRADE_EFFECT.Call(originalSkillId, sourceOb.GetSkillLevel(originalSkillId), SkillType.SE_RATE);

                    // 概率满足和mp满足条件，则允许释放
                    if (RandomMgr.GetRandom() < checkRate &&
                        sourceOb.Query<int>("mp") >= comboArgs.GetValue<int>("mp_check"))
                        return true;

                    return false;
                }
            default :
                return false;
        }
    }
}

/// <summary>
/// 棱镜射击连击检查脚本(检查每下连击是否该执行)
/// </summary>
public class SCRIPT_6337 : Script
{
    public override object Call(params object[] _param)
    {
        //Property sourceOb = _param[0] as Property;
        //int skillId = (int)_param[1];
        //LPCMapping comboArgs = (_param[2] as LPCValue).AsMapping;
        LPCMapping extraPara = _param[3] as LPCMapping;

        Property pickOb = Rid.FindObjectByRid(extraPara.GetValue<string>("pick_rid"));

        // 筛选目标
        List<Property> targetList = RoundCombatMgr.GetPropertyList(pickOb.CampId,
            new List<string>(){ "DIED" , "B_CAN_NOT_CHOOSE", });

        LPCArray finalList = new LPCArray();

        foreach (Property checkOb in targetList)
        {
            if (checkOb.Query<int>("hp") > 0 && checkOb.QueryTemp<int>("halo/halo_can_not_choose") == 0)
                finalList.Add(checkOb.GetRid());
        }

        if (finalList.Count == 0)
            return false;

        int comboTimes = extraPara.GetValue<int>("combo_times");

        LPCMapping costData = extraPara.GetValue<LPCMapping>("skill_cost");
        int costMp = costData.GetValue<int>("mp");

        if (comboTimes >= costMp)
            return false;

        return true;
    }
}

/// <summary>
/// 技能抽取权重计算脚本（己方队友血量百分比判断，聚能爆炸）
/// </summary>
public class SCRIPT_6338 : Script
{
    public override object Call(params object[] _param)
    {
        Property petOb = _param[0] as Property;
        LPCValue args = _param[1] as LPCValue;
        //LPCValue fetchType = _param[2] as LPCValue;

        LPCMapping fetchMap = new LPCMapping();

        // 获取角色列表
        List<Property> targetList = RoundCombatMgr.GetPropertyList(petOb.CampId);
        LPCArray hpTarList = new LPCArray();
        // 遍历角色
        foreach (Property checkOb in targetList)
        {
            // 如果角色已经死亡不处理
            if (checkOb.CheckStatus("DIED"))
                continue;
            if (checkOb.QueryTemp<int>("hp_rate") <= 300)
                hpTarList.Add(checkOb.GetRid());
        }

        // 如果有生命小于30%目标则抽取该技能
        if (hpTarList.Count > 0)
        {
            fetchMap.Add("fetch_level", 2);
            fetchMap.Add("weight", args.AsInt);
            return fetchMap;
        }
        // 否则默认不抽取该技能
        fetchMap.Add("fetch_level", -1);
        fetchMap.Add("weight", 0);
        return fetchMap;
    }
}