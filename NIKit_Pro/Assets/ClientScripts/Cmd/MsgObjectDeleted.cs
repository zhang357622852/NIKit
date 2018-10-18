using LPC;

/// <summary>
/// 对象的字段信息删除了
/// </summary>
public class MsgObjectDeleted : MsgHandler
{
    public string GetName() { return "msg_object_deleted"; }

    public void Go(LPCValue para)
    {
        LPCMapping m = para.AsMapping;

        Property ob = Rid.FindObjectByRid(m["rid"].AsString);
        if (ob == null)
            return;

        foreach (LPCValue v in m["fields"].AsArray.Values)
            ob.dbase.Delete(v.AsString);
    }
}
