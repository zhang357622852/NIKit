/// <summary>
/// UIRootInit.cs
/// Created by xuhd Nov/19/2014
/// UIRoot创建后的回调
/// 给WindowMgr传句柄
/// 打开战斗界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using LPC;

public class UIRootInit : MonoBehaviour
{
    #region 私有字段

    #if UNITY_EDITOR
    private GameObject mDebugWnd = null;
    #endif

    #endregion

    /// <summary>
    /// Awake this instance.
    /// </summary>
    void Awake()
    {
        // 初始化UIRoot
        WindowMgr.UIRoot = transform;
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 清除BloodTipMgr
        BloodTipMgr.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        // 响应返回键退出UCSDK(非正解)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QCCommonSDK.QCCommonSDK.RequestQuitGame();
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F10))
        {
            if (mDebugWnd == null || !mDebugWnd.activeSelf)
            {
                ShowDebugWnd();
            }
            else
            {
                mDebugWnd.GetComponent<DebugWnd>().Hide();
            }
        }
#endif
    }

    #if UNITY_EDITOR
    /// <summary>
    /// 显示调试按钮
    /// </summary>
    private void ShowDebugWnd()
    {
        if (mDebugWnd != null)
        {
            mDebugWnd.SetActive(true);
            return;
        }

        GameObject wndOb;

        // 加载窗口对象实例
        wndOb = ResourceMgr.Load("Assets/Prefabs/Window/Debug/DebugWnd.prefab") as GameObject;
            
        if (wndOb == null)
        {
            LogMgr.Trace("加载DebugWnd失败");
            return;
        }

        GameObject wnd = GameObject.Instantiate(wndOb, wndOb.transform.localPosition, Quaternion.identity) as GameObject;
        wnd.name = wndOb.name;

        // 挂到UIRoot下
        Transform ts = wnd.transform;
        ts.parent = transform;
        ts.localPosition = wndOb.transform.localPosition;
        ts.localScale = Vector3.one;
        mDebugWnd = wnd;
        mDebugWnd.SetActive(true);
        mDebugWnd.transform.localPosition = new Vector3(-550, 230, 0);
    }
    #endif

    /// <summary>
    /// 暂停游戏
    /// </summary>
    /// <param name="pauseStatus">If set to <c>true</c> pause status.</param>
    void OnApplicationPause(bool pauseStatus)
    {
        // 没有初始化完成，不响应
        if (!GameRoot.IsInit)
            return;

        // 回到前台的事件
        if (pauseStatus)
        {
            // 进入后台暂停战斗系统
            TimeMgr.DoPauseCombatLogic("ApplicationPause");
        }
        else
        {
            // 进入前台战斗系统
            TimeMgr.DoContinueCombatLogic("ApplicationPause");
        }

#if ! UNITY_EDITOR

        // 设置离线推送
        PushMgr.OnAppStatusChange(pauseStatus);
#endif
    }
}
