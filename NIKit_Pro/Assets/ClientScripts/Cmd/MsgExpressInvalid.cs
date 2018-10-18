using LPC;

// 服务器刷新客户端的邮件描述信息
public class MsgExpressInvalid : MsgHandler
{
    public string GetName()
    {
        return "msg_express_invalid";
    }

    public void Go(LPCValue para)
    {
        // 转换参数
        string experssRid = para.AsMapping.GetValue<string>("experss_rid");

        // 邮件失效
        MailMgr.DoExpressInvalid(experssRid);
    }
}
