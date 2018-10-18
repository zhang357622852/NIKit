/// <summary>
/// SCRIPT_2000.cs
/// Create by zhaozy 2014-11-6
/// 玩家属性脚本
/// </summary>

using System;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 使魔刷新属性处理脚本, 重新计算装备者的效果
/// </summary>
public class SCRIPT_2000 : Script
{
    public override object Call(params object[] _params)
    {
        return true;
    }
}

/// <summary>
/// 怪物刷新属性处理脚本
/// </summary>
public class SCRIPT_2001 : Script
{
    public override object Call(params object[] _params)
    {
        // 整理使魔的附加属性
        Property pet = _params[0] as Property;
        LPCMapping improvement = (_params[1] as LPCValue).AsMapping;

        // 记录original_attrib的计算结果
        LPCMapping originalAttrib = LPCMapping.Empty;
        for (int i = 0; i < PropertyInitConst.PetOriginalAttribs.Count; i++)
        {
            originalAttrib.Add(PropertyInitConst.PetOriginalAttribs[i],
                CALC_PET_ORIGINAL_ATTRIB.Call(pet, improvement, PropertyInitConst.PetOriginalAttribs[i]));
        }

        // 记录原始属性
        pet.SetTemp("original_attrib", LPCValue.Create(originalAttrib));

        string attribName;

        // 计算各基本属性加值
        for (int i = 0; i < PropertyInitConst.PetBasicAttribs.Count; i++)
        {
            // 计算属性加值
            attribName = PropertyInitConst.PetBasicAttribs[i];

            // 添加到improvement中
            improvement.Add(attribName, CALC_PET_IMP_ATTRIB.Call(pet, improvement, attribName));
        }

        // 属性刷新后处理
        PropMgr.RefreshAttribAfter(pet, PropertyInitConst.AttribList);

        // 返回成功
        return true;
    }
}

/// <summary>
/// 道具刷新属性处理脚本
/// </summary>
public class SCRIPT_2002 : Script
{
    public override object Call(params object[] _params)
    {
        return true;
    }
}
