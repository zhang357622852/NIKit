/// <summary>
/// Formula.cs
/// Copy from zhangyg 2014-10-22
/// 公式基类
/// </summary>

using System;
using System.Collections.Generic;
using System.Reflection;

// 脚本管理
public abstract class Formula
{
    #region 公共接口

    /// <summary>
    /// 调用公式
    /// </summary>
    public object Invoke(params object[] args)
    {
        // 获取公式入口函数
        Type type = this.GetType();
        MethodInfo methodInfo = type.GetMethod("Call");

        // 没有该接口不处理
        if(methodInfo == null)
            return null;

        // 调用公式计算
        return methodInfo.Invoke(this, args);
    }

    #endregion
}
