/// <summary>
/// LuaCsMgr.cs
/// Create by zhaozy 2018-07-13
/// lua cs 相关管理模块
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using UnityEngine;
using LPC;
using XLua;

/// <summary>
/// AccountMgr账户管理模块
/// </summary>
public static class LuaCsMgr
{
    #region 变量

    /// <summary>
    /// The m lua env ob.
    /// </summary>
    private static LuaEnv mLuaEnvOb;

    #endregion

    #region 属性

    /// <summary>
    /// The luaenv.
    /// </summary>
    public static LuaEnv LuaEnvOb
    {
        get
        {
            // 如果LuaEnv对象存在则，不在重复创建
            if (mLuaEnvOb != null)
                return mLuaEnvOb;

            // 创建一个LuaEnv
            mLuaEnvOb = new LuaEnv();

            // 返回mLuaEnvOb
            return mLuaEnvOb;
        }
    }

    #endregion

    #region LuaCallCSharp接口

    #region 创建List

    /// <summary>
    /// lua创建List
    /// </summary>
    /// <returns>The list object.</returns>
    public static List<object> CreateList()
    {
        return new List<object>();
    }

    #endregion

    #region Char相关

    /// <summary>
    /// 将Property转换为Char
    /// </summary>
    /// <returns>The list object.</returns>
    public static Char PropertyToChar(Property ob)
    {
        return ob as Char;
    }

    #endregion

    #endregion
}
