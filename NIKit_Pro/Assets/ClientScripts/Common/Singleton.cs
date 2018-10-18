/// <summary>
/// Singleton.cs
/// Created by wangxw 2014-10-22
/// 单件模板
/// </summary>

using System;
using System.Collections.Generic;

public class Singleton<T> where T : new()
{
    // 单件实例对象
    protected static T mInstance = default(T);

    /// <summary>
    /// 获取单件对象
    /// </summary>
    /// <value>单件实例</value>
    public static T Instance
    {
        get
        {
            // 没有单件，则立即创建一个
            // Thread Unsafe
            if (Singleton<T>.mInstance == null)
                Singleton<T>.mInstance = ((default(T) == null) ? Activator.CreateInstance<T>() : default(T));

            return Singleton<T>.mInstance;
        }
    }

    /// <summary>
    /// 清理单件对象
    /// </summary>
    public void CleanInstance()
    {
        Singleton<T>.mInstance = default(T);
    }
}
