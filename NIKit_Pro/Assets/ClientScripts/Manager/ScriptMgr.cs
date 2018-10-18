/// <summary>
/// ScriptMgr.cs
/// Created by zhaozy 2014-11-22
/// 脚本管理
/// </summary>

using System;
using System.Collections.Generic;
using System.Reflection;
using XLua;

// 脚本管理
public static class ScriptMgr
{
    #region 变量

    // 脚本列表
    static Dictionary<int, Script> mScripts = new Dictionary<int, Script>();

    /// <summary>
    /// The m lua scripts.
    /// </summary>
    static Dictionary<int, string> mLuaScripts = new Dictionary<int, string>();

    [CSharpCallLua]
    public delegate object ScriptDelegate(params object[] args);

    #endregion

    #region 公共接口

    /// <summary>
    /// Adds the lua script.
    /// </summary>
    /// <param name="scriptName">Script name.</param>
    /// <param name="scriptText">Script text.</param>
    public static void AddLuaScript(int sptNo, string scriptText)
    {
        // 已经有该公式名，需要移除
        if (mLuaScripts.ContainsKey(sptNo))
            mLuaScripts.Remove(sptNo);

        mLuaScripts.Add(sptNo, scriptText);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 收集Script
        Assembly asse = Assembly.GetExecutingAssembly();
        Type[] types = asse.GetTypes();
        foreach (Type t in types)
        {
            if (t.IsSubclassOf(typeof(Script)))
            {
                string strSptNo = t.Name.Replace("SCRIPT_", "");
                int sptNo;
                if (! int.TryParse(strSptNo, out sptNo))
                {
                    LogMgr.Trace("脚本名字 {0} 非法，必须类似 SCRIPT_1234", t.Name);
                    continue;
                }

                // 已经有该公式名，需要移除
                if (mScripts.ContainsKey(sptNo))
                    mScripts.Remove(sptNo);

                mScripts.Add(sptNo, asse.CreateInstance(t.Name) as Script);
            }
        }
    }

    // 根据脚本编号调用脚本
    public static object Call(int scriptNo, params object[] args)
    {
        // 如果有更新lua脚本
        string luaScript;
        if (mLuaScripts.TryGetValue(scriptNo, out luaScript))
        {
            // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
            // 载入脚本
            LuaTable scriptEnv = LuaCsMgr.LuaEnvOb.NewTable();

            // 为每个脚本设置一个独立的环境，可一定程度上防止脚本间全局变量、函数冲突
            LuaTable meta = LuaCsMgr.LuaEnvOb.NewTable();
            meta.Set("__index", LuaCsMgr.LuaEnvOb.Global);
            scriptEnv.SetMetaTable(meta);
            meta.Dispose();

            // 执行一个代码块
            LuaCsMgr.LuaEnvOb.DoString(luaScript, scriptNo.ToString(), scriptEnv);

            // 调用脚本
            ScriptDelegate luaCall = scriptEnv.Get<ScriptDelegate>("Call");
            if (luaCall == null)
            {
                LogMgr.Trace("调用lua脚本{0}异常，请确认问题。", scriptNo);
                return null;
            }

            // 调用call
            object luaCallRet = luaCall.Invoke(args);

            // 释放scriptEnv
            scriptEnv.Dispose();
            luaCall = null;

            // 返回lua的返回值
            return luaCallRet;
        }

        Script script;

        if (! mScripts.TryGetValue(scriptNo, out script))
        {
            LogMgr.Trace("调用脚本{0}不存在。", scriptNo);
            return null;
        }

        object ret = script.Call(args);
        return ret;
    }

    // 判断某个脚本是否存在
    public static bool Contains(int scriptNo)
    {
        if (! mScripts.ContainsKey(scriptNo))
            return false;

        return true;
    }

    #endregion
}
