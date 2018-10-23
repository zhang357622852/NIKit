/// <summary>
/// MixedValue.cs
/// Created by wangxw 2014-11-11
/// 混合类型
/// 通用的C#类型
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using UnityEngine;

public class MixedValue
{
    #region 内部实现

    // 存放数据的对象实例
    private IMixedValue mValue = null;

    // 接口类，隐藏具体类型
    interface IMixedValue
    {
        // 获取数据类型
        Type GetValueType();
    }

    // 具体的类型模板子类
    class MixedValueImpl<T> : IMixedValue
    {
        public T Value = default(T);

        public Type GetValueType()
        {
            return typeof(T);
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 获取值类型
    /// </summary>
    /// <returns>The value type.</returns>
    public Type GetValueType()
    {
        if (mValue == null)
            return null;
        else
            return mValue.GetValueType();
    }

    /// <summary>
    /// 获取数据值
    /// </summary>
    /// <returns>The value.</returns>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public T GetValue<T>()
    {
        if (mValue != null && mValue.GetValueType() == typeof(T))
            return (mValue as MixedValueImpl<T>).Value;

        NIDebug.LogError("MixedValue不是{0}", typeof(T).Name);

        return default(T);
    }

    /// <summary>
    /// 设置数据值
    /// </summary>
    /// <param name="value">Value.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public void SetValue<T>(T value)
    {
        MixedValueImpl<T> instance = new MixedValueImpl<T>();
        instance.Value = value;
        mValue = instance;
    }

    /// <summary>
    /// 构造并赋值
    /// 简化书写：new() + SetValue
    /// </summary>
    /// <returns>The mixed value.</returns>
    /// <param name="value">Value.</param>
    /// <typeparam name="T">The 1st type parameter.</typeparam>
    public static MixedValue NewMixedValue<T>(T value)
    {
        MixedValue mv = new MixedValue();
        mv.SetValue<T>(value);
        return mv;
    }

    #endregion
}
