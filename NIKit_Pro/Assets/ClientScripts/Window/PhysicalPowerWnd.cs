/// <summary>
/// PhysicalPowerWnd.cs
/// Created by fengsc 2016/07/21
///玩家体力值显示窗口
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class PhysicalPowerWnd : WindowBase<PhysicalPowerWnd>
{
    #region 成员变量

    /// <summary>
    ///打开购买体力窗口按钮
    /// </summary>
    public GameObject mAddBtn;

    /// <summary>
    ///点击背景显示倒计时
    /// </summary>
    public GameObject mBgBtn;

    /// <summary>
    ///显示体力值
    /// </summary>
    public UILabel mValue;

    /// <summary>
    ///显示计时器
    /// </summary>
    public UILabel mTimerLabel;

    /// <summary>
    ///体力值
    /// </summary>
    int mPower = 0;

    // 疲劳恢复的剩余时间
    int mRemainTime = 0;

    string eventName = string.Empty;

    bool mIsCountDown = false;
    float mLastTime = 0;

    #endregion

    // Use this for initialization
    void Start()
    {
        if (mBgBtn != null && mAddBtn != null)
        {
            UIEventListener.Get(mBgBtn).onClick = OnClickBgBtn;
            UIEventListener.Get(mAddBtn).onClick = OnClickAddBtn;
        }

        //默认不显示计时器;
        mTimerLabel.gameObject.SetActive(false);
    }

    void Update()
    {
        if (mIsCountDown)
        {
            if (Time.realtimeSinceStartup > mLastTime + 1)
            {
                mLastTime = Time.realtimeSinceStartup;
                CountDown();
            }
        }
    }

    void OnEnable()
    {
        //注册事件
        RegisterEvent();

        StartUpCountDown();

        //初始化体力值;
        RedrawPhysicalPower();
    }

    /// <summary>
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        //玩家对象不存在;
        if (ME.user == null)
            return;

        eventName = Game.GetUniqueName("PhysicalPowerWnd") + "_mp";

        // 关注字段变化
        ME.user.dbase.RegisterTriggerField(eventName, new string[] { "life", "max_life" }, new CallBack(OnPhysicalPowerChange));

        ME.user.dbase.RegisterTriggerField(eventName + "last_recover_time",
            new string[] { "last_recover_time" }, new CallBack(OnRecoverTimeChange));
    }

    /// <summary>
    ///点击显示购买窗口按钮事件
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
        wnd.GetComponent<QuickMarketWnd>().Bind(ShopConfig.LIFE_GROUP);
    }

    /// <summary>
    ///计时器显示按钮点击事件
    /// </summary>
    void OnClickBgBtn(GameObject go)
    {
        if (mTimerLabel.gameObject.activeSelf)
            mTimerLabel.gameObject.SetActive(false);
        else
            mTimerLabel.gameObject.SetActive(true);
    }

    /// <summary>
    /// 启动倒计时功能
    /// </summary>
    void StartUpCountDown()
    {

        if (mTimerLabel != null)
        {
            mTimerLabel.text = TimeMgr.ConvertTime(0);
        }

        if(ME.user == null)
            return;

        // 上一次疲劳恢复的时间
        LPCValue recoverTime = ME.user.Query<LPCValue>("last_recover_time");
        if (recoverTime == null || !recoverTime.IsInt)
            return;

        if (ME.user.Query<int>("life") >= ME.user.Query<int>("max_life"))
        {
            // 结束倒计时
            mIsCountDown = false;
            return;
        }

        // 当前的时间
        int currentTime = TimeMgr.GetServerTime();

        // 疲劳恢复的时间间隔
        int timeSpace = CALC_RECOVER_LIFE_INTERVAL.Call(ME.user);

        // 计算距离下次恢复疲劳剩余的时间
        mRemainTime = timeSpace - (currentTime - recoverTime.AsInt);
        if (mRemainTime < 0)
            return;

        // 开启倒计时
        mIsCountDown = true;
    }

    /// <summary>
    /// 体力恢复倒计时
    /// </summary>
    void CountDown()
    {
        if (mRemainTime < 0)
        {
            // 结束调用本方法
            mIsCountDown = false;

            return;
        }

        mTimerLabel.text = TimeMgr.ConvertTime(mRemainTime);

        mRemainTime--;
    }

    /// <summary>
    ///刷新体力值
    /// </summary>
    void RedrawPhysicalPower()
    {
        // 玩家对象不存在;
        if (ME.user == null)
            return;

        // 获取玩家当前体力值;
        mPower = ME.user.Query<int>("life");

        // 获取玩家体力值上限;
        int maxPower = ME.user.Query<int>("max_life");

        if (mValue == null)
            return;
        mValue.text = string.Format("{0}{1}{2}", mPower, "/", maxPower);
    }

    /// <summary>
    ///玩家体力值变化回调
    /// </summary>
    void OnPhysicalPowerChange(object param, params object[] paramEx)
    {
        //延迟刷新;
        MergeExecuteMgr.DispatchExecute(RedrawPhysicalPower);
    }

    /// <summary>
    /// 上次恢复的时间变化回调
    /// </summary>
    void OnRecoverTimeChange(object param, params object[] paramEx)
    {
        //延迟到下一帧调用;
        MergeExecuteMgr.DispatchExecute(StartUpCountDown);
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

        // 取消字段关注
        ME.user.dbase.RemoveTriggerField(eventName);
        ME.user.dbase.RemoveTriggerField(eventName + "last_recover_time");
    }
}
