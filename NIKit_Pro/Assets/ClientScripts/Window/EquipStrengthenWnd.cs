/// <summary>
/// EquipStrengthenWnd.cs
/// Created by fengsc 2016/08/10
/// 装备强化界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class EquipStrengthenWnd : WindowBase<EquipStrengthenWnd>
{

    #region 成员变量

    //关闭按钮;
    public GameObject CloseBtn;

    // 装备基础格子
    public GameObject mEquipItem;

    // 装备描述;
    public UILabel mEquipDesc;

    // 强化装备按钮
    public GameObject mStrengthenBtn;
    public UILabel mStrBtnLabel;

    // 强化装备至成功按钮
    public GameObject mStrengthenToSuccessBtn;
    public UILabel mStrToSuccBtnLabel;

    // 背景序列帧动画
    public UISpriteAnimation mAnimaBg;

    public TweenAlpha mAnimaAlpha;

    // 强化效果序列帧动画
    public UISpriteAnimation mStrEffectAnima;

    public UISpriteAnimation mStrSuccessFlashAnima;

    // 强化成功文字提示
    public GameObject mStrengthenSuccTips;
    public UILabel mStrengthenSuccLabel;

    // 强化失败文字提示
    public GameObject mStrengthenFailedTips;
    public UILabel mStrengthenFailedLabel;

    // 装备主属性
    public UILabel mMainProp;

    // 装备附加属性
    public GameObject mMinorProp;

    // 词缀属性;
    public UILabel mPrefixProp;

    // 排序组件
    public UIGrid mGrid;

    // 属性面板强化选项
    public GameObject mStrengthenOptions;
    public UILabel mStrLabel;

    // 增幅选项
    public GameObject mIncreaseOptions;
    public UILabel mIncreaseLabel;

    // 重置选项
    public GameObject mResetOptions;
    public UILabel mResetLabel;

    public UILabel mStrTitle;

    // 将要出现追加属性提示
    public UILabel mWillNewPropTips;

    // 出现追加属性提示
    public UILabel mAppearNewPropTips;
    public GameObject mTipsGo;

    // 强化消耗
    public UILabel mStrengthenCost;

    // 强化到下一等级的主属性值
    public UILabel mNextRankMainPropValue;

    public UISprite mEffectBg;

    // 点击强化至成功的强化次数
    public UILabel mRepeatTime;

    public TweenAlpha mTweenAlpha;

    public TweenScale mTweenScale;

    public GameObject mGuideMask;

    // 强化帮助按钮
    public UILabel mIntensifyHelpBtn;

    public UILabel mLimitTips;

    // 分享按钮
    public GameObject mShareBtn;
    public UILabel mShareBtnLb;

    public GameObject mShareMask;

    //装备对象;
    private Property mEquipOb;

    // 装备rid
    private string mEquipRid = string.Empty;

    //强化结果
    private int mResult = -1;

    // 强化效果动画是否播放完成;
    private bool IsPlayingEffectAnim = false;

    // 装备的属性数据
    private LPCMapping equipProp = LPCMapping.Empty;

    // 附加属性
    private LPCArray minorProp = LPCArray.Empty;

    // 词缀属性
    private LPCArray prefixProp = LPCArray.Empty;

    // 主属性
    private LPCArray mainProp = LPCArray.Empty;

    bool IsCache = false;

    bool IsGrey = false;

    // 是否是快速强化
    bool mIsFastIntensify = false;

    // 装备做最大强化等级
    private int mEquipIntensifyMaxLV = 0;

    private string filed = string.Empty;

    private LPCMapping costMap = LPCMapping.Empty;

    // 缓存预创建的游戏物体
    private List<GameObject> mCacheList = new List<GameObject>();

    // 缓存附加属性的id和属性值
    Dictionary<int, int> mMinorPropId = new Dictionary<int, int>();
    //缓存旧的附加属性
    Dictionary<int, int> mOldMinorPropId = new Dictionary<int, int>();

    // 强化成功提示文字 tween动画组件
    TweenAlpha mSuccessAlpha;
    TweenPosition mSuccessPos;
    TweenScale mSuccessScale;

    TweenPosition mMaxLVTipsPos;
    TweenAlpha[] mMaxLVTipsAlpha;

    int mMaxEquipIntensify = 0;

    int mLimitRank = 0;

    #endregion

    #region 内部函数

    void Awake()
    {
        // 脚本唤醒的时候创建一批
        CreateGameObject();
    }

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        // 初始化Tween动画组件
        InitTweenComponent();

        InitLocalLabel();

        // 刷新指引遮罩
        RefreshGuideMask();

        // 装备最大强化次数
        mMaxEquipIntensify = GameSettingMgr.GetSettingInt("max_equip_intensify");

        mLimitRank = GameSettingMgr.GetSettingInt("limit_equip_intensify_rank");

        // 显示装备强化次数限制
        RefreshLimitData();

        // 每天零点刷新一次
        InvokeRepeating("RefreshLimitData", (float) Game.GetZeroClock(1), 86400);

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

    void OnDestroy()
    {
        ClearCacheList();

        CancelInvoke("RefreshLimitData");

        EventMgr.UnregisterEvent("EquipStrengthenWnd");

        MsgMgr.RemoveDoneHook("MSG_LOGIN_NOTIFY_OK", "EquipStrengthenWnd");
    }

    void Update()
    {
        SetSpriteAnimation();
    }

    /// <summary>
    /// 判断序列帧动画
    /// </summary>
    void SetSpriteAnimation()
    {
        // 强化效果播放完成;(isPlaying 返回的是mActive的值)
        if (!mStrEffectAnima.isPlaying && mStrEffectAnima.gameObject.activeSelf)
        {
            // 重置(使UISpriteAnimation脚本中的mIndex == 0, mActive == true)
            mStrEffectAnima.ResetToBeginning();
            mStrEffectAnima.enabled = false;
            mStrEffectAnima.gameObject.SetActive(false);
            IsPlayingEffectAnim = true;
        }

        if (!mStrSuccessFlashAnima.isPlaying && mStrSuccessFlashAnima.gameObject.activeSelf)
        {
            mStrSuccessFlashAnima.ResetToBeginning();
            mStrSuccessFlashAnima.enabled = false;
            mStrSuccessFlashAnima.gameObject.SetActive(false);

            if (mResult == 1)
                // 还原装备的背景
                ReturnEquipBg();
        }

        // 强化效果动画播放完成
        if (IsPlayingEffectAnim)
        {
            IsPlayingEffectAnim = false;

            // 根据强化结果播放相应的动画
            AccrodingToResultPlayAnima();
        }
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocalLabel()
    {
        mStrengthenSuccLabel.text = LocalizationMgr.Get("EquipStrengthenWnd_6");
        mStrengthenFailedLabel.text = LocalizationMgr.Get("EquipStrengthenWnd_7");
        mStrLabel.text = LocalizationMgr.Get("EquipStrengthenWnd_1");
        mIncreaseLabel.text = LocalizationMgr.Get("EquipStrengthenWnd_2");
        mResetLabel.text = LocalizationMgr.Get("EquipStrengthenWnd_3");
        mStrTitle.text = LocalizationMgr.Get("EquipStrengthenWnd_4");
        mStrBtnLabel.text = LocalizationMgr.Get("EquipStrengthenWnd_1");
        mStrToSuccBtnLabel.text = LocalizationMgr.Get("EquipStrengthenWnd_5");
        mIntensifyHelpBtn.text = LocalizationMgr.Get("EquipStrengthenWnd_17");
        mShareBtnLb.text = LocalizationMgr.Get("EquipStrengthenWnd_21");
    }

    /// <summary>
    /// 显示装备强化次数限制
    /// </summary>
    void ShowLimitTips()
    {
        // 开启版署模式累计许愿次数
        if (ME.user.QueryTemp<int>("gapp_world") != 1)
        {
            mLimitTips.gameObject.SetActive(false);
            return;
        }

        LPCMapping limitData = LPCMapping.Empty;

        LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_equip_intensify");
        if (v != null && v.IsMapping)
            limitData = v.AsMapping;

        mLimitTips.text = string.Format(LocalizationMgr.Get("EquipStrengthenWnd_18"), mLimitRank, limitData.GetValue<int>("amount"), mMaxEquipIntensify);

        mLimitTips.gameObject.SetActive(true);
    }

    void RefreshLimitData()
    {
        // 开启版署模式累计许愿次数
        if (ME.user.QueryTemp<int>("gapp_world") == 1)
        {
            LPCMapping limitData = LPCMapping.Empty;

            LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_equip_intensify");
            if (v != null && v.IsMapping)
                limitData = v.AsMapping;

            // 重置数据
            if (!TimeMgr.IsSameDay(TimeMgr.GetServerTime(), limitData.GetValue<int>("refresh_time")))
                OptionMgr.SetLocalOption(ME.user, "limit_equip_intensify", LPCValue.Create(LPCMapping.Empty));
        }

        // 限制数据
        ShowLimitTips();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if (GuideMgr.IsGuided(GuideMgr.SHARE_SHOW_GUIDE_GROUP) && ShareMgr.IsOpenShare())
            mShareBtn.SetActive(true);
        else
            mShareBtn.SetActive(false);

        // 获取装备对象
        mEquipOb = Rid.FindObjectByRid(mEquipRid);

        if (mEquipOb == null || mGrid == null)
            return;

        if (mEquipItem != null)
            mEquipItem.GetComponent<EquipItemWnd>().SetBind(mEquipOb, false);

        // 获取强化等级、稀有度;
        int rank = mEquipOb.Query<int>("rank");
        int rarity = mEquipOb.GetRarity();

        costMap = GetStrengthenCost(rank);

        filed = FieldsMgr.GetFieldInMapping(costMap);

        // 获取装备的最大强化等级
        mEquipIntensifyMaxLV = GameSettingMgr.GetSettingInt("equip_intensify_limit_level");

        // 显示强化消耗
        mStrengthenCost.text = Game.SetMoneyShowFormat(costMap.GetValue<int>(filed));

        mWillNewPropTips.text = string.Empty;

        // 出现追加属性提示
        mWillNewPropTips.text = CALC_EQUIP_INTENSIFY_TIPS.CALL(rank, rarity, mEquipIntensifyMaxLV);

        // 获取颜色标签
        string colorLabel = ColorConfig.GetColor(mEquipOb.GetRarity());

        equipProp = mEquipOb.Query<LPCMapping>("prop");

        if (equipProp == null)
            return;

        // 主属性
        mainProp = equipProp.GetValue<LPCArray>(EquipConst.MAIN_PROP);

        // 获取词缀属性
        prefixProp = equipProp.GetValue<LPCArray>(EquipConst.PREFIX_PROP);

        // 附加属性
        minorProp = equipProp.GetValue<LPCArray>(EquipConst.MINOR_PROP);

        if (IsCache)
        {
            CacheMinorPropId();
            IsCache = false;
        }

        // 获取装备短描述;
        string shortDesc = mEquipOb.Short();

        // 装备的属性值
        string propValue = string.Empty;

        if (mainProp != null)
        {
            LPCArray array = LPCArray.Empty;
            int value = 0;
            foreach (LPCValue item in mainProp.Values)
            {
                // 强化到最大等级的时候不显示下一级属性值
                if (rank == mEquipIntensifyMaxLV)
                    mNextRankMainPropValue.transform.parent.gameObject.SetActive(false);

                propValue += PropMgr.GetPropDesc(item.AsArray, EquipConst.MAIN_PROP);

                value = FetchPropMgr.GetMainPropIntensifyValue(mEquipOb, item.AsArray[0].AsInt, 1) + item.AsArray[1].AsInt;

                array.Add(item.AsArray[0].AsInt);

                array.Add(value);

                mNextRankMainPropValue.text = PropMgr.GetPropValueDesc(array, rank);
            }

            mMainProp.text = propValue;
        }
        else
            mMainProp.text = string.Empty;

        if (prefixProp != null)
        {
            if (rank > 0)
                mEquipDesc.text = string.Format("[{0}]+{1}{2}[-]", colorLabel, rank + "  ", shortDesc);
            else
                mEquipDesc.text = string.Format("[{0}]{1}[-]", colorLabel, shortDesc);

            propValue = string.Empty;

            foreach (LPCValue item in prefixProp.Values)
                propValue += PropMgr.GetPropDesc(item.AsArray, EquipConst.MINOR_PROP);

            mPrefixProp.text = propValue;
        }
        else
        {
            if (rank > 0)
                mEquipDesc.text = string.Format("[{0}]+{1}{2}[-]", colorLabel, rank + "  ", shortDesc);
            else
                mEquipDesc.text = string.Format("[{0}]{1}[-]", colorLabel, shortDesc);

            mPrefixProp.text = string.Empty;

            mGrid.transform.localPosition = mPrefixProp.transform.localPosition;
        }

        if (minorProp != null)
        {
            int index = 0;
            foreach (LPCValue item in minorProp.Values)
            {
                GameObject go = mCacheList[index];
                index++;

                GameObject minorPropGo = go.transform.Find("minorProp").gameObject;

                if (minorPropGo == null)
                    continue;

                UILabel label = minorPropGo.GetComponent<UILabel>();

                if (label == null)
                    continue;

                string propDesc = PropMgr.GetPropDesc(item.AsArray, EquipConst.MINOR_PROP);

                UILabel newPropTips = go.transform.Find("add").Find("add_prop").GetComponent<UILabel>();
                if (newPropTips != null)
                    newPropTips.text = string.Format(LocalizationMgr.Get("EquipStrengthenWnd_15"), PropMgr.GetPropDesc(item.AsArray));

                // 设置新增属性的提示文字的颜色
                newPropTips.color = new Color(167f / 255, 255f / 255, 139f / 255);

                go.SetActive(true);

                if (mMinorPropId.ContainsKey(item.AsArray[0].AsInt))
                {
                    label.alpha = 1;

                    if (!mMinorPropId[item.AsArray[0].AsInt].Equals(item.AsArray[1].AsInt))
                        // 强化已有的属性播放Tween动画
                        IntensifyPropPlayTweenAnimation(go, item.AsArray);
                    else
                        label.text = propDesc;
                }
                else
                    label.text = propDesc;

                if (rank > 3)
                {
                    // 出现新的附加属性添加Tween动画
                    if (mMinorPropId.ContainsKey(item.AsArray[0].AsInt))
                        continue;

                    PropChangeAddTweenAnimation(go, rank, item.AsArray);
                }
                if (rank <= 3 && mMinorPropId.Count < 1)
                    PropChangeAddTweenAnimation(go, rank, item.AsArray);
            }
        }

        // 缓存附加属性id
        CacheMinorPropId();
    }

    /// <summary>
    ///  绘制窗口前先创建一批gameobject缓存
    /// </summary>
    void CreateGameObject()
    {
        mMinorProp.SetActive(false);

        for (int i = 0; i < 5; i++)
        {
            GameObject clone = Instantiate(mMinorProp.gameObject) as GameObject;

            clone.gameObject.SetActive(true);

            clone.transform.SetParent(mGrid.transform);

            clone.transform.localScale = Vector3.one;

            clone.transform.localPosition = new Vector3(0, 0 - 35 * i, 0);

            clone.SetActive(true);

            clone.transform.Find("minorProp").GetComponent<UILabel>().text = string.Empty;

            // 将创建的列表添加到列表中
            mCacheList.Add(clone);
        }
    }

    /// <summary>
    /// 清理缓存列表
    /// </summary>
    void ClearCacheList()
    {
        // 销毁创建的游戏物体
        foreach (GameObject item in mCacheList)
            GameObject.Destroy(item);

        // 清空缓存列表
        mCacheList.Clear();
    }

    /// <summary>
    /// 强化属性时播放动画
    /// </summary>
    void IntensifyPropPlayTweenAnimation(GameObject go, LPCArray array)
    {
        GameObject minorProp = go.transform.Find("minorProp").gameObject;

        if (minorProp == null)
            return;

        GameObject intensify = go.transform.Find("intensify").gameObject;

        if (intensify == null)
            return;

        GameObject arrow = intensify.transform.Find("arrow").gameObject;
        if (arrow == null)
            return;

        GameObject intensifyProp = intensify.transform.Find("intensify_prop").gameObject;
        if (intensifyProp == null)
            return;
        intensifyProp.GetComponent<UILabel>().text = PropMgr.GetPropValueDesc(array);

        // 间距
        int spacing = 75;

        // 箭头与新属性值label控件之间的间距
        float interval = 50f;

        float arrowWidth = arrow.GetComponent<UISprite>().localSize.x;

        float minorPropWidth = minorProp.GetComponent<UILabel>().localSize.x;

        // 设置属性提示的空间的位置,距离minorProp实体对象右边75个单位
        arrow.transform.localPosition = new Vector3(
            minorProp.transform.localPosition.x + minorPropWidth + spacing,
            arrow.transform.localPosition.y,
            arrow.transform.localPosition.z);

        intensifyProp.transform.localPosition = new Vector3(arrow.transform.localPosition.x + interval,
            arrow.transform.localPosition.y,
            arrow.transform.localPosition.z);

        // 获取intensify对象上所有挂载的TweenPosition控件
        TweenAlpha[] alphas = intensify.GetComponents<TweenAlpha>();
        for (int i = 0; i < alphas.Length; i++)
        {
            alphas[i].ResetToBeginning();
            alphas[i].PlayForward();
        }


        TweenPosition tweenPos = arrow.GetComponent<TweenPosition>();
        if (tweenPos == null)
            return;

        tweenPos.from = arrow.transform.localPosition;
        tweenPos.to = new Vector3(
            arrow.transform.localPosition.x - spacing + arrowWidth / 2,
            arrow.transform.localPosition.y,
            arrow.transform.localPosition.z);

        tweenPos.PlayForward();

        tweenPos.ResetToBeginning();

        // 添加回调函数,tweenPos动画播放完成执行TweenPositionOnfinish方法
        EventDelegate.Add(tweenPos.onFinished, TweenPositionOnfinish);

        TweenPosition intensiyPos = intensifyProp.GetComponent<TweenPosition>();
        if (intensiyPos == null)
            return;

        intensiyPos.from = intensifyProp.transform.localPosition;
        intensiyPos.to = new Vector3(
            intensifyProp.transform.localPosition.x - spacing,
            intensifyProp.transform.localPosition.y,
            intensifyProp.transform.localPosition.z);

        intensiyPos.PlayForward();

        intensiyPos.ResetToBeginning();
    }

    void TweenPositionOnfinish()
    {
        int index = 0;
        foreach (LPCValue item in minorProp.Values)
        {
            GameObject go = mCacheList[index];
            index++;

            GameObject minorPropGo = go.transform.Find("minorProp").gameObject;

            if (minorPropGo == null)
                continue;

            UILabel label = minorPropGo.GetComponent<UILabel>();

            if (label == null)
                continue;

            string propDesc = PropMgr.GetPropDesc(item.AsArray, EquipConst.MINOR_PROP);

            label.text = propDesc;

            go.SetActive(true);
        }
    }

    /// <summary>
    /// 追加新属性时添加Tween动画组件
    /// </summary>
    void PropChangeAddTweenAnimation(GameObject go, int rank, LPCArray array)
    {
        GameObject addGo = go.transform.Find("add").gameObject;

        if (addGo == null)
            return;

        TweenAlpha[] addGoAlpha = addGo.GetComponents<TweenAlpha>();
        for (int i = 0; i < addGoAlpha.Length; i++)
        {
            addGoAlpha[i].ResetToBeginning();
            addGoAlpha[i].PlayForward();
        }

        UILabel addProp = addGo.transform.Find("add_prop").GetComponent<UILabel>();

        GameObject underLine = addGo.transform.Find("uderline").gameObject;

        // 记录控件的位置
        Vector3 pos = addProp.transform.transform.localPosition;

        if (addProp != null && underLine != null)
        {
            TweenPosition[] tweenPos = addProp.gameObject.GetComponents<TweenPosition>();

            TweenPosition fromPos = tweenPos[0];
            TweenPosition toPos = tweenPos[1];

            Vector3 tempPos = new Vector3(
                                  underLine.transform.localPosition.x - (addProp.localSize.x / 2),
                                  addProp.transform.localPosition.y,
                                  addProp.transform.localPosition.z);

            fromPos.ResetToBeginning();
            fromPos.to = tempPos;
            fromPos.from = pos;
            fromPos.PlayForward();

            toPos.ResetToBeginning();
            toPos.from = tempPos;
            toPos.to = pos;
            toPos.PlayForward();
        }

        go.transform.Find("minorProp").GetComponent<TweenAlpha>().PlayForward();


        if (mTipsGo == null || mAppearNewPropTips == null)
            return;

        mAppearNewPropTips.text = string.Format(LocalizationMgr.Get("EquipStrengthenWnd_11"), rank, PropMgr.GetPropDesc(array, EquipConst.MINOR_PROP));

        TweenAlpha[] alphas = mTipsGo.GetComponents<TweenAlpha>();

        if (alphas == null)
            return;

        alphas[0].ResetToBeginning();
        alphas[1].ResetToBeginning();

        alphas[0].PlayForward();
        alphas[1].PlayForward();
    }

    /// <summary>
    /// 登陆成功回调
    /// </summary>
    private void WhenLoginOk(string cmd, LPCValue para)
    {
        // 刷新窗口
        Redraw();

        // 恢复按钮状态
        ReturnToNormalButtonState();

        // 刷新指引遮罩
        RefreshGuideMask();

        RefreshShareBtn();

        // 装备强化完成抛出事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_EQUIP_STRENGTHEN_FINISH, null);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮的点击事件
        RegisterPartEvent();

        // 注册装备强化事件
        EventMgr.RegisterEvent("EquipStrengthenWnd", EventMgrEventType.EVENT_EQUIP_STRENGTHEN, OnEquipStrengthen);

        MsgMgr.RegisterDoneHook("MSG_LOGIN_NOTIFY_OK", "EquipStrengthenWnd", WhenLoginOk);

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);
    }

    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 注册部分按钮点击事件,因为这些按钮在强化操作时需要禁用
    /// </summary>
    void RegisterPartEvent()
    {
        UIEventListener.Get(mStrengthenOptions).onClick = OnClickStrengthenOptions;
        UIEventListener.Get(mIncreaseOptions).onClick = OnClickIncreaseOptions;
        UIEventListener.Get(mResetOptions).onClick = OnClickResetOptions;
        UIEventListener.Get(CloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mStrengthenBtn).onClick = OnClickStrengthenBtn;
        UIEventListener.Get(mStrengthenToSuccessBtn).onClick = OnClickStrengthenToSuccessBtn;
        UIEventListener.Get(mIntensifyHelpBtn.gameObject).onClick = OnClickHelpBtn;
        UIEventListener.Get(mShareBtn).onClick = OnClickShareBtn;


        // 激活UIToggle控件
        mStrengthenOptions.GetComponent<UIToggle>().enabled = true;
        mIncreaseOptions.GetComponent<UIToggle>().enabled = true;
        mResetOptions.GetComponent<UIToggle>().enabled = true;
    }

    /// <summary>
    /// 解注册事件
    /// </summary>
    void UnRegisterPartEvent()
    {
        UIEventListener.Get(mStrengthenOptions).onClick -= OnClickStrengthenOptions;
        UIEventListener.Get(mIncreaseOptions).onClick -= OnClickIncreaseOptions;
        UIEventListener.Get(mResetOptions).onClick -= OnClickResetOptions;
        UIEventListener.Get(CloseBtn).onClick -= OnClickCloseBtn;
        UIEventListener.Get(mShareBtn).onClick -= OnClickShareBtn;

        // 激死UIToggle控件
        mStrengthenOptions.GetComponent<UIToggle>().enabled = false;
        mIncreaseOptions.GetComponent<UIToggle>().enabled = false;
        mResetOptions.GetComponent<UIToggle>().enabled = false;
    }


    /// <summary>
    /// 初始化Tween动画组件
    /// </summary>
    void InitTweenComponent()
    {
        if (mStrengthenFailedTips == null || mStrengthenSuccTips == null)
            return;

        mSuccessAlpha = mStrengthenSuccTips.GetComponent<TweenAlpha>();
        mSuccessPos = mStrengthenSuccTips.GetComponent<TweenPosition>();
        mSuccessScale = mStrengthenSuccTips.GetComponent<TweenScale>();
    }

    /// <summary>
    /// 刷新指引遮罩
    /// </summary>
    void RefreshGuideMask()
    {
        if (GuideMgr.IsGuiding())
        {
            mGuideMask.SetActive(true);
        }
        else
        {
            mGuideMask.SetActive(false);
        }
    }

    /// <summary>
    /// 向服务器发送消息
    /// </summary>
    void SendMessage()
    {
        // 发送消息通知服务器执行强化操作
        LPCMapping cmdAgrs = new LPCMapping();

        cmdAgrs.Add("rid", mEquipRid);
        Operation.CmdBlacksmithAction.Go("intensify", cmdAgrs);
    }

    /// <summary>
    /// 装备强化消息回调
    /// </summary>
    void OnEquipStrengthen(int eventId, MixedValue para)
    {
        if (!mIsFastIntensify)
        {
            if (para == null)
                return;

            LPCMapping map = para.GetValue<LPCMapping>();

            if (map == null)
                return;

            mResult = map.GetValue<int>("result");

            SetEquipBgAlphaAnima();

            ShowOnClickStrengthenAnima();
        }
        else
        {
            CacheOldMinorPropId();

            // 重绘窗口
            Redraw();

            ShowLimitTips();
        }

        // 刷新分享按钮状态
        RefreshShareBtn();

        // 播放音效
        GameSoundMgr.PlayGroupSound(mResult == 1 ? "equip_str_success":"equip_str_fail");
    }

    void RefreshShareBtn()
    {
        if (mEquipOb.GetRank() < GameSettingMgr.GetSetting<int>("share_equip_intensify_lv"))
        {
            mShareMask.SetActive(true);
        }
        else
        {
            mShareMask.SetActive(false);
        }
    }

    /// <summary>
    /// 根据强化结果播放相应的动画
    /// </summary>
    void AccrodingToResultPlayAnima()
    {
        if (mResult < 0)
            return;

        if (mResult == 1)
        {
            CacheOldMinorPropId();

            // 强化成功重绘窗口;
            Redraw();

            SuccessScaleOnFinished();

            if (mSuccessAlpha == null || mSuccessPos == null || mSuccessScale == null)
                return;

            // 重置组件;
            mSuccessAlpha.ResetToBeginning();
            mSuccessPos.ResetToBeginning();
            mSuccessScale.ResetToBeginning();

            mSuccessAlpha.PlayForward();
            mSuccessPos.PlayForward();
            mSuccessScale.PlayForward();

            // 添加动画执行完成的事件
            EventDelegate.Add(mSuccessAlpha.onFinished, SuccessAlphaOnfinished);

            mAnimaAlpha.ResetToBeginning();

            mAnimaAlpha.PlayForward();
        }
        // 强化失败
        else
        {
            mStrEffectAnima.enabled = false;
            mStrEffectAnima.gameObject.SetActive(false);

            mAnimaBg.framesPerSecond = 60;
            mAnimaBg.frameIndex = 23;
            mAnimaBg.PlayReverse();

            TweenAlpha[] alphas = mStrengthenFailedTips.GetComponents<TweenAlpha>();
            TweenScale scale = mStrengthenFailedTips.GetComponent<TweenScale>();

            if (scale == null || alphas == null)
                return;

            // 重置组件
            scale.ResetToBeginning();
            alphas[0].ResetToBeginning();
            alphas[1].ResetToBeginning();

            scale.PlayForward();
            alphas[0].PlayForward();
            alphas[1].PlayForward();

            EventDelegate.Add(alphas[1].onFinished, FailedPosOnfinished);

            ReturnEquipBg();
        }
    }

    /// <summary>
    /// 恢复按钮的点击事件和颜色
    /// </summary>
    void ReturnToNormalButtonState()
    {
        // 注册按钮的点击事件
        RegisterPartEvent();

        mStrToSuccBtnLabel.text = LocalizationMgr.Get("EquipStrengthenWnd_5");

        mStrToSuccBtnLabel.gradientBottom = new Color(195f / 255, 180f / 255, 154f / 255);

        // 恢复按钮颜色;
        float value = 255f / 255;

        Color color = new Color(value, value, value);

        mStrengthenBtn.GetComponent<UISprite>().color = color;
        mStrengthenToSuccessBtn.GetComponent<UISprite>().color = color;

        mRepeatTime.gameObject.SetActive(false);
    }

    /// <summary>
    /// 装备强化过程中按钮的状态
    /// </summary>
    void StrProcessButtonState()
    {
        // 解注册点击事件
        UIEventListener.Get(mStrengthenBtn).onClick -= OnClickStrengthenBtn;
        UIEventListener.Get(mStrengthenToSuccessBtn).onClick -= OnClickStrengthenToSuccessBtn;

        float value = 120f / 255;

        Color color = new Color(value, value, value);

        mStrengthenBtn.GetComponent<UISprite>().color = color;

        if (IsGrey)
            mStrengthenToSuccessBtn.GetComponent<UISprite>().color = color;
    }

    /// <summary>
    /// 失败提示执行完Tweenposition动画执行的函数
    /// </summary>
    void FailedPosOnfinished()
    {
        ShowLimitTips();

        ReturnToNormalButtonState();
    }

    /// <summary>
    /// 强化成功提示文字动画播放完成执行的操作
    /// </summary>
    void SuccessAlphaOnfinished()
    {
        // 装备强化完成抛出事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_EQUIP_STRENGTHEN_FINISH, null, true);

        ShowLimitTips();

        Coroutine.DispatchService(Reset());
    }

    IEnumerator Reset()
    {
        yield return null;

        // 恢复强化按钮的点击事件和颜色
        ReturnToNormalButtonState();
    }

    void SuccessScaleOnFinished()
    {
        mStrSuccessFlashAnima.enabled = false;
        mStrSuccessFlashAnima.gameObject.SetActive(false);

        // 设置序列帧动画组件的相关参数
        mStrSuccessFlashAnima.gameObject.SetActive(true);
        mStrSuccessFlashAnima.ResetToBeginning();
        mStrSuccessFlashAnima.framesPerSecond = 20;
        mStrSuccessFlashAnima.namePrefix = "PetStrStart";
        mStrSuccessFlashAnima.loop = false;
        mStrSuccessFlashAnima.enabled = true;
    }

    /// <summary>
    /// 强化成功提示TweenPosition执行完成的操作
    /// </summary>
    void SuccessPosOnFinished()
    {
        // 重新设置背景动画的帧率并倒序播放动画
        mAnimaBg.framesPerSecond = 30;
        mAnimaBg.frameIndex = 23;
        mAnimaBg.PlayReverse();
    }

    /// <summary>
    /// 显示点击强化的动画
    /// </summary>
    void ShowOnClickStrengthenAnima()
    {
        if (mAnimaBg == null || mStrEffectAnima == null)
            return;

        mAnimaBg.ResetToBeginning();
        mStrEffectAnima.ResetToBeginning();

        mAnimaBg.gameObject.GetComponent<UISprite>().alpha = 1;

        // 序列帧图片名称的前缀
        mAnimaBg.namePrefix = "qianghua";
        // 帧率
        mAnimaBg.framesPerSecond = 30;
        // 是否循环
        mAnimaBg.loop = false;
        mAnimaBg.enabled = true;


        // 强化效果图片名称前缀;
        mStrEffectAnima.gameObject.SetActive(true);
        mStrEffectAnima.namePrefix = "BeforeStrengthen";
        mStrEffectAnima.framesPerSecond = 30;
        mStrEffectAnima.loop = false;
        mStrEffectAnima.enabled = true;
    }

    /// <summary>
    /// 检查能否强化
    /// </summary>
    bool CheckCanStrengthen()
    {
        // 装备对象不存在
        if (mEquipOb == null)
            return false;

        // 当前装备的强化等级
        int intensify_level = mEquipOb.Query<int>("rank");

        if (intensify_level >= mEquipIntensifyMaxLV)
        {
            // 装备强化等级达到上限
            DialogMgr.Notify(LocalizationMgr.Get("EquipStrengthenWnd_8"));

            return false;
        }

        if (costMap == null || string.IsNullOrEmpty(filed))
            return false;

        // 金钱不足
        if (ME.user.Query<int>(filed) < costMap.GetValue<int>(filed))
        {
            // 提示金钱不足
            DialogMgr.Notify(LocalizationMgr.Get("EquipStrengthenWnd_12"));
            return false;
        }

        return true;
    }

    /// <summary>
    /// 获取强化消耗
    /// </summary>
    LPCMapping GetStrengthenCost(int rank)
    {
        CsvFile csv = BlacksmithMgr.IntensifyCsv;

        // 装备强化至最大等级
        if (rank + 1 > GameSettingMgr.GetSettingInt("equip_intensify_limit_level"))
            return LPCMapping.Empty;

        CsvRow row = csv.FindByKey(rank + 1);

        // 获取强化脚本编号
        int scriptNo = row.Query<int>("cost_script");

        LPCMapping args = new LPCMapping();

        args.Add("star", mEquipOb.Query<int>("star"));
        args.Add("rank", rank + 1);

        // 获取装备强化的消耗
        object ret = ScriptMgr.Call(scriptNo, args);

        if (ret == null)
        {
            LogMgr.Trace("没有获取到计算信息");
            return LPCMapping.Empty;
        }

        return ret as LPCMapping;
    }

    /// <summary>
    /// 最大等级提示position执行完成
    /// </summary>
    void MaxLVPosOnFinished()
    {
        ReturnToNormalButtonState();
    }

    /// <summary>
    /// 分享按钮点击回调
    /// </summary>
    void OnClickShareBtn(GameObject go)
    {
        // 装备分享等级限制
        int shareLimit = GameSettingMgr.GetSettingInt("share_equip_intensify_lv");

        // 装备强化等级不满足要求
        if (mEquipOb.GetRank() < shareLimit)
        {
            DialogMgr.Notify(string.Format(LocalizationMgr.Get("EquipStrengthenWnd_22"), shareLimit));
            return;
        }

        // 打开装备分享界面
        GameObject wndGo = WindowMgr.OpenWnd(ShareOperateWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wndGo == null)
            return;

        wndGo.GetComponent<ShareOperateWnd>().BindData(ShareOperateWnd.ShareOperateType.EquipIntensify, mEquipOb, mOldMinorPropId);
    }

    /// <summary>
    /// 帮助按钮点击事件回调
    /// </summary>
    void OnClickHelpBtn(GameObject go)
    {
        // 获取帮助信息界面;
        GameObject wnd = WindowMgr.OpenWnd(HelpWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
            return;

        // 装备强化
        wnd.GetComponent<HelpWnd>().Bind(HelpConst.EQUIP_INTENSIFY_ID);
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        if (GuideMgr.IsGuiding())
            return;

        WindowMgr.DestroyWindow(gameObject.name);

        // 显示装备信息窗口
        GameObject wnd = WindowMgr.GetWindow("EquipViewWnd_Equip");

        if (wnd != null)
        {
            WindowMgr.AddToOpenWndList(wnd, WindowOpenGroup.SINGLE_OPEN_WND);

            WindowMgr.ShowWindow(wnd);
        }

        GameObject unWnd = WindowMgr.GetWindow("EquipViewWnd_UnEquip");
        if (unWnd != null)
        {
            WindowMgr.AddToOpenWndList(unWnd, WindowOpenGroup.SINGLE_OPEN_WND);

            WindowMgr.ShowWindow(unWnd);
        }

        // 抛出事件刷新装备窗口
        EventMgr.FireEvent(EventMgrEventType.EVENT_CLOSE_EQUIP_STRENTHEN, null);
    }

    bool CheckLimit()
    {
        // 开启版署模式累计许愿次数
        if (ME.user.QueryTemp<int>("gapp_world") != 1)
            return true;

        LPCMapping limitData = LPCMapping.Empty;

        LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_equip_intensify");
        if (v != null && v.IsMapping)
            limitData = v.AsMapping;

        // 重置数据
        if (!TimeMgr.IsSameDay(TimeMgr.GetServerTime(), limitData.GetValue<int>("refresh_time")))
        {
            OptionMgr.SetLocalOption(ME.user, "limit_equip_intensify", LPCValue.Create(LPCMapping.Empty));

            return true;
        }

        if (limitData.GetValue<int>("amount") >= mMaxEquipIntensify)
        {
            DialogMgr.Notify(LocalizationMgr.Get("EquipStrengthenWnd_19"));
            return false;
        }

        return true;
    }

    /// <summary>
    /// 强化按钮点击事件
    /// </summary>
    void OnClickStrengthenBtn(GameObject go)
    {
        if (GuideMgr.IsGuiding())
            return;

        if (!CheckLimit())
            return;

        mIsFastIntensify = false;

        DoStrengthen();
    }

    /// <summary>
    /// 执行强化
    /// </summary>
    void DoStrengthen()
    {
        IsGrey = true;

        StrProcessButtonState();

        // 检查是否可以强化装备
        if (!CheckCanStrengthen())
        {
            // 恢复按钮的状态
            ReturnToNormalButtonState();
            return;
        }

        UnRegisterPartEvent();

        // 发送消息至服务器
        SendMessage();
    }

    /// <summary>
    /// 设置装备的背景动画
    /// </summary>
    void SetEquipBgAlphaAnima()
    {
        TweenAlpha effectBg = mEffectBg.gameObject.GetComponent<TweenAlpha>();
        effectBg.ResetToBeginning();
        effectBg.from = 0;
        effectBg.to = 1;
        effectBg.delay = 0.5f;
        effectBg.PlayForward();
    }

    /// <summary>
    /// 还原装备背景动画
    /// </summary>
    void ReturnEquipBg()
    {
        TweenAlpha effectBg = mEffectBg.gameObject.GetComponent<TweenAlpha>();
        effectBg.delay = 0;
        effectBg.PlayReverse();
    }

    /// <summary>
    /// 强化至成功按钮点击事件
    /// </summary>
    void OnClickStrengthenToSuccessBtn(GameObject go)
    {
        if (GuideMgr.IsGuiding())
            return;

        if (!CheckLimit())
            return;

        if (!CheckCanStrengthen())
            return;

        IsGrey = false;

        mIsFastIntensify = true;

        // 打开快速强化界面
        GameObject wnd = WindowMgr.OpenWnd(FastStrengthenWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<FastStrengthenWnd>().Bind(mEquipOb);
    }

    /// <summary>
    /// 强化选项点击事件
    /// </summary>
    void OnClickStrengthenOptions(GameObject go)
    {
        if (GuideMgr.IsGuiding())
            return;
        Debug.Log("OnClickStrengthenOptions");
    }

    /// <summary>
    /// 增幅选项点击事件
    /// </summary>
    void OnClickIncreaseOptions(GameObject go)
    {
        if (GuideMgr.IsGuiding())
            return;
        Debug.Log("OnClickIncreaseOptions");
    }

    /// <summary>
    /// 重置选项点击事件
    /// </summary>
    void OnClickResetOptions(GameObject go)
    {
        if (GuideMgr.IsGuiding())
            return;
        Debug.Log("OnClickResetOptions");
    }

    /// <summary>
    ///  缓存附加属性id
    /// </summary>
    void CacheMinorPropId()
    {
        if (minorProp == null)
            return;

        if (mMinorPropId == null)
            mMinorPropId = new Dictionary<int, int>();

        // 使用前清空列表
        mMinorPropId.Clear();

        foreach (LPCValue item in minorProp.Values)
            mMinorPropId.Add(item.AsArray[0].AsInt, item.AsArray[1].AsInt);
    }

    /// <summary>
    /// 缓存旧的属性
    /// </summary>
    private void CacheOldMinorPropId()
    {
        if (mMinorPropId == null)
            return;

        if (mOldMinorPropId == null)
            mOldMinorPropId = new Dictionary<int, int>();

        mOldMinorPropId.Clear();

        foreach (var item in mMinorPropId)
            mOldMinorPropId.Add(item.Key, item.Value);
    }

    #endregion

    #region 外部接口

    public void Bind(string equipRid)
    {
        mEquipRid = equipRid;

        IsCache = true;

        Redraw();

        RefreshShareBtn();
    }

    /// <summary>
    /// 指引点击强化按钮
    /// </summary>
    public void GuideOnClickStrengthenBtn()
    {
        DoStrengthen();
    }

    #endregion
}
