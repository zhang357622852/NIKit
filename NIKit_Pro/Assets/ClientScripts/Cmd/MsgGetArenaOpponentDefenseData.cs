using LPC;

/// <summary>
/// 服务器通知竞技场对手详细信息消息
/// </summary>
public class MsgGetArenaOpponentDefenseData : MsgHandler
{
    public string GetName()
    {
        return "msg_get_arena_opponent_defense_data";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
    }
}
