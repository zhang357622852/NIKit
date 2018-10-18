/// <summary>
/// SCRIPT_4000.cs
/// Create by zhaozy 2014-11-6
/// 怪物脚本
/// </summary>

using System;
using System.Collections.Generic;
using LPC;

// 怪物初始化脚本
public class SCRIPT_400 : Script
{
    public override object Call(params object[] _params)
    {
        return true;
    }
}

///<summary>
/// 宠物升级材料经验脚本
///</summary>
public class SCRIPT_4100 : Script
{
    public override object Call(params object[] _param)
    {
        // 此处仅仅是将该宠物提供的经验值返回，没有考虑其他考虑，比如元素等等。
        // 参数可以直接是int值，map，array。

        // 第一个参数为要加强的宠物，第二个为材料，第三个为参数（lpcvalue类型）
        Property matOb = _param[1] as Property;
        int arg = (_param[2] as LPCValue).AsInt;

        int exp = (int)(1.1f * Math.Pow(2.2f, matOb.Query<int>("star") - 1) *
                  (arg + 24 * (matOb.Query<int>("level") - 1)));

        return exp;
    }
}

///<summary>
/// 获取经验波利所提供的经验
///</summary>
public class SCRIPT_4101 : Script
{
    public override object Call(params object[] _param)
    {
        // 此处仅仅是将该宠物提供的经验值返回，没有考虑其他考虑，比如元素等等。
        // 参数可以直接是int值，map，array。

        // 第一个参数为要加强的宠物，第二个为材料，第三个为参数（lpcvalue类型）
        // 材料等级
        Property petlOb = _param[0] as Property;
        Property materialOb = _param[1] as Property;
        int arg = (_param[2] as LPCValue).AsInt;

        int level = materialOb.Query<int>("level");

        // 计算提供经验
        int exp = (int)( 11 * (level + 1) * (arg + 24 * (level - 1)) / 10 );

        // 检查是否觉醒
        int rank = materialOb.Query<int>("rank");
        int materialElement = materialOb.Query<int>("element");
        if (rank == 2)
        {
            // 检查是否相同属性
            if (petlOb.Query<int>("element") == materialElement || materialElement == MonsterConst.ELEMENT_NONE)
            {
                int extra_rate_e = materialOb.Query<int>("upgrade_element");
                exp += (int)( extra_rate_e * exp / 1000 );
            }

            int extra_rate_w = materialOb.Query<int>("upgrade_wake");
            exp += (int)( extra_rate_w * exp / 1000 );
        }

        return exp;
    }
}

///<summary>
/// 获取高阶经验波利（经验怪）所提供的经验
///</summary>
public class SCRIPT_4102 : Script
{
    public override object Call(params object[] _param)
    {
        // 此处仅仅是将该宠物提供的经验值返回，没有考虑其他考虑，比如元素等等。
        // 参数可以直接是int值，map，array。

        // 第一个参数为要加强的宠物，第二个为材料，第三个为参数（lpcvalue类型）
        // 材料等级
        Property petlOb = _param[0] as Property;
        Property materialOb = _param[1] as Property;
        int arg = (_param[2] as LPCValue).AsInt;

        int level = materialOb.Query<int>("level");

        // 计算提供经验
        int exp = (int)( 11 * (level + 1) * (arg + 48 * (level - 1)) / 10 );

        // 检查是否觉醒
        int rank = materialOb.Query<int>("rank");
        // 材料的元素属性
        int materialElement = materialOb.Query<int>("element");
        if (rank == 2)
        {
            // 检查是否相同属性
            if (petlOb.Query<int>("element") == materialElement || materialElement == MonsterConst.ELEMENT_NONE)
            {
                int extra_rate_e = materialOb.Query<int>("upgrade_element");
                exp += (int)( extra_rate_e * exp / 1000 );
            }

            int extra_rate_w = materialOb.Query<int>("upgrade_wake");
            exp += (int)( extra_rate_w * exp / 1000 );
        }

        return exp;
    }
}

///<summary>
/// 升星检测脚本(测试)
///</summary>
public class SCRIPT_4120 : Script
{
    public override object Call(params object[] _param)
    {
        // 能够返回"",不能返回原因
        //(如材料消耗(需要几个几星的)，金钱消耗等都已做检测，此处仅需检测额外条件)
        //Property pet_ob = _param[0] as Property;
        //List<Property> material_list = _param[1] as List<Property>;

        return "";
    }
}

///<summary>
/// 升星计算材料脚本(测试)
///</summary>
public class SCRIPT_4126 : Script
{
    public override object Call(params object[] _param)
    {

        // 第一个参数为pet_ob,第二个为args
        // 参数配置说明：如([2:2])表示2星宠物2个。
        //当然可以这样配置([1:1,2:3])表示该星级宠物觉醒需要1个1星，2个3星
        //Property pet_ob = _param[0] as Property;
        LPCMapping arg = _param[1] as LPCMapping;

        return arg;
    }
}


///<summary>
/// 升星消耗脚本(测试)
///</summary>
public class SCRIPT_4230 : Script
{
    public override object Call(params object[] _param)
    {
        // 第一个参数为pet_ob,第二个为material_list,第三个为参数
        LPCMapping arg = (_param[2] as LPCMapping);

        return arg;
    }
}

///<summary>
/// 觉醒描述脚本(测试)
///</summary>
public class SCRIPT_4240 : Script
{
    public override object Call(params object[] _param)
    {
        //Property monster = (_param[0] as Property);
        LPCMapping arg = (_param[1] as LPCValue).AsMapping;

        string desc = string.Empty;

        if (arg.Count == 0)
            return desc;

        // 描述新增技能
        if (arg.ContainsKey("new_skill"))
            desc += string.Format("◆ <split>imgLink={0},25,25,{1}</split>   {2}。<br>",
                SkillMgr.GetIconResPath(arg.GetValue<int>("new_skill")),
                arg.GetValue<int>("new_skill"),
                MonsterConst.awakeDescMap[MonsterConst.NEW_SKILL]);

        // 描述技能强化
        if (arg.ContainsKey("pre_skill"))
        {
            desc += string.Format("◆  <split>imgLink={0},25,25,{1}</split>  > <split>imgLink={2},25,25,{3}</split>   {4}<br>",
                SkillMgr.GetIconResPath(arg.GetValue<int>("pre_skill")), arg.GetValue<int>("pre_skill"),
                SkillMgr.GetIconResPath(arg.GetValue<int>("next_skill")), arg.GetValue<int>("next_skill"), MonsterConst.awakeDescMap[MonsterConst.UPGRADE_SKILL]);
        }

        // 描述属性强化
        if (arg.ContainsKey("new_attrib"))
        {
            LPCArray descArg = arg.GetValue<LPCArray>("new_attrib");
            desc += string.Format("◆  {0} {1}。<br>",
                MonsterConst.awakeDescMap[descArg[0].AsInt], descArg[1].AsString);
        }

        desc += string.Format("◆  {0}", LocalizationMgr.Get("base_up_desc"));

        return desc;
    }
}

///<summary>
/// 觉醒描述脚本(测试)
///</summary>
public class SCRIPT_4241 : Script
{
    public override object Call(params object[] _param)
    {
        //Property monster = (_param[0] as Property);
        LPCArray arg = (_param[1] as LPCValue).AsArray;

        int skill = arg[0].AsInt;

        int awakedSkill = arg[1].AsInt;

        string desc = string.Empty;

        // 可以指定字体格式大小，格式为eg:<font=22> </font>,如果不指定大小，以插件窗口label控件字体大小为准,可在窗口进行调节

        desc += string.Format("<br>◆  {0}", "基础属性值获得大幅提升。");

        desc += string.Format("<br>◆  <split>imgLink={0},25,25,{1}</split>  > <split>imgLink={2},25,25,{3}</split>   {4}",
            SkillMgr.GetIconResPath(skill), skill, SkillMgr.GetIconResPath(awakedSkill), awakedSkill, "技能强化");

        desc += string.Format("<br>◆  {0}", "啊哈哈哈哈哈哈哈");

        return desc;
    }
}


///<summary>
/// 使魔升级吞噬道具获得经验
///</summary>
public class SCRIPT_4400 : Script
{
    public override object Call(params object[] _param)
    {
        int arg = (int)_param[0];

        //模拟的是1星1级使魔提供的经验
        return (int)(1.1 * Math.Pow(2.2, 1 - 1) * (arg + 24 * (1 - 1)));
    }
}

///<summary>
/// 使魔升级吞噬道具消耗金币
///</summary>
public class SCRIPT_4500 : Script
{
    public override object Call(params object[] _param)
    {
        int costNum = (int)_param[0];
        int price = (int)_param[1];

        return costNum * price;
    }
}






