/// <summary>
/// CommunicationWnd.cs
/// Created by cql 2015/07/08
/// 通信等待窗口
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class CommunicationWnd : WindowBase<CommunicationWnd>
{
    #region 公共字段

    // 文字提示
    public UILabel mTips;
    public UISprite mBG;

    #endregion

    #region 内部成员

    // 窗口类型
    private WaitWndType wndType;
    private LPCMapping  extraPara;
    private CallBack    callBack = null;

    // 重复登陆信息
    private float remainTime = 0f;

    // 窗口id
    private string wndId = string.Empty;

    #endregion

    #region 内部接口

    // Use this for initialization
    void Start()
    {
        // 屏幕适配
        UIPanel panel = GetComponent<UIPanel>();
        panel.SetAnchor(WindowMgr.UIRoot);
        panel.leftAnchor.Set(0f, -10f);
        panel.rightAnchor.Set(1f, 10f);
        panel.bottomAnchor.Set(0f, -10f);
        panel.topAnchor.Set(1f, 10f);
    }

    /// <summary>
    /// OnDestroy
    /// </summary>
    void OnDestroy()
    {
        // 如果没有窗口id
        if (string.IsNullOrEmpty(wndId))
            return;

        // 注销事件监听
        EventMgr.UnregisterEvent(wndId);
    }

    /// <summary>
    /// 登陆失败.
    /// </summary>
    void OnLoginFailed(int eventId, MixedValue para)
    {
        // 如果不是恢复登陆等待，不处理
        if (WaitWndType.WAITWND_TYPE_RELOGIN != wndType)
            return;

        // 如果登陆失败为true, 表示gs直连登陆失败（服务器对象已经不存在）
        // 需要重新发起账号登陆
        if (para.GetValue<bool>())
            extraPara.Add("recover_times", LoginMgr.MAX_RECOVER_TIMES);

        // 重置remainTime
        remainTime = 0f;
    }

    /// <summary>
    /// 重绘窗口
    /// </summary>
    void redraw()
    {
        switch (wndType)
        {
            // 登录类型
            case WaitWndType.WAITWND_TYPE_LOGIN:
                mBG.alpha = 0.8f;
                mTips.text = LocalizationMgr.Get("LoginMgr_7");
                break;

            // 目前默认为等待服务器类型
            case WaitWndType.WAITWND_TYPE_NETWORK:
                mBG.alpha = 0.8f;
                mTips.text = string.Empty;
                break;

            // 目前默认为等待服务器类型
            case WaitWndType.WAITWND_TYPE_RELOGIN:
                mBG.alpha = 0.8f;
                mTips.text = string.Empty;
                break;

            // 目前默认为等待服务器类型
            default:
                mBG.alpha = 0.8f;
                mTips.text = string.Empty;
                break;
        }
    }

    /// <summary>
    /// 帧更新
    /// </summary>
    void Update()
    {
        // 不需要倒计时回调
        if (callBack == null)
            return;

        // 计算剩余时间
        remainTime -= Time.unscaledDeltaTime;

        // 如果还没有到结束
        if (remainTime > 0)
            return;

        // 执行回调
        callBack.Go(extraPara);

        // 重置callBack为null
        callBack = null;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 设置窗口绑定
    /// </summary>
    public void SetWndType(string source, WaitWndType type, CallBack cb, float waitTime, LPCMapping para)
    {
        // 绑定窗口数据
        wndType = type;
        callBack = cb;
        extraPara = para;
        remainTime = waitTime;

        // 网络连接失败
        wndId = Game.NewCookie(source);
        EventMgr.RegisterEvent(wndId, EventMgrEventType.EVENT_LOGIN_FAILED, OnLoginFailed);

        // 重绘窗口
        redraw();
    }

    #endregion
}
