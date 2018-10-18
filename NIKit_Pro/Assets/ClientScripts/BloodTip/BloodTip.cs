/// <summary>
/// BloodTip.cs
/// Created by lic 2016-10-20
/// 血条提示基类
/// </summary>
/// 
using UnityEngine;
using System.Collections;

public abstract class BloodTip : MonoBehaviour
{
    public abstract void BindData(object[] args);
}
