/// <summary>
/// CombatInfoWnd.cs
/// Created by fengsc 2018/03/05
/// 回放战斗信息窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class CombatInfoWnd : WindowBase<CombatInfoWnd>
{
    // 攻方头像
    public UITexture mAtkIcon;

    // 攻方名称
    public UILabel mAtkName;

    // 攻方竞技场积分
    public UILabel mAtkScore;

    // 攻方竞技场星级
    public UISprite[] mAtkStars;

    // 防守方头像
    public UITexture mDefenceIcon;

    // 防守方名称
    public UILabel mDenfeceName;

    // 防守方竞技场积分
    public UILabel mDefenceScore;

    // 防守方竞技场星级
    public UISprite[] mDefenceStars;

    // 对战信息按钮
    public GameObject mCombatInfBtn;
    public UILabel mCombatInfBtnLb;

    public CombatWnd mCombatWnd;

    LPCMapping mDetails = LPCMapping.Empty;

    void Start()
    {
        Redraw();

        // 注册按钮点击事件
        UIEventListener.Get(mCombatInfBtn).onClick = OnClickCombatInfoBtn;

        mCombatInfBtnLb.text = LocalizationMgr.Get("CombatWnd_7");
    }

    /// <summary>
    /// 回放按钮点击回调
    /// </summary>
    void OnClickCombatInfoBtn(GameObject go)
    {
        // 打开回放详细信息详细界面
        GameObject wnd = WindowMgr.OpenWnd(PlaybackInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<PlaybackInfoWnd>().Bind(mDetails.GetValue<string>("id"), new CallBack(OnCallBack), false, true);

        // 暂停回放
        mCombatWnd.PlaybackPause();
    }

    void OnCallBack(object para, params object[] param)
    {
        // 暂停回放
        mCombatWnd.PlaybackPause();
    }

    // 绘制窗口
    void Redraw()
    {
        // 初始化
        for (int i = 0; i < mAtkStars.Length; i++)
            mAtkStars[i].spriteName = "arena_star_bg";

        for (int i = 0; i < mDefenceStars.Length; i++)
            mDefenceStars[i].spriteName = "arena_star_bg";

        // 回放详细信息
        mDetails = VideoMgr.VideoDetails;
        if (mDetails.ContainsKey("attack"))
        {
            LPCMapping attack = mDetails.GetValue<LPCMapping>("attack");

            mAtkName.text = attack.GetValue<string>("name");

            int score = attack.GetValue<int>("score");

            mAtkScore.text = score.ToString();

            int classId = 0;

            if (int.TryParse(attack.GetValue<string>("icon"), out classId))
            {
                mAtkIcon.mainTexture = MonsterMgr.GetTexture(classId, MonsterMgr.GetDefaultRank(classId));
            }
            else
            {
                mAtkIcon.mainTexture = ResourceMgr.LoadTexture("Assets/Art/UI/Icon/monster/default_head_icon.png");
            }

            int step = ArenaMgr.GetStepByScoreAndRank(attack.GetValue<int>("rank"), score);

            CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(step);
            if (row != null)
            {
                for (int i = 0; i < row.Query<int>("star"); i++)
                    mAtkStars[i].spriteName = row.Query<string>("star_name");
            }
        }

        if (mDetails.ContainsKey("defence"))
        {
            LPCMapping defence = mDetails.GetValue<LPCMapping>("defence");

            mDenfeceName.text = defence.GetValue<string>("name");

            int score = defence.GetValue<int>("score");

            mDefenceScore.text = score.ToString();

            int classId = 0;

            if (int.TryParse(defence.GetValue<string>("icon"), out classId))
            {
                mDefenceIcon.mainTexture = MonsterMgr.GetTexture(classId, MonsterMgr.GetDefaultRank(classId));
            }
            else
            {
                // 玩家头像
                mDefenceIcon.mainTexture = ResourceMgr.LoadTexture("Assets/Art/UI/Icon/monster/default_head_icon.png");
            }

            int step = ArenaMgr.GetStepByScoreAndRank(defence.GetValue<int>("rank"), score);

            CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(step);
            if (row != null)
            {
                for (int i = 0; i < row.Query<int>("star"); i++)
                    mDefenceStars[i].spriteName = row.Query<string>("star_name");
            }
        }
    }
}
