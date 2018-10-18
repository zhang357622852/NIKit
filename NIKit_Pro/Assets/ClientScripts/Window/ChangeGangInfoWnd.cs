/// <summary>
/// ChangeGangInfoWnd.cs
/// Created by fengsc 2016/01/26
/// 修改公会信息
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ChangeGangInfoWnd : WindowBase<ChangeGangInfoWnd>
{
    #region 成员变量

    // 窗口标题
    public UILabel mTitle;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 公会旗帜提示
    public UILabel mFlagsTips;

    // 公会旗帜基础格子
    public FlagItemWnd mFlagItemWnd;

    // 重新制旗
    public GameObject mMakeFlagBtn;
    public UILabel mMakeFlagBtnLb;

    // 确认修改
    public GameObject mConfirmBtn;
    public UILabel mConfirmBtnLb;

    // 创建公会消耗
    public UILabel mCost;

    // 消耗品图标
    public UITexture mCostIcon;

    // 介绍提示
    public UILabel mIntroductionTips;

    // 介绍输入框
    public UIInput mIntroductionInput;

    public UILabel mIntroductionAmountTips;

    // 入会条件提示
    public UILabel mConditionTips;

    public UILabel mTips1;
    public UILabel mTips2;

    // 段位要求
    public GameObject mArenaRankBtn;
    public UILabel mArenaRankBtnLb;

    public GameObject mStars;

    public UISprite[] mArenaRankStars;

    // 是否需要申请条件
    public GameObject mApplicationConditionBtn;
    public UILabel mApplicationConditionBtnLb;

    public GameObject mSelectRankWnd;

    public GameObject mCloseSelectRankWnd;

    public GameObject mApplicationConditionWnd;

    public GameObject mCloseApplicationConditionWnd;

    public TweenScale mTweenScale;

    public UIToggle[] mRankConditionItem;

    // 不需要审核
    public UIToggle mNoCheckBtn;
    public UILabel mNoCheckLb;

    // 需要审核
    public UIToggle mNeedCheckBtn;
    public UILabel mNeedCheckLb;

    public UIToggle mRefuseJionBtn;
    public UILabel mRefuseJionLb;

    // 公会简介内容长度限制
    int mIntroductionLengthLimit = 0;

    int mCheckCondition = -1;

    int mStep = -2;

    LPCMapping mCreateCost = LPCMapping.Empty;

    // 旗帜数据
    LPCArray mFlagData = LPCArray.Empty;

    LPCMapping mGangInfo = LPCMapping.Empty;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 初始化文本
        InitText();

        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();
    }

    void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mTitle.text = LocalizationMgr.Get("CreateGuildWnd_31");
        mFlagsTips.text = LocalizationMgr.Get("CreateGuildWnd_2");
        mMakeFlagBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_29");
        mConfirmBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_30");
        mIntroductionTips.text = LocalizationMgr.Get("CreateGuildWnd_8");
        mIntroductionInput.defaultText = LocalizationMgr.Get("CreateGuildWnd_10");
        mConditionTips.text = LocalizationMgr.Get("CreateGuildWnd_11");
        mTips1.text = LocalizationMgr.Get("CreateGuildWnd_12");
        mTips2.text = LocalizationMgr.Get("CreateGuildWnd_13");
        mArenaRankBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_14");
        mApplicationConditionBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_14");
        mNoCheckLb.text = LocalizationMgr.Get("CreateGuildWnd_17");
        mNeedCheckLb.text = LocalizationMgr.Get("CreateGuildWnd_18");
        mRefuseJionLb.text = LocalizationMgr.Get("CreateGuildWnd_19");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mMakeFlagBtn).onClick = OnClickMakeFlagBtn;
        UIEventListener.Get(mConfirmBtn).onClick = OnClickConfirmBtn;
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mNoCheckBtn.gameObject).onClick = OnClickNoCheckBtn;
        UIEventListener.Get(mNeedCheckBtn.gameObject).onClick = OnClickNeedCheckBtn;
        UIEventListener.Get(mRefuseJionBtn.gameObject).onClick = OnClickRefuseJionBtn;
        UIEventListener.Get(mCloseSelectRankWnd).onClick = OnClickCloseSelectRankWnd;
        UIEventListener.Get(mCloseApplicationConditionWnd).onClick = OnClickCloseApplicationCondition;
        UIEventListener.Get(mArenaRankBtn).onClick = OnClickSelectRankConditinBtn;
        UIEventListener.Get(mApplicationConditionBtn).onClick = OnClickSelectCheckCondition;

        for (int i = 0; i < mRankConditionItem.Length; i++)
            UIEventListener.Get(mRankConditionItem[i].gameObject).onClick = OnClickRankConditionItem;

        EventDelegate.Add(mIntroductionInput.onChange, OnIntroductionChange);

        if (mTweenScale != null)
            EventDelegate.Add(mTweenScale.onFinished, OnFinish);
    }

    void OnFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 公会简介输入框变化事件回调
    /// </summary>
    void OnIntroductionChange()
    {
        mIntroductionAmountTips.text = string.Format("{0}/{1}", mIntroductionInput.value.Length, mIntroductionLengthLimit);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 介绍内容长度限制
        mIntroductionLengthLimit = GameSettingMgr.GetSettingInt("max_introduce_len");

        mIntroductionInput.characterLimit = mIntroductionLengthLimit;

        mIntroductionAmountTips.text = string.Format("{0}/{1}", 0, mIntroductionLengthLimit);

        // 重置旗帜消耗
        mCreateCost = GameSettingMgr.GetSetting<LPCMapping>("set_gang_flag_cost");

        string fields = FieldsMgr.GetFieldInMapping(mCreateCost);

        // 显示创建公会消耗
        mCost.text = mCreateCost.GetValue<int>(fields).ToString();

        // 显示消耗图标
        mCostIcon.mainTexture = ItemMgr.GetTexture(FieldsMgr.GetFieldTexture(fields));

        List<int> rankCondition = CALC_CREATE_GUILD_RANK_CONDITION.Call();

        for (int i = 0; i < mRankConditionItem.Length; i++)
        {
            ConditionItemWnd script = mRankConditionItem[i].GetComponent<ConditionItemWnd>();
            if (script == null)
                continue;

            script.Bind(rankCondition[i]);
        }

        mNoCheckBtn.GetComponent<UIEventListener>().parameter = (int) CHECK_CONDITION.NO_CHECK;
        mNeedCheckBtn.GetComponent<UIEventListener>().parameter = (int) CHECK_CONDITION.NEED_CHECK;
        mRefuseJionBtn.GetComponent<UIEventListener>().parameter = (int) CHECK_CONDITION.REFUSE_JION;

        mFlagItemWnd.Bind(mFlagData);

        string introduce = mGangInfo.GetValue<string>("introduce");

        // 初始化公会简介
        mIntroductionInput.Set(introduce);

        mIntroductionAmountTips.text = string.Format("{0}/{1}", introduce.Length, mIntroductionLengthLimit);

        LPCMapping condition = mGangInfo.GetValue<LPCMapping>("join_condition");

        mStep = condition.GetValue<int>("step");

        mCheckCondition = condition.GetValue<int>("check");

        // 刷新审核条件
        RefreshCheckCondition();

        //刷新段位条件
        RefreshRankCondition();
    }

    // 刷新单选框状态
    void RefreshToggle()
    {
        for (int i = 0; i < mRankConditionItem.Length; i++)
        {
            ConditionItemWnd script = mRankConditionItem[i].GetComponent<ConditionItemWnd>();
            if (script == null)
                continue;

            if (script.mStep != mStep)
                continue;

            mRankConditionItem[i].Set(true);
        }

        mNoCheckBtn.Set(false);
        mNeedCheckBtn.Set(false);
        mRefuseJionBtn.Set(false);

        if (mCheckCondition == (int)mNoCheckBtn.GetComponent<UIEventListener>().parameter)
        {
            mNoCheckBtn.Set(true);
        }
        else if (mCheckCondition == (int)mNeedCheckBtn.GetComponent<UIEventListener>().parameter)
        {
            mNeedCheckBtn.Set(true);
        }
        else if (mCheckCondition == (int)mRefuseJionBtn.GetComponent<UIEventListener>().parameter)
        {
            mRefuseJionBtn.Set(true);
        }
    }

    void OnClickSelectRankConditinBtn(GameObject go)
    {
        mSelectRankWnd.SetActive(true);
        mApplicationConditionWnd.SetActive(false);

        // 默认选项
        if (mStep < -1)
            mStep = -1;

        // 刷新单选框状态
        RefreshToggle();
    }

    void OnClickSelectCheckCondition(GameObject go)
    {
        mSelectRankWnd.SetActive(false);
        mApplicationConditionWnd.SetActive(true);

        // 默认选项
        if (mCheckCondition < 0)
            mCheckCondition = (int) CHECK_CONDITION.NO_CHECK;

        // 刷新单选框状态
        RefreshToggle();
    }

    /// <summary>
    /// 自主制旗点击事件
    /// </summary>
    void OnClickMakeFlagBtn(GameObject go)
    {
        LPCValue v = ME.user.Query<LPCValue>("my_gang_info");
        if (v != null && v.IsMapping && v.AsMapping.Count != 0)
        {
            // 只有会长可以修改旗帜
            if (v.AsMapping.GetValue<string>("station") != "gang_leader")
            {
                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_49"));
                return;
            }
        }

        // 打开旗帜制作窗口
        GameObject wnd = WindowMgr.OpenWnd(GangFlagMakeWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<GangFlagMakeWnd>().Bind(mFlagData, new CallBack(OnCallBack));
    }

    void OnCallBack(object para, params object[] param)
    {
        // 刷旗帜新数据
        mFlagData = param[0] as LPCArray;

        // 绑定数据
        mFlagItemWnd.GetComponent<FlagItemWnd>().Bind(mFlagData);

        // 修改旗帜
        GangMgr.SetGangFlag(mFlagData);
    }

    /// <summary>
    /// 确认修改按钮点击事件回调
    /// </summary>
    void OnClickConfirmBtn(GameObject go)
    {
        // 入会条件
        LPCMapping condition = LPCMapping.Empty;
        if (mStep >= -1 && mCheckCondition >= 0)
        {
            condition.Add("step" , mStep);
            condition.Add("check" , mCheckCondition);
        }

        if (condition.Count == 0)
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("CreateGuildWnd_26"));
            return;
        }

        // 修改简介
        LPCMapping data = LPCMapping.Empty;
        data.Add("introduce", mIntroductionInput.value.Trim());
        data.Add("join_condition", condition);

        // 修改公会信息
        GangMgr.SetGangInformation(data);

        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前按钮
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 段位条件基础格子点击事件
    /// </summary>
    void OnClickRankConditionItem(GameObject go)
    {
        ConditionItemWnd script = go.GetComponent<ConditionItemWnd>();
        if (script == null)
            return;

        mStep = script.mStep;

        mSelectRankWnd.SetActive(false);

        // 刷新段位条件显示
        RefreshRankCondition();
    }

    /// <summary>
    /// 刷新段位条件显示
    /// </summary>
    void RefreshRankCondition()
    {
        mArenaRankBtnLb.gameObject.SetActive(false);

        mStars.SetActive(false);

        if (mStep == -1)
        {
            mArenaRankBtnLb.gameObject.SetActive(true);
            mArenaRankBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_20");
            return;
        }

        mStars.SetActive(true);

        CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(mStep);
        if (row == null)
            return;

        for (int i = 0; i < mArenaRankStars.Length; i++)
            mArenaRankStars[i].spriteName = "arena_star_bg";

        for (int i = 0; i < row.Query<int>("star"); i++)
            mArenaRankStars[i].spriteName = row.Query<string>("star_name");
    }

    /// <summary>
    /// 直接加入按钮点击回调
    /// </summary>
    void OnClickNoCheckBtn(GameObject go)
    {
        mCheckCondition = (int) CHECK_CONDITION.NO_CHECK;

        mApplicationConditionWnd.SetActive(false);

        // 刷新审核条件显示
        RefreshCheckCondition();
    }

    /// <summary>
    /// 需要审核按钮点击回调
    /// </summary>
    void OnClickNeedCheckBtn(GameObject go)
    {
        mCheckCondition = (int) CHECK_CONDITION.NEED_CHECK;

        mApplicationConditionWnd.SetActive(false);

        // 刷新审核条件显示
        RefreshCheckCondition();
    }

    /// <summary>
    /// 拒绝加入按钮点击回调
    /// </summary>
    void OnClickRefuseJionBtn(GameObject go)
    {
        mCheckCondition = (int) CHECK_CONDITION.REFUSE_JION;

        mApplicationConditionWnd.SetActive(false);

        // 刷新审核条件显示
        RefreshCheckCondition();
    }

    /// <summary>
    /// 刷新审核条件显示
    /// </summary>
    void RefreshCheckCondition()
    {
        switch (mCheckCondition)
        {
            case (int) CHECK_CONDITION.NO_CHECK:
                mApplicationConditionBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_17");
                break;

            case (int) CHECK_CONDITION.NEED_CHECK:
                mApplicationConditionBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_18");
                break;

            case (int) CHECK_CONDITION.REFUSE_JION:
                mApplicationConditionBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_19");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 段位选择窗口
    /// </summary>
    void OnClickCloseSelectRankWnd(GameObject go)
    {
        mSelectRankWnd.SetActive(false);

        RefreshRankCondition();
    }

    void OnClickCloseApplicationCondition(GameObject go)
    {
        mApplicationConditionWnd.SetActive(false);

        RefreshCheckCondition();
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        mFlagData = data.GetValue<LPCArray>("flag");

        mGangInfo = data;
    }
}
