/// <summary>
/// UIRootInit
/// Created by WinMi 2018/11/16
/// 绑定UI Root
/// 挂载到UI Root
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRootInit : MonoBehaviour
{
    private void Awake()
    {
        UIMgr.Instance.mFormsUIRoot = gameObject;
    }
}
