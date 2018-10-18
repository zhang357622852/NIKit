/// <summary>
/// SCRIPT_1000.cs
/// Create by zhaozy 2014-11-6
/// 初始化脚本
/// </summary>

using System;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 计算装备基础属性价值(简单线性类)
/// </summary>
public class SCRIPT_101 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray prop = (LPCArray)_params[0];
        LPCValue arg = _params[2] as LPCValue;

        return (float)(arg.AsFloat * prop[1].AsFloat);
    }
}

/// <summary>
/// 默认初始化脚本
/// </summary>
public class SCRIPT_1002 : Script
{
    public override object Call(params object[] _params)
    {
        Property ob = _params[0] as Property;

        // 初始化技能
        SkillMgr.InitSkill(ob);

        return true;
    }
}
