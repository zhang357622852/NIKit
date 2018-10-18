/// <summary>
/// VersionTipMgr.cs
/// create by cql 2015/07/06
/// 版本提示管理
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;
using LPC;

/// 宠物管理
public class VersionTipMgr
{

    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 注册玩家登陆成功
        EventMgr.RegisterEvent("ShowVersionTips", EventMgrEventType.EVENT_LOGIN_OK, WhenLoginOk);
    }

    /// <summary>
    /// 显示版本提示窗口.
    /// </summary>
    public static void ShowVersionTipWnd()
    {
        // 获取版本提示
        LPCMapping tipData = ME.user.QueryTemp<LPCMapping>("version_tips");

        if (tipData == null || tipData.Count == 0)
            return;

        // 获取提示标识
        string cookie = tipData.GetValue<string>("cookie");
        if (string.IsNullOrEmpty(cookie))
            return;

        // 本地存储的已经提示过的消息
        LPCValue versionTipsOption = OptionMgr.GetOption(ME.user, "version_tips");
        if (versionTipsOption == null)
            return;

        string versionTips = versionTipsOption.AsString;
        string[] tips = versionTips.Split(',');
        foreach (var tip in tips)
        {
            // 已经提示过了
            if (tip.Equals(cookie))
                return;
        }

        // 获取提示信息
        string title = tipData.GetValue<string>("title");
        string desc = tipData.GetValue<string>("desc");
        string btnTip = tipData.GetValue<string>("btn_tip");
        string redirectUrl = tipData.GetValue<string>("url");

        // 获取窗口
        GameObject wnd = WindowMgr.GetWindow(VersionTipWnd.WndType);
        if (wnd == null)
            wnd = WindowMgr.CreateWindow(VersionTipWnd.WndType, VersionTipWnd.PrefebResource);

        // 创建失败
        if (wnd == null)
        {
            LogMgr.Trace("没有窗口prefab，创建失败");
            return;
        }

        WindowMgr.ShowWindow(wnd, false);
        wnd.GetComponent<UIPanel>().depth = WindowDepth.VersionTip;
        wnd.GetComponent<VersionTipWnd>().SetContent(title, desc, btnTip, redirectUrl);

        // 记录数据
        versionTips += versionTips.Length > 0 ? "," + cookie : "" + cookie;
        OptionMgr.SetOption(ME.user, "version_tips", LPCValue.Create(versionTips));
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 登陆成功回调
    /// </summary>
    private static void WhenLoginOk(int eventId, MixedValue para)
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 打开版本提示窗口
        ShowVersionTipWnd();
    }

    #endregion

}
