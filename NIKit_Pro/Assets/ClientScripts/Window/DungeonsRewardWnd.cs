/// <summary>
/// DungeonsRewardWnd.cs
/// Created by fengsc 2017/01/03
/// 地下城奖励界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DungeonsRewardWnd : WindowBase<DungeonsRewardWnd>
{
    // 窗口标题
    public UILabel mTitle;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    // 奖励描述
    public UILabel mDesc;

    // 完成奖励
    public UILabel mFinishRewardTips;

    // 排序控件
    public UIGrid mGrid;

    public GameObject mItem;

    // 确认按钮
    public GameObject mConfirmBtn;
    public UILabel mConfirmBtnLb;

    public TweenAlpha mTweenAlpha;

    public TweenScale mTweenScale;

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        if (mTweenAlpha == null || mTweenScale == null)
            return;

        // 播放动画
        mTweenAlpha.PlayForward();

        mTweenScale.PlayForward();

        // 重置动画
        mTweenAlpha.ResetToBeginning();

        mTweenScale.ResetToBeginning();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mConfirmBtn).onClick = OnClickCloseBtn;

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);
    }

    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 销毁当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 确认按钮点击事件
    /// </summary>
    void OnClickConfirmBtn(GameObject go)
    {
        
    }
}
