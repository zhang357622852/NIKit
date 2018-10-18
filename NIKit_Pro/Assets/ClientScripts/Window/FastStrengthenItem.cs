/// <summary>
/// FastStrengthenItem.cs
/// Created by fengsc 2017/12/21
/// 装备快速强化基础格子
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class FastStrengthenItem : WindowBase<FastStrengthenItem>
{
    // 强化次数描述
    public UILabel mCountDesc;

    public TweenAlpha mParent;

    // 强化结果
    public UILabel mResultDesc;

    // 强化等级
    public UILabel mRank;

    // 属性强化提示
    public UILabel mTips;

    // 属性描述
    public UILabel mPropDesc;

    public GameObject mMask;

    // 装备对象
    Property mEquipOb;

    LPCArray mProp = LPCArray.Empty;

    // 强化次数
    int mCurIntensifyCount = 0;

    // 强化结果
    int mResult = -1;

    // 是否是新增属性
    bool mIsAdd = false;

    // 是否金钱不足
    bool mIsNoMoney = false;

    // 强化之前的装备等级
    int mBeforeRank = 0;

    // 是否可以继续强化
    bool mIsIntensify = false;

    TweenAlpha[] mMaskAlpha;

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mMaskAlpha = mMask.GetComponents<TweenAlpha>();

        // 添加回调
        EventDelegate.Add(mParent.onFinished, OnParentAlphaFinish);

        // 强化次数
        mCountDesc.text = string.Format(LocalizationMgr.Get("FastStrengthenWnd_8"), mCurIntensifyCount);

        if (mBeforeRank + 1 > GameSettingMgr.GetSettingInt("equip_intensify_limit_level"))
        {
            // 强化等级达到上限
            mResultDesc.text = LocalizationMgr.Get("FastStrengthenWnd_9");

            // TODO :渐变颜色
            mResultDesc.gradientBottom = new Color(255f / 255f, 185f / 255f, 0f);

            mRank.text = string.Empty;

            mTips.text = string.Empty;

            mPropDesc.text = string.Empty;

            mProp = LPCArray.Empty;

            mParent.ResetToBeginning();
            mMaskAlpha[0].ResetToBeginning();
            mMaskAlpha[1].ResetToBeginning();

            mParent.PlayForward();
            mMaskAlpha[0].PlayForward();
            mMaskAlpha[1].PlayForward();

            return;
        }

        if (mIsNoMoney)
        {
            // 金钱不足
            mResultDesc.text = LocalizationMgr.Get("FastStrengthenWnd_7");

            // TODO :渐变颜色
            mResultDesc.gradientBottom = new Color(255f / 255f, 45f / 255f, 45f / 255f);

            mRank.text = string.Empty;

            mTips.text = string.Empty;

            mPropDesc.text = string.Empty;
        }
        else
        {
            if (mResult == 1)
            {
                // 强化成功
                mResultDesc.text = LocalizationMgr.Get("FastStrengthenWnd_3");

                // 渐变颜色
                mResultDesc.gradientBottom = new Color(91f / 255f, 255f / 255f, 60f / 255f);

                // 装备强化等级
                mRank.text = string.Format("+{0}", mEquipOb.GetRank());

                Color color = new Color(173f / 255f, 255f / 255f, 167f / 255f);

                mTips.color = color;

                mPropDesc.color = color;

                if (mIsAdd)
                {
                    // 新增属性
                    mTips.text = LocalizationMgr.Get("FastStrengthenWnd_6");

                }
                else
                {
                    // 属性描述
                    if (mProp != null && mProp.Count != 0)
                    {
                        // 增强属性
                        mTips.text = LocalizationMgr.Get("FastStrengthenWnd_5");
                    }
                    else
                    {
                        mTips.text = string.Empty;

                        mPropDesc.text = string.Empty;
                    }
                }

                // 属性描述
                if (mProp != null && mProp.Count != 0)
                    mPropDesc.text = PropMgr.GetPropDesc(mProp);
            }
            else
            {
                // 强化失败
                mResultDesc.text = LocalizationMgr.Get("FastStrengthenWnd_4");

                // 渐变颜色
                mResultDesc.gradientBottom = new Color(255f / 255f, 45f / 255f, 45f / 255f);

                mRank.text = string.Empty;

                mTips.text = string.Empty;

                mPropDesc.text = string.Empty;
            }

            if (!mIsIntensify)
            {
                mTips.text = LocalizationMgr.Get("EquipStrengthenWnd_20");

                mPropDesc.text = string.Empty;

                mTips.color = new Color(255f/255f, 141f/255f, 141f/255f);
            }
        }

        mProp = LPCArray.Empty;

        mParent.ResetToBeginning();
        mMaskAlpha[0].ResetToBeginning();
        mMaskAlpha[1].ResetToBeginning();

        mParent.PlayForward();
        mMaskAlpha[0].PlayForward();
        mMaskAlpha[1].PlayForward();
    }

    /// <summary>
    /// alpha动画执行完成回调
    /// </summary>
    void OnParentAlphaFinish()
    {
        GameObject wnd = WindowMgr.GetWindow(FastStrengthenWnd.WndType);
        if (wnd == null)
            return;

        // 通知服务器强化装备
        wnd.GetComponent<FastStrengthenWnd>().SendMessage();

        wnd.GetComponent<FastStrengthenWnd>().RefreshEquipDesc();
    }

    public void Bind(Property equipOb, LPCArray prop, int curIntensifyCount, int result, bool isAdd, bool isNoMoney, int beforeRank, bool isIntensify)
    {
        if (equipOb == null)
            return;

        mEquipOb = equipOb;

        mProp = prop;

        mCurIntensifyCount = curIntensifyCount;

        mResult = result;

        mIsNoMoney = isNoMoney;

        mBeforeRank = beforeRank;

        mIsIntensify = isIntensify;

        // 绘制窗口
        Redraw();
    }
}
