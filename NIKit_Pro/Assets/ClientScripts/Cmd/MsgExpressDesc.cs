using LPC;

// 服务器刷新客户端的邮件描述信息
public class MsgExpressDesc : MsgHandler
{
    public string GetName()
    {
        return "msg_express_desc";
    }

    public void Go(LPCValue para)
    {
        // 管理模块记录
        MailMgr.DoCacheExpressDesc(para.AsMapping ["cookie"].AsString, para.AsMapping ["desc"].AsMapping);
    }
}
