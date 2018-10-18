using LPC;

/// <summary>
/// 获取公会详情
/// </summary>
public class MsgGetGangDetails : MsgHandler
{
    public string GetName() { return "msg_get_gang_details"; }

    public void Go(LPCValue para)
    {
        // 获取公会详情成功
        EventMgr.FireEvent(EventMgrEventType.EVENT_GET_GANG_DETAILS, MixedValue.NewMixedValue<LPCMapping>(para.AsMapping));
    }
}