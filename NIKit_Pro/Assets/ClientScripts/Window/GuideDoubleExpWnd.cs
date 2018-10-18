/// <summary>
/// GuideDoubleExpWnd.cs
/// Created by fengsc 2018/04/03
/// 指引双倍经验窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GuideDoubleExpWnd : WindowBase<GuideDoubleExpWnd>
{
    // 窗口标题
    public UILabel mTitle;

    // 关闭窗口按钮
    public GameObject mCloseBtn;

    // 图文混排控件
    public RichTextContent mRtc;

    public UILabel mTips1;
    public UILabel mTips2;
    public UILabel mTips3;
    public UILabel mTips4;
    public UILabel mTips5;

    void Start()
    {
        // 初始化文本
        InitText();

        // 注册点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        TweenScale mTweenScale = GetComponent<TweenScale>();

        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// 初始化文本
    /// </summary>
    void InitText()
    {
        mTitle.text = LocalizationMgr.Get("GuideDoubleExpWnd_1");

        mRtc.ParseValue(LocalizationMgr.Get("GuideDoubleExpWnd_2"));

        mTips1.text = LocalizationMgr.Get("GuideDoubleExpWnd_3");
        mTips2.text = LocalizationMgr.Get("GuideDoubleExpWnd_4");
        mTips3.text = LocalizationMgr.Get("GuideDoubleExpWnd_5");
        mTips4.text = LocalizationMgr.Get("GuideDoubleExpWnd_6");
        mTips5.text = LocalizationMgr.Get("GuideDoubleExpWnd_7");
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }
}
