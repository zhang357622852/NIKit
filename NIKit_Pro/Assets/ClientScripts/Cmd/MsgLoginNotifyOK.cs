using LPC;

/// <summary>
/// 确认客户端获取了相应数据，可以显示界面
/// </summary>
public class MsgLoginNotifyOK : MsgHandler
{
    public string GetName() { return "msg_login_notify_ok"; }

    public void Go(LPCValue para)
    {
        LogMgr.Trace("获取登录数据完毕。");

        // 通知LoginMgr登陆成功
        LoginMgr.OnLoginOK();

        // 通知玩家登陆成功
        ME.LoginNotifyOK();
    }
}
