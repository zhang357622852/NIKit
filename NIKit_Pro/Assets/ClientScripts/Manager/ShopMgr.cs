/// <summary>
/// ShopMgr.cs
/// Created by fucj 2014-12-09
/// 商店
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public static class ShopMgr
{
    #region 公共接口

    /// <summary>
    /// 购买物品
    /// </summary>
    public static bool Buy(Property who, Property item, int shopType)
    {
        // 道具对象不存在
        if (item == null)
            return false;

        // 取得购买价格
        LPCMapping buyPrice = item.Query<LPCMapping>("buy_price");

        // 道具没有购买价格，不允许购买
        if (buyPrice == null)
            return false;

        // 判断购买道具消耗是否足够
        foreach (string key in buyPrice.Keys)
        {
            // 金钱不足
            if (who.QueryAttrib(key) < buyPrice[key].AsInt)
                return false;
        }

        // 调用公式判断一次
        if (!CAN_BUY_SHOP_ITEM.Call(who, item))
            return false;

        // 向服务端发消息CmdShopBuyItem
        Operation.CmdShopBuyItem.Go(item.GetRid(), shopType);

        return true;
    }

    /// <summary>
    /// 使用钻石刷新货物
    /// </summary>
    public static void UseCoinRefreshGoods(Property who)
    {
        // 刷新消耗
        LPCMapping costMap = CALC_REFRESH_GOODS_COST.Call(who);
        if (costMap == null)
            return;

        // 检测消耗是否足够
        foreach (string key in costMap.Keys)
        {
            // 消耗不够
            if (who.Query<int>(key) < costMap[key].AsInt)
                return;
        }

        // 调用公式检查一遍
        if (!CAN_REFRESH_SHOP_GOODS.Call(who))
            return;

        // 需要通知服务器存储数据
        Operation.CmdRefreshShop.Go();
    }

    /// <summary>
    /// 市集购买商品
    /// </summary>
    /// <param name="itemOb">Item ob.</param>
    public static void ShopBuy(Property itemOb)
    {
        LPCMapping cost = PropertyMgr.GetBuyPrice(itemOb);

        if (cost == null || cost.Count == 0)
            return;

        string fields = FieldsMgr.GetFieldInMapping(cost);

        if (ME.user.Query<int>(fields) < cost.GetValue<int>(fields))
        {
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("ShopMgr_1"), FieldsMgr.GetFieldName(fields)));
            return;
        }

        // 获取商品列表
        LPCArray shopList = ME.user.Query<LPCArray>("shop_goods");

        int index = -1;
        foreach (LPCValue value in shopList.Values)
        {
            index++;

            if (value.IsInt)
                continue;

            if (value.AsMapping.GetValue<string>("rid").Equals(itemOb.Query<string>("org_rid")))
                break;
        }

        // 通知服务器购买物品
        Operation.CmdBuyShopGoods.Go(index);
    }

    #endregion
}
