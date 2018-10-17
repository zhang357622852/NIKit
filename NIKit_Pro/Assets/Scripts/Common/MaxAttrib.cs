/// <summary>
/// MaxAttrib.cs
/// Created by zhaozy 2014-12-8
/// 属性最大值管理
/// </summary>

using System;
using System.Collections.Generic;
using System.Reflection;

// 脚本管理
public static class MaxAttrib
{
    #region 公共接口

    /// <summary>
    /// 属性最大值
    /// </summary>
    public static int GetMaxAttrib(string attrib)
    {
        // 返回配置信息
        return GameSettingMgr.GetSettingInt(attrib);
    }

    #endregion
}
