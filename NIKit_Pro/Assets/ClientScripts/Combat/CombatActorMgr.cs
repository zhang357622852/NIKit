/// <summary>
/// CombatActorMgr.cs
/// Created by wangxw 2014-11-05
/// 战斗角色管理器
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public static class CombatActorMgr
{
    #region 成员变量

    // 角色列表(有行为的对象)
    private static Dictionary<string, CombatActor> mCombatActorMap = new Dictionary<string, CombatActor>();
    private static List<CombatActor> mCombatActorList = new List<CombatActor>();

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 清空列表
        mCombatActorMap.Clear();
        mCombatActorList.Clear();
        // do nothing
    }

    /// <summary>
    /// 驱动战斗系统更新
    /// </summary>
    /// <param name="deltaTime">相对上一帧流失的时间</param>
    public static void Update(float deltaTime)
    {
        // 驱动所有角色
        for(int i = 0; i < mCombatActorList.Count; i++)
        {
            // 获取actor对象
            CombatActor actor = mCombatActorList[i];

            // actor对象不存在
            // actor对象没有激活
            if (actor == null || ! actor.isActive())
                continue;

            // actor对象更新
            actor.Update(deltaTime);
        }
    }

    /// <summary>
    /// 获取角色对象
    /// </summary>
    /// <returns>The combat actor.</returns>
    /// <param name="name">Name.</param>
    public static CombatActor GetCombatActor(string name)
    {
        CombatActor actor;

        // 先查找有行为角色列表
        if (! mCombatActorMap.TryGetValue(name, out actor))
            return null;

        // 返回null
        return actor;
    }

    /// <summary>
    /// 创建角色对象
    /// </summary>
    /// <returns>The combat actor.</returns>
    /// <param name="name">角色名</param>
    /// <param name="initArgs">初始化参数，预设名、缩放、颜色等</param>
    public static CombatActor CreateCombatActor(string name, LPCMapping initArgs)
    {
        if (mCombatActorMap.ContainsKey(name))
        {
            LogMgr.Trace("name = {0} 的战斗角色，不能重复创建", name);
            return null;
        }

        // 获取预设资源
        LPCValue modelId = initArgs["model"];
        if (modelId == null)
        {
            LogMgr.Trace("name = {0} 的战斗角色，无法获取prefeb信息", name);
            return null;
        }

        // 创建角色对象
        string prefebResource = string.Format("Assets/Prefabs/Model/{0}.prefab", modelId.AsString);
        CombatActor newActor = new CombatActor(name, prefebResource);

        // 先缓存信息
        mCombatActorMap.Add(name, newActor);
        mCombatActorList.Add(newActor);

        // 载入资源
        newActor.ModelId = modelId.AsString;
        newActor.Load();

        // 获取皮肤信息，如果没有皮肤信息则使用的默认配置
        string skin = CALC_ACTOR_SKIN.Call(initArgs.GetValue<int>("rank"));
        if (!string.IsNullOrEmpty(skin))
            newActor.SetSkin(skin);

        // 角色缩放
        LPCValue scale = initArgs["scale"];
        if (scale != null)
        {
            switch (scale.type)
            {
                case LPCValue.ValueType.FLOAT:
                    newActor.SetScale(scale.AsFloat, scale.AsFloat, scale.AsFloat);
                    break;
                case LPCValue.ValueType.INT:
                    newActor.SetScale(scale.AsInt, scale.AsInt, scale.AsInt);
                    break;
                case LPCValue.ValueType.ARRAY:
                    if (scale.AsArray.Count >= 3)
                        newActor.SetScale(scale.AsArray[0].AsFloat,
                            scale.AsArray[1].AsFloat,
                            scale.AsArray[2].AsFloat);
                    break;
            }
        }

        // 角色变色
        LPCValue color = initArgs["color"];
        if (color != null && color.IsString && !string.IsNullOrEmpty(color.AsString))
            newActor.PushColor(ColorConfig.ParseToColor(color.AsString), ColorChangerType.CCT_BASE, true);

        // 返回对象
        return newActor;
    }

    /// <summary>
    /// 销毁角色
    /// </summary>
    /// <param name="name">角色对象名</param>
    public static void DestroyCombatActor(string name)
    {
        // 卸载
        CombatActor actor = GetCombatActor(name);

        // actor对象不存在
        if (actor != null)
        {
            actor.CancelAllActionSet();
            actor.Unload();
        }

        // 从列表移出即可，走CSharp的托管资源流程
        mCombatActorMap.Remove(name);
        mCombatActorList.Remove(actor);

        // TODO:fire event?
    }

    /// <summary>
    /// 销毁所有角色
    /// </summary>
    public static void DestroyAllCombatActors()
    {
        // 清除有行为的角色列表
        foreach (CombatActor actor in mCombatActorList)
        {
            // actor对象不存在
            if (actor == null)
                continue;

            // 回收资源
            actor.CancelAllActionSet();
            actor.Unload();
        }

        // 清除数据
        mCombatActorMap.Clear();
        mCombatActorList.Clear();
    }

    #endregion
}
