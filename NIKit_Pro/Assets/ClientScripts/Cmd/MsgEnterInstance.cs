/// <summary>
/// MsgEnterInstance.cs
/// Created by xuhd Feb/10/2015
/// 进入副本成功返回的消息
/// </summary>
using LPC;

public class MsgEnterInstance : MsgHandler
{
    public string GetName()
    {
        return "msg_enter_instance";
    }

    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping;

        // 副本rid为空则表示进入副本失败
        // 副本进入失败
        if (string.IsNullOrEmpty(args.GetValue<string>("rid")))
        {
            EventMgr.FireEvent(EventMgrEventType.EVENT_ENTER_INSTANCE_FAIL, MixedValue.NewMixedValue<LPCMapping>(args));
            return;
        }

        // 进入副本
        InstanceMgr.DoEnterInstance(ME.user,
            args.GetValue<string>("instance_id"),
            args.GetValue<string>("rid"),
            args.GetValue<int>("random_seed"),
            args.GetValue<LPCMapping>("fighter_map"),
            args.GetValue<LPCMapping>("defenders"),
            args.GetValue<LPCMapping>("attrib_bonus"),
            args.GetValue<LPCMapping>("extra_para"));

        EventMgr.FireEvent(EventMgrEventType.EVENT_ENTER_INSTANCE_OK, MixedValue.NewMixedValue<LPCMapping>(args));
    }
}
