/// <summary>
/// MarketWnd.cs
/// Created by fengsc 2017/04/20
/// 商城界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;
using QCCommonSDK;
using QCCommonSDK.Addition;

public class MarketWnd : WindowBase<MarketWnd>
{
    #region 成员变量

    public UIGrid mPageGrid;

    // 功勋商店按钮
    public UIToggle mHonorMarketBtn;
    public UILabel mHonorMarketBtnLb;

    // 钻石商店
    public UIToggle mCrystalMarketBtn;
    public UILabel mCrystalMarketBtnLb;

    // 辅助道具商店
    public UIToggle mSpecialMarketBtn;
    public UILabel mSpecialMarketLb;

    // 礼包
    public UIToggle mPacksMarketBtn;
    public UILabel mPacksMarketBtnLb;

    // 限时商城按钮
    public UIToggle mLimitMarketBtn;
    public UILabel mLimitMarketBtnLb;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 商品格子
    public GameObject mMarketItemWnd;

    // 排序组件
    public UIGrid mGrid;

    public UIScrollView mScrollView;

    public TweenScale mTweenScale;

    public UITexture mIncrease;

    // 增幅提示
    public UILabel mIncreaseTips;

    // 增幅剩余时间
    public UILabel mRemainTime;

    public UIPanel mTipsWnd;

    public UILabel mTips;

    LPCMapping mLimitBuyData = new LPCMapping();

    List<LPCValue> mMarketCsvData = new List<LPCValue>();

    // 缓存商品格子对象
    List<GameObject> mMarketItemList = new List<GameObject>();

    LPCMapping itemData = new LPCMapping();

    // 当前分页
    int mCurPage = MarketMgr.MARKET_PAGE2;

    int mCountDown = 0;

    #endregion

    // Use this for initialization
    void Start ()
    {
        if (ME.user == null)
            return;

        // 版署模式
        if (ME.user.QueryTemp<int>("gapp_world") == 1)
        {
            mLimitMarketBtn.gameObject.SetActive(true);
        }
        else
        {
            mLimitMarketBtn.gameObject.SetActive(false);
        }

        // 获取功勋商店的商品数据
        mMarketCsvData = MarketMgr.GetPageData(ME.user, mCurPage);

        switch (mCurPage)
        {
            // 功勋商店分页
            case MarketMgr.MARKET_PAGE2:

                mHonorMarketBtn.value = true;

                break;

            // 钻石商店分页
            case MarketMgr.MARKET_PAGE3:

                mCrystalMarketBtn.value = true;

                break;

            // 辅助道具
            case MarketMgr.MARKET_PAGE4:

                mSpecialMarketBtn.value = true;

                break;

            // 礼包分页
            case MarketMgr.MARKET_PAGE5:

                mPacksMarketBtn.value = true;

                break;

            default:
                break;
        }

        CreatedGameObject(mMarketCsvData.Count);

        // 刷新界面
        Redraw();

        RedrawIncrease();

        RegisterEvent();

        // 初始化本地化文本
        InitLocalText();

        // 钻石页签显示
        mCrystalMarketBtn.gameObject.SetActive(MarketMgr.GetPageData(ME.user, MarketMgr.MARKET_PAGE3).Count > 0);

        mPageGrid.Reposition();


        // 零点调用，24小时调用一次
        InvokeRepeating("Redraw", (float) Game.GetZeroClock(1), 86400);
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("MarketWnd");

        // 取消调用
        CancelInvoke("Redraw");

        CancelInvoke("IncreaseRefreshTime");

        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("MarketWnd");

        ME.user.tempdbase.RemoveTriggerField("MarketWnd");
    }

    void OnEnable()
    {
#if  !UNITY_EDITOR

        // 注册获取商品列表回调
        SkuSupport.OnSkuResult += SkuHook;

        // 通知sdk尝试获取商品价格
        MarketMgr.CheckSkuPrice();

#endif
    }

    void OnDisable()
    {
#if ! UNITY_EDITOR

        // 取消注册获取商品列表回调
        SkuSupport.OnSkuResult -= SkuHook;

#endif

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mHonorMarketBtn.gameObject).onClick = OnClickHonorMarketBtn;
        UIEventListener.Get(mCrystalMarketBtn.gameObject).onClick = OnClickCrystalMarketBtn;
        UIEventListener.Get(mSpecialMarketBtn.gameObject).onClick = OnClickSpecialMarketBtn;
        UIEventListener.Get(mPacksMarketBtn.gameObject).onClick = OnClickPacksMarketBtn;
        UIEventListener.Get(mLimitMarketBtn.gameObject).onClick = OnClickLimitMarketBtn;
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mIncrease.gameObject).onPress = OnClickIncrease;

        EventMgr.RegisterEvent("MarketWnd", EventMgrEventType.EVENT_BUY_ITEM_SUCCESS, OnBuyItemSuccCallBack);

        if (ME.user != null)
        {
            ME.user.dbase.RegisterTriggerField("MarketWnd", new string[]{"increase_time"}, new CallBack(OnFieldsChange));

            ME.user.tempdbase.RegisterTriggerField("MarketWnd", new string[]{"close_pay"}, new CallBack(OnClosePay));
        }

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    void OnClosePay(object para, params object [] param)
    {
        // 重绘月卡增幅数据
        RedrawIncrease();
    }

    void OnFieldsChange(object para, params object [] param)
    {
        // 重绘月卡增幅数据
        RedrawIncrease();
    }

    /// <summary>
    /// tween动画播放完后回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 商店购买道具成功事件回调
    /// </summary>
    void OnBuyItemSuccCallBack(int eventId, MixedValue para)
    {
        // 获取分页数据
        mMarketCsvData = MarketMgr.GetPageData(ME.user, mCurPage);

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 先创建一批格子
    /// </summary>
    void CreatedGameObject(int count)
    {
        mMarketItemWnd.SetActive(false);

        for (int i = 0; i < count; i++)
        {
            GameObject clone = Instantiate(mMarketItemWnd);

            clone.transform.SetParent(mGrid.transform);

            clone.transform.localScale = Vector3.one;

            clone.transform.localPosition = Vector3.zero;

            // 注册商品的点击事件
            UIEventListener.Get(clone).onClick = OnClickItemBtn;

            clone.SetActive(false);

            mMarketItemList.Add(clone);
        }
    }


    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mScrollView.ResetPosition();

        LPCValue buyData = ME.user.Query<LPCValue>("limit_buy_data");

        if (buyData != null && buyData.IsMapping)
            mLimitBuyData = buyData.AsMapping;
        else
            mLimitBuyData = LPCMapping.Empty;

        if (mMarketCsvData == null)
            return;

        for (int i = 0; i < mMarketCsvData.Count; i++)
        {
            if (i + 1 > mMarketItemList.Count)
            {
                // 初始的格子数不够,创建新的商品格子
                CreatedGameObject(1);
            }

            GameObject item = mMarketItemList[i];
            if (item == null)
                continue;

            LPCMapping csvData = mMarketCsvData[i].AsMapping;

            // 绑定数据
            MarketItemWnd marketItemWnd = item.GetComponent<MarketItemWnd>();

            if (marketItemWnd == null)
                continue;

            item.SetActive(true);

            // 绑定数据
            marketItemWnd.Bind(csvData, mLimitBuyData);
        }

        // 隐藏多余的基础格子
        for (int i = mMarketCsvData.Count; i < mMarketItemList.Count; i++)
            mMarketItemList[i].SetActive(false);

        // 激活排序组件
        mGrid.Reposition();
    }

    /// <summary>
    /// 刷新月卡增幅时间
    /// </summary>
    void RedrawIncrease()
    {
        if (ME.user.QueryTemp<int>("close_pay") == 1)
        {
            if (mIncrease.gameObject.activeSelf)
                mIncrease.gameObject.SetActive(false);

            return;
        }

        if (!mIncrease.gameObject.activeSelf)
            mIncrease.gameObject.SetActive(true);

        LPCMapping giftBag = LPCMapping.Empty;

        // 获取礼包数据
        LPCValue giftBagData = ME.user.Query<LPCValue>("gift_bag_data");
        if (giftBagData != null && giftBagData.IsMapping)
            giftBag = giftBagData.AsMapping;

        bool isValid = false;

        if (giftBag.ContainsKey(ShopConfig.MONTH_CARD_ID))
        {
            LPCMapping receiveData = giftBagData.AsMapping.GetValue<LPCMapping>(ShopConfig.MONTH_CARD_ID);
            if (receiveData != null)
            {
                if (receiveData.GetValue<int>("receive_day") == receiveData.GetValue<int>("valid_day"))
                    isValid = false;
                else
                    isValid = true;
            }
        }
        else
        {
            isValid = false;
        }

        // 月卡增幅到期时间
        int increase_time = 0;

        // 月卡增幅数据
        LPCValue v = ME.user.Query<LPCValue>("increase_time");
        if (v != null && v.IsInt)
            increase_time = v.AsInt;

        if (increase_time == 0 || increase_time <= TimeMgr.GetServerTime())
        {
            // 月卡增幅到期
            mRemainTime.text = string.Format("{0}{1}", 0, LocalizationMgr.Get("MarketWnd_32"));

            float rgb = 124.0f / 255;

            mIncrease.color = new Color(rgb, rgb, rgb, rgb);
        }
        else
        {
            if (isValid)
            {
                mIncrease.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
            else
            {
                float rgb = 124.0f / 255;

                mIncrease.color = new Color(rgb, rgb, rgb, rgb);
            }

            mCountDown = Mathf.Max(0, increase_time - TimeMgr.GetServerTime());
            if (mCountDown <= 0)
                return;

            // 每秒钟调用一次
            InvokeRepeating("IncreaseRefreshTime", 0f, 1.0f);
        }
    }

    void IncreaseRefreshTime()
    {
        if (mCountDown < 0)
        {
            RedrawIncrease();

            CancelInvoke("IncreaseRefreshTime");

            return;
        }

        if (mCountDown < 60)
        {
            // 显示秒
            mRemainTime.text = string.Format("{0}{1}", mCountDown, LocalizationMgr.Get("MarketWnd_35"));
        }
        else if (mCountDown < 3600)
        {
            // 显示分钟
            mRemainTime.text = string.Format("{0}{1}", mCountDown / 60, LocalizationMgr.Get("MarketWnd_34"));
        }
        else if (mCountDown < 86400)
        {
            // 显示小时
            mRemainTime.text = string.Format("{0}{1}", mCountDown / 3600, LocalizationMgr.Get("MarketWnd_33"));
        }
        else
        {
            int day = mCountDown % 86400 == 0 ? mCountDown / 86400 : mCountDown / 86400 + 1;

            // 显示天
            mRemainTime.text = string.Format("{0}{1}", day, LocalizationMgr.Get("MarketWnd_32"));
        }

        mCountDown--;
    }

    /// <summary>
    /// 增幅按钮点击回调
    /// </summary>
    void OnClickIncrease(GameObject go, bool isPress)
    {
        if (isPress)
        {
            mTipsWnd.alpha = 1;
        }
        else
        {
            mTipsWnd.alpha = 0;
        }
    }

    /// <summary>
    /// 道具选择按钮点击事件
    /// </summary>
    void OnClickItemBtn(GameObject go)
    {
        MarketItemWnd item = go.GetComponent<MarketItemWnd>();

        // 获取道具相关的数据;
        itemData = item.mMarketData;

        Property itemOb = item.mMarketItemOb;

        LPCMapping applyArg = itemOb.Query<LPCMapping>("apply_arg");

        LPCMapping dbase = itemOb.Query<LPCMapping>("dbase");
        string attrib = string.Empty;
        if (dbase != null)
        {
            if (dbase.ContainsKey("attrib"))
                attrib = dbase.GetValue<string>("attrib");
        }

        if (!string.IsNullOrEmpty(attrib) && applyArg.ContainsKey(attrib))
        {
            LPCValue v = applyArg.GetValue<LPCValue>(attrib);

            // 当购买回满属性补给品,如果该属性达到上限则无法购买
            if (v.IsString && ME.user.Query<int>("max_" + attrib) <= ME.user.Query<int>(attrib))
            {
                DialogMgr.ShowSimpleSingleBtnDailog(
                    null,
                    string.Format(LocalizationMgr.Get("MarketWnd_16"), FieldsMgr.GetFieldName(attrib)),
                    string.Empty,
                    string.Empty,
                    this.transform
                );
                return;
            }
        }

        if (itemData == null)
            return;

        if (itemData.GetValue<int>("send_mail") == 1)
        {
            int freePos = 0;

            Container container = ME.user as Container;

            int classID = itemData.GetValue<int>("class_id");

            if (MonsterMgr.IsMonster(classID))
            {
                freePos = container.baggage.GetFreePosCount(ContainerConfig.POS_PET_GROUP);
            }
            else if (EquipMgr.IsEquipment(classID))
            {
                freePos = container.baggage.GetFreePosCount(ContainerConfig.POS_ITEM_GROUP);
            }
            else
            {
                // 道具不做处理
                freePos = 1;
            }

            if (freePos <= 0)
            {
                // 包裹空间不足无法购买
                DialogMgr.Notify(LocalizationMgr.Get("MarketWnd_23"));
                return;
            }
        }

        LPCValue clickScript = itemData.GetValue<LPCValue>("click_script");
        if (clickScript == null || clickScript.AsInt == 0)
            return;

        // 执行点击脚本
        ScriptMgr.Call(clickScript.AsInt, itemData, itemOb);
    }


    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocalText()
    {
        mHonorMarketBtnLb.text = LocalizationMgr.Get("MarketWnd_1");
        mCrystalMarketBtnLb.text = LocalizationMgr.Get("MarketWnd_2");
        mSpecialMarketLb.text = LocalizationMgr.Get("MarketWnd_3");
        mPacksMarketBtnLb.text = LocalizationMgr.Get("MarketWnd_4");
        mLimitMarketBtnLb.text = LocalizationMgr.Get("MarketWnd_24");
        mIncreaseTips.text = LocalizationMgr.Get("MarketWnd_31");
        mTips.text = LocalizationMgr.Get("MarketWnd_36");
    }

    /// <summary>
    /// 功勋商店按钮点击事件回调
    /// </summary>
    void OnClickHonorMarketBtn(GameObject go)
    {
        if (mCurPage == MarketMgr.MARKET_PAGE2)
            return;

        mCurPage = MarketMgr.MARKET_PAGE2;

        // 获取分页数据
        mMarketCsvData = MarketMgr.GetPageData(ME.user, mCurPage);

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 水晶商店按钮点击事件回调
    /// </summary>
    void OnClickCrystalMarketBtn(GameObject go)
    {
        if (mCurPage == MarketMgr.MARKET_PAGE3)
            return;

        mCurPage = MarketMgr.MARKET_PAGE3;

        // 获取分页数据
        mMarketCsvData = MarketMgr.GetPageData(ME.user, mCurPage);

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 特殊道具商店按钮点击事件
    /// </summary>
    void OnClickSpecialMarketBtn(GameObject go)
    {
        if (mCurPage == MarketMgr.MARKET_PAGE4)
            return;

        mCurPage = MarketMgr.MARKET_PAGE4;

        // 获取分页数据
        mMarketCsvData = MarketMgr.GetPageData(ME.user, mCurPage);

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 礼包商店按钮点击事件
    /// </summary>
    void OnClickPacksMarketBtn(GameObject go)
    {
        if (mCurPage == MarketMgr.MARKET_PAGE5)
            return;

        mCurPage = MarketMgr.MARKET_PAGE5;

        // 获取分页数据
        mMarketCsvData = MarketMgr.GetPageData(ME.user, mCurPage);

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 礼包商店按钮点击事件
    /// </summary>
    void OnClickLimitMarketBtn(GameObject go)
    {
        if (mCurPage == MarketMgr.MARKET_PAGE6)
            return;

        mCurPage = MarketMgr.MARKET_PAGE6;

        // 获取分页数据
        mMarketCsvData = MarketMgr.GetLimitMarketPageData(ME.user, mCurPage);

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 窗口关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 打开主城界面
        WindowMgr.OpenWnd(MainWnd.WndType);

        // 关闭当前窗口
        WindowMgr.DestroyWindow(this.gameObject.name);
    }

    /// <summary>
    /// 获取sku消息的回调
    /// </summary>
    private void SkuHook(QCEventResult result)
    {
        Redraw();
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int page)
    {
        // 绑定分页数据
        mCurPage = page;
    }
}
