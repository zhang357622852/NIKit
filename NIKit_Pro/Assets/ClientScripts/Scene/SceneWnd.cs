/// <summary>
/// SceneWnd.cs
/// Created by zhaozy 2016/06/15
/// 场景窗口处理脚本
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using TMPro;
using LPC;

public class SceneWnd : MonoBehaviour
{
    #region 变量

    // 窗口子控件
    public GameObject mTipWnd;

    public TextMeshPro mBtnLabel;

    // 红点提示类型
    public int TipsType = 0;

    public string mLocalText;

    // 是否需要显示tip
    private bool isShowTip = false;

    // 窗口点击回调
    public delegate void OnClickWndDelegate(GameObject ob);

    /// <summary>
    /// The m bounds size x.
    /// </summary>
    private float mBoundsSizeX = 0f;

    #endregion

    #region 属性

    // 点击敞口回调
    public OnClickWndDelegate OnClick { get; set; }

    #endregion

    /// <summary>
    /// Start this instance.
    /// </summary>
    public void Start()
    {
        // 重绘窗口
        Redraw();

        // 注册事件
        RegisterEvent();

        // 设置文本信息
        mBtnLabel.text = LocalizationMgr.Get(mLocalText);
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    private void Update()
    {
        // 调整窗口位置
        AdjustTipWndPosition();
    }

    /// <summary>
    /// Adjusts the tip window position.
    /// </summary>
    void AdjustTipWndPosition()
    {
        // 控件没有显示，不需要处理
        if (! mTipWnd.activeSelf)
            return;

        // mBtnLabel窗口大小
        Bounds bounds = mBtnLabel.bounds;

        // 如果bounds.size没有发生变化
        if (Game.FloatEqual(bounds.size.sqrMagnitude, mBoundsSizeX))
            return;

        // 记录mBoundsSizeX
        mBoundsSizeX = bounds.size.sqrMagnitude;

        // 设置红点位置
        mTipWnd.transform.localPosition = new Vector3(
            mBtnLabel.transform.localPosition.x + bounds.size.x - 0.05f,
            mBtnLabel.transform.localPosition.y - 0.14f,
            mBtnLabel.transform.localPosition.z
        );
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        if (ME.user == null)
            return;

        // 取消字段监听
        ME.user.dbase.RemoveTriggerField("SceneWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        switch (TipsType)
        {
            // 市集红点提示处理
            case RedTipsConst.SHOP_TIPS_TYPE:

                if (ME.user == null)
                    return;

                // 刷新市集红点提示
                RefreshShopTips();

                // 监听字段的变化
                ME.user.dbase.RegisterTriggerField("SceneWnd", new string[] { "refresh_shop" }, new CallBack(OnRefreshShopChange));

                break;

             // 许愿喷泉红点提示处理
            case RedTipsConst.LOTTERY_TIPS_TYPE:

                // 刷新抽奖提示
                RefreshLotteryTips();

                ME.user.dbase.RegisterTriggerField("SceneWnd", new string[] { "lottery_bonus" }, new CallBack(OnBonusTimesChanged));

                break;

            // 竞技场红点提示处理
            case RedTipsConst.ARENA_TIPS_TYPE:

                // 刷新竞技场提示
                RefreshArenaTips();

                ME.user.dbase.RegisterTriggerField("SceneWnd", new string[] { "revenge_data" }, new CallBack(OnRevengeDataChange));

                break;

            // 公会红点提示
            case RedTipsConst.GANG_TIPS_TYPE:

                // 刷新公会红点提示
                RefreshGangRedTips();

                ME.user.dbase.RegisterTriggerField("SceneWnd", new string[] { "my_gang_info" }, new CallBack(OnGangRequestChange));
                ME.user.dbase.RegisterTriggerField("SceneWnd", new string[] { "user_requests" }, new CallBack(OnGangRequestChange));

                break;

            // 不处理
            default:
                break;
        }
    }

    /// <summary>
    /// gang_request字段变化回调
    /// </summary>
    void OnGangRequestChange(object para, params object[] param)
    {
        // 刷新公会红点提示
        RefreshGangRedTips();
    }

    /// <summary>
    /// lottery_bonus字段变化回调
    /// </summary>
    void OnBonusTimesChanged(object para, params object[] param)
    {
        // 刷新抽奖提示
        RefreshLotteryTips();
    }

    /// <summary>
    /// revenge_data字段变化回调
    /// </summary>
    void OnRevengeDataChange(object para, params object[] param)
    {
        // 刷新竞技场提示
        RefreshArenaTips();
    }

    /// <summary>
    /// refresh_shop 字段变化回调
    /// </summary>
    void OnRefreshShopChange(object para, params object[] param)
    {
        // 刷新市集红点提示
        RefreshShopTips();
    }

    /// <summary>
    /// 刷新抽奖提示
    /// </summary>
    void RefreshLotteryTips()
    {
        // 抽奖次数
        int times = ME.user.Query<int>("lottery_bonus/lottery_times");

        if (times > 0)
        {
            SetShowTip(true);
        }
        else
        {
            SetShowTip(false); 
        }
    }

    /// <summary>
    /// 刷新市集红点提示
    /// </summary>
    void RefreshShopTips()
    {
        LPCValue refreshShop = ME.user.Query<LPCValue>("refresh_shop");

        if (refreshShop == null || !refreshShop.IsMapping)
        {
            SetShowTip(false);
            return;
        }

        if (refreshShop.AsMapping.GetValue<int>("refresh_flag") == 1)
            SetShowTip(true);
        else
            SetShowTip(false);
    }

    /// <summary>
    /// 刷新竞技场提示
    /// </summary>
    void RefreshArenaTips()
    {
        // 如果是审核模式不需要显示反击列表提示小红点
        if (ME.user.QueryTemp<int>("gapp_world") == 1)
        {
            SetShowTip(false);
            return;
        }

        LPCValue revengeData = ME.user.Query<LPCValue>("revenge_data");
        if (revengeData == null || !revengeData.IsArray)
        {
            SetShowTip(false);
            return;
        }

        LPCArray revenges = revengeData.AsArray;

        // 累计未挑战过的反击数量
        int amount = 0;

        for (int i = 0; i < revenges.Count; i++)
        {
            if (revenges[i] == null || !revenges[i].IsMapping)
                continue;

            if (revenges[i].AsMapping.GetValue<int>("revenged") > ArenaConst.ARENA_REVENGE_TYPE_NONE)
                continue;

            amount++;
        }

        if (amount < 1)
        {
            SetShowTip(false);
        }
        else
        {
            SetShowTip(true);
        }
    }

    /// <summary>
    /// 刷新公会红点提示
    /// </summary>
    private void RefreshGangRedTips()
    {
        LPCMapping myGangInfo = LPCMapping.Empty;

        LPCValue value = ME.user.Query<LPCValue>("my_gang_info");
        if (value != null && value.IsMapping && value.AsMapping.Count != 0)
            myGangInfo = value.AsMapping;

        LPCArray gangRequests = LPCArray.Empty;
        if (myGangInfo.ContainsKey("gang_requests") && myGangInfo["gang_requests"].IsArray)
            gangRequests = myGangInfo["gang_requests"].AsArray;

        if (gangRequests.Count > 0)
        {
            SetShowTip(true);

            return;
        }

        LPCArray userRequests = LPCArray.Empty;

        LPCValue v = ME.user.Query<LPCValue>("user_requests");
        if (v != null && v.IsArray)
            userRequests = v.AsArray;

        if (userRequests.Count > 0)
        {
            SetShowTip(true);

            return;
        }

        SetShowTip(false);
    }

    /// <summary>
    /// Raises the click window event.
    /// </summary>
    public void OnClickWnd()
    {
        // 抛出窗口点击事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_CLICK_SECNE_WND, MixedValue.NewMixedValue<string>(gameObject.name));

        if (TipsType.Equals(RedTipsConst.SHOP_TIPS_TYPE))
            SetShowTip(false);

        // 通知音效播放点击音效
        GameSoundMgr.OnUIClicked();

        if (OnClick == null)
            return;

        // 执行点击回调
        OnClick(gameObject);
    }

    /// <summary>
    /// Raises the click window event.
    /// </summary>
    public void SetShowTip(bool isShow)
    {
        // 记录数据
        isShowTip = isShow;

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// Raises the click window event.
    /// </summary>
    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    /// <summary>
    /// Redraw this instance.
    /// </summary>
    private void Redraw()
    {
        // 设置提示按钮状态
        if (isShowTip != mTipWnd.activeSelf)
            mTipWnd.SetActive(isShowTip);
    }
}
