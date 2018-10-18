/// <summary>
/// CreateGangWnd.cs
/// Created by fengsc 2018/01/24
/// 创建公会界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public enum CHECK_CONDITION
{
    NO_CHECK = 0,
    NEED_CHECK = 1,
    REFUSE_JION = 2,
}

public class CreateGangWnd : WindowBase<CreateGangWnd>
{
    #region 成员变量

    // 窗口标题
    public UILabel mTitle;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 公会旗帜提示
    public UILabel mFlagsTips;

    // 公会旗帜基础格子
    public GameObject mFlagItemWnd;

    // 随机制旗
    public GameObject mRandomBtn;
    public UILabel mRandomBtnLb;

    // 自主制旗
    public GameObject mCustomBtn;
    public UILabel mCustomBtnLb;

    // 公会名称提示
    public UILabel mNameTips;

    // 公会名称输入框
    public UIInput mNameInput;

    // 名称字数提示
    public UILabel mNameAmountTips;

    // 创建公会按钮
    public GameObject mCreateBtn;
    public UILabel mCreateBtnLb;

    // 创建公会消耗
    public UILabel mCost;

    // 消耗品图标
    public UITexture mCostIcon;

    // 介绍提示
    public UILabel mIntroductionTips;

    // 创建提示
    public UILabel mCreateTips;

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

    public GameObject[] mRankConditionItem;

    // 不需要审核
    public GameObject mNoCheckBtn;
    public UILabel mNoCheckLb;

    // 需要审核
    public GameObject mNeedCheckBtn;
    public UILabel mNeedCheckLb;

    public GameObject mRefuseJionBtn;
    public UILabel mRefuseJionLb;

    // 公会名称长度限制
    int mNameLengthLimit = 0;

    // 公会简介内容长度限制
    int mIntroductionLengthLimit = 0;

    // 创建公会等级限制
    int mLevelLimit = 0;

    int mCheckCondition = -1;

    int mStep = -2;

    LPCMapping mCreateCost = LPCMapping.Empty;

    // 旗帜数据
    LPCArray mFlagData = LPCArray.Empty;

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

        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("CreateGangWnd");
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mTitle.text = LocalizationMgr.Get("CreateGuildWnd_1");
        mFlagsTips.text = LocalizationMgr.Get("CreateGuildWnd_2");
        mRandomBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_3");
        mCustomBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_4");
        mNameTips.text = LocalizationMgr.Get("CreateGuildWnd_5");
        mNameInput.defaultText = LocalizationMgr.Get("CreateGuildWnd_6");
        mCreateBtnLb.text = LocalizationMgr.Get("CreateGuildWnd_7");
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
        UIEventListener.Get(mRandomBtn).onClick = OnClickRandomBtn;
        UIEventListener.Get(mCustomBtn).onClick = OnClickCustomBtn;
        UIEventListener.Get(mCreateBtn).onClick = OnClickCreateBtn;
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mNoCheckBtn).onClick = OnClickNoCheckBtn;
        UIEventListener.Get(mNeedCheckBtn).onClick = OnClickNeedCheckBtn;
        UIEventListener.Get(mRefuseJionBtn).onClick = OnClickRefuseJionBtn;
        UIEventListener.Get(mCloseSelectRankWnd).onClick = OnClickCloseSelectRankWnd;
        UIEventListener.Get(mCloseApplicationConditionWnd).onClick = OnClickCloseApplicationCondition;
        UIEventListener.Get(mArenaRankBtn).onClick = OnClickSelectRankConditinBtn;
        UIEventListener.Get(mApplicationConditionBtn).onClick = OnClickSelectCheckCondition;

        for (int i = 0; i < mRankConditionItem.Length; i++)
            UIEventListener.Get(mRankConditionItem[i]).onClick = OnClickRankConditionItem;

        // 注册输入框变化事件
        EventDelegate.Add(mNameInput.onChange, OnNameInputChange);
        EventDelegate.Add(mIntroductionInput.onChange, OnIntroductionChange);

        if (mTweenScale != null)
            EventDelegate.Add(mTweenScale.onFinished, OnFinish);

        if (ME.user == null)
            return;

        ME.user.dbase.RegisterTriggerField("CreateGangWnd", new string[]{"my_gang_info"}, new CallBack(OnMyGangInfoFieldsChange));
    }

    /// <summary>
    /// my_gang_info字段变化回调
    /// </summary>
    void OnMyGangInfoFieldsChange(object para, params object[] param)
    {
        // 关闭创建公会窗口
        WindowMgr.DestroyWindow(CreateGangWnd.WndType);
    }

    void OnFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 名称输入框变化事件回调
    /// </summary>
    void OnNameInputChange()
    {
        mNameAmountTips.text = string.Format("{0}/{1}", mNameInput.value.Length, mNameLengthLimit);
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
        // 等级限制
        mLevelLimit = GameSettingMgr.GetSettingInt("create_gang_level_limit");
        mCreateTips.text = string.Format(LocalizationMgr.Get("CreateGuildWnd_9"), mLevelLimit);

        // 名称长度限制
        mNameLengthLimit = GameSettingMgr.GetSettingInt("max_gang_name_len");

        mNameInput.characterLimit = mNameLengthLimit;

        // 介绍内容长度限制
        mIntroductionLengthLimit = GameSettingMgr.GetSettingInt("max_introduce_len");

        mIntroductionInput.characterLimit = mIntroductionLengthLimit;

        mNameAmountTips.text = string.Format("{0}/{1}", 0, mNameLengthLimit);
        mIntroductionAmountTips.text = string.Format("{0}/{1}", 0, mIntroductionLengthLimit);

        mCreateCost = GameSettingMgr.GetSetting<LPCMapping>("create_gang_cost");

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

        // 随机公会图标
        RandomFlag();
    }

    void RandomFlag()
    {
        mFlagData = LPCArray.Empty;

        List<string> baseIconList = GangMgr.BaseIconList;

        List<string> iconList = GangMgr.IconList;

        List<string> styleList = GangMgr.StyleIconList;

        string baseIcon = baseIconList[Random.Range(0, baseIconList.Count)];

        LPCArray baseData = LPCArray.Empty;
        baseData.Add(baseIcon);
        baseData.Add(Random.Range(0, 256));
        baseData.Add(Random.Range(0, 256));

        mFlagData.Add(baseData);

        string icon = iconList[Random.Range(0, iconList.Count)];
        LPCArray iconData = LPCArray.Empty;
        iconData.Add(icon);

        mFlagData.Add(iconData);

        string styleIcon = styleList[Random.Range(0, styleList.Count)];

        LPCArray styleData = LPCArray.Empty;
        styleData.Add(styleIcon);
        styleData.Add(Random.Range(0, 256));

        mFlagData.Add(styleData);

        // 绑定数据
        mFlagItemWnd.GetComponent<FlagItemWnd>().Bind(mFlagData);
    }

    void OnClickSelectRankConditinBtn(GameObject go)
    {
        mSelectRankWnd.SetActive(true);
        mApplicationConditionWnd.SetActive(false);

        // 默认选项
        if (mStep < -1)
            mStep = -1;
    }

    void OnClickSelectCheckCondition(GameObject go)
    {
        mSelectRankWnd.SetActive(false);
        mApplicationConditionWnd.SetActive(true);

        // 默认选项
        if (mCheckCondition < 0)
            mCheckCondition = (int) CHECK_CONDITION.NO_CHECK;
    }

    /// <summary>
    /// 随机制旗按钮点击事件
    /// </summary>
    void OnClickRandomBtn(GameObject go)
    {
        // 随机旗帜
        RandomFlag();
    }

    /// <summary>
    /// 自主制旗点击事件
    /// </summary>
    void OnClickCustomBtn(GameObject go)
    {
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
    }

    /// <summary>
    /// 创建公会按钮点击事件
    /// </summary>
    void OnClickCreateBtn(GameObject go)
    {
        if (ME.user == null)
            return;

        string fields = FieldsMgr.GetFieldInMapping(mCreateCost);

        DialogMgr.ShowDailog(
            new CallBack(OnCreateConfirmCallBack),
            string.Format(
                LocalizationMgr.Get("CreateGuildWnd_16"),
                mCreateCost.GetValue<int>(fields),
                FieldsMgr.GetFieldTexture(fields)
            )
        );
    }

    /// <summary>
    /// 创建确认框回调
    /// </summary>
    void OnCreateConfirmCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        // 入会条件
        LPCMapping condition = LPCMapping.Empty;
        if (mStep >= -1 && mCheckCondition >= 0)
        {
            condition.Add("step" , mStep);
            condition.Add("check" , mCheckCondition);
        }

        if (mNameInput.value.Trim().Length == 0)
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("GangWnd_143"));
        }

        // 通知服务器创建公会
        GangMgr.CreateGang(mNameInput.value.Trim(), mFlagData, mIntroductionInput.value.Trim(), condition);
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
}
