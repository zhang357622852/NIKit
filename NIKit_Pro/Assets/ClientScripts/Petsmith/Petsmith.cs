/// <summary>
/// Petsmith.cs
/// Created by lic 2016-9-8
/// 宠物工坊基类
/// </summary>

using UnityEngine;
using System.Collections;

public abstract class Petsmith
{
    public abstract bool DoAction(params object[] args);

    public abstract bool DoActionResult(params object[] args);
}
