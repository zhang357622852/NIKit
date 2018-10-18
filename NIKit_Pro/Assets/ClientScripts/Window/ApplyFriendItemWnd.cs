/// <summary>
/// ApplyFriendItemWnd.cs
/// Created by fengsc 2017/01/19
/// 等待通过申请的好友基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ApplyFriendItemWnd : WindowBase<ApplyFriendItemWnd>
{
    // 玩家头像
    public UITexture mIcon;

    // 玩家等级
    public UILabel mLevel;

    // 玩家名称
    public UILabel mName;

    public UISprite mGender;

    // 段位图标
    public UISprite mRankIcon;

    // 星级
    public UISprite[] mStars;

    // 竞技场积分
    public UILabel mScore;

    public UILabel ApplyTips;

    // 取消申请好友
    public GameObject mCancelBtn;

    LPCMapping mUser = LPCMapping.Empty;

    // Use this for initialization
    void Start ()
    {
        ApplyTips.text = LocalizationMgr.Get("FindFriendWnd_5");

        UIEventListener.Get(mCancelBtn).onClick = OnClickCancelBtn;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 等级
        mLevel.text = string.Format(LocalizationMgr.Get("FindFriendWnd_4"), mUser.GetValue<int>("level"));

        mName.text = mUser.GetValue<string>("name");

        mGender.spriteName = UserMgr.GetGenderIcon(mUser.GetValue<int>("gender"));

        LPCMapping arenaTop = mUser.GetValue<LPCMapping>("arena_top");

        for (int i = 0; i < mStars.Length; i++)
            mStars[i].spriteName = "arena_star_bg";

        mRankIcon.spriteName = "ordinary_icon";

        mScore.text = 0.ToString();

        // 加载玩家头像
        LPCValue iconValue = mUser.GetValue<LPCValue>("icon");
        if (iconValue != null && iconValue.IsString)
            mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconValue.AsString));
        else
            mIcon.mainTexture = null;

        if (arenaTop == null)
            return;

        // 竞技场排名
        int rank = arenaTop.GetValue<int>("rank") + 1;

        // 竞技场积分
        int score = arenaTop.GetValue<int>("score");

        // 竞技场积分
        mScore.text = Mathf.Max(0, score).ToString();

        // 竞技场阶位
        int step = ArenaMgr.GetStepByScoreAndRank(rank - 1, score);

        // 获取配置表数据
        CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(step);

        if (row == null)
            return;

        mRankIcon.spriteName = row.Query<string>("rank_icon");

        // 星级
        int star = row.Query<int>("star");

        for (int i = 0; i < star; i++)
            mStars[i].spriteName = row.Query<string>("star_name");
    }

    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    void OnClickCancelBtn(GameObject go)
    {
        // 通知服务器取消好友申请
        Operation.CmdCancelRequest.Go(mUser.GetValue<string>("opp"));
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        if (data == null || data.Count < 1)
            return;

        mUser = data;

        Redraw();
    }
}
