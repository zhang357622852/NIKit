/// <summary>
/// Blacksmith.cs
/// Created by fucj 2014-12-16
/// 工坊基类
/// </summary>

using UnityEngine;
using System.Collections;

public abstract class Blacksmith
{
    public abstract bool DoAction(params object[] args);

    public abstract bool DoActionResult(params object[] args);
}
