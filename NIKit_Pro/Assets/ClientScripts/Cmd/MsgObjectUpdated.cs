using LPC;

/// <summary>
/// 主角的信息更新了
/// </summary>
public class MsgObjectUpdated : MsgHandler
{
    public string GetName() { return "msg_object_updated"; }

    public void Go(LPCValue para)
    {
        LPCMapping m = para.AsMapping;

        Property ob = Rid.FindObjectByRid(m["rid"].AsString);
        if (ob == null)
            return;

        // TODO: followers字段处理

        // pos字段不允许通过此方法刷新
        m["dbase"].AsMapping.Remove("pos");

        // TODO: 调用接口进行字段过滤

        // 更新数据
        ob.dbase.Absorb(m["dbase"].AsMapping);

        // TODO : 通知对象被更新了

        // TODO : 通知物品更新
    }
}
