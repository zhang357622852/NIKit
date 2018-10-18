/// <summary>
/// HistoryRankingItemWnd.cs
/// Created by fengsc 2017/03/07
/// 历史排名基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class HistoryRankingItemWnd : WindowBase<HistoryRankingItemWnd>
{
    // 标题
    public UILabel mTitle;

    // 排名背景
    public UISprite mRankBg;

    // 排名图标
    public UISprite mRankIcon;

    // 排名星级
    public UISprite[] mStars;

    // 排名
    public UILabel mRank;

    // 竞技场积分
    public UILabel mScore;

    // 胜率
    public UILabel mWinRate;
    public UILabel mWinRateLb;

    // 防守率
    public UILabel mDefenceRate;
    public UILabel mDefenceRateLb;

    // 排行榜数据
    LPCMapping mArenaTopData;

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        int rank = 0;
        if (mArenaTopData.ContainsKey("rank") && mArenaTopData.GetValue<LPCValue>("rank").IsInt)
            rank = mArenaTopData.GetValue<int>("rank") + 1;

        int score = 0;
        if (mArenaTopData.ContainsKey("score") && mArenaTopData.GetValue<LPCValue>("score").IsInt)
            score = mArenaTopData.GetValue<int>("score");

        // 获取竞技场积分
        mScore.text = score.ToString();

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

        for (int i = star; i < mStars.Length; i++)
            mStars[i].spriteName = "arena_star_bg";

        mRankIcon.spriteName = row.Query<string>("rank_icon");
        mRankIcon.MakePixelPerfect();

        mRankBg.spriteName = row.Query<string>("rank_bg");
        mRankBg.MakePixelPerfect();

        int winTimes = mArenaTopData.GetValue<int>("challenge_win_times");

        int times = mArenaTopData.GetValue<int>("challenge_times");

        // 胜率
        float winRate = 0f;
        if (times == 0)
            winRate = 0f;
        else
            winRate = winTimes / (float) times;

        mWinRate.text = string.Format("{0:F1}% ({1}/{2})", winRate * 100, winTimes, times);

        int defneceWinTimes = mArenaTopData.GetValue<int>("defence_win_times");
        int defneceTimes = mArenaTopData.GetValue<int>("defence_times");

        // 防守率
        float defenceRate = 0f;
        if (defneceTimes == 0)
            defenceRate = 0f;
        else
            defenceRate = defneceWinTimes / (float) defneceTimes;

        mDefenceRate.text = string.Format("{0:F1}% ({1}/{2})", defenceRate * 100, defneceWinTimes, defneceTimes);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping arenaTopData, string title)
    {
        // 初始化本地化文本
        mTitle.text = title;
        mWinRateLb.text = LocalizationMgr.Get("HistoryRankingWnd_5");
        mDefenceRateLb.text = LocalizationMgr.Get("HistoryRankingWnd_6");

        if (arenaTopData == null)
            return;

        mArenaTopData = arenaTopData;

        // 绘制窗口
        Redraw();
    }
}
