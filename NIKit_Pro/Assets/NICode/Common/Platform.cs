using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 平台判断
/// </summary>
public static class Platform
{

    /// <summary>
    /// 是否是安卓模式
    /// </summary>
    public static bool IsAndroid
    {
        get
        {
            bool retValue = false;

#if UNITY_ANDROID
            retValue = true;
#endif
            return retValue;
        }
    }

    /// <summary>
    /// 是否是编辑器模式
    /// </summary>
    public static bool IsEditor
    {
        get
        {
            bool retValue = false;

#if UNITY_EDITOR
            retValue = true;
#endif
            return retValue;
        }
    }

    /// <summary>
    /// 是否是ios模式
    /// </summary>
    public static bool IsIos
    {
        get
        {
            bool retValue = false;

#if UNITY_IOS
                retValue = true;
#endif
            return retValue;
        }
    }

    /// <summary>
    /// 是否是iphone模式
    /// </summary>
    public static bool IsIphone
    {
        get
        {
            bool retValue = false;

#if UNITY_IPHONE
                retValue = true;
#endif
            return retValue;
        }
    }

    /// <summary>
    /// 是否是StandaloneWin模式
    /// </summary>
    public static bool IsStandaloneWin
    {
        get
        {
            bool retValue = false;

#if UNITY_STANDALONE_WIN
            retValue = true;
#endif
            return retValue;
        }
    }

    /// <summary>
    /// 是否是StandaloneLinux模式
    /// </summary>
    public static bool IsStandaloneLinux
    {
        get
        {
            bool retValue = false;

#if UNITY_STANDALONE_LINUX
            retValue = true;
#endif
            return retValue;
        }
    }
}
