/// <summary>
/// TrialActivityWnd.cs
/// Created by fengsc 2018/03/21
/// 伽美斯·奎因的试炼活动窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class TrialActivityWnd : WindowBase<TrialActivityWnd>
{
    #region 成员变量

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    public UILabel mTips1;

    public UILabel mCountDown;

    // 活动标题
    public UILabel mTitle;

    // 活动描述
    public UILabel mDesc;

    public UILabel mTips2;

    public UILabel mGetTips1;
    public UILabel mGetTips2;
    public UILabel mGetTips3;

    // 子活动名称
    public UILabel[] mSubName;

    // 子活动描述
    public UILabel[] mSubDesc;

    // 子活动奖励
    public UILabel[] mSubBonus;

    public UILabel mTips3;

    public UILabel mTips4;

    // 获得的总积分
    public UILabel mTotalScore;

    // 积分滑动条
    public UISlider mScoreSlider;

    public UILabel[] mSliderScale;

    // 奖励item
    public GameObject mBonusItem;

    // 排序组件
    public UIGrid mGrid;

    public UILabel mTips5;

    // 活动数据
    private LPCMapping mActivityData = LPCMapping.Empty;

    List<GameObject> mItems = new List<GameObject>();

    // cookie
    string mActivityCookie;

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
        EventMgr.UnregisterEvent("TrialActivityWnd");

        MsgMgr.RemoveDoneHook("MSG_RECEIVE_SCORE_BONUS", "TrialActivityWnd");

        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("TrialActivityWnd");
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        // 初始化本地化文本
        mTips1.text = LocalizationMgr.Get("TrialActivityWnd_1");
        mDesc.text = LocalizationMgr.Get("TrialActivityWnd_4");
        mTips2.text = LocalizationMgr.Get("TrialActivityWnd_5");
        mGetTips1.text = LocalizationMgr.Get("TrialActivityWnd_6");
        mGetTips2.text = LocalizationMgr.Get("TrialActivityWnd_7");
        mGetTips3.text = LocalizationMgr.Get("TrialActivityWnd_8");
        mTips3.text = LocalizationMgr.Get("TrialActivityWnd_21");
        mTips4.text = LocalizationMgr.Get("TrialActivityWnd_22");
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
        ME.user.dbase.RemoveTriggerField("TrialActivityWnd");
        ME.user.dbase.RegisterTriggerField("TrialActivityWnd", new string[] {"activity_data"}, new CallBack (OnActivityChanged));

        // 监听领取奖励成功消息
        MsgMgr.RegisterDoneHook("MSG_RECEIVE_SCORE_BONUS", "TrialActivityWnd", OnReceiveScoreBonusMsg);
    }

    void OnReceiveScoreBonusMsg(string cmd, LPCValue para)
    {
        DialogMgr.ShowSingleBtnDailog(
            null,
            LocalizationMgr.Get("TrialActivityWnd_27"),
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

        // 绘制窗口
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

        string activityId = mActivityData.GetValue<string>("activity_id");

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

            // 子活动名称
            mSubName[i].text = LocalizationMgr.Get(row.Query<string>("name"));

            // 子活动描述
            int taskId = row.Query<int>("task_id");
            mSubDesc[i].text = ActivityMgr.GetActivityTaskDesc(ME.user, mActivityCookie, taskId);

            // 任务活动奖励
            foreach(LPCValue tBonus in ActivityMgr.GetActivityTaskBonus(row.Query<int>("task_id")).Values)
            {
                // 转换数据格式
                LPCMapping taskBonus = tBonus.AsMapping;
                string fields = FieldsMgr.GetFieldInMapping(taskBonus);
                mSubBonus[i].text = taskBonus.GetValue<int>(fields).ToString();
            }
        }

        // 当前积分
        int score = mActivityData.GetValue<int>("score");

        // 显示总积分
        mTotalScore.text = score.ToString();

        mTitle.text = ActivityMgr.GetActivityTitle(activityId, LPCMapping.Empty);

        // 活动配置数据
        CsvRow config = ActivityMgr.ActivityCsv.FindByKey(activityId);

        LPCMapping dbase = config.Query<LPCMapping>("dbase");

        // 进度条显示
        mScoreSlider.value = score / (float) dbase.GetValue<int>("max_score");

        LPCArray validPeriod = mActivityData.GetValue<LPCArray>("valid_period");

        // 活动开启时间
        mCountDown.text = ActivityMgr.GetActivityTimeDesc(activityId, validPeriod);

        // 奖励领取提示
        mTips5.text = ActivityMgr.GetActivityBonusTipDesc(activityId, mActivityData);

        // 获取活动奖励列表
        LPCValue v = ActivityMgr.GetActivityBonus(mActivityData);

        LPCMapping bonus = LPCMapping.Empty;
        if (v != null)
            bonus = v.AsMapping;

        int amount = bonus.Count - mItems.Count;
        if (amount >= 0)
        {
            for (int i = 0; i < amount; i++)
            {
                GameObject item = Instantiate(mBonusItem);

                // 设置父级
                item.transform.SetParent(mGrid.transform);

                // 初始坐标
                item.transform.localPosition = Vector3.zero;

                // 初始化大小
                item.transform.localScale = Vector3.one;

                // 缓存到列表中
                mItems.Add(item);
            }
        }
        else
        {
            for (int i = Mathf.Abs(amount); i < mItems.Count; i++)
                mItems[i].SetActive(false);
        }

        int j = 0;
        foreach (int key in bonus.Keys)
        {
            mSliderScale[j].text = key.ToString();

            GameObject item = mItems[j];

            // 激活item
            item.SetActive(true);

            j++;

            // 脚本不存在
            TrialBonusItem script = item.GetComponent<TrialBonusItem>();
            if (script == null)
                continue;

            // 绑定数据
            script.Bind(bonus[key].AsArray, key, mActivityData);
        }

        // 排序子物体
        mGrid.Reposition();
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
    }
}
