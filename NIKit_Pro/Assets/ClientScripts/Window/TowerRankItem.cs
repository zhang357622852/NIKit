/// <summary>
/// TowerRankItem.cs
/// Created by lic 2016/09/23
/// 通天塔世界排名列表基础格子
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class TowerRankItem : WindowBase<TowerRankItem>
{
    #region 成员变量

    public UITexture  mIcon;                 // 玩家头像
    public UILabel    mLevel;                  // 玩家等级
    public UILabel    mName;                   // 玩家名称
    public UILabel    mFloorInfo;              // 层数
    public UILabel    mTimeDesc;               // 时间
    public UILabel    mRanking;                // 竞技场排名
    public UISprite   mRankBg;                // 排名背景
    public GameObject mBg;                  // 背景

    // 玩家rid
    [HideInInspector]
    public string mRid;

    // 排行榜数据
    LPCMapping mRankingData = new LPCMapping ();

    // 排行数据
    int mIndex = 0;

    #endregion

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 排行榜数据不存在
        if (mRankingData == null)
            return;

        mRid = mRankingData.GetValue<string>("rid");

        int rank = mIndex + 1;

        // 加载玩家头像
        LPCValue iconValue = mRankingData.GetValue<LPCValue>("icon");
        if (iconValue != null && iconValue.IsString)
            mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconValue.AsString));
        else
            mIcon.mainTexture = null;

        // 玩家名称
        mName.text = mRankingData.GetValue<string>("name");
        mLevel.text = mRankingData.GetValue<int>("level").ToString();

        // 竞技场排名
        mRanking.text = rank.ToString();

        // 设置字体大小
        mRanking.fontSize = GetFontSize(rank);

        int shift = (rank > 2 ? 2 : (rank - 1)) * 2;

        mRanking.transform.localPosition = new Vector3(130 + shift, 0, 0);

        // 名次背景图标
        mRankBg.spriteName = GetRankBgName(rank);

        // 获取最高通关层数
        // 通天塔层数数从0开始的，顾这个地方需要layer + 1
        int floor = mRankingData.GetValue<int>("layer") + 1;

        // 获取通关时间
        int time = mRankingData.GetValue<int>("time") - TowerMgr.RunTowerData.GetValue<int>("refresh_time");

        // 显示通天塔开启到通关该层时间间隔
        mFloorInfo.text = string.Format("{0}{1}", floor, LocalizationMgr.Get("TowerRankItem_4"));
        mTimeDesc.text = string.Format("({0})", TransTimeToChinese(time));

        if (mIndex % 2 == 0)
            mBg.SetActive(false);
        else
            mBg.SetActive(true);

    }

    /// <summary>
    /// 获取名次字体大小
    /// </summary>
    int GetFontSize(int rank)
    {
        switch (rank)
        {
            case 1: 
                return 45;
            case 2:
                return 40;
            case 3: 
                return 35;
            default : 
                return 25;
        }
    }

    /// <summary>
    /// 根据排名获取名次背景图标
    /// </summary>
    string GetRankBgName(int rank)
    {
        switch (rank)
        {
            case 1 :
                return "frist_top";
            case 2 : 
                return "second_top";
            case 3 :
                return "thrid_top";
            default :
                return "other_top";
        }
    }

    string TransTimeToChinese(int time)
    {
        if (time < 0)
            return string.Empty;

        // 不足一分钟
        if (time < 60)
            return string.Format("{0} {1}", time, LocalizationMgr.Get("TowerRankItem_3"));

        // 不足1小时
        if(time < 3600)
            return string.Format("{0} {1}", time/60, LocalizationMgr.Get("TowerRankItem_2"));

        int hour = time / 3600;

        if (time % 3600 == 0)
            return string.Format("{0} {1}", hour, LocalizationMgr.Get("TowerRankItem_1"));
        else
            return string.Format("{0} {1} {2} {3}", hour, LocalizationMgr.Get("TowerRankItem_1"),
                time % 3600 / 60, LocalizationMgr.Get("TowerRankItem_2"));
    }

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="data">Data.</param>
    /// <param name="index">Index.</param>
    public void Bind(LPCMapping data, int index)
    {
        mRankingData = data;

        mIndex = index;

        Redraw();
    }

    #endregion
}
