using LPC;

/// <summary>
/// 玩家使用道具
/// </summary>
public class MsgUseItem : MsgHandler
{
    public string GetName()
    {
        return "msg_use_item";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
    }
}
