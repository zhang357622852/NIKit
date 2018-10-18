/// <summary>
/// MsgNotifyDetailAppearance.cs
/// Created by fengxl 2015-3-25
/// 通过名字所查询到的玩家信息
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class MsgNotifyDetailAppearance : MsgHandler
{

    public string GetName()
    {
        return "msg_notify_detail_appearance";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping;

        // 抛出事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_SEARCH_DETAIL_INFO_SUCC, MixedValue.NewMixedValue(args));
    }
}

