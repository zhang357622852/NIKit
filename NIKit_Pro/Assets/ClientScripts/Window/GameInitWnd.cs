/// <summary>
/// GameInitWnd.cs
/// Created by fengsc 2017/03/14
/// 游戏初始化界面
/// </summary>
using UnityEngine;
using System.Collections;

public class GameInitWnd : WindowBase<GameInitWnd>
{
    public GameObject mBg;
    public GameObject mSplashLogo;

    // Use this for initialization
    void Start ()
    {
        mSplashLogo.GetComponent<UISpriteAnimation>().onPlayFinish += OnSprAniFinish;
        EventDelegate.Add(mBg.GetComponent<TweenAlpha>().onFinished, OnTweenFinish);

        StartCoroutine(PlayLogoAni());
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    private IEnumerator PlayLogoAni()
    {
        yield return new UnityEngine.WaitForSeconds(0.2f);

        mSplashLogo.GetComponent<UISpriteAnimation>().ResetToBeginning();
        mSplashLogo.GetComponent<UISpriteAnimation>().enabled = true;
    }

    /// <summary>
    /// 播放完成回调
    /// </summary>
    void OnSprAniFinish(bool isRevers)
    {
        StartCoroutine(WaitForInit());
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    IEnumerator WaitForInit()
    {
        // 固定显示1s
        yield return new UnityEngine.WaitForSeconds(1f);

        // 等到初始化完成
        while(!ConfigMgr.InitSuccessed)
            yield return null;

        // 播放淡出动画
        mSplashLogo.GetComponent<TweenAlpha>().ResetToBeginning();
        mSplashLogo.GetComponent<TweenAlpha>().PlayForward();

        mBg.GetComponent<TweenAlpha>().ResetToBeginning();
        mBg.GetComponent<TweenAlpha>().PlayForward();
    }

    /// <summary>
    /// 播放Tween完成
    /// </summary>
    void OnTweenFinish()
    {
        // 执行事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_MSG_START_ANIMATION_FINISH, null);

        // 播放相应的场景背景音效
        GameSoundMgr.PlayStartBgmMusic("login.wav", true);
    }
}
