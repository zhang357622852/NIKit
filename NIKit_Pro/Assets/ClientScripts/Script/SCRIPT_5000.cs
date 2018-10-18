/// <summary>
/// SCRIPT_5000.cs
/// Create by fengkk 2014-11-25
/// 装备脚本
/// </summary>

using System;
using UnityEngine;
using System.Collections.Generic;
using LPC;

// 装备初始化脚本
public class SCRIPT_5000 : Script
{
    public override object Call(params object[] _params)
    {
        return true;
    }
}

// 穿上装备后的处理脚本
public class SCRIPT_5001 : Script
{
    public override object Call(params object[] _params)
    {
        Property equip = _params[1] as Property;

        // 删除新装备字段
        equip.DeleteTemp("is_new");

        return true;
    }
}

// 卸载装备后的处理脚本
public class SCRIPT_5002 : Script
{
    public override object Call(params object[] _params)
    {
        return true;
    }
}

/// <summary>
/// 附加属性值的描述脚本（不带百分比的数值，整型或者浮点型）
/// </summary>
public class SCRIPT_5003 : Script
{
    public override object Call(params object[] _params)
    {
        string desc_arg = _params[0] as String;
        LPCArray prop = _params[1] as LPCArray;
        string colorLabel = _params[2] as string;

        float propValue = prop[1].AsFloat;

        string desc = string.Empty;
        if (!string.IsNullOrEmpty(colorLabel))
            desc = string.Format(desc_arg, ((propValue >= 0f) ? colorLabel + "+" : ""), propValue + "[-]");
        else
            desc = string.Format(desc_arg, ((propValue >= 0f) ? "+" : ""), propValue);

        return desc;
    }
}

/// <summary>
/// 附加属性百分比的描述脚本
/// </summary>
public class SCRIPT_5004 : Script
{
    public override object Call(params object[] _params)
    {
        string desc_arg = _params[0] as String;
        LPCArray prop = _params[1] as LPCArray;
        string colorLabel = _params[2] as string;

        float temp_prop = (float)prop[1].AsInt / 10;

        string desc = string.Empty;
        if (!string.IsNullOrEmpty(colorLabel))
            desc = string.Format(desc_arg, ((prop[1].AsInt >= 0) ? colorLabel + "+" : ""), temp_prop.ToString() + "%" + "[-]");
        else
            desc = string.Format(desc_arg, ((prop[1].AsInt >= 0) ? "+" : ""), temp_prop.ToString() + "%");

        return desc;
    }
}

/// <summary>
/// 装备道具的短描述脚本
/// </summary>
public class SCRIPT_5005 : Script
{
    public override object Call(params object[] _params)
    {
        Property equip = _params[0] as Property;
        string text = string.Empty;

        //获取装备的属性;
        LPCMapping propMap = equip.Query<LPCMapping>("prop");

        // 获取装备的词缀属性;
        LPCArray prefixProp = propMap.GetValue<LPCArray>(EquipConst.PREFIX_PROP);

        // 获取装备的主属性
        LPCArray mainProp = propMap.GetValue<LPCArray>(EquipConst.MAIN_PROP);

        // 获取装备的前缀属性名称
        if (prefixProp != null)
        {
            foreach (LPCValue prop in prefixProp.Values)
                text = string.Format("{0}{1}{2}", text, PropMgr.GetPropPrefix(prop.AsArray[0].AsInt),LocalizationMgr.Get("prop_prefix_none"));
        }

        // 获取装备的主属性名称
        if (mainProp != null)
        {
            foreach (LPCValue main in mainProp.Values)
                text = string.Format("{0}{1}", text, PropMgr.GetPropPrefix(main.AsArray[0].AsInt));
        }

        // 获取套装名
        CsvRow suitMap = EquipMgr.GetSuitTemplate(equip.Query<int>("suit_id"));
        if (suitMap != null)
        {
            text = string.Format("{0}{1}", text, LocalizationMgr.Get(suitMap.Query<string>("name")));
        }

        // 装备自身名称
        text = string.Format("{0}{1}", text, LocalizationMgr.Get(equip.Query<string>("name")));

        // 获取装备的稀有度
        int rarity = equip.GetRarity();
        if (rarity != EquipConst.RARITY_WHITE)
            text = string.Format("{0}-{1}", text, EquipMgr.GetRarityAlias(rarity));

        // 返回描述信息
        return text;
    }
}

/// <summary>
/// 基础属性百分比的描述脚本
/// </summary>
public class SCRIPT_5006 : Script
{
    public override object Call(params object[] _params)
    {
        string desc_arg = _params[0] as String;
        LPCValue prop = _params[1] as LPCValue;

        float temp_prop = (float)prop.AsArray[1].AsInt / 10;

        return string.Format(desc_arg, temp_prop.ToString() + "%");
        
    }
}

/// <summary>
/// 计算装备属性的价值（固定值）
/// </summary>
public class SCRIPT_5008 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray prop = _params[0] as LPCArray;
        int star = (int)_params[1];
        int equipType = (int)_params[2];
        int propType = (int)_params[3];
        LPCMapping args = (_params[4] as LPCValue).AsMapping;

        int propValue = prop[1].AsInt;

        if (propType == EquipConst.MAIN_PROP)
            propValue = FetchPropMgr.CalcPropValue(prop[0].AsInt, equipType, star, propType, 15);

        int fightValue = args.GetValue<int>("fight_value");

        int starValue = args.GetValue<LPCMapping>("star_value").GetValue<int>(star);

        // 多除以10是战力价值的小数换算
        return (int)(propValue * fightValue * starValue / 10);
    }
}

/// <summary>
/// 计算装备属性的战力（固定值）
/// </summary>
public class SCRIPT_5009 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray prop = _params[0] as LPCArray;

        int args = (int)_params[2];

        return args * prop[1].AsInt;
    }
}

/// <summary>
/// 计算装备属性的价值（百分比）
/// </summary>
public class SCRIPT_5010 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray prop = _params[0] as LPCArray;
        int star = (int)_params[1];
        int equipType = (int)_params[2];
        int propType = (int)_params[3];
        LPCMapping args = (_params[4] as LPCValue).AsMapping;

        int propValue = prop[1].AsInt;

        if (propType == EquipConst.MAIN_PROP)
            propValue = FetchPropMgr.CalcPropValue(prop[0].AsInt, equipType, star, propType, 15);

        int fightValue = args.GetValue<int>("fight_value");

        int starValue = args.GetValue<LPCMapping>("star_value").GetValue<int>(star);

        // 多除以10是战力价值的小数换算
        return (int)(propValue * fightValue * starValue / 100);
    }
}

/// <summary>
/// 计算装备属性的战力（百分比）
/// </summary>
public class SCRIPT_5011 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray prop = _params[0] as LPCArray;

        int args = (int)_params[2];

        return (int)(args * prop[1].AsInt / 1000);
    }
}

/// <summary>
/// // 计算装备出售价格
/// </summary>
public class SCRIPT_5012 : Script
{
    public override object Call(params object[] _params)
    {
        Property ob = _params[0] as Property;

        return ob.Query<LPCMapping>("price");
    }
}

/// <summary>
/// // 计算装备购买价格
/// </summary>
public class SCRIPT_5013 : Script
{
    public override object Call(params object[] _params)
    {
        Property ob = _params[0] as Property;
        // 获取装备属性
        LPCMapping equipProp = ob.Query<LPCMapping>("prop");
        int fixValue = 0;
        int rarity = ob.GetRarity();

        foreach (int propType in equipProp.Keys)
        {
            // 如果是主属性不处理
            if (propType == EquipConst.MAIN_PROP)
                continue;

            // 遍历类型属性
            foreach (LPCValue prop in equipProp.GetValue<LPCArray>(propType).Values)
                fixValue += PropMgr.GetPropFixValue(prop.AsArray[0].AsInt, rarity);
        }

        LPCMapping buyPrice = new LPCMapping();
        buyPrice.Add("money", 17 * ob.Query<LPCMapping>("price").GetValue<int>("money") * (1000 + fixValue) / 1000);

        return buyPrice;
    }
}

/// <summary>
/// 怪物的短描述脚本
/// </summary>
public class SCRIPT_5030 : Script
{
    public override object Call(params object[] _params)
    {
        Property ob = _params[0] as Property;

        // 如果是已经觉醒的怪物则获取觉醒mine
        if (MonsterMgr.IsAwaken(ob))
            return LocalizationMgr.Get(ob.Query<string>("awake_name"));

        // 返回描述信息
        return LocalizationMgr.Get(ob.Query<string>("name"));
    }
}

/// <summary>
/// 以下为装备prop属性数值初始化计算脚本
/// </summary>

/// <summary>
/// 计算常数类属性抽取的参考值(暴击伤害等)
/// </summary>
public class SCRIPT_5103 : Script
{
    public override object Call(params object[] _params)
    {
        return 1000;
    }
}

/// <summary>
/// 计算技能变色附加属性值
/// </summary>
public class SCRIPT_5127 : Script
{
    public override object Call(params object[] _params)
    {
        return new LPCArray();
    }
}

/// <summary>
/// 以下为装备prop属性字段记录到玩家身上的处理脚本
/// </summary>

/// <summary>
/// // 计算装备宝石属性的价值
/// </summary>
public class SCRIPT_5515 : Script
{
    public override object Call(params object[] _params)
    {
        LPCArray prop = _params[0] as LPCArray;
        int level = (int)_params[1];
        //float arg = (_params [2] as LPCValue).AsFloat;
        LPCArray prop_arr = prop[1].AsArray;

        // 此类属性只能间接计算，以同级满值的1条属性为参考
        float max_value_single = 0.3f * CALC_STD_POWER.Call(level);

        return max_value_single * prop_arr[1].AsInt;
    }
}

/// <summary>
/// 附加属性值的描述脚本（不带百分比的数值，整型或者浮点型）
/// </summary>
public class SCRIPT_5516 : Script
{
    public override object Call(params object[] _params)
    {
        string desc_arg = _params[0] as String;
        LPCValue prop = _params[1] as LPCValue;

        float temp_prop = prop.AsArray[1].AsFloat;

        return string.Format(desc_arg, (temp_prop > 0 ? "+" : "-"), Mathf.Abs(temp_prop));
    }
}

/// <summary>
/// 计算装备强化消耗
/// </summary>
public class SCRIPT_5520 : Script
{
    public override object Call(params object[] _params)
    {
        // TODO: 计算装备强化消耗

        LPCMapping para = _params[0] as LPCMapping;

        // 装备强化的目标等级
        int rank = para.GetValue<int>("rank");

        // 装备的星级
        int star = para.GetValue<int>("star");

        // 获取配置表数据
        CsvRow row = BlacksmithMgr.GetIntensifyData(rank);

        // 根据星级获取消耗参数
        int args = row.Query<int>(star.ToString());

        // TODO:根据参数计算消耗

        LPCMapping costMap = new LPCMapping();
        costMap.Add("money", args);

        return costMap;
    }
}
