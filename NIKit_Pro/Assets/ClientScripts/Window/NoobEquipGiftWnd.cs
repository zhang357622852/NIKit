/// <summary>
/// NoobEquipGiftWnd.cs
/// Created by zhangwm 2018/09/06
/// 等级礼包界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class NoobEquipGiftWnd : WindowBase<NoobEquipGiftWnd>
{

    #region 成员变量

    // 新手装备礼包
    public UILabel mTitle;

    // 限购提示
    public UILabel[] mLimitBuyTips;

    // 限购次数
    public UILabel mLimitBuy;

    // 礼包icon
    public UITexture mGiftBagIcon;

    // 礼包背景光效
    public GameObject mBgEffect;

    // 2件套装
    public UILabel mSuitSubCount;

    // 暴击率+12%
    public UILabel mSuitDesc;

    // (点击查看装备详细属性)
    public UILabel mSuitTips;

    // 装备grid
    public UIGrid mEquipGrid;

    // 装备item预制体
    public GameObject mEquipItemPrefab;

    /// 装备信息悬浮
    public GameObject mEquipView;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 购买按钮
    public UISprite mBuyBtn;

    // $12
    public UILabel mBuyRmbCost;

    // 购买消耗
    public UILabel mCost;

    // 消耗品图标
    public UISprite mCostIcon;

    // 物品item预制体
    public GameObject mGoodsPrefab;

    public UIGrid mGoodsGrid;

    public TweenScale mTweenScale;

    // 道具class_id
    private int mItemClassId;

    // 商城数据
    private LPCMapping mMarketData = LPCMapping.Empty;

    private CsvRow mGiftBagContent = null;

    private List<Property> mEquipPropertyList = new List<Property>();

    #endregion

    private void Start()
    {
        RegisterEvent();
    }

    private void Update()
    {
        if (mBgEffect != null)
            mBgEffect.transform.Rotate(Vector3.forward * Time.unscaledDeltaTime * 40);
    }

    private void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        EventMgr.UnregisterEvent("NoobEquipGiftWnd");

        for (int i = 0; i < mEquipPropertyList.Count; i++)
        {
            if (mEquipPropertyList[i] == null)
                continue;

            mEquipPropertyList[i].Destroy();
        }
    }

    /// <summary>
    /// 初始化文本
    /// </summary>
    private void InitLabel()
    {
        if (mGiftBagContent == null)
            return;

        // 限购次数描述
        LPCArray limitDesc = mGiftBagContent.Query<LPCArray>("limit_desc");

        for (int i = 0; i < mLimitBuyTips.Length; i++)
        {
            if (i + 1 > limitDesc.Count)
                continue;

            mLimitBuyTips[i].text = LocalizationMgr.Get(limitDesc[i].AsString);
        }

        // 新手装备礼包
        mTitle.text = LocalizationMgr.Get(mGiftBagContent.Query<string>("title"));

        mSuitTips.text = LocalizationMgr.Get("NoobEquipGiftWnd_1");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        // 按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        UIEventListener.Get(mBuyBtn.gameObject).onClick = OnClickBuyBtn;

        EventMgr.RegisterEvent("NoobEquipGiftWnd", EventMgrEventType.EVENT_BUY_ITEM_SUCCESS, OnBuyItemSuccCallBack);

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    private void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 道具购买成功回调
    /// </summary>
    private void OnBuyItemSuccCallBack(int eventId, MixedValue para)
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

    private void OnDialogCallBack(object para, params object[] param)
    {
        if (this == null)
            return;

        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    private void Redraw()
    {
        // 道具的配置表数据
        CsvRow itemConfig = ItemMgr.GetRow(mItemClassId);

        if (itemConfig == null)
            return;

        mMarketData = MarketMgr.GetMarketConfig(mItemClassId).ConvertLpcMap();

        if (mMarketData == null)
            return;

        LPCMapping buyArgs = mMarketData.GetValue<LPCMapping>("buy_args");

        // 限购次数
        mLimitBuy.text = buyArgs.GetValue<int>("amount_limit").ToString();

        mGiftBagContent = MarketMgr.GetGiftContentConfig(mItemClassId);

        // 礼包icon
        if (mGiftBagContent != null)
        {
            mGiftBagIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/item/{0}.png", mGiftBagContent.Query<string>("icon")));
        }

        LPCMapping applyArg = itemConfig.Query<LPCMapping>("apply_arg");

        if (applyArg == null)
            return;

        LPCArray goodsList = LPCArray.Empty;

        if (applyArg.ContainsKey("goods_list"))
            goodsList.Append(applyArg.GetValue<LPCArray>("goods_list"));

        if (applyArg.ContainsKey("pet_list"))
            goodsList.Append(applyArg.GetValue<LPCArray>("pet_list"));

        // 套装 装备
        if (goodsList.Count > 0 && goodsList[0].AsMapping.ContainsKey("suit_id") && goodsList[0].AsMapping.ContainsKey("property_list"))
        {
            CsvFile csv = EquipMgr.SuitTemplateCsv;

            if (csv == null)
                return;

            CsvRow row = csv.FindByKey(goodsList[0].AsMapping.GetValue<int>("suit_id"));

            if (row == null)
                return;

            LPCArray props = row.Query<LPCArray>("props");

            if (props == null)
                return;

            mSuitSubCount.text = LocalizationMgr.Get(row.Query<string>("name")) + string.Format("{0}{1}", row.Query<int>("sub_count"), LocalizationMgr.Get("EquipViewWnd_1"));

            string suitDesc = string.Empty;

            //获取套装描述信息;
            foreach (LPCValue item in props.Values)
                suitDesc += PropMgr.GetPropDesc(item.AsArray, EquipConst.SUIT_PROP);

            mSuitDesc.text = suitDesc;

            // 装备奖励
            LPCArray equipArray = goodsList[0].AsMapping.GetValue<LPCArray>("property_list");

            mEquipItemPrefab.SetActive(true);

            GameObject go;

            Property pro;

            Vector3 parentVec = mEquipItemPrefab.transform.localScale;

            for (int i = 0; i < equipArray.Count; i++)
            {
                go = NGUITools.AddChild(mEquipGrid.gameObject, mEquipItemPrefab);

                go.transform.localScale = parentVec;

                UIEventListener.Get(go).onPress = ClickEquipShowHoverWnd;

                pro = EquipMgr.GetTempEquipProperty(equipArray[i].AsMapping);

                if (pro == null)
                    continue;

                mEquipPropertyList.Add(pro);

                go.GetComponent<EquipItemWnd>().SetBind(pro);
            }

            mEquipItemPrefab.SetActive(false);
            mEquipGrid.Reposition();
        }

        //创建列表奖励物品
        mGoodsPrefab.SetActive(true);
        for (int i = 0; i < goodsList.Count; i++)
        {
            if (goodsList[i] == null || !goodsList[i].IsMapping)
                continue;

            GameObject item = NGUITools.AddChild(mGoodsGrid.gameObject, mGoodsPrefab);
            if (item == null)
                continue;

            GoodsItemWnd script = item.GetComponent<GoodsItemWnd>();
            if (script == null)
                continue;

            script.Bind(goodsList[i].AsMapping);
        }
        mGoodsPrefab.SetActive(false);
        mGoodsGrid.Reposition();

        //购买按钮信息
        if (string.IsNullOrEmpty(mMarketData.GetValue<string>("purchase_id")))
        {
            //非人民币
            LPCMapping buyPrice = mMarketData.GetValue<LPCMapping>("buy_price");
            if (buyPrice == null)
                return;

            string costFields = FieldsMgr.GetFieldInMapping(buyPrice);
            mCost.gameObject.SetActive(true);
            mCost.text = buyPrice.GetValue<int>(costFields).ToString();
            mCostIcon.spriteName = FieldsMgr.GetFieldIcon(costFields);
            mBuyRmbCost.text = string.Empty;
        }
        else
        {
            //人民币
            mCost.gameObject.SetActive(false);
            mBuyRmbCost.text = MarketMgr.GetSkuPrice(mMarketData.GetValue<int>("class_id"));
        }
    }

    /// <summary>
    ///点击装备显示悬浮窗口
    /// </summary>
    void ClickEquipShowHoverWnd(GameObject go, bool isPress)
    {
        if (isPress)
        {
            //获取装备对象
            Property equipOb = go.GetComponent<EquipItemWnd>().ItemOb;

            if (equipOb == null)
                return;

            BoxCollider box = go.GetComponent<BoxCollider>();

            if (box == null || mEquipView == null)
                return;

            Vector3 boxPos = box.transform.localPosition;

            mEquipView.transform.localPosition = new Vector3(boxPos.x, boxPos.y, boxPos.z);

            mEquipView.GetComponent<EquipViewWnd>().ShowView(equipOb.GetRid(), null);
        }
        else
        {
            if (mEquipView == null)
                return;

            mEquipView.GetComponent<EquipViewWnd>().HideView();
        }
    }

    /// <summary>
    /// 关闭按钮点击事件回调
    /// </summary>
    private void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 购买按钮点击事件回调
    /// </summary>
    private void OnClickBuyBtn(GameObject go)
    {
        if (mMarketData == null)
            return;

        DoBuy(mMarketData);
    }

    /// <summary>
    /// 购买
    /// </summary>
    /// <param name="marketData"></param>
    private void DoBuy(LPCMapping marketData)
    {
        // 是否可以购买
        object ret = MarketMgr.IsBuy(ME.user, marketData.GetValue<int>("class_id"));

        // 无法购买
        if (ret is string)
        {
            DialogMgr.Notify((string)ret);

            return;
        }

        LPCMapping buyPrice = marketData.GetValue<LPCMapping>("buy_price");
        if (buyPrice == null)
            return;

        string costFields = FieldsMgr.GetFieldInMapping(buyPrice);

        string desc = string.Empty;

        if (string.IsNullOrEmpty(marketData.GetValue<string>("purchase_id")))
            desc = string.Format(LocalizationMgr.Get("QuickMarketWnd_2"), buyPrice.GetValue<int>(costFields), FieldsMgr.GetFieldName(costFields));
        else
            desc = string.Format(LocalizationMgr.Get("QuickMarketWnd_4"), MarketMgr.GetSkuPrice(marketData.GetValue<int>("class_id")));

        DialogMgr.ShowDailog(
            new CallBack(OnDailogCallBack, marketData),
            desc,
            string.Empty,
            string.Empty,
            string.Empty,
            true,
            transform
        );
    }

    private void OnDailogCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        // 购买商品
        MarketMgr.Buy(ME.user, (LPCMapping)para, 1);
    }

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void BindData(int classId)
    {
        mItemClassId = classId;

        // 绘制窗口
        Redraw();

        InitLabel();
    }

    #endregion
}
