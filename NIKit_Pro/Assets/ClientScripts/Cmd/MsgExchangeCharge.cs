using LPC;

/// <summary>
/// 充值兑换消息
/// </summary>
public class MsgExchangeCharge : MsgHandler
{
    public string GetName()
    {
        return "msg_exchange_charge";
    }

    public void Go(LPCValue para)
    {
        // 玩家已经离开游戏
        if (ME.user == null || ME.user.IsDestroyed)
            return;

        // 增加数据para
        ME.user.AddAttrib(para.AsMapping);
    }
}
