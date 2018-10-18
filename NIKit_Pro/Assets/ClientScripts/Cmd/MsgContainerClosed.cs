using LPC;

/// <summary>
/// 服务器通知客户端容器开启了
/// </summary>
public class MsgContainerClosed : MsgHandler
{
    public string GetName() { return "msg_container_closed"; }

    public void Go(LPCValue para)
    {
        ContainerMgr.ContainerClosed(para.AsMapping["rid"].AsString);
    }
}
