/// <summary>
/// WindowBase.cs
/// Created by wangxw 2014-11-04
/// 窗口对象基类模板
/// 提供窗口的静态shou/hide方法
/// </summary>

using UnityEngine;
using System.Collections;

public class WindowBaseSource : MonoBehaviour
{
    #region protect成员

    // 是否是子窗口
    protected bool mIsSubWnd = true;

    #endregion

    #region 公共成员

    /// <summary>
    /// 是否是子窗口
    /// </summary>
    /// <value><c>true</c> if this instance is sub window; otherwise, <c>false</c>.</value>
    public bool IsSubWnd
    {
        get
        {
            return mIsSubWnd;
        }
        
        set
        {
            mIsSubWnd = value;
            
            // 如果是主窗口，加入窗口列表
            if (!mIsSubWnd)
                WindowMgr.WindowList.Add(this);
        }
    }

    #endregion

    /// <summary>
    /// 窗口初始化
    /// </summary>
    public virtual void _InitWnd()
    {
        // 如果子类没有重载该接口，将IsSubWnd设为false
        // 则该窗口为子窗口
        IsSubWnd = true;
    }

    /// <summary>
    /// 隐藏窗口
    /// </summary>
    public virtual void _DetachWnd()
    {
        if (mIsSubWnd)
            return;

        if (WindowMgr.WindowList.Contains(this))
            WindowMgr.WindowList.Remove(this);
    }

    /// <summary>
    /// 手机按下返回键
    /// </summary>
    public virtual bool OnBack()
    {
        // 如果子类没有重载该接口，修改返回值
        // 则默认不响应手机返回键
        return false;
    }
}

public class WindowBase<T> : WindowBaseSource
{
    // 窗口类型名
    public static readonly string WndType = typeof(T).Name;

    // 预设资源路径
    public static string PrefebResource
    {
        get
        {
            return string.Format("Assets/Prefabs/Window/{0}.prefab", WindowMgr.GetCustomWindowName(WndType));
        }
    }

    /// <summary>
    /// 以默认名查找实例窗口对象，显示
    /// </summary>
    /// <param name="createWhenMiss">没有同名对象的时候，是否创建</param>
    public static void Show(bool createWhenMiss = true)
    {
        // 默认窗口对象名就是子类的名字
        GameObject wnd = WindowMgr.GetWindow(WndType);
        if (wnd != null)
        {
            WindowMgr.ShowWindow(wnd);
            return;
        }

        // 尝试创建同一个实例
        if (createWhenMiss)
        {
            wnd = WindowMgr.CreateWindow(WndType, PrefebResource);
            WindowMgr.ShowWindow(wnd);
        }
    }

    /// <summary>
    /// 隐藏窗口
    /// </summary>
    public static void Hide(bool toDestroy = false)
    {
        if (!toDestroy)
        {
            // 隐藏窗口
            GameObject wnd = WindowMgr.GetWindow(WndType);
            if (wnd != null)
                WindowMgr.HideWindow(wnd);
        }
        else
        {
            // 销毁窗口
            WindowMgr.DestroyWindow(WndType);
        }
    }

    /// <summary>
    /// 计算下一个可用的控件深度值，没有控件时返回0
    /// </summary>
    /// <returns>The next widget depth.</returns>
    /// <param name="go">Go.</param>
    public int CalcNextWidgetDepth(GameObject go)
    {
        int nextDepth = -1;

        UIWidget[] widgets = go.GetComponentsInChildren<UIWidget>(true);
        if (widgets == null || widgets.Length <= 0)
            return 0;

        for (int i = 0; i < widgets.Length; ++i)
            nextDepth = Mathf.Max(nextDepth, widgets[i].depth);

        return nextDepth + 1;
    }
}
