/// <summary>
/// LotteryBonusWnd.cs
/// Created by lic 11/22/2016
/// 抽奖窗口
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;
using System.Collections.Generic;

public class LotteryBonusWnd : WindowBase<LotteryBonusWnd>
{
    #region 公共字段
    public float scale = 0.25f;

    public GameObject mCloseBtn;
    public UILabel mTitle;
    public UILabel mLimitAmount;
    public UILabel mTimeDesc;
    public UILabel mTime;
    public GameObject mLotteryBtn;
    public UILabel mLotteryDesc;
    public UILabel mLotteryFreeLb;
    public UISprite mLotteryCostIcon;
    public UILabel mLotteryCostLb;
    public UITexture mLotteryCostTexture;

    public TweenAlpha mLotteryBtnTween;

    //播放动画时屏蔽点击操作事件遮罩层
    public GameObject mCover;

    public GameObject mShowBonusWnd;
    public UITexture mBonusIcon;
    public UILabel mBonusDesc;
    public GameObject mBonusDescBg;
    public GameObject mBigBonusBg;
    public GameObject mSmallBonusBg;
    public GameObject mBonusBg;

    //滚条动画结束后的闪屏遮罩
    public GameObject mWhiteFlash;

    public GameObject mWhiteLight;

    public Vector3 mInfoWndPosition = new Vector3(0, 0, 0);

    public GameObject mGoldCoinWnd;

    public GameObject mMoneyWnd;
    #endregion

    #region 私有
    private int mSelectId;

    private Property mLotteryBonusOb = null;

    private GameObject mBonusInfoWnd = null;

    #endregion

    #region 内部函数

    void Start()
    {
        // 注册事件
        RegisterEvent();

        // 初始化窗口
        InitWnd();

        // 刷新奖励
        Redraw();

        // 每天零点刷新一次
        InvokeRepeating("RefreshLimitData", (float) Game.GetZeroClock(1), 86400);
    }

    /// <summary>
    /// 注册窗口事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;
        UIEventListener.Get(mLotteryBtn).onClick = OnLotteryBtn;
        UIEventListener.Get(mBonusBg).onClick = OnBonusBg;

        // 关注抽奖成功事件
        MsgMgr.RegisterDoneHook("MSG_LOTTERY_BONUS", "LotteryBonusWnd", OnMsgLotteryBonus);

        //动画结束
        EventMgr.RegisterEvent("LotteryBonusWnd", EventMgrEventType.EVENT_LOTTERY_BONUS_ANI_DONE, OnAnimDone);

        ME.user.dbase.RemoveTriggerField("LotteryBonusWnd");
        ME.user.dbase.RegisterTriggerField("LotteryBonusWnd", new string[]
            {
                "lottery_bonus",
            }, new CallBack(OnBonusListChanged));

        ME.user.tempdbase.RegisterTriggerField("LotteryBonusWnd", new string[]{ "gapp_world" }, new CallBack(OnGappWorldFieldsChange));

        InvokeRepeating("RedrawTime", 0f, 1f);

        float scale = Game.CalcWndScale();
        transform.localScale = new Vector3(scale, scale, scale);

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.MULTIPLE_OPEN_WND);
        WindowMgr.RemoveOpenWnd(WindowMgr.GetWindow(LotteryBonusSVWnd.WndType), WindowOpenGroup.MULTIPLE_OPEN_WND);
    }

    void OnGappWorldFieldsChange(object para, params object[] param)
    {
        // 刷新限制数据
        RefreshLimitData();
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        mTitle.text = LocalizationMgr.Get("LotteryBonusWnd_1");
        mLotteryDesc.text = LocalizationMgr.Get("LotteryBonusWnd_2");
        mTimeDesc.text = LocalizationMgr.Get("LotteryBonusWnd_3");

        // 刷新限制数据
        RefreshLimitData();
    }

    /// <summary>
    /// 刷新限制数据
    /// </summary>
    void ShowLimitData()
    {
        if (ME.user.QueryTemp<int>("gapp_world") == 1)
        {
            LPCMapping limitData = LPCMapping.Empty;

            LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_lottery_bonus");
            if (v != null && v.IsMapping)
                limitData = v.AsMapping;

            mLimitAmount.text = string.Format(
                LocalizationMgr.Get("LotteryBonusWnd_10"),
                limitData.GetValue<int>("amount"),
                GameSettingMgr.GetSettingInt("max_lottery_bonus")
            );

            mLimitAmount.gameObject.SetActive(true);
        }
        else
        {
            mLimitAmount.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 刷新限制数据
    /// </summary>
    void RefreshLimitData()
    {
        // 开启版署模式累计许愿次数
        if (ME.user.QueryTemp<int>("gapp_world") == 1)
        {
            LPCMapping limitData = LPCMapping.Empty;

            LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_lottery_bonus");
            if (v != null && v.IsMapping)
                limitData = v.AsMapping;

            // 重置数据
            if (!TimeMgr.IsSameDay(TimeMgr.GetServerTime(), limitData.GetValue<int>("lottery_time")))
                OptionMgr.SetLocalOption(ME.user, "limit_lottery_bonus", LPCValue.Create(LPCMapping.Empty));
        }

        // 限制数据
        ShowLimitData();
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    void OnDestroy()
    {
        // 取消消息关注变化
        MsgMgr.RemoveDoneHook("MSG_LOTTERY_BONUS", "LotteryBonusWnd");

        // 取消属性字段关注变化
        if (ME.user != null)
        {
            ME.user.dbase.RemoveTriggerField("LotteryBonusWnd");
            ME.user.tempdbase.RemoveTriggerField("LotteryBonusWnd");
        }

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.MULTIPLE_OPEN_WND);
        WindowMgr.RemoveOpenWnd(WindowMgr.GetWindow(LotteryBonusSVWnd.WndType), WindowOpenGroup.MULTIPLE_OPEN_WND);
        EventMgr.UnregisterEvent("LotteryBonusWnd");

        // 关闭倒计时
        CancelInvoke("RedrawTime");

        CancelInvoke("RefreshLimitData");

        if (mLotteryBonusOb != null)
            mLotteryBonusOb.Destroy();
    }

    /// <summary>
    /// 关闭按钮点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<MaskWnd>().Play();
        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnLotteryMaskCallBack));

        WindowMgr.DestroyWindow(gameObject.name);

        //关闭附属界面
        WindowMgr.DestroyWindow(LotteryBonusSVWnd.WndType);
    }

    /// <summary>
    /// 遮罩云回调
    /// </summary>
    /// <param name="para"></param>
    /// <param name="param"></param>
    void OnLotteryMaskCallBack(object para, object[] param)
    {
        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OnEnterMainCityScene));
    }

    /// <summary>
    /// 打开主城回调
    /// </summary>
    private void OnEnterMainCityScene(object para, object[] param)
    {
        // 打开主窗口
        WindowMgr.OpenMainWnd();

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();
    }

    /// <summary>
    /// 许愿按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnLotteryBtn(GameObject ob)
    {
        // 开启版署模式
        if (ME.user.QueryTemp<int>("gapp_world") == 1)
        {
            LPCMapping limitData = LPCMapping.Empty;

            LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_lottery_bonus");
            if (v != null && v.IsMapping)
                limitData = v.AsMapping;

            // 今日许愿已达到上限次数
            if (limitData.GetValue<int>("amount") >= GameSettingMgr.GetSettingInt("max_lottery_bonus")
                && TimeMgr.IsSameDay(TimeMgr.GetServerTime(), limitData.GetValue<int>("lottery_time")))
            {
                DialogMgr.Notify(LocalizationMgr.Get("LotteryBonusWnd_11"));
                return;
            }
        }

        if (mBonusInfoWnd != null)
            WindowMgr.DestroyWindow(mBonusInfoWnd.name);

        int LotteryTimes = ME.user.Query<int>("lottery_bonus/lottery_times");

        if (LotteryTimes >= 1)
        {
            DoLottery(null, true);
            return;
        }

        // 许愿次数不足
        LPCMapping costMap = CACL_LOTTERY_BONUS_COST.Call();

        string field = FieldsMgr.GetFieldInMapping(costMap);
        int num = costMap.GetValue<int>(field);

        if (ME.user.Query<int>(field) < num)
        {
            DialogMgr.ShowDailog(
                new CallBack(GotoShop, ShopConfig.GOLD_COIN_GROUP),
                string.Format(LocalizationMgr.Get("LotteryBonusWnd_9"), FieldsMgr.GetFieldName(field)),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                this.transform
            );

            return;
        }

        DialogMgr.ShowDailog(
            new CallBack(DoLottery),
            string.Format(LocalizationMgr.Get("LotteryBonusWnd_8"), num, FieldsMgr.GetFieldName(field)),
            string.Empty,
            string.Empty,
            string.Empty,
            true,
            this.transform
        );

    }

    /// <summary>
    /// 前往商店
    /// </summary>
    public void GotoShop(object para, params object[] _params)
    {
        if (! (bool)_params[0])
            return;

        // 前往商店
        GameObject wnd = WindowMgr.OpenWnd(QuickMarketWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;
        wnd.GetComponent<QuickMarketWnd>().Bind(para as string);
    }

    /// <summary>
    /// 开始抽奖
    /// </summary>
    public void DoLottery(object para, params object[] _params)
    {
        if (!(bool)_params[0])
            return;

        // 宠物包裹格子不足，弹出窗口提示
        if (!BaggageMgr.TryStoreToBaggage(ME.user, ContainerConfig.POS_PET_GROUP, 1))
            return;

        // 装备包裹格子已满
        if ((ME.user as Container).baggage.GetFreePosCount(ContainerConfig.POS_ITEM_GROUP) == 0)
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("MailWnd_15"),
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return;
        }

        // 发送抽奖消息
        bool ret = Operation.CmdLotteryBonus.Go();

        if (!ret)
            return;

        // 屏蔽属性变化
        mGoldCoinWnd.GetComponent<DiamondWnd>().UnRegisterField();
        mMoneyWnd.GetComponent<GoldWnd>().UnRegisterField();

        // 打开遮罩
        mCover.SetActive(true);

        // 淡出按钮
        if (mLotteryBtnTween != null)
        {
            mLotteryBtnTween.enabled = true;
            mLotteryBtnTween.from = 1f;
            mLotteryBtnTween.to = 0f;
            mLotteryBtnTween.ResetToBeginning();
            mLotteryBtnTween.PlayForward();
        }
    }

    /// <summary>
    /// 抽奖消息到达
    /// </summary>
    void OnMsgLotteryBonus(string cmd, LPCValue para)
    {
        if (ME.user != null)
        {
            // 开启版署模式累计许愿次数
            if (ME.user.QueryTemp<int>("gapp_world") == 1)
            {
                LPCMapping limitData = LPCMapping.Empty;

                LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_lottery_bonus");
                if (v != null && v.IsMapping)
                    limitData = v.AsMapping;

                if (!TimeMgr.IsSameDay(limitData.GetValue<int>("lottery_time"), TimeMgr.GetServerTime()))
                    limitData = LPCMapping.Empty;

                // 累计次数
                limitData.Add("amount", limitData.GetValue<int>("amount") + 1);

                // 缓存本次抽奖的时间
                limitData.Add("lottery_time", TimeMgr.GetServerTime());

                // 将数据缓存到本地
                OptionMgr.SetLocalOption(ME.user, "limit_lottery_bonus", LPCValue.Create(limitData));
            }

            ShowLimitData();
        }

        LPCMapping args = para.AsMapping;
        mSelectId = args.GetValue<int>("lottery_id");
        LPCMapping bonusMap = args.GetValue<LPCMapping>("lottery_bonus");
        LPCMapping cost_map = args.GetValue<LPCMapping> ("cost_map");

        // 需要先扣除水晶
        if (cost_map != null && cost_map.ContainsKey ("gold_coin"))
            mGoldCoinWnd.GetComponent<DiamondWnd> ().ChangeNumber (-cost_map.GetValue<int>("gold_coin"));

        if (mLotteryBonusOb != null)
        {
            mLotteryBonusOb.Destroy();
            mLotteryBonusOb = null;
        }

        // 如果bonusMap包含classid说明是非属性物品
        if (bonusMap.ContainsKey("class_id"))
        {
            LPCMapping dbase = LPCValue.Duplicate(bonusMap).AsMapping;

            string original_rid = dbase.GetValue<string>("rid");

            dbase.Add("original_rid", original_rid);

            dbase.Add("rid", Rid.New());

            mLotteryBonusOb = PropertyMgr.CreateProperty(dbase);
        }

        Redraw();
    }

    /// <summary>
    /// 动画结束
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="para"></param>
    private void OnAnimDone(int eventId, MixedValue para)
    {
        Coroutine.DispatchService(PlayFlash(), "PlayFlash");
    }

    /// <summary>
    /// 奖励重置刷新事件回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    void OnBonusListChanged(object param, params object[] paramEx)
    {
        Redraw();
    }

    /// <summary>
    /// 播放闪屏
    /// </summary>
    /// <returns>The flash.</returns>
    IEnumerator PlayFlash()
    {
        yield return new WaitForSeconds(1f);

        mWhiteFlash.GetComponent<TweenAlpha>().enabled = true;
        mWhiteFlash.GetComponent<TweenAlpha>().ResetToBeginning();

        Coroutine.DispatchService(ShowLotteryResult(), "ShowLotteryResult");
    }

    /// <summary>
    /// 显示结果
    /// </summary>
    /// <returns>The lottery result.</returns>
    IEnumerator ShowLotteryResult()
    {
        yield return new WaitForSeconds(0.1f);

        // 淡入按钮
        if (mLotteryBtnTween != null)
        {
            mLotteryBtnTween.enabled = true;
            mLotteryBtnTween.from = 0f;
            mLotteryBtnTween.to = 1f;
            mLotteryBtnTween.ResetToBeginning();
            mLotteryBtnTween.PlayForward();
        }

        if (gameObject == null)
            yield break;

        // 重新关注属性变化回调
        mGoldCoinWnd.GetComponent<DiamondWnd>().RestartRegisterField();
        mMoneyWnd.GetComponent<GoldWnd>().RestartRegisterField();

        mShowBonusWnd.SetActive(true);

        CsvRow bonusData = LotteryBonusMgr.GetLotteryBonus(mSelectId);

        string iconName = bonusData.Query<string>("icon");
        mBonusIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/item/{0}.png", iconName));
        mBonusDesc.text = LocalizationMgr.Get(bonusData.Query<string>("desc"));

        mBonusIcon.GetComponent<TweenAlpha>().enabled = true;
        mBonusIcon.GetComponent<TweenAlpha>().ResetToBeginning();
        mBonusDescBg.GetComponent<TweenAlpha>().enabled = true;
        mBonusDescBg.GetComponent<TweenAlpha>().ResetToBeginning();

        mSmallBonusBg.GetComponent<TweenAlpha>().enabled = true;
        mSmallBonusBg.GetComponent<TweenAlpha>().ResetToBeginning();
        mSmallBonusBg.GetComponent<TweenScale>().enabled = true;
        mSmallBonusBg.GetComponent<TweenScale>().ResetToBeginning();

        mBigBonusBg.GetComponent<TweenAlpha>().enabled = true;
        mBigBonusBg.GetComponent<TweenAlpha>().ResetToBeginning();
        mBigBonusBg.GetComponent<TweenScale>().enabled = true;
        mBigBonusBg.GetComponent<TweenScale>().ResetToBeginning();


        mWhiteLight.GetComponent<TweenAlpha>().enabled = true;
        mWhiteLight.GetComponent<TweenAlpha>().ResetToBeginning();
        mWhiteLight.GetComponent<TweenScale>().enabled = true;
        mWhiteLight.GetComponent<TweenScale>().ResetToBeginning();

        Coroutine.DispatchService(ShowInfo(), "ShowInfo");
    }

    /// <summary>
    /// 显示详细结果
    /// </summary>
    /// <returns>The lottery result.</returns>
    IEnumerator ShowInfo()
    {
        yield return new WaitForSeconds(2f);

        if (this == null)
            yield break;

        OnLotteryOver();
    }

    /// <summary>
    /// 奖励窗口被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnBonusBg(GameObject ob)
    {
        // 中断协程
        Coroutine.StopCoroutine("ShowInfo");

        Coroutine.StopCoroutine("PlayFlash");

        Coroutine.StopCoroutine("ShowLotteryResult");

        OnLotteryOver();
    }

    /// <summary>
    /// 显示抽奖详细信息
    /// </summary>
    void OnLotteryOver()
    {
        mShowBonusWnd.SetActive(false);

        // 如果奖励重置了，刷新
        GameObject wnd = WindowMgr.GetWindow(LotteryBonusSVWnd.WndType);

        if (wnd != null)
            wnd.GetComponent<LotteryBonusSVWnd>().RefreshResetBonus();

        // 去除遮罩
        mCover.SetActive(false);

        // 还需要显示奖励详细信息
        if (mLotteryBonusOb != null)
        {
            if (MonsterMgr.IsMonster(mLotteryBonusOb))
            {
                // 打开窗口
                mBonusInfoWnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, gameObject.transform);
                if (mBonusInfoWnd != null)
                {
                    PetSimpleInfoWnd petInfo = mBonusInfoWnd.GetComponent<PetSimpleInfoWnd>();

                    petInfo.Bind(mLotteryBonusOb, false);
                    petInfo.ShowBtn(true);
                }
            }
            else
            {
                // 打开窗口
                mBonusInfoWnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, gameObject.transform);
                if (mBonusInfoWnd != null)
                {

                    RewardItemInfoWnd ItemInfo = mBonusInfoWnd.GetComponent<RewardItemInfoWnd>();

                    if (EquipMgr.IsEquipment(mLotteryBonusOb))
                        ItemInfo.SetEquipData(mLotteryBonusOb);
                    else
                        ItemInfo.SetPropData(mLotteryBonusOb);

                    ItemInfo.SetCallBack(new CallBack(OnDialogCallBack, mLotteryBonusOb));
                }
            }

            if (mBonusInfoWnd != null)
                mBonusInfoWnd.transform.localPosition = mInfoWndPosition;
        }

        Redraw();
    }

    void OnDialogCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        Property ob = para as Property;

        //获取装备的数量;
        int amount = ob.GetAmount();

        if(amount < 1)
            return;

        //构建参数;
        LPCMapping data= new LPCMapping ();

        LPCMapping itemData = new LPCMapping();
        itemData.Add(string.IsNullOrEmpty(ob.Query<string>("original_rid")) ? ob.GetRid() : ob.Query<string>("original_rid"), amount);

        data.Add("rid", itemData);

        //通知服务器出售道具
        Operation.CmdSellItem.Go(data);
    }

    /// <summary>
    /// 刷新界面
    /// </summary>
    void Redraw()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        int LotteryTimes = ME.user.Query<int>("lottery_bonus/lottery_times");

        // 显示许愿次数
        if (LotteryTimes <= 0)
        {
            mLotteryFreeLb.gameObject.SetActive(false);
            mLotteryCostLb.gameObject.SetActive(true);

            LPCMapping costMap = CACL_LOTTERY_BONUS_COST.Call();

            if (costMap.ContainsKey("lottery_card"))
            {
                mLotteryCostIcon.gameObject.SetActive(false);
                mLotteryCostTexture.gameObject.SetActive(true);

                mLotteryCostTexture.mainTexture = ItemMgr.GetTexture(FieldsMgr.GetFieldTexture("lottery_card"));
                mLotteryCostLb.text = string.Format("× {0}", ME.user.Query<int>("lottery_card"));
            }
            else
            {
                mLotteryCostIcon.gameObject.SetActive(true);
                mLotteryCostTexture.gameObject.SetActive(false);

                string field = FieldsMgr.GetFieldInMapping(costMap);
                int num = costMap.GetValue<int>(field);

                mLotteryCostIcon.spriteName = FieldsMgr.GetFieldIcon(field);
                mLotteryCostLb.text = num.ToString();
            }
        }
        else
        {
            mLotteryCostIcon.gameObject.SetActive(false);
            mLotteryCostTexture.gameObject.SetActive(false);
            mLotteryCostLb.gameObject.SetActive(false);
            mLotteryFreeLb.gameObject.SetActive(true);
            mLotteryFreeLb.text = string.Format(LocalizationMgr.Get("LotteryBonusWnd_7"), LotteryTimes);
        }

        // 刷新限制数据
        RefreshLimitData();
    }

    /// <summary>
    /// 刷新下次免费时间
    /// </summary>
    void RedrawTime()
    {
        int countDownTime = Mathf.Max((int)Game.GetZeroClock(1), 0);

        mTime.text = TimeMgr.ConvertTimeToChineseTimer(countDownTime);
    }

    #endregion
}
