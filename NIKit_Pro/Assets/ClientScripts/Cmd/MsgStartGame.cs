using LPC;

/// <summary>
/// 确认客户端连接到用户域上，开始游戏
/// </summary>
public class MsgStartGame : MsgHandler
{
    public string GetName() { return "msg_start_game"; }

    public void Go(LPCValue para)
    {
        LogMgr.Trace("开始游戏。");
        ME.StartGame();
    }
}
