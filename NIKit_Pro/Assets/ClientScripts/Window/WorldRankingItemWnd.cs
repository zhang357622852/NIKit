/// <summary>
/// WorldRankingItemWnd.cs
/// Created by fengsc 2016/09/23
/// 竞技场世界排名列表基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class WorldRankingItemWnd : WindowBase<WorldRankingItemWnd>
{
    #region 成员变量

    public UITexture mIcon;                 // 玩家头像
    public UILabel mLevel;                  // 玩家等级
    public UILabel mName;                   // 玩家名称
    public UILabel mScore;                  // 竞技场积分
    public UILabel mRanking;                // 竞技场排名
    public UISprite mRankBg;                // 排名背景

    // 排行榜数据
    LPCMapping mRankingData = new LPCMapping ();

    #endregion

    // Use this for initialization
    void Start ()
    {
        UIEventListener.Get(this.gameObject).onClick = OnClickItem;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 排行榜数据不存在
        if (mRankingData == null)
            return;

        // 加载玩家头像
        LPCValue iconValue = mRankingData.GetValue<LPCValue>("icon");
        if (iconValue != null && iconValue.IsString)
            mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconValue.AsString));
        else
            mIcon.mainTexture = null;

        // 获取玩家等级
        mLevel.text = string.Format(LocalizationMgr.Get("RankingBattleWnd_12"), mRankingData.GetValue<int>("level").ToString());

        // 玩家名称
        mName.text = mRankingData.GetValue<string>("name");

        // 竞技场积分
        mScore.text = mRankingData.GetValue<int>("score").ToString();

        // 竞技场排名
        mRanking.text = (mRankingData.GetValue<int>("rank") + 1).ToString();

        // 设置字体大小
        mRanking.fontSize = ArenaMgr.GetFontSize(mRankingData.GetValue<int>("rank") + 1);

        // 名次背景图标
        mRankBg.spriteName = ArenaMgr.GetRankBgName(mRankingData.GetValue<int>("rank") + 1);

        mRanking.transform.localPosition = new Vector3 (mRankBg.transform.localPosition.x,
            mRanking.transform.localPosition.y,
            mRanking.transform.localPosition.z);

        // 使用图片默认大小
        mRankBg.MakePixelPerfect();
    }

    /// <summary>
    /// 格子点击事件
    /// </summary>
    void OnClickItem(GameObject go)
    {
        // TODO: 点击查看对方魔灵信息
    }

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        mRankingData = data;

        Redraw();
    }

    #endregion
}
