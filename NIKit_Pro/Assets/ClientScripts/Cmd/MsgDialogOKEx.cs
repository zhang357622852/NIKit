using LPC;

/// <summary>
/// 收到一条重要提示消息给客户端，需要点击OK，无需反馈
/// </summary>
public class MsgDialogOKEx : MsgHandler
{
    public string GetName()
    {
        return "msg_dialog_ok_ex";
    }

    public void Go(LPCValue para)
    {
        string tip = LocalizationMgr.GetServerDesc(para.AsMapping.GetValue<LPCValue>("msg"));

        if (string.IsNullOrEmpty(tip))
            return;

        DialogMgr.Notify(tip);
    }
}
