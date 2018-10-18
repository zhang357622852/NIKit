/// <summary>
/// BaggerWnd.cs
/// Created by lic 2016-6-17
/// 包裹界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BaggageWnd : WindowBase<BaggageWnd>
{
    public GameObject closeBtn;

    public GameObject mPetToolTipWnd;

    public GameObject mShowPetWnd;

    public TweenScale mTweenScale;

    public TweenAlpha mTweenAlpha;

    public PetToolTipWnd mPetTooltipWnd;

    public GameObject mMaskGo;

    public FilterWnd mFilterWnd;

    bool mCloseShowMainWnd = true;

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();
    }

    void OnDisable()
    {
        // 重置mCloseShowMainWnd标识
        mCloseShowMainWnd = true;

        // 清空数据
        ClearData();

        //遮罩
        mMaskGo.SetActive(false);

        // 重置打开窗口列表中移除
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void OnEnable()
    {
        // 重新播放缩放动画
        mTweenScale.enabled = true;
        mTweenScale.ResetToBeginning();

        // 重新播放渐变动画
        mTweenAlpha.enabled = true;
        mTweenAlpha.ResetToBeginning();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(closeBtn).onClick = OnCloseBtn;

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// tween动画回调
    /// </summary>
    void OnTweenFinish()
    {
        // 标识动画已经结束
        mPetTooltipWnd.OnTweenFinished();

        // 从正在打开窗口列表中移除
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void ClearData()
    {
        mFilterWnd.ClearData();

        mFilterWnd.gameObject.SetActive(false);

        GameObject multipleSellWnd = WindowMgr.GetWindow(MultipleSellWnd.WndType);
        if (multipleSellWnd != null)
        {
            MultipleSellWnd script = multipleSellWnd.GetComponent<MultipleSellWnd>();

            // 请空数据
            script.ClearData();
        }
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        if (mCloseShowMainWnd)
        {
            GameObject wnd = WindowMgr.GetWindow(MainWnd.WndType);
            if (wnd != null)
                WindowMgr.ShowWindow(wnd);
        }

        // 隐藏窗口
        WindowMgr.HideWindow(gameObject);
    }

    /// <summary>
    /// 绑定打开界面
    /// </summary>
    /// <param name="page">Page.</param>
    public void BindPage(int page, bool closeShowMainWnd = true)
    {
        mPetToolTipWnd.GetComponent<PetToolTipWnd>().BindPage(page);
        mCloseShowMainWnd = closeShowMainWnd;
    }

}
