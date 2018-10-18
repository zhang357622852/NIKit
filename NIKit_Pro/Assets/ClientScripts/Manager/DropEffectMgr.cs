/// <summary>
/// DropEffectMgr.cs
/// Created by lic 2016-8-31
/// 物品掉落效果管理器
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public static class DropEffectMgr
{
    #region 成员变量

    // 场景中的可拾取窗口列表
    private static Dictionary<string, List<GameObject>> mDropWndMap = new Dictionary<string, List<GameObject>>();

    // 缓存掉落光效配置列表
    private static Dictionary<string, int> mCacheSettingMap = new Dictionary<string, int>()
    {
        { "Drop_diamond", 2},
        { "Drop_life", 2},
        { "Drop_arena", 2},
        { "Drop_money", 12},
    };

    // 对象缓存
    private static Dictionary<string, List<GameObject>> mCacheDropEffectMap = new Dictionary<string, List<GameObject>>();

    // 掉落物体Z轴随机偏移量(防止玩家主动拾取出现box colider前后顺序问题)
    private static float mZOffset = 0f;

    // 是否已初始化
    private static bool isInit = false;

    #endregion

    #region 内部函数

    /// <summary>
    /// 创建掉落光效
    /// </summary>
    private static GameObject DoCreateDropEffect(string effectName, bool isActive = false)
    {
        GameObject effectOb = CreateDropEffect(Game.GetUniqueName(effectName), effectName);

        // 设置道具不需要Destroy
        GameObject.DontDestroyOnLoad(effectOb);

        // 设置光效的激活状态
        effectOb.SetActive(isActive);

        // 加入缓存
        return effectOb;
    }

    /// <summary>
    /// 加载掉落光效
    /// </summary>
    /// <returns>The create drop effect.</returns>
    /// <param name="name">Name.</param>
    /// <param name="prefabName">Prefab name.</param>
    private static GameObject CreateDropEffect(string name, string prefabName)
    {
        // 载入资源
        string prefabRes = string.Format("Assets/Prefabs/Drop/{0}.prefab", prefabName);
        GameObject DropModel = ResourceMgr.Load(prefabRes) as GameObject;

        // 再克隆一份
        GameObject DropOb = GameObject.Instantiate(DropModel) as GameObject;

        // 设置模型的原始位置
        DropOb.transform.localPosition = Vector3.zero;
        DropOb.transform.localScale = Vector3.one;
        DropOb.name = name;

        return DropOb;
    }

    /// <summary>
    /// 初始化创建缓存掉落光效
    /// </summary>
    private static void DoInItCacheDropEffect()
    {
        // 获取初始化配置
        List<GameObject> allEffectList = new List<GameObject>();
        GameObject effectOb;

        // 组个类型创建光效
        foreach (KeyValuePair<string, int> pair in mCacheSettingMap)
        {
            List<GameObject> effectList = new List<GameObject>();

            // 根据配置初始化掉落对象列表
            // 创建光效
            for (int i = 0; i < pair.Value; i++)
            {
                effectOb = DoCreateDropEffect(pair.Key, true);
                effectList.Add(effectOb);
                allEffectList.Add(effectOb);
            }

            // 添加缓存列表
            mCacheDropEffectMap.Add(pair.Key, effectList);
        }

        // 将光效active为false
        // 为什么这个地方需要等一帧，主要是让光效动画完成后续加载
        foreach (GameObject effect in allEffectList)
            effect.SetActive(false);
    }

    /// <summary>
    /// 创建一个掉落物品
    /// </summary>
    /// <returns>光效对象</returns>
    /// <param name="cookie">Cookie.</param>
    /// <param name="item">物品信息</param>
    /// <param name="num">物品数量</param>
    /// <param name="entityInfo">掉落者信息</param>
    private static GameObject CreateBonusItem(string cookie, string wndName, int num, LPCMapping entityInfo)
    {
        // 从缓存中获取光效对象
        GameObject effectOb = GetUsableEffectItem(wndName);
        if (effectOb == null)
            return null;

        // 获取轨迹组件
        DropEffect comDE = effectOb.GetComponent<DropEffect>();
        if (comDE == null)
            return null;

        // 掉落Z轴偏移
        if(mZOffset < - 1.0f)
            mZOffset = 0f;

        mZOffset -= 0.01f;

        // 绑定信息
        comDE.BindItemInfo(wndName, num, entityInfo, mZOffset);

        // 返回光效对象
        return effectOb;
    }

    /// <summary>
    /// 执行拾取
    /// </summary>
    /// <param name="effectOb">Effect ob.</param>
    private static void Pick(GameObject effectOb)
    {
        DropEffect de = effectOb.GetComponent<DropEffect>();

        if(de == null)
            return;

        // 执行拾取
        de.DoPick();
    }

    /// <summary>
    /// 立即回收场上的所有掉落物品
    /// </summary>
    private static void WhenRoundEnd(int eventId, MixedValue para)
    {
        PickAll();
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 已经初始化过了
        if (isInit)
            return;
 
        isInit = true;

        // 取消光效掉落相关事件回调
        EventMgr.UnregisterEvent("DropEffectMgr");

        // 注册战斗回合结束的回调
        EventMgr.RegisterEvent("DropEffectMgr", EventMgrEventType.EVENT_ROUND_COMBAT_END, WhenRoundEnd);

        // 注册副本通关事件的回调
        EventMgr.RegisterEvent("DropEffectMgr", EventMgrEventType.EVENT_INSTANCE_CLEARANCE, WhenRoundEnd);

        // 初始化掉落光效
        DoInItCacheDropEffect();
    }

#if UNITY_ANDROID || UNITY_IPHONE

    /// <summary>
    /// 驱动更新
    /// 目前用于检测拾取操作
    /// </summary>
    public static void Update()
    {
        // 若场景中没有掉落的物品，不检测
        if (mDropWndMap.Count == 0)
            return;

        int num = 0;
        foreach(List<GameObject> mapList in mDropWndMap.Values)
        {
            num += mapList.Count;
        }

        if(num == 0)
            return;

        if (Input.touchCount <= 0)
        {
            return;
        }

        // 记录当前位置
        Touch currentTouch = Input.GetTouch(0);
        Vector3 mCurrentPos = currentTouch.position;

        switch (currentTouch.phase)
        {
            case TouchPhase.Began:
                {
                    DoRayCast(mCurrentPos);
                }
                break;
        }
    }

#else

    /// <summary>
    /// 驱动更新
    /// 目前用于检测拾取操作
    /// </summary>
    public static void Update()
    {
        if (mDropWndMap.Count == 0)
            return;

        int num = 0;
        foreach(List<GameObject> mapList in mDropWndMap.Values)
        {
            num += mapList.Count;
        }

        if(num == 0)
            return;

        if (Input.GetMouseButtonUp(0))
        {
            // 获取鼠标当前位置
            Vector3 curPos = Input.mousePosition;

            DoRayCast(curPos);
        }
    }

#endif

    private static void DoRayCast(Vector3 position)
    {
        // 发出射线
        Ray ray = SceneMgr.SceneCamera.ScreenPointToRay(position);
        RaycastHit hit;

        // 取得drop物体的layer
        int layer = 1<<(LayerMask.NameToLayer("Drop"));

        // 执行碰撞检测
        if (!Physics.Raycast(ray, out hit, SceneMgr.SceneCamera.farClipPlane, layer))
            return;

        // 通知碰撞实体执行点击场景wnd事件
        GameObject wnd = hit.transform.gameObject;
        DropEffect de = wnd.GetComponent<DropEffect>();

        if (de != null)
        {
            Pick(wnd);
        }
    }

    /// <summary>
    ///  提交一个奖励效果
    /// </summary>
    /// <param name="who">奖励给谁</param>
    /// <param name="cookie">Cookie</param>
    /// <param name="args">奖励信息参数</param>
    public static void SubmitBonusWnd(Property who, string cookie, LPCMapping args)
    {
        // 掉落者信息
        LPCMapping entityInfo = args["entityInfo"].AsMapping;
        if (entityInfo == null)
            return;

        // 物品列表
        System.Diagnostics.Debug.Assert(args.ContainsKey("propertyList"));
        LPCMapping itemMap = args["propertyList"].AsMapping;
        if (itemMap == null ||
            itemMap.Count <= 0)
            return;

        // 遍历所有物品，拆分掉落
        foreach (string key in itemMap.Keys)
        {
            int count = itemMap[key].AsInt;
            if (count <= 0)
                continue;

            string wndName = GET_DROP_ITEM_ICON.Call(key);

            if(string.IsNullOrEmpty(wndName))
                continue;

            if (!mDropWndMap.ContainsKey(wndName))
                mDropWndMap.Add(wndName, new List<GameObject>());

            // 清理一下
            mDropWndMap[wndName].RemoveAll((i) =>
            {
                return i == null;
            });

            // 多个物品，批量创建
            int[] splitList = CALC_DROP_ITEM_SPLITE.Call(key, count, entityInfo);
            foreach (int num in splitList)
            {
                GameObject wnd = CreateBonusItem(cookie, wndName, num, entityInfo);
                if (wnd == null)
                {
                    LogMgr.Trace("创建光效失败。");
                    continue;
                }

                mDropWndMap[wndName].Add(wnd);
            }
        }
    }

    /// <summary>
    /// 执行所有奖励
    /// </summary>
    public static void PickAll()
    {
        Dictionary<string, List<GameObject>>.Enumerator it = mDropWndMap.GetEnumerator();
        while (it.MoveNext())
        {
            foreach (GameObject e in it.Current.Value)
            {
                if (e != null)
                    Pick(e);
            }
        }

        mDropWndMap.Clear();
    }

    /// <summary>
    /// 获取可用掉落对象
    /// </summary>
    public static GameObject GetUsableEffectItem(string effectName)
    {
        List<GameObject> effectList = null;
        if (!mCacheDropEffectMap.TryGetValue(effectName, out effectList))
            effectList = new List<GameObject>();

        // 列表中没有光效可以使用
        if (effectList.Count == 0)
            return DoCreateDropEffect(effectName, true);

        // 从缓存列表中获取光效
        GameObject effectOb = effectList[0];
        effectList.RemoveAt(0);
        effectOb.SetActive(true);

        // 返回光效
        return effectOb;
    }

    /// <summary>
    /// 释放已用掉落对象
    /// </summary>
    public static void ReleaseUsedEffectItem(string effectName, GameObject effectOb)
    {
        // 设置光效active为false
        effectOb.SetActive(false);

        List<GameObject> effectList = null;
        if (!mCacheDropEffectMap.TryGetValue(effectName, out effectList))
        {
            effectList = new List<GameObject>();
            effectList.Add(effectOb);
            mCacheDropEffectMap.Add(effectName, effectList);
        }
        else
        {
            if (!effectList.Contains(effectOb))
                effectList.Add(effectOb);
        }

        List<GameObject> effObList = null;
        if(mDropWndMap.TryGetValue(effectName, out effObList))
        {
            if(effObList.Contains(effectOb))
                effObList.Remove(effectOb);
        }
    }

    #endregion
}
