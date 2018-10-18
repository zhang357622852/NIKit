/// <summary>
/// CreateGangAnimationWnd.cs
/// Cretaed by fengsc 2018/01/31
/// 创建公会成功动画界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class CreateGangAnimationWnd : WindowBase<CreateGangAnimationWnd>
{
    #region 成员变量

    // 公会名称
    public UILabel mGangName;

    // 会长名称
    public UILabel mLeaderName;

    // 公会旗帜
    public FlagItemWnd mFlagItemWnd;

    public UILabel mTips;

    public GameObject mCloseBtn;

    public GameObject mFlagMask;

    public TweenScale mFlagMaskScale;

    public GameObject mWhiteSplash;

    public TweenScale mTweenScale;

    // 公会数据
    LPCMapping mGangData = LPCMapping.Empty;

    #endregion

    // Use this for initialization
    void Start ()
    {
        EventDelegate.Add(mTweenScale.onFinished, OnTweenScaleFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);

        EventMgr.RegisterEvent("CreateGangAnimationWnd", EventMgrEventType.EVENT_NOTIFY_GANG_INFO, OnNotifyGangInfo);

        // 获取公会信息
        GangMgr.GetGangInfo();
    }

    /// <summary>
    /// 获取公会信息事件回调
    /// </summary>
    void OnNotifyGangInfo(int eventId, MixedValue para)
    {
        // 重绘界面
        Redraw();
    }

    void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        // 解注册事件
        EventMgr.UnregisterEvent("CreateGangAnimationWnd");
    }

    void OnTweenScaleFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 窗口关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mGangData = GangMgr.GangDetail;
        if (mGangData == null || mGangData.Count == 0)
            return;

        // 公会名称
        mGangName.text = mGangData.GetValue<string>("gang_name");

        // 会长名称
        mLeaderName.text = string.Format(LocalizationMgr.Get("GangWnd_70"), mGangData.GetValue<string>("leader_name"));

        // 绑定旗帜数据
        mFlagItemWnd.Bind(mGangData.GetValue<LPCArray>("flag"));

        mTips.text = LocalizationMgr.Get("GangWnd_69");

        EventDelegate.Add(mFlagMaskScale.onFinished, OnScaleFinish);

        mFlagMask.SetActive(true);

        mGangName.gameObject.SetActive(true);

        mLeaderName.gameObject.SetActive(true);

        mTips.gameObject.SetActive(true);

        mWhiteSplash.SetActive(true);
    }

    void OnScaleFinish()
    {
        mFlagItemWnd.gameObject.SetActive(true);

        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
    }
}
