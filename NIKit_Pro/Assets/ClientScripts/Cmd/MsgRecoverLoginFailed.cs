using LPC;

/// <summary>
/// GS直连恢复登陆失败
/// </summary>
public class MsgRecoverLoginFailed : MsgHandler
{
    public string GetName() 
    {
        return "msg_recover_login_failed";
    }

    public void Go(LPCValue para)
    {
        // 抛出登陆失败
        EventMgr.FireEvent(EventMgrEventType.EVENT_LOGIN_FAILED, MixedValue.NewMixedValue<bool>(true));
    }
}
