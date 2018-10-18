/// <summary>
/// ReferencesCounter.cs
/// Created by zhaozy 2015-05-15
/// 引用计数脚本
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

/// <summary>
/// 资源计数器
/// </summary>
public class ReferencesCounter : MonoBehaviour
{
#region 变量

    public string resPath = string.Empty;
    private Resource res = null;

#endregion

    /// <summary>
    /// Awake
    /// </summary>
    public void Awake()
    {
        // 没有资源路径
        if (string.IsNullOrEmpty(resPath))
            return;

        // 缓存列表中没有获取到资源信息
        ResourceMgr.ResourceMap.TryGetValue(resPath, out res);
        if (res == null)
            return;

        // 增加引用关系
        res.References.Add(gameObject);

        // 更新访问时间
        res.UpdateAccessTime();
    }

    /// <summary>
    /// OnDestroy
    /// </summary>
    void OnDestroy()
    {
        // 没有资源路径
        if (string.IsNullOrEmpty(resPath) ||
            res == null)
            return;

        // 删除引用关系
        res.References.Remove(gameObject);

        // 更新访问时间
        res.UpdateAccessTime();
    }
}
