/// <summary>
/// SetToggleLabel.cs
/// Created by fengsc 2017/05/17
/// 设置toggle选中前后label的颜色
/// </summary>
using UnityEngine;
using System.Collections;

public class SetToggleLabel : MonoBehaviour
{
    public UIToggle mToggle;

    public UILabel mLabel;

    // 选中前的颜色
    public Color mBeforeColor;

    // 选中后的颜色
    public Color mAfterColor;

    // Use this for initialization
    void Start ()
    {
        // 注册回调
        EventDelegate.Add(mToggle.onChange, OnChangeCallBack);
    }

    /// <summary>
    /// toggle值变化回调
    /// </summary>
    void OnChangeCallBack()
    {
        if (mToggle.value)
        {
            mLabel.color = mAfterColor;
        }
        else
        {
            mLabel.color = mBeforeColor;
        }
    }
}
