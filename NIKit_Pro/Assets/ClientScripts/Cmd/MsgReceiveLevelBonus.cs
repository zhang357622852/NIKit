using LPC;

/// <summary>
/// 领取等级奖励成功消息
/// </summary>
public class MsgReceiveLevelBonus : MsgHandler
{
    public string GetName()
    {
        return "msg_receive_level_bonus";
    }

    public void Go(LPCValue para)
    {
    }
}
