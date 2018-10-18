using LPC;

/// <summary>
/// 玩家召唤宠物
/// </summary>
public class MsgSummonPet : MsgHandler
{
    public string GetName()
    {
        return "msg_summon_pet";
    }

    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping;
        LPCMapping infoMap = args.GetValue<LPCMapping>("summon_map");

        // 触发事件
        EventMgr.FireEvent (EventMgrEventType.EVENT_SUMMON_SUCCESS, MixedValue.NewMixedValue(infoMap));
    }
}