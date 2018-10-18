/// <summary>
/// ArenaEnergyWnd.cs
/// Created by fengsc 2016/07/21
///竞技场入场卷数量显示窗口
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class ArenaEnergyWnd : WindowBase<ArenaEnergyWnd>
{

    #region 成员变量

    /// <summary>
    ///打开购买入场卷窗口按钮
    /// </summary>
    public GameObject mAddBtn;

    /// <summary>
    ///点击背景显示倒计时
    /// </summary>
    public GameObject mBgBtn;

    /// <summary>
    ///显示入场卷数量
    /// </summary>
    public UILabel mValue;

    /// <summary>
    ///显示计时器
    /// </summary>
    public UILabel mTimerLabel;

    /// <summary>
    ///竞技场入场券数量
    /// </summary>
    int arenaAmount = 0;

    string eventName = string.Empty;

    // 是否开启倒计时
    bool mEnableCountDown = false;

    // 上次更新的时间
    float mLastUpdateTime = 0;

    // 剩余时间
    int mRemainTime = 0;

    #endregion

    void Start()
    {
        if (mTimerLabel != null)
            //计时器默认隐藏;
            mTimerLabel.gameObject.SetActive(false);

        if (mAddBtn != null && mBgBtn != null)
        {
            UIEventListener.Get(mAddBtn).onClick = OnClickAddBtn;
            UIEventListener.Get(mBgBtn).onClick = OnClickBgBtn;
        }

        RedarwArenaAmount();
    }

    void Update()
    {
        if (mEnableCountDown)
        {
            // 每秒钟刷新一次
            if (Time.realtimeSinceStartup > mLastUpdateTime + 1)
            {
                mLastUpdateTime = Time.realtimeSinceStartup;
                CountDown();
            }
        }
    }

    void OnEnable()
    {
        RedarwArenaAmount();

        RegisterEvent();

        StartUpCountDown();
    }

    /// <summary>
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 玩家对象不存在;
        if (ME.user == null)
            return;

        eventName = Game.GetUniqueName("ArenaEnergyWnd") + "_arena";

        // 注册入场券变化回调;
        ME.user.dbase.RegisterTriggerField(eventName, new string[] { "ap", "max_ap" }, new CallBack(OnArenaChange));
        ME.user.dbase.RegisterTriggerField(eventName + "last_recover_ap_time",
            new string[] { "last_recover_ap_time" }, new CallBack(OnRecoverTimeChange));
    }

    /// <summary>
    /// 上次恢复的时间变化回调
    /// </summary>
    void OnRecoverTimeChange(object param, params object[] paramEx)
    {
        // 延迟到下一帧调用;
        MergeExecuteMgr.DispatchExecute(StartUpCountDown);
    }

    /// <summary>
    /// 启动倒计时功能
    /// </summary>
    void StartUpCountDown()
    {
        if (mTimerLabel == null)
        {
            // 结束倒计时
            mEnableCountDown = false;
            return;
        }

        mTimerLabel.text = TimeMgr.ConvertTime(0);

        // 上一次疲劳恢复的时间
        LPCValue recoverTime = ME.user.Query<LPCValue>("last_recover_ap_time");
        if (recoverTime == null || !recoverTime.IsInt)
            return;

        if (ME.user.Query<int>("ap") >= ME.user.Query<int>("max_ap"))
        {
            // 结束倒计时
            mEnableCountDown = false;
            return;
        }

        // 当前的时间
        int currentTime = TimeMgr.GetServerTime();

        // 恢复的时间间隔
        int timeSpace = CALC_RECOVER_AP_INTERVAL.Call(ME.user);

        // 计算距离下次恢复疲劳剩余的时间
        mRemainTime = timeSpace - (currentTime - recoverTime.AsInt);
        if (mRemainTime < 0)
            return;

        // 开启倒计时
        mEnableCountDown = true;
    }

    /// <summary>
    /// 倒计时
    /// </summary>
    void CountDown()
    {
        if (mRemainTime < 0)
        {
            // 结束调用本方法
            mEnableCountDown = false;

            return;
        }

        if (mTimerLabel != null)
            mTimerLabel.text = TimeMgr.ConvertTime(mRemainTime);

        mRemainTime--;
    }

    /// <summary>
    ///显示购买入场券按钮点击事件
    /// </summary>
    void OnClickAddBtn(GameObject go)
    {
        // 通关兰达平原普通所有副本
        if (! GuideMgr.IsGuided(4))
        {
            DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

            return;
        }

        // 打开快捷购买窗口
        GameObject wnd = WindowMgr.OpenWnd(QuickMarketWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;
        wnd.GetComponent<QuickMarketWnd>().Bind(ShopConfig.AP_GROUP);
    }

    /// <summary>
    ///显示计时器按钮点击事件
    /// </summary>
    void OnClickBgBtn(GameObject go)
    {
        if (mTimerLabel.gameObject.activeSelf)
            mTimerLabel.gameObject.SetActive(false);
        else
            mTimerLabel.gameObject.SetActive(true);
    }

    /// <summary>
    ///刷新入场券数量
    /// </summary>
    void RedarwArenaAmount()
    {
        //玩家对象不存在;
        if (ME.user == null)
            return;

        if (mValue == null)
            return;

        arenaAmount = ME.user.Query<int>("ap");

        int maxAp = ME.user.Query<int>("max_ap");

        // 获取玩家竞技场入场券;
        mValue.text = string.Format("{0}/{1}", arenaAmount, maxAp);
    }

    /// <summary>
    ///入场券变化回调
    /// </summary>
    void OnArenaChange(object param, params object[] paramEx)
    {
        //延迟刷新;
        MergeExecuteMgr.DispatchExecute(RedarwArenaAmount);
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDisable()
    {
        // 没有关注的属性
        if (string.IsNullOrEmpty(eventName))
            return;

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 移除属性字段关注回调
        ME.user.dbase.RemoveTriggerField(eventName);
        ME.user.dbase.RemoveTriggerField(eventName + "last_recover_ap_time");
    }
}
