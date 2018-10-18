/// <summary>
/// QuickMarketWnd.cs
/// Created by fengsc 2017/02/22
/// 快捷购买窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;
using QCCommonSDK;
using QCCommonSDK.Addition;

public class QuickMarketWnd : WindowBase<QuickMarketWnd>
{
    #region 成员变量

    // 窗口标题
    public UILabel mTitle;

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    // 商品基础格子
    public GameObject mItem;

    // 排序组件
    public UIGrid mGrid;

    public TweenAlpha mTweenAlpha;

    public TweenScale mTweenScale;

    string mGroup = string.Empty;

    GameObject mItemPos;

    LPCMapping mLimitBuyData = new LPCMapping();

    // 缓存商品格子对象
    List<GameObject> mMarketItemList = new List<GameObject>();

    List<LPCValue> mMarketCsvData = new List<LPCValue>();

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 注册点击事件
        RegisterEvent();

        if (mTweenAlpha != null && mTweenScale != null)
        {
            // 播放动画
            mTweenAlpha.PlayForward();

            mTweenScale.PlayForward();

            // 动画组件重置
            mTweenScale.ResetToBeginning();

            mTweenAlpha.ResetToBeginning();
        }

        // 初始化本地化文本
        InitLocalText();

        // 获取某个分页的数据
        mMarketCsvData = MarketMgr.GetQucikMarketData(ME.user, mGroup);

        // 创建一批商品格子
        CreatedGameObject(mMarketCsvData.Count);

        // 绘制窗口
        Redraw();
    }

    void OnEnable()
    {
#if ! UNITY_EDITOR

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

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent(QuickMarketWnd.WndType);

#if ! UNITY_EDITOR

        SkuSupport.OnSkuResult -= SkuHook;

#endif
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        // 监听物品购买成功事件
        EventMgr.RegisterEvent(QuickMarketWnd.WndType, EventMgrEventType.EVENT_BUY_ITEM_SUCCESS, OnBuyItemSuccess);

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);
    }

    /// <summary>
    /// tween动画播放完成回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 物品购买成功回调
    /// </summary>
    void OnBuyItemSuccess(int eventId, MixedValue para)
    {
        DialogMgr.Notify(LocalizationMgr.Get("QuickMarketWnd_3"));

        // 刷新数据
        Redraw();
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocalText()
    {
        mTitle.text = LocalizationMgr.Get("QuickMarketWnd_1");
    }

    void CreatedGameObject(int count)
    {
        mItem.SetActive(false);

        for (int i = 0; i < count; i++)
        {
            GameObject clone = Instantiate(mItem);

            clone.transform.SetParent(mGrid.transform);

            clone.transform.localScale = Vector3.one;

            clone.transform.localPosition = Vector3.zero;

            // 注册商品的点击事件
            UIEventListener.Get(clone).onClick = OnClickItem;

            clone.SetActive(false);

            mMarketItemList.Add(clone);
        }
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if (mItem.activeSelf)
            mItem.SetActive(false);

        LPCValue v = ME.user.Query<LPCValue>("limit_buy_data");
        if (v != null && v.IsMapping)
            mLimitBuyData = v.AsMapping;

        for (int i = 0; i < mMarketCsvData.Count; i++)
        {
            if (mMarketCsvData[i] == null || !mMarketCsvData[i].IsMapping)
                continue;

            // 增加新的格子
            if (i + 1 > mMarketItemList.Count)
                CreatedGameObject(1);

            GameObject clone = mMarketItemList[i];

            clone.SetActive(true);

            // 绑定数据
            clone.GetComponent<MarketItemWnd>().Bind(mMarketCsvData[i].AsMapping, mLimitBuyData);

            // 注册商品格子的点击事件
            UIEventListener.Get(clone).onClick = OnClickItem;
        }

        // 激活排序组件
        mGrid.Reposition();
    }

    /// <summary>
    /// 商品格子点击事件
    /// </summary>
    void OnClickItem(GameObject go)
    {
        MarketItemWnd script = go.GetComponent<MarketItemWnd>();
        if (script == null)
            return;

        // 获取商品数据
        LPCMapping marketData = script.mMarketData;
        if (marketData == null || marketData.Count < 1)
            return;

        CsvRow itemData = ItemMgr.GetRow(marketData.GetValue<int>("class_id"));
        if (itemData == null)
            return;

        LPCMapping applyArg = itemData.Query<LPCMapping>("apply_arg");

        LPCMapping dbase = itemData.Query<LPCMapping>("dbase");
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

        object result = MarketMgr.IsBuy(ME.user, marketData.GetValue<int>("class_id"));

        // 无法购买
        if(result is string)
        {
            DialogMgr.Notify((string) result);

            return;
        }

        LPCMapping costMap = marketData.GetValue<LPCMapping>("buy_price");

        string field = FieldsMgr.GetFieldInMapping(costMap);

        string desc = string.Empty;

        if(string.IsNullOrEmpty(marketData.GetValue<string>("purchase_id")))
            desc = string.Format(LocalizationMgr.Get("QuickMarketWnd_2"), 
                costMap.GetValue<int>(field), FieldsMgr.GetFieldName(field));
        else
            desc = string.Format(LocalizationMgr.Get("QuickMarketWnd_4"),
                MarketMgr.GetSkuPrice(marketData.GetValue<int>("class_id")));

        DialogMgr.ShowDailog(
            new CallBack(OnDailogCallBack, marketData),
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

        LPCMapping itemData = para as LPCMapping;

        // 购买商品
        MarketMgr.Buy(ME.user, itemData, 1);
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        if (this == null)
            return;

        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
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
    public void Bind(string group)
    {
        mGroup = group;
    }
}
