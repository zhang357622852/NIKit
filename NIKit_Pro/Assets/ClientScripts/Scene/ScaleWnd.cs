/// <summary>
/// ScaleWnd.cs
/// Created by zhaozy 2017/10/26
/// 指引窗口
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScaleWnd : MonoBehaviour
{
    /// <summary>
    /// 标准屏幕分辨率(16:10)
    /// </summary>T
    public float mStdAspect = 1.6f;

    /// <summary>
    /// Awake this instance.
    /// </summary>
    void Awake()
    {
        // 获取当前的屏幕分辨率
        float aspect =  mStdAspect / SceneMgr.UiCamera.aspect;

        // 设置窗口缩放
        gameObject.transform.localScale = new Vector3(aspect, aspect, 1f);
    }
}
