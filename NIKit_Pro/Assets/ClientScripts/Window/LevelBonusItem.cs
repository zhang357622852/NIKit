/// <summary>
/// LevelBonusItem.cs
/// Created by lic 11/13/2017
/// 等级奖励item
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public enum RECEIVE_STATE
{
    UNCOMPLETE, // 未完成
    RECEIVING, // 待领取
    RECEIVED,  // 已领取

}

public class LevelBonusItem : WindowBase<LevelBonusItem>
{
    #region 成员变量

    public UISprite mReceiveIcon;
    public UILabel mReceiveLb;
    public UILabel mLevelLb;

    public GameObject[] mBonusOb;

    public TweenAlpha mTipsAlphaAnim;

    #endregion

    #region 私有变量及属性

    LPCArray mBonusData;

    public bool IsSelected { get; private set; }

    public int Level { get; private set; }

    public RECEIVE_STATE State { get; private set; }

    #endregion

    #region 内部函数

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        for (int i = 0; i < mBonusOb.Length; i++)
            mBonusOb[i].SetActive(false);

        for (int i = 0; i < mBonusData.Count; i++)
        {
            if (i >= mBonusOb.Length)
                break;

            mBonusOb[i].SetActive(true);
            mBonusOb[i].GetComponent<SimpleBonusItem>().BindData(mBonusData[i].AsMapping);
        }


        SetState(State);
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="bonusData">Bonus data.</param>
    /// <param name="level">Level.</param>
    /// <param name="receiveState">Receive state.</param>
    public void BindData(LPCArray bonusData, int level)
    {
        mBonusData = bonusData;
        Level = level;

        Redraw();
    }


    /// <summary>
    /// 设置状态
    /// </summary>
    public void SetState(RECEIVE_STATE state)
    {
        State = state;

        switch (state)
        {
            case  RECEIVE_STATE.UNCOMPLETE:
                mReceiveIcon.gameObject.SetActive(false);
                mLevelLb.gameObject.SetActive(true);
                mLevelLb.text = string.Format(LocalizationMgr.Get("LevelBonusItem_3"), Level);
                GetComponent<UIWidget>().alpha = 0.5f;

                break;
            case  RECEIVE_STATE.RECEIVED:
                mReceiveIcon.gameObject.SetActive(true);
                mLevelLb.gameObject.SetActive(false);
                mReceiveIcon.spriteName = "yes";
                mReceiveLb.text = LocalizationMgr.Get("LevelBonusItem_1");
                GetComponent<UIWidget>().alpha = 1f;

                break;
            case  RECEIVE_STATE.RECEIVING:
                mReceiveIcon.gameObject.SetActive(true);
                mLevelLb.gameObject.SetActive(false);
                mReceiveIcon.spriteName = "circle";
                mReceiveLb.text = LocalizationMgr.Get("LevelBonusItem_2");
                GetComponent<UIWidget>().alpha = 1f;

                break;
        }
    }

    /// <summary>
    /// 设置选中
    /// </summary>
    public void SetSelect(bool isSelect)
    {
        IsSelected = isSelect;

        if (isSelect)
        {
            mTipsAlphaAnim.gameObject.SetActive(true);
            mTipsAlphaAnim.ResetAllToBeginning();
            mTipsAlphaAnim.PlayForward();
        }
        else
            mTipsAlphaAnim.gameObject.SetActive(false);
    }

    #endregion
}
