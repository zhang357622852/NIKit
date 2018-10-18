using UnityEngine;
using System.Collections;
using LPC;

public class MsgSearchBaggageInfo : MsgHandler
{

    public string GetName()
    {
        return "msg_search_baggage_info";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping;

        if (args == null)
            return;

        // 抛出事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_SEARCH_BAGGAGE_INFO_SUCC, MixedValue.NewMixedValue(args));
    }
}
