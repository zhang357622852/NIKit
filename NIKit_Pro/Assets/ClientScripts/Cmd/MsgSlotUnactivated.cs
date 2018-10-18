using LPC;

/// <summary>
/// 服务器通知客户容器的某些格子没有激活
/// </summary>
public class MsgSlotUnactivated : MsgHandler
{
    public string GetName() { return "msg_slot_unactivated"; }
    
    public void Go(LPCValue para)
    {
        LPCMapping m = para.AsMapping;

        Container container = Rid.FindObjectByRid(m["container_rid"].AsString) as Container;
        if (container == null)
            return;

        ContainerMgr.MarkUnactivateSlot(container, m["begin"].AsString, m["count"].AsInt);
    }
}
