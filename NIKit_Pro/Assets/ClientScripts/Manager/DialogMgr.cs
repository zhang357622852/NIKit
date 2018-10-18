/// <summary>
/// DialogMgr.cs
/// Created by fucj 2014-12-6
/// 确认框
/// </summary>

using System.Collections;
using UnityEngine;
using LPC;
using QCCommonSDK;

public static class DialogMgr
{
    #region 公共接口

    private static int mNotifyTime = 0;

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
    }

    /// <summary>
    /// 显示两个按钮的确认框
    /// </summary>
    /// <param name="_task">回掉函数</param>
    /// <param name="cost_item">消耗物品信息</param>
    /// <param name="content">内容</param>
    /// <param name="title">标题</param>
    /// <param name="ok_text">左边按钮名字</param>
    /// <param name="cancel_text">右边按钮名字</param>
    public static void ShowDailog(CallBack task, string content, string title = "", string ok_text = "", string cancel_text = "", bool isRespondMask = true, Transform parent = null)
    {
        string name = Game.GetUniqueName("MessageBoxWnd");

        // 取得确认框窗口
        GameObject wnd = WindowMgr.GetWindow(name);
        if (wnd == null)
        {
            wnd = WindowMgr.CreateWindow(name, MessageBoxWnd.PrefebResource, parent);
        }

        if (wnd == null)
        {
            LogMgr.Trace("创建MessageBoxWnd窗口失败！");
            return;
        }

        wnd.SetActive(true);
        wnd.GetComponent<MessageBoxWnd>().ShowDialog(task, content, title, ok_text, cancel_text, isRespondMask);
    }

    /// <summary>
    /// 显示中间一个按钮的确认框
    /// </summary>
    /// <param name="_task">回掉函数</param>
    /// <param name="cost_item">消耗物品信息</param>
    /// <param name="content">内容</param>
    /// <param name="title">标题</param>
    /// <param name="ok_text">左边按钮名字</param>
    /// <param name="cancel_text">右边按钮名字</param>
    public static void ShowSingleBtnDailog(CallBack task, string content, string title = "", string btn_text = "", bool isRespondMask = true, Transform parent = null)
    {
        string name = Game.GetUniqueName("MessageBoxWnd");

        // 取得确认框窗口
        GameObject wnd = WindowMgr.GetWindow(name);
        if (wnd == null)
        {
            wnd = WindowMgr.CreateWindow(name, MessageBoxWnd.PrefebResource, parent);
        }

        if (wnd == null)
            return;

        wnd.SetActive(true);
        wnd.GetComponent<MessageBoxWnd>().ShowSingleBtnDialog(task, content, title, btn_text, isRespondMask);
    }

    /// <summary>
    /// 显示中间一个按钮的确认框(简易)
    /// </summary>
    /// <param name="_task">回掉函数</param>
    /// <param name="cost_item">消耗物品信息</param>
    /// <param name="content">内容</param>
    /// <param name="title">标题</param>
    /// <param name="ok_text">左边按钮名字</param>
    /// <param name="cancel_text">右边按钮名字</param>
    public static void ShowSimpleSingleBtnDailog(CallBack task, string content, string title = "", string btn_text = "", Transform parent = null)
    {
        string name = Game.GetUniqueName("SimpleMessageBoxWnd");

        // 取得确认框窗口
        GameObject wnd = WindowMgr.GetWindow(name);
        if (wnd == null)
        {
            wnd = WindowMgr.CreateWindow(name, SimpleMessageBoxWnd.PrefebResource, parent);
        }

        if (wnd == null)
            return;

        wnd.SetActive(true);
        wnd.GetComponent<SimpleMessageBoxWnd>().ShowSingleBtnDialog(task, content, title, btn_text);
    }

    /// <summary>
    /// 显示两个按钮的确认框(简易)
    /// </summary>
    /// <param name="_task">回掉函数</param>
    /// <param name="cost_item">消耗物品信息</param>
    /// <param name="content">内容</param>
    /// <param name="title">标题</param>
    /// <param name="ok_text">左边按钮名字</param>
    /// <param name="cancel_text">右边按钮名字</param>
    public static void ShowSimpleDailog(CallBack task, string content, string title = "", string ok_text = "", string cancel_text = "", Transform parent = null)
    {
        string name = Game.GetUniqueName("SimpleMessageBoxWnd");

        // 取得确认框窗口
        GameObject wnd = WindowMgr.GetWindow(name);
        if (wnd == null)
        {
            wnd = WindowMgr.CreateWindow(name, SimpleMessageBoxWnd.PrefebResource, parent);
        }

        if (wnd == null)
            return;

        wnd.SetActive(true);
        wnd.GetComponent<SimpleMessageBoxWnd>().ShowDialog(task, content, title, ok_text, cancel_text);
    }

    /// <summary>
    /// 错误提示
    /// </summary>
    public static void Notify(string desc, string color = "ff5151")
    {
        Coroutine.DispatchService(_Notify(desc, color));
    }

    /// <summary>
    /// 协程通知
    /// </summary>
    private static IEnumerator _Notify(string desc, string color)
    {
        // 两次时间间隔太短，等待
        if (TimeMgr.GetTime() - mNotifyTime < 1)
            yield return new WaitForSeconds(0.3f);

        string name = Game.GetUniqueName("NotifyWnd");

        mNotifyTime = TimeMgr.GetTime();

        // 取得提示框窗口
        GameObject wnd = WindowMgr.GetWindow(name);
        if (wnd == null)
        {
            wnd = WindowMgr.CreateWindow(name, NotifyWnd.PrefebResource);
        }

        wnd.SetActive(true);
        wnd.GetComponent<UIPanel>().depth = WindowDepth.Notify;
        wnd.GetComponent<NotifyWnd>().Notify(desc, color);
    }

    #endregion
}
