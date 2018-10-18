/// <summary>
/// ClickEffectWnd.cs
/// Created by wangxw 2015-01-31
/// 点击效果窗口
/// </summary>

using UnityEngine;
using System.Collections;

public partial class ClickEffectWnd : WindowBase<ClickEffectWnd>
{
    #region 公共函数

    void Awake()
    {
        UIEventShowAndHide.Get(gameObject).OnShow = OnShow;
    }

    // Use this for initialization
    void OnShow()
    {
        // 重播动画
        TweenRotation tr = gameObject.GetComponentInChildren<TweenRotation>();
        tr.ResetToBeginning();
        tr.PlayForward();
        TweenScale ts = gameObject.GetComponentInChildren<TweenScale>();
        ts.ResetToBeginning();
        ts.PlayForward();
        TweenAlpha ta = gameObject.GetComponentInChildren<TweenAlpha>();
        ta.ResetToBeginning();
        ta.PlayForward();

        // 跟随鼠标
        gameObject.transform.position = SceneMgr.UiCamera.ScreenToWorldPoint(Input.mousePosition);
    }

    /// <summary>
    /// 动画播放结束
    /// </summary>
    public void OnFinished()
    {
        Hide();
    }

    #endregion
}
