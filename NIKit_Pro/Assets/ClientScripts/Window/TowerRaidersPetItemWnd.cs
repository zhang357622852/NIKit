/// <summary>
/// TowerRaidersPetItemWnd.cs
/// Created by fengsc 2017/08/25
/// 攻略使魔基础格子
/// </summary>
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class TowerRaidersPetItemWnd : WindowBase<TowerRaidersPetItemWnd>
{
    #region 成员变量

    // 使魔排名
    public UILabel mRankLb;

    public UITexture mIcon;

    // 使魔排名背景
    public UISprite mRankBg;

    // 使魔元素
    public UISprite mElement;

    // 使魔名称
    public UILabel mName;

    // 使用百分比
    public UISlider mSlider;

    public UISprite mSliderSprite;

    // 使用百分比
    public UILabel mRate;

    public GameObject mBg;

    public Property mPetOb;

    public Color[] mSliderColor;

    public Color[] mRankBgColor;

    private LPCArray mTopData;

    // 排名
    private int mRank;

    private int mSumTimes;

    #endregion

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        if (mPetOb != null)
            mPetOb.Destroy();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 名次显示
        mRankLb.text = mRank.ToString();

        if (mRank % 2 == 0)
            mBg.SetActive(true);
        else
            mBg.SetActive(false);

        LPCMapping data = mTopData[mRank - 1].AsMapping;
        if (data == null)
            return;

        LPCMapping firstData = mTopData[0].AsMapping;
        if (firstData == null)
            return;

        int classId = data.GetValue<int>("class_id");

        LPCMapping para = LPCMapping.Empty;

        para.Add("class_id", classId);
        para.Add("rid", Rid.New());

        if (mPetOb != null)
            mPetOb.Destroy();

        mPetOb = PropertyMgr.CreateProperty(para);
        if (mPetOb == null)
            return;

        // 显示宠物图标
        mIcon.mainTexture = MonsterMgr.GetTexture(mPetOb.GetClassID(), mPetOb.GetRank());

        // 显示宠物名称
        mName.text = LocalizationMgr.Get(mPetOb.Query<string>("name"));

        // 显示元素
        mElement.spriteName = PetMgr.GetElementIconName(MonsterMgr.GetElement(classId));

        float firstRate = firstData.GetValue<int>("times") / (float)mSumTimes;

        float rate = data.GetValue<int>("times") / (float) mSumTimes;

        mRate.text = string.Format("{0}%", Math.Truncate(rate * 1000) / 10);

        if (mRank == 1)
            mSlider.value = 1;
        else
            mSlider.value = rate / firstRate;

        if (mRank < 4)
        {
            mSliderSprite.color = mSliderColor[0];
            mRankBg.color = mRankBgColor[0];
        }
        else if (mRank < 7)
        {
            mSliderSprite.color = mSliderColor[1];
            mRankBg.color = mRankBgColor[1];
        }
        else
        {
            mSliderSprite.color = mSliderColor[2];
            mRankBg.color = mRankBgColor[2];
        }
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCArray topData, int rank, int sumTimes)
    {
        if (topData == null)
            return;

        mTopData = topData;

        mRank = rank;

        mSumTimes = sumTimes;

        // 重绘窗口
        Redraw();
    }
}

