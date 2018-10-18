
/// <summary>
/// SCRIPT_10000.cs
/// Created by zhaozy 2014-12-25
/// 光环相关脚本
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using LPC;
using UnityEngine;

// 受创动作计算
public class SCRIPT_10000 : Script
{
    public override object Call(params object[] _params)
    {
        // 获取角色对象
        LPCMapping args = _params[0] as LPCMapping;
        string targetRid = args.GetValue<string>("target_rid");
        if (string.IsNullOrEmpty(targetRid))
            return MixedValue.NewMixedValue<string>("Damaged");

        Property ob = Rid.FindObjectByRid(targetRid);

        // 根据角色当前状态，区分受创动作
        if (ob.CheckStatus("DIED"))
            return MixedValue.NewMixedValue<string>("Die");
        else
            return MixedValue.NewMixedValue<string>("Damaged");
    }
}

/// <summary>
/// SCRIP t 10001.
/// </summary>
public class SCRIPT_10001 : Script
{
    public override object Call(params object[] _params)
    {
        // 获取角色对象
        LPCMapping args = _params[0] as LPCMapping;

        // 获取移动的目标位置
        LPCArray pos = args.GetValue<LPCArray>("target_pos");

        // 返回目标位置
        return MixedValue.NewMixedValue<Vector3>(new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat));
    }
}

/// <summary>
/// 光效的通用固定位置计算脚本（按照双方阵营不同位置不同，敌方目标为基准）
/// </summary>
public class SCRIPT_10002 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping args = _params[0] as LPCMapping;
        string targetRid = args.GetValue<string>("rid");
        Property ob;

        if (string.IsNullOrEmpty(targetRid) ||
            (ob = Rid.FindObjectByRid(targetRid)) == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        // 根据阵营返回目标位置
        if (ob.CampId == CampConst.CAMP_TYPE_ATTACK)
            return MixedValue.NewMixedValue<Vector3>(new Vector3(3.0f, -1.0f, -0.25f));
        else
            return MixedValue.NewMixedValue<Vector3>(new Vector3(-3.0f, -1.0f, -0.25f));
    }
}

// 接近目标
public class SCRIPT_10003 : Script
{
    public override object Call(params object[] _params)
    {
        // 去角色身上查询受创偏移字段
        LPCMapping args = _params[0] as LPCMapping;

        // 获取被攻击者对象
        CombatActor damageOb = CombatActorMgr.GetCombatActor(args["pick_rid"].AsString);
        if (damageOb == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        // 获取攻击者对象
        CombatActor ob = CombatActorMgr.GetCombatActor(args["rid"].AsString);
        if (ob == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        // 获取被攻击者位置
        Vector3 pos = damageOb.GetPosition();
        float rang = 0.0f;

        // 根据攻击者和被攻击者体型半径计算落点位置
        if (damageOb.GetPositionX() > ob.GetPositionX())
            rang = -damageOb.GetBodyRang() - ob.GetBodyRang();
        else
            rang = damageOb.GetBodyRang() + ob.GetBodyRang();

        // 返回目标位置
        // 这个地方z多加0.1纯粹是为了表现，让攻击者层级在被攻击者上面
        return MixedValue.NewMixedValue<Vector3>(new Vector3(pos.x + rang, pos.y, pos.z + 0.1f * damageOb.GetDirectoion2D()));
    }
}

// 归位
public class SCRIPT_10004 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping args = _params[0] as LPCMapping;
        string targetRid = args.GetValue<string>("rid");
        Property ob;

        if (string.IsNullOrEmpty(targetRid) ||
            (ob = Rid.FindObjectByRid(targetRid)) == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        // 返回目标位置
        return MixedValue.NewMixedValue<Vector3>(ob.MoveBackPos);
    }
}

// 通用受创光效计算脚本
public class SCRIPT_10005 : Script
{
    public override object Call(params object[] _params)
    {
        //int damageType = args["damage_type"].AsInt;
        LPCMapping args = _params[0] as LPCMapping;
        LPCMapping sourceInfo = args["source_profile"].AsMapping;
        LPCValue value = (_params[1] as MixedValue).GetValue<LPCValue>();

        int element = sourceInfo.GetValue<int>("element");

        switch (element)
        {
            case MonsterConst.ELEMENT_LIGHT:
                return MixedValue.NewMixedValue<string>(value.AsArray[0].AsString); // 光系受创
            case MonsterConst.ELEMENT_DARK:
                return MixedValue.NewMixedValue<string>(value.AsArray[1].AsString); // 暗系受创
            case MonsterConst.ELEMENT_FIRE:
                return MixedValue.NewMixedValue<string>(value.AsArray[2].AsString); // 火系受创
            case MonsterConst.ELEMENT_STORM:
                return MixedValue.NewMixedValue<string>(value.AsArray[3].AsString); // 风系受创
            case MonsterConst.ELEMENT_WATER:
                return MixedValue.NewMixedValue<string>(value.AsArray[4].AsString); // 水系受创
            default:
                return MixedValue.NewMixedValue<string>(value.AsArray[0].AsString); // 默认受创
        }

    }
}

// 通用穿刺命中光效计算脚本
public class SCRIPT_10006 : Script
{
    public override object Call(params object[] _params)
    {
        //int damageType = args["damage_type"].AsInt;
        LPCMapping args = _params[0] as LPCMapping;
        string rid = args.GetValue<string>("rid");
        Property sourceOb = Rid.FindObjectByRid(rid);
        LPCValue value = (_params[1] as MixedValue).GetValue<LPCValue>();

        int element = sourceOb.Query<int>("element");

        switch (element)
        {
            case MonsterConst.ELEMENT_LIGHT:
                return MixedValue.NewMixedValue<string>(value.AsArray[0].AsString); // 光系穿刺
            case MonsterConst.ELEMENT_DARK:
                return MixedValue.NewMixedValue<string>(value.AsArray[1].AsString); // 暗系穿刺
            case MonsterConst.ELEMENT_FIRE:
                return MixedValue.NewMixedValue<string>(value.AsArray[2].AsString); // 火系穿刺
            case MonsterConst.ELEMENT_STORM:
                return MixedValue.NewMixedValue<string>(value.AsArray[3].AsString); // 风系穿刺
            case MonsterConst.ELEMENT_WATER:
                return MixedValue.NewMixedValue<string>(value.AsArray[4].AsString); // 水系穿刺
            default:
                return MixedValue.NewMixedValue<string>(value.AsArray[0].AsString); // 默认穿刺
        }
    }
}

/// <summary>
/// SCRIP t 10007.
/// </summary>
public class SCRIPT_10007 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping args = _params[0] as LPCMapping;
        string targetRid = args.GetValue<string>("rid");
        Property ob;

        if (string.IsNullOrEmpty(targetRid) ||
            (ob = Rid.FindObjectByRid(targetRid)) == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        // 返回目标位置
        return MixedValue.NewMixedValue<Vector3>(ob.GetPosition());
    }
}

/// <summary>
/// SCRIP t 10008.
/// </summary>
public class SCRIPT_10008 : Script
{
    public override object Call(params object[] _params)
    {
        // 去角色身上查询受创偏移字段
        LPCMapping args = _params[0] as LPCMapping;

        // 获取被攻击者对象
        CombatActor damageOb = CombatActorMgr.GetCombatActor(args["pick_rid"].AsString);
        if (damageOb == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        // 碰撞盒中心位置
        Vector3 pos = damageOb.GetBoxColiderPos();

        // 返回目标位置
        // 这个地方z多加0.1纯粹是为了表现，让攻击者层级在被攻击者上面
        return MixedValue.NewMixedValue<Vector3>(new Vector3(pos.x, pos.y, pos.z + 0.1f * damageOb.GetDirectoion2D()));
    }
}

// 通用护盾受创光效计算脚本
public class SCRIPT_10009 : Script
{
    private static List<string> statusList = new List<string>(){ "B_HP_SHIELD", "B_EQPT_SHIELD", "B_DMG_IMMUNE", "B_NO_DIE" };

    public override object Call(params object[] _params)
    {
        //int damageType = args["damage_type"].AsInt;
        LPCMapping args = _params[0] as LPCMapping;
        //LPCMapping sourceInfo = args["source_profile"].AsMapping;
        LPCValue value = (_params[1] as MixedValue).GetValue<LPCValue>();

        // 如果存在护盾情况下，播放护盾光效
        string targetRid = args.GetValue<string>("rid");
        Property targetOb = Rid.FindObjectByRid(targetRid);
        if (targetOb == null)
            return MixedValue.NewMixedValue<string>(string.Empty);

        if (targetOb.CheckStatus(statusList))
            return MixedValue.NewMixedValue<string>(value.AsString);

        return MixedValue.NewMixedValue<string>(string.Empty);
    }
}

/// <summary>
/// 通用护盾受创光效缩放控制脚本
/// </summary>
public class SCRIPT_10010 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping args = _params[0] as LPCMapping;
        Property targetOb = Rid.FindObjectByRid(args.GetValue<string>("rid"));

        // 获取角色的模型缩放
        LPCValue modleScale = targetOb.Query<LPCValue>("scale");

        // 没有配置模型缩放，默认是1
        if (modleScale == null)
            return MixedValue.NewMixedValue<Vector3>(new Vector3(1.5f, 1.5f, 1.5f));

        float scale = 1.5f * modleScale.AsFloat;
        return MixedValue.NewMixedValue<Vector3>(new Vector3(scale, scale, scale));
    }
}

/// <summary>
/// 角色的通用固定位置计算脚本（按照双方阵营不同位置不同）
/// </summary>
public class SCRIPT_10011 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping args = _params[0] as LPCMapping;
        string targetRid = args.GetValue<string>("rid");
        Property ob;

        if (string.IsNullOrEmpty(targetRid) ||
            (ob = Rid.FindObjectByRid(targetRid)) == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        // 根据阵营返回目标位置
        if (ob.CampId == CampConst.CAMP_TYPE_ATTACK)
            return MixedValue.NewMixedValue<Vector3>(new Vector3(3.0f, -1.0f, 0.0f));
        else
            return MixedValue.NewMixedValue<Vector3>(new Vector3(-3.0f, -1.0f, 0.0f));
    }
}

/// <summary>
/// 光效位置计算脚本
/// </summary>
public class SCRIPT_10012 : Script
{
    public override object Call(params object[] _params)
    {
        // 去角色身上查询受创偏移字段
        LPCMapping args = _params[0] as LPCMapping;

        // 获取被攻击者对象
        CombatActor damageOb = CombatActorMgr.GetCombatActor(args["rid"].AsString);
        if (damageOb == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        float offsetY = damageOb.GetHpbarOffestY();

        // 返回目标位置
        // 这个地方z多加0.1纯粹是为了表现，让攻击者层级在被攻击者上面
        return MixedValue.NewMixedValue<Vector3>(new Vector3(0, offsetY, 0));
    }
}

/// <summary>
/// 光球落点固定位置计算脚本（按照双方阵营不同位置不同）
/// </summary>
public class SCRIPT_10013 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping args = _params[0] as LPCMapping;
        string targetRid = args.GetValue<string>("rid");
        Property ob;

        if (string.IsNullOrEmpty(targetRid) ||
            (ob = Rid.FindObjectByRid(targetRid)) == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        // 根据阵营返回目标位置
        if (ob.CampId == CampConst.CAMP_TYPE_ATTACK)
            return MixedValue.NewMixedValue<Vector3>(new Vector3(2.0f, -1.0f, 0.0f));
        else
            return MixedValue.NewMixedValue<Vector3>(new Vector3(-2.0f, -1.0f, 0.0f));
    }
}

/// <summary>
/// 闪电风暴落点固定位置计算脚本（按照双方阵营不同位置不同）
/// </summary>
public class SCRIPT_10014 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping args = _params[0] as LPCMapping;
        string targetRid = args.GetValue<string>("rid");
        Property ob;

        if (string.IsNullOrEmpty(targetRid) ||
            (ob = Rid.FindObjectByRid(targetRid)) == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        // 根据阵营返回目标位置
        if (ob.CampId == CampConst.CAMP_TYPE_ATTACK)
            return MixedValue.NewMixedValue<Vector3>(new Vector3(1.0f, -1.5f, 0.0f));
        else
            return MixedValue.NewMixedValue<Vector3>(new Vector3(-1.0f, -1.5f, 0.0f));
    }
}

/// <summary>
/// 受创光效位置计算脚本（中心）
/// </summary>
public class SCRIPT_10015 : Script
{
    public override object Call(params object[] _params)
    {
        // 去角色身上查询受创偏移字段
        LPCMapping args = _params[0] as LPCMapping;

        // 获取被攻击者对象
        CombatActor damageOb = CombatActorMgr.GetCombatActor(args["actor_name"].AsString);
        if (damageOb == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        //int direct = damageOb.GetDirectoion2D();

        // 碰撞盒中心位置
        Vector3 pos = damageOb.GetBoxColiderPos();

        // 返回目标位置
        // 这个地方z多加0.1纯粹是为了表现，让攻击者层级在被攻击者上面
        return MixedValue.NewMixedValue<Vector3>(new Vector3(pos.x, pos.y, pos.z - 0.1f));
        
    }
}

// 随机飞行轨迹计算脚本
public class SCRIPT_10016 : Script
{
    public override object Call(params object[] _params)
    {
        //LPCMapping args = _params[0] as LPCMapping;
        LPCValue value = (_params[1] as MixedValue).GetValue<LPCValue>();

        // 返回目标位置
        return MixedValue.NewMixedValue<string>(value.AsArray[new System.Random().Next(0, value.AsArray.Count)].AsString);
    }
}

/// <summary>
/// 激光类目标光效位置计算脚本
/// </summary>
public class SCRIPT_10017 : Script
{
    public override object Call(params object[] _params)
    {
        // 去角色身上查询受创偏移字段
        LPCMapping args = _params[0] as LPCMapping;

        // 获取被攻击者对象
        CombatActor sourceOb = CombatActorMgr.GetCombatActor(args["rid"].AsString);
        if (sourceOb == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        int direct = sourceOb.GetDirectoion2D();
        float xfloat = 0;
        // 返回目标位置
        if (direct == ObjectDirection2D.LEFT)
            xfloat = -3.9f;
        else
            xfloat = 3.9f;

        return MixedValue.NewMixedValue<Vector3>(new Vector3(xfloat, 0, 0));
    }
}

/// <summary>
/// 死亡动作播放计算脚本
/// </summary>
public class SCRIPT_10018 : Script
{
    public override object Call(params object[] _params)
    {
        // 去角色身上查询受创偏移字段
        LPCMapping args = _params[0] as LPCMapping;

        Property ob = Rid.FindObjectByRid(args["actor_name"].AsString);

        if (ob == null || ob.Query<LPCValue>("die_action") == null)
            return MixedValue.NewMixedValue<string>(string.Empty);

        return MixedValue.NewMixedValue<string>(ob.Query<LPCValue>("die_action").AsString);
    }
}

/// <summary>
/// 受创光效位置计算脚本(脚底)
/// </summary>
public class SCRIPT_10019 : Script
{
    public override object Call(params object[] _params)
    {
        // 去角色身上查询受创偏移字段
        LPCMapping args = _params[0] as LPCMapping;

        // 获取被攻击者对象
        CombatActor damageOb = CombatActorMgr.GetCombatActor(args["actor_name"].AsString);
        if (damageOb == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        //int direct = damageOb.GetDirectoion2D();

        // 碰撞盒中心位置
        Vector3 pos = damageOb.GetPosition();

        // 返回目标位置
        // 这个地方z多加0.1纯粹是为了表现，让攻击者层级在被攻击者上面
        return MixedValue.NewMixedValue<Vector3>(new Vector3(pos.x, pos.y, pos.z - 0.1f));

    }
}

// 攻击降低光效计算脚本
public class SCRIPT_10020 : Script
{
    public override object Call(params object[] _params)
    {
        //int damageType = args["damage_type"].AsInt;
        LPCMapping args = _params[0] as LPCMapping;
        LPCValue value = (_params[1] as MixedValue).GetValue<LPCValue>();

        LPCMapping condition = args.GetValue<LPCMapping>("condition");
        if (condition == null)
            return MixedValue.NewMixedValue<string>(value.AsArray[0].AsString); // 普通攻击降低受创
        LPCMapping sourceProfile = condition.GetValue<LPCMapping>("source_profile");
        if (sourceProfile == null)
            return MixedValue.NewMixedValue<string>(value.AsArray[0].AsString); // 普通攻击降低受创
        int skillId = sourceProfile.GetValue<int>("skill_id");

        if (skillId == 5102)
            return MixedValue.NewMixedValue<string>(value.AsArray[1].AsString); // 特殊攻击降低受创

        return MixedValue.NewMixedValue<string>(value.AsArray[0].AsString); // 普通攻击降低受创
    }
}

/// <summary>
/// 需要区分前后排的激光类目标光效位置计算脚本
/// </summary>
public class SCRIPT_10021 : Script
{
    public override object Call(params object[] _params)
    {
        // 去角色身上查询受创偏移字段
        LPCMapping args = _params[0] as LPCMapping;
        Property targetOb = Rid.FindObjectByRid(args["pick_rid"].AsString);
        if (targetOb == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);
        // 获取被攻击者对象
        CombatActor sourcetOb = CombatActorMgr.GetCombatActor(args["rid"].AsString);
        if (sourcetOb == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        int direct = sourcetOb.GetDirectoion2D();
        float xfloat = 0;
        // 返回目标位置
        if (direct == ObjectDirection2D.LEFT)
        {
            if (targetOb.FormationRaw == FormationConst.RAW_FRONT)
                xfloat = -2.0f;
            else
                xfloat = -5.2f;
        }
        else
        {
            if (targetOb.FormationRaw == FormationConst.RAW_FRONT)
                xfloat = 2.0f;
            else
                xfloat = 5.2f;
        }

        return MixedValue.NewMixedValue<Vector3>(new Vector3(xfloat, 0, 0));
    }
}

/// <summary>
/// 需要区分前后排的激光类目标光效位置计算脚本(优先攻击前排目标)
/// </summary>
public class SCRIPT_10022 : Script
{
    public override object Call(params object[] _params)
    {
        // 去角色身上查询受创偏移字段
        LPCMapping args = _params[0] as LPCMapping;
        Property targetOb = Rid.FindObjectByRid(args["pick_rid"].AsString);
        if (targetOb == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);
        // 获取被攻击者对象
        CombatActor sourcetOb = CombatActorMgr.GetCombatActor(args["rid"].AsString);
        if (sourcetOb == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        // 整理筛选结果，将手选结果放在第一个
        List<Property> petList = new List<Property>();
        List<Property> finalList = RoundCombatMgr.GetPropertyList(targetOb.CampId,
            new List<string>(){ "DIED" },
            new List<string>(){ args["pick_rid"].AsString });

        // 筛选前后排信息
        foreach (Property ob in finalList)
        {
            if (ob.FormationRaw == FormationConst.RAW_FRONT)
                petList.Add(ob);
        }

        int direct = sourcetOb.GetDirectoion2D();
        float xfloat = 0;
        // 返回目标位置
        if (direct == ObjectDirection2D.LEFT)
        {
            if (petList.Count == 0)
                xfloat = -5.2f;
            else
                xfloat = -2.0f;
        }
        else
        {
            if (petList.Count == 0)
                xfloat = 5.2f;
            else
                xfloat = 2.0f;
        }

        return MixedValue.NewMixedValue<Vector3>(new Vector3(xfloat, 0, 0));
    }
}

/// <summary>
/// 接近中心，不计算体型半径
/// </summary>
public class SCRIPT_10023 : Script
{
    public override object Call(params object[] _params)
    {
        // 去角色身上查询受创偏移字段
        LPCMapping args = _params[0] as LPCMapping;

        // 获取被攻击者对象
        CombatActor damageOb = CombatActorMgr.GetCombatActor(args["pick_rid"].AsString);
        if (damageOb == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        // 获取攻击者对象
        CombatActor ob = CombatActorMgr.GetCombatActor(args["rid"].AsString);
        if (ob == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        // 获取被攻击者位置
        Vector3 pos = damageOb.GetPosition();

        // 返回目标位置
        // 这个地方z多加0.1纯粹是为了表现，让攻击者层级在被攻击者上面
        return MixedValue.NewMixedValue<Vector3>(new Vector3(pos.x, pos.y, pos.z + 0.1f * damageOb.GetDirectoion2D()));
    }
}

/// <summary>
/// 死亡动作播放计算脚本
/// </summary>
public class SCRIPT_10024 : Script
{
    public override object Call(params object[] _params)
    {
        // 去角色身上查询受创偏移字段
        LPCMapping args = _params[0] as LPCMapping;

        // 有死亡动作需要保留die_action状态
        Property ob = Rid.FindObjectByRid(args["actor_name"].AsString);
        if (ob != null && ob.Query<LPCValue>("die_action") != null)
            return MixedValue.NewMixedValue<string>(string.Empty);

        // 默认换回idle
        return MixedValue.NewMixedValue<string>(CombatConfig.DEFAULT_PLAY);
    }
}

/// <summary>
/// 光效的通用固定位置(靠后)计算脚本（按照双方阵营不同位置不同）
/// </summary>
public class SCRIPT_10025 : Script
{
	public override object Call(params object[] _params)
	{
		LPCMapping args = _params[0] as LPCMapping;
		string targetRid = args.GetValue<string>("rid");
		Property ob;

		if (string.IsNullOrEmpty(targetRid) ||
			(ob = Rid.FindObjectByRid(targetRid)) == null)
			return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

		// 根据阵营返回目标位置
		if (ob.CampId == CampConst.CAMP_TYPE_ATTACK)
			return MixedValue.NewMixedValue<Vector3>(new Vector3(4.0f, -1.0f, 0.0f));
		else
			return MixedValue.NewMixedValue<Vector3>(new Vector3(-4.0f, -1.0f, 0.0f));
	}
}

/// <summary>
/// 受创光效位置计算脚本（目标中心）（冰雹风暴）
/// </summary>
public class SCRIPT_10026 : Script
{
    public override object Call(params object[] _params)
    {
        // 选取冰冻目标位置
        LPCMapping extraArg = _params[0] as LPCMapping;

        LPCArray freezeList = extraArg.GetValue<LPCArray>("freeze_list");

        foreach (LPCValue rid in freezeList.Values)
        {
            Property freezeOb = Rid.FindObjectByRid(rid.AsString);
            if (!freezeOb.CheckStatus("D_FREEZE"))
                continue;
            CombatActor combatFreezeOb = CombatActorMgr.GetCombatActor(rid.AsString);
            Vector3 pos = combatFreezeOb.GetBoxColiderPos();
            return MixedValue.NewMixedValue<Vector3>(new Vector3(pos.x, pos.y, pos.z - 0.1f));
        }

        return MixedValue.NewMixedValue<Vector3>(Vector3.zero);
    }
}

// 攻击节点，攻击目标计算脚本
public class SCRIPT_10027 : Script
{
    public override object Call(params object[] _params)
    {
        // 去角色身上查询受创偏移字段
        LPCMapping args = _params[0] as LPCMapping;

        return MixedValue.NewMixedValue<string>(args["pick_rid"].AsString);
    }
}

/// <summary>
/// 光效的通用固定位置计算脚本（按照双方阵营不同位置不同，己方目标为基准）
/// </summary>
public class SCRIPT_10028 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping args = _params[0] as LPCMapping;
        string targetRid = args.GetValue<string>("rid");
        Property ob;

        if (string.IsNullOrEmpty(targetRid) ||
            (ob = Rid.FindObjectByRid(targetRid)) == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        // 根据阵营返回目标位置
        if (ob.CampId == CampConst.CAMP_TYPE_ATTACK)
            return MixedValue.NewMixedValue<Vector3>(new Vector3(-3.0f, -1.0f, -0.25f));
        else
            return MixedValue.NewMixedValue<Vector3>(new Vector3(3.0f, -1.0f, -0.25f));
    }
}

// 通用释放光效计算脚本（根据属性不同光效不同）
public class SCRIPT_10029 : Script
{
    public override object Call(params object[] _params)
    {
        //int damageType = args["damage_type"].AsInt;
        LPCMapping args = _params[0] as LPCMapping;
        LPCValue value = (_params[1] as MixedValue).GetValue<LPCValue>();

        Property sourceOb = Rid.FindObjectByRid(args.GetValue<string>("rid"));

        int element = sourceOb.Query<int>("element");

        switch (element)
        {
            case MonsterConst.ELEMENT_LIGHT:
                return MixedValue.NewMixedValue<string>(value.AsArray[0].AsString); // 光系受创
            case MonsterConst.ELEMENT_DARK:
                return MixedValue.NewMixedValue<string>(value.AsArray[1].AsString); // 暗系受创
            case MonsterConst.ELEMENT_FIRE:
                return MixedValue.NewMixedValue<string>(value.AsArray[2].AsString); // 火系受创
            case MonsterConst.ELEMENT_STORM:
                return MixedValue.NewMixedValue<string>(value.AsArray[3].AsString); // 风系受创
            case MonsterConst.ELEMENT_WATER:
                return MixedValue.NewMixedValue<string>(value.AsArray[4].AsString); // 水系受创
            default:
                return MixedValue.NewMixedValue<string>(value.AsArray[0].AsString); // 默认受创
        }

    }
}

// 通用重生状态光效计算脚本
public class SCRIPT_10030 : Script
{
    private static List<string> statusList = new List<string>(){ "D_NO_REBORN", };

    public override object Call(params object[] _params)
    {
        //int damageType = args["damage_type"].AsInt;
        LPCMapping args = _params[0] as LPCMapping;
        //LPCMapping sourceInfo = args["source_profile"].AsMapping;
        LPCValue value = (_params[1] as MixedValue).GetValue<LPCValue>();

        string targetRid = args.GetValue<string>("rid");
        Property targetOb = Rid.FindObjectByRid(targetRid);

        if (targetOb.CheckStatus(statusList) && targetOb.QueryAttrib("no_reborn_immu") == 0)
            return MixedValue.NewMixedValue<string>(string.Empty);

        return MixedValue.NewMixedValue<string>(value.AsString);
    }
}

/// <summary>
/// 光效end_pos计算脚本，目标随机变化类的正确位置计算
/// </summary>
public class SCRIPT_10031 : Script
{
    public override object Call(params object[] _params)
    {
        // 去角色身上查询受创偏移字段
        LPCMapping args = _params[0] as LPCMapping;
        LPCMapping hitEntityMap = args.GetValue<LPCMapping>("hit_entity_map");
        // 获取第一个hit点的目标列表的第一个对象rid
        LPCArray targetList = hitEntityMap.GetValue<LPCArray>(0);
        string currentRid = targetList[0].AsString;
        // 获取被攻击者对象
        CombatActor damageOb = CombatActorMgr.GetCombatActor(currentRid);
        if (damageOb == null)
            return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

        // 碰撞盒中心位置
        Vector3 pos = damageOb.GetBoxColiderPos();

        // 返回目标位置
        // 这个地方z多加0.1纯粹是为了表现，让攻击者层级在被攻击者上面
        return MixedValue.NewMixedValue<Vector3>(new Vector3(pos.x, pos.y, pos.z + 0.1f * damageOb.GetDirectoion2D()));
    }
}

// 连击次数计算脚本（获取连击次数，次数决定在单独的6000系列脚本里）
public class SCRIPT_10100 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping args = _params[0] as LPCMapping;

        // 返回次数
        return MixedValue.NewMixedValue<int>(args.GetValue<int>("combo_times"));
    }
}

// 连击次数计算脚本（圣灵召唤）
public class SCRIPT_10101 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping args = _params[0] as LPCMapping;

        // 获取hit目标
        LPCMapping hitEntityMap = args.GetValue<LPCMapping>("hit_entity_map");
        if (hitEntityMap == null)
            return MixedValue.NewMixedValue<int>(0);

        // 没有收集到任何目标
        int comboTimes = 0;
        foreach (LPCValue entityList in hitEntityMap.Values)
        {
            comboTimes += entityList.AsArray.Count;
        }

        // 返回目标位置
        return MixedValue.NewMixedValue<int>(comboTimes);
    }
}

// 连击目标计算脚本（圣灵召唤目标收集）
public class SCRIPT_10102 : Script
{
    public override object Call(params object[] _params)
    {
        // Actor.ActorName, ActionSet.Cookie, mHitIndex, mCurComboTimes, ActionSet.ExtraArg

        int hitIndex = (int)_params[2];

        int curComboTime = (int)_params[3];

        LPCMapping extraArg = _params[4] as LPCMapping;

        // 获取hit目标
        LPCMapping hitEntityMap = extraArg.GetValue<LPCMapping>("hit_entity_map");
        if (hitEntityMap == null || !hitEntityMap.ContainsKey(hitIndex))
            return LPCMapping.Empty;

        LPCMapping newHitEntityMap = LPCMapping.Empty;

        newHitEntityMap.Add(hitIndex, new LPCArray(hitEntityMap.GetValue<LPCArray>(hitIndex)[curComboTime]));

        // 返回目标位置
        return newHitEntityMap;
    }
}

// 连击目标计算脚本（冰雹风暴目标收集）
public class SCRIPT_10103 : Script
{
    public override object Call(params object[] _params)
    {
        // Actor.ActorName, ActionSet.Cookie, mHitIndex, mCurComboTimes, ActionSet.ExtraArg

        //int hitIndex = (int)_params[2];

        //int curComboTime = (int)_params[3];

        LPCMapping extraArg = _params[4] as LPCMapping;

        LPCArray freezeList = extraArg.GetValue<LPCArray>("freeze_list");

        foreach (LPCValue rid in freezeList.Values)
        {
            Property freezeOb = Rid.FindObjectByRid(rid.AsString);
            if (!freezeOb.CheckStatus("D_FREEZE"))
                continue;
            // 每次清除 1 个目标冰冻状态
            freezeOb.ClearStatus("D_FREEZE");
            break;
        }

        // 获取hit目标
        LPCMapping hitEntityMap = extraArg.GetValue<LPCMapping>("hit_entity_map");

        // 返回目标位置
        return hitEntityMap;
    }
}

/// <summary>
/// SCRIP t 10104.
/// </summary>
public class SCRIPT_10104 : Script
{
	public override object Call(params object[] _params)
	{
		// 去角色身上查询受创偏移字段
		LPCMapping args = _params[0] as LPCMapping;

		// 获取被攻击者对象
		CombatActor damageOb = CombatActorMgr.GetCombatActor(args["pick_rid"].AsString);
		if (damageOb == null)
			return MixedValue.NewMixedValue<Vector3>(Vector3.zero);

		// 碰撞盒中心位置
		Vector3 pos = damageOb.GetPosition();

		// 返回目标位置
		// 这个地方z多加0.1纯粹是为了表现，让攻击者层级在被攻击者上面
		return MixedValue.NewMixedValue<Vector3>(new Vector3(pos.x, pos.y, pos.z + 0.1f * damageOb.GetDirectoion2D()));
	}
}

/// <summary>
/// 战斗系统判断是否需要连击检测脚本
/// </summary>
public class SCRIPT_10105 : Script
{
    public override object Call(params object[] _params)
    {
        // Actor.ActorName, ActionSet.Cookie, ActionSet.ExtraArgs
        string rid = _params[0] as string;
        LPCMapping extraArgs = _params[2] as LPCMapping;

        // 查找对象
        Property ob = Rid.FindObjectByRid(rid);
        if (ob == null)
            return false;

        // 返回是否需要连击
        return SkillMgr.CanComboAttack(ob, extraArgs.GetValue<int>("skill_id"), extraArgs);
    }
}

/// <summary>
/// 深渊追击cookie脚本
/// </summary>
public class SCRIPT_10106 : Script
{
    public override object Call(params object[] _params)
    {
        LPCMapping args = _params[0] as LPCMapping;

        string cookie = args.GetValue<string>("original_cookie");

        return MixedValue.NewMixedValue<string>(cookie);
    }
}

// 音效播放计算脚本
public class SCRIPT_11000 : Script
{
    private static List<string> statusList = new List<string>(){ "B_HP_SHIELD", "B_EQPT_SHIELD", "B_DMG_IMMUNE", "B_NO_DIE" };

    public override object Call(params object[] _params)
    {
        LPCMapping args = _params[0] as LPCMapping;
        LPCValue value = (_params[1] as MixedValue).GetValue<LPCValue>();

        // 检查伤害分摊过来的类型是否为分摊伤害，如果是分摊伤害则不再继续进行分摊
        // 如果是分摊伤害，不需要播放音效
        LPCMapping damageMap = args.GetValue<LPCMapping>("damage_map");
        if (damageMap != null && damageMap.ContainsKey("link_id"))
            return MixedValue.NewMixedValue<string>(string.Empty);

        string targetRid = args.GetValue<string>("rid");
        Property targetOb = Rid.FindObjectByRid(targetRid);
        int damageType = args["damage_type"].AsInt;

        // 暴击受创音效
        if ((damageType & CombatConst.DAMAGE_TYPE_DEADLY) == CombatConst.DAMAGE_TYPE_DEADLY)
            return MixedValue.NewMixedValue<string>(value.AsArray[0].AsString);

        // 轻微受创音效
        if ((damageType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
            return MixedValue.NewMixedValue<string>(value.AsArray[1].AsString);

        // 护盾受创音效
        if (targetOb.CheckStatus(statusList))
            return MixedValue.NewMixedValue<string>(value.AsArray[2].AsString);

        int skillId = args.GetValue<LPCMapping>("source_profile").GetValue<int>("skill_id");
        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);
        int soundType = skillInfo.Query<int>("normal_damge_sound");

        if (soundType == CombatConst.SOUND_DAMAGE_MAGIC)
            return MixedValue.NewMixedValue<string>(value.AsArray[3].AsString);
        else
            return MixedValue.NewMixedValue<string>(value.AsArray[4].AsString);
    }
}

/// <summary>
/// 队友受创自身反击光环作用脚本（给光环享受方上触发属性）
/// </summary>
public class SCRIPT_12000 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        LPCArray fixedPropValue = new LPCArray(sourceOb.GetRid(), propValue[0].AsArray[0].AsInt, propValue[0].AsArray[1].AsInt);

        LPCMapping condition = new LPCMapping();
        condition.Add("props", new LPCArray(new LPCArray(104, fixedPropValue)));
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_SPCOUNTER", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友受创自身反击光环清除脚本
/// </summary>
public class SCRIPT_12001 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_SPCOUNTER");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            // 状态来源不同不处理
            if (allStatus[i].GetValue<string>("source_rid") != sourceRid)
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 免疫光环作用脚本（给光环享受方上免疫状态）
/// </summary>
public class SCRIPT_12002 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        LPCMapping condition = new LPCMapping();
        condition.Add("round", propValue);
        condition.Add("halo_sign", 1);

        targetOb.ApplyStatus("B_DEBUFF_IMMUNE", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 免疫光环清除脚本
/// </summary>
public class SCRIPT_12003 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_DEBUFF_IMMUNE");
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            // 如果没有halo_sign标识，不处理
            if (allStatus[i].GetValue<int>("halo_sign") != 1)
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 体力护盾光环作用脚本（给光环享受方上 3 回合体力护盾）
/// </summary>
public class SCRIPT_12004 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;
        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_EQPT_SHIELD");

        int oldAbsorbDmg = 0;

        if (allStatus.Count != 0)
        {
            LPCMapping oldCondition = new LPCMapping();
            oldCondition = allStatus[0];
            oldAbsorbDmg = oldCondition.GetValue<int>("can_absorb_damage");
        }

        string tips = string.Format("{0}{1}", BloodTipMgr.mCureColorDict[TipsWndType_CureTip.HpTip], LocalizationMgr.Get("suit_name_14"));
        BloodTipMgr.AddTip(TipsWndType.CureTip, targetOb, tips);

        LPCMapping condition = new LPCMapping();
        condition.Add("round", 3);
        condition.Add("can_absorb_damage", oldAbsorbDmg + sourceOb.QueryAttrib("max_hp", false) * propValue / 1000);
        condition.Add("halo_sign", 1); 

        targetOb.ApplyStatus("B_EQPT_SHIELD", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 免疫光环清除脚本
/// </summary>
public class SCRIPT_12005 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_EQPT_SHIELD");
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            // 没有halo_sign标识
            if (allStatus[i].GetValue<int>("halo_sign") != 1)
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友受控制，自身上隐忍状态之光环作用脚本
/// </summary>
public class SCRIPT_12006 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        // 参数：光环来源RID、常量
        LPCArray propValue = new LPCArray(sourceOb.GetRid(), (_param[3] as LPCValue).AsInt);

        LPCMapping condition = new LPCMapping();
        condition.Add("props", new LPCArray(new LPCArray(129, propValue)));
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_FORBEAR_COLLECT", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友受控制，自身上隐忍状态之光环清除脚本
/// </summary>
public class SCRIPT_12007 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_FORBEAR_COLLECT");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (allStatus[i].GetValue<string>("source_rid") != sourceRid)
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友攻击死亡标记单位，自身协同攻击光环作用脚本
/// </summary>
public class SCRIPT_12008 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int value = (_param[3] as LPCValue).AsInt;

        // 参数：光环来源RID、隐忍叠加作用起效上限次数
        LPCArray propValue = new LPCArray(sourceOb.GetRid(), value);

        LPCMapping condition = new LPCMapping();
        condition.Add("round", 9999);
        condition.Add("props", new LPCArray(new LPCArray(134, propValue)));
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_MARK_JOINT", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友攻击死亡标记单位，自身协同攻击光环清除脚本
/// </summary>
public class SCRIPT_12009 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_MARK_JOINT");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (allStatus[i].GetValue<string>("source_rid") != sourceRid)
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友受创反射伤害光环作用脚本（给光环享受方上反伤属性）
/// </summary>
public class SCRIPT_12010 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        //Property sourceOb = _param[1] as Property;

        //LPCArray propValue = new LPCArray(sourceOb.GetRid(), (_param[3] as LPCValue).AsInt);

        int rate = (_param[3] as LPCValue).AsInt;

        if (RandomMgr.GetRandom() < rate)
            targetOb.ApplyStatus("B_TEAM_REFLEX", LPCMapping.Empty);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友受创反射伤害清除脚本
/// </summary>
public class SCRIPT_12011 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_TEAM_REFLEX");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (allStatus[i].GetValue<string>("source_rid") != sourceRid)
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友受致命伤代替承伤光环
/// </summary>
public class SCRIPT_12012 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int skillId = (_param[3] as LPCValue).AsArray[0].AsArray[1].AsInt;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("skill_id", skillId);
        statusMap.Add("limit_hp_per", (_param[3] as LPCValue).AsArray[0].AsArray[0].AsInt);

        targetOb.ApplyStatus("B_TEAM_INSTEAD", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友受致命伤代替承伤光环清除脚本
/// </summary>
public class SCRIPT_12013 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_TEAM_INSTEAD");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (allStatus[i].GetValue<string>("source_rid") != sourceRid)
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队攻击提升光环
/// </summary>
public class SCRIPT_12014 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int skillId = (_param[3] as LPCValue).AsInt;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("skill_id", skillId);

        targetOb.ApplyStatus("B_ATK_UP_SP", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队攻击提升光环清除脚本
/// </summary>
public class SCRIPT_12015 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_ATK_UP_SP");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友减伤光环
/// </summary>
public class SCRIPT_12016 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int skillId = (_param[3] as LPCValue).AsInt;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("skill_id", skillId);

        targetOb.ApplyStatus("B_REDUCE_DMG", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友减伤光环清除脚本
/// </summary>
public class SCRIPT_12017 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_REDUCE_DMG");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 聚能光环作用脚本（给光环享受方初始能量+）
/// </summary>
public class SCRIPT_12018 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        int mp = Math.Min(targetOb.QueryAttrib("max_mp") - targetOb.Query<int>("mp"), propValue);
        LPCMapping sourceProfile = targetOb.GetProfile();
        LPCMapping cureMap = new LPCMapping();
        cureMap.Add("mp", mp);
        sourceProfile.Add("skill_id", 11001);
        sourceProfile.Add("cookie", Game.NewCookie(targetOb.GetRid()));

        string tips = string.Format("{0}{1}", BloodTipMgr.mCureColorDict[TipsWndType_CureTip.HpTip], LocalizationMgr.Get("suit_name_13"));
        BloodTipMgr.AddTip(TipsWndType.CureTip, targetOb, tips);

        if (mp != 0)
            BloodTipMgr.AddDamageOrCureTip(targetOb, CombatConst.CURE_TYPE_MAGIC, cureMap);

        (targetOb as Char).ReceiveCure(sourceProfile, CombatConst.CURE_TYPE_MAGIC | CombatConst.CURE_TYPE_ROUND_MAGIC, cureMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 敌方全体致盲光环
/// </summary>
public class SCRIPT_12019 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        LPCArray blindProp = new LPCArray(16, propValue);

        LPCArray propArr = new LPCArray(blindProp);

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", propArr);

        targetOb.ApplyStatus("D_BLIND_HALO", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 敌方全体致盲光环清除脚本
/// </summary>
public class SCRIPT_12020 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("D_BLIND_HALO");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 太阳图腾回合开始自动回血光环
/// </summary>
public class SCRIPT_12021 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("skill_id", propValue[0].AsArray[0].AsInt);
        condition.Add("hp_cure", sourceOb.QueryAttrib("max_hp") * propValue[0].AsArray[1].AsInt / 1000);
        LPCMapping sourceProfile = new LPCMapping ();
        sourceProfile = sourceOb.GetProfile();
        sourceProfile.Add("skill_effect", sourceOb.QueryAttrib("skill_effect"));
        condition.Add("source_profile", sourceProfile);
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_HP_RECOVER_SP", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全体特殊回血光环（全队生命最低的起效）清除脚本
/// </summary>
public class SCRIPT_12022 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_HP_RECOVER_SP");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队基础防御提升光环(对应元素属性)
/// </summary>
public class SCRIPT_12023 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(1001,propValue[0].AsArray[1].AsInt)));

        targetOb.ApplyStatus("B_DEF_UP_LEADER", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队基础防御提升光环清除脚本
/// </summary>
public class SCRIPT_12024 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_DEF_UP_LEADER");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队暴击率提升光环
/// </summary>
public class SCRIPT_12025 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(1002,propValue)));

        targetOb.ApplyStatus("B_CRT_UP_LEADER", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队暴击率提升光环清除脚本
/// </summary>
public class SCRIPT_12026 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_CRT_UP_LEADER");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队效果命中提升光环
/// </summary>
public class SCRIPT_12027 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(1005,propValue)));

        targetOb.ApplyStatus("B_ACC_UP_LEADER", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队效果命中提升光环清除脚本
/// </summary>
public class SCRIPT_12028 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_ACC_UP_LEADER");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队基础攻击提升光环
/// </summary>
public class SCRIPT_12029 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(1000,propValue)));

        targetOb.ApplyStatus("B_ATK_UP_LEADER", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队基础攻击提升光环清除脚本
/// </summary>
public class SCRIPT_12030 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_ATK_UP_LEADER");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队基础敏捷提升光环
/// </summary>
public class SCRIPT_12031 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(1004,propValue)));

        targetOb.ApplyStatus("B_AGI_UP_LEADER", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队基础敏捷提升光环清除脚本
/// </summary>
public class SCRIPT_12032 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_AGI_UP_LEADER");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队基础生命提升光环
/// </summary>
public class SCRIPT_12033 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(1003,propValue)));

        targetOb.ApplyStatus("B_HP_UP_LEADER", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队基础生命提升光环清除脚本
/// </summary>
public class SCRIPT_12034 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_HP_UP_LEADER");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队基础防御提升光环
/// </summary>
public class SCRIPT_12035 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(1001,propValue)));

        targetOb.ApplyStatus("B_DEF_UP_LEADER", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友死亡自身进行复仇攻击光环
/// </summary>
public class SCRIPT_12036 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int skillId = (_param[3] as LPCValue).AsInt;

        // 参数：光环来源RID、隐忍叠加作用起效上限次数
        LPCArray propValue = new LPCArray(sourceOb.GetRid(), skillId);

        LPCMapping condition = new LPCMapping();
        condition.Add("round", 9999);
        condition.Add("props", new LPCArray(new LPCArray(161, propValue)));
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_AVENAGE", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友死亡自身进行复仇攻击光环清除脚本
/// </summary>
public class SCRIPT_12037 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_AVENAGE");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队效果抵抗提升光环
/// </summary>
public class SCRIPT_12038 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(1006,propValue)));

        targetOb.ApplyStatus("B_RES_UP_LEADER", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队效果抵抗提升光环清除脚本
/// </summary>
public class SCRIPT_12039 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_RES_UP_LEADER");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队格挡提升光环
/// </summary>
public class SCRIPT_12040 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(29,propValue)));

        targetOb.ApplyStatus("B_BLOCK_UP_HALO", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队效果抵抗提升光环清除脚本
/// </summary>
public class SCRIPT_12041 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_BLOCK_UP_HALO");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友濒死上状态光环作用脚本
/// </summary>
public class SCRIPT_12042 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propArgs = (_param[3] as LPCValue).AsArray;

        // 参数：光环来源RID、状态别名、状态回合、skillId
        LPCArray propValue = new LPCArray(sourceOb.GetRid(), 
            propArgs[0].AsArray[0].AsString,
            propArgs[0].AsArray[1].AsInt,
            propArgs[0].AsArray[2].AsInt);

        LPCMapping condition = new LPCMapping();
        condition.Add("props", new LPCArray(new LPCArray(165, propValue)));
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_NEAR_DIE_CAST", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友濒死上状态光环清除脚本
/// </summary>
public class SCRIPT_12043 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_NEAR_DIE_CAST");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队基础生命提升光环(对应元素属性)
/// </summary>
public class SCRIPT_12044 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(1003,propValue[0].AsArray[1].AsInt)));

        targetOb.ApplyStatus("B_HP_UP_LEADER", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队基础生命提升光环清除脚本
/// </summary>
public class SCRIPT_12045 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_HP_UP_LEADER");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队元素使魔基础敏捷提升光环
/// </summary>
public class SCRIPT_12046 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsArray[0].AsArray[1].AsInt;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(1004,propValue)));

        targetOb.ApplyStatus("B_AGI_UP_LEADER", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队元素使魔暴击率提升光环
/// </summary>
public class SCRIPT_12047 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsArray[0].AsArray[1].AsInt;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(1002,propValue)));

        targetOb.ApplyStatus("B_CRT_UP_LEADER", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 愤怒的仙人掌光环
/// </summary>
public class SCRIPT_12048 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCArray propArg = propValue[0].AsArray;
        // 自身光环受攻击上状态,属性格式：光环源RID，状态别名，状态数值，skillId，默认该状态无回合限制
        LPCMapping selfStatusMap = new LPCMapping();
        selfStatusMap.Add("source_rid", sourceOb.GetRid());
        selfStatusMap.Add("props", new LPCArray(new LPCArray(170,new LPCArray(sourceOb.GetRid(), propArg[0].AsString, propArg[1].AsInt, propArg[3].AsInt, propArg[4].AsInt))));

        // 队友光环受创上状态,属性格式：光环源RID，状态别名，状态数值，skillId，默认该状态无回合限制
        LPCMapping mateStatusMap = new LPCMapping();
        mateStatusMap.Add("source_rid", sourceOb.GetRid());
        mateStatusMap.Add("props", new LPCArray(new LPCArray(169, new LPCArray(sourceOb.GetRid(), propArg[0].AsString, propArg[2].AsInt, propArg[3].AsInt, propArg[4].AsInt))));

        if (Property.Equals(sourceOb,targetOb))
            targetOb.ApplyStatus("B_HIT_DMGUP_HALO", selfStatusMap);
        else
            targetOb.ApplyStatus("B_DMG_DMGUP_HALO", mateStatusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友濒死上状态光环清除脚本
/// </summary>
public class SCRIPT_12049 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_DMG_DMGUP_HALO");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        if (Property.Equals(sourceOb, targetOb))
            allStatus = targetOb.GetStatusCondition("B_HIT_DMGUP_HALO");

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队最大冷却减少光环
/// </summary>
public class SCRIPT_12050 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        LPCMapping condition = new LPCMapping();
        condition.Add("round", 9999);
        condition.Add("max_reduce_cd", propValue[0].AsArray[1].AsInt);
        condition.Add("source_rid", sourceOb.GetRid());
        condition.Add("skill_id", propValue[0].AsArray[0].AsInt);

        BloodTipMgr.AddSkillTip(targetOb, propValue[0].AsArray[0].AsInt);
        targetOb.ApplyStatus("B_MAX_CD_REDUCE", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队最大冷却减少光环清除脚本
/// </summary>
public class SCRIPT_12051 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_MAX_CD_REDUCE");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 镜花水月护盾光环作用脚本（给光环享受方上镜花水月护盾）
/// </summary>
public class SCRIPT_12052 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;
        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        LPCMapping condition = new LPCMapping();
        // 光环源RID，概率，触发限制，技能id
        condition.Add("props", new LPCArray(new LPCArray(340,new LPCArray(sourceOb.GetRid(),propValue[0].AsArray[0].AsInt, propValue[0].AsArray[1].AsInt, propValue[0].AsArray[2].AsInt))));
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_MIRAGE_SHIELD", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 镜花水月护盾光环清除脚本
/// </summary>
public class SCRIPT_12053 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_MIRAGE_SHIELD");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 神魔灭杀光环作用脚本
/// </summary>
public class SCRIPT_12054 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;
        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        LPCMapping condition = new LPCMapping();
        // 光环源RID，概率，回能数量，技能id
        condition.Add("props", new LPCArray(new LPCArray(193, new LPCArray(sourceOb.GetRid(),propValue[0].AsArray[0].AsInt, propValue[0].AsArray[1].AsInt, propValue[0].AsArray[2].AsInt))));
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_SHENMO", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 神魔灭杀光环清除脚本
/// </summary>
public class SCRIPT_12055 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_SHENMO");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 摄魂夺魄光环作用脚本
/// </summary>
public class SCRIPT_12056 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;
        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        LPCMapping condition = new LPCMapping();
        // 光环源RID，技能id
        condition.Add("props", new LPCArray(new LPCArray(195, new LPCArray(sourceOb.GetRid(),propValue))));
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_SOUL_GAIN", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 摄魂夺魄光环清除脚本
/// </summary>
public class SCRIPT_12057 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_SOUL_GAIN");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 百塔增幅光环作用脚本
/// </summary>
public class SCRIPT_12058 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;
        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        LPCMapping condition = new LPCMapping();
        // 光环源RID，技能id
        condition.Add("props", new LPCArray(new LPCArray(321, propValue)));
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_TOWER_ATK_UP", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 百塔增幅光环清除脚本
/// </summary>
public class SCRIPT_12059 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_TOWER_ATK_UP");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 非凡之物光环作用脚本
/// </summary>
public class SCRIPT_12060 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;
        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;

        LPCMapping condition = new LPCMapping();
        condition.Add("skill_id", propValue);
        condition.Add("source_rid", sourceOb.GetRid());
        targetOb.ApplyStatus("B_DMG_IMMUNE_SP", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 非凡之物光环清除脚本
/// </summary>
public class SCRIPT_12061 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_DMG_IMMUNE_SP");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 冰封结构光环作用脚本
/// </summary>
public class SCRIPT_12062 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;
        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        LPCMapping condition = new LPCMapping();
        condition.Add("skill_id", propValue[0].AsArray[0].AsInt);
        condition.Add("damage_type",propValue[0].AsArray[1].AsInt);
        condition.Add("source_rid", sourceOb.GetRid());
        targetOb.ApplyStatus("B_DMG_IMMUNE_TYPE", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 冰封结构光环清除脚本
/// </summary>
public class SCRIPT_12063 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_DMG_IMMUNE_TYPE");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 通用属性提升光环
/// </summary>
public class SCRIPT_12064 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;
        LPCMapping applyArgs = (_param[4] as LPCValue).AsMapping;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(applyArgs.GetValue<int>("prop_id"), propValue)));

        targetOb.ApplyStatus(applyArgs.GetValue<string>("status"), statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 通用属性提升状态光环清除
/// </summary>
public class SCRIPT_12065 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clearArgs

        Property targetOb = _param[0] as Property;
        Property sourceOb = _param[1] as Property;

        LPCMapping clearArgs = (_param[4] as LPCValue).AsMapping;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition(clearArgs.GetValue<string>("status"));
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队回能光环
/// </summary>
public class SCRIPT_12066 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray prop = (_param[3] as LPCValue).AsArray;
        int skillId = prop[0].AsArray[0].AsInt;
        int propValue = prop[0].AsArray[1].AsInt;
        LPCMapping applyArgs = (_param[4] as LPCValue).AsMapping;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(applyArgs.GetValue<int>("prop_id"), new LPCArray(skillId,propValue))));

        targetOb.ApplyStatus(applyArgs.GetValue<string>("status"), statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友死亡自身上状态光环
/// </summary>
public class SCRIPT_12067 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray prop = (_param[3] as LPCValue).AsArray;

        LPCArray statusList = new LPCArray();

        for (int i = 0; i < prop.Count; i++)
        {
            if (i == 0)
                continue;
            statusList.Add(prop[i].AsMapping);
        }

        // 参数：光环来源RID、skillId、状态list
        LPCArray propValue = new LPCArray(sourceOb.GetRid(), prop[0].AsArray[0].AsInt, statusList);

        LPCMapping condition = new LPCMapping();
        condition.Add("props", new LPCArray(new LPCArray(502, propValue)));
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_LIGHT_DARK", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友死亡自身上状态光环清除脚本
/// </summary>
public class SCRIPT_12068 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, clear_args

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        List<LPCMapping> allStatus = targetOb.GetStatusCondition("B_LIGHT_DARK");
        string sourceRid = sourceOb.GetRid();
        List<int> cookieList = null;

        for (int i = 0; i < allStatus.Count; i++)
        {
            if (!string.Equals(allStatus[i].GetValue<string>("source_rid"), sourceRid))
                continue;

            // 添加到清除列表中
            if (cookieList == null)
                cookieList = new List<int>() { allStatus[i].GetValue<int>("cookie") };
            else
                cookieList.Add(allStatus[i].GetValue<int>("cookie"));
        }

        // 清除状态
        targetOb.ClearStatusByCookie(cookieList);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友死亡自身扣血光环
/// </summary>
public class SCRIPT_12069 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray prop = (_param[3] as LPCValue).AsArray;

        // 参数：光环来源RID、skillId、状态list
        LPCArray propValue = new LPCArray(sourceOb.GetRid(), prop[0].AsArray);

        LPCMapping condition = new LPCMapping();
        condition.Add("props", new LPCArray(new LPCArray(506, propValue)));
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_SECRET_HP_COST", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 自身不可选中光环
/// </summary>
public class SCRIPT_12070 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        //Property sourceOb = _param[1] as Property;

        //LPCArray prop = (_param[3] as LPCValue).AsArray;

        LPCMapping condition = new LPCMapping();
        LPCMapping stopCondition = new LPCMapping();

        targetOb.ApplyStatus("B_CAN_NOT_CHOOSE", condition);

        targetOb.ApplyStatus("B_STOP_ACTION", stopCondition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 任一单位死亡增加基础攻击力光环
/// </summary>
public class SCRIPT_12071 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        LPCMapping condition = new LPCMapping();
        condition.Add("source_rid", sourceOb.GetRid());
        // sourceId, propValue, maxLimit, skillId
        condition.Add("props", new LPCArray(new LPCArray(512, new LPCArray(sourceOb.GetRid(), propValue[0].AsArray[0].AsInt, propValue[0].AsArray[1].AsInt, propValue[0].AsArray[2].AsInt))));
        targetOb.ApplyStatus("B_DEAD_S_ATK_UP", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 出其不意专用光环，受创消耗能量无敌，并反击
/// </summary>
public class SCRIPT_12072 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        int propValue = (_param[3] as LPCValue).AsInt;
        // 获取当前能量消耗
        LPCMapping mpCost = SkillMgr.GetCasTCost(sourceOb, propValue);
        LPCMapping condition = new LPCMapping();
        condition.Add("source_rid", sourceOb.GetRid());
        condition.Add("props", new LPCArray(new LPCArray(513, 0)));
        condition.Add("mp_cost", mpCost.GetValue<int>("mp"));
        targetOb.ApplyStatus("B_EN_COST_SHIELD", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全队基础攻击提升光环(对应元素属性)
/// </summary>
public class SCRIPT_12073 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        LPCMapping statusMap = new LPCMapping();
        statusMap.Add("source_rid", sourceOb.GetRid());
        statusMap.Add("props", new LPCArray(new LPCArray(1000,propValue[0].AsArray[1].AsInt)));

        targetOb.ApplyStatus("B_ATK_UP_LEADER", statusMap);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友暴击自身进行协同攻击光环
/// </summary>
public class SCRIPT_12074 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propArgs = (_param[3] as LPCValue).AsArray;
        int jointRate = propArgs[0].AsArray[0].AsInt;
        int skillId = propArgs[0].AsArray[1].AsInt;

        // 参数：光环来源RID、隐忍叠加作用起效上限次数
        LPCArray propValue = new LPCArray(sourceOb.GetRid(), jointRate, skillId);

        LPCMapping condition = new LPCMapping();
        condition.Add("props", new LPCArray(new LPCArray(520, propValue)));
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_CRT_JOINT", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 特殊生命链接光环，队友指定生命百分比下才起效
/// </summary>
public class SCRIPT_12075 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propArgs = (_param[3] as LPCValue).AsArray;

        // 上状态
        LPCMapping condition = new LPCMapping();
        condition.Add("source_profile", sourceOb.GetProfile());
        condition.Add("work_hp_rate", propArgs[0].AsArray[0].AsInt);
        condition.Add("skill_id", propArgs[0].AsArray[1].AsInt);
        condition.Add("value_rate", 1000);
        condition.Add("source_rid", sourceOb.Query<string>("rid"));

        // 附加状态
        targetOb.ApplyStatus("B_SACRIFICE_SP", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友回合结束，双方回血，并概率追加普攻的光环作用脚本
/// </summary>
public class SCRIPT_12076 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        // 参数：光环来源RID、触发概率、回血百分比、技能ID、追加技能ID
        LPCArray fixedPropValue = new LPCArray(sourceOb.GetRid(), propValue[0].AsArray[0].AsInt, propValue[0].AsArray[1].AsInt, propValue[0].AsArray[2].AsInt, propValue[0].AsArray[3].AsInt);

        LPCMapping condition = new LPCMapping();
        condition.Add("props", new LPCArray(new LPCArray(530, fixedPropValue)));
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_HEAL_JOINT", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 任一单位死亡光环源攻击光环
/// </summary>
public class SCRIPT_12077 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        LPCMapping condition = new LPCMapping();
        condition.Add("source_rid", sourceOb.GetRid());
        // sourceId, 源技能ID，释放技能ID
        condition.Add("props", new LPCArray(new LPCArray(542, new LPCArray(sourceOb.GetRid(), propValue[0].AsArray[0].AsInt, propValue[0].AsArray[1].AsInt))));
        targetOb.ApplyStatus("B_SOUL_REAPER", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 肾上腺素光环作用脚本
/// </summary>
public class SCRIPT_12078 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;
        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        LPCMapping condition = new LPCMapping();
        // 光环源RID，概率，技能id，额外概率，源技能id
        condition.Add("props", new LPCArray(new LPCArray(545, new LPCArray(
            sourceOb.GetRid(),
            propValue[0].AsArray[0].AsInt, 
            propValue[0].AsArray[1].AsInt, 
            propValue[0].AsArray[0].AsInt, 
            propValue[0].AsArray[2].AsInt))));

        targetOb.ApplyStatus("B_ADRENALINE", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 任一单位死亡光环源自身技能释放光环
/// </summary>
public class SCRIPT_12079 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;
        Property sourceOb = _param[1] as Property;
        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCMapping applyArgs = (_param[4] as LPCValue).AsMapping;

        LPCMapping condition = new LPCMapping();
        condition.Add("source_rid", sourceOb.GetRid());
        // sourceId, 源技能ID，释放技能ID
        condition.Add("props", new LPCArray(new LPCArray(applyArgs.GetValue<int>("prop_id"), new LPCArray(sourceOb.GetRid(), propValue[0].AsArray[0].AsInt, propValue[0].AsArray[1].AsInt))));
        targetOb.ApplyStatus(applyArgs.GetValue<string>("status"), condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 受创触发光环源回能光环
/// </summary>
public class SCRIPT_12080 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        LPCMapping condition = new LPCMapping();
        condition.Add("source_rid", sourceOb.GetRid());
        // sourceId, 源技能ID，回能值
        condition.Add("props", new LPCArray(new LPCArray(547, new LPCArray(sourceOb.GetRid(), propValue[0].AsArray[0].AsInt, propValue[0].AsArray[1].AsInt))));
        targetOb.ApplyStatus("B_CHARGE_MP", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 全体生命链接光环（该生命链接无法被驱散，且是光环效果死亡不清除）
/// </summary>
public class SCRIPT_12081 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        //LPCArray propValue = (_param[3] as LPCValue).AsArray;

        // 生命链接的目标（这里是全体，不管死活，起效时在伤害分摊里判断死亡单位不链接）
        List<Property> targetList = RoundCombatMgr.GetPropertyList(sourceOb.CampId);

        LPCArray ridList = new LPCArray();
        // 筛选非死亡单位
        foreach (Property checkOb in targetList)
        {
            ridList.Add(checkOb.GetRid());
        }

        LPCMapping condition = new LPCMapping();
        condition.Add("source_rid", sourceOb.GetRid());
        condition.Add("rid_list", ridList);
        condition.Add("round_cookie", Game.NewCookie(sourceOb.GetRid()));
        targetOb.ApplyStatus("B_SACRIFICE_ALL_SP", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 纯正血统光环（场上任意单位死亡触发光环源释放技能复活）
/// </summary>
public class SCRIPT_12082 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;

        LPCMapping condition = new LPCMapping();
        condition.Add("source_rid", sourceOb.GetRid());
        // sourceId, 源技能ID，释放技能ID
        condition.Add("props", new LPCArray(new LPCArray(546, new LPCArray(sourceOb.GetRid(), propValue[0].AsArray[0].AsInt, propValue[0].AsArray[1].AsInt))));
        targetOb.ApplyStatus("B_SOUL_SMELL", condition);

        // 返回数据
        return true;
    }
}

/// <summary>
/// 队友死亡触发光环源释放复活技能光环
/// </summary>
public class SCRIPT_12083 : Script
{
    public override object Call(params object[] _param)
    {
        // 参数格式
        // targetOb, sourceOb, propCsv,propValue, selectArgs

        Property targetOb = _param[0] as Property;

        Property sourceOb = _param[1] as Property;

        LPCArray propValue = (_param[3] as LPCValue).AsArray;
        LPCArray propArgs = propValue[0].AsArray;

        // 参数：光环来源RID、光环源技能ID、释放技能ID
        LPCArray prop = new LPCArray(sourceOb.GetRid(), propArgs[0].AsInt, propArgs[1].AsInt);

        LPCMapping condition = new LPCMapping();
        condition.Add("props", new LPCArray(new LPCArray(562, prop)));
        condition.Add("source_rid", sourceOb.GetRid());

        targetOb.ApplyStatus("B_CONTRACT", condition);

        // 返回数据
        return true;
    }
}