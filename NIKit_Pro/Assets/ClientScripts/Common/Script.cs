/// <summary>
/// Script.cs
/// Copy from zhangyg 2014-10-22
/// 脚本基类
/// </summary>

using System;
using System.Collections.Generic;
using System.Reflection;

// 脚本管理
public abstract class Script
{
    public abstract object Call(params object[] args);
}
