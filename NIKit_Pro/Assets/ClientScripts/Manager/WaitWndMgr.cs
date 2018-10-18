/// <summary>
/// WaitCommunicatMgr.cs
/// Create by zhaozy 2015-07-13
/// 等待通信管理模块
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;
using LPC;



// 等待窗口类型
public enum WaitWndType
{
    WAITWND_TYPE_LOGIN,
    WAITWND_TYPE_NETWORK,
    WAITWND_TYPE_RESET,
    WAITWND_TYPE_RELOGIN,
}

/// <summary>
/// WaitCommunicatMgr登陆管理模块
/// </summary>
public static class WaitWndMgr
{

    #region 变量

    private static Dictionary<WaitWndType, List<string>> mWaitMap = new Dictionary<WaitWndType, List<string>>();

    #endregion

    #region 属性

    // 是否是返回登陆界面
    public static Dictionary<WaitWndType, List<string>> WaitMsgMap
    {
        get
        {
            return mWaitMap;
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 添加等待消息
    /// </summary>
    public static void AddWaitWnd(string source, WaitWndType wndType, CallBack cb = null, float waitTime = 0f, LPCMapping para = null)
    {
        // 添加缓存信息
        if (mWaitMap.ContainsKey(wndType) && mWaitMap[wndType].Count != 0)
        {
            // 没有数据添加数据
            if (mWaitMap[wndType].IndexOf(source) == -1)
                mWaitMap[wndType].Add(source);

            return;
        }

        // 添加数据
        mWaitMap.Add(wndType, new List<string>(){ source });

        // 获取窗口
        string wndName = CommunicationWnd.WndType + ((int)wndType).ToString();
        GameObject wnd = WindowMgr.GetWindow(wndName);

        // 如果窗口已经显现
        if (wnd != null && wnd.activeSelf)
            return;

        // 窗口还没有显示
        if (wnd == null)
            wnd = WindowMgr.CreateWindow(wndName, CommunicationWnd.PrefebResource);

        // 创建失败
        if (wnd == null)
        {
            LogMgr.Trace("没有窗口prefab，创建失败");
            return;
        }

        // 绑定窗口类型
        wnd.GetComponent<CommunicationWnd>().SetWndType(source, wndType, cb, waitTime, para);

        // 显示窗口
        WindowMgr.ShowWindow(wnd, false);
    }

    /// <summary>
    /// 移除等待消息(移除的是type对应的"子消息")
    /// </summary>
    public static void RemoveWaitWnd(string source, WaitWndType wndType)
    {
        // 没有该类型的等待窗口
        if (!mWaitMap.ContainsKey(wndType))
            return;

        // 移除数据
        mWaitMap[wndType].Remove(source);

        // 若type对应的list不为空
        if (mWaitMap[wndType].Count != 0)
            return;

        // 移除数据
        mWaitMap.Remove(wndType);

        // 获取窗口
        GameObject wnd = WindowMgr.GetWindow(CommunicationWnd.WndType + ((int)wndType).ToString());

        // 窗口对象不存在
        if (wnd == null)
            return;

        // hide窗口
        WindowMgr.DestroyWindow(wnd.name);
    }

    #endregion
}