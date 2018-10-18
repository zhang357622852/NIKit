/// <summary>
/// AccumulateScoreWnd.cs
/// Created by lic 06/07/2017
/// 累积积分活动界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class AccumulateScoreWnd : WindowBase<AccumulateScoreWnd>
{

    #region 成员变量

    public UILabel mTitle;
    public UILabel mShadowTitle;
    public UILabel mTips;
    public UILabel mTips1;
    public UILabel mTips2;
    public UILabel mTips3;
    public UILabel mTips4;
    public UILabel mTips5;
    public UILabel mTips6;
    public UILabel mTips7;
    public UILabel mTips9;

    public UILabel mExchangeTips;

    public UILabel mGetWayTips;

    public UILabel mActivityNumTips;
    public UILabel mActivityNumTips1;

    public UILabel mTips19;
    public UILabel mTips20;
    public UILabel mTips21;

    public UILabel mScoreTips;

    public UILabel mPointTips;

    // 召唤、升星使魔积分显示
    public UILabel[] mMonsterScore;
    public UILabel[] mSummonScore;

    // 竞技场排位战胜利/失败积分显示
    public UILabel mArenaWinsScore;
    public UILabel mArenaFailScore;
    public UILabel mWinTips;
    public UILabel mFailTips;
    public UILabel mVTips;
    public UILabel mSTips;

    // 地下城挑战层数提示
    public UILabel mDungeons1;
    public UILabel mDungeons2;
    public UILabel mDungeons3;
    public UILabel mDungeons4;
    public UILabel mDungeons5;
    public UILabel mDungeons6;
    public UILabel mDungeons7;
    public UILabel mDungeons8;
    public UILabel mDungeons9;
    public UILabel mDungeons10;
    public UILabel mDungeons11;
    public UILabel mDungeons12;
    public UILabel mDungeons13;
    public UILabel mDungeons14;
    public UILabel mDungeons15;
    public UILabel mDungeons16;

    // 地下城挑战层数积分显示
    public UILabel[] mDungeonsScore1;

    public UILabel[] mDungeonsScore2;

    public UILabel[] mDungeonsScore3;

    public UILabel[] mDungeonsScore4;

    // 挑战地下城提示信息
    public UILabel mDungeonsTips1;
    public UILabel mDungeonsTips2;
    public UILabel mDungeonsTips3;
    public UILabel mDungeonsTips4;
    public UILabel mDungeonsTips5;
    public UILabel mDungeonsTips6;
    public UILabel mDungeonsTips7;
    public UILabel mDungeonsTips8;

    public UILabel mDungeonsTitle1;
    public UILabel mDungeonsTitle2;
    public UILabel mDungeonsTitle3;
    public UILabel mDungeonsTitle4;

    // arena
    public GameObject mArenaPage;

    // under
    public GameObject mUnderPage;

    public GameObject mMonsterPage;

    public GameObject mSmallBonusItem;
    public GameObject mBigBonusItem;


    public GameObject mCloseBtn;

    public UILabel mStarUpBtnLb;
    public UILabel mArenaBtnLb;
    public UILabel mDungeonsBtnLb;

    public GameObject[] mToggleBtnGroup;

    // 当前积分
    public UILabel mCurrentScore;

    // 当前累计点数
    public UILabel mCurrentPoint;

    public LPCMapping ActivityInfo { get; private set; }

    Dictionary<string, GameObject> mItems = new Dictionary<string, GameObject>();

    #endregion

    #region 私有变量

    const int SCORE_BOUNUS = 1;
    const int POINT_BONUS = 2;

    int currentPage = -1;

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();

        TweenScale mTweenScale = GetComponent<TweenScale>();

        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;

        UIEventListener.Get(mToggleBtnGroup[0]).onClick = OnToggle0Btn;
        UIEventListener.Get(mToggleBtnGroup[1]).onClick = OnToggle1Btn;
        UIEventListener.Get(mToggleBtnGroup[2]).onClick = OnToggle2Btn;

        // 关注玩家
        ME.user.dbase.RemoveTriggerField("AccumulateScoreWnd");
        ME.user.dbase.RegisterTriggerField("AccumulateScoreWnd", new string[] {"activity_data"}, new CallBack (OnActivityChanged));

        // 监听领取奖励成功消息
        MsgMgr.RegisterDoneHook("MSG_RECEIVE_SCORE_BONUS", AccumulateScoreWnd.WndType, OnReceiveScoreBonusMsg);
    }

    void OnReceiveScoreBonusMsg(string cmd, LPCValue para)
    {
        LPCMapping data = para.AsMapping;

        int classId = data.GetValue<int>("class_id");

        string name = string.Empty;

        if (MonsterMgr.IsMonster(classId))
            name = MonsterMgr.GetName(classId, MonsterMgr.GetDefaultRank(classId));
        else if (EquipMgr.IsEquipment(classId))
            name = EquipMgr.GetName(classId);
        else
            name = ItemMgr.GetName(classId);

        string desc = string.Format(LocalizationMgr.Get("AccumulateScoreWnd_26"), name);

        DialogMgr.ShowSingleBtnDailog(
            null,
            desc,
            LocalizationMgr.Get("AccumulateScoreWnd_27"),
            string.Empty,
            true,
            this.transform
        );
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        mTips.text = LocalizationMgr.Get("AccumulateScoreWnd_36");
        mTips1.text = LocalizationMgr.Get("AccumulateScoreWnd_1");
        mTips2.text = LocalizationMgr.Get("AccumulateScoreWnd_2");
        mTips3.text = LocalizationMgr.Get("AccumulateScoreWnd_3");
        mTips4.text = LocalizationMgr.Get("AccumulateScoreWnd_4");
        mTips6.text = LocalizationMgr.Get("AccumulateScoreWnd_6");
        mTips7.text = LocalizationMgr.Get("AccumulateScoreWnd_7");
        mTips9.text = LocalizationMgr.Get("AccumulateScoreWnd_42");

        mExchangeTips.text = LocalizationMgr.Get("AccumulateScoreWnd_38");

        mGetWayTips.text = LocalizationMgr.Get("AccumulateScoreWnd_40");

        mActivityNumTips.text = LocalizationMgr.Get("AccumulateScoreWnd_37");
        mActivityNumTips1.text = LocalizationMgr.Get("AccumulateScoreWnd_41");

        mScoreTips.text = LocalizationMgr.Get("AccumulateScoreWnd_39");
        mPointTips.text = LocalizationMgr.Get("AccumulateScoreWnd_43");

        mTips19.text = LocalizationMgr.Get("AccumulateScoreWnd_19");
        mTips20.text = LocalizationMgr.Get("AccumulateScoreWnd_20");

        mWinTips.text = LocalizationMgr.Get("AccumulateScoreWnd_44");
        mVTips.text = LocalizationMgr.Get("AccumulateScoreWnd_45");
        mSTips.text = LocalizationMgr.Get("AccumulateScoreWnd_46");
        mFailTips.text = LocalizationMgr.Get("AccumulateScoreWnd_47");

        mBigBonusItem.SetActive(false);
        mSmallBonusItem.SetActive(false);

        mDungeons1.text = LocalizationMgr.Get("AccumulateScoreWnd_32");
        mDungeons2.text = LocalizationMgr.Get("AccumulateScoreWnd_33");
        mDungeons3.text = LocalizationMgr.Get("AccumulateScoreWnd_34");
        mDungeons4.text = LocalizationMgr.Get("AccumulateScoreWnd_35");
        mDungeons5.text = LocalizationMgr.Get("AccumulateScoreWnd_32");
        mDungeons6.text = LocalizationMgr.Get("AccumulateScoreWnd_33");
        mDungeons7.text = LocalizationMgr.Get("AccumulateScoreWnd_34");
        mDungeons8.text = LocalizationMgr.Get("AccumulateScoreWnd_35");
        mDungeons9.text = LocalizationMgr.Get("AccumulateScoreWnd_32");
        mDungeons10.text = LocalizationMgr.Get("AccumulateScoreWnd_33");
        mDungeons11.text = LocalizationMgr.Get("AccumulateScoreWnd_34");
        mDungeons12.text = LocalizationMgr.Get("AccumulateScoreWnd_35");
        mDungeons13.text = LocalizationMgr.Get("AccumulateScoreWnd_32");
        mDungeons14.text = LocalizationMgr.Get("AccumulateScoreWnd_33");
        mDungeons15.text = LocalizationMgr.Get("AccumulateScoreWnd_34");
        mDungeons16.text = LocalizationMgr.Get("AccumulateScoreWnd_35");

        mDungeonsTips1.text = LocalizationMgr.Get("AccumulateScoreWnd_13");
        mDungeonsTips2.text = LocalizationMgr.Get("AccumulateScoreWnd_14");
        mDungeonsTips3.text = LocalizationMgr.Get("AccumulateScoreWnd_15");
        mDungeonsTips4.text = LocalizationMgr.Get("AccumulateScoreWnd_16");
        mDungeonsTips5.text = LocalizationMgr.Get("AccumulateScoreWnd_17");
        mDungeonsTips6.text = LocalizationMgr.Get("AccumulateScoreWnd_18");
        mDungeonsTips7.text = LocalizationMgr.Get("AccumulateScoreWnd_52");
        mDungeonsTips8.text = LocalizationMgr.Get("AccumulateScoreWnd_53");

        mDungeonsTitle1.text = LocalizationMgr.Get("AccumulateScoreWnd_48");
        mDungeonsTitle2.text = LocalizationMgr.Get("AccumulateScoreWnd_49");
        mDungeonsTitle3.text = LocalizationMgr.Get("AccumulateScoreWnd_50");
        mDungeonsTitle4.text = LocalizationMgr.Get("AccumulateScoreWnd_51");

        mStarUpBtnLb.text = LocalizationMgr.Get("AccumulateScoreWnd_10");
        mArenaBtnLb.text = LocalizationMgr.Get("AccumulateScoreWnd_11");
        mDungeonsBtnLb.text = LocalizationMgr.Get("AccumulateScoreWnd_12");

        string title = ActivityMgr.GetActivityTitle(ActivityInfo.GetValue<string>("activity_id"));

        mTitle.text = title;

        if (mShadowTitle != null)
            mShadowTitle.text = title;

        InvokeRepeating("RefreshTime", 0, 60);

        LPCArray list = CALC_CHALLENGE_DUNGEONS_SCORE.Call();

        for (int i = 0; i < mDungeonsScore1.Length; i++)
            mDungeonsScore1[i].text = string.Format(LocalizationMgr.Get("AccumulateScoreWnd_31"), list[0].AsArray[i].AsInt + " ");

        for (int i = 0; i < mDungeonsScore2.Length; i++)
            mDungeonsScore2[i].text = string.Format(LocalizationMgr.Get("AccumulateScoreWnd_31"), list[1].AsArray[i].AsInt + " ");

        for (int i = 0; i < mDungeonsScore3.Length; i++)
            mDungeonsScore3[i].text = string.Format(LocalizationMgr.Get("AccumulateScoreWnd_31"), list[2].AsArray[i].AsInt + " ");

        for (int i = 0; i < mDungeonsScore4.Length; i++)
            mDungeonsScore4[i].text = string.Format(LocalizationMgr.Get("AccumulateScoreWnd_31"), list[3].AsArray[i].AsInt + " ");

        LPCArray starupList = CALC_STARUP_SUMMON_SCORE.Call("starup");

        LPCArray summonList = CALC_STARUP_SUMMON_SCORE.Call("summon");

        for (int i = 0; i < mMonsterScore.Length; i++)
            mMonsterScore[i].text = string.Format(LocalizationMgr.Get("AccumulateScoreWnd_31"), starupList[i].AsInt);

        for (int i = 0; i < mSummonScore.Length; i++)
            mSummonScore[i].text = string.Format(LocalizationMgr.Get("AccumulateScoreWnd_31"), summonList[i].AsInt);

        LPCMapping arenaScore = CALC_RANK_BATTLE_SCORE.Call();

        mArenaWinsScore.text = string.Format(LocalizationMgr.Get("AccumulateScoreWnd_31"), arenaScore.GetValue<int>("win_score") + " ");

        mArenaFailScore.text = string.Format(LocalizationMgr.Get("AccumulateScoreWnd_31"), arenaScore.GetValue<int>("fail_score") + " ");
    }

    void RefreshTime()
    {
        int weekDay = Game.GetWeekDay(TimeMgr.GetServerTime());

        // 获取配置信息
        LPCMapping data = ActivityMgr.GetActivityInfo(ActivityInfo.GetValue<string>("activity_id"));

        // 距离本周日零点的时间间隔
        mTips21.text = string.Format(
            LocalizationMgr.Get("AccumulateScoreWnd_21"),
            data.GetValue<LPCMapping>("dbase").GetValue<int>("weekly_accumulate_point_limit"),
            TimeMgr.ConvertTimeToChinese((int)Game.GetZeroClock(7 - (weekDay == 0 ? 7 : weekDay) + 1), false));

        LPCMapping activity = ME.user.Query<LPCMapping>("activity_data");

        if (activity == null)
            return;

        string cookie = ActivityInfo.GetValue<string>("cookie");

        // 活动不存在或已关闭
        if (!activity.ContainsKey(cookie))
            return;

        LPCMapping activityData = activity.GetValue<LPCMapping>(cookie);

        if (activityData == null)
            activityData = LPCMapping.Empty;

        int addScore = 0;

        LPCValue v = activityData.GetValue<LPCValue>("add_score");
        if (v != null && v.IsInt)
            addScore = v.AsInt;

        // 距离当天零点的时间间隔
        mTips5.text = string.Format(
            LocalizationMgr.Get("AccumulateScoreWnd_5"),
            addScore,
            data.GetValue<LPCMapping>("dbase").GetValue<int>("daily_accumulate_score_limit"),
            TimeMgr.ConvertTimeToChinese((int)Game.GetZeroClock(1), false));
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 结束调用
        CancelInvoke("RefreshTime");

        // 移除消息的监听
        MsgMgr.RemoveDoneHook("MSG_RECEIVE_SCORE_BONUS", AccumulateScoreWnd.WndType);

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 移除属性字段关注回调
        ME.user.dbase.RemoveTriggerField("AccumulateScoreWnd");
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnToggle0Btn(GameObject ob)
    {
        if (currentPage == 0)
            return;

        mArenaPage.SetActive(false);
        mMonsterPage.SetActive(true);
        mUnderPage.SetActive(false);

        currentPage = 0;
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnToggle1Btn(GameObject ob)
    {
        if (currentPage == 1)
            return;

        mArenaPage.SetActive(true);
        mMonsterPage.SetActive(false);
        mUnderPage.SetActive(false);

        currentPage = 1;
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnToggle2Btn(GameObject ob)
    {
        if (currentPage == 2)
            return;

        mArenaPage.SetActive(false);
        mMonsterPage.SetActive(false);
        mUnderPage.SetActive(true);

        currentPage = 2;
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// activity字段变化回调
    /// </summary>
    void OnActivityChanged(object para, params object[] _params)
    {
        Redraw();

        RefreshTime();
    }

    /// <summary>
    /// 重绘奖励内容
    /// </summary>
    void Redraw()
    {
        OnToggle0Btn(mToggleBtnGroup[0]);

        if(ActivityInfo == null)
            return;

        LPCMapping activity = ME.user.Query<LPCMapping>("activity_data");

        if (activity == null)
            return;

        string cookie = ActivityInfo.GetValue<string>("cookie");

        // 活动不存在或已关闭
        if (!activity.ContainsKey(cookie))
            return;

        LPCMapping activityData = activity.GetValue<LPCMapping>(cookie);

        if (activityData == null)
            return;

        mCurrentScore.text = activityData.GetValue<int>("score").ToString();

        // 获取配置信息
        LPCMapping data = ActivityMgr.GetActivityInfo(ActivityInfo.GetValue<string>("activity_id"));
        mCurrentPoint.text = string.Format("[FFFFFFFF]{0}[-][FFFFFF78]/{1}[-]",
            activityData.GetValue<int>("point").ToString(),
            data.GetValue<LPCMapping>("dbase").GetValue<int>("weekly_accumulate_point_limit"));

        LPCMapping bonus = ActivityMgr.GetActivityBonus(ActivityInfo).AsMapping;

        LPCMapping bonusData = activityData.GetValue<LPCMapping>("bonus_data");

        // 取得奖励内容
        LPCMapping scoreBonus = bonus.GetValue<LPCMapping>(SCORE_BOUNUS);
        LPCMapping pointBonus = bonus.GetValue<LPCMapping>(POINT_BONUS);

        LPCMapping scoreData = bonusData.GetValue<LPCMapping>(SCORE_BOUNUS);
        LPCMapping pointData = bonusData.GetValue<LPCMapping>(POINT_BONUS);

        // 根据key值排序
        List<int> scoreIndexList = new List<int>();
        foreach (int index in scoreBonus.Keys)
            scoreIndexList.Add(index);
        scoreIndexList.Sort();

        for (int i = 0; i < scoreIndexList.Count; i++)
        {
            string name = string.Format("score_item_{0}", i);

            if (!mItems.ContainsKey(name))
                continue;

            GameObject wnd = mItems[name];
            wnd.GetComponent<ReceiveBonusItem>().BindData(scoreBonus[scoreIndexList[i]].AsMapping, scoreData == null ? 0 : scoreData.GetValue<int>(scoreIndexList[i]), SCORE_BOUNUS,
                new CallBack(ReceiveBtnCallBack, scoreIndexList[i]));
        }

        // 根据key值排序
        List<int> pointIndexList = new List<int>();
        foreach (int index in pointBonus.Keys)
            pointIndexList.Add(index);
        pointIndexList.Sort();

        for (int i = 0; i < pointIndexList.Count; i++)
        {
            string name = string.Format("point_item_{0}", i);

            if (!mItems.ContainsKey(name))
                continue;

            GameObject wnd = mItems[name];

            wnd.GetComponent<ReceiveBonusItem>().BindData(pointBonus[pointIndexList[i]].AsMapping, pointData == null ? 0 : pointData.GetValue<int>(pointIndexList[i]), POINT_BONUS,
                new CallBack(ReceiveBtnCallBack, pointIndexList[i]));
        }
    }

    void CreateObjects()
    {
        if(ActivityInfo == null)
            return;

        LPCMapping bonus = ActivityMgr.GetActivityBonus(ActivityInfo).AsMapping;

        LPCMapping scoreBonus = bonus.GetValue<LPCMapping>(SCORE_BOUNUS);
        LPCMapping pointBonus = bonus.GetValue<LPCMapping>(POINT_BONUS);

        for (int i = 0; i < scoreBonus.Count; i++)
            CreateBonusItem(mSmallBonusItem, mSmallBonusItem.transform.parent, i, scoreBonus.Count, "score_item_", SCORE_BOUNUS);

        for (int i = 0; i < pointBonus.Count; i++)
            CreateBonusItem(mBigBonusItem, mBigBonusItem.transform.parent, i, pointBonus.Count, "point_item_", POINT_BONUS);
    }

    /// <summary>
    /// 创建奖励图标
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="icon">Icon.</param>
    /// <param name="amount">Amount.</param>
    GameObject CreateBonusItem(GameObject item, Transform tf, int index, int totalNum, string prefix, int type)
    {
        float x;
        if (totalNum % 2 == 1)
            x = (index - (totalNum / 2)) * (type == 1 ? 185f : 250f);
        else
            x = (index - ((totalNum - 1) / 2f)) * (type == 1 ? 185f : 250f);

        GameObject wnd = Instantiate (item) as GameObject;
        wnd.transform.parent = tf;

        string name = prefix + index;

        wnd.name = name;
        wnd.transform.localScale = Vector3.one;
        wnd.transform.localPosition = new Vector3 (x, item.transform.localPosition.y, 0);
        wnd.SetActive(true);

        if (!mItems.ContainsKey(name))
            mItems.Add(name, wnd);

        return wnd;
    }

    /// <summary>
    /// 领取奖励按钮回调
    /// </summary>
    void ReceiveBtnCallBack(object para, params object[] _params)
    {
        if (ActivityInfo == null)
            return;

        int type = (int) _params[0];

        LPCMapping bonusData = (LPCMapping)_params[1];

        int index = (int) para;

        string cookie = ActivityInfo.GetValue<string>("cookie");

        LPCMapping activity = ME.user.Query<LPCMapping>("activity_data");

        LPCMapping activityData = activity.GetValue<LPCMapping>(cookie);

        string desc = string.Empty;

        if (type.Equals(SCORE_BOUNUS))
        {
            // 积分不足无法领取
            if (activityData.GetValue<int>("score") < bonusData.GetValue<int>("cost"))
            {
                desc = LocalizationMgr.Get("AccumulateScoreWnd_29");
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    desc,
                    LocalizationMgr.Get("AccumulateScoreWnd_28"),
                    string.Empty,
                    true,
                    this.transform
                );

                return;
            }
        }
        else
        {
            // 累计点数不足
            if (activityData.GetValue<int>("point") < bonusData.GetValue<int>("cost"))
            {
                desc = LocalizationMgr.Get("AccumulateScoreWnd_30");
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    desc,
                    LocalizationMgr.Get("AccumulateScoreWnd_28"),
                    string.Empty,
                    true,
                    this.transform
                );

                return;
            }
        }

        // 构造参数
        LPCMapping data = LPCMapping.Empty;
        data.Add("index", index);
        data.Add("type", type);

        // 领取活动奖励
        ActivityMgr.ReceiveActivityBonus(cookie, LPCValue.Create(data));
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="itemId">Item identifier.</param>
    public void BindData(LPCMapping activityInfo)
    {
        if (activityInfo == null)
            return;

        ActivityInfo = activityInfo;

        CreateObjects();

        Redraw();
    }

    #endregion
}
