/// <summary>
/// TrackMgr.cs
/// Created by zhaozy 2015/10/16
/// Track曲线管理器
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class TrackMgr
{
    #region 公有字段

    // 轨迹数据
    private static Dictionary<string, TrackInfo> mTrackInfoMap = new Dictionary<string, TrackInfo>();

    #endregion

    #region 外部接口

    /// <summary>
    /// 获取指定轨迹
    /// </summary>
    /// <returns>The track. 没有指定名称的轨迹则返回null</returns>
    /// <param name="name">Name.</param>
    public static TrackInfo GetTrack(string name)
    {
        TrackInfo track;

        // 如果本地有缓存则直接返回
        if (mTrackInfoMap.TryGetValue(name, out track))
            return track;

        // 资源预加载后再调整逻辑
        track = (TrackInfo)ResourceMgr.Load(string.Format("Assets/Prefabs/Curve/{0}.prefab", name));

        // 载入资源失败
        if (track == null)
            return null;

        // 添加缓存信息
        mTrackInfoMap.Add(name, track);

        // 返回资源
        return track;
    }

    #endregion
}
