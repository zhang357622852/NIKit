/// <summary>
/// Main.cs
/// Created by wangxw 2014-10-22
/// 应用程序的入口
/// </summary>

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Main : MonoBehaviour
{
    IEnumerator Start()
    {
        try
        {
            // 初始化根对象
            // 场景中的StartGame对象和本Main对象都是Unity3d启动流程的过客
            // 一切从下面这个GameRoot开始；
            GameRoot.Init();
        } catch (Exception e)
        {
            LogMgr.Exception(e);
        }

        yield break;
    }

    // 只有在手机上才相应Update
#if UNITY_ANDROID || UNITY_IPHONE

    void Update()
    {
        // 响应返回键退出UCSDK(非正解)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QCCommonSDK.QCCommonSDK.RequestQuitGame();
        }
    }

#endif
}