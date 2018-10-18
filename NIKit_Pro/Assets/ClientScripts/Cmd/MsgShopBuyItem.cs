/// <summary>
/// MsgShopBuyItem.cs
/// Created by xuhd Feb/12/2015
/// 市场购买成功的消息
/// </summary>
using LPC;

public class MsgShopBuyItem : MsgHandler
{
    public string GetName()
    {
        return "msg_shop_buy_item";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
    }
}
