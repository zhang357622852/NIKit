/// <summary>
/// SplashWnd.cs
/// Created by zhaozy 2016/07/05
/// 闪屏窗口
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 闪屏窗口
/// </summary>
public class SplashWnd : WindowBase<SplashWnd>
{
    #region 公共字段

    [System.Serializable]
    public class AlphaAniCurve
    {
        public AnimationCurve AlphaCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float AlphaFrom = 1f;
        public float AlphaTo = 0f;
        public float duration = 1f;
    }

    // 动画控件
    public TweenAlpha tweenWnd;
    public UISprite bgWnd;

    // 闪屏动画列表
    public List<AlphaAniCurve> AlphaCurveList = new List<AlphaAniCurve>();

    // 当前CurveId
    private int mCurveId = -1;

    #endregion

    #region 内部方法

    /// <summary>
    /// Start this instance.
    /// </summary>
    void Awake()
    {
        // 动画结束
        tweenWnd.AddOnFinished(OnFinished);
    }

    /// <summary>
    /// Raises the finished event.
    /// </summary>
    void OnFinished()
    {
        // 注册事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_INSTANCE_SPLASH_END, MixedValue.NewMixedValue<int>(mCurveId));

        // 关闭窗口自己(暂时不能隐藏，有bug会导致副本过图闪屏)
        // 原因是过图闪屏结束如果将自己隐藏，副本转阶段是在异步协程中处理的，中间会有一段空窗期
        // WindowMgr.HideWindow(gameObject);
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 设置闪屏动画
    /// </summary>
    public void SetSplashCurve(int curveId)
    {
        // 没有该动画
        if (AlphaCurveList.Count < curveId)
            return;

        // 记录当前curveId
        mCurveId = curveId;

        // 获取动画
        AlphaAniCurve aniCurve = AlphaCurveList[mCurveId];

        // 设置动画数据
        tweenWnd.animationCurve = aniCurve.AlphaCurve;
        tweenWnd.from = aniCurve.AlphaFrom;
        tweenWnd.to = aniCurve.AlphaTo;
        tweenWnd.duration = aniCurve.duration;

        // 设置窗口当前alpha
        bgWnd.alpha = aniCurve.AlphaFrom;

        // 重新播放闪屏效果
        tweenWnd.ResetToBeginning();
        tweenWnd.enabled = true;
    }

    #endregion
}
