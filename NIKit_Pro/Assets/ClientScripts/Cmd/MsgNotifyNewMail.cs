using LPC;

/// <summary>
/// 新邮件到达通知
/// </summary>
public class MsgNotifyNewMail : MsgHandler
{
    public string GetName()
    {
        return "msg_notify_new_mail";
    }

    public void Go(LPCValue para)
    {
        MailMgr.NotifyNewExpress();
    }
}
