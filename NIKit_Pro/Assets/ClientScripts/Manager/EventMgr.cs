/// <summary>
/// EventMgr.cs
/// Created by zhaozy 2014-11-21
/// 事件管理
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 事件管理
/// </summary>
public static class EventMgr
{
    #region 变量

    /// <summary>
    /// 事件回调 
    /// </summary>
    public delegate void EventHook(int eventId,MixedValue para);

    // 回调列表
    private static Dictionary<int, Dictionary<string, EventHook>> EventHooks = new Dictionary<int, Dictionary<string, EventHook>>();

    #endregion

    #region 内部函数

    /// <summary>
    /// 异步fire事件实际处理函数
    /// </summary>
    private static IEnumerator NonSyncFireEvent(int eventId, MixedValue para)
    {
        // 确保异步操作，需要延迟到下一帧执行
        yield return null;

        // fire事件
        FireEventImpl(eventId, para);
    }

    /// <summary>
    /// fire事件实际处理函数
    /// </summary>
    private static void FireEventImpl(int eventId, MixedValue para)
    {
        // 没有登记回调
        if (!EventHooks.ContainsKey(eventId))
            return;

        // 获取事件信息
        Dictionary<string, EventHook> Hooks = EventHooks[eventId];

        // 没有事件不处理
        if (Hooks.Count == 0)
            return;

        // 需要执行回调的列表
        List<EventHook> eventHookList = new List<EventHook>();

        // 遍历各个回调收集需要执行的回调
        foreach (string listenerId in Hooks.Keys)
        {
            // 获取回调
            EventHook fun = Hooks[listenerId];

            // 函数不存在
            if (fun == null)
            {
                EventHooks[eventId].Remove(listenerId);
                continue;
            }

            // 添加到回调列表中
            eventHookList.Add(fun);
        }

        try
        {
            // 遍历各个回调
            foreach (EventHook fun in eventHookList)
            {
                // 函数不存在
                if (fun == null)
                    continue;

                // 执行回调
                fun(eventId, para);
            }
        } catch (Exception e)
        {
            LogMgr.Exception(e);
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 注册一个事件fire回调
    /// </summary>
    public static void RegisterEvent(string listenerId, int eventId, EventHook fun)
    {
        // 还没有注册过该类型心跳
        if (!EventHooks.ContainsKey(eventId))
            EventHooks[eventId] = new Dictionary<string, EventHook>();

        // 如果已经注册过了同名的事件给出提示
        if (EventHooks[eventId].ContainsKey(listenerId))
            EventHooks[eventId].Remove(listenerId);

        // 记录数据
        EventHooks[eventId].Add(listenerId, fun);
    }

    /// <summary>
    /// 移除一个事件回调
    /// </summary>
    public static void UnregisterEvent(string listenerId)
    {
        if (string.IsNullOrEmpty(listenerId))
            return;

        // 遍历列表删除listenerId的事件回调
        foreach (int eventId in EventHooks.Keys)
        {
            // 没有需要删除的listenerId
            if (!EventHooks[eventId].ContainsKey(listenerId))
                continue;

            // 移除数据
            EventHooks[eventId].Remove(listenerId);
        }
    }

    /// <summary>
    /// fire事件
    /// </summary>
    /// <param name="eventId">事件类型</param>
    /// <param name="para">事件参数</param>
    /// <param name="isSync">是否同步调用</param>
    public static void FireEvent(int eventId, MixedValue para, bool isSync = false, bool fixedMode = false)
    {
        if (isSync)
            // 同步调用
            FireEventImpl(eventId, para);
        else
            // 异步调用
            Coroutine.DispatchService(NonSyncFireEvent(eventId, para), fixedMode);
    }

    #endregion
}
