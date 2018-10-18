/// <summary>
/// IToggle.cs
/// Created by zhangwm 2018/08/21
/// 开关抽象类
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IToggle : MonoBehaviour
{
    // 勾
    public GameObject mHook;

    public TweenAlpha mTweenAlpha;

    public delegate void ToggleDelegate(bool isOn, object arg);

    protected ToggleDelegate mCallback;

    protected object mArg;

    public ToggleDelegate Callback {
        get
        {
            return mCallback;
        }
        set
        {
            mCallback = value;
        }
    }

    private bool mIsOn;

    public bool IsOn { get { return mIsOn; } }

    private void Awake()
    {
        UIEventListener.Get(gameObject).onClick = OnSwitch;
    }

    /// <summary>
    /// 点击事件处理
    /// </summary>
    /// <param name="go"></param>
    private void OnSwitch(GameObject go)
    {
        SetState(mIsOn ? false : true);
    }

    /// <summary>
    /// 开关打开
    /// </summary>
    protected virtual void SwitchToOn() { }

    /// <summary>
    /// 开关关闭
    /// </summary>
    protected virtual void SwitchToOff() { }

    /// <summary>
    /// 触发事件
    /// </summary>
    protected void TriggleEvent()
    {
        if (mCallback != null)
            mCallback(mIsOn, mArg);
    }

    #region 外部接口

    /// <summary>
    /// 设置开关状态，并且刷新状态和触发事件
    /// </summary>
    /// <param name="isOn"></param>
    public void SetState(bool isOn)
    {
        mIsOn = isOn;

        RefreshState();

        TriggleEvent();
    }

    /// <summary>
    /// 设置开关状态，只刷新状态，不触发事件
    /// </summary>
    /// <param name="isOn"></param>
    public void SetStateNoTrigger(bool isOn)
    {
        mIsOn = isOn;

        RefreshState();
    }

    /// <summary>
    /// 刷新状态
    /// </summary>
    public void RefreshState()
    {
        if (mIsOn && mTweenAlpha != null)
        {
            mTweenAlpha.enabled = true;
            mTweenAlpha.ResetToBeginning();
        }

        mHook.SetActive(mIsOn);

        if (mIsOn)
            SwitchToOn();
        else
            SwitchToOff();
    }

    /// <summary>
    /// 绑定参数
    /// </summary>
    /// <param name="arg"></param>
    public virtual void BindData(object arg)
    {
        mArg = arg;
    }
    #endregion

}
