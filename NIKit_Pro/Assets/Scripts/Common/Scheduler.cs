/// <summary>
/// Scheduler.cs
/// Created by wangxw 2014-11-05
/// 全局逻辑驱动器
/// 本对象挂接在GameRootObject上，通过MonoBehaviour的update驱动
/// </summary>
using UnityEngine;
using System.Collections;

public class Scheduler : MonoBehaviour
{
    /// <summary>
    /// 安全执行一段代码
    /// </summary>
    public delegate void SafeCallFunc();

    public static void Call(SafeCallFunc f)
    {
        try
        {
            f();
        }
        catch (System.Exception e)
        {
            NIDebug.LogException(e);
        }
    }


    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR

#else
        // 音效更新驱动
        //Call(() => {
        //    GameSoundMgr.Update();
        //});
#endif
    }
}
