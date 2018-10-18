/// <summary>
/// PropertyMgr.cs
/// Copy from zhangyg 2014-10-22
/// 物件管理
/// </summary>

using System;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 物件管理
/// </summary>
public class PropertyMgr
{
    #region

    // class_id对象的道具类型
    private static Dictionary<int, int> mPropertyTypeMap = new Dictionary<int, int>();

    #endregion

    #region 内部借口

    /// <summary>
    /// 创建道具，这是生成道具的唯一途径，不允许从其它途径生成
    /// </summary>
    private static Item CreateItem(LPCMapping propertyInfo, bool doActivate)
    {
        // 构建道具对象
        Item item = new Item(ConvertDbase(propertyInfo));

        // 构建完成
        return item;
    }

    /// <summary>
    /// 创建装备，这是生成道具的唯一途径，不允许从其它途径生成
    /// </summary>
    private static Equipment CreateEquipment(LPCMapping propertyInfo, bool doActivate)
    {
        // 构建道具对象
        Equipment equip = new Equipment(ConvertDbase(propertyInfo));

        // 初始化物件
        int initScript = equip.Query<int>("init_script");
        if (initScript != 0 && !doActivate)
        {
            // 调用初始化脚本
            ScriptMgr.Call(initScript, equip);
        }

        // 构建完成
        return equip;
    }

    /// <summary>
    /// 创建怪物，这是生成怪物的唯一途径，不允许从其它途径生成
    /// </summary>
    private static Monster CreateMonster(LPCMapping propertyInfo, bool doActivate)
    {
        // 构建怪物对象
        Monster monster = new Monster(ConvertDbase(propertyInfo));

        // 根据初始化规则初始化怪物属性,
        // 如果没有指定初始化规则，则默认是宠物初始化规则
        string ruleId = monster.Query<string>("init_rule");
        if (string.IsNullOrEmpty(ruleId))
            MonsterMgr.InitMonster(monster);
        else
            MonsterMgr.InitMonster(monster, ruleId);

        // 如果是还原对象
        if (doActivate)
            return monster;

        // 初始化脚本初始化物件
        int initScript = monster.Query<int>("init_script");
        if (initScript != 0)
            ScriptMgr.Call(initScript, monster);

        // 构建完成
        return monster;
    }

    /// <summary>
    /// 创建NPC，这是生成NPC的唯一途径，不允许从其它途径生成
    /// </summary>
    private static NPC CreateNPC(LPCMapping propertyInfo, bool doActivate)
    {
        // 构建NPC对象
        NPC npc = new NPC(ConvertDbase(propertyInfo));

        // 构建完成
        return npc;
    }

    /// <summary>
    /// Queries the row.
    /// </summary>
    private static LPCValue QueryRow(CsvRow row, string path)
    {
        // 在行中获取路径path的信息
        object r = null;
        if (row.Contains(path))
        {
            LPCValue v = row.Query<LPCValue>(path);
            if (v.IsInt)
                r = v.AsInt;
            else if (v.IsString)
                r = v.AsString;
            else
                r = LPCSaveString.SaveToString(v);
        }
        else if (row.Contains("dbase"))
        {
            LPCValue dbase = row.Query<LPCValue>("dbase");
            LPCValue v = dbase.AsMapping[path];
            if (v == null)
                return null;
            if (v.IsInt)
                r = v.AsInt;
            else if (v.IsString)
                r = v.AsString;
            else
                r = LPCSaveString.SaveToString(v);
        }
        if (r == null)
            return null;
        if (r is int)
            return LPCValue.Create((int)r);
        if (r is string)
            return LPCValue.Create((string)r);
        return null;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 复制一份对象
    /// </summary>
    public static Property DuplicateProperty(Property ob, LPCMapping extraDbase)
    {
        // 复制base数据
        LPCMapping dbase = LPCValue.Duplicate(ob.QueryEntireDbase()).AsMapping;

        // 替换掉rid
        dbase.Add("rid", Rid.New());

        // 吸收指定的附加信息
        dbase.Append(extraDbase);

        // CreateProperty
        Property copyOb = CreateProperty(dbase, true);

        // clone 对象失败
        if (copyOb == null)
            return null;

        // 不是Container不需要处理
        Container container = ob as Container;
        if (container == null)
            return copyOb;

        // 复制角色的装备信息
        Dictionary<string, Property> pageList = container.baggage.GetPageCarry(ContainerConfig.POS_EQUIP_GROUP);

        // 巡视所有位置的装备
        foreach (KeyValuePair<string, Property> item in pageList)
        {
            // 复制装备道具base数据
            LPCMapping equipDbase = LPCValue.Duplicate(item.Value.QueryEntireDbase()).AsMapping;
            equipDbase.Add("rid", Rid.New());

            // CreateProperty
            Property equipOb = CreateProperty(equipDbase, true);

            // clone 对象失败
            if (equipOb == null)
                continue;

            // 载入道具
            (copyOb as Container).LoadProperty(equipOb, item.Key);
        }

        // 刷新角色属性
        PropMgr.RefreshAffect(copyOb);

        // 返回clone对象
        return copyOb;
    }

    /// <summary>
    /// 创建一个物件 - 根据类型调用相应的构造函数
    /// </summary>
    public static Property CreateProperty(LPCMapping propertyInfo, bool doActivate = false)
    {
        // 必须包含rid
        System.Diagnostics.Debug.Assert(propertyInfo.ContainsKey("rid"));

        // 获取道具的class_id
        int classId = propertyInfo["class_id"].AsInt;

        // 获取CreateProperty类型
        int propertyType = GetPropertyType(classId);
        Property ob = null;

        // 根据不同类型创建不同的实体
        switch (propertyType)
        {
            case ObjectType.OBJECT_TYPE_MONSTER:
                // 怪物宠物
                ob = CreateMonster(propertyInfo, doActivate);
                break;

            case ObjectType.OBJECT_TYPE_ITEM:
                // 一般普通道具
                ob = CreateItem(propertyInfo, doActivate);
                break;

            case ObjectType.OBJECT_TYPE_EQUIP:
                // 装备道具
                ob = CreateEquipment(propertyInfo, doActivate);
                break;

            case ObjectType.OBJECT_TYPE_NPC:
                // 创建NPC
                ob = CreateNPC(propertyInfo, doActivate);
                break;

            default:
                // 不支持类型抛出异常
                throw new Exception(string.Format("无法识别的类型({0})，不能构造物件。", propertyType));
        }

        // 如果道具对象create失败
        // 或者是还原道具，不处理
        if (ob == null || doActivate)
            return ob;

        // 计算道具的标准价格
        ob.Set("price", LPCValue.Create(CALC_STANDARD_PRICE.Call(ob)));

        // 返回clone对象
        return ob;
    }

    /// <summary>
    /// 根据class_id获取对象的type
    /// </summary>
    public static int GetPropertyType(int classId)
    {
        // 如果已经在缓存列表中
        if (mPropertyTypeMap.ContainsKey(classId))
            return mPropertyTypeMap[classId];

        // 一般普通道具
        int propertyType = 0;

        // 收集类型
        if (ItemMgr.GetRow(classId) != null)
            propertyType = ObjectType.OBJECT_TYPE_ITEM;
        else if (EquipMgr.GetRow(classId) != null)
            propertyType = ObjectType.OBJECT_TYPE_EQUIP;
        else if (MonsterMgr.GetRow(classId) != null)
            propertyType = ObjectType.OBJECT_TYPE_MONSTER;
        else
            // 不支持类型抛出异常
            throw new Exception(string.Format("无法识别类型的class_id : ({0})", classId));

        // 添加缓存数据
        mPropertyTypeMap.Add(classId, propertyType);

        // 返回数据
        return propertyType;
    }

    /// <summary>
    /// 查找本初对象上的数据，目前直接从数据库中检索读取
    /// 后续对于一些频繁访问的数据，需要考虑在内存中缓存一份
    /// </summary>
    public static LPCValue BasicQuery(int class_id, string path)
    {
        if (class_id == 0)
            return null;

        CsvRow row = ItemMgr.ItemCsv.FindByKey(class_id);
        if (row != null)
            return QueryRow(row, path);

        row = EquipMgr.EquipCsv.FindByKey(class_id);
        if (row != null)
            return QueryRow(row, path);

        row = MonsterMgr.MonsterCsv.FindByKey(class_id);
        if (row != null)
            return QueryRow(row, path);

        return null;
    }

    /// <summary>
    /// 根据物件类型获取物件完整数据
    /// </summary>
    public static CsvRow QueryBaseByType(int classId, int objectType)
    {
        switch (objectType)
        {
            case ObjectType.OBJECT_TYPE_ITEM:
                return QueryItemBase(classId);
            case ObjectType.OBJECT_TYPE_MONSTER:
                return QueryMonsterBase(classId);
            default:
                return null;
        }
    }

    /// <summary>
    /// 根据怪物id查询怪物信息
    /// </summary>
    /// <param name='class_id'>
    /// 怪物id
    /// </param>
    public static CsvRow QueryMonsterBase(int class_id)
    {
        return MonsterMgr.MonsterCsv.FindByKey(class_id);
    }

    /// <summary>
    /// 根据道具id查询道具
    /// </summary>
    /// <returns>
    public static CsvRow QueryItemBase(int class_id)
    {
        CsvRow row;

        // 普通道具
        row = ItemMgr.ItemCsv.FindByKey(class_id);
        if (row != null)
            return row;

        // 装备
        row = EquipMgr.EquipCsv.FindByKey(class_id);
        if (row != null)
            return row;

        // 返回null
        return null;
    }

    // 将道具数据转换为dbase为key的数据
    public static LPCMapping ConvertDbase(LPCMapping propertyInfo)
    {
        LPCMapping v = LPCMapping.Empty;
        v.Add("dbase", propertyInfo);

        return v;
    }

    /// <summary>
    /// 获取物件的购买价格
    /// </summary>
    public static LPCMapping GetBuyPrice(Property ob, bool isMarket = false)
    {
        LPCValue buyPrice;

        if (isMarket)
        {
            CsvRow row = MarketMgr.MarketCsv.FindByKey(ob.GetClassID());
            if (row == null)
                return LPCMapping.Empty;

            // 获取物件的购买信息
            buyPrice = row.Query<LPCValue>("buy_price");
        }
        else
        {
            // 获取物件的购买信息
            buyPrice = ob.Query<LPCValue>("buy_price");
        }

        // 没有配置购买价格
        if (buyPrice == null)
            return LPCMapping.Empty;

        // 如果直接配置的购买价格
        if (buyPrice.IsMapping)
            return buyPrice.AsMapping;

        // 通过脚本计算获得
        if (buyPrice.IsInt)
            return ScriptMgr.Call(buyPrice.AsInt, ob) as LPCMapping;

        // 没有配置购买价格
        return LPCMapping.Empty;
    }

    /// <summary>
    /// 获取物件的出售价格
    /// </summary>
    public static LPCMapping GetSellPrice(Property ob)
    {
        // 获取物件的出售信息
        LPCValue sellPrice = ob.Query<LPCValue>("sell_price");

        // 该物件没有出售价格
        if (sellPrice == null)
            return LPCMapping.Empty;

        // 如果配置的是([])格式
        if (sellPrice.IsMapping)
            return sellPrice.AsMapping;

        // 通过脚本计算
        if (sellPrice.IsInt)
            return ScriptMgr.Call(sellPrice.AsInt, ob) as LPCMapping;

        // 否则道具没有出售价格
        return LPCMapping.Empty;
    }

    #endregion
}
