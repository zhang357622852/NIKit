using LPC;

public class MsgRefreshExpressList : MsgHandler
{
    public string GetName()
    {
        return "msg_refresh_express_list";
    }

    /// <summary>
    /// 消息入口
    /// </summary>
    /// <param name="para">Para.</param>
    public void Go(LPCValue para)
    {
        // 标记有新邮件重新获取邮件列表
        MailMgr.NotifyNewExpress();

        // 重新获取邮件列表
        MailMgr.RequestGetExpressList();

        // 重置标识
        MailMgr.ResetNewExpress(false);

        // 抛出事件刷新界面
        EventMgr.FireEvent(EventMgrEventType.EVENT_REFRESH_EXPRESS_LIST, null);
    }
}
