/// <summary>
/// SummonMgr.cs
/// Created by zhaozy 2015-03-18
/// 召唤管理器
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using LPC;
using Spine;
using Spine.Unity;

/// <summary>
/// 召唤管理器
/// </summary>
public static class SummonMgr
{
    #region 私有接口

    // 宠物召唤配置
    private static CsvFile mSummonCsv;

    // 召唤宠物模型列表
    private static List<GameObject> mSummonObjects = new List<GameObject>();

    #endregion

    #region 属性接口

    /// <summary>
    /// 宠物召唤配置
    /// </summary>
    public static CsvFile SummonCsv { get { return mSummonCsv; } }

    #endregion

    #region 私有接口



    #endregion

    #region 公共接口

    /// <summary>
    /// SummonMgr初始化
    /// </summary>
    public static void InIt()
    {
        // 载入宠物召唤配置表信息
        mSummonCsv = CsvFileMgr.Load("summon");
    }

    /// <summary>
    /// 获得数据
    /// </summary>
    public static CsvRow GetRow(int classId)
    {
        CsvRow itemRow = SummonCsv.FindByKey(classId);

        return itemRow;
    }

    /// <summary>
    /// 获取召唤道具的名称
    /// </summary>
    /// <returns>The name.</returns>
    /// <param name="classId">Class identifier.</param>
    public static string GetName(int classId)
    {
        CsvRow item = GetRow(classId);

        if(item == null)
            return string.Empty;

        return LocalizationMgr.Get(item.Query<string>("title"));
    }

    /// <summary>
    /// 获取召唤限制次数
    /// </summary>
    /// <returns>The limit times.</returns>
    /// <param name="classId">Class identifier.</param>
    public static int GetLimitTimes(int classId)
    {
        CsvRow item = GetRow(classId);

        if(item == null)
            return 0;

        return item.Query<int>("limit_summon_times");
    }

    /// <summary>
    /// 获取召唤图片
    /// </summary>
    /// <returns>The summon icon.</returns>
    /// <param name="icon">Icon.</param>
    public static string GetSummonIcon(string icon)
    {
        return string.Format("s{0}", icon);
    }

    /// <summary>
    /// 加载宠物模型
    /// </summary>
    /// <param name="petItems">Pet items.</param>
    public static void LoadModel(List<Property> petItems, Vector3[] petPos)
    {
        for(int i = 0; i < petItems.Count; i++)
        {
            if(petItems[i] == null)
                continue;

            string modelId = MonsterMgr.GetModel(petItems[i].GetClassID());

            // 没有模型id
            if (string.IsNullOrEmpty(modelId))
                return;

            // 创建角色对象
            string prefabRes = string.Format("Assets/Prefabs/Model/{0}.prefab", modelId);

            // 模型默认位置
            Vector3 pos =new Vector3(-0.23f, - 1.09f, -0.05f);

            if(petItems.Count != 1 && i < petPos.Length)
                pos = petPos[i];

            GameObject preObj = ResourceMgr.Load(prefabRes) as GameObject;
            GameObject mGameObject = GameObject.Instantiate(preObj, pos, preObj.transform.localRotation) as GameObject;
            mGameObject.name = "model_" + petItems[i].GetRid();

            string skin = CALC_ACTOR_SKIN.Call(petItems[i].GetRank());

            // 指定皮肤
            if (!string.IsNullOrEmpty(skin))
            {
                // 获取骨骼动组件
                SkeletonRenderer skeletonRender = mGameObject.GetComponent<SkeletonRenderer>();
                if (skeletonRender != null)
                {
                    // 设置皮肤
                    skeletonRender.initialSkinName = skin;
                    skeletonRender.Initialize(true);
                }
            }

            SkeletonAnimator sRender = mGameObject.GetComponent<SkeletonAnimator>();
            if (sRender != null)
            {
                MeshRenderer mr = sRender.gameObject.GetComponent<MeshRenderer>();
                mr.sortingOrder = i;
            }

            mSummonObjects.Add(mGameObject);
        }
    }
        
    /// <summary>
    /// 卸载旧模型
    /// </summary>
    public static void UnLoadModel()
    {
        foreach(GameObject itemOb in mSummonObjects)
        {
            // 模型对象不存在
            if (itemOb == null)
                continue;

            // 回收资源
            UnityEngine.Object.Destroy(itemOb);
        }

        mSummonObjects.Clear();
    }

    /// <summary>
    /// 检查能否进行召唤(消耗，宠物格子)
    /// </summary>
    public static bool CheckSummon(CsvRow summonItem, int summonTimes, int subId)
    {
        if(summonItem == null)
            return false;

        if(ME.user.QueryTemp<int>("gapp_world") == 1)
        {
            int limitTimes = summonItem.Query<int>("limit_summon_times");

            if(limitTimes > 0)
            {
                string itemName = LocalizationMgr.Get(summonItem.Query<string>("title"));

                if(GetLocalSummonTimes(summonItem.Query<int>("type")) + summonTimes > limitTimes)
                {
                    if(summonTimes > 1)
                        DialogMgr.Notify(string.Format(LocalizationMgr.Get("SummonWnd_32"), itemName, summonTimes));
                    else
                        DialogMgr.Notify(string.Format(LocalizationMgr.Get("SummonWnd_33"), itemName));

                    return false;
                }
            }
        }


        int scriptNo = summonItem.Query<int>("summon_cost_script");

        LPCArray costList = new LPCArray();

        if(scriptNo > 0)
            costList = (LPCArray) ScriptMgr.Call(scriptNo, ME.user, summonItem.Query<LPCArray>("summon_cost_args"), subId);

        for(int i = 0; i < costList.Count; i++)
        {
            LPCMapping costMap = costList[i].AsMapping;

            if(costMap.ContainsKey("class_id"))
            {
                int classid = costMap.GetValue<int>("class_id");
                int cost_amount = costMap.GetValue<int>("amount")*summonTimes;

                // 取得玩家道具的数量
                int itemAmount = UserMgr.GetAttribItemAmount(ME.user, classid);

                if(cost_amount > itemAmount)
                {
                    DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("SummonWnd_18"), ItemMgr.GetName(classid)));
                    return false;
                }
            }
            else
            {
                string field = FieldsMgr.GetFieldInMapping(costMap);
                int cost_amount = costMap.GetValue<int>(field)*summonTimes;

                int my_money = ME.user.Query<int>(field);

                if(cost_amount > my_money)
                {
                    if(field.Equals("gold_coin"))
                        DialogMgr.ShowDailog(new CallBack(GotoShop, ShopConfig.GOLD_COIN_GROUP), string.Format(LocalizationMgr.Get("SummonWnd_19"), FieldsMgr.GetFieldName(field)));
                    else if(field.Equals("money"))
                        DialogMgr.ShowDailog(new CallBack(GotoShop, ShopConfig.MONEY_GROUP), string.Format(LocalizationMgr.Get("SummonWnd_19"), FieldsMgr.GetFieldName(field)));
                    else
                        DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("SummonWnd_18"), FieldsMgr.GetFieldName(field)));

                    return false;
                }
            }
        }

        if(!BaggageMgr.TryStoreToBaggage(ME.user, ContainerConfig.POS_PET_GROUP, summonTimes))
            return false;

        return true;
    }

    /// <summary>
    /// 前往商店
    /// </summary>
    public static void GotoShop(object para, params object[] _params)
    {
        if (!(bool)_params[0])
            return;

        if (!GuideMgr.IsGuided(4))
        {
            DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

            return;
        }

        // 前往商店
        GameObject wnd = WindowMgr.OpenWnd(QuickMarketWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;
        wnd.GetComponent<QuickMarketWnd>().Bind(para as string);
    }


    /// <summary>
    /// 获取钻石刷新次数
    /// </summary>
    public static int GetLocalSummonTimes(int classId)
    {
        LPCValue data = OptionMgr.GetLocalOption(ME.user, "summon_limits");

        if(data == null || !data.IsMapping)
            return 0;

        LPCMapping itemLimitData = data.AsMapping.GetValue<LPCMapping>(classId);

        if(itemLimitData == null)
            return 0;

        if(!Game.IsSameDay(itemLimitData.GetValue<int>("time"), TimeMgr.GetServerTime()))
            return 0;

        return itemLimitData.GetValue<int>("count");
    }

    /// <summary>
    /// 设置钻石刷新次数
    /// </summary>
    public static void SetLocalSummonTimes(int classId, int times)
    {
        LPCMapping data = LPCMapping.Empty;

        LPCValue summonData = OptionMgr.GetLocalOption(ME.user, "summon_limits");

        if(summonData != null && summonData.IsMapping)
            data = summonData.AsMapping;

        LPCMapping itemData = LPCMapping.Empty;

        if(!data.ContainsKey(classId))
        {
            itemData.Add("count", times);
            itemData.Add("time", TimeMgr.GetServerTime());

            data.Add(classId, itemData);
        } 
        else
        {
            itemData = data.GetValue<LPCMapping>(classId);

            if(!Game.IsSameWeek(itemData.GetValue<int>("time"), TimeMgr.GetServerTime()))
            {
                itemData.Add("count", times);
                itemData.Add("time", TimeMgr.GetServerTime());
            } 
            else
            {
                itemData.Add("count", itemData.GetValue<int>("count") + times);
                itemData.Add("time", TimeMgr.GetServerTime());
            }

            data.Add(classId, itemData);
        }

        //保存数据
        OptionMgr.SetLocalOption(ME.user, "summon_limits", LPCValue.Create(data));
    }

    #endregion
}