/// <summary>
/// MonsterMgr.cs
/// Created from lic 2017/12/12
/// 图鉴管理
/// </summary>

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using LPC;

public class ManualMgr
{
    #region 成员变量

    private static Dictionary<int, int> mAmountMap = new Dictionary<int, int>();

    // 是否有新的数据
    private static bool mIsNewData = false;

    #endregion

    #region 内部函数

    /// <summary>
    /// 登陆成功回调
    /// </summary>
    private static void WhenLoginOk(int eventId, MixedValue para)
    {
        // 初始化数据
        GatherData();

        mIsNewData = false;

        if (ME.user == null)
            return;

        ME.user.dbase.RegisterTriggerField("ManualMgr", new string[]{"manual_data"}, new CallBack(OnFieldsChange));
    }

    /// <summary>
    /// 字段变化回调
    /// </summary>
    private static void OnFieldsChange(object para, params object[] param)
    {
        mIsNewData = true;
    }

    /// <summary>
    /// 收集数据
    /// </summary>
    private static void GatherData()
    {
        if (ME.user == null)
            return;

        mAmountMap.Clear();

        foreach (CsvRow row in MonsterMgr.MonsterCsv.rows)
        {
            int rank = row.Query<int>("rank");

            int classId = row.Query<int>("class_id");

            int element = row.Query<int>("element");

            int amount = 0;

            if (rank.Equals(MonsterConst.RANK_UNABLEAWAKE))
            {
                if (IsCompleted(ME.user, classId, rank))
                {
                    mAmountMap.TryGetValue(element, out amount);

                    amount++;

                    mAmountMap[element] = amount;
                }
            }
            else
            {
                mAmountMap.TryGetValue(element, out amount);

                if (IsCompleted(ME.user, classId, MonsterConst.RANK_AWAKED))
                {
                    amount++;
                }

                if (IsCompleted(ME.user, classId, MonsterConst.RANK_UNAWAKE))
                {
                    amount++;
                }

                // 缓存数据
                mAmountMap[element] = amount;
            }
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 注册登陆成功回调
        EventMgr.UnregisterEvent("ManualMgr");
        EventMgr.RegisterEvent("ManualMgr", EventMgrEventType.EVENT_LOGIN_OK, WhenLoginOk);
    }

    /// <summary>
    /// 判断任务是否已经完成
    /// </summary>
    public static bool IsCompleted(Property user, int classId, int rank)
    {
        CsvRow csv = MonsterMgr.GetRow(classId);

        if (csv == null)
            return false;

        // 获取普通主线任务
        LPCValue v = user.Query<LPCValue> ("manual_data/complete");
        if (v == null || !v.IsMapping)
            return false;

        LPCMapping completeData = v.AsMapping;

        if (!completeData.ContainsKey(rank))
            return false;

        LPCArray complete = completeData.GetValue<LPCArray>(rank);

        // 取不到任何记录肯定是未完成
        if (complete == null)
            return false;

        // 计算当前任务的存储位置
        int flag = csv.Query<int> ("flag");
        int index = flag / 31;

        // 当前分配的长度已经足够，说明任务还没有完成过
        if (complete.Count <= index)
            return false;

        // 计算偏移量
        int offset = 1 << (flag % 31);

        // 如果没有完成不处理
        if ((complete [index].AsInt & offset) == 0)
            return false;

        // 检测一下是否已经完成
        return true;
    }

    /// <summary>
    /// 是否是新的图鉴
    /// </summary>
    public static bool IsNewManual(Property user, int rank, int classId)
    {
        if (ME.user == null)
            return false;

        // 获取图鉴的奖励数据
        LPCValue v = user.Query<LPCValue>("manual_data/bonus");
        if (v == null || ! v.IsMapping)
            return false;

        LPCMapping bonus = v.AsMapping;
        if (!bonus.ContainsKey(rank))
            return false;

        LPCArray list = bonus.GetValue<LPCArray>(rank);

        if (list.IndexOf(classId) == -1)
            return false;

        return true;
    }

    /// <summary>
    /// 获取奖励列表
    /// </summary>
    public static LPCArray GetBonusList(Property user, int rank)
    {
        // 获取图鉴的奖励数据
        LPCValue bonus = user.Query<LPCValue>("manual_data/bonus");
        if (bonus == null || ! bonus.IsMapping)
            return null;

        return bonus.AsMapping.GetValue<LPCArray>(rank);
    }

    /// <summary>
    /// 根据使魔元素获取新使魔数量提示
    /// </summary>
    public static int GetNewTipsByElement(Property user, int element)
    {
        int amount = 0;

        // 获取图鉴的奖励数据
        LPCValue bonus = user.Query<LPCValue>("manual_data/bonus");
        if (bonus == null || ! bonus.IsMapping)
            return amount;

        List<int> elementList = GetManualListByElement(element);

        LPCMapping data = bonus.AsMapping;

        foreach (LPCValue v in data.Values)
        {
            if (!v.IsArray)
                continue;

            foreach (LPCValue classId in v.AsArray.Values)
            {
                if (!classId.IsInt)
                    continue;

                if (elementList.Contains(classId.AsInt))
                    amount++;
            }
        }

        return amount;
    }

    /// <summary>
    /// 获取某个元素的图鉴宠物列表
    /// </summary>
    public static List<int> GetManualListByElement(int element)
    {
        List<int> list = new List<int>();

        foreach (CsvRow row in MonsterMgr.MonsterCsv.rows)
        {
            if (row == null)
                continue;

            if (row.Query<int>("show_in_manual") != 1)
                continue;

            if (row.Query<int>("element") != element)
                continue;

            list.Add(row.Query<int>("class_id"));
        }

        return list;
    }

    /// <summary>
    /// 获取图鉴宠物总数
    /// </summary>
    public static int GetManualAmount()
    {
        int amount = 0;

        foreach (CsvRow row in MonsterMgr.MonsterCsv.rows)
        {
            if (row == null)
                continue;

            if (row.Query<int>("show_in_manual") != 1)
                continue;

            if (row.Query<int>("rank") == MonsterConst.RANK_UNABLEAWAKE)
            {
                amount++;
            }
            else
            {
                amount += 2;
            }
        }

        return amount;
    }

    /// <summary>
    /// 获取玩家获得使魔总数（图鉴数量）
    /// </summary>
    public static int GetHoldManualAmount()
    {
        if (mIsNewData)
        {
            // 收集新的数据
            GatherData();

            mIsNewData = false;
        }

        int amount = 0;

        // 拥有的图鉴总数
        foreach (int item in mAmountMap.Values)
            amount += item;

        return amount;
    }

    /// <summary>
    /// 获取某个元素的使魔总数（图鉴数量）
    /// </summary>
    public static int GetHoldManualAmountByElement(int element)
    {
        if (mIsNewData)
        {
            // 收集新的数据
            GatherData();

            mIsNewData = false;
        }

        if (!mAmountMap.ContainsKey(element))
            return 0;

        return mAmountMap[element];
    }

    #endregion
}
