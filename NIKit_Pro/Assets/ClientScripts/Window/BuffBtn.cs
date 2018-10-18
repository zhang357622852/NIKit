/// <summary>
/// BuffBtn.cs
/// Created by tanzy 2016-5-16
/// Note
/// </summary>

using System;
using UnityEngine;
using System.Collections;
using LPC;

public class BuffBtn : WindowBase<BuffBtn>
{
    #region 成员变量
    public UILabel  lbCount;    // 持续回合数
    public UISprite icon;      // 图标
    #endregion


    #region 公共函数

    /// <summary>
    /// 赋值
    /// </summary>
    public void SetType(int statusId, int round)
    {
        // 获取Status信息
        CsvRow statusInfo = StatusMgr.GetStatusInfo(statusId);
        icon.spriteName = statusInfo.Query<string>("icon");
        lbCount.text = round.ToString();
    }


    #endregion
}

