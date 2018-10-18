/// <summary>
/// GiftTipsAnimWnd.cs
/// Created by zhangwm 2018/08/29
/// 礼包提示动画界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GiftTipsAnimWnd : WindowBase<GiftTipsAnimWnd>
{
    #region 成员变量
    public UITexture mIcon;

    public GameObject mBgEffect;

    public GameObject mItemParent;

    public TweenPosition mTweenPos;

    public TweenAlpha mItemTA;

    public TweenScale mItemTS;

    private int mGiftClassId;

    // 动画终点目标
    private Transform mItemTransform;
    #endregion

    private void Start()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    private void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    private void Update()
    {
        if (mBgEffect != null)
            mBgEffect.transform.Rotate(Vector3.forward * Time.unscaledDeltaTime * 40);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    private void Redraw()
    {
        // icon
        mIcon.mainTexture = ItemMgr.GetTexture(ItemMgr.GetClearIcon(mGiftClassId));

        if (ME.user == null)
        {
            CloseWindow();
            return;
        }

        CsvRow row = MarketMgr.GetMarketConfig(mGiftClassId);

        if (row == null)
        {
            CloseWindow();
            return;
        }

        LPCValue scriptNo = row.Query<LPCValue>("tips_menu_script");

        // 没有配置显示脚本
        if (scriptNo == null || !scriptNo.IsInt || scriptNo.AsInt == 0)
        {
            CloseWindow();
            return;
        }

        mItemTransform = (Transform)ScriptMgr.Call(scriptNo.AsInt, ME.user, row.ConvertLpcMap());

        // 复制功能item
        if (mItemTransform == null)
        {
            CloseWindow();
            return;
        }

        Vector3 pos = transform.InverseTransformPoint(mItemTransform.position);

        GameObject go = NGUITools.AddChild(mItemParent, mItemTransform.gameObject);

        BoxCollider boxcol = go.GetComponentInChildren<BoxCollider>();

        if (boxcol != null)
            boxcol.enabled = false;

        mItemParent.transform.localPosition = pos;
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    private void PlayAnimation()
    {
        if (mItemTransform == null)
        {
            CloseWindow();
            return;
        }

        mTweenPos.enabled = true;
        mTweenPos.to = transform.InverseTransformPoint(mItemTransform.position);
        mTweenPos.from = mTweenPos.transform.localPosition;
        mTweenPos.ResetToBeginning();
        mTweenPos.PlayForward();

        EventDelegate.Add(mTweenPos.onFinished, OnTweenPositionFinished);
    }

    /// <summary>
    /// 动画结束回调
    /// </summary>
    private void OnTweenPositionFinished()
    {
        mItemTA.enabled = true;
        mItemTA.ResetToBeginning();
        mItemTA.PlayForward();

        mItemTS.enabled = true;
        mItemTS.ResetToBeginning();
        mItemTS.PlayForward();

        EventDelegate.Add(mItemTS.onFinished, OnItemAnimationFinished);
    }

    /// <summary>
    /// 动画结束回调
    /// </summary>
    private void OnItemAnimationFinished()
    {
        CloseWindow();
    }

    /// <summary>
    /// 关闭界面
    /// </summary>
    private void CloseWindow()
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="giftClassId"></param>
    /// <param name="wndName"></param>
    public void BindData(int giftClassId)
    {
        mGiftClassId = giftClassId;

        Redraw();

        PlayAnimation();
    }
}
