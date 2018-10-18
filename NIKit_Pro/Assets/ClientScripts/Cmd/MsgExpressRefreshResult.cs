using LPC;

// 服务器刷新客户端的寄售信息
public class MsgExpressRefreshResult : MsgHandler
{
    public string GetName() { return "msg_express_refresh_result"; }

    public void Go(LPCValue para)
    {
#if false
        // 通知管理模块记录信息
        ExpressManager.RefreshResult(
            para.AsMapping["cookie"].AsString,
            para.AsMapping["start"].AsInt,
            para.AsMapping["total_count"].AsInt,
            para);
#endif
    }
}
