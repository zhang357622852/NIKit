/// <summary>
/// MarketItemWnd.cs
/// Created by fengsc 2017/04/20
/// 商城物品基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class MarketItemWnd : WindowBase<MarketItemWnd>
{
    #region 成员变量

    // 商品格子角标
    public GameObject mSub;
    public UILabel mSubDesc;

    // 赠送显示
    public GoodsItemWnd mGivingItem;

    public Transform mGivingItemEffect;

    // 醒目图片
    public UISprite mStriking;

    // 首充标识
    public GameObject mFirstFlag;

    public UILabel mFirstFlagLab;

    // 商品图标
    public UITexture mIcon;

    // 获得物品的数量
    public UILabel mAmount;

    // 商品描述
    public UILabel mShortDesc;

    // 消耗品图标
    public UISprite mCostIcon;

    // 购买商品的消耗
    public UILabel mCost;

    // 限购提示
    public UILabel mLimitTips;

    // 月卡剩余天数
    public GameObject mMonthCardResidueGo;

    public UILabel mMonthCardResidueLab;

    public UILabel mMonthCardAmountLab;

    public UISprite mBuyBg;

    public GameObject mMask;

    public GameObject mLock;

    public Property mMarketItemOb;

    public LPCMapping mMarketData = new LPCMapping();

    public UILabel mSkuCostLb;

    public GameObject mIncrease;

    public UILabel mIncreaseTips;

    public UILabel mIncreaseTime;

    LPCMapping mLimitBuyData = new LPCMapping();

    double mRemainTime = 0;

    #endregion

    void OnDestroy()
    {
        if (mMarketItemOb != null)
            mMarketItemOb.Destroy();
    }

    private void Update()
    {
        if (mGivingItemEffect != null && mGivingItem!= null && mGivingItem.gameObject.activeSelf)
            mGivingItemEffect.Rotate(Vector3.forward * Time.unscaledDeltaTime * 40);
    }


    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mIncreaseTips.text = LocalizationMgr.Get("MarketWnd_31");

        mFirstFlagLab.text = LocalizationMgr.Get("MarketWnd_38");

        mSub.SetActive(false);

        mMonthCardResidueGo.SetActive(false);

        mMonthCardAmountLab.gameObject.SetActive(false);

        mAmount.text = string.Empty;

        if (mMarketData == null || mMarketData.Count == 0)
            return;

        // 商品的附加属性
        LPCMapping marketDbase = mMarketData.GetValue<LPCMapping>("dbase");
        if (marketDbase.ContainsKey("increase_time"))
        {
            // 存在月卡增幅

            if (!mIncrease.activeSelf)
                mIncrease.SetActive(true);

            // 显示月卡增幅天数
            mIncreaseTime.text = string.Format("+{0}{1}", marketDbase.GetValue<int>("increase_time") / 86400, LocalizationMgr.Get("MarketWnd_32"));

            mBuyBg.width = 138;

            mSkuCostLb.width = 123;

            mCost.transform.localPosition = new Vector3(68.5f, mCost.transform.localPosition.y, mCost.transform.localPosition.z);

            mLock.transform.localPosition = new Vector3(36.5f, mLock.transform.localPosition.y, mLock.transform.localPosition.z);
        }
        else
        {
            // 没有月卡增幅
            if (mIncrease.activeSelf)
                mIncrease.SetActive(false);

            mBuyBg.width = 211;

            mSkuCostLb.width = 201;

            mCost.transform.localPosition = new Vector3(32, mCost.transform.localPosition.y, mCost.transform.localPosition.z);

            mLock.transform.localPosition = new Vector3(0, mLock.transform.localPosition.y, mLock.transform.localPosition.z);
        }

        int classId = mMarketData.GetValue<int>("class_id");

        // 构建参数
        LPCMapping para = LPCMapping.Empty;
        para.Add("rid", Rid.New());
        para.Add("class_id", classId);

        // 克隆一个物品对象
        if (mMarketItemOb != null)
            mMarketItemOb.Destroy();

        mMarketItemOb = PropertyMgr.CreateProperty(para);
        if (mMarketItemOb == null)
            return;

        string resPath = mMarketData.GetValue<string>("icon");
        if (string.IsNullOrEmpty(resPath))
        {
            if (MonsterMgr.IsMonster(mMarketItemOb))
            {
                mIcon.mainTexture = MonsterMgr.GetTexture(classId, mMarketItemOb.GetRank());
            }
            else if (ItemMgr.IsItem(mMarketItemOb))
            {
                mIcon.mainTexture = ItemMgr.GetTexture(classId, true);
            }
            else if (EquipMgr.IsEquipment(mMarketItemOb))
            {
                mIcon.mainTexture = EquipMgr.GetTexture(classId, mMarketItemOb.GetRarity());
            }
            else
            {
                mIcon.mainTexture = null;
            }
        }
        else
        {
            // 获取资源信息
            mIcon.mainTexture = ResourceMgr.LoadTexture(resPath, false);
        }

        // 醒目显示
        if (mStriking != null)
            mStriking.gameObject.SetActive(mMarketData.GetValue<int>("show_striking") == 1);

        // 首充标识
        mFirstFlag.SetActive(false);

        // 获取玩家都买次数信息
        LPCValue buyData = ME.user.Query<LPCValue>("limit_buy_data");
        LPCMapping limitBuyData;

        if (buyData != null && buyData.IsMapping)
            limitBuyData = buyData.AsMapping;
        else
            limitBuyData = LPCMapping.Empty;

        if (mFirstFlag != null && mMarketData.GetValue<int>("show_first") == 1)
        {
            // 没有购买次数信息，则任务是首次购买
            if (! limitBuyData.ContainsKey(classId) ||
                limitBuyData[classId].AsMapping["amount"].AsInt < 1)
                mFirstFlag.SetActive(true);
        }

        // 礼包数据
        LPCValue giftBagData = ME.user.Query<LPCValue>("gift_bag_data");

        if (giftBagData != null && giftBagData.IsMapping)
        {
            // 月卡礼包
            if (classId == ShopConfig.MONTH_CARD_ID)
            {
                int receiveDay = 0;

                LPCMapping receiveData = giftBagData.AsMapping.GetValue<LPCMapping>(classId);

                if (receiveData != null)
                {
                    mMonthCardResidueGo.SetActive(true);

                    if (receiveData.ContainsKey("receive_day"))
                        receiveDay = receiveData.GetValue<int>("receive_day");

                    // 已经购买月卡 月卡剩余？天
                    if (receiveData.ContainsKey("valid_day"))
                        mMonthCardResidueLab.text = string.Format(LocalizationMgr.Get("MarketWnd_39"), (receiveData.GetValue<int>("valid_day") - receiveDay));
                }
            }
        }

        // 月卡amount
        if (classId == ShopConfig.MONTH_CARD_ID && marketDbase.ContainsKey("show_days"))
        {
            mMonthCardAmountLab.gameObject.SetActive(true);

            mMonthCardAmountLab.text = string.Format(LocalizationMgr.Get("MarketWnd_40"), marketDbase.GetValue<int>("show_days"));
        }

        // 获取物品短描述
        mShortDesc.text = mMarketItemOb.Short();

        LPCMapping applyArg = mMarketItemOb.Query<LPCMapping>("apply_arg");

        LPCMapping dbase = mMarketItemOb.Query<LPCMapping>("dbase");

        LPCValue amount = null;

        if (dbase != null)
        {
            string attrib = string.Empty;

            if (dbase.ContainsKey("attrib"))
                attrib = dbase.GetValue<string>("attrib");

            if (!string.IsNullOrEmpty(attrib))
            {
                amount = applyArg.GetValue<LPCValue>(attrib);
            }
        }

        if (amount != null && amount.IsInt)
        {
            mAmount.text = amount.AsInt.ToString();

            mAmount.gameObject.SetActive(true);
        }
        else if (amount != null && amount.IsString)
        {
            mAmount.text = LocalizationMgr.Get("MarketWnd_19");

            mAmount.gameObject.SetActive(true);
        }
        else
        {
            mAmount.gameObject.SetActive(false);
        }

        if (mAmount.gameObject.activeInHierarchy)
            mIcon.transform.localPosition = new Vector3(0, -27, 0);
        else
            mIcon.transform.localPosition = new Vector3(0, -35, 0);

        string group = mMarketData.GetValue<string>("group");

        // 赠送物品数据
        if (mGivingItem != null)
            mGivingItem.gameObject.SetActive(false);

        LPCMapping givingGoods = mMarketData.GetValue<LPCMapping>("giving_goods");
        if (givingGoods.Count > 0)
        {
            string giftFields = FieldsMgr.GetFieldInMapping(givingGoods);

            mSub.gameObject.SetActive(true);

            mSubDesc.text = string.Format(LocalizationMgr.Get("MarketWnd_12"), givingGoods.GetValue<int>(giftFields));

            // 赠品显示效果
            LPCMapping givingMap = null;

            if (givingGoods.ContainsKey("show_effect") && givingGoods.GetValue<int>("show_effect") == 1)
                givingMap = givingGoods;
            else
            {
                if (givingGoods.ContainsKey("goods_list"))
                {
                    LPCArray goodsList = givingGoods.GetValue<LPCArray>("goods_list");

                    for (int i = 0; i < goodsList.Count; i++)
                    {
                        if (goodsList[i].AsMapping.GetValue<int>("show_item") == 1)
                        {
                            givingMap = goodsList[i].AsMapping;
                            break;
                        }
                    }
                }
            }

            if (givingMap != null && mGivingItem != null && mMarketData.GetValue<int>("show_giving") == 1 &&
                (!limitBuyData.ContainsKey(classId) || !Game.IsSameMonth(limitBuyData[classId].AsMapping.GetValue<int>("buy_time"), TimeMgr.GetServerTime())))
            {
                mGivingItem.gameObject.SetActive(true);
                mGivingItem.Bind(givingMap);
                mGivingItem.RefreshAmountNoName();
            }
        }

        object result = MarketMgr.IsBuy(ME.user, mMarketData.GetValue<int>("class_id"));

        if (mLimitBuyData == null)
            mLimitBuyData = LPCMapping.Empty;

        LPCMapping limitData = LPCMapping.Empty;
        if (mLimitBuyData.ContainsKey(classId))
            limitData = mLimitBuyData.GetValue<LPCMapping>(classId);

        LPCMapping buyArgs = mMarketData.GetValue<LPCMapping>("buy_args");

        int limiyType = buyArgs.GetValue<int>("limit_type");

        // 已经购买的数量
        int haveBuyAmount = limitData.GetValue<int>("amount");

        int buyTime = limitData.GetValue<int>("buy_time");

        int limit = 0;

        mLimitTips.gameObject.SetActive(true);

        if (result is string)
        {
            // 当前时间距离明天0点的时间差
            double seconds = Game.GetZeroClock(1);

            int hours = (int) seconds / 3600;

            switch (limiyType)
            {
                case ShopConfig.DAY_CYCLE_MARKET :
                    mRemainTime = seconds;

                    // 显示剩余多久
                    mLimitTips.text = string.Format(LocalizationMgr.Get("MarketWnd_13"), hours);
                    break;

                case ShopConfig.WEEK_CYCLE_MARKET:
                    int days = Game.GetWeekDay(TimeMgr.GetServerTime());

                    if (group.Equals(ShopConfig.WEEKEND_GIFT_GROUP))
                    {
                        days = days == 0 ? (7 - 1) : (6 - days);

                        mRemainTime = seconds + days * 24 * 60 * 60;

                        // 显示剩余多久
                        mLimitTips.text = string.Format(LocalizationMgr.Get("MarketWnd_7"), days);
                    }
                    else
                    {
                        days = days == 0 ? 7 : days;

                        mRemainTime = seconds + (7 - days) * 24 * 60 * 60;

                        // 显示剩余多久
                        mLimitTips.text = string.Format(LocalizationMgr.Get("MarketWnd_7"), 7 - days + 1);
                    }

                    break;

                case ShopConfig.MONTH_CYCLE_MARKET :

                    days = Game.GetDaysInMonth(TimeMgr.GetServerTime()) - Game.GetDaysMonth(TimeMgr.GetServerTime());

                    mRemainTime = seconds + days * 24 * 60 * 60;

                    // 显示剩余多久
                    mLimitTips.text = string.Format(LocalizationMgr.Get("MarketWnd_7"), days + 1);

                    break;

                case ShopConfig.ACCOUNT_LIMIT_MARKET :
                    // 该账号已购买
                    mLimitTips.text = LocalizationMgr.Get("MarketWnd_9");
                    break;

                case ShopConfig.LEVEL_LIMIT_MARKET :
                    limit = buyArgs.GetValue<int>("level_limit");

                    // 显示账号购买的等级限制
                    mLimitTips.text = string.Format(LocalizationMgr.Get("MarketWnd_10"), limit);
                    break;

                case ShopConfig.NO_LIMIT_MARKET:

                    mLimitTips.gameObject.SetActive(false);

                    break;

                case ShopConfig.NONE_LIMIT_MARKET:

                    mLimitTips.gameObject.SetActive(false);

                    break;

                default:
                    break;
            }

            if (mRemainTime != 0)
                Invoke("Redraw", (float) mRemainTime);

            mMask.SetActive(true);
            mBuyBg.spriteName = "redBtn";

            mCost.gameObject.SetActive(false);

            mSkuCostLb.gameObject.SetActive(false);

            mLock.SetActive(true);

            mLimitTips.color = new Color(1, 1, 1);

            mLimitTips.gradientBottom = new Color(1, 1, 1);

            mLimitTips.effectColor = new Color(161 / 255f, 0f, 0f);
        }
        else
        {
            switch (limiyType)
            {
                case ShopConfig.DAY_CYCLE_MARKET :
                    // 账号级限制购买数量
                    limit = buyArgs.GetValue<int>("amount_limit");

                    // 不是同一天
                    if (!Game.IsSameDay(TimeMgr.GetServerTime(), buyTime))
                        haveBuyAmount = 0;

                    mLimitTips.text = string.Format(LocalizationMgr.Get("MarketWnd_6"), 1, haveBuyAmount, limit);
                    break;

                case ShopConfig.WEEK_CYCLE_MARKET :
                    // 账号级限制购买数量
                    limit = buyArgs.GetValue<int>("amount_limit");

                    // 不是同一周
                    if (!Game.IsSameWeek(TimeMgr.GetServerTime(), buyTime))
                        haveBuyAmount = 0;

                    mLimitTips.text = string.Format(LocalizationMgr.Get("MarketWnd_6"), 7, haveBuyAmount, limit);
                    break;

                case ShopConfig.MONTH_CYCLE_MARKET :
                    // 账号级限制购买数量
                    limit = buyArgs.GetValue<int>("amount_limit");

                    // 不是同一月
                    if (!Game.IsSameMonth(TimeMgr.GetServerTime(), buyTime))
                        haveBuyAmount = 0;

                    mLimitTips.text = string.Format(LocalizationMgr.Get("MarketWnd_11"), haveBuyAmount, limit);
                    break;

                case ShopConfig.ACCOUNT_LIMIT_MARKET :
                    // 账号级限制购买数量
                    limit = buyArgs.GetValue<int>("amount_limit");

                    // 显示每个账号限购的次数
                    mLimitTips.text = string.Format(LocalizationMgr.Get("MarketWnd_8"), haveBuyAmount, limit);
                    break;

                case ShopConfig.LEVEL_LIMIT_MARKET :
                    mLimitTips.text = LocalizationMgr.Get("MarketWnd_5");
                    break;

                case ShopConfig.NO_LIMIT_MARKET :

                    mLimitTips.gameObject.SetActive(false);

                    break;

                case ShopConfig.NONE_LIMIT_MARKET:

                    mLimitTips.gameObject.SetActive(false);

                    break;

                default:
                    break;
            }

            mMask.SetActive(false);
            mBuyBg.spriteName = "marketBuyBtn";

            if(string.IsNullOrEmpty(mMarketData.GetValue<string>("purchase_id")))
            {
                LPCMapping costMap = PropertyMgr.GetBuyPrice(mMarketItemOb, true);

                mCost.gameObject.SetActive(true);
                mSkuCostLb.gameObject.SetActive(false);

                string costFields = FieldsMgr.GetFieldInMapping(costMap);
                mCostIcon.spriteName = FieldsMgr.GetFieldIcon(costFields);
                mCost.text = costMap.GetValue<int>(costFields).ToString();
            }
            else
            {
                mCost.gameObject.SetActive(false);
                mSkuCostLb.gameObject.SetActive(true);

                mSkuCostLb.text = MarketMgr.GetSkuPrice(classId);
            }

            mLock.SetActive(false);

            mLimitTips.color = new Color(1, 1, 1);

            mLimitTips.gradientBottom = new Color(1, 1, 1);

            mLimitTips.effectColor = new Color(148f / 255f, 100f / 255f, 15f / 255f);
        }
    }

    /// <summary>
    /// Bind the specified marketCsvData and limitBuydata.
    /// </summary>
    /// <param name="marketCsvData">Market csv data.</param>
    /// <param name="limitBuydata">Limit buydata.</param>
    public void Bind(LPCMapping marketCsvData, LPCMapping limitBuydata)
    {
        // 配置信息null
        if (marketCsvData == null || marketCsvData.Count == 0)
            return;

        // 重置mainTexture
        if (mIcon != null)
            mIcon.mainTexture = null;

        // 设置配置信息
        mMarketData = marketCsvData;
        mLimitBuyData = limitBuydata;

        // 绘制窗口
        Redraw();
    }
}
