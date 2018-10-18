/// <summary>
/// WeekendGiftItemWnd.cs
/// Created by zhangwm 2018/09/01
/// 周末特惠礼包界面
/// </summary>
using LPC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeekendGiftItemWnd : MonoBehaviour
{
    public WeekendGiftWnd mParent;

    public UITexture mIcon;

    public Transform mIconEffect;

    public UILabel mNameLab;

    public UILabel mDesLab;

    public UIGrid mGrid;

    public GameObject mBuyBtn;

    public GameObject mBtnMask;

    public UILabel mBuyCost;

    public UILabel mBuyRmbCost;

    public GameObject mMaskGo;

    public GameObject mGoodsPrefab;


    //月卡增幅
    public GameObject mIncrease;

    public UILabel mIncreaseTips;

    public UILabel mIncreaseTime;

    //已购买
    public UILabel mBuyedLab;

    private LPCMapping mMarketConfig;

    private void Start()
    {
        // 初始化文本
        InitText();

        // 注册事件
        RegisterEvent();
    }

    private void Update()
    {
        if (mIconEffect != null && mIconEffect.gameObject.activeSelf)
            mIconEffect.transform.Rotate(Vector3.forward * Time.unscaledDeltaTime * 40);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    private void InitText()
    {
        mBuyedLab.text = LocalizationMgr.Get("LevelGiftWnd_2");
        mIncreaseTips.text = LocalizationMgr.Get("MarketWnd_31");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mBuyBtn).onClick = OnClickBuyBtn;
    }

    /// <summary>
    /// 重绘
    /// </summary>
    private void Redraw()
    {
        if (mMarketConfig == null)
            return;

        LPCMapping marketDbase = mMarketConfig.GetValue<LPCMapping>("dbase");

        // 标题(icon/name/des)
        CsvRow giftBagContent = MarketMgr.GetGiftContentConfig(mMarketConfig.GetValue<int>("class_id"));

        if (giftBagContent != null)
        {
            mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/item/{0}.png", giftBagContent.Query<string>("icon")));

            mNameLab.text = LocalizationMgr.Get(giftBagContent.Query<string>("title"));

            LPCArray tips = giftBagContent.Query<LPCArray>("tips");
            mDesLab.text = LocalizationMgr.Get(tips.Get(0).AsString);
        }

        // 标题特效
        mIconEffect.gameObject.SetActive(marketDbase.GetValue<int>("show_effect") == 1);

        // 创建列表奖励物品
        CsvRow itemConfig = ItemMgr.GetRow(mMarketConfig.GetValue<int>("class_id"));
        if (itemConfig == null)
            return;

        LPCMapping applyArg = itemConfig.Query<LPCMapping>("apply_arg");
        if (applyArg == null)
            return;

        LPCArray goodsList = LPCArray.Empty;

        if (applyArg.ContainsKey("goods_list"))
            goodsList.Append(applyArg.GetValue<LPCArray>("goods_list"));

        if (applyArg.ContainsKey("pet_list"))
            goodsList.Append(applyArg.GetValue<LPCArray>("pet_list"));



        mGoodsPrefab.SetActive(true);
        mGrid.transform.DestroyChildren();

        for (int i = 0; i < goodsList.Count; i++)
        {
            if (goodsList[i] == null || !goodsList[i].IsMapping)
                continue;

            GameObject item = NGUITools.AddChild(mGrid.gameObject, mGoodsPrefab);
            if (item == null)
                continue;

            GoodsItemWnd script = item.GetComponent<GoodsItemWnd>();
            if (script == null)
                continue;

            script.Bind(goodsList[i].AsMapping);
        }

        mGoodsPrefab.SetActive(false);
        mGrid.Reposition();

        //购买按钮信息
        if (string.IsNullOrEmpty(mMarketConfig.GetValue<string>("purchase_id")))
        {
            //非人民币
            LPCMapping buyPrice = mMarketConfig.GetValue<LPCMapping>("buy_price");
            if (buyPrice == null)
                return;

            string costFields = FieldsMgr.GetFieldInMapping(buyPrice);
            mBuyCost.gameObject.SetActive(true);
            mBuyCost.text = buyPrice.GetValue<int>(costFields).ToString();
            mBuyCost.GetComponentInChildren<UISprite>().spriteName = FieldsMgr.GetFieldIcon(costFields);
            mBuyRmbCost.text = string.Empty;
        }
        else
        {
            //人民币
            mBuyCost.gameObject.SetActive(false);
            mBuyRmbCost.text = MarketMgr.GetSkuPrice(mMarketConfig.GetValue<int>("class_id"));
        }

        // 商品的附加属性
        if (marketDbase.ContainsKey("increase_time"))
        {
            // 存在月卡增幅
            if (!mIncrease.activeSelf)
                mIncrease.SetActive(true);

            // 显示月卡增幅天数
            mIncreaseTime.text = string.Format("+{0}{1}", marketDbase.GetValue<int>("increase_time") / 86400, LocalizationMgr.Get("MarketWnd_32"));
        }
        else
        {
            // 没有月卡增幅
            if (mIncrease.activeSelf)
                mIncrease.SetActive(false);
        }

        //遮罩
        mMaskGo.SetActive(false);

        object ret = MarketMgr.IsBuy(ME.user, mMarketConfig.GetValue<int>("class_id"));

        mBtnMask.SetActive(ret is string);

        if (ret.GetType() == typeof(bool))
            if ((bool)ret)
                return;

        LPCMapping limitBuyData = LPCMapping.Empty;

        LPCMapping buyArgs = mMarketConfig.GetValue<LPCMapping>("buy_args");

        // 获取玩家购买的限购商品数据
        LPCValue v = ME.user.Query<LPCValue>("limit_buy_data");
        if (v != null && v.IsMapping)
            limitBuyData = v.AsMapping;

        // 已经购买的数量
        int haveBuyAmount = 0;

        // 没有该商品的限购数据
        if (limitBuyData.ContainsKey(mMarketConfig.GetValue<int>("class_id")))
        {
            LPCMapping data = limitBuyData.GetValue<LPCMapping>(mMarketConfig.GetValue<int>("class_id"));

            haveBuyAmount = data.GetValue<int>("amount");
        }

        if (haveBuyAmount >= buyArgs.GetValue<int>("amount_limit"))
            mMaskGo.SetActive(true);
    }

    /// <summary>
    /// 购买点击回调
    /// </summary>
    private void OnClickBuyBtn(GameObject go)
    {
        DoBuy(mMarketConfig);
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
            mParent.transform
        );
    }

    private void OnDailogCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        // 购买商品
        MarketMgr.Buy(ME.user, (LPCMapping)para, 1);
    }

    public void BindData(LPCMapping config)
    {
        if (config == null)
            return;

        mMarketConfig = config;

        Redraw();
    }
}
