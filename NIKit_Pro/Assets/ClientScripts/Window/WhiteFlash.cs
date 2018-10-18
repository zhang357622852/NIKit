/// <summary>
/// WhiteFlash.cs
/// Created by zhaozy 2016/07/05
/// 普通刷屏窗口
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 刷屏窗口
/// </summary>
public class WhiteFlash : WindowBase<WhiteFlash>
{
    #region 公共接口

    /// <summary>
    /// Raises the finished event.
    /// </summary>
    public void OnFinished()
    {
        // 动画播放完毕需要析构本窗口
        WindowMgr.DestroyWindow(WhiteFlash.WndType);
    }

    #endregion
}
