using LPC;

/// <summary>
/// 服务器通知客户端容器开启了
/// </summary>
public class MsgContainerOpened : MsgHandler
{
    public string GetName() { return "msg_container_opened"; }

    public void Go(LPCValue para)
    {
        // 构建参数
        LPCMapping args = para.AsMapping;
        int type = args.GetValue<int>("type");

        // 触发事件
        EventMgr.FireEvent (EventMgrEventType.EVENT_CONTAINER_OPEN, MixedValue.NewMixedValue(type));
    }
}
