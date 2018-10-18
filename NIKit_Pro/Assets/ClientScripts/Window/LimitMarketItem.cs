/// <summary>
/// LimitMarketItem.cs
/// Created by fengsc 2017/12/11
/// 限时商城基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class LimitMarketItem : WindowBase<LimitMarketItem>
{
    // 商品图标显示
    public UITexture mIcon;

    // 名称显示
    public UILabel mName;

    // 剩余倒计时显示
    public UILabel mTimer;

    public UISprite mTimerIcon;

    // 预告提示背景
    public UISprite mTipsBg;

    // 预告提示信息
    public UILabel mTips;

    // 道具classId
    [HideInInspector]
    public int mClassId = 0;

    int mRemainTime = 0;

    bool mEnableCountDown = false;

    float mLastTime = 0f;

    void Update()
    {
        if (mEnableCountDown)
        {
            if (Time.realtimeSinceStartup > mLastTime + 1)
            {
                mLastTime = Time.realtimeSinceStartup;

                // 倒计时
                CountDown();
            }
        }
    }

    /// <summary>
    /// 倒计时
    /// </summary>
    void CountDown()
    {
        if (mRemainTime <= 0)
        {
            mTimer.text = string.Empty;

            // 结束调用
            mEnableCountDown = false;

            gameObject.SetActive(false);
        }

        if (mRemainTime > 86400)
        {
            // 倒计时格式：  XX天
            mTimer.text = string.Format(LocalizationMgr.Get("LimitMarketView_2"), mRemainTime / 86400);
        }
        else if (mRemainTime > 3600)
        {
            // 倒计时格式：  XX小时
            mTimer.text = string.Format(LocalizationMgr.Get("LimitMarketView_3"), mRemainTime / 3600);
        }
        else
        {
            // 倒计时格式：  XX分:XX秒
            mTimer.text = TimeMgr.ConvertTimeToChineseTimer(mRemainTime, true, false);
        }

        mRemainTime--;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mTimer.text = string.Empty;

        mTipsBg.alpha = 0;

        mTips.gameObject.SetActive(false);

        CsvRow itemConfig = ItemMgr.GetRow(mClassId);
        if (itemConfig == null)
            return;

        // 礼包名称
        mName.text = LocalizationMgr.Get(itemConfig.Query<string>("name"));

        // 道具图标
        mIcon.mainTexture = ItemMgr.GetTexture(mClassId, mIcon.gameObject);

        // 商城道具配置数据
        CsvRow marketConfig = MarketMgr.GetMarketConfig(mClassId);
        if (marketConfig == null)
            return;

        // 礼包类型
        string group = marketConfig.Query<string>("group");

        // 初始化文字渐变颜色
        mName.gradientTop = new Color(1.0f, 1.0f, 1.0f);
        mName.gradientBottom = new Color(195 / 255f, 180 / 255f, 154 / 255f);

        mTimerIcon.color = new Color(1.0f, 1.0f, 1.0f);

        mTimer.color = new Color(167 / 255f, 147 / 255f, 109 / 255f);

        if (group == ShopConfig.WEEKEND_GIFT_GROUP)
        {
            // 周末礼包
            if (! Game.IsWeekend(TimeMgr.GetServerTime()))
            {
                // 显示预告信息
                mTipsBg.alpha = 1;

                mTips.text = LocalizationMgr.Get("LimitMarketView_1");

                mTips.gameObject.SetActive(true);

                // 修改名称的渐变色
                float topRGB = 115 / 255f;
                mName.gradientTop = new Color(topRGB, topRGB, topRGB);

                float bottomRGB = 196 / 255f;
                mName.gradientBottom = new Color(bottomRGB, bottomRGB, bottomRGB);

                mTimerIcon.color = new Color(255 / 255f, 119 / 255f, 134 / 255f);

                mTimer.color = new Color(255 / 255f, 88 / 255f, 88 / 255f);
            }

            // 计算剩余时间，距离下周一零点的时间
            mRemainTime = Game.GetThisSundayTime() + 86400;
        }
        else if (group == ShopConfig.DAILY_GIFT_GROUP)
        {
            // 每日礼包
            LPCMapping buyArgs = marketConfig.Query<LPCMapping>("buy_args");
            if (buyArgs == null)
                return;

            // 结束时长, 结束时长都是按照小时设定的
            LPCMapping buyTimeZone = buyArgs.GetValue<LPCMapping>("buy_time_zone");
            if (buyTimeZone == null)
                return;

            // 距离明天零点的时间
            mRemainTime = (int)Game.GetZeroClock(0) + buyTimeZone.GetValue<int>("end") * 3600;
        }
        else if (group == ShopConfig.INTENSIFY_GIFT_GROUP)
        {
            // 元素强化限时礼包
            LPCMapping buyArgs = marketConfig.Query<LPCMapping>("buy_args");

            if (buyArgs == null)
                return;

            LPCArray gift = MarketMgr.GetLimitStrengthList(ME.user);

            if (gift == null || gift.Count == 0)
                return;

            int startTime = 0;

            for (int i = 0; i < gift.Count; i++)
            {
                if (gift[i].AsMapping["class_id"].AsInt == mClassId)
                {
                    startTime = gift[i].AsMapping["start_time"].AsInt;
                    break;
                }
            }

            // 结束时长
            int endTime = buyArgs.GetValue<int>("valid_time");

            mRemainTime = endTime - (TimeMgr.GetServerTime() - startTime) - 1;
        }

        if (mRemainTime <= 0)
            this.gameObject.SetActive(false);
        else
            mEnableCountDown = true;
    }

    /// <summary>
    /// 获取item宽度
    /// </summary>
    public int GetItemWidth()
    {
        int width = Mathf.Max(mIcon.width, mName.width);

        width = Mathf.Max(width, mTipsBg.width);

        width = Mathf.Max(width, mTimer.width + mTimerIcon.width + 4);

        return width;
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int classId)
    {
        mClassId = classId;

        // 绘制窗口
        Redraw();
    }
}
