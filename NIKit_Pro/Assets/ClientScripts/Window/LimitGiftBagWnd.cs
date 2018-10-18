/// <summary>
/// LimitGiftBagWnd.cs
/// Created by fengsc 2017/12/23
/// 限时礼包界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class LimitGiftBagWnd : WindowBase<LimitGiftBagWnd>
{
    #region 成员变量

    // 礼包标题
    public UILabel mTitle;

    // 限购提示
    public UILabel[] mLimitBuyTips;
    public UILabel mLimitBuy;

    public UILabel[] mTips;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 购买立即获得物品列表
    public GameObject[] mGoodsList;

    // 购买按钮
    public UISprite mBuyBtn;

    public UILabel mSkuCost;

    // 购买消耗
    public UILabel mCost;

    // 消耗品图标
    public UISprite mCostIcon;

    public UITexture mGiftBagIcon;

    // 下拉框
    public UIPopupListEx mPopupList;

    public UISprite mTipsIcon;

    // 礼包背景光效
    public GameObject mBgEffect;

    public GameObject mSelect;

    // 限购倒计时显示体
    public UILabel mLimitTimer;

    // 道具class_id
    private int mItemClassId;

    // 商城数据
    private LPCMapping mMarketData = LPCMapping.Empty;

    private CsvRow mGiftBagContent = null;

    Dictionary<string, int> mDic = new Dictionary<string, int>();

    bool mIsSelect = false;

    // 限购倒计时
    private int mRemainTime = 0;

    private bool mEnableCountDown = false;

    private float mLastTime = 0f;

    private string mLimitTimerStr = string.Empty;

    #endregion

    void Start()
    {
        // 注册事件
        RegisterEvent();

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        // 计算当前时间到晚上零点的时间, 尝试刷新按钮的状态
        Invoke("SetBuyButtonState", (int)Game.GetZeroClock(1));
    }

    void Update()
    {
        if (mBgEffect != null)
            mBgEffect.transform.Rotate(Vector3.forward * Time.unscaledDeltaTime * 40);

        if (mEnableCountDown)
        {
            if (Time.realtimeSinceStartup > mLastTime + 1.0f)
            {
                mLastTime = Time.realtimeSinceStartup;

                // 倒计时
                CountDown();
            }
        }
    }

    void OnDestroy()
    {
        // 解注册事件
        UIPopupListEx.onValueChange -= OnValueChange;

        EventMgr.UnregisterEvent("LimitGiftBagWnd");

        // 取消调用
        CancelInvoke("SetBuyButtonState");
    }

    /// <summary>
    /// 初始化文本
    /// </summary>
    void InitLabel()
    {
        if (mGiftBagContent == null)
            return;

        LPCArray limitDesc = mGiftBagContent.Query<LPCArray>("limit_desc");

        for (int i = 0; i < mLimitBuyTips.Length; i++)
        {
            if (i + 1 > limitDesc.Count)
                continue;

            mLimitBuyTips[i].text = LocalizationMgr.Get(limitDesc[i].AsString);
        }

        LPCArray tips = mGiftBagContent.Query<LPCArray>("tips");

        for (int i = 0; i < mTips.Length; i++)
        {
            if (i + 1 > tips.Count)
                continue;

            mTips[i].text = LocalizationMgr.Get(tips[i].AsString);
        }
        mTitle.text = LocalizationMgr.Get(mGiftBagContent.Query<string>("title"));

        mLimitTimerStr = LocalizationMgr.Get("LimitGiftBagWnd_3");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        UIEventListener.Get(mBuyBtn.gameObject).onClick += OnClickBuyBtn;

        EventMgr.RegisterEvent("LimitGiftBagWnd", EventMgrEventType.EVENT_BUY_ITEM_SUCCESS, OnBuyItemSuccCallBack);

        UIPopupListEx.onValueChange += OnValueChange;
    }

    void OnValueChange()
    {
        mPopupList.SetSelectValuePos(new Vector3(-60.6f, -0.3f, 0));
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
            if (this != null)
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
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 道具的配置表数据
        CsvRow itemConfig = ItemMgr.GetRow(mItemClassId);
        if (itemConfig == null)
            return;

        for (int i = 0; i < mGoodsList.Length; i++)
            mGoodsList[i].SetActive(false);

        mMarketData = MarketMgr.GetMarketConfig(mItemClassId).ConvertLpcMap();

        LPCMapping dbase = mMarketData.GetValue<LPCMapping>("dbase");

        int type = dbase.GetValue<int>("type");

        if (type != ShopConfig.NORMAL_EXP_PET_TYPE && type != ShopConfig.SPECIAL_EXP_PET_TYPE)
        {
            mIsSelect = false;

            mSelect.SetActive(mIsSelect);
        }
        else
        {
            mIsSelect = true;

            mSelect.SetActive(mIsSelect);

            // 获取当前类型的列表
            List<int> list = MarketMgr.GetTypeList(type);

            int amount = 0;

            foreach (int classId in list)
            {
                CsvRow row = ItemMgr.GetRow(classId);
                if (row == null)
                    continue;

                // 道具作用参数
                LPCMapping arg = row.Query<LPCMapping>("apply_arg");
                if (arg == null)
                    continue;

                LPCArray selectList = arg.GetValue<LPCArray>("select_list");
                if (selectList == null)
                    continue;

                for (int i = 0; i < selectList.Count; i++)
                {
                    if (selectList[i] == null || !selectList[i].IsMapping)
                        continue;

                    LPCMapping data = selectList[i].AsMapping;

                    int id = data.GetValue<int>("class_id");

                    string desc = string.Empty;

                    amount = data.GetValue<int>("amount");

                    if (MonsterMgr.IsMonster(id))
                    {
                        int rank = data.GetValue<int>("rank");

                        desc = string.Format("<split>img={0},25,25</split>  {1} × {2}",
                            string.Format("Assets/Art/UI/Icon/monster/{0}.png", MonsterMgr.GetIcon(id, rank)), MonsterMgr.GetName(id, rank), amount);
                    }
                    else if (ItemMgr.IsItem(id))
                    {
                        desc = string.Format("<split>img={0},25,25</split>  {1} × {2}",
                            string.Format("Assets/Art/UI/Icon/item/{0}.png", ItemMgr.GetIcon(id)), ItemMgr.GetName(id), amount);
                    }

                    mPopupList.AddItem(desc);

                    mDic.Add(desc, classId);
                }

                mPopupList.value = LocalizationMgr.Get("LimitGiftBagWnd_1");

                mPopupList.SetSelectValuePos(new Vector3(34f, -0.3f, 0));
            }
        }

        mGiftBagContent = MarketMgr.GetGiftContentConfig(mItemClassId);

        if (mGiftBagContent != null)
        {
            mGiftBagIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/item/{0}.png", mGiftBagContent.Query<string>("icon")));
        }

//        int receiveDay = 1;
//
//        // 礼包数据
//        LPCValue giftBagData = ME.user.Query<LPCValue>("gift_bag_data");
//
//        if (giftBagData != null && giftBagData.IsMapping && giftBagData.AsMapping.Count > 0)
//        {
//            LPCArray list = giftBagData.AsMapping.GetValue<LPCArray>(mItemOb.GetClassID());
//            if (list != null && list.Count > 0)
//            {
//                LPCMapping receiveData = list[0].AsMapping;
//
//                if (receiveData != null && receiveData.ContainsKey("receive_day"))
//                    receiveDay = receiveData.GetValue<int>("receive_day");
//            }
//        }

        // 道具作用参数
        LPCMapping applyArg = itemConfig.Query<LPCMapping>("apply_arg");
        if (applyArg == null)
            return;

        LPCArray goodsList = LPCArray.Empty;

        if (applyArg.ContainsKey("goods_list"))
            goodsList.Append(applyArg.GetValue<LPCArray>("goods_list"));

        if (! mIsSelect && applyArg.ContainsKey("pet_list"))
            goodsList.Append(applyArg.GetValue<LPCArray>("pet_list"));

        for (int i = 0; i < goodsList.Count; i++)
        {
            if (goodsList[i] == null || !goodsList[i].IsMapping)
                continue;

            if (i > mGoodsList.Length)
                break;

            GameObject item = mGoodsList[i];
            if (item == null)
                continue;

            item.SetActive(true);

            GoodsItemWnd script = item.GetComponent<GoodsItemWnd>();
            if (script == null)
                continue;

            script.Bind(goodsList[i].AsMapping);
        }

        if(string.IsNullOrEmpty(mMarketData.GetValue<string>("purchase_id")))
        {
            LPCMapping buyPrice = mMarketData.GetValue<LPCMapping>("buy_price");
            if (buyPrice == null)
                return;

            string costFields = FieldsMgr.GetFieldInMapping(buyPrice);

            mCost.text = buyPrice.GetValue<int>(costFields).ToString();

            mCostIcon.spriteName = FieldsMgr.GetFieldIcon(costFields);
        }
        else
        {
            mCost.gameObject.SetActive(false);

            mSkuCost.gameObject.SetActive(true);

            mSkuCost.text = MarketMgr.GetSkuPrice(mMarketData.GetValue<int>("class_id"));
        }

        LPCMapping buyArgs = mMarketData.GetValue<LPCMapping>("buy_args");
        if (buyArgs == null || buyArgs.Count == 0)
        {
            for (int i = 0; i < mLimitBuyTips.Length; i++)
                mLimitBuyTips[i].gameObject.SetActive(false);

            mLimitBuy.gameObject.SetActive(false);

            return;
        }

        mLimitBuy.text = buyArgs.GetValue<int>("amount_limit").ToString();

        string group = mMarketData["group"].AsString;

        // 元素之强化限时礼包
        if (group == ShopConfig.INTENSIFY_GIFT_GROUP)
        {
            LPCArray gift = MarketMgr.GetLimitStrengthList(ME.user);

            if (gift == null || gift.Count == 0)
                return;

            //开始时间
            int startTime = 0;

            for (int i = 0; i < gift.Count; i++)
            {
                if (gift[i].AsMapping["class_id"].AsInt == mItemClassId)
                {
                    startTime = gift[i].AsMapping["start_time"].AsInt;
                    break;
                }
            }

            // 结束时长
            int endTime = buyArgs.GetValue<int>("valid_time");

            // 限购所剩时间
            mRemainTime = endTime - (TimeMgr.GetServerTime() - startTime) - 1;

            mEnableCountDown = true;

            mLimitTimer.gameObject.SetActive(true);

            mLimitBuy.gameObject.SetActive(false);

            for (int i = 0; i < mLimitBuyTips.Length; i++)
            {
                mLimitBuyTips[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 设置购买按钮状态
    /// </summary>
    void SetBuyButtonState()
    {
        float rgb = 0;

        // 调用脚本判断能否购买道具
        object result = MarketMgr.IsBuy(ME.user, mMarketData.GetValue<int>("class_id"));

        // 无法购买
        if (result is string)
        {
            rgb = 124f / 255f;

            // 购买按钮置灰
            mBuyBtn.color = new Color(rgb, rgb, rgb);
            mCostIcon.color = new Color(rgb, rgb, rgb);
            mCost.color = new Color(rgb, rgb, rgb);
        }
        else
        {
            rgb = 255f / 255f;

            mBuyBtn.color = new Color(rgb, rgb, rgb);
            mCostIcon.color = new Color(rgb, rgb, rgb);
            mCost.color = new Color(rgb, rgb, rgb);
        }
    }

    /// <summary>
    /// 关闭按钮点击事件回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        if (this == null)
            return;

        // 销毁当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 购买按钮点击事件回调
    /// </summary>
    void OnClickBuyBtn(GameObject go)
    {
        int selectId = 0;

        if (mIsSelect)
        {
            if(! string.IsNullOrEmpty(mPopupList.value))
                mDic.TryGetValue(mPopupList.value, out selectId);

            if (selectId != 0)
                mMarketData = MarketMgr.GetMarketConfig(selectId).ConvertLpcMap();
        }

        // 是否可以购买
        object ret = MarketMgr.IsBuy(ME.user, mMarketData.GetValue<int>("class_id"));

        // 无法购买
        if (ret is string)
        {
            DialogMgr.Notify((string) ret);

            return;
        }

        if (mIsSelect && selectId == 0)
        {
            // 尚未选择目标道具，无法购买。
            DialogMgr.Notify(LocalizationMgr.Get("LimitGiftBagWnd_2"));

            return;
        }

        LPCMapping buyPrice = mMarketData.GetValue<LPCMapping>("buy_price");
        if (buyPrice == null)
            return;

        string costFields = FieldsMgr.GetFieldInMapping(buyPrice);

        string desc = string.Empty;

        if (string.IsNullOrEmpty(mMarketData.GetValue<string>("purchase_id")))
        {
            desc = string.Format(LocalizationMgr.Get("QuickMarketWnd_2"), buyPrice.GetValue<int>(costFields), FieldsMgr.GetFieldName(costFields));
        }
        else
        {
            desc = string.Format(LocalizationMgr.Get("QuickMarketWnd_4"), MarketMgr.GetSkuPrice(mMarketData.GetValue<int>("class_id")));
        }

        DialogMgr.ShowDailog(
            new CallBack(OnDailogCallBack),
            desc,
            string.Empty,
            string.Empty,
            string.Empty,
            true,
            this.transform
        );
    }

    void OnDailogCallBack(object para, params object[] param)
    {
        if (!(bool) param[0])
            return;

        // 购买商品
        MarketMgr.Buy(ME.user, mMarketData, 1);
    }

    /// <summary>
    /// 限购倒计时
    /// </summary>
    void CountDown()
    {
        if (mRemainTime <= 0)
        {
            mLimitTimer.text = string.Empty;

            mEnableCountDown = false;

            if (this != null)
                WindowMgr.DestroyWindow(gameObject.name);

            return;
        }

        mLimitTimer.text = string.Format(mLimitTimerStr, TimeMgr.ConvertTime(mRemainTime, true));

        mRemainTime--;
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int classId)
    {
        mItemClassId = classId;

        // 绘制窗口
        Redraw();

        InitLabel();

        SetBuyButtonState();
    }
}