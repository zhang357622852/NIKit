using LPC;

/// <summary>
/// 帮派相关操作执行完毕
/// </summary>
public class MsgGangOperateDone : MsgHandler
{
    public string GetName() { return "msg_gang_operate_done"; }

    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping;

        // 操作结果详细处理
        GangMgr.DoMsgGangOperateDone(args.GetValue<string>("oper"),
            args.GetValue<int>("result"),
            args.GetValue<LPCValue>("extra_data")
        );
    }
}