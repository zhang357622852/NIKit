/// <summary>
/// InstanceChallengeTaskWnd.cs
/// Created by fengsc 2017/01/05
/// 副本挑战任务界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class InstanceChallengeTaskWnd : WindowBase<InstanceChallengeTaskWnd>
{
    #region 成员变量

    // 标题
    public UILabel mTitle;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    // 任务列表基础格子
    public GameObject mTaskItem;

    // 任务列表排序组件
    public UIWrapContent mTaskListGrid;

    // 任务标题
    public UILabel mTaskTitle;

    // 任务内容
    public UILabel mTaskContent;

    // 奖励列表基础格子
    public GameObject mRewardItem;

    // 奖励列表排序组件
    public UIGrid mRewardListGrid;

    // 奖励领取按钮
    public GameObject mReceiveBtn;
    public UILabel mReceiveBtnLb;

    public TweenAlpha mTweenAlpha;

    public TweenScale mTweenScale;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        // 初始化本地化文本s
        InitLocaText();

        if (mTweenAlpha == null || mTweenScale == null)
            return;

        // 播放动画
        mTweenAlpha.PlayForward();

        mTweenScale.PlayForward();

        // 重置动画组件
        mTweenAlpha.ResetToBeginning();

        mTweenScale.ResetToBeginning();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocaText()
    {
        mReceiveBtnLb.text = LocalizationMgr.Get("InstanceChallengeTaskWnd_2");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mReceiveBtn).onClick = OnClickReceiveBtn;

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);
    }

    /// <summary>
    /// tween动画播放完后回调
    /// </summary>
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
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 奖励领取按钮点击事件
    /// </summary>
    void OnClickReceiveBtn(GameObject go)
    {
        
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(string instanceId)
    {
        
    }
}
