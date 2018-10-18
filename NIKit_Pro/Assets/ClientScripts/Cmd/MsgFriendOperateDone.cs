using LPC;

/// <summary>
/// 好友相关操作执行完毕
/// </summary>
public class MsgFriendOperateDone : MsgHandler
{
    public string GetName() { return "msg_friend_operate_done"; }

    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping;

        // 操作结果详细处理
        FriendMgr.DoMsgFriendOperateDone(args.GetValue<string>("oper"),
            args.GetValue<int>("result"),
            args.GetValue<LPCMapping>("extra_data")
        );
    }
}