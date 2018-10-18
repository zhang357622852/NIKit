/// <summary>
/// BonusMgr.cs
/// Created by zhaozy 2014-12-12
/// 奖励管理模块
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System;
using LPC;

public static class BonusMgr
{
    #region 属性

    // 战斗奖励配置表
    private static CsvFile mCombatBonusCsv;

    // 奖励集合
    private static Dictionary<string, Bonus> mBonusList = new Dictionary<string, Bonus>();

    // 当前副本剩余奖励集合
    private static LPCMapping mRemainBonusMap = new LPCMapping();

    // 当前副本已掉落集合
    private static LPCMapping mDropBonusMap = new LPCMapping();

    #endregion

    #region 属性

    // 获取配置表信息
    public static CsvFile CombatBonusCsv { get { return mCombatBonusCsv; } }

    // 获取当前副本剩余奖励信息
    public static LPCMapping RemainBonusMap { get { return mRemainBonusMap;} }

    // 获取当前副本已掉落信息
    public static LPCMapping DropBonusMap { get { return mDropBonusMap;} }

    #endregion

    #region 内部接口

    /// <summary>
    /// 加载奖励子模块
    /// </summary>
    private static void LoadEntryBonus()
    {
        mBonusList.Clear();

        // 收集所有奖励模块
        Assembly asse = Assembly.GetExecutingAssembly();
        Type[] types = asse.GetTypes();
        foreach (Type t in types)
        {
            // 不是策略子模块不处理
            if (!t.IsSubclassOf(typeof(Bonus)))
                continue;

            // 添加到列表中
            mBonusList.Add(t.Name, asse.CreateInstance(t.Name) as Bonus);
        }
    }

    /// <summary>
    /// 收集怪物执行掉落时的参数
    /// </summary>
    private static LPCMapping GetCombatEntityData(Property who)
    {
        // 实体对象不存在
        if (who == null)
            return null;

        // 获取怪物的奖励规则列表
        LPCArray bonusRule = new LPCArray();

        bonusRule = who.Query<LPCArray>("bonus_rule");

        // 获取怪物的奖励规则列表
        if (bonusRule == null || bonusRule.Count == 0)
            return null;

        // 记录怪物的奖励列表
        LPCMapping info = new LPCMapping();
        info.Add("bonus_rule", bonusRule);

        string instanceId = who.Query<string>("instance/id");
        LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(instanceId);

        // 获取实体的基础数据
        info.Add("rid", who.GetRid());
        info.Add("level", who.Query("level"));
        info.Add("color", who.Query("color"));
        info.Add("instance_rid", who.Query("instance/rid"));
        info.Add("instance_id", instanceId);
        info.Add("instance_info", instanceInfo);

        // info.Add("is_boss", IS_BOSS.Call(who) ? 1 : 0);

        info.Add("exp", who.Query("exp"));
        info.Add("bm", who.Query("bm"));
        info.Add("max_hp", who.QueryAttrib("max_hp"));

        // 获取位置信息
        Vector3 pos = who.GetPosition();
        info.Add("position", new LPCArray(pos.x, pos.y, 0.0f));

        // 收集信息
        return info;
    }

    /// <summary>
    /// 修正奖励
    /// </summary>
    /// <returns>The bonus.</returns>
    /// <param name="">.</param>
    private static LPCMapping FixBonus(LPCMapping bonus)
    {
        if (bonus == null || bonus.Count == 0)
            return null;

        if (mRemainBonusMap == null || mRemainBonusMap.Count == 0)
            return null;

        LPCMapping fixedBonus = new LPCMapping();

        foreach (string key in bonus.Keys)
        {
            if (!mRemainBonusMap.ContainsKey(key))
                continue;

            int fixedNum = 0;

            if (mRemainBonusMap[key].AsInt <= bonus[key].AsInt)
                fixedNum = mRemainBonusMap[key].AsInt;
            else
                fixedNum = bonus[key].AsInt;
          
            fixedBonus.Add(key, fixedNum);

            // 添加到掉落集合中
            if (mDropBonusMap != null && mDropBonusMap.ContainsKey(key))
                mDropBonusMap[key] = LPCValue.Create(fixedNum + mDropBonusMap[key].AsInt); //(fixedNum + dropBonusMap[key].AsInt) as LPCValue;
            else
                mDropBonusMap.Add(key, fixedNum);

            // 从剩余map中去掉已掉数量
            mRemainBonusMap[key] = LPCValue.Create(mRemainBonusMap[key].AsInt - fixedNum);

            if (mRemainBonusMap[key].AsInt <= 0)
                mRemainBonusMap.Remove(key);
        }

        return fixedBonus;
    }

    /// <summary>
    /// 计算固定掉落
    /// </summary>
    private static LPCMapping CalcFixedBonus(Property assailanter, LPCMapping entityInfo)
    {
        // 获取怪物的固定掉落列表
        LPCArray bonusRule = entityInfo.GetValue<LPCArray>("bonus_rule");

        // 该怪物没有配置奖励规则，则无奖励
        if (bonusRule == null || bonusRule.Count == 0)
            return null;

        // 获取奖励数据
        LPCMapping bonus = new LPCMapping();
        CsvRow data = null;
        int calcScript;
        object ret;

        // 取奖励配置信息
        for (int i = 0; i < bonusRule.Count; i++)
        {
            // 没有配置的掉落规则
            data = CombatBonusCsv.FindByKey(bonusRule[i].AsString);
            if (data == null)
                continue;

            // 调用脚本计算
            calcScript = data.Query<int>("calc_script");
            if (calcScript == 0)
                continue;

            // 调用计算脚本进行计算
            ret = ScriptMgr.Call(calcScript, entityInfo, data.Query<LPCValue>("calc_arg"));

            // 返回数据不正确
            if (ret == null)
                continue;

            // 获取奖励数据
            if (!(ret is LPCMapping))
                continue;

            LPCMapping retMap = ret as LPCMapping;

            foreach (string key in retMap.Keys)
            {   
                int num = retMap[key].AsInt;

                if (bonus != null && bonus.ContainsKey(key))
                    bonus[key] = LPCValue.Create(bonus[key].AsInt + num);
                else
                    bonus.Add(key, num);
            }
        }

        // 返回数据
        return bonus;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 载入战斗奖励配置表
        mCombatBonusCsv = CsvFileMgr.Load("combat_bonus");

        // 加载奖励子模块
        LoadEntryBonus();
    }

    /// <summary>
    /// 战斗掉落奖励
    /// </summary>
    public static void DoCombatBonus(Property who, LPCMapping extraPara)
    {
        // 怪物对象不存在了
        if (who == null)
            return;

        // 怪物死亡时没有assailant_info, 不处理
        LPCMapping assailantInfo = who.QueryTemp<LPCMapping>("assailant_info");

        // 没有杀死怪物的凶手信息
        if (assailantInfo == null)
            return;

        // 没有凶手就不掉了
        // 凶手已经死亡不执行掉落
        Property assailanter = Rid.FindObjectByRid(assailantInfo.GetValue<string>("rid"));
        if (assailanter == null || assailanter.CheckStatus("DIED"))
            return;

        // 缓存死亡实体的数据，防止后续死亡实体被析构了导致出错
        LPCMapping entityInfo = GetCombatEntityData(who);
        if (entityInfo == null || entityInfo.Count == 0)
            return;

        // 增加附加参数
        entityInfo.Append(extraPara);

        // 计算执行固定奖励列表
        LPCMapping BonusMap = CalcFixedBonus(assailanter, entityInfo);

        // 无奖励不处理
        if (BonusMap == null || BonusMap.Count == 0)
            return;

        foreach (string attribName in BonusMap.Keys)
        {
            int multiple = GetBonusMultiple(attribName);

            BonusMap[attribName].AsInt *= multiple;
        }

        // 修正并扣除奖励
        LPCMapping fixedBonus = FixBonus(BonusMap);

        if (fixedBonus == null || fixedBonus.Count == 0)
            return;

        // 生成一个唯一cookie
        string cookie = Game.NewCookie(who.GetRid());

        // 构建参数
        LPCMapping para = new LPCMapping();
        para.Add("rid", who.GetRid());
        para.Add("propertyList", fixedBonus);
        para.Add("entityInfo", entityInfo);

        // 掉落物品效果管理器
        DropEffectMgr.SubmitBonusWnd(who, cookie, para);
    }

    /// <summary>
    /// 执行奖励接口.
    /// </summary>
    public static bool DoBonus(Property who, string bonusType, LPCArray bonusList, LPCMapping extraPara = null)
    {
        // 奖励对象不存在
        if (who == null)
            return false;

        // 获取奖励子模块
        Bonus mod;
        if (!mBonusList.TryGetValue(bonusType, out mod))
        {
            LogMgr.Trace("无效奖励类型{0}，奖励失败。", bonusType);
            return false;
        }

        // 执行奖励
        return mod.DoBonus(who, bonusList, extraPara);
    }

    // 检查对象的等级是否超过
    public static bool CheckLevel(Property who)
    {
        // 不是玩家不处理
        if (!who.IsUser())
            return false;

        // 返回玩家是否达到了等级上限
        return who.Query<int>("level") >= MaxAttrib.GetMaxAttrib("level");
    }

    /// <summary>
    /// 获取奖励倍数
    /// </summary>
    public static int GetBonusMultiple(string attribName)
    {
        // 当前运行的活动列表
        List<LPCMapping> activityList = ActivityMgr.GetActivityList();
        if (activityList == null)
            activityList = new List<LPCMapping>();

        // 对象使用道具产生的双倍道具数据
        LPCValue v = ME.user.Query<LPCValue>("double_bonus_data");

        LPCMapping doubleBonusData = LPCMapping.Empty;
        if (v != null && v.IsMapping)
            doubleBonusData = v.AsMapping;

        int multiple = 1;

        if (activityList.Count == 0 && doubleBonusData.Count == 0)
            return multiple;

        Dictionary<string, int> indexMap = new Dictionary<string, int>();

        int index = 0;
        foreach (LPCValue item in doubleBonusData.Values)
        {
            indexMap.Add(item.AsMapping.GetValue<string>("activity_id"), index);
            index++;
        }

        List<LPCMapping> newList = new List<LPCMapping>();

        string attrib = string.Empty;
        for (int i = 0; i < activityList.Count; i++)
        {
            string activityId = activityList[i].GetValue<string>("activity_id");

            LPCValue bonus = ActivityMgr.GetActivityBonus(activityList[i]);

            LPCMapping bonusArg = LPCMapping.Empty;

            if (bonus.IsArray)
            {
                for (int j = 0; j < bonus.AsArray.Count; j++)
                {
                    bonusArg = bonus.AsArray[j].AsMapping;

                    if (bonusArg == null)
                        continue;

                    if (! bonusArg.ContainsKey("attrib"))
                        continue;

                    attrib = bonusArg.GetValue<string>("attrib");

                    if (indexMap.ContainsKey(activityId))
                    {
                        doubleBonusData.Remove(activityId);
                    }


                    activityList[i].Add("attrib", attrib);
                    activityList[i].Add("multiple", bonusArg.GetValue<int>("multiple"));
                    newList.Add(activityList[i]);
                }
            }
            else if (bonus.IsMapping)
            {
                bonusArg = bonus.AsMapping;

                if (! bonusArg.ContainsKey("attrib"))
                    continue;

                attrib = bonusArg.GetValue<string>("attrib");

                if (indexMap.ContainsKey(activityId))
                {
                    doubleBonusData.Remove(activityId);
                }

                activityList[i].Add("attrib", attrib);
                activityList[i].Add("multiple", bonusArg.GetValue<int>("multiple"));
                newList.Add(activityList[i]);
            }
            else
            {
            }
        }

        foreach (string key in doubleBonusData.Keys)
        {
            LPCMapping value = doubleBonusData.GetValue<LPCMapping>(key);

            if (TimeMgr.GetServerTime() >= value.GetValue<int>("end_time"))
                continue;

            value.Add("attrib", key);

            newList.Add(value);
        }

        if (newList == null || newList.Count == 0)
            return multiple;

        CsvRow paraRow = null;

        if (attribName != "exp")
        {
            // 获取属性对应道具的classId
            int paraClassId = FieldsMgr.GetClassIdByAttrib(attribName);

            // 道具的配置表数据
            paraRow = ItemMgr.GetRow(paraClassId);
            if (paraRow == null)
                return multiple;
        }

        int item_type = 0;

        for (int i = 0; i < newList.Count; i++)
        {
            multiple = newList[i].GetValue<int>("multiple");

            attrib = newList[i].GetValue<string>("attrib");

            if (attrib == "exp")
            {
                if (attrib != attribName)
                {
                    multiple = 1;

                    continue;
                }

                break;
            }
            else if(attrib == "soul")
            {
                if (paraRow == null)
                {
                    multiple = 1;

                    continue;
                }

                // 魂石
                item_type = ItemType.ITEM_TYPE_SOUL;

                if (item_type == paraRow.Query<int>("item_type"))
                    break;
            }
            else
            {
                if (attrib != attribName)
                {
                    multiple = 1;

                    continue;
                }

                break;
            }
        }

        return multiple;
    }

    /// <summary>
    /// 每次进入副本时重置副本当前掉落奖励
    /// </summary>
    public static void ResetBonus(LPCMapping dropBonus)
    {
        // 清空奖励列表
        mDropBonusMap = new LPCMapping();

        // 重置剩余掉落
        mRemainBonusMap = dropBonus;
    }

    #endregion
}

/// <summary>
/// 奖励基类
/// </summary>
public abstract class Bonus
{
    // 触发策略
    public abstract bool DoBonus(Property who, LPCArray bonusList, LPCMapping extraPara = null, bool isFormulaFloor = true);
}