/// <summary>
/// VersionWnd.cs
/// Created by xuhd Sec/22/2014
/// 显示版本号
/// </summary>
using UnityEngine;
using System.Collections;

public class VersionWnd : MonoBehaviour
{
    public UILabel mVersion;

    // Use this for initialization
    void Start()
    {
        mVersion.text = string.Format("Ver {0}", ConfigMgr.ClientVersion);
    }
}
