using LPC;

/// <summary>
/// 角色的位置更新了
/// </summary>
public class MsgNotifyPosUpdated : MsgHandler
{
    public string GetName() { return "msg_notify_pos_updated"; }

    public void Go(LPCValue para)
    {
        // 取得服务器数据
        LPCMapping m = para.AsMapping;

        string pos = m["pos"].AsString;
        Property ob = Rid.FindObjectByRid(m["rid"].AsString);
        if (ob == null)
            return;

        ob.move.SetPos(pos);
    }
}
