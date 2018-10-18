/// <summary>
/// FormulaMgr.cs
/// Created by zhaozy 2014-11-22
/// 公式管理
/// </summary>

using System;
using System.Collections.Generic;
using System.Reflection;
using XLua;

// 脚本管理
public static class FormulaMgr
{
    #region 变量

    // 公式列表
    static Dictionary<string, Formula> mFormulas = new Dictionary<string, Formula>();

    /// <summary>
    /// The m lua formulas.
    /// </summary>
    static Dictionary<string, string> mLuaFormulas = new Dictionary<string, string>();

    [CSharpCallLua]
    public delegate object FormulaDelegate(params object[] args);

    #endregion

    #region 公共接口

    /// <summary>
    /// Adds the lua formula.
    /// </summary>
    /// <param name="formulaName">Formula name.</param>
    /// <param name="formulaText">Formula text.</param>
    public static void AddLuaFormula(string formulaName, string formulaText)
    {
        // 已经有该公式名，需要移除
        if (mLuaFormulas.ContainsKey(formulaName))
            mLuaFormulas.Remove(formulaName);

        mLuaFormulas.Add(formulaName, formulaText);
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 收集Formula
        Assembly asse = Assembly.GetExecutingAssembly();
        Type[] types = asse.GetTypes();
        foreach (Type t in types)
        {
            if (! t.IsSubclassOf(typeof(Formula)))
                continue;

            // 获取公式名
            string formulaName = t.Name;

            // 已经有该公式名，需要移除
            if (mFormulas.ContainsKey(formulaName))
                mFormulas.Remove(formulaName);

            // 添加列表
            mFormulas.Add(formulaName, asse.CreateInstance(formulaName) as Formula);
        }
    }

    /// <summary>
    /// 增加公式
    /// </summary>
    public static void AddFormula(string formulaName, Formula formula)
    {
        mFormulas.Add(formulaName, formula);
    }

    /// <summary>
    /// 根据公式名查找公式
    /// </summary>
    public static Formula FindFormulaByName(string formulaName)
    {
        Formula formula;

        // 在列表中没有找到公式, 直接返回
        if (! mFormulas.TryGetValue(formulaName, out formula))
            return null;

        // 返回公式
        return formula;
    }

    /// <summary>
    /// 调用公式
    /// </summary>
    public static object InvokeFormulaByName(string formulaName, params object[] args)
    {
        // 如果有更新lua脚本
        string luaFormula;
        if (mLuaFormulas.TryGetValue(formulaName, out luaFormula))
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
            LuaCsMgr.LuaEnvOb.DoString(luaFormula, formulaName, scriptEnv);

            // 调用脚本
            FormulaDelegate luaCall = scriptEnv.Get<FormulaDelegate>("Call");
            if (luaCall == null)
            {
                LogMgr.Trace("调用lua公式{0}异常，请确认问题。", formulaName);
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

        Formula formula;

        // 在列表中没有找到公式, 直接返回
        if (! mFormulas.TryGetValue(formulaName, out formula))
            return null;

        // 返回公式执行结果
        return formula.Invoke(args);
    }

    #endregion
}
