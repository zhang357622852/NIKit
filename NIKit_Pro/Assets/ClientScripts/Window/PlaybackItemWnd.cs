/// <summary>
/// PlaybackItemWnd.cs
/// Created by fengsc 2018/03/05
/// 战斗回放列表格子
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class PlaybackItemWnd : WindowBase<PlaybackItemWnd>
{
    // 对战玩家信息
    public UITexture mLeftIcon;

    public UILabel mLeftName;

    public UILabel mLeftScore;

    public UISprite[] mLeftStars;

    public UILabel mCombatTime;

    public UILabel mRightName;

    public UILabel mRightScore;

    public UISprite[] mRightStars;

    public UITexture mRightIcon;

    // 对战信息按钮
    public GameObject mCombatInfoBtn;
    public UILabel mCombatInfoBtnLb;

    // 战斗回放按钮
    public GameObject mPlaybackBtn;
    public UILabel mPlaybackBtnLb;

    LPCMapping mData = LPCMapping.Empty;

    void Start()
    {
        // 注册事件
        RegisterEvent();

        mCombatInfoBtnLb.text = LocalizationMgr.Get("PlaybackWnd_4");
        mPlaybackBtnLb.text = LocalizationMgr.Get("PlaybackWnd_5");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCombatInfoBtn).onClick = OnClickCombatInfoBtn;
        UIEventListener.Get(mPlaybackBtn).onClick = OnClickPlaybackBtn;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 初始化
        for (int i = 0; i < mLeftStars.Length; i++)
            mLeftStars[i].spriteName = "arena_star_bg";

        for (int i = 0; i < mRightStars.Length; i++)
            mRightStars[i].spriteName = "arena_star_bg";

        if (mData == null || mData.Count == 0)
            return;

        if (mData.ContainsKey("attack"))
        {
            LPCMapping attack = mData.GetValue<LPCMapping>("attack");

            int classId = 0;

            if (int.TryParse(attack.GetValue<string>("icon"), out classId))
            {
                // 玩家头像
                mLeftIcon.mainTexture = MonsterMgr.GetTexture(classId, MonsterMgr.GetDefaultRank(classId));
            }
            else
            {
                mLeftIcon.mainTexture = ResourceMgr.LoadTexture("Assets/Art/UI/Icon/monster/default_head_icon.png");
            }

            mLeftName.text = attack.GetValue<string>("name");

            // 竞技场积分
            int score = attack.GetValue<int>("score");

            mLeftScore.text = score.ToString();

            int step = ArenaMgr.GetStepByScoreAndRank(attack.GetValue<int>("rank"), score);

            CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(step);
            if (row != null)
            {
                for (int i = 0; i < row.Query<int>("star"); i++)
                    mLeftStars[i].spriteName = row.Query<string>("star_name");
            }
        }

        if (mData.ContainsKey("defence"))
        {
            LPCMapping defence = mData.GetValue<LPCMapping>("defence");

            int classId = 0;

            if (int.TryParse(defence.GetValue<string>("icon"), out classId))
            {
                // 玩家头像
                mRightIcon.mainTexture = MonsterMgr.GetTexture(classId, MonsterMgr.GetDefaultRank(classId));
            }
            else
            {
                mRightIcon.mainTexture = ResourceMgr.LoadTexture("Assets/Art/UI/Icon/monster/default_head_icon.png");
            }

            mRightName.text = defence.GetValue<string>("name");

            // 竞技场积分
            int score = defence.GetValue<int>("score");

            mRightScore.text = score.ToString();

            int step = ArenaMgr.GetStepByScoreAndRank(defence.GetValue<int>("rank"), score);

            CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(step);
            if (row != null)
            {
                for (int i = 0; i < row.Query<int>("star"); i++)
                    mRightStars[i].spriteName = row.Query<string>("star_name");
            }
        }

        // 战斗时间
        mCombatTime.text = TimeMgr.ConvertTimeToSimpleChinese(mData.GetValue<int>("time"));
    }

    /// <summary>
    /// 对战信息按钮点击回调
    /// </summary>
    void OnClickCombatInfoBtn(GameObject go)
    {
        if (TimeMgr.GetServerTime() - mData.GetValue<int>("time") >= GameSettingMgr.GetSettingInt("video_valid_time"))
        {
            DialogMgr.Notify(LocalizationMgr.Get("ArenaWnd_11"));

            return;
        }

        GameObject wnd = WindowMgr.OpenWnd(PlaybackInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<PlaybackInfoWnd>().Bind(mData.GetValue<string>("id"));
    }

    /// <summary>
    /// 战斗回放按钮点击回调
    /// </summary>
    void OnClickPlaybackBtn(GameObject go)
    {
        if (TimeMgr.GetServerTime() - mData.GetValue<int>("time") >= GameSettingMgr.GetSettingInt("video_valid_time"))
        {
            DialogMgr.Notify(LocalizationMgr.Get("ArenaWnd_11"));

            return;
        }

        // 播放战斗回放视频
        VideoMgr.PlayVideo(mData.GetValue<string>("id"));
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        mData = data;

        // 绘制窗口
        Redraw();
    }
}
