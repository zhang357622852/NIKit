/// <summary>
/// GangDetalisItemWnd.cs
/// Created by fengsc 2018/01/31
/// 公会详情基础格子
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GangDetalisItemWnd : WindowBase<GangDetalisItemWnd>
{
    #region 成员

    // 玩家头像
    public UITexture mIcon;

    // 玩家等级
    public UILabel mLevel;

    // 玩家名称
    public UILabel mName;

    // 登录时间
    public UILabel mLoginTime;

    // 竞技场星级
    public UISprite[] mStars;

    // 竞技场积分
    public UILabel mArenaScore;

    LPCMapping mData = LPCMapping.Empty;

    #endregion

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 加载玩家头像
        LPCValue iconValue = mData.GetValue<LPCValue>("icon");
        if (iconValue != null && iconValue.IsString)
            mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconValue.AsString));
        else
            mIcon.mainTexture = null;

        // 显示玩家等级
        mLevel.text = string.Format(LocalizationMgr.Get("GangWnd_22"), mData.GetValue<int>("level"));

        // 显示玩家名称
        mName.text = mData.GetValue<string>("name");

        int lastLogoutTime = mData.GetValue<int>("last_logout_time");

        int lastLoginTime = mData.GetValue<int>("last_login_time");

        // 好友在线
        if (lastLoginTime > lastLogoutTime)
        {
            mLoginTime.text = string.Format(LocalizationMgr.Get("GangWnd_27"));
        }
        else
        {
            mLoginTime.text = string.Format(LocalizationMgr.Get("GangWnd_25"), TimeMgr.ConvertTimeToSimpleChinese(lastLogoutTime));
        }

        for (int i = 0; i < mStars.Length; i++)
            mStars[i].spriteName = "arena_star_bg";

        // 竞技场数据
        LPCMapping arenaTop = mData.GetValue<LPCMapping>("arena_top");

        // 竞技场积分
        int score = arenaTop.GetValue<int>("score");

        mArenaScore.text = score.ToString();

        // 段位
        int step = ArenaMgr.GetStepByScoreAndRank(arenaTop.GetValue<int>("rank"), score);

        CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(step);
        if (row != null)
        {
            for (int j = 0; j < row.Query<int>("star"); j++)
                mStars[j].spriteName = row.Query<string>("star_name");
        }
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        mData = data;
        if (mData == null || mData.Count == 0)
            return;

        // 绘制窗口
        Redraw();
    }
}
