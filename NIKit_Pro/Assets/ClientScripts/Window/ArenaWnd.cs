/// <summary>
/// ArenaWnd.cs
/// Created by fengsc 2016/09/08
/// 竞技场窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public enum ARENA_PAGE
{
    PRACTICEBATTLE_PAGE,
    RANKINGBATTLE_PAGE,
    DEFENCEDEPLOY_PAGE,
    DEFENCERECORD_PAGE,
    WORLDRANK_PAGE,
    RANKREWARD_PAGE,
    PLAYBACK_PAGE,
    CHESTBATTLE_PAGE,
}

public class ArenaWnd : WindowBase<ArenaWnd>
{
    #region 成员变量

    // 练习对战按钮
    public GameObject mPracticeBattleBtn;
    public UILabel mPracticeBattleLb;

    // 练习对战提示信息
    public GameObject mPracticeTips;
    public UILabel mPracticeAmount;

    // 排位对战
    public GameObject mRankingBattleBtn;
    public UILabel mRankingBattleLb;

    // 防御部署按钮
    public GameObject mDefenceDeployBtn;
    public UILabel mDefenceDeployLb;

    // 防御记录提示
    public GameObject mRecordTips;
    public UILabel mRecordAmount;

    // 防御记录按钮
    public GameObject mDefenceRecordBtn;
    public UILabel mDefenceRecordLb;

    // 世界排名按钮
    public GameObject mWorldRankBtn;
    public UILabel mWorldRankLb;

    // 排名奖励按钮
    public GameObject mRankRewardBtn;
    public UILabel mRankRewardLb;

    // 宝箱战按钮
    public GameObject mChestBattleBtn;
    public UILabel mChestBattleBtnLb;

    // 战斗回放按钮
    public GameObject mPlaybackBtn;
    public UILabel mPlaybackBtnLb;

    // 关闭界面按钮
    public GameObject mCloseBtn;

    // 世界排名
    public UILabel mWorldRanking;
    public UILabel mWorldRankingLb;

    // 历史排名
    public GameObject mHistoryRankingBtn;
    public UILabel mHistoryRankingLb;

    // 角色基本信息窗口
    public UILabel mRank;
    public UISprite mRankIcon;
    public UISprite[] mStars;
    public UILabel mArenaInteger;
    public UILabel mArenaHonor;

    // 子窗口列表
    public GameObject[] mWnds;

    public GameObject[] mCheckMark;

    public TweenScale mTweenScale;
    public TweenAlpha mTweenAlpha;

    int mCurPage = 0;

    #endregion

    #region 内部函数

    void OnEnable()
    {
        // 关注MSG_ENTER_INSTANCE消息
        MsgMgr.RegisterDoneHook("MSG_ENTER_INSTANCE", "ArenaWnd", OnMsgEnterInstance);

        // 关注字段变化
        if (ME.user != null)
        {
            ME.user.dbase.RegisterTriggerField("ArenaWnd", new string[] { "revenge_data" }, new CallBack(OnRevengeDataChange));
            ME.user.dbase.RegisterTriggerField("ArenaWnd", new string[] { "instance_cooldown" }, new CallBack(OnRevengeDataChange));
            ME.user.dbase.RegisterTriggerField("ArenaWnd", new string[] { "exploit" }, new CallBack(OnHonorChange));
            ME.user.dbase.RegisterTriggerField("ArenaWnd", new string[] { "level" }, new CallBack(OnLevelChange));
            ME.user.dbase.RegisterTriggerField("ArenaWnd", new string[] { "arena_top" }, new CallBack(OnArenaTopChange));
        }

        // 重新播放缩放动画
        mTweenScale.enabled = true;
        mTweenScale.ResetToBeginning();

        // 重新播放渐变动画
        mTweenAlpha.enabled = true;
        mTweenAlpha.ResetToBeginning();

        // 重置选择tap
        SelectedWnd(mWnds[mCurPage]);

        // RefreshCheckMark
        RefreshCheckMark(mCurPage);

        // 重回窗口
        Redraw();
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDisable()
    {
        // 重置mCurPage
        mCurPage = 0;

        // 取消消息关注
        MsgMgr.RemoveDoneHook("MSG_ENTER_INSTANCE", "ArenaWnd");

        // 移除字段关注事件
        if (ME.user != null)
            ME.user.dbase.RemoveTriggerField("ArenaWnd");

        // 从正在打开列表中移除
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    // Use this for initialization
    void Start()
    {
        // 初始化本地化文本
        InitLocalText();

        // 注册事件;
        RegisterEvent();
    }

    void OnDestroy()
    {
        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("ArenaWnd");
    }

    /// <summary>
    /// 进入副本
    /// </summary>
    void OnMsgEnterInstance(string cmd, LPCValue para)
    {
        // 延迟到下一帧调用隐藏自己;
        MergeExecuteMgr.DispatchExecute(DoHideWindow);
    }

    /// <summary>
    /// 延迟隐藏窗口
    /// </summary>
    private void DoHideWindow()
    {
        // 调用隐藏自己;
        WindowMgr.HideWindow(gameObject);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mDefenceDeployBtn).onClick = OnClickDefenceDeployBtn;
        UIEventListener.Get(mDefenceRecordBtn).onClick = OnClickDefenceRecordBtn;
        UIEventListener.Get(mPracticeBattleBtn).onClick = OnClickPracticeBattleBtn;
        UIEventListener.Get(mChestBattleBtn).onClick = OnClickChestBattleBtn;
        UIEventListener.Get(mRankingBattleBtn).onClick = OnClickRankingBattleBtn;
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mHistoryRankingBtn).onClick = OnClickHistoryRankingBtn;
        UIEventListener.Get(mPlaybackBtn).onClick = OnClickPlaybackBtn;

        UIEventListener.Get(mWorldRankBtn).onClick = OnClickWorldRankBtn;
        UIEventListener.Get(mRankRewardBtn).onClick = OnClickRankRewardBtn;

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    void OnFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 竞技场数据变化
    /// </summary>
    void OnArenaTopChange(object param, params object[] _param)
    {
        Redraw();
    }

    /// <summary>
    /// 玩家等级变化
    /// </summary>
    void OnLevelChange(object param, params object[] _param)
    {
        // 刷新窗口
        Redraw();
    }

    /// <summary>
    /// 副本冷却时间变化回调
    /// </summary>
    void OnInstanceCooldownChange(object param, params object[] _param)
    {
        // 刷新NPC对战提示信息
        RefreshNPCAmount();
    }

    /// <summary>
    /// 防御数据变化回调
    /// </summary>
    void OnRevengeDataChange(object param, params object[] _param)
    {
        // 刷新防御记录提示信息
        RefreshDefencerRecordTips();
    }

    void OnHonorChange(object param, params object[] _param)
    {
        RedrawHonor();
    }

    void RedrawHonor()
    {
        mArenaHonor.text = ME.user.Query<int>("exploit").ToString();
    }

    // 绘制窗口
    void Redraw()
    {
        // 刷新提示信息
        RefreshNPCAmount();

        RedrawHonor();

        // 刷新防御记录提示信息
        RefreshDefencerRecordTips();

        LPCValue v = ME.user.Query<LPCValue>("arena_top");

        mRankIcon.spriteName = "ordinary_icon";

        for (int i = 0; i < mStars.Length; i++)
            mStars[i].spriteName = "arena_star_bg";

        mRank.text = string.Empty;

        mArenaInteger.text = string.Empty;

        mWorldRanking.text = string.Empty;

        LPCMapping arenaTop = LPCMapping.Empty;

        LPCValue openArena = ME.user.Query<LPCValue>("open_arena");
        if (openArena == null || !openArena.IsInt || openArena.AsInt != 1)
            return;

        if (v != null && v.IsMapping)
            arenaTop = v.AsMapping;

        int rank = 0;
        if (arenaTop.ContainsKey("rank") && arenaTop.GetValue<LPCValue>("rank").IsInt)
            rank = arenaTop.GetValue<int>("rank") + 1;

        int score = 0;
        if (arenaTop.ContainsKey("score") && arenaTop.GetValue<LPCValue>("score").IsInt)
            score = arenaTop.GetValue<int>("score");

        // 获取竞技场积分
        mArenaInteger.text = score.ToString();

        // 世界排名
        mWorldRanking.text = rank.ToString();

        // 玩家当前的阶位
        int step = ArenaMgr.GetStepByScoreAndRank(rank - 1, score);

        // 获取配置表数据
        CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(step);

        if (row == null)
            return;

        mRank.text = LocalizationMgr.Get(row.Query<string>("name"));

        int star = row.Query<int>("star");

        // 设置星级图标
        for (int i = 0; i < star; i++)
            mStars[i].spriteName = row.Query<string>("star_name");

        mRankIcon.spriteName = row.Query<string>("rank_icon");
        mRankIcon.MakePixelPerfect();
    }

    /// <summary>
    /// 刷新NPC剩余可挑战数量
    /// </summary>
    void RefreshNPCAmount()
    {
        // 获取npc可挑战的数量
        int amount = ArenaMgr.GetNpcChallengeAmount();

        if (amount < 1)
        {
            mPracticeTips.SetActive(false);
        }
        else
        {
            mPracticeAmount.text = amount.ToString();
            mPracticeTips.SetActive(true);
        }
    }

    /// <summary>
    /// 刷新防御记录提示信息
    /// </summary>
    void RefreshDefencerRecordTips()
    {
        // 如果是审核模式不需要显示反击列表
        if (ME.user.QueryTemp<int>("gapp_world") == 1)
        {
            mDefenceRecordBtn.SetActive(false);
            return;
        }

        // 设置mDefenceDeployBtn为激活状态
        mDefenceRecordBtn.SetActive(true);

        LPCValue revengeData = ME.user.Query<LPCValue>("revenge_data");
        if (revengeData == null || !revengeData.IsArray)
        {
            mRecordTips.SetActive(false);
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
            mRecordTips.SetActive(false);
        }
        else
        {
            mRecordAmount.text = amount.ToString();
            mRecordTips.SetActive(true);
        }
    }

    /// <summary>
    /// 关闭不是当前选择打开的窗口
    /// </summary>
    void SelectedWnd(GameObject go)
    {
        // 对象不存在
        if (go == null)
            return;

        // 显示窗口
        go.SetActive(true);

        // 隐藏其它的窗口
        for (int i = 0; i < mWnds.Length; i++)
        {
            // 如果如当前选择按钮不处理
            if (go.Equals(mWnds[i]))
                continue;

            // 隐藏窗口
            mWnds[i].SetActive(false);
        }
    }

    /// <summary>
    /// 练习对战选项点击事件
    /// </summary>
    void OnClickPracticeBattleBtn(GameObject go)
    {
        if (mCurPage == 0)
            return;

        mCurPage = 0;

        DoClick();
    }

    /// <summary>
    /// 排位战选项点击事件
    /// </summary>
    void OnClickRankingBattleBtn(GameObject go)
    {
        LPCValue openArena = ME.user.Query<LPCValue>("open_arena");
        if (openArena == null || !openArena.IsInt || openArena.AsInt != 1)
        {
            DialogMgr.Notify(LocalizationMgr.Get("DefenceDeployWnd_13"));
            return;
        }

        if (mCurPage == 1)
            return;

        mCurPage = 1;

        DoClick();
    }

    /// <summary>
    /// 防御部署选项点击事件
    /// </summary>
    void OnClickDefenceDeployBtn(GameObject go)
    {
        // 当前页面已经打开不做以下处理
        if (mCurPage == 2)
            return;

        mCurPage = 2;

        DoClick();
    }

    /// <summary>
    /// 防御记录选项点击事件
    /// </summary>
    void OnClickDefenceRecordBtn(GameObject go)
    {
        if (mCurPage == 3)
            return;

        mCurPage = 3;

        DoClick();
    }

    /// <summary>
    /// 世界排名按钮点击事件
    /// </summary>
    void OnClickWorldRankBtn(GameObject go)
    {
        if (mCurPage == 4)
            return;

        mCurPage = 4;

        DoClick();
    }

    /// <summary>
    /// 排名奖励按钮点击事件
    /// </summary>
    void OnClickRankRewardBtn(GameObject go)
    {
        if (mCurPage == 5)
            return;

        mCurPage = 5;

        DoClick();
    }

    /// <summary>
    /// 宝箱战选项点击事件
    /// </summary>
    void OnClickChestBattleBtn(GameObject go)
    {
//        if (mCurPage == 6)
//            return;
//
//        mCurPage = 6;
//
//        DoClick();
    }

    /// <summary>
    /// 战斗回放按钮点击回调
    /// </summary>
    void OnClickPlaybackBtn(GameObject go)
    {
        if (mCurPage == 6)
            return;

        mCurPage = 6;

        DoClick();
    }

    /// <summary>
    /// 历史排名查看点击事件
    /// </summary>
    void OnClickHistoryRankingBtn(GameObject go)
    {
        // 获取历史排名窗口
        GameObject wnd = WindowMgr.OpenWnd(HistoryRankingWnd.WndType);

        if (wnd == null)
        {
            LogMgr.Trace("HistoryRankingWnd窗口创建失败");
            return;
        }
    }

    /// <summary>
    /// 窗口关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口;
        WindowMgr.HideWindow(gameObject);

        ArenaMgr.PlaybackPage = 1;

        GameObject wnd = WindowMgr.GetWindow(MainWnd.WndType);

        if (wnd == null)
            return;

        // 显示主城窗口
        if (string.Equals(SceneMgr.MainScene, SceneConst.SCENE_WORLD_MAP))
        {
            wnd.GetComponent<MainWnd>().ShowMainUIBtn(false);
            WindowMgr.ShowWindow(wnd);
        } else
        {
            wnd.GetComponent<MainWnd>().ShowMainUIBtn(true);
            WindowMgr.ShowWindow(wnd);
        }
    }

    /// <summary>
    /// 初始化本地文本
    /// </summary>
    void InitLocalText()
    {
        mDefenceDeployLb.text = LocalizationMgr.Get("ArenaWnd_1");
        mDefenceRecordLb.text = LocalizationMgr.Get("ArenaWnd_2");
        mPracticeBattleLb.text = LocalizationMgr.Get("ArenaWnd_3");
        mChestBattleBtnLb.text = LocalizationMgr.Get("ArenaWnd_4");
        mRankingBattleLb.text = LocalizationMgr.Get("ArenaWnd_5");
        mWorldRankingLb.text = LocalizationMgr.Get("ArenaWnd_8");
        mHistoryRankingLb.text = LocalizationMgr.Get("ArenaWnd_9");
        mWorldRankLb.text = LocalizationMgr.Get("RankingBattleWnd_3");
        mRankRewardLb.text = LocalizationMgr.Get("RankingBattleWnd_4");
        mPlaybackBtnLb.text = LocalizationMgr.Get("ArenaWnd_14");
    }

    void RefreshCheckMark(int curPage)
    {
        for (int i = 0; i < mCheckMark.Length; i++)
        {
            if (i == curPage)
            {
                mCheckMark[i].SetActive(true);
            }
            else
            {
                mCheckMark[i].SetActive(false);
            }
        }
    }

    /// <summary>
    /// 绑定初始打开界面
    /// </summary>
    /// <param name="page">Page.</param>
    public void BindPage(int page)
    {
        mCurPage = page;

        // RefreshCheckMark
        RefreshCheckMark(mCurPage);

        // SelectedWnd
        SelectedWnd(mWnds[mCurPage]);
    }

    public void DoClick()
    {
        RefreshCheckMark(mCurPage);

        SelectedWnd(mWnds[mCurPage]);
    }

    // 指引打开防御部署界面
    public void GuideOpenDefenceWnd()
    {
        OnClickDefenceDeployBtn(mDefenceDeployBtn);
    }

    /// <summary>
    /// 指引选择使魔
    /// </summary>
    public void GuideSelectDefencePet(string itemName)
    {
        mWnds[2].GetComponent<DefenceDeployWnd>().GuideSelectPet(itemName);
    }

    /// <summary>
    /// 指引点击确认部署按钮
    /// </summary>
    public void GuideConfirmDeploy()
    {
        mWnds[2].GetComponent<DefenceDeployWnd>().GuideOnClickConfirmDeploy();
    }

    /// <summary>
    /// 指引点击练习对战
    /// </summary>
    public void GuideClickPracticeBattle()
    {
        OnClickPracticeBattleBtn(mPracticeBattleBtn);
    }

    /// <summary>
    /// 指引玩家选择npc操作
    /// </summary>
    public void GuideSelectNpcBattle(int index)
    {
        mWnds[0].GetComponent<ArenaNPCBattleWnd>().GuideSelectNpc(index);
    }

    #endregion
}
