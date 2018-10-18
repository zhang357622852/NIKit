/// <summary>
/// LogMgr.cs
/// Copy from zhangyg 2014-10-22
/// Log类的接口封装
/// </summary>

using System;
using System.Reflection;
using System.Diagnostics;
using UnityEngine;
using System.Collections.Generic;
using LPC;

public class LogMgr
{
    /// <summary>
    /// Trace the specified message and paras.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="paras">Paras.</param>
    public static void Trace(string message, params object[] paras)
    {
#if UNITY_EDITOR
        // 编辑器模式显示信息
        string content = message;
        if (paras != null)
            content = string.Format(message, paras);

        UniLogger.Log.Minor(content);
#endif
    }

    /// <summary>
    /// Normal the specified message and paras.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="paras">Paras.</param>
    public static void Normal(string message, params object[] paras)
    {
#if UNITY_EDITOR
        // 内容
        string content = message;
        if (paras != null)
            content = string.Format(message, paras);

        UniLogger.Log.Normal(content);
#endif
    }

    /// <summary>
    /// Important the specified message and paras.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="paras">Paras.</param>
    public static void Important(string message, params object[] paras)
    {
#if UNITY_EDITOR
        // 内容
        string content = message;
        if (paras != null)
            content = string.Format(message, paras);

        UniLogger.Log.Important(content);
#endif
    }

    /// <summary>
    /// Critical the specified message and paras.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="paras">Paras.</param>
    public static void Critical(string message, params object[] paras)
    {
#if UNITY_EDITOR
        // 内容
        string content = message;
        if (paras != null)
            content = string.Format(message, paras);

        UniLogger.Log.Critical(content);
#endif
    }

    /// <summary>
    /// Error the specified message and paras.
    /// </summary>
    /// <param name="message">Message.</param>
    /// <param name="paras">Paras.</param>
    public static void Error(string message, params object[] paras)
    {
        // 内容
        string content = message;
        if (paras != null)
            content = string.Format(message, paras);

        // 记录log信息
        UnityEngine.Debug.LogError(content);
    }

    /// <summary>
    /// Exception the specified e.
    /// </summary>
    /// <param name="e">E.</param>
    public static void Exception(Exception e)
    {
        // 输出log信息
        UnityEngine.Debug.LogException(e);
    }

    /// <summary>
    /// Network the specified msgNo and param.
    /// </summary>
    /// <param name="msgNo">Message no.</param>
    /// <param name="param">Parameter.</param>
    public static void Network(int msgNo, LPCValue param)
    {
#if UNITY_EDITOR
        string msgName = MsgMgr.MsgName(msgNo).ToUpper();
        byte[] data = PktAnalyser.Pack(msgNo, (LPCValue)param);
        int size = data.Length;

        string callerName = msgName.Substring(0, 3) == "CMD" ? GetCallerName(new StackTrace()) : "";

        // 打印到 log view
        string message = string.Format("===== {0} : {1} : {2} ===== \n {3} ", callerName, msgName, size, param.GetDescription());
        UniLogger.Log.Record(UniLogger.Log.Category.Network, UniLogger.Log.Level.Normal, message);
#endif
    }

    /// <summary>
    /// Gets the name of the caller.
    /// </summary>
    /// <returns>The caller name.</returns>
    /// <param name="trace">Trace.</param>
    public static string GetCallerName(StackTrace trace)
    {
        int top = trace.GetFrames().Length - 1;
        StackFrame frame = trace.GetFrames() [top];
        MethodBase method = frame.GetMethod();
        return string.Format("{0}:{1}", method.ReflectedType.Name, method.Name);
    }

    /// <summary>
    /// @Description: 获取当前堆栈的上级调用方法列表,直到最终调用者,只会返回调用的各方法，而不会返回具体的出错行数
    /// 可参考：微软真是个十足的混蛋啊！让我们跟踪Exception到行把！（不明真相群众请入） 
    /// 具体参考文档链接 ： https://www.cnblogs.com/zc22/archive/2009/12/25/1631773.html
    /// </summary>
    /// <returns></returns>
    public static string GetStackTrace()
    {
        // 当前堆栈信息
        System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
        System.Diagnostics.StackFrame[] sfs = st.GetFrames();

        // 全部调用栈信息
        string _fullTrace = string.Empty;

        //过虑的方法名称,以下方法将不会出现在返回的方法调用列表中
        for (int i = 1; i < sfs.Length; ++i)
        {
            // 非用户代码,系统方法及后面的都是系统调用，不获取用户代码调用结束
            if (System.Diagnostics.StackFrame.OFFSET_UNKNOWN == sfs[i].GetILOffset())
                break;

            // 拼接调用栈信息
            _fullTrace = _fullTrace + string.Format("{0}:{1}\n",
                sfs[i].GetMethod().DeclaringType,
                sfs[i].GetMethod().ToString());
        }

        // 释放当前堆栈信息
        st = null;
        sfs = null;

        // 返回获取调用栈信息
        return _fullTrace;
    }

}
