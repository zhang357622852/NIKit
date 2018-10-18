/// <summary>
/// SettlementFinishWnd.cs
/// Created by fengsc 2017/06/30
/// 竞技场结算完毕信息显示界面
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class SettlementFinishWnd : WindowBase<SettlementFinishWnd>
{
    #region 成员变量

    // 标题
    public UILabel mTitle;

    public UILabel mTips;

    public UISprite mRankBg;

    public UISprite mRankIcon;

    public UISprite[] mStar;

    public GameObject mItem;

    // 确定按钮
    public GameObject mConfirmBtn;
    public UILabel mConfirmLb;

    public GameObject mRank;

    LPCArray mLastBonus = LPCArray.Empty;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 初始化本地化文本
        InitLocalText();

        // 注册事件
        RegisterEvent();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 按钮点击事件
        UIEventListener.Get(mConfirmBtn).onClick = OnClickConfirmBtn;
    }

    void InitLocalText()
    {
        mTitle.text = LocalizationMgr.Get("SettlementFinishWnd_1");
        mConfirmLb.text = LocalizationMgr.Get("SettlementFinishWnd_4");
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mItem.SetActive(false);

        mRank.SetActive(false);

        mRankIcon.spriteName = "ordinary_icon";

        mRankBg.spriteName = "ordinary_bg";

        for (int i = 0; i < mStar.Length; i++)
            mStar[i].spriteName = "arena_star_bg";

        LPCMapping arenaTop = LPCMapping.Empty;

        // 竞技场排名数据
        LPCValue v = ME.user.Query<LPCValue>("arena_top");
        if (v == null)
            return;

        if (v.IsMapping)
            arenaTop = v.AsMapping;

        LPCMapping data = LPCMapping.Empty;

        // 上周竞技场排名数据
        LPCValue topData = arenaTop.GetValue<LPCValue>("last_top_data");
        if (topData == null)
            return;

        if (topData.IsMapping)
            data = topData.AsMapping;

        // 计算奖励阶位
        int step = ArenaMgr.GetStepByScoreAndRank(data.GetValue<int>("rank"), data.GetValue<int>("score"));

        // 获取排行榜奖励配置表信息
        CsvFile csv = ArenaMgr.TopBonusCsv;

        CsvRow row = csv.FindByKey(step);
        if (row == null)
            return;

        mTips.text = string.Format(LocalizationMgr.Get("SettlementFinishWnd_3"), LocalizationMgr.Get(row.Query<string>("name")));

        int starAmount = row.Query<int>("star");

        mRankBg.spriteName = row.Query<string>("rank_bg");
        mRankBg.MakePixelPerfect();

        mRankIcon.spriteName = row.Query<string>("rank_icon");

        for (int i = 0; i < starAmount; i++)
            mStar[i].spriteName = row.Query<string>("star_name");

        for (int j = starAmount; j < mStar.Length; j++)
            mStar[j].spriteName = "arena_star_bg";

        if (mLastBonus == null)
            return;

        // bg显示内容的宽度
        float bgWidth = 670;

        // 排名图标于奖励物品的间距
        float rankSpace = 28;

        // 物品间的空隙
        float goodSpace = 20;

        float itemWidth = mItem.GetComponent<UISprite>().localSize.x;

        float startX = 0 - bgWidth / 2 + (bgWidth - mRankBg.localSize.x - rankSpace - goodSpace * (mLastBonus.Count - 1) - itemWidth * (mLastBonus.Count - 1)) / 2;

        mRank.SetActive(true);

        mRank.transform.localPosition = new Vector3(
            startX,
            mRank.transform.localPosition.y,
            mRank.transform.localPosition.z);

        float goodStartX = mRank.transform.localPosition.x + mRankBg.localSize.x / 2 + rankSpace + itemWidth / 2;

        for (int i = 0; i < mLastBonus.Count; i++)
        {
            GameObject clone = Instantiate(mItem);
            clone.transform.SetParent(mItem.transform.parent);
            clone.transform.localScale = Vector3.one;

            clone.transform.localPosition = new Vector3(
                goodStartX + i * 100,
                mItem.transform.localPosition.y,
                mItem.transform.localPosition.z);

            clone.GetComponent<RankRewardGoodsItemWnd>().Bind(mLastBonus[i].AsMapping);

            clone.SetActive(true);
        }
    }

    /// <summary>
    /// 确定窗口
    /// </summary>
    void OnClickConfirmBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCArray bonus)
    {
        mLastBonus = bonus;

        LPCMapping lastSettlementData = LPCMapping.Empty;

        lastSettlementData.Add("last_bonus", LPCArray.Empty);

        ArenaMgr.UpdateSettlementBonusData(ME.user, lastSettlementData);

        // 通知服务器更新结算显示标识
        Operation.CmdResetArenaSettlementTips.Go(1);

        // 重绘界面
        Redraw();
    }
}
