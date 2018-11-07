/// <summary>
/// UIMark.cs
/// Created by zhangwm 2018/11/05
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMark : MonoBehaviour, IMark
{
    public UIMarkType mUIMarkType = UIMarkType.GameObject;

    public string mCustomComponentTypeName;

    /// <summary>
    /// 获取组件类型名
    /// </summary>
    public string mComponentTypeName
    {
        get
        {
            if (mUIMarkType == UIMarkType.CustomComponent)
                return mCustomComponentTypeName;
            else
                return mUIMarkType.ToString();
        }
    }

    public Transform mTransform
    {
        get
        {
            return transform;
        }
    }
}
