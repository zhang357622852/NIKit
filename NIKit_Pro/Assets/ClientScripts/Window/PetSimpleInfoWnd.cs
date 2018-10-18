/// <summary>
/// PetSimpleInfoWnd.cs
/// Created by fengsc 2016/10/31
/// 简单宠物信息显示界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class PetSimpleInfoWnd : WindowBase<PetSimpleInfoWnd>
{
    #region 成员变量

    /// <summary>
    ///宠物元素图片
    /// </summary>
    public UISprite mElement;

    /// <summary>
    ///宠物名字
    /// </summary>
    public UILabel mPetName;

    /// <summary>
    ///宠物星级
    /// </summary>
    public GameObject[] mStars;

    /// <summary>
    ///宠物技能
    /// </summary>
    public GameObject[] mSkills;

    /// <summary>
    ///关闭按钮
    /// </summary>
    public GameObject mCloseBtn;

    /// <summary>
    ///玩家评价按钮
    /// </summary>
    public GameObject mEvaluateBtn;
    public UILabel mEvaluateLabel;

    /// <summary>
    ///合成信息按钮
    /// </summary>
    public GameObject mCompoundBtn;
    public UILabel mCompoundLabel;

    /// <summary>
    ///宠物种族
    /// </summary>
    public UILabel mRace;

    /// <summary>
    ///宠物类型
    /// </summary>
    public UILabel mType;

    /// <summary>
    ///宠物最大等级
    /// </summary>
    public UILabel mMaxLevel;
    public UILabel mMaxLevelTips;

    /// <summary>
    ///宠物体力值
    /// </summary>
    public UILabel mPower;
    public UILabel mPowerTips;

    /// <summary>
    ///宠物攻击力
    /// </summary>
    public UILabel mAttack;
    public UILabel mAttackTips;

    /// <summary>
    ///宠物防御力
    /// </summary>
    public UILabel mDefence;
    public UILabel mDefenceTips;

    /// <summary>
    ///宠物攻击速度
    /// </summary>
    public UILabel mATKSpeed;
    public UILabel mATKSpeedTips;

    // 购买按钮
    public GameObject mBuyBtn;
    public UILabel mBuyBtnLb;

    //分享按钮
    public GameObject mShareBtn;

    // 购买消耗
    public UILabel mCost;

    // 货币图标
    public UISprite mCoinIcon;

    // 确认窗口
    public GameObject mConfirmBtn;
    public UILabel mConfirmLb;

    public PetItemWnd mItenWnd;

    public UILabel mEvolveChangeTips;

    /// <summary>
    ///技能悬浮窗口
    /// </summary>
    public GameObject mSkillView;

    public GameObject mRichTextContent;

    public GameObject mScrowView;

    public UILabel mCanNotAwakeLb;

    public UISprite mMask;

    public TweenAlpha mTweenAlpha;

    public TweenScale mTweenScale;

    Property petData = null;

    CallBack mCallBack;

    CallBack mShareCallback;

    int mBuyPrice = 0;

    bool isExWnd = false;

    // 是否是购买的
    bool mIsBuy = false;

    bool mIsMarket = false;

    // 初始动画是否播放完毕（如果没有播放完毕，界面上的按钮是不能点击的）
    bool mIsTweenOver = false;

    /// <summary>
    /// The m cooke.
    /// </summary>
    string mCooke = string.Empty;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 初始动画是否播放完毕（如果没有播放完毕，界面上的按钮是不能点击的）
       mIsTweenOver = false;

        // 初始化文本
        InitLabel();

        RegisterEvent();

        if(mTweenAlpha == null || mTweenScale == null)
            return;

        // 播放动画
        mTweenAlpha.PlayForward();

        mTweenScale.PlayForward();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void OnDestroy()
    {
        EventMgr.UnregisterEvent("PetSimpleInfoWnd");
    }

    /// <summary>
    ///刷新宠物信息
    /// </summary>
    void RedrawPetInfo(Property ob)
    {
        //没有宠物数据
        if (ob == null)
            return;

        // 非合成材料不显示合成按钮
//        mCompoundBtn.SetActive(MonsterMgr.IsSyntheMaterial(ob.GetClassID()));

        LPCMapping cost = LPCMapping.Empty;
        if (mIsBuy)
            cost = PropertyMgr.GetBuyPrice(ob, mIsMarket);
        else
            cost = PropertyMgr.GetSellPrice(ob);

        if (cost != null && cost.Count > 0)
        {
            string fields = FieldsMgr.GetFieldInMapping(cost);

            mBuyPrice = cost.GetValue<int>(fields);
            mCost.text = Game.SetMoneyShowFormat(mBuyPrice);
            mCoinIcon.spriteName = FieldsMgr.GetFieldIcon(fields);
        }

        int classId = ob.GetClassID();

        //获取宠物种族;
        mRace.text = MonsterConst.MonsterRaceTypeMap[MonsterMgr.GetRace(classId)];

        //获取宠物类型;
        mType.text = MonsterConst.MonsterStyleTypeMap[MonsterMgr.GetType(classId)];

        //获取宠物当前星级的最大等级;
        mMaxLevel.text = MonsterMgr.GetMaxLevel(ob).ToString();

        //获取宠物的名字;
        mPetName.text = string.Format("{0}{1}[-]", PetMgr.GetAwakeColor(ob.GetRank()), ob.Short());

        //获取宠物的攻击力
        mAttack.text = ob.QueryAttrib("attack").ToString();

        //获取宠物体力
        mPower.text = ob.QueryAttrib("max_hp").ToString();

        //获取宠物防御力;
        mDefence.text = ob.QueryAttrib("defense").ToString();

        //获取宠物攻击速度;
        mATKSpeed.text = ob.QueryAttrib("agility").ToString();

        //获取宠物元素图标;
        mElement.spriteName = PetMgr.GetElementIconName(MonsterMgr.GetElement(classId));

        // 获取星级
        int star = ob.GetStar();
        int count = star < mStars.Length ? star : mStars.Length;
        string IconName = PetMgr.GetStarName(ob.GetRank());

        for (int i = 0; i < mStars.Length; i++)
            mStars[i].gameObject.SetActive(false);

        for (int i = 0; i < count; i++)
        {
            mStars[i].GetComponent<UISprite>().spriteName = IconName;
            mStars[i].gameObject.SetActive(true);
        }

        if(MonsterMgr.IsCanAwaken(ob.GetClassID()))
        {
            mScrowView.SetActive(true);
            mCanNotAwakeLb.gameObject.SetActive(false);

            string desc = MonsterMgr.GetEvolutionDesc(ob);

            RichTextContent content = mRichTextContent.GetComponent<RichTextContent>();

            if (content == null)
                return;

            // 清空组件的内容
            content.clearContent();

            content.ParseValue(desc);
        }
        else
        {
            mScrowView.SetActive(false);
            mCanNotAwakeLb.gameObject.SetActive(true);
        }
    }

    /// <summary>
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mEvaluateBtn).onClick = OnClickEvaluateBtn;
        UIEventListener.Get(mCompoundBtn).onClick = OnClickCompundBtn;
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mConfirmBtn).onClick = OnClickConfirmBtn;
        if (mShareBtn != null)
            UIEventListener.Get(mShareBtn).onClick = OnClickShareBtn;
        UIEventListener.Get(mMask.gameObject).onClick = OnClickCloseBtn;

        //注册进化图标点击事件;
        EventMgr.RegisterEvent("PetSimpleInfoWnd", EventMgrEventType.EVENT_CLICK_PICTURE, WhenShowHover);

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);

        // 注册购买成功事件
        EventMgr.RegisterEvent("PetSimpleInfoWnd", EventMgrEventType.EVENT_BUY_ITEM_SUCCESS, OnBuyItemSuccCallBack);
    }

    /// <summary>
    /// 道具购买成功回调
    /// </summary>
    void OnBuyItemSuccCallBack(int eventId, MixedValue para)
    {
        if (ME.user == null)
            return;

        LPCMapping itemData = para.GetValue<LPCMapping>().GetValue<LPCMapping>("item_data");

        int classId = itemData.GetValue<int>("class_id");

        CsvRow data = MarketMgr.GetMarketConfig(classId);
        if (data == null)
            return;

        int showDialog = data.Query<int>("show_dialog");

        if (showDialog != 1)
        {
            // 关闭当前窗口
            if(this != null)
                WindowMgr.DestroyWindow(gameObject.name);

            DialogMgr.Notify(LocalizationMgr.Get(data.Query<string>("buy_tips")));
        }
        else
        {
            DialogMgr.ShowSimpleSingleBtnDailog(
                new CallBack(OnDialogCallBack),
                LocalizationMgr.Get(data.Query<string>("buy_tips")),
                LocalizationMgr.Get("MarketWnd_14"),
                string.Empty
            );
        }
    }

    void OnDialogCallBack(object para, params object[] param)
    {
        if (this == null)
            return;

        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// tween播放回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        mIsTweenOver = true;
    }

    /// <summary>
    /// 显示悬浮
    /// </summary>
    void WhenShowHover(int eventId, MixedValue para)
    {
        List<object> data = para.GetValue<List<object>>();

        if (data == null || data.Count < 1)
            return;

        bool isPress = (bool)data[0];

        int skillId = (int)data[1];

        Vector3 iconPos = (Vector3)data[2];

        if (mSkillView == null)
            return;

        SkillViewWnd script = mSkillView.GetComponent<SkillViewWnd>();
        if (script == null)
            return;

        //鼠标按下
        if (isPress)
        {
            // 显示悬浮窗口
            script.ShowView(skillId, petData, true);

            mSkillView.transform.position = iconPos;

            // 限制悬浮在屏幕范围内
            script.LimitPosInScreen();
        }
        else
        {
            // 隐藏悬浮窗口
            script.HideView();
        }
    }

    /// <summary>
    ///玩家评价按钮点击事件回调
    /// </summary>
    void OnClickEvaluateBtn(GameObject go)
    {
        // 动画还没有结束，不响应
        if (!mIsTweenOver)
            return;

        // 打开玩家评价窗口
        GameObject wnd = WindowMgr.OpenWnd(AppraiseWnd.WndType);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<AppraiseWnd>().Bind(petData.GetClassID(), petData.GetRank(), petData.GetStar());
    }

    /// <summary>
    ///合成信息按钮点击事件
    /// </summary>
    void OnClickCompundBtn(GameObject go)
    {
        // 动画还没有结束，不响应
        if (!mIsTweenOver)
            return;

        GameObject wnd = WindowMgr.OpenWnd ("PetSynthesisViewWnd");

        wnd.GetComponent<PetSynthesisViewWnd> ().BindData (petData.GetClassID());

        if (isExWnd && this != null)
            WindowMgr.DestroyWindow (gameObject.name);
    }

    /// <summary>
    ///关闭按钮点击事件回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 动画还没有结束，不响应
        if(!mIsTweenOver)
            return;

        if (this != null)
            WindowMgr.DestroyWindow(gameObject.name);

        if (mCallBack != null)
            mCallBack.Go(false, mCooke, PetSimpleInfoWnd.WndType);
    }

    /// <summary>
    /// 购买按钮点击事件
    /// </summary>
    void OnClickBuyBtn(GameObject go)
    {
        // 动画还没有结束，不响应
        if (!mIsTweenOver)
            return;

        if (mCallBack != null)
            mCallBack.Go(true, mCooke, PetSimpleInfoWnd.WndType);
    }

    /// <summary>
    /// 分享按钮点击事件
    /// </summary>
    /// <param name="go"></param>
    private void OnClickShareBtn(GameObject go)
    {
        if (mShareCallback != null)
            mShareCallback.Go();
    }

    /// <summary>
    /// 出售按钮点击事件
    /// </summary>
    void OnClickSellBtn(GameObject go)
    {
        // 动画还没有结束，不响应
        if (!mIsTweenOver)
            return;
    }

    /// <summary>
    /// 确认按钮点击事件
    /// </summary>
    void OnClickConfirmBtn(GameObject go)
    {
        // 动画还没有结束，不响应
        if (!mIsTweenOver)
            return;

        if (mCallBack != null)
            mCallBack.Go(false, mCooke, PetSimpleInfoWnd.WndType);

        if (this == null)
            return;

        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    ///初始化本地化文本
    /// </summary>
    void InitLabel()
    {
        mEvaluateLabel.text = LocalizationMgr.Get("RewardPetInfoWnd_1");
        mCompoundLabel.text = LocalizationMgr.Get("RewardPetInfoWnd_2");
        mMaxLevelTips.text = LocalizationMgr.Get("RewardPetInfoWnd_5");
        mPowerTips.text = LocalizationMgr.Get("RewardPetInfoWnd_6");
        mAttackTips.text = LocalizationMgr.Get("RewardPetInfoWnd_7");
        mDefenceTips.text = LocalizationMgr.Get("RewardPetInfoWnd_8");
        mATKSpeedTips.text = LocalizationMgr.Get("RewardPetInfoWnd_9");
        mEvolveChangeTips.text = LocalizationMgr.Get("RewardPetInfoWnd_10");

        mCanNotAwakeLb.text = LocalizationMgr.Get("AwakeWnd_3");

    }

    /// <summary>
    ///初始化技能
    /// </summary>
    void RedrawSkill()
    {
        for (int i = 0; i < mSkills.Length; i++)
        {
            mSkills[i].GetComponent<SkillItem>().SetBind(-1);
            mSkills[i].GetComponent<SkillItem>().SetSelected(false);

            mSkills[i].SetActive(true);
        }

        // 获取绑定宠物的技能
        LPCArray skillInfo = petData.GetAllSkills();

        // 遍历技能列表
        foreach (LPCValue mks in skillInfo.Values)
        {
            // 获取技能类型
            int skillId = mks.AsArray[0].AsInt;
            int type = SkillMgr.GetSkillPosType(skillId);

            if (type <= 0 || type > mSkills.Length)
                continue;

            SkillItem item = mSkills[type - 1].GetComponent<SkillItem>();

            //获取技能等级;
            int level = petData.GetSkillLevel(skillId);

            item.SetBind(skillId);

            if (!SkillMgr.IsLeaderSkill(skillId))
                item.SetMaxLevel(level);
            else
                item.SetLeader(true);

            item.SetSelected(false);

            //添加点击事件;
            UIEventListener.Get(mSkills[type - 1]).onPress = ClickShowHoverWnd;
        }
    }

    /// <summary>
    ///按下显示悬浮窗口
    /// </summary>
    void ClickShowHoverWnd(GameObject go, bool isPress)
    {
        SkillItem data = go.GetComponent<SkillItem>();
        if (data == null)
            return;

        if (mSkillView == null)
            return;

        SkillViewWnd script = mSkillView.GetComponent<SkillViewWnd>();
        if (script == null)
            return;

        //按下
        if (isPress)
        {
            if (data.mSkillId <= 0)
                return;

            data.SetSelected(true);

            // 显示悬浮窗口
            script.ShowView(data.mSkillId, petData);

            BoxCollider box = go.GetComponent<BoxCollider>();

            Vector3 boxPos= box.transform.localPosition;

            mSkillView.transform.localPosition = new Vector3 (boxPos.x, boxPos.y + box.size.y / 2, boxPos.z);

            // 限制悬浮在窗口范围内
            script.LimitPosInScreen();
        }
        else
        {
            data.SetSelected(false);

            // 隐藏悬浮窗口
            script.HideView();
        }
    }

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="args">Arguments.</param>
    public void ShowWindow(Dictionary<string, object> args)
    {
        // 检查数据规范
        if (!args.ContainsKey("data") || !args.ContainsKey("is_single"))
            return;

        Bind((Property)args["data"], (args.ContainsKey("is_showMask") ? (bool)args["is_showMask"] :true),
            (args.ContainsKey("is_exWnd") ? (bool)args["is_exWnd"] : false), (args.ContainsKey("is_clickMask") ? (bool)args["is_clickMask"] : true));

        ShowBtn((bool)args["is_single"], (args.ContainsKey("is_buy") ? (bool)args["is_buy"] : true),
            (args.ContainsKey("is_market") ? (bool)args["is_market"] : false), (args.ContainsKey("cancel_text") ? (string)args["cancel_text"] : ""));

        // 获取打开窗口的cookie
        if (args.ContainsKey("cookie"))
            mCooke = (string) args["cookie"];

        if (args.ContainsKey("call_back"))
            SetCallBack((CallBack)args["call_back"]);
    }

    /// <summary>
    ///绑定数据
    /// </summary>
    public void Bind(Property data, bool isShowMask = true, bool _isExWnd = false, bool isClickMask = true)
    {
        petData = data;

        this.isExWnd = _isExWnd;

        RedrawPetInfo(petData);

        mItenWnd.SetBind(data);

        // 判断怪物是否可以评论
        // 不能显示在图鉴中就不能显示参与评论
        mEvaluateBtn.SetActive((petData.Query<int>("show_in_manual") == 1));

        //刷新宠物技能;
        RedrawSkill();

        if (!isClickMask)
        {
            mMask.gameObject.SetActive(false);
            return;
        }

        // 部分需求不需要mask，但是保留boxcollider
        mMask.alpha = isShowMask ? 0.5f : 0.01f;
    }

    /// <summary>
    /// 显示按钮
    /// </summary>
    public void ShowBtn(bool isSingle, bool isBuy = true, bool isMarket = false, string cancelText = "")
    {
        mIsMarket = isMarket;
        mIsBuy = isBuy;
        if (mShareBtn != null)
            mShareBtn.SetActive(false);
        if (isSingle)
        {
            mConfirmBtn.transform.localPosition = new Vector3(0,
                mConfirmBtn.transform.localPosition.y,
                mConfirmBtn.transform.localPosition.z);

            mBuyBtn.SetActive(false);
        }
        else
        {
            if (isBuy)
            {
                // 购买
                UIEventListener.Get(mBuyBtn).onClick = OnClickBuyBtn;

                mBuyBtnLb.text = LocalizationMgr.Get("RewardPetInfoWnd_13");

                mBuyBtn.SetActive(true);
            }
            else
            {
                // 出售
                UIEventListener.Get(mBuyBtn).onClick = OnClickSellBtn;

                mBuyBtnLb.text = LocalizationMgr.Get("RewardPetInfoWnd_14");

                mBuyBtn.SetActive(true);
            }
        }

        if (! string.IsNullOrEmpty(cancelText))
            mConfirmLb.text = cancelText;
        else
            mConfirmLb.text = LocalizationMgr.Get("RewardPetInfoWnd_12");

    }

    /// <summary>
    /// 分享按钮
    /// </summary>
    public void ShowShareBtn()
    {
        mBuyBtn.SetActive(false);

        mShareBtn.SetActive(true);
        mShareBtn.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("RewardPetInfoWnd_16");

        mConfirmBtn.SetActive(true);
        mConfirmLb.text = LocalizationMgr.Get("RewardPetInfoWnd_12");
    }

    public void SetCallBack(CallBack callBack)
    {
        mCallBack = callBack;
    }

    /// <summary>
    /// 绑定点击分享按钮回调
    /// </summary>
    public void SetShareCallBack(CallBack callBack)
    {
        mShareCallback = callBack;
    }

    #endregion
}
