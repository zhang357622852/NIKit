/// <summary>
/// OptionItem.cs
/// Created by xuhd Sec/04/2014
/// 选项数据结构
/// </summary>
using UnityEngine;
using System.Collections;

public class OptionItem
{
    #region 公共字段

    /// <summary>
    /// 选项名
    /// </summary>
    /// <value>The name of the option.</value>
    public string OptionName
    {
        get
        {
            return mOptionName;
        }
    }

    /// <summary>
    /// 回调
    /// </summary>
    /// <value>The callback.</value>
    public OptionItemWnd.ItemClickedDelegate Callback
    {
        get
        {
            return mCallback;
        }
    }
    #endregion

    #region 私有字段
    private string mOptionName;               // 选项名称
    private OptionItemWnd.ItemClickedDelegate mCallback;      // 选项回调
    #endregion

    /// <summary>
    /// 构造一个调试选项
    /// </summary>
    /// <param name="optionName">Option name.</param>
    /// <param name="callback">Callback.</param>
    public OptionItem(string optionName, OptionItemWnd.ItemClickedDelegate callback)
    {
        mOptionName = optionName;
        mCallback = callback;
    }
}
