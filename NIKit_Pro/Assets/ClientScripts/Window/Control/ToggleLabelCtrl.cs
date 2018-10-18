using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 文本开关控制脚本
/// </summary>
public class ToggleLabelCtrl : IToggle
{
    // 文本
    public UILabel mDes;

    //  关闭时的透明度值
    public float offAlpha = 1f;

    /// <summary>
    /// 开关关闭处理事件
    /// </summary>
    protected override void SwitchToOff()
    {
        mDes.alpha = offAlpha;
    }

    /// <summary>
    /// 开关打开处理事件
    /// </summary>
    protected override void SwitchToOn()
    {
        mDes.alpha = 1f;
    }

    /// <summary>
    /// 设置语言文本
    /// </summary>
    /// <param name="key"></param>
    public void SetDes(string key)
    {
        mDes.text = LocalizationMgr.Get(key);
    }

    /// <summary>
    /// 设置文本颜色
    /// </summary>
    /// <param name="color"></param>
    public void SetDesColor(string color)
    {
        mDes.color = ColorConfig.ParseToColor(color);
    }

}
