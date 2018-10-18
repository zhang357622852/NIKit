/// <summary>
/// LoadingMgr.cs
/// Changed by lic 2018/01/15
/// loading管理器
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class LoadingMgr
{  
    // 当前的loading窗口
    public static GameObject CurrLoadingWnd{ get; private set;}

    // 当前的loading类型
    public static int CurrLoadingType{ get; private set;}

    // 进度完成表
    private static Dictionary<int, Dictionary<int, bool>> mLoadEndMap = new Dictionary<int, Dictionary<int, bool>>();

    // 进度条阶段变化
    public delegate void LoadingStateChangeHook(int proceduce, int state);

    public static event LoadingStateChangeHook eventStateChange;

    // 进度条阶段变化
    public delegate void LoadingProgressChangeHook(float progress);

    public static event LoadingProgressChangeHook eventProgressChange;


    /// <summary>
    /// 隐藏LoadingWnd
    /// </summary>
    public static void HideLoadingWnd(int loadingType)
    {
        // 当前CurrLoadingType不是需要hide类型
        if (loadingType != CurrLoadingType || CurrLoadingWnd == null)
            return;

        // 销毁load窗口
        WindowMgr.DestroyWindow(CurrLoadingWnd.name);

        // 重置CurrLoadingWnd数据
        CurrLoadingWnd = null;
        loadingType = -1;
    }

    /// <summary>
    /// 显示LoadingWnd
    /// </summary>
    public static void ShowLoadingWnd(string WindowName, int loadingType, LPCMapping data)
    {
        // 获取窗口
        GameObject wnd = WindowMgr.GetWindow(WindowName);

        // 如果窗口已经显现
        if (wnd != null && wnd.activeSelf)
            return;

        // 窗口还没有显示
        if (wnd == null)
            wnd = WindowMgr.CreateWindow(WindowName, LoadingWnd.PrefebResource);

        // 创建失败
        if (wnd == null)
        {
            LogMgr.Trace("没有窗口prefab，创建失败");
            return;
        }

        if(mLoadEndMap.ContainsKey(loadingType))
            mLoadEndMap[loadingType] = new Dictionary<int, bool>();
        else
            mLoadEndMap.Add(loadingType, new Dictionary<int, bool>());

        // 记录进度条类型
        CurrLoadingType = loadingType;

        // 记录当前窗口
        CurrLoadingWnd = wnd;

        // 绑定窗口
        wnd.GetComponent<LoadingWnd>().SetLoading(loadingType, data);

        // 显示窗口
        WindowMgr.ShowWindow(wnd);
    }

    /// <summary>
    /// 显示Resloading窗口
    /// </summary>
    /// <param name="WindowName">Window name.</param>
    /// <param name="loadingType">Loading type.</param>
    /// <param name="precentMap">Precent map.</param>
    public static void ShowResLoadingWnd(string WindowName, int loadingType)
    {
        // 获取窗口
        GameObject wnd = WindowMgr.GetWindow(WindowName);

        // 如果窗口已经显现
        if (wnd != null && wnd.activeSelf)
            return;

        // 窗口还没有显示
        if (wnd == null)
            wnd = WindowMgr.CreateWindow(WindowName, ResourceLoadingWnd.PrefebResource);

        // 创建失败
        if (wnd == null)
        {
            LogMgr.Trace("没有窗口prefab，创建失败");
            return;
        }

        if(mLoadEndMap.ContainsKey(loadingType))
            mLoadEndMap[loadingType] = new Dictionary<int, bool>();
        else
            mLoadEndMap.Add (loadingType, new Dictionary<int, bool>());

        // 记录进度条类型
        CurrLoadingType = loadingType;

        // 记录当前窗口
        CurrLoadingWnd = wnd;

        // 显示窗口
        WindowMgr.ShowWindow(wnd);
    }

    /// <summary>
    /// 设置当前进度条进度
    /// </summary>
    /// <param name="progress">Progress.</param>
    public static void SetProgress(float progress)
    {
        if(eventProgressChange != null)
            eventProgressChange(progress);
    }

    /// <summary>
    /// 状态变化
    /// </summary>
    /// <param name="Procedure">Procedure.</param>
    /// <param name="state">State.</param>
    public static void ChangeState(int Procedure, int state)
    {
        if(eventProgressChange != null)
            eventStateChange(Procedure, state);
    }

    /// <summary>
    /// loading结束
    /// </summary>
    /// <param name="loadingType">Loading type.</param>
    public static void DoLoadingEnd(int loadingType, int proceduce)
    {
        if (!mLoadEndMap.ContainsKey (loadingType))
        {
            LogMgr.Trace("无该类型的loading窗口");
            return;
        }

        if(mLoadEndMap[loadingType].ContainsKey(proceduce))
            mLoadEndMap[loadingType][proceduce] = true;
        else
            mLoadEndMap[loadingType].Add(proceduce, false);
    }

    /// <summary>
    /// loading是否结束
    /// </summary>
    /// <returns><c>true</c> if is loading end the specified loadingType; otherwise, <c>false</c>.</returns>
    /// <param name="loadingType">Loading type.</param>
    public static bool IsLoadingEnd(int loadingType, int proceduce)
    {
        if (!mLoadEndMap.ContainsKey (loadingType))
            return false;

        if(!mLoadEndMap[loadingType].ContainsKey(proceduce))
            return false;

        return mLoadEndMap [loadingType][proceduce];
    }


}

