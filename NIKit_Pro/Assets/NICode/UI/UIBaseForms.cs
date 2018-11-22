using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIBaseFormsRoot : MonoBehaviour
{
    [Tooltip("窗口类型")]
    public UIFormsType mFormsType = UIFormsType.Normal;

    [Tooltip("窗口生命周期类型")]
    public UIFormsLifeType mFormsLifeType = UIFormsLifeType.HumanLife;

    [Tooltip("窗口层级类型")]
    public UIFormsLayer mFormsLayerType = UIFormsLayer.CommonUILayer;

    /// <summary>
    /// 生命周期中只会执行一次,类似Awake
    /// </summary>
    public abstract void Init();

    /// <summary>
    /// 隐藏
    /// </summary>
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 每次通过ShowForms打开，都会被调用
    /// PopForms出战的方式打开的话，是不调用的
    /// </summary>
    public abstract void Show();

    /// <summary>
    /// 结束-在销毁之前调用
    /// </summary>
    public virtual void End()
    {

    }
}

[RequireComponent(typeof(UIPanel))]
public abstract class UIBaseForms<T> : UIBaseFormsRoot
{
    /// <summary>
    /// 窗口名
    /// </summary>
    public static readonly string FormsName = typeof(T).Name;
}
