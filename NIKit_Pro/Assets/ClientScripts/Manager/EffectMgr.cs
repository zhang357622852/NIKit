/// <summary>
/// EffectMgr.cs
/// Created by wangxw 2014-01-08
/// 光效管理器
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public static class EffectMgr
{
    #region 成员变量

    // 普通光效对象列表
    private static HashSet<GameObject> mEffectMap = new HashSet<GameObject>();

    // 光效详细信息
    private static CsvFile mEffectCsv;

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 载入光效详细信息文件
        mEffectCsv = CsvFileMgr.Load("effect");
    }

    /// <summary>
    /// 创建组合光效对象
    /// </summary>
    /// <returns>光效对象；创建失败返回null</returns>
    /// <param name="name">对象名</param>
    /// <param name="prefabName">资源名</param>
    public static GameObject CreateGroupEffect(string name, string prefabName)
    {
        // 加载资源
        string path = string.Format("Assets/{0}.prefab", prefabName);
        Object prefebOb = ResourceMgr.Load(path) as Object;

        if (prefebOb == null)
        {
            LogMgr.Trace("加载 {0} 光效资源失败，请检查资源是否存在", prefabName);
            return null;
        }

        // 初始化光效
        GameObject effectOb = GameObject.Instantiate(prefebOb) as GameObject;
        if (effectOb == null)
        {
            LogMgr.Trace("初始化 {0} 光效失败，请检查资源数据是否正确", prefabName);
            return null;
        }

        // 设置光效名称
        effectOb.name = name;

        // 根据类型分类光效效果
        mEffectMap.Add(effectOb);

        // 获取该组合光效下所有Animator
        Animator[] aorList = effectOb.GetComponentsInChildren<Animator>();
        foreach (Animator aor in aorList)
        {
            aor.Play(CombatConfig.ANIMATION_BASE_LAYER + CombatConfig.DEFAULT_PLAY,
                CombatConfig.ANIMATION_BASE_LAYER_INEDX,
                0f);
        }

        // 获取该组合光效下所有ParticleSystem
        ParticleSystem[] particleList = effectOb.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem particle in particleList)
            particle.Play();

        // 返回null
        return effectOb;
    }

    /// <summary>
    /// 获取光效粒子残留时长
    /// </summary>
    public static float GetEffectLifeTime(string effect)
    {
        // 没有配置信息
        if (mEffectCsv == null)
            return 0f;

        // 模型数据不存在
        CsvRow data = mEffectCsv.FindByKey(effect);
        if (data == null)
            return 0f;

        // 获取particle_life_time信息
        LPCValue lifeTime = data.Query<LPCValue>("life_time");
        if (lifeTime == null)
            return 0f;

        // 返回指定动画时间长度
        return lifeTime.AsFloat;
    }

    /// <summary>
    /// 获取光效时长
    /// </summary>
    public static float GetEffectLength(string effect, string animationName = CombatConfig.DEFAULT_PLAY)
    {
        // 没有配置信息
        if (mEffectCsv == null)
            return 0f;

        // 模型数据不存在
        CsvRow data = mEffectCsv.FindByKey(effect);
        if (data == null)
            return 0f;

        // 获取CombatConfig.DEFAULT_PLAY信息
        LPCMapping animation = data.Query<LPCMapping>("animation");
        if (animation == null)
            return 0f;

        // 获取指定动画详细信息
        LPCValue length = animation.GetValue<LPCValue>(animationName);
        if (length == null)
            return 0f;

        // 返回指定动画时间长度
        return length.AsFloat;
    }

    /// <summary>
    /// 创建光效对象
    /// </summary>
    /// <returns>光效对象；创建失败返回null</returns>
    /// <param name="name">对象名</param>
    /// <param name="prefabName">资源名</param>
    public static GameObject CreateEffect(string name, string prefabName)
    {
        // 光效对象
        GameObject effectOb;

        // 如果是战斗验证客户端不需要实例化mGameObject
        if (AuthClientMgr.IsAuthClient)
        {
            // 战斗验证客户端只能创建一个模拟对象
            effectOb = new GameObject(name);

            // 根据类型分类光效效果
            mEffectMap.Add(effectOb);

            // 返回光效对象
            return effectOb;
        }

        // 加载资源
        string path = string.Format("Assets/{0}.prefab", prefabName);
        Object prefebOb = ResourceMgr.Load(path) as Object;

        if (prefebOb == null)
        {
            LogMgr.Trace("加载 {0} 光效资源失败，请检查资源是否存在", prefabName);
            return null;
        }

        // 初始化光效
        effectOb = GameObject.Instantiate(prefebOb) as GameObject;
        if (effectOb == null)
        {
            LogMgr.Trace("初始化 {0} 光效失败，请检查资源数据是否正确", prefabName);
            return null;
        }

        // 设置光效名称
        effectOb.name = name;

        // 如果光效是序列光效
        Animator aor = effectOb.GetComponent<Animator>();
        if (aor != null)
        {
            aor.Play(CombatConfig.ANIMATION_BASE_LAYER + CombatConfig.DEFAULT_PLAY,
                CombatConfig.ANIMATION_BASE_LAYER_INEDX,
                0f);

            // 根据类型分类光效效果
            mEffectMap.Add(effectOb);

            // 返回光效对象
            return effectOb;
        }

        // 如果光效是ParticleSystem
        ParticleSystem particle = effectOb.GetComponent<ParticleSystem>();
        if (particle != null)
        {
            // 播放光效
            particle.Play();

            // 根据类型分类光效效果
            mEffectMap.Add(effectOb);

            // 返回光效对象
            return effectOb;
        }

        // 不合格的光效
        // LogMgr.Trace("光效 {0} 没有Animator或者ParticleSystem组件", prefabName);
        // GameObject.Destroy(effectOb);
        mEffectMap.Add(effectOb);

        // 返回effectOb
        return effectOb;
    }

    /// <summary>
    /// 销毁指定光效
    /// </summary>
    public static void DestroyEffect(GameObject ob)
    {
        // 从普通光效列表中移除对象
        if (!mEffectMap.Contains(ob))
            return;

        // 销毁对象
        if (ob != null)
            GameObject.Destroy(ob);

        // 从列表中移除对象
        mEffectMap.Remove(ob);
    }

    #endregion
}
