/// <summary>
/// ItemWindowBase.cs
/// Created by xuhd Nov/26/2014
/// 用于封装无Panel子窗口
/// </summary>
using UnityEngine;
using System.Collections;

public class ItemWndBase<T> : WindowBase<T>
{
    /// <summary>
    /// 设置窗口深度值
    /// 这类窗口没有panel，但是为了保证该类窗口显示的正常
    /// 需要设置其Widget的depth，创建它的地方知道它应该显示在什么深度
    /// WindowMgr做的工作只是对资源加载封装了一层
    /// </summary>
    public void SetWindowDepth(int depth)
    {
        UIWidget[] widgets = gameObject.GetComponentsInChildren<UIWidget>(true);

        if (widgets == null || widgets.Length <= 0)
        {
            LogMgr.Trace("{0}没有深度值信息", typeof(T).Name);
            return;
        }

        // 先找到深度值最小的
        // 假设初始控件的深度值不会被设置为大于1000000
        int minDepth = 1000000;
        for (int i = 0; i < widgets.Length; ++i)
        {
            minDepth = Mathf.Min(widgets [i].depth, minDepth);
        }

        for (int i = 0; i < widgets.Length; ++i)
        {
            widgets [i].depth = depth + (widgets [i].depth - minDepth);
        }
    }
}
