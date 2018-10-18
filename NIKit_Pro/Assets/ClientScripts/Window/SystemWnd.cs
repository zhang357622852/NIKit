/// <summary>
/// SystemWnd.cs
/// Created by fengsc 2016/07/05
/// 系统相关设置窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

enum PAGE
{
    ACCOUNT_PAGE = 0,
    SYSTEM_PAGE  = 1,
    HELP_PAGE    = 2,
}

public class SystemWnd : WindowBase<SystemWnd> 
{

    #region 成员变量

    /// <summary>
    ///账号设置选项
    /// </summary>
    public UIToggle mAccountOptions;

    public UILabel mAccountOptionsLabel;

    /// <summary>
    ///账号设置界面
    /// </summary>
    public GameObject mAccountSetWnd;

    /// <summary>
    ///系统设置选项
    /// </summary>
    public UIToggle mSystemOptions;

    public UILabel mSystemOptionsLabel;

    /// <summary>
    ///系统设置界面
    /// </summary>
    public GameObject mSystemSetWnd;

    /// <summary>
    ///帮助信息选项
    /// </summary>
    public UIToggle mHelpOptions;

    public UILabel mHelpOptionsLabel;

    /// <summary>
    ///帮助信息界面
    /// </summary>
    public GameObject mHelpWnd;

    /// <summary>
    ///关闭界面按钮
    /// </summary>
    public GameObject mCloseBtn;

    /// <summary>
    ///界面名称
    /// </summary>
    public UILabel mTitle;

    public TweenAlpha mTweenAlpha;

    public TweenScale mTweenScale;

    private PAGE mPage = PAGE.ACCOUNT_PAGE;

    #endregion

    void Start () 
    {
        //初始化UI
        ShowUI();

        //注册事件
        RegisterEvent();

        //初始化label;
        InitLabel();

        if (mTweenScale == null || mTweenAlpha == null)
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
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mAccountOptions.gameObject).onClick = OnClickAccountSet;
        UIEventListener.Get(mSystemOptions.gameObject).onClick = OnClickSystemSet;
        UIEventListener.Get(mHelpOptions.gameObject).onClick = OnClickHelpInfo;
        UIEventListener.Get(mCloseBtn).onClick = OnClickClose;

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
    ///初始化文本
    /// </summary>
    void InitLabel()
    {
        //账号设置界面文本初始化;
        mAccountOptionsLabel.text = LocalizationMgr.Get("SystemWnd_1");
        mSystemOptionsLabel.text = LocalizationMgr.Get("SystemWnd_2");
        mHelpOptionsLabel.text = LocalizationMgr.Get("SystemWnd_3");
    }

    /// <summary>
    ///选择选项显示相应的UI
    /// </summary>
    void ShowUI()
    {
        //选择账号设置选项
        if(mAccountOptions.value)
        {
            mTitle.text = LocalizationMgr.Get("SystemWnd_4");

            mAccountSetWnd.SetActive(true);
        }
        else
            mAccountSetWnd.SetActive(false);

        if(mSystemOptions.value)
        {
            mTitle.text = LocalizationMgr.Get("SystemWnd_5");

            mSystemSetWnd.SetActive(true);
        }
        else
            mSystemSetWnd.SetActive(false);

        if(mHelpOptions.value)
        {
            mTitle.text = LocalizationMgr.Get("SystemWnd_6");

            mHelpWnd.SetActive(true);

            // 绑定数据
            mHelpWnd.GetComponent<HelpWnd>().Bind(-1);
        }
        else
            mHelpWnd.SetActive(false);
    }

    /// <summary>
    ///账号设置点击事件
    /// </summary>
    void OnClickAccountSet(GameObject go)
    {
        if (mPage == PAGE.ACCOUNT_PAGE)
            return;

        mPage = PAGE.ACCOUNT_PAGE;

        ShowUI();
    }

    /// <summary>
    ///系统设置点击事件
    /// </summary>
    void OnClickSystemSet(GameObject go)
    {
        if (mPage == PAGE.SYSTEM_PAGE)
            return;

        mPage = PAGE.SYSTEM_PAGE;

        ShowUI();
    }

    /// <summary>
    //帮助信息点击事件
    /// </summary>
    void OnClickHelpInfo(GameObject go)
    {
        if (mPage == PAGE.HELP_PAGE)
            return;

        mPage = PAGE.HELP_PAGE;

        ShowUI();
    }

    /// <summary>
    ///关闭按钮点击事件
    /// </summary>
    void OnClickClose(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }
}
