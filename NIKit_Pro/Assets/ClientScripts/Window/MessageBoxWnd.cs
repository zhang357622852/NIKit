﻿/// <summary>
/// MessageBoxWnd.cs
/// Created by Who 2014-
/// Note
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public partial class MessageBoxWnd : WindowBase<MessageBoxWnd>
{
    #region 成员变量
    public GameObject ok_btn;
    public GameObject cancel_btn;
    public UILabel ok_lable;
    public UILabel cancel_lable;
    public UILabel title_lable;
    public GameObject single_btn;
    public UILabel single_btn_lable;
    public GameObject mRichText;
    public GameObject mMask;

    bool isRespondMask = true;

    CallBack task;
    #endregion

    #region 公共函数

    /// <summary>
    /// 窗口初始化
    /// </summary>
    public override void _InitWnd()
    {
        // 标记为
        IsSubWnd = false;
    }

    /// <summary>
    /// 手机按下返回键
    /// </summary>
    public override bool OnBack()
    {
        if (this != null)
            WindowMgr.DestroyWindow(gameObject.name);

        return true;
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
    public void ShowDialog(CallBack _task, string content, string title, string ok_text, string cancel_text, bool is_respondMask)
    {
        single_btn.SetActive(false);
        ok_btn.SetActive(true);
        cancel_btn.SetActive(true);

        isRespondMask = is_respondMask;

        task = _task;

        // 标题
        if (! string.IsNullOrEmpty(title))
            title_lable.text = title;
        else
            title_lable.text = LocalizationMgr.Get("MessageBoxWnd_1");

        // 确定按钮名字
        if (! string.IsNullOrEmpty(ok_text))
            ok_lable.text = ok_text;
        else
            ok_lable.text = LocalizationMgr.Get("MessageBoxWnd_2");

        // 取消按钮名字
        if (! string.IsNullOrEmpty(cancel_text))
            cancel_lable.text = cancel_text;
        else
            cancel_lable.text = LocalizationMgr.Get("MessageBoxWnd_3");

        if(string.IsNullOrEmpty(content))
        {
            mRichText.SetActive(false);
            return;
        }

        mRichText.SetActive(true);

        RichTextContent richText = mRichText.GetComponent<RichTextContent>();

        richText.clearContent();

        richText.ParseValue(content);

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
    public void ShowSingleBtnDialog(CallBack _task, string content, string title, string btn_text, bool is_respondMask)
    {
        single_btn.SetActive(true);
        ok_btn.SetActive(false);
        cancel_btn.SetActive(false);

        isRespondMask = is_respondMask;

        task = _task;

        // 标题
        if (! string.IsNullOrEmpty(title))
            title_lable.text = title;
        else
            title_lable.text = LocalizationMgr.Get("MessageBoxWnd_1");

        // 按钮名字
        if (! string.IsNullOrEmpty(btn_text))
            single_btn_lable.text = btn_text;
        else
            single_btn_lable.text = LocalizationMgr.Get("MessageBoxWnd_2");

        if(string.IsNullOrEmpty(content))
        {
            mRichText.SetActive(false);
            return;
        }

        mRichText.SetActive(true);

        RichTextContent richText = mRichText.GetComponent<RichTextContent>();

        richText.clearContent();

        richText.ParseValue(content);
    }

    /// <summary>
    /// 点击确认
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnOKClicked(GameObject ob)
    {
        if (task != null)
            task.Go(true);

        if (this == null)
            return;

        // 直接销毁窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 点击取消
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCancelClicked(GameObject ob)
    {
        if (task != null)
            task.Go(false);

        if (this == null)
            return;

        // 直接销毁窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 点击取消
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnMaskClicked(GameObject ob)
    {
        // 不响应遮罩点击
        if (!isRespondMask)
            return;

        if (task != null)
            task.Go(false, MesssgeBoxConst.CLOSE);

        if (this == null)
            return;

        // 直接销毁窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }


    /// <summary>
    /// 单独的按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnSingleClicked(GameObject ob)
    {
        if (task != null)
            task.Go(false);

        if (this == null)
            return;

        // 直接销毁窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        // 初始化窗口
        InitWnd();
    }

    // 由于确认框不是通关WindowMgr Show出来的
    // 所以在窗口显示时，自己将其加入WindowList
    void OnEnable()
    {
        _InitWnd();
    }

    // 由于确认窗口关闭时，不是通过WindowMgr关闭的
    // 所以在窗口关闭时，需要自己将其从WindowList移除
    void OnDisable()
    {
        _DetachWnd();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(ok_btn).onClick = OnOKClicked;
        UIEventListener.Get(cancel_btn).onClick = OnCancelClicked;
        UIEventListener.Get(mMask).onClick = OnMaskClicked;
        UIEventListener.Get(single_btn).onClick = OnSingleClicked;
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
    }
    #endregion
}
