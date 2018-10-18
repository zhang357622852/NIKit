using LPC;

/// <summary>
/// 重置角色结果
/// </summary>
public class MsgReset : MsgHandler
{
    public string GetName()
    {
        return "msg_reset";
    }

    public void Go(LPCValue para)
    {
    }
}
