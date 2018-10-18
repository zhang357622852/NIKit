/// <summary>
/// CmdShopBuyItem.cs
/// Created by xuhd Fab/12/2015
/// 市场购买物品
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdShopBuyItem : CmdHandler
    {
        public string GetName()
        {
            return "cmd_shop_buy_item";
        }

        /// <summary>
        /// 商店刷新
        /// </summary>
        public static bool Go(string rid, int shopType)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            LogMgr.Trace("[CmdShopBuyItem.cs] 市场购买, shopType = {0}, rid = {1}", shopType, rid);
            LPCValue agrs = PackArgs("rid", rid, "shop_type", shopType);
            Communicate.Send2GS("CMD_SHOP_BUY_ITEM", agrs);
            return true;
        }
    }
}
