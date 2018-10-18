/// <summary>
/// MaskWnd.cs
/// 
/// </summary>
using UnityEngine;
using System.Collections;

public class MaskWnd : WindowBase<MaskWnd>
{
    public UISpriteAnimation mSpriteAnimation;

    CallBack mCallBack;

    void Awake()
    {
        mSpriteAnimation.onPlayFinish += OnPlayFinish;
    }

    /// <summary>
    /// 播放完成回调
    /// </summary>
    void OnPlayFinish(bool isRevers)
    {
        if (isRevers)
        {
            WindowMgr.DestroyWindow(MaskWnd.WndType);
        }
        else
        {
            if (mCallBack != null)
                mCallBack.Go();
        }
    }

    public void PlayerRevers()
    {
        mSpriteAnimation.frameIndex = 9;

        mSpriteAnimation.PlayReverse();
    }

    public void Play()
    {
        mSpriteAnimation.frameIndex = 0;

        mSpriteAnimation.enabled = true;

        mSpriteAnimation.Play();
    }

    public void Bind(CallBack callBack)
    {
        mCallBack = callBack;
    }
}
