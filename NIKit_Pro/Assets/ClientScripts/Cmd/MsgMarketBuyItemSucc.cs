/// <summary>
/// MsgMarketBuyItemSucc.cs
/// Created by fucj 2015-03-03
/// 商城购买物品成功
/// </summary>
using LPC;

public class MsgMarketBuyItemSucc : MsgHandler
{
    public string GetName()
    {
        return "msg_market_buy_item_succ";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping;

        // 抛出商城购买道具成功事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_BUY_ITEM_SUCCESS, MixedValue.NewMixedValue<LPCMapping>(args));
    }
}
