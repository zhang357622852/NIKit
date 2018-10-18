/// <summary>
/// ElementMgr.cs
/// Create by Lic 2016-8-5
/// cd管理模块
/// </summary>
using System;
using System.Diagnostics;
using System.Collections.Generic;
using LPC;

public class ElementMgr 
{

    /// <summary>
    /// 获取元素克制关系(前者表示攻击者)
    /// </summary>
    /// <returns>The element restraint.</returns>
    /// <param name="element">attacker_element.</param>
    /// <param name="element1">defender_element.</param>
    public static int GetElementCounter(int attacker_element,int defender_element)
    {
        int elementType = ElementConst.ELEMENT_COUNTER_ARRAY.Length;

        if(attacker_element >= elementType || defender_element >= elementType)
        {
            LogMgr.Trace(string.Format("无元素{0}和{1}的克制关系", attacker_element, defender_element));
            return 0;
        }

        return ElementConst.ELEMENT_COUNTER_ARRAY[attacker_element, defender_element];
    }

    /// <summary>
    /// 获取宠物克制关系(前者表示攻击者)
    /// </summary>
    /// <returns>The monster restraint.</returns>
    /// <param name="attacker">Attacker.</param>
    /// <param name="defender">Be attacker.</param>
    public static int GetMonsterCounter(Property attacker, Property defender)
    {
        if(attacker == null || defender == null)
            return 0;

        // 获取两者的元素
        int attacker_element = attacker.BasicQueryNoDuplicate<int>("element");
        int defender_element = defender.BasicQueryNoDuplicate<int>("element");

        int elementType = ElementConst.ELEMENT_COUNTER_ARRAY.Length;

        if(attacker_element >= elementType || defender_element >= elementType)
        {
            LogMgr.Trace(string.Format("无元素{0}和{1}的克制关系", attacker_element, defender_element));
            return 0;
        }

        return ElementConst.ELEMENT_COUNTER_ARRAY[attacker_element, defender_element];
    }
}
