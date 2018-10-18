using LPC;

/// <summary>
/// 通知登录结果
/// </summary>
public class MsgDisconnect : MsgHandler
{
    public string GetName() { return "msg_disconnect"; }

    public void Go(LPCValue para)
    {
        if (Communicate.IsConnected())
            Communicate.Disconnect();
    }
}
