/// <summary>
/// UserMgr.cs
/// Copy from zhangyg 2014-10-22
/// 玩家管理
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

/// <summary>
/// 玩家管理
/// </summary>
public static class UserMgr
{
    #region 公共接口

    /// <summary>
    /// Dos the upgrade.
    /// </summary>
    /// <param name="ob">Ob.</param>
    public static void DoUpgrade(Property ob)
    {
        // 刷新属性
        PropMgr.RefreshAffect(ob);
    }

    /// <summary>
    /// Collects the user dbase.
    /// </summary>
    public static LPCMapping CollectUserDbase(Property user)
    {
        LPCMapping dbaseMap = LPCMapping.Empty;

        // 必须参数（level）
        dbaseMap.Add("level", user.GetLevel());

        // 获取服务器指定收集参数
        LPCArray specificArgs = user.QueryTemp<LPCArray>("specific_collect_attrib");
        if (specificArgs == null || specificArgs.Count == 0)
            return dbaseMap;

        // 遍历参数逐个参数收集
        string attrib = string.Empty;
        for (int i = 0; i < specificArgs.Count; i++)
        {
            attrib = specificArgs[i].AsString;
            dbaseMap.Add(attrib, user.QueryAttrib(attrib));
        }

        // 返回收集参数
        return dbaseMap;
    }

    /// <summary>
    /// 创建玩家对象
    /// </summary>
    /// <param name="rid">玩家RID</param>
    /// <param name="data">玩家的数据</param>
    public static User CreateUser(string rid, LPCMapping data)
    {
        // 构建User对象
        User user = new User(PropertyMgr.ConvertDbase(data));

        // 刷新玩家属性
        PropMgr.RefreshAffect(user);

        // 构建完成
        return user;
    }

    /// <summary>
    /// 获取玩家属性道具数量
    /// </summary>
    public static int GetAttribItemAmount(Property user, int classId, int subId = 0)
    {
        // 获取道具对应的属性
        string attrib = FieldsMgr.GetAttribByClassId(classId);

        // 该道具不是属性道具
        if (string.IsNullOrEmpty(attrib))
            return 0;

        LPCValue attribValue = user.Query<LPCValue>(attrib);
        if (attribValue == null)
            return 0;

        // 如果是int属性数据
        if (attribValue.IsInt)
            return attribValue.AsInt;

        // 如果属性是mapping
        if (attribValue.IsMapping)
            return attribValue.AsMapping.GetValue<int>(subId);

        // 返回道具数量
        return 0;
    }

    /// <summary>
    /// 检测属性道具是否足够
    /// </summary>
    public static bool CheckUserAttribItem(Property user, LPCMapping costInfo)
    {
        // 如果不消耗任何道具，足够花销
        if (costInfo.Count == 0)
            return true;

        string attrib;
        LPCMapping amountMap;
        LPCMapping costAmount;

        // 遍历先检查各种消耗是否足够消耗
        foreach (int classId in costInfo.Keys)
        {
            // 获取道具对应的属性
            attrib = FieldsMgr.GetAttribByClassId(classId);

            // 该道具不是属性道具
            if (string.IsNullOrEmpty(attrib))
            {
                LogMgr.Trace("非属性道具，请检视扣除消耗流程!!!");
                return false;
            }

            // 如果是int型
            if (costInfo[classId].IsInt)
            {
                // 属性不足以支付
                if (user.QueryAttrib(attrib) < costInfo[classId].AsInt)
                    return false;

                // 属性足够
                continue;
            }

            // 如果是mapping
            if (costInfo[classId].IsMapping)
            {
                // 获取属性
                amountMap = user.Query<LPCMapping>(attrib);
                if (amountMap == null)
                    return false;

                costAmount = costInfo[classId].AsMapping;
                foreach (int id in costAmount.Keys)
                {
                    // 属性不足以支付
                    if (amountMap[id].AsInt < costAmount[id].AsInt)
                        return false;
                }

                // 属性足够
                continue;
            }

            // 其他格式，暂时不支持
            LogMgr.Trace("无效的属性道具消耗!!!");
        }

        // 返回属性足够
        return true;
    }

    /// <summary>
    /// 获取玩家宠物
    /// </summary>
    /// <param name="who">Who.</param>
    /// <param name="getStore">是否获取仓库中的宠物.</param>
    /// <param name="getMaterialPet">是否获取材料宠物</param>
    /// <param name="limitLevel">获取多少级以上的宠物</param>
    public static List<Property> GetUserPets(Property who, bool getStore, bool getMaterialPet, int limitLevel = 1)
    {
        List<Property> items = new List<Property>();

        if (who == null)
            return items;

        // 取得包裹的物品
        List<Property> all_items = BaggageMgr.GetItemsByPage(who, ContainerConfig.POS_PET_GROUP);
        if (all_items == null || all_items.Count <= 0)
            return items;

        // 获取仓库中的所有宠物
        if (getStore)
            all_items.AddRange(BaggageMgr.GetItemsByPage(who, ContainerConfig.POS_STORE_GROUP));

        // 遍历宠物抽取满足条件的宠物
        for (int i = 0; i < all_items.Count; i++)
        {
            Property item = all_items[i];
            if (item == null)
                continue;

            // 不需要材料宠物
            if (! getMaterialPet &&
                MonsterMgr.IsMaterialMonster(item.GetClassID()))
                continue;

            // 不满足等级限制
            if (item.GetLevel() < limitLevel)
                continue;

            // 添加到列表中
            items.Add(item);
        }

        // 返回数据
        return items;
    }

    /// <summary>
    /// 显示升级效果窗口
    /// </summary>
    public static void ShowUpgradeEffectWnd(int preLevel)
    {
        // 打开升级效果窗口
        GameObject wnd = WindowMgr.OpenWnd(UpgradeEffectWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<UIPanel>().depth = WindowDepth.UpgradeEffect;

        wnd.GetComponent<UpgradeEffectWnd>().Bind(preLevel);
    }

    public static string GetGenderIcon(int gender)
    {
        if (gender == CharConst.MALE)
            return "male";
        else
            return "female";
    }

    #endregion
}
