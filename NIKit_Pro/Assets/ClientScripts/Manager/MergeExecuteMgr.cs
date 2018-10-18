/// <summary>
/// MergeExecuteMgr.cs
/// Created by wangxw 2015-01-16
/// 合并延迟执行管理模块
/// 标记要调用的函数“dirty”，然后在固定的时间调用一次即可
/// 主要用于window的redraw()类函数
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public static class MergeExecuteMgr
{
    #region 成员变量

    public delegate void ExecuteFun();
    private static List<ExecuteFun> mNeedExecuteFunList = new List<ExecuteFun>();

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // do nothing
    }

    /// <summary>
    /// 更新模块，执行相应操作
    /// </summary>
    public static void Update()
    {
        do
        {
            // 列表为空
            if (mNeedExecuteFunList.Count == 0)
                break;

            // 从第一个原始开始处理
            ExecuteFun fun = mNeedExecuteFunList[0];
            mNeedExecuteFunList.RemoveAt(0);

            // 如果回调函数不存在，不处理
            if (fun == null)
                continue;

            // 执行ExecuteFun
            fun();

        } while(true);
    }

    /// <summary>
    /// 添加一个待执行的函数
    /// </summary>
    /// <param name="fun">Fun.</param>
    public static void DispatchExecute(ExecuteFun fun)
    {
        // 如果已经在列表中
        if (mNeedExecuteFunList.IndexOf(fun) != -1)
            return;

        // 添加到列表中
        mNeedExecuteFunList.Add(fun);
    }

    #endregion
}
