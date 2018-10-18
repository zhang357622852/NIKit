/// <summary>
/// StoreMgr.cs
/// Created by xuhd Apri/13/2015
/// 仓库管理器
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LPC;

public static class StoreMgr
{
    #region 外部接口

    /// <summary>
    /// 根据class_id获取仓库道具.
    /// </summary>
    public static Property GetStoreItemByClassId(Property who, int classId)
    {
        // 取得仓库的物品
        Dictionary<string, Property> allItems = (who as User).baggage.GetPageCarry(ContainerConfig.POS_STORE_GROUP);
        if (allItems == null || allItems.Count <= 0)
            return null;

        Property item = null;
        foreach (KeyValuePair<string, Property> kv in allItems)
        {
            if (kv.Value.Query<int>("class_id") != classId)
                continue;

            item = kv.Value;
            break;
        }

        return item;
    }

    #endregion

    #region 内部方法

    /// <summary>
    /// 自动整理包裹时，获取物品的权重
    /// </summary>
    private static string GetItemSortWeight(Property ob)
    {
        /// 规则：new>类型>等级>颜色>战力。
        /// 类型的顺序设定：武器>手套，盔甲>头盔，鞋子>腰带，项链>戒指，药品>宝箱。
        /// 道具等级顺序设定：高级别>低级别
        /// 道具颜色顺序设定：暗金>金>蓝>白

        // new标识
        int isNew = ob.QueryTemp<int>("is_new");

        // 类型标识
        int itemType = ob.Query<int>("item_type");

        // 等级标识
        int levelRequest = ob.Query<int>("level_request");

        // 品质标识
        int rank = ob.Query<int>("rank");

        // 返回权重
        return string.Format("@{0}{1:D5}{2:D3}{3:D2}", isNew, itemType, levelRequest, rank);
    }

    #endregion
}
