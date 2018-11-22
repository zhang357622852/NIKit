/// <summary>
/// IMark.cs
/// Created by WinMi 2018/11/05
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 填写组件类型名
/// </summary>
public enum UIMarkType
{
    GameObject,

    UILabel,

    UISprite,

    UIButton,

    UITexture,

    UIGrid,

    UITable,

    /// <summary>
    /// 自定义组件
    /// </summary>
    CustomComponent,
}

public interface IMark
{
    string mComponentTypeName { get; }

    Transform mTransform { get; }
}
