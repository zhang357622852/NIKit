using LPC;

/// <summary>
/// 收到一条重要提示消息给客户端，需要点击OK，无需反馈
/// </summary>
public class MsgDialogOK : MsgHandler
{
    public string GetName() { return "msg_dialog_ok"; }

    public void Go(LPCValue para)
    {
        // Alart.Show(para.AsMapping["msg"].AsString);
    }
}
