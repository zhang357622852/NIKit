/// <summary>
/// EquipMgr.cs
/// Copy from zhangyg 2014-10-22
/// 装备管理器
/// </summary>

using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using LPC;

/// 装备管理器
public class EquipMgr
{
    #region 变量

    private static int equipmentId = 0;
    private static CsvRow equipmentRow = null;

    // 装备配置表
    private static CsvFile mEquipCsv;

    // 装备可以装备的位置描述表
    private static CsvFile mEquipBindCsv;

    // 套装置表
    private static CsvFile mSuitTemplateCsv;

    #endregion

    #region 属性

    // 获取配置表信息
    public static CsvFile EquipCsv { get { return mEquipCsv; } }

    // 获取装备可以装备的位置描述表
    public static CsvFile EquipBindCsv { get { return mEquipBindCsv; } }

    // 套装置表
    public static CsvFile SuitTemplateCsv { get { return mSuitTemplateCsv; } }

    #endregion

    #region 内部接口

    static EquipMgr()
    {
        // 注册刷新角色属性的回调函数
        PropMgr.RegisterRefreshAffectHook("equip", RefreshEquipAffect);
    }

    /// <summary>
    /// 收集某装备的所有附加属性
    /// </summary>
    private static LPCArray GatherEquipProps(Property equip)
    {
        // 获取装备的附加属性属性
        LPCMapping props = equip.Query<LPCMapping>("prop");
        if (props == null || props.Count == 0)
            return LPCArray.Empty;

        // 全部属性
        LPCArray allProps = LPCArray.Empty;

        // 合并各个属性
        foreach (LPCValue propList in props.Values)
            allProps.Append(propList.AsArray);

        // 返回属性
        return allProps;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 载入装备配置表
        mEquipCsv = CsvFileMgr.Load("equipment");

        // 载入装备的位置配置表
        mEquipBindCsv = CsvFileMgr.Load("equip_bind");

        // 套装配置表
        mSuitTemplateCsv = CsvFileMgr.Load("suit");
    }

    /// <summary>
    /// 获取套装模板
    /// </summary>
    /// <returns>The suit template.</returns>
    /// <param name="suitId">Suit identifier.</param>
    public static CsvRow GetSuitTemplate(int suitId)
    {
        return SuitTemplateCsv.FindByKey(suitId);
    }

    /// <summary>
    /// 获取装备的价值
    /// </summary>
    public static int GetEquipValue(Property equipOb)
    {
        // 获取装备属性
        LPCMapping equipProp = equipOb.Query<LPCMapping>("prop");
        int value = 0;
        int star = equipOb.Query<int>("star");
        int equipType = equipOb.Query<int>("equip_type");

        // 遍历各种类型的
        foreach (int propType in equipProp.Keys)
        {
            // 遍历该类型属性
            foreach(LPCValue prop in equipProp[propType].AsArray.Values)
                value += PropMgr.GetPropValue(prop.AsArray, star, equipType, propType);
        }

        // 返回该装备价值
        return value;
    }

    /// <summary>
    /// 判断此位置是否是装备位置
    /// </summary>
    public static bool IsEquippedPos(Property petOb, string pos)
    {
        // 获取该位置的装备
        Property equipOb = (petOb as Container).baggage.GetCarryByPos(pos);

        if (equipOb != null)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 获得装备位置
    /// </summary>
    /// <returns>The weapon position by base type.</returns>
    /// <param name="equip_type">Equip_type.</param>
    public static string GetEquipPos(int equipType)
    {
        // 取本类装备可以装备的位置信息
        CsvRow data = mEquipBindCsv.FindByKey(equipType);

        // 这类装备没有装备信息，不能装备到任何位置
        if (data == null)
            return string.Empty;

        // 去除位置信息
        LPCValue pos = data.Query<LPCValue>("pos");

        // 配置单个位置信息
        if (pos.IsString)
            return pos.AsString;

        // 需要动态计算
        if (pos.IsInt)
            return (string)ScriptMgr.Call(pos.AsInt, equipType);

        // 其他格式暂不支持
        return string.Empty;
    }

    /// <summary>
    /// 取得装备的装备位置
    /// </summary>
    public static string GetEquipPos(Property equip)
    {
        // 取装备的类别
        LPCValue equipType = equip.Query("equip_type");

        // 不是装备类型道具不能装备
        if (equipType == null)
            return string.Empty;

        // 根据取equipType得装备的装备位置
        return GetEquipPos(equipType.AsInt);
    }

    /// <summary>
    /// 装备品质别名
    /// </summary>
    public static string GetRarityAlias(int rarity)
    {
        switch (rarity)
        {
            case EquipConst.RARITY_WHITE:
                return LocalizationMgr.Get("EquipRarityAlias_0");

            case EquipConst.RARITY_GREEN:
                return LocalizationMgr.Get("EquipRarityAlias_1");

            case EquipConst.RARITY_BLUE:
                return LocalizationMgr.Get("EquipRarityAlias_2");

            case EquipConst.RARITY_PURPLE:
                return LocalizationMgr.Get("EquipRarityAlias_3");

            case EquipConst.RARITY_DARKGOLDENROD:
                return LocalizationMgr.Get("EquipRarityAlias_4");

            default:
                return string.Empty;
        }
    }

    /// <summary>
    /// 根据equip_type取得部位名字
    /// </summary>
    public static string GetNameByEquipType(int equipType)
    {
        // 取本类装备可以装备的位置信息
        CsvRow data = mEquipBindCsv.FindByKey(equipType);

        // 这类装备没有装备信息，不能装备到任何位置
        if (data == null)
            return string.Empty;

        return LocalizationMgr.Get(data.Query<string>("name"));
    }

    // 判断装备是否能够装备到指定位置
    public static bool CanBeEquipped(Property who, Property equip)
    {
        // 对象必须是包裹容器
        if (!(who is Container))
            return false;

        // 不是装备不允许Equip
        if (!(equip is Equipment))
            return false;

        // 取本类装备可以装备的位置信息
        string pos = GetEquipPos(equip);

        // 这类装备没有装备信息，不能装备到任何位置
        if (string.IsNullOrEmpty(pos))
            return false;

        // 获取包裹对象
        BaggageContainer baggage = (who as Container).baggage;

        // 判断是否一个有效位置
        if (!baggage.ValidPosFor(equip, pos, true))
            return false;

        // 已经在装备页面上的道具不允许重复装备
        if (IsEquippedPos(who, equip.move.GetPos()))
            return false;

        // 返回true
        return true;
    }

    /// <summary>
    /// 判断装备是否能够卸载到指定位置
    /// </summary>
    public static bool CanBeUnequipped(Property who, Property equip)
    {
        // 对象必须是包裹容器
        if (!(who is Container))
            return false;

        // 获取包裹对象
        Container container = who as Container;

        // 获取玩家包裹空位
        string pos = container.baggage.GetFreePos(ContainerConfig.POS_ITEM_GROUP);
        if (string.IsNullOrEmpty(pos))
        {
            DialogMgr.Notify(LocalizationMgr.Get("BaggageMgr_2"));

            return false;
        }

        // 判断是否一个有效位置
        if (!container.baggage.ValidPosFor(equip, pos, true))
            return false;

        // 只有在装备页面上的道具才允许卸载
        if (IsEquippedPos(who, equip.move.GetPos()))
            return false;

        // 返回true
        return true;
    }

    /// <summary>
    /// 装备道具
    /// </summary>
    public static bool Equip(Property who, Property equip)
    {
        // 检查能否装备
        if (!CanBeEquipped(who, equip))
            return false;

        // 通知服务器装备道具
        Operation.CmdEquip.Go(who.GetRid(), equip.GetRid());
        return true;
    }

    /// <summary>
    /// 解除装备，放到指定位置
    /// </summary>
    public static bool UnEquip(Property who, Property equip)
    {
        // 对象必须是包裹容器
        if (!(who is Container))
            return false;

        // 卸载装备目标位置不能有东西
        if (!CanBeUnequipped(who, equip))
            return false;

        // 通知服务器卸载装备
        Operation.CmdUnEquip.Go(who.GetRid(), equip.GetRid());
        return true;
    }

    /// <summary>
    /// 刷新装备带来的附加属性
    /// </summary>
    /// <param name="who">Who.</param>
    public static void RefreshEquipAffect(Property who)
    {
        // 不是怪物不需要刷新属性
        if (!who.IsMonster())
            return;

        // 获取装备道具
        Container container = who as Container;
        Dictionary<string, Property> pageList = container.baggage.GetPageCarry(ContainerConfig.POS_EQUIP_GROUP);
        Dictionary<int, int> suitMap = new Dictionary<int, int>();
        LPCArray equipProps = new LPCArray();
        int suitId = 0;

        // 巡视所有位置的装备
        foreach (KeyValuePair<string, Property> item in pageList)
        {
            // 取装备所有附加属性并应用到目标身上
            equipProps.Append(GatherEquipProps(item.Value));

            // 收集套装属性
            suitId = item.Value.Query<int>("suit_id");
            if (suitId == 0)
                continue;

            // 记录套装数据
            if (suitMap.ContainsKey(suitId))
                suitMap[suitId] += 1;
            else
                suitMap[suitId] = 1;
        }

        // 没有套装
        if (suitMap.Count != 0)
        {
            LPCArray props;
            int amount = 0;
            LPCArray suitProps = new LPCArray();

            // 收集激活的套装属性
            foreach (int key in suitMap.Keys)
            {
                // 获取该套装属性配置信息
                CsvRow data = SuitTemplateCsv.FindByKey(key);
                if (data == null)
                    continue;

                // 计算激活套数, 如果没有套装直接返回
                amount = suitMap[key] / data.Query<int>("sub_count");
                if (amount <= 0)
                    continue;

                // 获取套装属性
                props = data.Query<LPCArray>("props");

                // 每激活一套增加一套属性
                for (int i = 0; i < amount; i++)
                    suitProps.Append(props);
            }

            // 记录到角色装备属性上
            who.SetTemp("equip_props", LPCValue.Create(suitProps));

            // 汇总到equipProps列表中
            equipProps.Append(suitProps);
        }

        // 刷新玩家的套装属性
        PropMgr.CalcAllProps(who, equipProps, "equip");
    }

    /// <summary>
    /// 获取角色激活套装信息
    /// </summary>
    public static List<int> GetActivitySuit(Property who)
    {
        // 不是玩家或者怪物不需要刷新属性
        if (!who.IsUser() || !who.IsMonster())
            return new List<int>();

        // 获取装备道具
        Container container = who as Container;
        Dictionary<string, Property> pageList = container.baggage.GetPageCarry(ContainerConfig.POS_EQUIP_GROUP);
        LPCMapping suitMap = LPCMapping.Empty;
        int suitId = 0;

        // 巡视所有位置的装备
        foreach (Property item in pageList.Values)
        {
            // 收集套装属性
            suitId = item.Query<int>("suit_id");
            if (suitId == 0)
                continue;

            // 记录套装数据
            suitMap.Add(suitId, suitMap.GetValue<int>(suitId) + 1);
        }

        // 没有套装
        if (suitMap.Count == 0)
            return new List<int>();

        // 收集激活的套装属性
        List<int> suitList = new List<int>();
        foreach (int key in suitMap.Keys)
        {
            // 获取该套装属性配置信息
            CsvRow data = SuitTemplateCsv.FindByKey(key);
            if (data == null)
                continue;

            // 计算激活套数
            int amount = suitMap[key].AsInt / data.Query<int>("sub_count");

            // 每激活一套增加一套
            for (int i = 0; i < amount; i++)
                suitList.Add(key);
        }

        // 返回激活套装列表
        return suitList;
    }

    /// <summary>
    /// 收集某装备的所有附加属性
    /// </summary>
    public static Dictionary<int, LPCArray> GetEquipProps(Property equip)
    {
        // 获取装备的附加属性
        LPCMapping props = equip.Query<LPCMapping>("prop");
        if (props == null || props.Count == 0)
            return new Dictionary<int, LPCArray>();

        Dictionary<int, LPCArray> allProps = new Dictionary<int, LPCArray>();

        // 合并各个属性
        foreach (int type in props.Keys)
            allProps.Add(type, props[type].AsArray);

        // 获取该套装属性配置信息
        CsvRow data = SuitTemplateCsv.FindByKey(equip.Query<int>("suit_id"));
        if (data == null)
            return allProps;

        // 每激活一套增加一套属性
        allProps.Add(EquipConst.SUIT_PROP, data.Query<LPCArray>("props"));

        // 返回属性
        return allProps;
    }

    /// <summary>
    /// 是否是装备
    /// </summary>
    public static bool IsEquipment(Property ob)
    {
        return IsEquipment(ob.Query<int>("class_id"));
    }

    /// <summary>
    /// 是否是装备
    /// </summary>
    public static bool IsEquipment(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row == null)
            return false;

        return true;
    }

    //获得名称
    public static string GetName(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return LocalizationMgr.Get(row.Query<string>("name"));
        return null;
    }

    /// <summary>
    /// 获取装备的短描述
    /// </summary>
    public static string Short(LPCMapping data)
    {
        if (data == null || data.Count == 0)
            return string.Empty;

        int classId = data.GetValue<int>("class_id");

        // 装备的配置表数据
        CsvRow row = GetRow(classId);

        if (row == null)
            return string.Empty;

        // 获取短描述脚本
        int scriptNo = row.Query<int>("short_desc_script");

        // 没有描述脚本
        if (scriptNo == 0)
            return LocalizationMgr.Get(row.Query<string>("name"));

        string text = string.Empty;

        //获取装备的属性;
        LPCMapping propMap = data.GetValue<LPCMapping>("prop");

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
        CsvRow suitMap = EquipMgr.SuitTemplateCsv.FindByKey(row.Query<int>("suit_id"));
        if (suitMap != null)
        {
            text = string.Format("{0}{1}", text, LocalizationMgr.Get(suitMap.Query<string>("name")));
        }

        // 装备自身名称
        text = string.Format("{0}{1}", text, LocalizationMgr.Get(row.Query<string>("name")));

        // 获取装备的稀有度
        int rarity = data.GetValue<int>("rarity");
        if (rarity != EquipConst.RARITY_WHITE)
            text = string.Format("{0}-{1}", text, EquipMgr.GetRarityAlias(rarity));

        // 返回描述信息
        return text;
    }

    //获得icon
    public static string GetIcon(int classId, int rarity)
    {
        CsvRow row = GetRow(classId);

        if (row != null)
            return string.Format("{0}", row.Query<int>("icon") + rarity);

        return null;
    }

    /// <summary>
    /// 获取套装id
    /// </summary>
    public static int GetSuitId(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row == null)
            return -1;

        return row.Query<int>("suit_id");
    }

    /// <summary>
    /// 获取装备的texture
    /// </summary>
    /// <returns>The texture.</returns>
    /// <param name="classId">Class identifier.</param>
    public static Texture2D GetTexture(int classId, int rarity)
    {
        string textureName = GetIcon(classId, rarity);

        if (string.IsNullOrEmpty(textureName))
            return null;

        return EquipMgr.LoadTexture(textureName);
    }

    /// <summary>
    /// 加载装备texture.
    /// </summary>
    /// <returns>The texture.</returns>
    /// <param name="textureName">Texture name.</param>
    public static Texture2D LoadTexture(string textureName)
    {
        return ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/equipment/{0}.png", textureName));
    }

    /// <summary>
    /// 获取套装图标的texture
    /// </summary>
    /// <returns>The texture.</returns>
    /// <param name="classId">Class identifier.</param>
    public static Texture2D GetSuitTexture(int suitId)
    {
        CsvRow data = EquipMgr.SuitTemplateCsv.FindByKey(suitId);

        if (data == null)
            return null;

        string textureName = data.Query<string>("icon");

        if (string.IsNullOrEmpty(textureName))
            return null;

        return EquipMgr.LoadTexture(textureName);
    }

    /// <summary>
    /// 获取套装名称
    /// </summary>
    /// <param name="suitId"></param>
    /// <returns></returns>
    public static string GetSuitName(int suitId)
    {
        CsvRow data = EquipMgr.SuitTemplateCsv.FindByKey(suitId);

        if (data == null)
            return string.Empty;

        string name = data.Query<string>("name");

        if (string.IsNullOrEmpty(name))
            return string.Empty;

        return LocalizationMgr.Get(name);
    }

    /// <summary>
    /// 获取套装属性描述
    /// </summary>
    /// <returns>The suit property desc.</returns>
    /// <param name="suitId">Suit identifier.</param>
    public static string GetSuitPropDesc(int suitId)
    {
        CsvRow data = EquipMgr.SuitTemplateCsv.FindByKey(suitId);

        string propDesc = string.Empty;

        if (data == null)
            return propDesc;

        LPCArray prop = data.Query<LPCArray>("props");

        if (prop == null)
            return propDesc;

        // 遍历各个属性
        for (int i = 0; i < prop.Count; i++)
            propDesc += PropMgr.GetPropDesc(prop[i].AsArray, EquipConst.SUIT_PROP);

        return propDesc;
    }

    /// <summary>
    /// 获得类型(equip_type)
    /// </summary>
    public static int GetEquipType(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return row.Query<int>("equip_type");

        return -1;
    }

    //获得附加属性
    public static LPCValue GetProp(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return row.Query<LPCValue>("prop");
        return null;
    }

    /// <summary>
    /// 获取装备稀有度颜色值
    /// </summary>
    /// <param name="rarity"></param>
    /// <returns></returns>
    public static string GetEquipRarityColorByRarity(int rarity)
    {
        string color;

        if (ColorConfig.mEquipColor.TryGetValue(rarity, out color))
            return color;

        return string.Empty;
    }

    //获得描述
    public static string GetDesc(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return LocalizationMgr.Get(row.Query<string>("desc"));
        return null;
    }

    //获得属性描述
    public static string GetPropDesc(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return LocalizationMgr.Get(row.Query<string>("prop_desc"));
        return null;
    }

    /// <summary>
    /// 装备的短描述
    /// 5星强攻套装（4）件-稀有
    /// </summary>
    public static string GetShortDesc(int suitId, int rarity, int star)
    {
        // 套装配置表信息
        CsvRow suitRow = mSuitTemplateCsv.FindByKey(suitId);

        if (suitRow == null)
            return string.Empty;

        // 套装名称
        string suitName = LocalizationMgr.Get(suitRow.Query<string>("name"));

        // 套装组件数量
        int subCount = suitRow.Query<int>("sub_count");

        // 获取装备的稀有度别名
        string alias = GetRarityAlias(rarity);

        // 根据装备稀有度获取装备的颜色标签
        string colorLable = ColorConfig.GetColor(rarity);

        string desc = string.Format(LocalizationMgr.Get("BelongingGoodsInfoWnd_1"), "[" + colorLable + "]", star, suitName, subCount);

        if (string.IsNullOrEmpty(alias))
            return desc;
        else
            return desc + "-" + alias;
    }

    //获得数据列
    public static CsvRow GetRow(int classId)
    {
        if (equipmentId != classId)
        {
            equipmentId = classId;
            equipmentRow = EquipCsv.FindByKey(classId);
        }
        return equipmentRow;
    }

    /// <summary>
    /// 获取equip_type类型装备数量
    /// </summary>
    public static int GetEquipAmountByEquipType(Property who, int equipType)
    {
        // 获取装备道具
        Container container = who as Container;
        Dictionary<string, Property> pageList = container.baggage.GetPageCarry(ContainerConfig.POS_EQUIP_GROUP);
        int amount = 0;

        // 巡视所有位置的装备
        foreach (KeyValuePair<string, Property> item in pageList)
        {
            // 不是需要收集equip_type
            if (item.Value.Query<int>("equip_type") != equipType)
                continue;

            // 记录数据
            amount = amount + 1;
        }

        // 返回equip_type类型装备数量
        return amount;
    }

    /// <summary>
    /// 获取装备类型名称
    /// </summary>
    /// <param name="equipType"></param>
    /// <returns></returns>
    public static string GetEquipTypeNameByEquipType(int equipType)
    {
        string name;

        if (EquipConst.EquipTypeToName.TryGetValue(equipType, out name))
            return name;

        return string.Empty;
    }

    /// <summary>
    /// 根据套装id和装备类型获取class_id
    /// </summary>
    public static int GetClassId(int suitId, int equipType)
    {
        foreach (CsvRow row in EquipCsv.rows)
        {
            if (row == null)
                continue;

            if (!row.Query<int>("suit_id").Equals(suitId))
                continue;

            if (!row.Query<int>("equip_type").Equals(equipType))
                continue;

            return row.Query<int>("class_id");
        }

        return 0;
    }

    /// <summary>
    /// 是否有新的装备
    /// </summary>
    public static bool IsNewEquip(Property user)
    {
        if (user == null)
            return false;

        List<Property> equips = BaggageMgr.GetItemsByPage(user, ContainerConfig.POS_ITEM_GROUP);

        for (int j = 0; j < equips.Count; j++)
        {
            if (BaggageMgr.IsNew(equips[j]))
                return true;
        }

        return false;
    }


    /// <summary>
    /// 获取零时装备Property,
    /// 【警告】不使用时，必须销毁
    /// </summary>
    /// <param name="equipInfo"></param>
    /// <returns></returns>
    public static Property GetTempEquipProperty(LPCMapping equipInfo)
    {
        if (equipInfo == null)
            return null;

        int classId = equipInfo.GetValue<int>("class_id");

        if (!IsEquipment(classId))
            return null;

        // 构造参数
        LPCMapping dbase = LPCMapping.Empty;

        dbase.Append(equipInfo);
        dbase.Add("rid", Rid.New());

        return PropertyMgr.CreateProperty(dbase);
    }

    #endregion
}
