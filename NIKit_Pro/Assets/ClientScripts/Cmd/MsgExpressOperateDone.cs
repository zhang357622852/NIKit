using LPC;

// 服务器刷新客户端一个邮寄相关操作结束
public class MsgExpressOperateDone : MsgHandler
{
    public string GetName()
    {
        return "msg_express_operate_done";
    }

    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping;

        // 不是获取邮件
        if (!string.Equals(args.GetValue<string>("oper"), "get_list"))
            return;

        // 邮件系统已经重新初始化
        MailMgr.IsInit = true;
    }
}
