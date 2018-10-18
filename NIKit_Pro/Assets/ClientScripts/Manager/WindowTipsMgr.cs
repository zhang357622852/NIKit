/// <summary>
/// PetItemWnd.cs
/// Created by lic 2016-10-10
/// 窗口提示管理
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class WindowTipsMgr
{
    // 窗口提示缓存池
    private static List<Dictionary<string, object>> windowsList = new List<Dictionary<string, object>>();

    private static Dictionary<string, CallBack> mCbDic = new Dictionary<string, CallBack>();

    /// <summary>
    /// Gets the show tip window cookie.
    /// </summary>
    /// <value>The show tip window cookie.</value>
    public static string ShowTipWndCookie { get; private set; }

    #region 内部接口
    /// <summary>
    /// 关闭技能升级窗口回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="_params">Parameters.</param>
    private static void OnCloseWindow(object para, params object[] _params)
    {
        // 如果是正常窗口关闭
        if (_params.Length != 3)
            return;

        // 比对当先显示ShowTipWndCookie是否一致
        if (! string.Equals(ShowTipWndCookie, _params[1]))
            return;

        // 重置当前显示窗口id
        ShowTipWndCookie = null;

        // 缓存池为空
        if (windowsList.Count == 0)
        {
            // _params[2]  窗口名称
            string wndName =(string)_params[2];

            CallBack cb = null;

            if (!mCbDic.TryGetValue(wndName, out cb))
                return;

            // 执行回调
            if (cb != null)
                cb.Go();

            // 移除缓存
            mCbDic.Remove(wndName);

            return;
        }

        // 显示窗口
        Coroutine.DispatchService(ShowWindow());
    }

    /// <summary>
    /// 显示窗口
    /// </summary>
    /// <returns>The window.</returns>
    private static IEnumerator ShowWindow()
    {
        // 延迟等待下一帧处理
        yield return null;

        // 显示窗口
        TryShowTipWindow();
    }

    /// <summary>
    /// Dos the show window.
    /// </summary>
    private static void TryShowTipWindow()
    {
        do
        {
            // 缓存池为空
            // 如果当前有窗口正在显示
            if(windowsList.Count == 0 ||
                ! string.IsNullOrEmpty(ShowTipWndCookie))
                break;

            // 获取参数
            Dictionary<string, object> data = windowsList[0];

            // 打开窗口
            GameObject wnd = WindowMgr.OpenWnd((string)data["window_name"]);

            // 如果窗口打开失败
            if (wnd == null)
            {
                // 删除第一个元素
                windowsList.RemoveAt(0);
                continue;
            }

            // 记录当前显示窗口id
            ShowTipWndCookie = (string) data["cookie"];
            windowsList.RemoveAt(0);

            // 绑定数据
            wnd.SendMessage("ShowWindow", data, SendMessageOptions.RequireReceiver);

            // 显示窗口成功等待关闭
            break;

        } while(true);
    }

    #endregion

    #region  公共接口

    /// <summary>
    /// Dos the reset all.
    /// </summary>
    public static void DoResetAll()
    {
        // 重置窗口提示缓存池
        windowsList.Clear();

        // 重置回调缓存列表
        mCbDic.Clear();

        // 重置cookie
        ShowTipWndCookie = null;
    }

    /// <summary>
    /// 添加窗口
    /// </summary>
    /// <param name="windowName">Window name.</param>
    /// <param name="args">Arguments.</param>
    public static void AddWindow(string windowName, Dictionary<string, object> args, CallBack cb = null)
    {
        // 没有窗口名称不显示
        if (string.IsNullOrEmpty(windowName))
            return;

        // 构建参数
        args.Add("window_name", windowName);
        args.Add("call_back", new CallBack(OnCloseWindow));
        args.Add("cookie", Game.GetUniqueName("tip_wnd"));

        // 缓存回调
        if (! mCbDic.ContainsKey(windowName))
            mCbDic[windowName] = cb;

        // 添加到缓存列表中
        windowsList.Add(args);

        // 主动触发显示窗口
        TryShowTipWindow();
    }

    #endregion


   
}
