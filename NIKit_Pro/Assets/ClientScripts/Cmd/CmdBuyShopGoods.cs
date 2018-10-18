/// <summary>
/// CmdBuyShopGoods.cs
/// Created by zhaozy 2016/11/19
/// 购买商店道具
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public partial class Operation
{
    public class CmdBuyShopGoods
    {
        public string GetName()
        {
            return "cmd_buy_shop_goods";
        }

        // 消息入口
        public static bool Go(int itemIndex)
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_BUY_SHOP_GOODS", PackArgs("index", itemIndex));
            return true;
        }
    }
}
