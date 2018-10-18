/// <summary>
/// SystemSetWnd.cs
/// Created by fengsc 2016/07/05
/// 系统设置窗口
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class SystemSetWnd : MonoBehaviour 
{

    #region 成员变量

    /// <summary>
    ///游戏音效滑动条
    /// </summary>
    public UISlider mGameSoundSlider;

    public UILabel mGameSoundLabel;

    /// <summary>
    ///背景音乐滑动条
    /// </summary>
    public UISlider mBgMusicSlider;

    public UILabel mBgMusicLabel;

    /// <summary>
    ///屏幕是否旋转开关    
    /// </summary>
    public GameObject mRotateSwitch;

    public UILabel mRotateLabel;

    /// <summary>
    //BOSS出场动画开关
    /// </summary>
    public GameObject mAnimationSwitch;

    public UILabel mAnimationLabel;

    /// <summary>
    ///低配模式开关
    /// </summary>
    public GameObject mLowModeSwitch;

    public UILabel mLowModelLabel;

    /// <summary>
    ///省电模式开关
    /// </summary>
    public GameObject mPowerModeSwitch;

    public UILabel mPowerModeLabel;

    /// <summary>
    ///是否旋转屏幕,0表示false;
    /// </summary>
    int IsRotate = 0;

    /// <summary>
    /// 是否打开Boss出场动画,0表示false;
    /// </summary>
    int IsOpenAnimation = 0;

    /// <summary>
    ///是否打开低配模式,0表示false;
    /// </summary>
    int IsOpenLowMode = 0;

    /// <summary>
    ///是否打开省电模式,0表示false;
    /// </summary>
    int IsPowerMode = 0;

    /// <summary>
    ///TweenPosition组件的From坐标
    /// </summary>
    Vector3 mFrom;

    /// <summary>
    ///TweenPosition组件的To坐标
    /// </summary>
    Vector3 mTo;

    #endregion

    // Use this for initialization
    void Start () 
    {
        //初始化label;
        InitLabel();

        //初始化玩家保存的设置数据;
        PlayerSaveSetData();

        //注册事件;
        RegisterEvent();
    }

    /// <summary>
    ///脚本销毁时保存玩家数据
    /// </summary>
    void OnDestroy()
    {
        OptionMgr.SetPublicOption("is_rotate", LPCValue.Create(IsRotate));
        OptionMgr.SetPublicOption("is_open_animation", LPCValue.Create(IsOpenAnimation));
        OptionMgr.SetPublicOption("is_open_low_mode", LPCValue.Create(IsOpenLowMode));
        OptionMgr.SetPublicOption("is_power_mode", LPCValue.Create(IsPowerMode));
    }

    /// <summary>
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mRotateSwitch).onClick = OnClickRotateSwitch;
        UIEventListener.Get(mAnimationSwitch).onClick = OnClickAnimSwitch;
        UIEventListener.Get(mLowModeSwitch).onClick = OnClickLowModeSwitch;
        UIEventListener.Get(mPowerModeSwitch).onClick = OnClickPowerModeSwitch;

        EventDelegate.Add(mGameSoundSlider.onChange, new EventDelegate.Callback(OnGameMusicChange));
        EventDelegate.Add(mBgMusicSlider.onChange, new EventDelegate.Callback(OnBgMusicChange));
    }

    void OnGameMusicChange()
    {
        //设置游戏音效的音量
        GameSoundMgr.SetSoundVolume(mGameSoundSlider.value);
    }

    void OnBgMusicChange()
    {
        //背景音乐音量；
        GameSoundMgr.SetMusicVolume( mBgMusicSlider.value);
    }

    /// <summary>
    ///设置开关的选项
    /// </summary>
    void PlayerSaveSetData()
    {
        // 获取TweenPosition组件的From坐标
        mFrom = mRotateSwitch.GetComponent<TweenPosition>().from;

        // 获取TweenPosition组件的To坐标;
        mTo = mRotateSwitch.GetComponent<TweenPosition>().to;

        // 设值初始音效大小;
        mGameSoundSlider.value = GameSoundMgr.GetSoundVolume();

        // 设置初始背景音乐音量;
        mBgMusicSlider.value = GameSoundMgr.GetMusicVolume();

        // 获取玩家保存的设置数据;
        IsRotate = OptionMgr.GetPublicOption("is_rotate").AsInt;
        IsOpenAnimation = OptionMgr.GetPublicOption("is_open_animation").AsInt;
        IsOpenLowMode = OptionMgr.GetPublicOption("is_open_low_mode").AsInt;
        IsPowerMode = OptionMgr.GetPublicOption("is_power_mode").AsInt;

        // 旋转屏幕开关处于ON状态;
        if(IsRotate == 1)
        {
            mRotateSwitch.transform.localPosition = mFrom;

            // 设置屏幕转为横向左;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
        }
        // 旋转屏幕开关处于OFF状态;
        else
        {
            mRotateSwitch.transform.localPosition = mTo;

            //设置屏幕转为横向右;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = true;
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
        }

        // Boss出场动画开关处于ON状态;
        if(IsOpenAnimation == 1)
            mAnimationSwitch.transform.transform.localPosition = mFrom;
        else
            mAnimationSwitch.transform.localPosition = mTo;

        // 低配模式开关处于ON状态;
        if(IsOpenLowMode == 1)
            mLowModeSwitch.transform.localPosition = mFrom;
        else
            mLowModeSwitch.transform.localPosition = mTo;

        // 省电模式开关处于ON状态;
        if(IsPowerMode == 1)
            mPowerModeSwitch.transform.localPosition = mFrom;
        else
            mPowerModeSwitch.transform.localPosition = mTo;
    }

    /// <summary>
    /// 初始化界面的label
    /// </summary>
    void InitLabel()
    {
        mGameSoundLabel.text = LocalizationMgr.Get("SystemWnd_7");
        mBgMusicLabel.text = LocalizationMgr.Get("SystemWnd_8");
        mRotateLabel.text = LocalizationMgr.Get("SystemWnd_9");
        mAnimationLabel.text = LocalizationMgr.Get("SystemWnd_10");
        mLowModelLabel.text = LocalizationMgr.Get("SystemWnd_11");
        mPowerModeLabel.text = LocalizationMgr.Get("SystemWnd_12");

    }

    /// <summary>
    ///屏幕是否旋转开关点击事件
    /// </summary>
    void OnClickRotateSwitch(GameObject go)
    {
        if(IsRotate == 1)
        {
            //启用TweenPosition组件;
            TweenPosition.Begin(go, 0.15f, mTo);

            IsRotate = 0;
        }
        else if(IsRotate == 0)
        {
            TweenPosition.Begin(go, 0.15f, mFrom);

            IsRotate = 1;
        }
    }

    /// <summary>
    ///出场动画开关点击事件
    /// </summary>
    void OnClickAnimSwitch(GameObject go)
    {
        if(IsOpenAnimation == 1)
        {
            //启用TweenPosition组件;
            TweenPosition.Begin(go, 0.15f, mTo);

            IsOpenAnimation = 0;
        }
        else if(IsOpenAnimation == 0)
        {
            TweenPosition.Begin(go, 0.15f, mFrom);

            IsOpenAnimation = 1;
        }
    }

    /// <summary>
    ///低配模式开关点击事件
    /// </summary>
    void OnClickLowModeSwitch(GameObject go)
    {
        if(IsOpenLowMode == 1)
        {
            //启用TweenPosition组件;
            TweenPosition.Begin(go, 0.15f, mTo);

            IsOpenLowMode = 0;
        }
        else if(IsOpenLowMode == 0)
        {
            TweenPosition.Begin(go, 0.15f, mFrom);

            IsOpenLowMode = 1;
        }
    }

    /// <summary>
    ///省电模式点击事件
    /// </summary>
    void OnClickPowerModeSwitch(GameObject go)
    {
        if(IsPowerMode == 1)
        {
            //启用TweenPosition组件;
            TweenPosition.Begin(go, 0.15f, mTo);

            IsPowerMode = 0;
        }
        else if(IsPowerMode == 0)
        {
            TweenPosition.Begin(go, 0.15f, mFrom);

            IsPowerMode = 1;
        }
    }
}
