/// <summary>
/// RankingRewardItemWnd.cs
/// Created by fensc 2016/09/23
/// 竞技场排名奖励列表格子对象
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class RankingRewardItemWnd : WindowBase<RankingRewardItemWnd>
{
    #region 成员变量

    public UISprite mRankBg;
    // 段位背景图标;
    public UISprite mCrownIcon;
    // 皇冠图标;
    public GameObject[] mStars;
    // 星级
    public UILabel mSelfPosTips;
    // 当前自己的排名在列表中的提示
    public UILabel mDan;
    // 段位
    public UILabel mScore;
    // 竞技场积分
    public UILabel mRanking;
    // 竞技场排名

    public UILabel mRankLb;

    public GameObject mItem;

    // 奖励数据
    LPCArray mRewardData = new LPCArray();

    int step = 0;

    #endregion

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {

        mRankLb.text = LocalizationMgr.Get("RankingBattleWnd_15");

        // 获取某个指定的阶位信息
        mRewardData = ArenaMgr.GetBonus(step);

        mSelfPosTips.text = LocalizationMgr.Get("RankingBattleWnd_6");

        mSelfPosTips.gameObject.SetActive(false);

        if (mRewardData == null)
            return;

        // 获取排行榜奖励配置表信息
        CsvFile csv = ArenaMgr.TopBonusCsv;

        CsvRow row = csv.FindByKey(step);
        if (row == null)
            return;

        mDan.text = LocalizationMgr.Get(row.Query<string>("name"));

        int score = row.Query<int>("score");
        if (score == -1)
            score = 0;

        mScore.text = score.ToString();

        int rank = row.Query<int>("rank");

        if (rank == -1)
        {
            mRanking.gameObject.SetActive(false);
            mRankLb.gameObject.SetActive(false);
        }

        mRanking.text = (rank + 1).ToString();

        int starAmount = row.Query<int>("star");

        mRankBg.spriteName = row.Query<string>("rank_bg");
        mRankBg.MakePixelPerfect();

        mCrownIcon.spriteName = row.Query<string>("rank_icon");

        for (int i = 0; i < starAmount; i++)
            mStars[i].GetComponent<UISprite>().spriteName = row.Query<string>("star_name");

        for (int j = starAmount; j < mStars.Length; j++)
            mStars[j].GetComponent<UISprite>().spriteName = "arena_star_bg";

        int index = 0;
        mItem.SetActive(false);
        foreach (LPCValue data in mRewardData.Values)
        {
            GameObject clone = Instantiate(mItem);
            clone.transform.SetParent(mItem.transform.parent);
            clone.transform.localScale = Vector3.one;

            clone.transform.localPosition = new Vector3(mItem.transform.localPosition.x - index * 100, mItem.transform.localPosition.y, 1);

            clone.GetComponent<RankRewardGoodsItemWnd>().Bind(data.AsMapping);

            clone.SetActive(true);

            index++;
        }

        LPCValue arenaTop = ME.user.Query<LPCValue>("arena_top");

        if (arenaTop == null || ! arenaTop.IsMapping)
            return;

        // 玩家当前的阶位
        int userStep = ArenaMgr.GetStepByScoreAndRank(arenaTop.AsMapping.GetValue<int>("rank"), arenaTop.AsMapping.GetValue<int>("score"));

        if (step == userStep)
            mSelfPosTips.gameObject.SetActive(true);
    }

    #region 外部接口

    public void Bind(int index)
    {
        step = index;

        Redraw();
    }

    #endregion
}
