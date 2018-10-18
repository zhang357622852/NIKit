/// <summary>
/// StarUpChallengeWnd.cs
/// Created by zhaozy 2018/05/30
/// 小魔女的升星大挑战活动
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class StarUpChallengeWnd : WindowBase<StarUpChallengeWnd>
{
    #region 成员变量

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    public UILabel mCountDown;

    // 活动标题
    public UILabel mPreTitle;
    public UILabel mTitle;

    // 活动描述
    public UILabel mDesc;

    public UILabel mTips1;
    public UILabel mTips2;
    public UILabel mTips3;

    public ActivityTaskWnd[] mActivityTaskWnd;

    // 活动数据
    private LPCMapping mActivityData = LPCMapping.Empty;

    /// <summary>
    /// The m activity cookie.
    /// </summary>
    private string mActivityCookie = string.Empty;

    #endregion

    void Start()
    {
        // 初始化窗口
        InitWnd();

        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();

        TweenScale mTweenScale = GetComponent<TweenScale>();

        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("StarUpChallengeWnd");

        MsgMgr.RemoveDoneHook("MSG_RECEIVE_SCORE_BONUS", "StarUpChallengeWnd");

        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("StarUpChallengeWnd");
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        // 初始化本地化文本
        mTips1.text = LocalizationMgr.Get("StarUpChallengeWnd_1");
        mTips2.text = LocalizationMgr.Get("StarUpChallengeWnd_2");
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        // 关注玩家
        ME.user.dbase.RemoveTriggerField("StarUpChallengeWnd");
        ME.user.dbase.RegisterTriggerField("StarUpChallengeWnd",
            new string[] {"activity_data"}, new CallBack (OnActivityChanged));
    }

    /// <summary>
    /// 活动字段变化事件回调
    /// </summary>
    void OnActivityChanged(object para, params object[] _params)
    {
        // 刷新数据
        RefreshData();

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 没有活动数据
        if (mActivityData == null ||
            mActivityData.Count == 0)
            return;

        // 获取活动id
        string activityId = mActivityData.GetValue<string>("activity_id");

        // 绘制活动Title信息
        mPreTitle.text = ActivityMgr.GetActivityPreTitle(activityId, LPCMapping.Empty);
        mTitle.text = ActivityMgr.GetActivityTitle(activityId, LPCMapping.Empty);

        // 设置活动描述
        mDesc.text = ActivityMgr.GetActivityDesc(activityId);

        // 活动任务列表
        List<CsvRow> taskList = ActivityMgr.GetActivityTaskList(activityId);
        if (taskList == null)
            taskList = new List<CsvRow>();

        // 显示子活动描述，奖励等数据
        for (int i = 0; i < taskList.Count; i++)
        {
            CsvRow row = taskList[i];
            if (row == null)
                continue;

            // 控件不够
            if (mActivityTaskWnd.Length <= i)
                continue;

            // 绑定到各个任务窗口上
            mActivityTaskWnd[i].Bind(ME.user, mActivityCookie, activityId, row);
        }

        // 活动开启时间
        mCountDown.text = ActivityMgr.GetActivityTimeDesc(activityId,
            mActivityData.GetValue<LPCArray>("valid_period"));

        // 奖励领取提示
        mTips3.text = ActivityMgr.GetActivityBonusTipDesc(activityId, mActivityData);
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    void RefreshData()
    {
        // 玩家对象已经不存在
        if (ME.user == null)
            return;

        // 玩家活动数据
        LPCValue activity = ME.user.Query<LPCValue>("activity_data");
        if (activity == null || !activity.IsMapping)
            return;

        // 转换数据格式
        LPCMapping activityData = activity.AsMapping;

        // 获取当前活动数据
        if (!activityData.ContainsKey(mActivityCookie))
            return;

        // 重置数据
        mActivityData.Append(activityData.GetValue<LPCMapping>(mActivityCookie));
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping activityInfo)
    {
        // 数据格式不正确
        if (activityInfo == null)
            return;

        // 重置数据
        mActivityData = LPCMapping.Empty;
        mActivityData.Append(activityInfo);

        // 获取活动cookie
        mActivityCookie = mActivityData.GetValue<string>("cookie");

        // 刷新数据
        RefreshData();

        // 重绘窗口
        Redraw();
    }
}
