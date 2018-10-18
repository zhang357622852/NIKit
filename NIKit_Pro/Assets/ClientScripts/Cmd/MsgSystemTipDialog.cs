using LPC;

/// <summary>
/// 收到一条重要提示消息给客户端，需要点击OK，无需反馈
/// </summary>
public class MsgSystemTipDialog : MsgHandler
{
    public string GetName()
    {
        return "msg_system_tip_dialog";
    }

    public void Go(LPCValue para)
    {
        // 转换数据格式
        LPCMapping args = para.AsMapping;

        // 给出提示窗口
        DialogMgr.ShowSimpleSingleBtnDailog(
            null,
            LocalizationMgr.GetServerDesc(args.GetValue<LPCValue>("content")),
            LocalizationMgr.GetServerDesc(args.GetValue<LPCValue>("title")),
            LocalizationMgr.GetServerDesc(args.GetValue<LPCValue>("btn_text"))
        );
    }
}
