/// <summary>
/// NoobGiftBagWnd.cs
/// Created by zhangwm 2018 /9/13
/// 新手超惠礼包
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class NoobGiftBagWnd : WindowBase<NoobGiftBagWnd>
{
    // 礼包标题
    public UILabel mTitle;

    // 限购提示
    public UILabel[] mLimitBuyTips;
    public UILabel mLimitBuy;

    // 关闭按钮
    public GameObject mCloseBTn;

    // 道具获得还提示
    public UILabel[] mTips;

    // 每日获得道具
    public GameObject mItemPrefab;

    public UIGrid mItemGrid;

    // 购买按钮
    public GameObject mBuyBtn;

    // 购买消耗
    public UILabel mCost;

    public UILabel mSkuCost;

    // 消耗品图标
    public UISprite mCostIcon;

    public UITexture mGiftBagIcon;

    public GameObject mBgEffect;

    // 道具对象
    private Property mItemOb;

    // 商城数据
    private LPCMapping mMarketData = LPCMapping.Empty;

    private CsvRow mGiftBagContent;

    void Start()
    {
        // 注册事件
        RegisterEvent();

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("NoobGiftBagWnd");
    }

    void Update()
    {
        if (mBgEffect != null)
            mBgEffect.transform.Rotate(Vector3.forward * Time.unscaledDeltaTime * 40);
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
            mLimitBuyTips[i].text = LocalizationMgr.Get(limitDesc[i].AsString);

        LPCArray tips = mGiftBagContent.Query<LPCArray>("tips");

        for (int i = 0; i < mTips.Length; i++)
            mTips[i].text = LocalizationMgr.Get(tips[i].AsString);

        mTitle.text = LocalizationMgr.Get(mGiftBagContent.Query<string>("title"));
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 按钮点击事件
        UIEventListener.Get(mCloseBTn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mBuyBtn).onClick = OnClickBuyBtn;

        // 注册EVENT_BUY_ITEM_SUCCESS事件
        EventMgr.RegisterEvent("NoobGiftBagWnd", EventMgrEventType.EVENT_BUY_ITEM_SUCCESS, OnBuyItemSuccCallBack);
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
        if (mItemOb == null)
            return;

        mGiftBagContent = MarketMgr.GetGiftContentConfig(mItemOb.GetClassID());

        mGiftBagIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/item/{0}.png", mGiftBagContent.Query<string>("icon")));

        int receiveDay = 1;

        // 礼包数据
        LPCValue giftBagData = ME.user.Query<LPCValue>("gift_bag_data");

        if (giftBagData != null && giftBagData.IsMapping && giftBagData.AsMapping.Count > 0)
        {
            LPCArray list = giftBagData.AsMapping.GetValue<LPCArray>(mItemOb.GetClassID());
            if (list != null && list.Count > 0)
            {
                LPCMapping receiveData = list[0].AsMapping;

                if (receiveData != null && receiveData.ContainsKey("receive_day"))
                    receiveDay = receiveData.GetValue<int>("receive_day");
            }
        }

        // 购买道具后每日登录可获得的道具列表
        LPCMapping giftBagConfig = MarketMgr.GetGiftBagData(mItemOb.GetClassID(), receiveDay);

        if (giftBagConfig == null)
            return;

        LPCArray loginGoods = giftBagConfig.GetValue<LPCArray>("bonus_list");

        mItemPrefab.SetActive(true);

        for (int i = 0; i < loginGoods.Count; i++)
        {
            if (loginGoods[i] == null || !loginGoods[i].IsMapping)
                continue;

            GameObject item = NGUITools.AddChild(mItemGrid.gameObject, mItemPrefab);

            if (item == null)
                continue;

            GoodsItemWnd script = item.GetComponent<GoodsItemWnd>();

            if (script == null)
                continue;

            script.Bind(loginGoods[i].AsMapping);
        }

        mItemPrefab.SetActive(false);
        mItemGrid.Reposition();

        // 购买按钮
        mMarketData = MarketMgr.GetMarketConfig(mItemOb.GetClassID()).ConvertLpcMap();

        LPCMapping buyArgs = mMarketData.GetValue<LPCMapping>("buy_args");
        if (buyArgs == null)
            return;

        mLimitBuy.text = buyArgs.GetValue<int>("amount_limit").ToString();

        LPCMapping buyPrice = mMarketData.GetValue<LPCMapping>("buy_price");
        if (buyPrice == null)
            return;

        string costFields = FieldsMgr.GetFieldInMapping(buyPrice);

        if (string.IsNullOrEmpty(mMarketData.GetValue<string>("purchase_id")))
        {
            mCost.gameObject.SetActive(true);
            mSkuCost.gameObject.SetActive(false);
            mCostIcon.gameObject.SetActive(true);

            mCost.text = buyPrice.GetValue<int>(costFields).ToString();
            mCostIcon.spriteName = costFields;
        }
        else
        {
            mCost.gameObject.SetActive(false);
            mCostIcon.gameObject.SetActive(false);
            mSkuCost.gameObject.SetActive(true);

            mSkuCost.text = MarketMgr.GetSkuPrice(mItemOb.GetClassID());
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
        // 是否可以购买
        object ret = MarketMgr.IsBuy(ME.user, mMarketData.GetValue<int>("class_id"));

        // 无法购买
        if (ret is string)
        {
            DialogMgr.Notify((string)ret);

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
        if (!(bool)param[0])
            return;

        // 购买商品
        MarketMgr.Buy(ME.user, mMarketData, 1);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void BindData(Property itemOb, string pos)
    {
        mItemOb = itemOb;

        // 绘制窗口
        Redraw();

        InitLabel();
    }
}
