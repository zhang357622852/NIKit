using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameUtility
{
    /// <summary>
    /// 比较两个浮点数是否相等
    /// </summary>
    /// <returns><c>true</c>, if equal was floated, <c>false</c> otherwise.</returns>
    /// <param name="tolerance">误差范围</param>
    public static bool FloatEqual(float a, float b, float tolerance = float.Epsilon)
    {
        return (Math.Abs(b - a) <= tolerance);
    }

    public static string[] Explode(string path, string seperator)
    {
        return path.Split(seperator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
    }

    public static string ConvertToNGUIFormat(string c_format_text)
    {
        string temp_text = c_format_text.Replace("<br/>", "\n");
        temp_text = temp_text.Replace(@"<\>", "[");
        temp_text = temp_text.Replace(@"</>", "]");

        foreach (KeyValuePair<string, string> v in ColorConfig.mColorTag)
            temp_text = temp_text.Replace(v.Key, v.Value);

        return temp_text;
    }

    /// <summary>
    /// 计算屏幕缩放
    /// NGUI3.7之前，没有基于宽度的适配，可以使用这个
    /// </summary>
    /// <returns>The screen scale.</returns>
    public static float CalcWndScale()
    {
        float scale = 1.0f;

        float basicScale = (float)16 / 9;
        float screenScale = (float)Screen.width / Screen.height;

        if (basicScale < screenScale)
            scale = basicScale / screenScale;

        return scale;
    }
}
