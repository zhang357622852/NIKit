/// <summary>
/// CombatRootMgr.cs
/// Created by wangxw 2014-11-05
/// 战斗系统管理器
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public static class CombatRootMgr
{
    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 角色管理器
        CombatActorMgr.Init();

        // 行为管理器
        CombatActionMgr.Init();
    }

    /// <summary>
    /// 驱动战斗系统更新
    /// </summary>
    public static void Update()
    {
        float deltaTime = Coroutine.DeltaTime;

        // 驱动角色
        CombatActorMgr.Update(deltaTime);

        // 驱动行为
        CombatActionMgr.Update(deltaTime);
    }

    /// <summary>
    /// 清空战斗系统
    /// </summary>
    public static void CleanAll()
    {
        // 析构所有战斗对象
        CombatActorMgr.DestroyAllCombatActors();

        // 析构所有战斗action
        CombatActionMgr.DestroyAllActionSet();
    }

    #endregion
}

