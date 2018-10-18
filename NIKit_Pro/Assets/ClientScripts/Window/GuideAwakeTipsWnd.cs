/// <summary>
/// GuideAwakeTipsWnd.cs
/// Created by fengsc 2018/04/10
/// 指引觉醒提示窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GuideAwakeTipsWnd : WindowBase<GuideAwakeTipsWnd>
{
    // 窗口标题
    public UILabel mTitle;

    // 关闭按钮
    public GameObject mCloseBtn;

    public UILabel mTips1;

    // 生命提示
    public UILabel mLifeTips;

    // 攻击提示
    public UILabel mAtkTips;

    // 防御提示
    public UILabel mDefenceTips;

    // 敏捷提示
    public UILabel mAgilityTips;

    public UILabel mTips2;

    public UILabel mTips3;

    // 技能名称
    public UILabel mSkillName;

    // 技能描述
    public UILabel mSkillDesc;

    // 攻击强化buff名称
    public UILabel mAtkBuffName;

    // buff描述
    public UILabel mAtkBuffDesc;

    // 暴击强化buff名称
    public UILabel mCrtBuffName;

    // buff描述
    public UILabel mCrtBuffDesc;

    // 帮助提示信息
    public UILabel mHelpTips;

    void Start()
    {
        // 初始化窗口
        InitWnd();

        // 注册事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mHelpTips.gameObject).onClick = OnClickHelpTipsBtn;

        TweenScale mTweenScale = GetComponent<TweenScale>();

        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 帮助提示点击回调
    /// </summary>
    void OnClickHelpTipsBtn(GameObject go)
    {
        // 打开帮助窗口
        GameObject mHelpWnd = WindowMgr.OpenWnd("HelpWnd");

        mHelpWnd.GetComponent<HelpWnd>().Bind(HelpConst.STATS_ID);
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        // 初始化本地化文本
        mTitle.text = LocalizationMgr.Get("GuideAwakeTipsWnd_1");
        mTips1.text = LocalizationMgr.Get("GuideAwakeTipsWnd_2");
        mLifeTips.text = LocalizationMgr.Get("GuideAwakeTipsWnd_3");
        mAtkTips.text = LocalizationMgr.Get("GuideAwakeTipsWnd_4");
        mDefenceTips.text = LocalizationMgr.Get("GuideAwakeTipsWnd_5");
        mAgilityTips.text = LocalizationMgr.Get("GuideAwakeTipsWnd_6");
        mTips2.text = LocalizationMgr.Get("GuideAwakeTipsWnd_7");
        mTips3.text = LocalizationMgr.Get("GuideAwakeTipsWnd_8");
        mSkillName.text = LocalizationMgr.Get("GuideAwakeTipsWnd_9");
        mSkillDesc.text = LocalizationMgr.Get("GuideAwakeTipsWnd_10");
        mAtkBuffName.text = LocalizationMgr.Get("GuideAwakeTipsWnd_11");
        mAtkBuffDesc.text = LocalizationMgr.Get("GuideAwakeTipsWnd_12");
        mCrtBuffName.text = LocalizationMgr.Get("GuideAwakeTipsWnd_13");
        mCrtBuffDesc.text = LocalizationMgr.Get("GuideAwakeTipsWnd_14");
        mHelpTips.text = LocalizationMgr.Get("GuideAwakeTipsWnd_15");
    }
}
