/// <summary>
/// WeekendGiftWnd.cs
/// Created by zhangwm 2018/09/01
/// 周末特惠礼包界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class WeekendGiftWnd : WindowBase<WeekendGiftWnd>
{
    #region 成员变量
    //周末特惠礼包
    public UILabel mTitleLab;

    // 限购提示
    public UILabel[] mLimitBuyTips;
    public UILabel mLimitBuy;

    public GameObject mCloseBtn;

    public WeekendGiftItemWnd mRightItemCtrl;

    public WeekendGiftItemWnd mLeftItemCtrl;

    public TweenScale mTweenScale;

    private int mGiftClassId;

    #endregion

    private void Start()
    {
        RegisterEvent();
    }

    private void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        EventMgr.UnregisterEvent("WeekendGiftWnd");
    }

    /// <summary>
    /// 初始化文本
    /// </summary>
    private void InitText()
    {
        CsvRow giftContentConfig = MarketMgr.GetGiftContentConfig(mGiftClassId);

        if (giftContentConfig == null)
            return;

        LPCArray limitDesc = giftContentConfig.Query<LPCArray>("limit_desc");

        for (int i = 0; i < mLimitBuyTips.Length; i++)
        {
            if (i + 1 > limitDesc.Count)
                continue;

            mLimitBuyTips[i].text = LocalizationMgr.Get(limitDesc[i].AsString);
        }
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        // 购买成功回调监听
        EventMgr.RegisterEvent("WeekendGiftWnd", EventMgrEventType.EVENT_BUY_ITEM_SUCCESS, OnBuyItemSuccCallBack);

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

        List<int> giftList = MarketMgr.GetValidWeekendGifts(ME.user);

        if (giftList.Count < 2)
        {
            OnClickCloseBtn(null);

            return;
        }

        //---Right---
        CsvRow config = MarketMgr.GetMarketConfig(giftList[0]);

        if (config != null)
        {
            mRightItemCtrl.BindData(config.ConvertLpcMap());

            // 限购标题提示
            LPCMapping buyArgs = config.Query<LPCMapping>("buy_args");

            mLimitBuy.text = buyArgs.GetValue<int>("amount_limit").ToString();
        }

        //---Left---
        config = MarketMgr.GetMarketConfig(giftList[1]);

        if (config != null)
            mLeftItemCtrl.BindData(config.ConvertLpcMap());

        //标题
        mTitleLab.text = LocalizationMgr.Get("WeekendGiftWnd_1");
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    private void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void BindData(int classId)
    {
        mGiftClassId = classId;

        InitText();

        Redraw();
    }
}
