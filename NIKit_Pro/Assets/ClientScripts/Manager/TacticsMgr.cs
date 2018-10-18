/// <summary>
/// TacticsMgr.cs
/// Created by zhaozy 2014-11-17
/// 策略管理模块
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using LPC;

public static class TacticsMgr
{
    #region 属性

    // 策略集合
    static Dictionary<string, Tactics> mTactiscList = new Dictionary<string, Tactics>();

    #endregion

    #region 内部函数

    /// <summary>
    /// 加载策略子模块
    /// </summary>
    private static void LoadEntryTactics()
    {
        mTactiscList.Clear();

        // 收集所有策略
        Assembly asse = Assembly.GetExecutingAssembly();
        Type[] types = asse.GetTypes();
        foreach (Type t in types)
        {
            // 不是策略子模块不处理
            if (!t.IsSubclassOf(typeof(Tactics)))
                continue;

            // 添加到列表中
            mTactiscList.Add(t.Name, asse.CreateInstance(t.Name) as Tactics);
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 策略管理模块初始化借口
    /// </summary>
    public static void Init()
    {
        // 加载策略子模块
        LoadEntryTactics();
    }

    /// <summary>
    /// 执行策略
    /// </summary>
    public static bool DoTactics(Property who, string tacticsId, LPCMapping para)
    {
        // 对象已经不存在
        if (who == null)
            return false;

        // 调用子模块
        Tactics mod;
        if (!mTactiscList.TryGetValue(tacticsId, out mod))
            return false;

        // 调用子模块
        return mod.Trigger(who, para);
    }

    #endregion
}

/// <summary>
/// 策略基类
/// </summary>
public abstract class Tactics
{
    // 触发策略
    public abstract bool Trigger(params object[] args);
}
