using LPC;

/// <summary>
/// 确认消息
/// </summary>
public class MsgCmdVerify : MsgHandler
{
    public string GetName() { return "msg_cmd_verify"; }

    public void Go(LPCValue para)
    {
    }
}
