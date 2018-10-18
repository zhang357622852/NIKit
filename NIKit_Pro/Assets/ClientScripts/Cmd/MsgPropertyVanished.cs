using LPC;

/// <summary>
/// 包裹中的物件消失了
/// </summary>
public class MsgPropertyVanished : MsgHandler
{
    public string GetName()
    {
        return "msg_property_vanished";
    }

    public void Go(LPCValue para)
    {
        LPCMapping m = para.AsMapping;

        Property ob = Rid.FindObjectByRid(m["rid"].AsString);
        if (ob == null)
            return;

        // 析构掉
        ob.Destroy();
    }
}
