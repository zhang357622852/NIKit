/// <summary>
/// NPCTipsWnd.cs
/// Created by fengsc 2018/08/27
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class NPCTipsWnd : WindowBase<NPCTipsWnd>
{
    public UILabel mDesc;

    public UIPanel mPanel;

    public TweenAlpha mTweenAlpha;

    public UISprite mBg;

    public Vector3 mOffset = Vector3.zero;

    private int mGroup = 0;

    private bool mIsFadeOut = false;

    private Vector3 mTarget = Vector3.zero;

    void OnEnable()
    {
        // 绘制窗口
        Redraw();
    }

    // Use this for initialization
    void Start ()
    {
        UIEventListener.Get(mBg.gameObject).onClick = OnClickBg;

        // 注册动画播放完成回调
        EventDelegate.Add(mTweenAlpha.onFinished, OnFinish);
    }

    void OnDisable()
    {
        CancelInvoke("ShowTips");
        CancelInvoke("FadeOutWindow");
    }

    void Update()
    {
        // 更新位置
        UpdatePos();
    }

    /// <summary>
    /// 背景点击回调
    /// </summary>
    void OnClickBg(GameObject go)
    {
        // 淡出
        FadeOutWindow();
    }

    /// <summary>
    /// 窗口淡出
    /// </summary>
    void FadeOutWindow()
    {
        // 正在淡出
        if (mIsFadeOut)
            return;

        // 淡出
        mTweenAlpha.from = 1.0f;

        mTweenAlpha.to = 0f;

        // 播放动画
        mTweenAlpha.PlayForward();

        mTweenAlpha.ResetToBeginning();
    }

    void OnFinish()
    {
        // 标识淡出结束
        mIsFadeOut = false;

        // 隐藏当前窗口
        WindowMgr.HideWindow(NPCTipsWnd.WndType);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        int active = 0;

        // 是否激活过评价系统
        LPCValue active_evaluate = OptionMgr.GetOption(ME.user, "active_evaluate");
        if (active_evaluate != null && active_evaluate.IsInt)
            active = active_evaluate.AsInt;

        if (active == 0)
            mGroup = 0;
        else
            mGroup = 1;

        mPanel.alpha = 0;

        CancelInvoke("ShowTips");

        // 0~5秒内弹出悬浮对话
        Invoke("ShowTips", Random.Range(0, 6));
    }

    /// <summary>
    /// 显示提示信息
    /// </summary>
    void ShowTips()
    {
        // 抽取提示信息
        mDesc.text = SystemTipsMgr.FetchTip("evaluate", LPCMapping.Empty, mGroup);

        mPanel.alpha = 1.0f;

        mBg.width = mDesc.width + 50;

        // 显示完成立即更新一次位置
        UpdatePos();

        CancelInvoke("FadeOutWindow");

        // 5秒后淡出窗口
        Invoke("FadeOutWindow", 5);
    }

    void UpdatePos()
    {
        if (this == null)
            return;

        // 窗口隐藏不处理
        if (mPanel.alpha == 0 || !gameObject.activeSelf)
            return;

        Vector3 position = Game.WorldToUI(mTarget);

        // 判断位置是否需要变化
        if (Game.FloatEqual((transform.position - position).sqrMagnitude, 0f))
            return;

        transform.position = position;
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="targetPos">Target position.</param>
    public void Bind(Vector3 targetPos)
    {
        mTarget = targetPos;
    }
}
