/// <summary>
/// CameraShakeMgr.cs
/// Created by xuhd Nov/11/2014
/// 相机抖动管理器
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class CameraShakeMgr
{
    #region 外部接口

    /// <summary>
    /// 相机初始化接口
    /// </summary>
    public static void Init()
    {
    }

    /// <summary>
    /// Dos the shake.
    /// </summary>
    /// <returns><c>true</c>, if shake was done, <c>false</c> otherwise.</returns>
    /// <param name="camera">Camera.</param>
    /// <param name="track">Track.</param>
    /// <param name="duration">Duration.</param>
    /// <param name="cookie">Cookie.</param>
    /// <param name="cb">Cb.</param>
    public static bool DoShake(Camera camera, TrackInfo track, float duration, string cookie, CallBack cb)
    {
        // 相机对象不存在
        if (camera == null)
            return false;

        // 获取ShakeCamera组件
        ShakeCamera cam = camera.gameObject.AddMissingComponent<ShakeCamera>();
        if (cam == null)
        {
            LogMgr.Trace("找不到相机ShakeCamera，抖动失败");
            return false;
        }

        // 执行相机抖动
        cam.DoShake(track, duration, cookie, cb);

        // 激活ShakeCamera主键
        cam.enabled = true;

        // 返回抖动成功
        return true;
    }

    #endregion
}
