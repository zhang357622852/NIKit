/// <summary>
/// DotnetExtensions.cs
/// Created by WinMi 2018/11/05
/// 自定义类型扩展
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

public static class ClassExtensions
{
    /// <summary>
    /// 判断本身为null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="selfObj"></param>
    /// <returns></returns>
    public static bool IsNull<T>(this T selfObj) where T : class
    {
        return null == selfObj;
    }

    /// <summary>
    /// 判断本身不为null
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="selfObj"></param>
    /// <returns></returns>
    public static bool IsNotNull<T>(this T selfObj) where T : class
    {
        return null != selfObj;
    }
}

public static class StringExtensions
{
    public static StringBuilder Append(this string selfStr, string toAppend)
    {
        return new StringBuilder(selfStr).Append(toAppend);
    }
}

/// <summary>
/// 文件读写复制操作，对System.IO的一些封装
/// </summary>
public static class IOExtensions
{
    /// <summary>
    /// 创建新的文件夹，如果存在则不创建
    /// </summary>
    /// <param name="dirFullPath"></param>
    /// <returns></returns>
    public static string CreateDirIfNotExist(this string dirFullPath)
    {
        if (!Directory.Exists(dirFullPath))
            Directory.CreateDirectory(dirFullPath);

        return dirFullPath;
    }
}

public static class ReflectionExtension
{
    public static void Example()
    {
        var selfType = ReflectionExtension.GetAssemblyCSharp().GetType("QFramework.ReflectionExtension");

        Debug.Log(selfType);
    }

    public static Assembly GetAssemblyCSharp()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var a in assemblies)
        {
            if (a.FullName.StartsWith("Assembly-CSharp,"))
            {
                return a;
            }
        }

        Debug.LogError(">>>>>>>Error: Can\'t find Assembly-CSharp.dll");

        return null;
    }

    public static Assembly GetAssemblyCSharpEditor()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var a in assemblies)
        {
            if (a.FullName.StartsWith("Assembly-CSharp-Editor,"))
            {
                return a;
            }
        }

        Debug.LogError(">>>>>>>Error: Can\'t find Assembly-CSharp-Editor.dll");

        return null;
    }

    /// <summary>
    /// 通过反射方式调用函数
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="methodName">方法名</param>
    /// <param name="args">参数</param>
    /// <returns></returns>
    public static object InvokeByReflect(this object obj, string methodName, params object[] args)
    {
        var methodInfo = obj.GetType().GetMethod(methodName);

        return methodInfo == null ? null : methodInfo.Invoke(obj, args);
    }

    /// <summary>
    /// 通过反射方式获取域值
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="fieldName">域名</param>
    /// <returns></returns>
    public static object GetFieldByReflect(this object obj, string fieldName)
    {
        var fieldInfo = obj.GetType().GetField(fieldName);

        return fieldInfo == null ? null : fieldInfo.GetValue(obj);
    }

    /// <summary>
    /// 通过反射方式获取属性
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="fieldName">属性名</param>
    /// <returns></returns>
    public static object GetPropertyByReflect(this object obj, string propertyName, object[] index = null)
    {
        var propertyInfo = obj.GetType().GetProperty(propertyName);
        return propertyInfo == null ? null : propertyInfo.GetValue(obj, index);
    }

    /// <summary>
    /// 拥有特性
    /// </summary>
    /// <returns></returns>
    public static bool HasAttribute(this PropertyInfo prop, Type attributeType, bool inherit)
    {
        return prop.GetCustomAttributes(attributeType, inherit).Length > 0;
    }

    /// <summary>
    /// 拥有特性
    /// </summary>
    /// <returns></returns>
    public static bool HasAttribute(this FieldInfo field, Type attributeType, bool inherit)
    {
        return field.GetCustomAttributes(attributeType, inherit).Length > 0;
    }

    /// <summary>
    /// 拥有特性
    /// </summary>
    /// <returns></returns>
    public static bool HasAttribute(this Type type, Type attributeType, bool inherit)
    {
        return type.GetCustomAttributes(attributeType, inherit).Length > 0;
    }

    /// <summary>
    /// 拥有特性
    /// </summary>
    /// <returns></returns>
    public static bool HasAttribute(this MethodInfo method, Type attributeType, bool inherit)
    {
        return method.GetCustomAttributes(attributeType, inherit).Length > 0;
    }

    /// <summary>
    /// 获取第一个特性
    /// </summary>
    public static T GetFirstAttribute<T>(this MethodInfo method, bool inherit) where T : Attribute
    {
        var attrs = (T[])method.GetCustomAttributes(typeof(T), inherit);

        if (attrs != null && attrs.Length > 0)
            return attrs[0];

        return null;
    }

    /// <summary>
    /// 获取第一个特性
    /// </summary>
    public static T GetFirstAttribute<T>(this FieldInfo field, bool inherit) where T : Attribute
    {
        var attrs = (T[])field.GetCustomAttributes(typeof(T), inherit);

        if (attrs != null && attrs.Length > 0)
            return attrs[0];

        return null;
    }

    /// <summary>
    /// 获取第一个特性
    /// </summary>
    public static T GetFirstAttribute<T>(this PropertyInfo prop, bool inherit) where T : Attribute
    {
        var attrs = (T[])prop.GetCustomAttributes(typeof(T), inherit);

        if (attrs != null && attrs.Length > 0)
            return attrs[0];

        return null;
    }

    /// <summary>
    /// 获取第一个特性
    /// </summary>
    public static T GetFirstAttribute<T>(this Type type, bool inherit) where T : Attribute
    {
        var attrs = (T[])type.GetCustomAttributes(typeof(T), inherit);

        if (attrs != null && attrs.Length > 0)
            return attrs[0];

        return null;
    }
}
