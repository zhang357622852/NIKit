using LPC;

/// <summary>
/// 通知登录结果
/// </summary>
public class MsgContainerLoaded : MsgHandler
{
    public string GetName()
    {
        return "msg_container_loaded";
    }

    public void Go(LPCValue para)
    {
        LPCMapping m = para.AsMapping;

        // 将lazy_load标记纳入到dbase参数中
        m["dbase"].AsMapping["lazy_load"] = m["lazy_load"];

        ContainerMgr.ContainerLoaded(m["type"].AsInt, m["dbase"]);
    }
}
