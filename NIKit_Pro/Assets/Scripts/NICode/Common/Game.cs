using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Game
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
}
