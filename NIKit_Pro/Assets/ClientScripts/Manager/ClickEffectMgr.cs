/// <summary>
/// ClickEffectMgr.cs
/// Created by wangxw 2015-01-08
/// 点击效果管理器
/// </summary>

using UnityEngine;
using System.Collections;

public static class ClickEffectMgr
{
    #region 成员变量

    private const string mClickEffectPrefab = "Prefabs/Effect/E9010";

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
    }

    /// <summary>
    /// 通知一次点击效果
    /// </summary>
    public static IEnumerator NotifyClick()
    {
        yield return null;

        // 创建光效
        if (SceneMgr.UiCamera != null)
        {
            ClickEffectWnd.Show();
        }
    }

    #endregion
}
