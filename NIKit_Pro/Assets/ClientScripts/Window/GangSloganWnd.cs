/// <summary>
/// GangSloganWnd.cs
/// Created by fengsc 2018/02/06
/// 公会宣传界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GangSloganWnd : WindowBase<GangSloganWnd>
{
    #region 成员变量

    // 标题
    public UILabel mTitle;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    public UILabel mTips;

    public UIInput mInput;

    public UILabel mAmountTips;

    // 发送按钮
    public GameObject mSendBtn;
    public UILabel mSendBtnLb;

    // 消耗图标
    public UITexture mCostIcon;

    public TweenScale mTweenScale;

    // 消耗数量
    public UILabel mCost;

    LPCMapping mCostData = LPCMapping.Empty;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 初始化本地化文本
        InitText();

        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("GangSloganWnd");

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mTitle.text = LocalizationMgr.Get("GangWnd_131");

        mTips.text = LocalizationMgr.Get("GangWnd_132");

        mSendBtnLb.text = LocalizationMgr.Get("GangWnd_133");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mSendBtn).onClick = OnClickSendBtn;

        // 注册EVENT_SEND_GANG_SLOGAN事件
        EventMgr.RegisterEvent("GangSloganWnd", EventMgrEventType.EVENT_SEND_GANG_SLOGAN, OnEventSendGangSlogan);

        EventDelegate.Add(mInput.onChange, OnInputChange);

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);
    }

    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void OnInputChange()
    {
        mAmountTips.text = string.Format("{0}/{1}", mInput.value.Trim().Length, GameSettingMgr.GetSettingInt("max_gang_slogan"));
    }

    /// <summary>
    /// 发送宣传成功事件回调
    /// </summary>
    void OnEventSendGangSlogan(int eventId, MixedValue para)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 关闭按钮点击事件回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 发送按钮
    /// </summary>
    void OnClickSendBtn(GameObject go)
    {
        // 消耗不足
        if (!ME.user.CanCostAttrib(mCostData))
        {
            string field = FieldsMgr.GetFieldInMapping(mCostData);

            DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("GangWnd_136"), FieldsMgr.GetFieldName(field)));

            return;
        }

        // 宣传内容不能为空
        string str = mInput.value.Trim();
        if (string.IsNullOrEmpty(str))
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_137"));

            return;
        }

        // 字数过长
        if (str.Length > GameSettingMgr.GetSettingInt("max_gang_slogan"))
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_140"));

            return;
        }

        LPCArray message = LPCArray.Empty;
        message.Add(str);

        // 发送宣传标语
        GangMgr.SendGangSlogan(message);

        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        /// 发送公会宣传消耗
        mCostData = GameSettingMgr.GetSetting<LPCMapping>("send_gang_slogan_cost");

        string field = FieldsMgr.GetFieldInMapping(mCostData);

        // 消耗图标
        mCostIcon.mainTexture = ItemMgr.GetTexture(FieldsMgr.GetClassIdByAttrib(field), true);

        // 消耗数量
        mCost.text = string.Format("×{0}", mCostData.GetValue<int>(field));

        // 限制字符数量
        mInput.characterLimit = GameSettingMgr.GetSettingInt("max_gang_slogan");

        mAmountTips.text = string.Format("{0}/{1}", mInput.value.Trim().Length, GameSettingMgr.GetSettingInt("max_gang_slogan"));
    }
}
