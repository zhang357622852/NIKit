/// <summary>
/// ColorConfig.cs
/// Created by zhaozy 2014-11-19
/// 颜色管理
/// </summary>

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// 脚本管理
public static class ColorConfig
{
    #region 变量

    // 颜色配置
    public const int NC_WHITE = 201;
    public const int NC_GREEN = 202;
    public const int NC_BLUE = 203;
    public const int NC_RED = 204;
    public const int NC_PURPLE = 205;
    public const int NC_GREY = 206;
    public const int NC_YELLOW = 207;
    public const int NC_DIAMOND = 208;

    // 装备颜色
    public const int EC_WHITE = 500;
    public const int EC_GREEN = 501;
    public const int EC_BLUE = 502;
    public const int EC_PURPLE = 503;
    public const int EC_DARKGOLDENROD = 504;

    // 颜色标签
    public static Dictionary<string, string> mColorTag = new Dictionary<string, string>()
    {
        { "#NC_WHITE", "ffffff" },
        { "#NC_GREEN", "82ff75" },
        { "#NC_BLUE", "01d8f8" },
        { "#NC_RED", "ff5151" },
        { "#NC_PURPLE", "b534fe" },
        { "#NC_GREY", "afafaf" },
        { "#NC_YELLOW", "ffe96d" },
        { "#NC_DIAMOND", "a3fff9" },

        { "#EC_WHITE", "ffffff" },
        { "#EC_GREEN", "82ff75" },
        { "#EC_BLUE", "6ed1ff" },
        { "#EC_PURPLE", "e26eff" },
        { "#EC_DARKGOLDENROD", "ffb26e" },
    };

    #endregion

    #region 私有接口

    /// <summary>
    /// 构造函数
    /// </summary>
    static ColorConfig()
    {
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 16进制颜色转为浮点型颜色("ffffffff")
    /// </summary>
    public static Color ParseToColor(string strColor)
    {
        // 转换为16进制
        uint iColor = Convert.ToUInt32(strColor, 16);

        // 计算alpha数值
        // 没有输入alpha，则默认为1
        uint a = 0;
        if (strColor.Length <= 6)
            a = 0xFF;
        else
            a = (iColor >> 24);

        // 计算RGB数值
        uint r = (iColor >> 16) & 0x00FF;
        uint g = (iColor >> 8) & 0x0000FF;
        uint b = iColor & 0x000000FF;

        // 计算rate
        float rate = 1.0f / 255.0f;

        // 转换为unity的color
        return new Color(r * rate, g * rate, b * rate, a * rate);
    }

    /// <summary>
    /// hex转换到color
    /// </summary>
    /// <param name="hex"></param>
    /// <returns></returns>
    public static Color HexToColor(string hex)
    {
        byte br = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte bg = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte bb = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        byte cc = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
        float r = br / 255f;
        float g = bg / 255f;
        float b = bb / 255f;
        float a = cc / 255f;
        return new Color(r, g, b, a);
    }

    /// <summary>
    /// 提取十六进制
    /// [FF4512] => FF4512
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ExtractColorHex(string str)
    {
        return str.Replace("[", "").Replace("]", "");
    }

    #endregion
}
