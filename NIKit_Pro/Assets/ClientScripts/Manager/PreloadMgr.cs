/// <summary>
/// PreloadMgr.cs
/// Create by zhaozy 2017-04-11
/// 预加载管理模块
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using LPC;

/// 预加载管理模块
public static class PreloadMgr
{
    #region 变量

    // 资源预加载列表
    private static Dictionary<string, PreloadQueue> mPreloadQueue = new Dictionary<string, PreloadQueue>();

    // 资源预加载信息
    private static Dictionary<string, List<KeyValuePair<string, bool>>> mPreloadMap = new Dictionary<string, List<KeyValuePair<string, bool>>>();

    // (技能和状态)资源表
    private static Dictionary<int, List<KeyValuePair<string, bool>>> skillResource = new Dictionary<int, List<KeyValuePair<string, bool>>>();
    private static List<KeyValuePair<string, bool>> statusResource = new List<KeyValuePair<string, bool>>();

    #endregion

    #region 内部接口

    /// <summary>
    /// 异步载入资源
    /// </summary>
    private static IEnumerator DoPreloadAsync(string type, bool instantiate)
    {
        // 没有该类型资源预加载
        PreloadQueue queue = null;
        if (! mPreloadQueue.TryGetValue(type, out queue))
            yield break;

        // 资源是否是sprite资源
        bool isSprite = false;

        // 载入资源
        do
        {
            // queue队列不存在
            if (queue == null)
                yield break;

            // 资源已经加载完，没有剩余资源需要加载
            if (queue.RemainCount == 0)
                yield break;

            // 获取当前需要加载资源
            KeyValuePair<string, bool> res = queue.Next();

            // 如果资源无效, 不处理
            if (string.IsNullOrEmpty(res.Key))
                continue;

            // 判断资源是否是sprite资源
            // 目前我们用到的Sprite资源后缀名都是png格式资源
            string extension = Path.GetExtension(res.Key);
            isSprite = string.Equals(extension, ".png");

            // 载入资源
            // 如果资源是后缀为png，则标识是sprite
            yield return Coroutine.DispatchService(ResourceMgr.LoadAsync(res.Key, res.Value, isSprite));

            // 预加载的时候尝试创建创建对象
            if (instantiate && string.Equals(extension, ".prefab"))
            {
                // 载入资源对象
                GameObject preObj = ResourceMgr.Load(res.Key) as GameObject;
                if (preObj == null)
                    continue;

                // 实例化对象
                GameObject preloadOb = GameObject.Instantiate(preObj,
                    preObj.transform.localPosition, preObj.transform.localRotation) as GameObject;
                preloadOb.name = Game.NewCookie("Preload");
                yield return null;

                UnityEngine.GameObject.Destroy(preloadOb);
            }

        } while(true);
    }

    /// <summary>
    /// 加载preload配置文件
    /// </summary>UC94ISKD
    private static void LoadPreloadFile(string fileName)
    {
        mPreloadMap.Clear();

        // 载入状态配置表信息
        CsvFile preloadFile = CsvFileMgr.Load(fileName);

        // 构造状态别名映射表
        foreach (CsvRow row in preloadFile.rows)
        {
            // 获取资源所属类型
            string type = row.Query<string>("type");

            // 如果没有该类型
            if (!mPreloadMap.ContainsKey(type))
                mPreloadMap.Add(type, new List<KeyValuePair<string, bool>>());

            // 添加资源信息
            mPreloadMap[type].Add(new KeyValuePair<string, bool>(
                row.Query<string>("resource"),
                row.Query<int>("is_static") == 1 ? true : false));
        }
    }

    /// <summary>
    /// 资源配置表
    /// </summary>
    private static void LoadResourceCsv(string fileName)
    {
        statusResource.Clear();
        skillResource.Clear();

        // 载入资源表
        CsvFile resourceCsv = CsvFileMgr.Load(fileName);
        KeyValuePair<string, bool> pair = new KeyValuePair<string, bool>();
        string id;
        int skillId;

        // 便利各个资源
        foreach (CsvRow data in resourceCsv.rows)
        {
            // 获取资源分组id
            id = data.Query<string>("id");

            // 如果是技能
            if (! int.TryParse(id, out skillId))
            {
                // 添加各个资源
                foreach(LPCValue res in data.Query<LPCArray>("resources").Values)
                {
                    // 构建数据
                    pair = new KeyValuePair<string, bool>(res.AsString, true);

                    // 列表中已经有该资源不处理
                    if (statusResource.Contains(pair))
                        continue;

                    // 添加资源
                    statusResource.Add(pair);
                }

                continue;
            }

            // 获取该技能的原始技能
            int originalSkillId = SkillMgr.GetOriginalSkillId(skillId);

            // 初始化数据
            if (! skillResource.ContainsKey(originalSkillId))
                skillResource.Add(originalSkillId, new List<KeyValuePair<string, bool>>());

            // 添加各个资源
            foreach(LPCValue res in data.Query<LPCArray>("resources").Values)
            {
                // 构建数据
                pair = new KeyValuePair<string, bool>(res.AsString, true);

                // 列表中已经有该资源不处理
                if (skillResource[originalSkillId].Contains(pair))
                    continue;

                // 添加资源
                skillResource[originalSkillId].Add(pair);
            }
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 预加载初始化
    /// </summary>
    public static void Init()
    {
        // 载入配置文件
        LoadPreloadFile("preload");

        // 载入资源表
        LoadResourceCsv("resource");
    }

    /// <summary>
    /// 获取资源列表
    /// </summary>
    public static List<KeyValuePair<string, bool>> GetResourceList(List<int> skillList, bool isContainStatus)
    {
        // 资源列表
        List<KeyValuePair<string, bool>> resList = new List<KeyValuePair<string, bool>>();

        // 如果需要包含状态
        if (isContainStatus)
            resList.AddRange(statusResource);

        // 遍历技能列表中的各个技能
        foreach (int skillId in skillList)
        {
            // 没有该技能资源
            if (!skillResource.ContainsKey(skillId))
                continue;

            // 遍历该技能下全部资源
            foreach (KeyValuePair<string, bool> res in skillResource[skillId])
            {
                // 已经在资源中
                if (resList.Contains(res))
                    continue;

                // 添加资源
                resList.Add(res);
            }
        }

        // 返回收集的资源列表
        return resList;
    }

    /// <summary>
    /// 获取技能资源
    /// </summary>
    public static List<KeyValuePair<string, bool>> GetSkillResourceList(List<int> skillList)
    {
        // 资源列表
        List<KeyValuePair<string, bool>> resList = new List<KeyValuePair<string, bool>>();

        // 遍历技能列表中的各个技能
        foreach (int skillId in skillList)
        {
            // 没有该技能资源
            if (!skillResource.ContainsKey(skillId))
                continue;

            // 遍历该技能下全部资源
            foreach (KeyValuePair<string, bool> res in skillResource[skillId])
            {
                // 已经在资源中
                if (resList.Contains(res))
                    continue;

                // 添加资源
                resList.Add(res);
            }
        }

        // 返回收集的资源列表
        return resList;
    }

    /// <summary>
    /// 获取all状态资源
    /// </summary>
    public static List<KeyValuePair<string, bool>> GetStatusResourceList()
    {
        // 返回全部状态资源
        return statusResource;
    }

    /// <summary>
    /// 卸载预加载资源
    /// </summary>
    public static void Unload(string type, List<KeyValuePair<string, bool>> resourceList)
    {
        // 没有指定type不允许预加载
        if (string.IsNullOrEmpty(type))
        {
            // 给出提示信息
            LogMgr.Trace("指定预加载类型，预加载失败。");
            return;
        }

        // 将preloadRes添加到资源列表中
        if (mPreloadMap.ContainsKey(type))
        {
            if (resourceList == null)
                resourceList = mPreloadMap[type];
            else
                resourceList.AddRange(mPreloadMap[type]);
        }

        // 卸载资源
        foreach (KeyValuePair<string, bool> res in resourceList)
            ResourceMgr.UnLoad(res.Key);
    }

    /// <summary>
    /// 执行资源预加载
    /// </summary>
    public static void DoPreload(string type, List<KeyValuePair<string, bool>> resourceList = null, bool reload = false, bool instantiate = false)
    {
        // 没有指定type不允许预加载
        if (string.IsNullOrEmpty(type))
        {
            // 给出提示信息
            LogMgr.Trace("指定预加载类型，预加载失败。");
            return;
        }

        // 没有该类型资源预加载
        PreloadQueue queue = null;
        if (mPreloadQueue.TryGetValue(type, out queue))
        {
            // 如果不需要重新加载
            if (! reload)
                return;

            // 停止正常进行中的线程
            Coroutine.StopCoroutine(type);

            // 清除数据
            mPreloadQueue.Remove(type);
            queue.Clear();
            queue = null;
        }

        // 将preloadRes添加到资源列表中
        if (mPreloadMap.ContainsKey(type))
        {
            if (resourceList == null)
                resourceList = mPreloadMap[type];
            else
                resourceList.AddRange(mPreloadMap[type]);
        }

        // 重新添加资源
        queue = new PreloadQueue(resourceList);
        mPreloadQueue.Add(type, queue);

        // 开启线程开始异步载入资源
        Coroutine.DispatchService(DoPreloadAsync(type, instantiate), type);
    }

    /// <summary>
    /// 获取资源预加载进度
    /// </summary>
    public static float GetProgress(string type)
    {
        PreloadQueue queue = null;

        // 没有该类型资源预加载
        if (! mPreloadQueue.TryGetValue(type, out queue))
            return 0f;

        // 返回资源加载进度
        return queue.Progress;
    }

    /// <summary>
    /// 获取资源预加载进度
    /// </summary>
    public static bool IsLoadEnd(string type)
    {
        PreloadQueue queue = null;

        // 没有该类型资源预加载
        if (! mPreloadQueue.TryGetValue(type, out queue))
            return true;

        // 返回资源加载进度
        return queue.RemainCount == 0 ? true : false;
    }

    #endregion
}

/// <summary>
/// Preload queue.
/// </summary>
public class PreloadQueue
{
    #region 变量

    /// <summary>
    /// 资源列表
    /// </summary>
    private List<KeyValuePair<string, bool>> mRes = null;

    /// <summary>
    /// The INVALI.
    /// </summary>
    public static int INVALID = -1;

    #endregion

    #region 属性

    /// <summary>
    /// 获取资源数量
    /// </summary>
    public int Count
    {
        get
        {
            return mRes != null ? mRes.Count : 0;
        }
    }

    /// <summary>
    /// 获取等待加载资源剩余数量
    /// </summary>
    public int RemainCount
    {
        get
        {
            return CurrentIndex != INVALID ? Count - CurrentIndex : 0;
        }
    }

    /// <summary>
    /// 获取资源预加载进度
    /// </summary>
    public float Progress
    {
        get
        {
            // 没有资源需要加载
            if (mRes == null || mRes.Count == 0)
                return 1f;

            // 如果CurrentIndex当前无效
            if (CurrentIndex == INVALID)
                return 0f;

            // 返回当前资源加载进度
            return 1f * (CurrentIndex + 1) / mRes.Count;
        }
    }

    /// <summary>
    /// 获取当前资源加载进度
    /// </summary>
    public int CurrentIndex { get; private set; }

    /// <summary>
    /// 获取当前正在加载资源
    /// </summary>
    public KeyValuePair<string, bool> Current{
        get
        {
            // 返回正在加载资源
            if(mRes != null && CurrentIndex >= 0 && CurrentIndex < mRes.Count)
                return mRes[CurrentIndex];

            // 当前没有资源正在加载
            return new KeyValuePair<string, bool>();
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="res">Res.</param>
    public PreloadQueue(List<KeyValuePair<string, bool>> res)
    {
        // 记录资源列表
        mRes = res;

        // 初始化CurrentIndex
        if (mRes != null && mRes.Count > 0)
            CurrentIndex = 0;
        else
            CurrentIndex = INVALID;
    }

    /// <summary>
    /// 获取下一个资源
    /// </summary>
    public KeyValuePair<string, bool> Next()
    {
        // 没有资源
        if (mRes == null)
            return new KeyValuePair<string, bool>();

        // 当前CurrentIndex无效
        if(CurrentIndex == INVALID)
            return new KeyValuePair<string, bool>();

        int next = CurrentIndex + 1;
        if(next > mRes.Count)
        {
            CurrentIndex = INVALID;
            return new KeyValuePair<string, bool>();
        }

        // 返回资源名
        KeyValuePair<string, bool> asset = mRes[CurrentIndex];
        CurrentIndex = next;
        return asset;
    }

    /// <summary>
    /// 清除资源队列
    /// </summary>
    public void Clear()
    {
        mRes = null;
        CurrentIndex = INVALID;
    }

    #endregion
}
