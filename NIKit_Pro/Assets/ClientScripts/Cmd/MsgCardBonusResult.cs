using LPC;

/// <summary>
/// 通知获得的礼包信息
/// </summary>
public class MsgCardBonusResult: MsgHandler
{
    public string GetName()
    {
        return "msg_card_bonus_result";
    }

    public void Go(LPCValue para)
    {
    }
}