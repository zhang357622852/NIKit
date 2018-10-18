/// <summary>
/// StdCameraInit.cs
/// Created by zhaozy 2017/07/07
/// StdCameraInit创建后的回调

/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LPC;

public class StdCameraInit : MonoBehaviour
{
    public Camera stdCamera;

    /// <summary>
    /// Awake this instance.
    /// </summary>
    void Awake()
    {
        // 初始化SceneMgr标准相机
        SceneMgr.StdCamera = stdCamera;
    }
}
