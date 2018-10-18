using LPC;

/// <summary>
/// 通知帐号信息
/// </summary>
public class MsgNotifyAccountInfo : MsgHandler
{
    public string GetName()
    {
        return "msg_notify_account_info";
    }

    public void Go(LPCValue para)
    {
        LPCMapping data = para.AsMapping;
        Communicate.AccountInfo.Set("account", data ["notify_account"].AsString);
    }
}
