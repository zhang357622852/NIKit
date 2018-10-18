/// <summary>
/// LoadingSplashWnd.cs
/// Created by lic 2017/03/29
/// 闪屏窗口
/// </summary>

using UnityEngine;
using System.Collections;

public class LoadingSplashWnd : WindowBase<LoadingSplashWnd>
{
    public TweenAlpha tweenWnd;

    private int mType = -1;

	// Use this for initialization
	void Start ()
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
        EventMgr.FireEvent(EventMgrEventType.EVENT_RES_LOADING_WND_SPLASH_OK, MixedValue.NewMixedValue<int>(mType));
    }

    /// <summary>
    /// 绑定类型
    /// </summary>
    /// <param name="type">Type.</param>
    public void BindType(int type)
    {
        mType = type;
    }
}
