using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 窗体的抽象基类
/// </summary>
[RequireComponent(typeof(UIPanel))]
public abstract class UIBaseForms : MonoBehaviour
{
    public UIFormsType formsType = UIFormsType.Normal;
    public UIFormsLifeType formsLifeType = UIFormsLifeType.HumanLife;
    public UIFormsLayer formsLayerType = UIFormsLayer.CommonUILayer;
    public string formsName;

    /// <summary>
    /// 第一次创建窗口时调用，相当于init
    /// </summary>
    public abstract void Show();

    /// <summary>    
    /// 隐藏
    /// </summary>
    public virtual void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 再次显示
    /// </summary>
    public virtual void ReShow()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 结束-在销毁之前调用
    /// </summary>
    public virtual void End()
    {

    }

    
}
