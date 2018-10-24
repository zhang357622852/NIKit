/// <summary>
/// EventNotifyMgr.cs
/// Created by WinMi 2018/10/24
/// 消息事件管理类
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;

public sealed class EventNotifyMgr: Singleton<EventNotifyMgr>
{
    // 委托定义
    public delegate void EventHandle(string eventId, params object[] paras);

    // key->eventId, key->listenerId
    private Dictionary<string, Dictionary<string, EventHandle>> mEventHandleDic = new Dictionary<string, Dictionary<string, EventHandle>>();

    #region 公共接口

    /// <summary>
    /// 注册事件
    /// </summary>
    public void RegisterEvent(string listenerId, string eventId, EventHandle eventHandle)
    {
        if (!mEventHandleDic.ContainsKey(eventId))
            mEventHandleDic[eventId] = new Dictionary<string, EventHandle>();

        if (mEventHandleDic[eventId].ContainsKey(listenerId))
            mEventHandleDic[eventId].Remove(listenerId);

        mEventHandleDic[eventId].Add(listenerId, eventHandle);
    }

    /// <summary>
    /// 注销事件
    /// </summary>
    public void UnregisterEvent(string listenerId)
    {
        if (string.IsNullOrEmpty(listenerId))
            return;

        foreach (string eventId in mEventHandleDic.Keys)
        {
            if (!mEventHandleDic[eventId].ContainsKey(listenerId))
                continue;

            mEventHandleDic[eventId].Remove(listenerId);
        }
    }

    /// <summary>
    /// 分发事件
    /// </summary>
    public void FireEvent(string eventId, params object[] paras)
    {
        Dictionary<string, EventHandle> tempDic;

        if (!mEventHandleDic.TryGetValue(eventId, out tempDic))
            return;

        if (tempDic.Count <= 0)
            return;

        List<EventHandle> eventHandleList = new List<EventHandle>();

        foreach (var item in tempDic)
        {
            if (item.Value == null)
            {
                mEventHandleDic[eventId].Remove(item.Key);

                continue;
            }

            eventHandleList.Add(item.Value);
        }

        try
        {
            foreach (EventHandle func in eventHandleList)
            {
                if (func == null)
                    continue;

                func(eventId, paras);
            }
        }
        catch (Exception e)
        {
            NIDebug.LogException(e);
        }
    }

    #endregion
}
