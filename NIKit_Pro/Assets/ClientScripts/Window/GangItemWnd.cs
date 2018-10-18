/// <summary>
/// GangItemWnd.cs
/// Created by fengsc 2018/01/30
/// 公会基础格子
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GangItemWnd : WindowBase<GangItemWnd>
{
    #region 成员变量

    // 公会旗帜
    public FlagItemWnd mFlagItemWnd;

    // 公会名称
    public UILabel mGangName;

    // 公会成员数量
    public UILabel mMemberAmount;

    // 段位星级
    public UISprite[] mStars;

    public UILabel mTips;

    // 审核条件
    public UILabel mCheckCondition;

    public UISprite mBg;

    // 公会数据
    public LPCMapping mGangData = LPCMapping.Empty;

    #endregion

    // Use this for initialization
    void Start ()
    {

    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 旗帜绑定数据
        mFlagItemWnd.Bind(mGangData.GetValue<LPCArray>("flag"));

        // 公会名称
        mGangName.text = mGangData.GetValue<string>("gang_name");

        // 成员数量
        mMemberAmount.text = string.Format("{0}/{1}", mGangData.GetValue<int>("amount"), mGangData.GetValue<int>("max_count"));

        // 筛选条件
        LPCMapping condition = mGangData.GetValue<LPCMapping>("join_condition");
        if (condition == null)
            return;

        // 段位
        int step = condition.GetValue<int>("step");

        if (step == -1)
        {
            mTips.text = LocalizationMgr.Get("CreateGuildWnd_20");

            for (int i = 0; i < mStars.Length; i++)
                mStars[i].gameObject.SetActive(false);
        }
        else
        {
            CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(step);
            if (row == null)
                return;

            for (int i = 0; i < mStars.Length; i++)
            {
                mStars[i].gameObject.SetActive(true);
                mStars[i].spriteName = "arena_star_bg";
            }

            for (int i = 0; i < row.Query<int>("star"); i++)
                mStars[i].spriteName = row.Query<string>("star_name");
        }

        // 审核条件
        int check = condition.GetValue<int>("check");

        switch (check)
        {
            case (int) CHECK_CONDITION.NO_CHECK:
                mCheckCondition.text = LocalizationMgr.Get("CreateGuildWnd_17");
                break;

            case (int) CHECK_CONDITION.NEED_CHECK:
                mCheckCondition.text = LocalizationMgr.Get("CreateGuildWnd_18");
                break;

            case (int) CHECK_CONDITION.REFUSE_JION:
                mCheckCondition.text = LocalizationMgr.Get("CreateGuildWnd_19");
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        mGangData = data;
        if (mGangData == null || mGangData.Count == 0)
            return;

        Redraw();
    }

    /// <summary>
    /// 选中处理
    /// </summary>
    public void Select(bool isSelect)
    {
        if (isSelect)
        {
            mBg.spriteName = "summonSelectBg";
        }
        else
        {
            mBg.spriteName = "summonNoSelectBg";
        }
    }
}
