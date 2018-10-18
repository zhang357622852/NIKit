/// <summary>
/// LevelGiftWnd.cs
/// Created by zhangwm 2018/08/09
/// 等级礼包界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class LevelGiftWnd : WindowBase<LevelGiftWnd>
{
    #region 成员变量
    //限时！初心者特惠礼包
    public UILabel mTitleLab;

    public UILabel mLimitTimer;

    public GameObject mCloseBtn;

    public LevelGiftItemWnd mRightItemCtrl;

    public LevelGiftItemWnd mLevelItemCtrl;

    public TweenScale mTweenScale;

    private List<CsvRow> mMarkitInfoList = new List<CsvRow>();

    private bool mEnableCountDown = false;

    private float mLastTime = 0f;

    private int mRemainTime = 0;
    #endregion

    private void Start()
    {
        RegisterEvent();

        Redraw();
    }

    private void Update()
    {
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

    /// <summary>
    /// 限购倒计时
    /// </summary>
    private void CountDown()
    {
        if (mRemainTime <= 0)
        {
            mLimitTimer.text = string.Empty;

            mEnableCountDown = false;

            if (this != null)
                WindowMgr.DestroyWindow(gameObject.name);

            return;
        }


        if (mRemainTime > 86400)
        {
            // 倒计时格式: 该礼包还剩余XX天
            mLimitTimer.text = LocalizationMgr.Get("LevelGiftWnd_1") + string.Format(LocalizationMgr.Get("LimitMarketView_2"), mRemainTime / 86400);
        }
        else if (mRemainTime > 3600)
        {
            // 倒计时格式：  该礼包还剩余XX小时
            mLimitTimer.text = LocalizationMgr.Get("LevelGiftWnd_1") + string.Format(LocalizationMgr.Get("LimitMarketView_3"), mRemainTime / 3600);
        }
        else
        {
            // 倒计时格式： 该礼包还剩余 XX分:XX秒
            mLimitTimer.text = LocalizationMgr.Get("LevelGiftWnd_1") + TimeMgr.ConvertTimeToChineseTimer(mRemainTime, true, false);
        }

        mRemainTime--;
    }

    private void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
        EventMgr.UnregisterEvent("LevelGiftWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        EventMgr.RegisterEvent("LevelGiftWnd", EventMgrEventType.EVENT_BUY_ITEM_SUCCESS, OnBuyItemSuccCallBack);

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
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

    private void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    private void Redraw()
    {
        if (ME.user == null)
            return;

        LPCMapping giftMap = ME.user.Query<LPCMapping>("level_gift");

        if (giftMap == null || !giftMap.ContainsKey("level") || !giftMap.ContainsKey("overdue_time"))
            return;

        mMarkitInfoList = MarketMgr.GetLevelGifts(giftMap.GetValue<int>("level"));

        if (mMarkitInfoList.Count < 2)
            return;

        //---Right---
        mRightItemCtrl.BindData(mMarkitInfoList[0].ConvertLpcMap());

        //---Left---
        mLevelItemCtrl.BindData(mMarkitInfoList[1].ConvertLpcMap());

        // 限购所剩时间
        mRemainTime = giftMap.GetValue<int>("overdue_time") - TimeMgr.GetServerTime() - 1;
        mEnableCountDown = true;

        //标题
        int classId = mMarkitInfoList[0].Query<int>("class_id");
        CsvRow contentConfig = MarketMgr.GetGiftContentConfig(classId);
        mTitleLab.text = LocalizationMgr.Get(contentConfig.Query<string>("title"));
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    private void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }
}
