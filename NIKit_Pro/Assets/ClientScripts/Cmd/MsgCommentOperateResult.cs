using LPC;

/// <summary>
/// 评论相关操作执行完毕
/// </summary>
public class MsgCommentOperateResult : MsgHandler
{
    public string GetName() { return "msg_comment_operate_result"; }

    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping;

        // 操作结果详细处理
        CommentMgr.DoMsgCommentOperateDone(args.GetValue<string>("oper"),
            args.GetValue<int>("result"),
            args.GetValue<LPCValue>("extra_data")
        );
    }
}