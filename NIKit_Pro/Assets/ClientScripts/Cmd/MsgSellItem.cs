using LPC;

/// <summary>
/// 玩家出售道具
/// </summary>
public class MsgSellItem : MsgHandler
{
    public string GetName()
    {
        return "msg_sell_item";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
    }
}
