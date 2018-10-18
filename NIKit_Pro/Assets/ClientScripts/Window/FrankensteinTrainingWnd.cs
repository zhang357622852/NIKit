/// <summary>
/// FrankensteinTrainingWnd.cs
/// Created by zhangwm 2018/09/18
/// 科学怪人的秘密特训活动
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class FrankensteinTrainingWnd : WindowBase<FrankensteinTrainingWnd>
{
    #region 成员变量

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    // 活动时间
    public UILabel mActivityTime;

    // 活动标题 科学怪人的秘密特训
    public UILabel mTitle;

    // 活动描述
    public UILabel mDesc;

    // 如何获得训练点
    public UILabel mTips1;

    // 点击图标可查看道具说明，兑换成功后将发送至您的邮箱
    public UILabel mTips2;

    // 训练点可在神秘研究所兑换道具
    public UILabel mTips3;

    // 兑换次数还可领取更多的额外奖励
    public UILabel mTips4;

    // 请在活动结束后24小时内尽快领取奖励，否则奖励将会消失
    public UILabel mTips5;

    // 训练点获取方式
    public UILabel mGetTips1;

    // 训练点数
    public UILabel mGetTips2;

    // 子活动描述 竞技场战斗胜利/失败 获得1~3★装备 获得4★装备 获得5★装备 获得6★装备
    public UILabel[] mSubDesc;

    // 子活动奖励 3 3
    public UILabel[] mSubBonus;

    // 奖励列表
    public FrankensteinTrainingBonusItem[] mFrankensteinTrainingBonusWnds;

    // 额外奖励列表
    public FrankensteinTrainingExtraBonusItem[] mFrankensteinTrainingExtraBonusWnds;

    // 获得的总积分
    public UILabel mTotalScore;

    // 目前已兑换0/8次道具
    public UILabel mTotalExchangeTimes;

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
        EventMgr.UnregisterEvent("FrankensteinTrainingWnd");

        // 取消消息关注
        MsgMgr.RemoveDoneHook("MSG_RECEIVE_SCORE_BONUS", "FrankensteinTrainingWnd");

        // 移除字段监听
        if (ME.user != null)
            ME.user.dbase.RemoveTriggerField("FrankensteinTrainingWnd");
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        mGetTips1.text = LocalizationMgr.Get("FrankensteinTrainingWnd_9");
        mGetTips2.text = LocalizationMgr.Get("FrankensteinTrainingWnd_10");

        // 初始化本地化文本
        mTips1.text = LocalizationMgr.Get("FrankensteinTrainingWnd_5");
        mTips2.text = LocalizationMgr.Get("ChestTokenWnd_4");
        mTips3.text = LocalizationMgr.Get("FrankensteinTrainingWnd_6");
        mTips4.text = LocalizationMgr.Get("FrankensteinTrainingWnd_7");
        mTips5.text = LocalizationMgr.Get("FrankensteinTrainingWnd_8");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        // 关注玩家
        ME.user.dbase.RemoveTriggerField("FrankensteinTrainingWnd");

        ME.user.dbase.RegisterTriggerField("FrankensteinTrainingWnd",
            new string[] { "activity_data" }, new CallBack(OnActivityChanged));

        // 监听领取奖励成功消息
        MsgMgr.RegisterDoneHook("MSG_RECEIVE_SCORE_BONUS", "FrankensteinTrainingWnd", OnReceiveScoreBonusMsg);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 没有活动数据
        if (mActivityData == null || mActivityData.Count == 0)
            return;

        // 获取活动id
        string activityId = mActivityData.GetValue<string>("activity_id");

        // 活动配置数据
        LPCArray validPeriod = mActivityData.GetValue<LPCArray>("valid_period");

        // 活动开启时间
        mActivityTime.text = ActivityMgr.GetActivityTimeDesc(activityId, validPeriod);

        // 绘制活动Title信息
        mTitle.text = ActivityMgr.GetActivityTitle(activityId, LPCMapping.Empty);

        // 设置活动描述
        mDesc.text = "　　" + ActivityMgr.GetActivityDesc(activityId);

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
            foreach (LPCValue tBonus in ActivityMgr.GetActivityTaskBonus(taskId).Values)
            {
                // 转换数据格式
                LPCMapping taskBonus = tBonus.AsMapping;
                string fields = FieldsMgr.GetFieldInMapping(taskBonus);
                mSubBonus[i].text = taskBonus.GetValue<int>(fields).ToString();
            }
        }

        // 显示总积分
        int score = mActivityData.GetValue<int>("score");

        int maxScore = ActivityMgr.GetActivityDbaseMaxScore(activityId);

        mTotalScore.text = string.Format(LocalizationMgr.Get("FrankensteinTrainingWnd_1"), score, maxScore);

        // 获取活动奖励列表
        LPCValue bonus = ActivityMgr.GetActivityBonus(mActivityData);

        LPCArray bonusList = LPCArray.Empty;

        if (bonus != null)
            bonusList = bonus.AsArray;

        // 最大兑换次数
        int maxExchangeTimes = 0;

        // 填充奖励领取数据
        for (int i = 0; i < bonusList.Count; i++)
        {
            // 控件不够
            if (i >= mFrankensteinTrainingBonusWnds.Length ||
                mFrankensteinTrainingBonusWnds[i] == null)
                continue;

            // 绑定数据
            mFrankensteinTrainingBonusWnds[i].Bind(i, bonusList[i].AsMapping, mActivityData);

            maxExchangeTimes++;
        }

        // 获取额外活动奖励列表
        LPCValue extraBonus = ActivityMgr.GetActivityExtraBonus(mActivityData);

        LPCMapping bonusMap = LPCMapping.Empty;

        if (extraBonus != null)
            bonusMap = extraBonus.AsMapping;

        int index = 0;

        // 填充奖励领取数据
        foreach (int key in bonusMap.Keys)
        {
            // 控件不够
            if (index >= mFrankensteinTrainingExtraBonusWnds.Length ||
                mFrankensteinTrainingExtraBonusWnds[index] == null)
                continue;

            // 绑定数据
            mFrankensteinTrainingExtraBonusWnds[index].Bind(key, bonusMap[key].AsArray, mActivityData);

            index++;
        }

        // 目前已兑换0/8次道具
        int exchangeTimes = mActivityData.GetValue<int>("exchange_times");

        mTotalExchangeTimes.text = string.Format(LocalizationMgr.Get("FrankensteinTrainingWnd_11"), exchangeTimes, maxExchangeTimes);
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
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
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
