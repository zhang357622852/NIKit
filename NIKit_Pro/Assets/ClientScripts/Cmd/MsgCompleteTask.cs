using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class MsgCompleteTask : MsgHandler
{
    public string GetName() { return "msg_complete_task"; }

    public void Go(LPCValue para)
    {
        LPCMapping data = para.AsMapping;

        // 完成任务抛出事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_COMPLETE_TASK, MixedValue.NewMixedValue<int>(data.GetValue<int>("task_id")), true);
    }
}
