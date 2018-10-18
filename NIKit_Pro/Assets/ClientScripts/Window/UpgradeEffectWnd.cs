/// <summary>
/// UpgradeEffectWnd.cs
/// Created by fengsc 2017/06/05
/// 升级效果窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class UpgradeEffectWnd : WindowBase<UpgradeEffectWnd>
{
    #region 成员变量

    public UITexture mIcon;

    public UILabel mLevelDesc;

    public TweenAlpha mIconAlpha;

    public TweenAlpha mDescBgAlpha;

    public TweenAlpha mSmallBgAlpha;

    public TweenScale mSmallbgScale;

    public TweenAlpha mBigAlpha;

    public TweenScale mBigScale;

    public TweenAlpha mWhiteLightAlpha;

    public TweenScale mWhiteLightScale;

    public UILabel mAddAmount;

    public GameObject mBg;

    public TweenAlpha mPanelAlha;

    public UILabel mLevelLb;

    public UILabel mTitlelight;
    public UILabel mTitle;
    public UILabel mTitleShaow;

    public UILabel mDescLb;

    int mPreLevel = 0;

    bool mIsClickBg = false;

    #endregion

    void Start()
    {
        RegisterEvent();

        mTitle.text = LocalizationMgr.Get("UpgradeEffectWnd_3");
        mTitlelight.text = LocalizationMgr.Get("UpgradeEffectWnd_3");
        mTitleShaow.text = LocalizationMgr.Get("UpgradeEffectWnd_3");
        mLevelLb.text = LocalizationMgr.Get("UpgradeEffectWnd_4");

        float scale = Game.CalcWndScale();
        transform.localScale = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mBg).onClick = OnClickBg;
    }

    void Redraw()
    {
        mLevelDesc.text = ME.user.Query<int>("level").ToString();

        int curLevelLife = CALC_MAX_LIFE.Call(ME.user.Query<int>("level"));

        int addLife = curLevelLife - CALC_MAX_LIFE.Call(mPreLevel);

        mDescLb.text = string.Format("+ {0}", curLevelLife);

        if (addLife < 0)
        {
            WindowMgr.HideWindow(gameObject);
            return;
        }

        mAddAmount.text = string.Format(LocalizationMgr.Get("UpgradeEffectWnd_1"), addLife);

        mIconAlpha.PlayForward();
        mIconAlpha.ResetToBeginning();

        mDescBgAlpha.PlayForward();
        mDescBgAlpha.ResetToBeginning();

        mSmallBgAlpha.PlayForward();
        mSmallBgAlpha.ResetToBeginning();

        mSmallbgScale.PlayForward();
        mSmallbgScale.ResetToBeginning();

        mBigAlpha.PlayForward();
        mBigAlpha.ResetToBeginning();

        mBigScale.PlayForward();
        mBigScale.ResetToBeginning();

        mWhiteLightAlpha.PlayForward();
        mWhiteLightAlpha.ResetToBeginning();

        mWhiteLightAlpha.PlayForward();
        mWhiteLightScale.ResetToBeginning();

        Invoke("PlayPanelTweenAlpha", 6.0f);
    }

    /// <summary>
    /// 背景点击回调
    /// </summary>
    void OnClickBg(GameObject go)
    {
        if (mIsClickBg)
            return;

        PlayPanelTweenAlpha();

        mIsClickBg = true;
    }

    void PlayPanelTweenAlpha()
    {
        EventDelegate.Add(mPanelAlha.onFinished, OnTweenAlphaFinshed);
        mPanelAlha.PlayForward();
        mPanelAlha.ResetToBeginning();
    }

    void OnTweenAlphaFinshed()
    {
        mIsClickBg = false;

        WindowMgr.DestroyWindow(UpgradeEffectWnd.WndType);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int preLevel)
    {
        mPreLevel = preLevel;

        // 绘制窗口
        Redraw();

        // 抛出玩家升级事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_USER_LEVEL_UP, null);
    }
}
