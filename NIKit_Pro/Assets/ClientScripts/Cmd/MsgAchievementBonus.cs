/// <summary>
/// MsgAchievementBonus.cs
/// Created by zhaozy 2015/0707
/// 领取成就奖励消息
/// </summary>
using LPC;

public class MsgAchievementBonus : MsgHandler
{
    public string GetName()
    {
        return "msg_achievement_bonus";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
    }
}
