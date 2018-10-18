/// <summary>
/// TowerBonusItem.cs
/// Created by lic 2016/09/23
/// 通天塔奖励列表基础格子
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class TowerBonusItem  : WindowBase<TowerBonusItem>
{

    #region 成员变量

    public UILabel mDesc;                   // 描述
    public GameObject mBg;                  // 背景
    public GameObject mRewardItem;           // 奖励项

    // 奖励内容
    public LPCMapping mBonusData { get; private set; }

    // 排行数据
    int mIndex = 0;

    // 层数
    int mLayer = 0;

    #endregion

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 排行榜数据不存在
        if (mBonusData == null)
            return;

        mRewardItem.GetComponent<SignItemWnd>().OnlyShowAttribAmount(false);

        mRewardItem.GetComponent<SignItemWnd>().ShowAmount(true);

        // 描述
        if (mIndex == 0)
            mDesc.text = string.Format(LocalizationMgr.Get("TowerBonusItem_1"), 10, mLayer);
        else if (mIndex == 1)
        {
            mRewardItem.GetComponent<SignItemWnd>().ShowAmount(false);
            mDesc.text = string.Format(LocalizationMgr.Get("TowerBonusItem_1"), 10, mLayer);
        }
        else if (mIndex == 2)
            mDesc.text = LocalizationMgr.Get("TowerBonusItem_3");
        else
            mDesc.text = string.Format(LocalizationMgr.Get("TowerBonusItem_2"), mLayer);

        // 奖励
        mRewardItem.GetComponent<SignItemWnd>().Bind(mBonusData, string.Empty, false, false);

        if (mIndex % 2 == 0)
            mBg.SetActive(false);
        else
            mBg.SetActive(true);

    }

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="data">Data.</param>
    /// <param name="index">Index.</param>
    public void Bind(LPCMapping data, int index)
    {
        mBonusData = data.GetValue<LPCMapping>("bonus");

        mLayer = data.GetValue<int>("layer");

        mIndex = index;

        Redraw();
    }

    #endregion
}
