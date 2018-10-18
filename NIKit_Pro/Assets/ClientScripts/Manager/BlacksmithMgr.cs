/// <summary>
/// BlacksmithMgr.cs
/// Created by fucj 2014-12-16
/// 工坊管理
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using LPC;

public static class BlacksmithMgr
{
    #region 属性

    // 工坊模块集合
    private static Dictionary<string, Blacksmith> mBlacksmithList = new Dictionary<string, Blacksmith>();

    // 道具合成配置表信息
    private static CsvFile mSynthesisCsv;
    private static Dictionary<int, List<LPCMapping>> mSynthesisTypeList = new Dictionary<int, List<LPCMapping>>();

    // 工坊操作强化配置表信息
    private static CsvFile mIntensifyCsv;

    #endregion

    #region 属性

    // 获取道具合成配置表信息
    public static CsvFile SynthesisCsv { get { return mSynthesisCsv; } }

    // 获取工坊操作配置表信息
    public static CsvFile IntensifyCsv { get { return mIntensifyCsv; } }

    #endregion

    #region 内部接口

    /// <summary>
    /// MSG_BLACKSMITH_ACTION的回调
    /// </summary>
    private static void OnMsgBlacksmithAction(string cmd, LPCValue para)
    {
        // 玩家对象不存在
        if (ME.user == null || ME.user.IsDestroyed)
            return;

        LPCMapping args = para.AsMapping;
        string action = args.GetValue<string>("action");
        LPCValue extra_para = args.GetValue<LPCValue>("extra_para");

        Blacksmith mod;
        mBlacksmithList.TryGetValue(action, out mod);

        // 没有处理模块
        if (mod == null)
            return;

        // 调用子模块操作
        mod.DoActionResult(ME.user, extra_para);
    }

    /// <summary>
    /// 载入合成规则配置表
    /// </summary>
    private static void LoadSynthesisFile(string fileName)
    {
        mSynthesisTypeList.Clear();

        // 载入道具合成配置表信息
        mSynthesisCsv = CsvFileMgr.Load(fileName);

        Dictionary<int, Dictionary<string, LPCMapping>> synthesis = new Dictionary<int, Dictionary<string, LPCMapping>>();
        string name;
        int type;

        // 遍历装备数据
        foreach (CsvRow data in mSynthesisCsv.rows)
        {
            // 初始化类型数据
            type = data.Query<int>("type");
            if (!synthesis.ContainsKey(type))
                synthesis.Add(type, new Dictionary<string, LPCMapping>());

            // 获取该规则的合成分组别名
            name = data.Query<string>("name");

            // 初始化数据
            if (!synthesis[type].ContainsKey(name))
            {
                LPCMapping tData = LPCMapping.Empty;
                tData.Add("name", name);
                tData.Add("sort", data.Query<int>("sort"));
                tData.Add("icon", data.Query<string>("type_icon"));
                tData.Add("rules", new LPCArray(data.Query<int>("rule")));

                // 记录数据
                synthesis[type].Add(name, tData);

                continue;
            }

            // 记录数据
            synthesis[type][name]["rules"].AsArray.Add(data.Query<int>("rule"));
        }

        // 重新排序
        foreach (int keyType in synthesis.Keys)
        {
            // 初始化数据
            if (!mSynthesisTypeList.ContainsKey(keyType))
                mSynthesisTypeList.Add(keyType, new List<LPCMapping>());

            // 遍历该类型的全部数据
            foreach (LPCMapping data in synthesis[keyType].Values)
                mSynthesisTypeList[keyType].Add(data);

            // 排序
            mSynthesisTypeList[keyType].Sort(Compare);
        }
    }

    /// <summary>
    /// Compare the specified x and y.
    /// </summary>
    private static int Compare(LPCMapping left, LPCMapping right)
    {
        int xi = left.GetValue<int>("sort");
        int yi = right.GetValue<int>("sort");

        if (xi < yi)
            return -1;

        if (xi > yi)
            return 1;

        return 0;
    }

    /// <summary>
    /// 前往商店
    /// </summary>
    private static void GotoShop(object para, params object[] _params)
    {
        // 前往商店
        GameObject wnd = WindowMgr.OpenWnd(QuickMarketWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;
        wnd.GetComponent<QuickMarketWnd>().Bind(para as string);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 载入工坊子模块
        LoadSubmodule();

        // 载入合成规则配置表
        LoadSynthesisFile("synthesis");

        // 载入工坊操纵配置信息表
        mIntensifyCsv = CsvFileMgr.Load("intensify");

        // 关注MSG_BLACKSMITH_ACTION消息
        MsgMgr.RemoveDoneHook("MSG_BLACKSMITH_ACTION", "BlacksmithMgr");
        MsgMgr.RegisterDoneHook("MSG_BLACKSMITH_ACTION", "BlacksmithMgr", OnMsgBlacksmithAction);
    }

    /// <summary>
    /// 执行工坊操作
    /// </summary>
    public static bool DoAction(Property who, string actionName, LPCMapping para)
    {
        Blacksmith mod;
        mBlacksmithList.TryGetValue(actionName, out mod);

        if (mod == null)
        {
            LogMgr.Trace("工坊子模块{0}不存在。", actionName);
            return false;
        }

        // 调用子模块操作
        return mod.DoAction(who, para);
    }

    /// <summary>
    /// 获取工坊强化信息
    /// </summary>
    public static CsvRow GetIntensifyData(int rank)
    {
        return IntensifyCsv.FindByKey(rank);
    }

    /// <summary>
    /// 获取合成规则配置.
    /// </summary>
    /// <returns>The synthetic rule.</returns>
    /// <param name="classId">Class identifier.</param>
    public static CsvRow GetSyntheticData(int rule)
    {
        // 获取合成规则配置
        return SynthesisCsv.FindByKey(rule);
    }

    /// <summary>
    /// 获取道具合成规则列表
    /// </summary>
    public static List<LPCMapping> GetSyntheticList(int type)
    {
        // 没有该类型的数据返回
        if (!mSynthesisTypeList.ContainsKey(type))
            return new List<LPCMapping>();

        // 返回配置配置表数据
        return mSynthesisTypeList[type];
    }

    /// <summary>
    /// 检测消耗是否足够(包括道具消耗和属性消耗)
    /// </summary>
    /// <returns><c>true</c>, if cost enough was checked, <c>false</c> otherwise.</returns>
    /// <param name="who">Who.</param>
    /// <param name="cost">Cost.</param>
    public static bool CheckCostEnough(Property who, LPCArray costArray)
    {
        for(int i = 0; i < costArray.Count; i++)
        {
            LPCMapping costMap = costArray[i].AsMapping;

            if(costMap.ContainsKey("class_id"))
            {
                int classid = costMap.GetValue<int>("class_id");
                int cost_amount = costMap.GetValue<int>("amount");

                Property item_ob = BaggageMgr.GetBaggageItemByClassId(ME.user, classid);

                int my_item = 0;

                if(item_ob != null)
                    my_item = item_ob.Query<int>("amount");

                if(cost_amount > my_item)
                {
                    DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("BlacksmithMgr_1"), ItemMgr.GetName(classid)));
                    return false;
                }
            }
            else
            {
                string field = FieldsMgr.GetFieldInMapping(costMap);
                int cost_amount = costMap.GetValue<int>(field);

                int my_money = ME.user.Query<int>(field);

                if(cost_amount > my_money)
                {
                    if(field.Equals("gold_coin"))
                        DialogMgr.ShowDailog(new CallBack(GotoShop, ShopConfig.GOLD_COIN_GROUP), string.Format(LocalizationMgr.Get("SummonWnd_19"), FieldsMgr.GetFieldName(field)));
                    else if(field.Equals("money"))
                        DialogMgr.ShowDailog(new CallBack(GotoShop, ShopConfig.MONEY_GROUP), string.Format(LocalizationMgr.Get("SummonWnd_19"), FieldsMgr.GetFieldName(field)));
                    else
                        DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("SummonWnd_18"), field));

                    return false;
                }
            }
        }

        return true;

    }

    /// <summary>
    /// 判断道具是否可合成
    /// </summary>
    /// <returns><c>true</c>, if can synthesis was checked, <c>false</c> otherwise.</returns>
    /// <param name="rule">Rule.</param>
    public static bool CheckCanSynthesis(int rule)
    {
        CsvRow data = GetSyntheticData(rule);

        if(data == null)
            return false;

        if(data.Query<int>("synthesis_script") == 0)
            return false;

        return true;
    }

    /// <summary>
    /// 载入工坊子模块
    /// </summary>
    private static void LoadSubmodule()
    {
        mBlacksmithList.Clear();

        Assembly asse = Assembly.GetExecutingAssembly();
        Type[] types = asse.GetTypes();
        foreach (Type t in types)
        {
            // 不是工坊模块，无视
            if (!t.IsSubclassOf(typeof(Blacksmith)))
                continue;

            mBlacksmithList.Add(t.Name.ToLower(), asse.CreateInstance(t.Name) as Blacksmith);
        }
    }

    #endregion
}
