/// <summary>
/// LegendContractWnd.cs
/// Created by fengsc 2018/09/18
/// 传说之契约任务界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class LegendContractWnd : WindowBase<LegendContractWnd>
{
    #region 成员变量

    // 剩余时间
    public UILabel mRemainTime;

    // 标题
    public UILabel mTitle;

    public UILabel mTips1;
    public UILabel mTips2;
    public UILabel mTips3;

    public UILabel[] mScoreTips;

    // 描述
    public UILabel[] mDesc;

    // 积分
    public UILabel[] mScore;

    // 每一组的总积分
    public UILabel[] mGroupScore;

    public UILabel mTotalScore;

    // 积分滑动条
    public UISlider mScoreSlider;

    public UILabel[] mSliderScore;

    // 领取按钮
    public GameObject[] mReceiveBtn;
    public UILabel[] mReceiveLb;
    public GameObject[] mReceiveMask;

    // 奖励物品
    public SignItemWnd[] mRewardItem;

    public GameObject mCloseBtn;

    public UILabel mReceiveTips;

    private Property mPropOb;

    // 活动数据
    private LPCMapping mActivityData = LPCMapping.Empty;

    // cookie
    private string mActivityCookie;

    private bool mIsClickReceive = false;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 初始化本地化文本
        InitLocalText();

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
        // 析构物件对象
        if (mPropOb != null)
            mPropOb.Destroy();

        MsgMgr.RemoveDoneHook("MSG_RECEIVE_SCORE_BONUS", "LegendContractWnd");

        MsgMgr.RemoveDoneHook("MSG_LOGIN_NOTIFY_OK", "LegendContractWnd");

        // 取消调用
        CancelInvoke("RefreshCloseTime");

        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("LegendContractWnd");
    }

    /// <summary>
    /// 初始化本地文本
    /// </summary>
    void InitLocalText()
    {
        mTips2.text = LocalizationMgr.Get("LegendContractWnd_0");
        mTips3.text = LocalizationMgr.Get("LegendContractWnd_1");
        mScoreTips[0].text = LocalizationMgr.Get("LegendContractWnd_2");
        mScoreTips[1].text = LocalizationMgr.Get("LegendContractWnd_3");
        mScoreTips[2].text = LocalizationMgr.Get("LegendContractWnd_4");
        mScoreTips[3].text = LocalizationMgr.Get("LegendContractWnd_5");
        mScoreTips[4].text = LocalizationMgr.Get("LegendContractWnd_6");
        mScoreTips[5].text = LocalizationMgr.Get("LegendContractWnd_7");
        mScoreTips[6].text = LocalizationMgr.Get("LegendContractWnd_8");
        mScoreTips[7].text = LocalizationMgr.Get("LegendContractWnd_9");
    }

    /// <summary>
    /// 刷新活动的关闭时间
    /// </summary>
    void RefreshCloseTime()
    {
        // 没有活动数据
        if (mActivityData == null ||
            mActivityData.Count == 0)
            return;

        string activityId = mActivityData.GetValue<string>("activity_id");

        // 活动关闭时间
        int closeTime = ActivityMgr.GetActivityCloseTime(activityId);

        // 剩余时间
        int remainTime = Mathf.Max(closeTime - TimeMgr.GetServerTime(), 0);

        int day = remainTime / 86400;

        int hour = (remainTime - 86400 * day) / 3600;

        int minute = (remainTime - 86400 * day - hour * 3600) / 60;

        // 活动剩余时间
        mRemainTime.text = string.Format(LocalizationMgr.Get("LegendContractWnd_15"), day, hour, minute);
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

        // 活动描述
        mTips1.text = ActivityMgr.GetActivityDesc(activityId);

        // 奖励领取提示
        mReceiveTips.text = ActivityMgr.GetActivityBonusTipDesc(activityId, mActivityData);

        // 每分钟刷新一次
        CancelInvoke("RefreshCloseTime");
        InvokeRepeating("RefreshCloseTime", 0, 60);

        mTitle.text = ActivityMgr.GetActivityTitle(activityId, LPCMapping.Empty);

        // 活动任务列表
        List<CsvRow> taskList = ActivityMgr.GetActivityTaskList(activityId);
        for (int i = 0; i < taskList.Count; i++)
        {
            CsvRow row = taskList[i];
            if (row == null)
                continue;

            if (i + 1 > mDesc.Length)
                continue;

            // 任务活动描述
            mDesc[i].text = ActivityMgr.GetActivityTaskDesc(ME.user, mActivityCookie, row.Query<int>("task_id"));

            LPCMapping bonusArgs = row.Query<LPCMapping>("bonus_args");
            if (bonusArgs == null)
                continue;

            mScore[i].text = bonusArgs.GetValue<int>("score").ToString();
        }

        for (int i = 1; i <= mGroupScore.Length; i++)
        {
            // 初始化
            mGroupScore[i - 1].text = string.Empty;

            // 获取分组积分
            int scroe =  mActivityData.GetValue<int>(i);

            List<CsvRow> groupList = ActivityMgr.GetActivityTaskListByGroup(activityId, i);
            if (groupList.Count == 0)
                continue;

            LPCMapping taskDbase = groupList[0].Query<LPCMapping>("dbase");

            // 分组积分显示： 当前积分/积分上限
            mGroupScore[i - 1].text = string.Format("{0}/{1}", scroe, taskDbase.GetValue<int>("max_score"));
        }

        int totalScore = mActivityData.GetValue<int>("score");

        mTotalScore.text = totalScore.ToString();

        // 活动配置数据
        CsvRow config = ActivityMgr.ActivityCsv.FindByKey(activityId);
        if (config != null)
        {
            LPCMapping dbase = config.Query<LPCMapping>("dbase");

            // 进度条显示
            mScoreSlider.value = totalScore / (float) dbase.GetValue<int>("max_score");
        }

        LPCMapping bonusData = LPCMapping.Empty;

        List<LPCMapping> bonus = new List<LPCMapping>();

        // 获取活动奖励数据
        LPCValue v = ActivityMgr.GetActivityBonus(mActivityData);
        if (v != null && v.IsMapping)
            bonusData = v.AsMapping;

        int j = 0;
        foreach (int key in bonusData.Keys)
        {
            // 刷新按钮状态
            RereshReceiveBtn(j, key, totalScore);

            if (j + 1 > mSliderScore.Length)
                continue;

            mSliderScore[j].text = key.ToString();

            foreach (LPCValue item in bonusData.GetValue<LPCArray>(key).Values)
                bonus.Add(item.AsMapping);

            j++;
        }

        // 绑定数据
        for (int i = 0; i < bonus.Count; i++)
        {
            if (i + 1 > mRewardItem.Length)
                continue;

            mRewardItem[i].Bind(bonus[i], "", false, false, -1, "small_icon_bg");
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
    /// 刷新领取按钮
    /// </summary>
    void RereshReceiveBtn(int index, int score, int totalScore)
    {
        if (ActivityMgr.ActivityBonusIsReceived(ME.user, mActivityCookie, score))
        {
            // 已经领取
            mReceiveLb[index].text = LocalizationMgr.Get("LegendContractWnd_12");

            if (!mReceiveMask[index].activeSelf)
                mReceiveMask[index].gameObject.SetActive(true);
        }
        else
        {
            if (score > totalScore)
            {
                // 条件不满足
                if (!mReceiveMask[index].activeSelf)
                    mReceiveMask[index].gameObject.SetActive(true);

                mReceiveLb[index].text = string.Format(LocalizationMgr.Get("LegendContractWnd_13"), score);
            }
            else
            {
                // 奖励可以领取
                if (mReceiveMask[index].activeSelf)
                    mReceiveMask[index].gameObject.SetActive(false);

                mReceiveLb[index].text = LocalizationMgr.Get("LegendContractWnd_14");

                // 注册按钮点击事件
                UIEventListener.Get(mReceiveBtn[index]).onClick = OnClickReceiveBtn;

                // 绑定数据
                mReceiveBtn[index].GetComponent<UIEventListener>().parameter = score;
            }
        }
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        for (int i = 0; i < mRewardItem.Length; i++)
            UIEventListener.Get(mRewardItem[i].gameObject).onClick = OnClickItemWnd;

        // 关注玩家
        ME.user.dbase.RemoveTriggerField("LegendContractWnd");
        ME.user.dbase.RegisterTriggerField("LegendContractWnd", new string[] {"activity_data"}, new CallBack (OnActivityChanged));

        // 监听领取奖励成功消息
        MsgMgr.RegisterDoneHook("MSG_RECEIVE_SCORE_BONUS", "LegendContractWnd", OnReceiveScoreBonusMsg);

        MsgMgr.RegisterDoneHook("MSG_LOGIN_NOTIFY_OK", "LegendContractWnd", WhenLoginOk);
    }

    void WhenLoginOk(string cmd, LPCValue para)
    {
        mIsClickReceive = false;
    }

    void OnReceiveScoreBonusMsg(string cmd, LPCValue para)
    {
        mIsClickReceive = false;

        DialogMgr.ShowSingleBtnDailog(
            null,
            LocalizationMgr.Get("LegendContractWnd_10"),
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

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 领取按钮点击事件
    /// </summary>
    void OnClickReceiveBtn(GameObject go)
    {
        if (mIsClickReceive)
            return;

        int score = (int) go.GetComponent<UIEventListener>().parameter;

        // 奖励已经领取
        if (ActivityMgr.ActivityBonusIsReceived(ME.user, mActivityCookie, score))
            return;

        // 条件不满足
        if (score > mActivityData.GetValue<int>("score"))
            return;

        mIsClickReceive = true;

        // 领取任务奖励
        ActivityMgr.ReceiveActivityBonus(mActivityData.GetValue<string>("cookie"), LPCValue.Create(score));
    }

    /// <summary>
    /// 奖励格子点击事件
    /// </summary>
    void OnClickItemWnd(GameObject go)
    {
        // 获取奖励数据
        LPCMapping itemData = go.GetComponent<SignItemWnd>().mData;
        if (itemData == null)
            return;

        if (itemData.ContainsKey("class_id"))
        {
            int classId = itemData.GetValue<int>("class_id");

            // 构造参数
            LPCMapping dbase = LPCMapping.Empty;

            dbase.Append(itemData);
            dbase.Add("rid", Rid.New());

            // 克隆物件对象
            if (mPropOb != null)
                mPropOb.Destroy();

            mPropOb = PropertyMgr.CreateProperty(dbase);

            if (MonsterMgr.IsMonster(classId))
            {
                // 显示宠物悬浮窗口
                GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();

                script.Bind(mPropOb);
                script.ShowBtn(true, false, false);
            }
            else if (EquipMgr.IsEquipment(classId))
            {
                GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

                script.SetEquipData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

                script.SetMask(true);
            }
            else
            {
                GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

                script.SetPropData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

                script.SetMask(true);
            }
        }
        else
        {
            string fields = FieldsMgr.GetFieldInMapping(itemData);

            int classId = FieldsMgr.GetClassIdByAttrib(fields);

            // 构造参数
            LPCMapping dbase = LPCMapping.Empty;
            dbase.Add("class_id", classId);
            dbase.Add("amount", itemData.GetValue<int>(fields));
            dbase.Add("rid", Rid.New());

            if (mPropOb != null)
                mPropOb.Destroy();

            mPropOb = PropertyMgr.CreateProperty(dbase);

            GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return;

            RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

            script.SetPropData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

            script.SetMask(true);
        }
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
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
