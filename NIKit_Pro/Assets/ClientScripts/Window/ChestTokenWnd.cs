/// <summary>
/// ChestTokenWnd.cs
/// Created by zhaozy 2018/05/30
/// 进击宝箱怪的呼啦啦代币活动
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ChestTokenWnd : WindowBase<ChestTokenWnd>
{
    #region 成员变量

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    public UILabel mActivityTime;

    public UILabel mPreTitle;

    // 活动标题
    public UILabel mTitle;

    // 活动描述
    public UILabel mDesc;

    public UILabel mTips1;
    public UILabel mTips2;
    public UILabel mTips3;
    public UILabel mTips4;
    public UILabel mTips5;

    public UILabel mGetTips1;
    public UILabel mGetTips2;

    // 子活动描述
    public UILabel[] mSubDesc;

    // 子活动奖励
    public UILabel[] mSubBonus;

    // 奖励列表
    public ChestTokenBonusItem[] mChestTokenBonusWnds;

    // 获得的总积分
    public UILabel mTotalScore;

    // 活动数据
    private LPCMapping mActivityData = LPCMapping.Empty;

    /// <summary>
    /// The m activity cookie.
    /// </summary>
    private string mActivityCookie = string.Empty;

    #endregion

    void Start()
    {
        // 注册事件
        RegisterEvent();

        // 初始化窗口
        InitWnd();

        // 绘制窗口
        Redraw();

        TweenScale mTweenScale = GetComponent<TweenScale>();

        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("ChestTokenWnd");

        // 取消消息关注
        MsgMgr.RemoveDoneHook("MSG_RECEIVE_SCORE_BONUS", "ChestTokenWnd");

        // 移除字段监听
        if (ME.user != null)
            ME.user.dbase.RemoveTriggerField("ChestTokenWnd");
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        mGetTips1.text = LocalizationMgr.Get("ChestTokenWnd_2");
        mGetTips2.text = LocalizationMgr.Get("ChestTokenWnd_3");

        // 初始化本地化文本
        mTips1.text = LocalizationMgr.Get("ChestTokenWnd_1");
        mTips2.text = LocalizationMgr.Get("ChestTokenWnd_4");
        mTips3.text = LocalizationMgr.Get("ChestTokenWnd_5");
        mTips4.text = LocalizationMgr.Get("ChestTokenWnd_6");
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
        ME.user.dbase.RemoveTriggerField("ChestTokenWnd");
        ME.user.dbase.RegisterTriggerField("ChestTokenWnd",
            new string[] {"activity_data"}, new CallBack (OnActivityChanged));

        // 监听领取奖励成功消息
        MsgMgr.RegisterDoneHook("MSG_RECEIVE_SCORE_BONUS", "ChestTokenWnd", OnReceiveScoreBonusMsg);
    }

    /// <summary>
    /// Raises the receive score bonus message event.
    /// </summary>
    /// <param name="cmd">Cmd.</param>
    /// <param name="para">Para.</param>
    void OnReceiveScoreBonusMsg(string cmd, LPCValue para)
    {
        DialogMgr.ShowSingleBtnDailog(
            null,
            LocalizationMgr.Get("ChestTokenWnd_7"),
            string.Empty,
            string.Empty,
            true,
            this.transform
        );
    }

    /// <summary>
    /// 活动字段变化事件回调
    /// </summary>
    void OnActivityChanged(object para, params object[] _params)
    {
        // 刷新数据
        RefreshData();

        // 刷新窗口
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
            // 类型不符合要求
            CsvRow row = taskList[i];
            if (row == null)
                continue;

            // 子活动描述
            int taskId = row.Query<int>("task_id");
            mSubDesc[i].text = ActivityMgr.GetActivityTaskDesc(ME.user, mActivityCookie, taskId);

            // 任务活动奖励
            foreach(LPCValue tBonus in ActivityMgr.GetActivityTaskBonus(taskId).Values)
            {
                // 转换数据格式
                LPCMapping taskBonus = tBonus.AsMapping;
                string fields = FieldsMgr.GetFieldInMapping(taskBonus);
                mSubBonus[i].text = taskBonus.GetValue<int>(fields).ToString();
            }
        }

        // 显示总积分
        int score = mActivityData.GetValue<int>("score");
        mTotalScore.text = score.ToString();

        // 活动配置数据
        CsvRow config = ActivityMgr.ActivityCsv.FindByKey(activityId);
        LPCArray validPeriod = mActivityData.GetValue<LPCArray>("valid_period");

        // 活动开启时间
        mActivityTime.text = ActivityMgr.GetActivityTimeDesc(config.Query<string>("activity_id"), validPeriod);

        // 奖励领取提示
        mTips5.text = ActivityMgr.GetActivityBonusTipDesc(config.Query<string>("activity_id"), mActivityData);

        // 获取活动奖励列表
        LPCValue bonus = ActivityMgr.GetActivityBonus(mActivityData);
        LPCArray bonusList = LPCArray.Empty;
        if (bonus != null)
            bonusList = bonus.AsArray;

        // 填充奖励领取数据
        for (int i = 0; i < bonusList.Count; i++)
        {
            // 控件不够
            if (i >= mChestTokenBonusWnds.Length ||
                mChestTokenBonusWnds[i] == null)
                continue;

            // 绑定数据
            mChestTokenBonusWnds[i].Bind(i, bonusList[i].AsMapping, mActivityData);
        }
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
