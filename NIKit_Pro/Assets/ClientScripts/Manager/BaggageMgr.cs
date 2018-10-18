/// <summary>
/// BaggageMgr.cs
/// Created by fucj 2014-12-03
/// 背包管理
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LPC;

public static class BaggageMgr
{
    /// <summary>
    /// 清除包裹新物品字段
    /// </summary>
    public static void ClearNewField(Property who, int page)
    {
        List<Property> items = GetItemsByPage(who, page);
        if (items.Count < 1)
            return;

        foreach (Property item in items)
            item.DeleteTemp("is_new");

        // 通知有新物品信息被清除
        EventMgr.FireEvent(EventMgrEventType.EVENT_CLEAR_NEW, null);
    }

    /// <summary>
    /// 清除包裹新物品字段
    /// </summary>
    public static void ClearNewField(Property equip)
    {
        if (equip.QueryTemp<int>("is_new") <= 0)
            return;

        equip.DeleteTemp("is_new");

        // 通知有新物品信息被清除
        EventMgr.FireEvent(EventMgrEventType.EVENT_CLEAR_NEW, null);
    }

    /// <summary>
    /// 判断物品是否是新的
    /// </summary>
    public static bool IsNew(Property item_ob)
    {
        return item_ob.QueryTemp<int>("is_new") > 0;
    }

    /// <summary>
    /// 是否有新物品
    /// </summary>
    public static bool HasNewItem(List<Property> list)
    {
        if (list == null)
            return false;

        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == null)
                continue;

            // 有新物品
            if (IsNew(list[i]))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 清除包裹新物品字段
    /// </summary>
    public static void ClearNewField(List<Property> equipList)
    {
        // 如果没有装备道具
        if (equipList.Count == 0)
            return;

        // 遍历各个装备
        foreach (Property equip in equipList)
        {
            // 装备对象不存在
            if (equip == null)
                continue;

            // 删除装备的new标识
            equip.DeleteTemp("is_new");
        }

        // 通知有新物品信息被清除
        EventMgr.FireEvent(EventMgrEventType.EVENT_CLEAR_NEW, null);
    }


    /// <summary>
    /// 对包裹中的装备排序 
    /// </summary>
    /// <returns>The items in bag.</returns>
    /// <param name="itemlist">Itemlist.</param>
    public static List<Property> SortEquipsInBag(List<Property> itemlist)
    {
        // 需要排序的装备列表为空
        if (itemlist == null)
            return new List<Property>();

        List<Property> sortItems = new List<Property>();

        foreach (Property item in SortEquips(itemlist))
            sortItems.Add(item);

        // 按照权重进行排序
        return sortItems;
    }

    /// <summary>
    /// 对包裹中的装备排序 
    /// </summary>
    /// <returns>The items in bag.</returns>
    /// <param name="itemlist">Itemlist.</param>
    public static List<Property> SortEquipsAttribInBag(List<Property> itemlist, LPCArray minorCondition)
    {
        List<Property> sortItems = new List<Property>();

        foreach (Property item in SortEquipsAttrib(itemlist, minorCondition))
            sortItems.Add(item);

        // 按照权重进行排序
        return sortItems;
    }

    /// <summary>
    /// 对包裹中的宠物排序 
    /// </summary>
    /// <returns>The items in bag.</returns>
    /// <param name="itemlist">Itemlist.</param>
    public static List<Property> SortPetInBag(List<Property> itemlist, int sortType)
    {
        List<Property> sortItems = new List<Property>();

        foreach (Property item in SortMonsters(itemlist, sortType))
            sortItems.Add(item);

        // 按照权重进行排序
        return sortItems;
    }

    /// <summary>
    /// 对宠物进行排序
    /// </summary>
    public static IEnumerable<Property> SortMonsters(List<Property> itemList, int sortType)
    {
        // 根据道具权重排序
        IEnumerable<Property> ItemQuery = from ob in itemList orderby CALC_MONSTER_SORT_RULE.Call(ob, sortType) descending
                                                select ob;
        return  ItemQuery;
    }

    /// <summary>
    /// 对装备进行排序
    /// </summary>
    public static IEnumerable<Property> SortEquips(List<Property> itemList)
    {
        // 根据道具权重排序
        IEnumerable<Property> ItemQuery = from ob in itemList orderby CALC_EQUIP_SORT_RULE.Call(ob) descending
                                                select ob;
        
        return ItemQuery;
    }

    /// <summary>
    /// 对装备进行排序
    /// </summary>
    public static IEnumerable<Property> SortEquipsAttrib(List<Property> itemList, LPCArray minorCondition)
    {
        // 根据道具权重排序
        IEnumerable<Property> ItemQuery = from ob in itemList orderby CALC_EQUIP_ATTRIB_SORT_RULE.Call(ob, minorCondition) descending
            select ob;

        return ItemQuery;
    }

    /// <summary>
    /// 根据包裹分页类型，取得分类装备列表
    /// </summary>
    public static  Dictionary<int, List<Property>> GetSuitByPage(Property ob, int page)
    {
        // 1. 对象不存在
        if (ob == null)
            return null;

        Dictionary<int, List<Property>> equipMap = new Dictionary<int, List<Property>>();
        int suitId = 0;

        // 2. 按照套装id分类
        List<Property> pageList = GetItemsByPage(ob, page);
        foreach (Property item in pageList)
        {
            // 不是装备不处理
            if (!EquipMgr.IsEquipment(item))
                continue;

            // 获取装备suit_id
            suitId = item.Query<int>("suit_id");
            if (suitId == 0)
                continue;

            // 没有该套装数据，初始化equipMap
            if (!equipMap.ContainsKey(suitId))
                equipMap[suitId] = new List<Property>();

            // 添加数据
            equipMap[suitId].Add(item);
        }

        return equipMap;
    }

    /// <summary>
    /// 根据包裹分页类型，取得某页所有物品
    /// </summary>
    public static List<Property> GetItemsByPage(Property who, int page)
    {
        List<Property> items = new List<Property>();

        // 玩家不存在
        if (who == null)
            return items;

        // 取得包裹的物品
        Dictionary<string, Property> all_items = (who as Container).baggage.GetPageCarry(page);
        if (all_items == null || all_items.Count <= 0)
            return items;

        // 遍历各个道具
        foreach (Property item in all_items.Values)
        {
            if (item == null)
                continue;

            // 添加到列表中
            items.Add(item);
        }

        // 返回数据
        return items;
    }

    /// <summary>
    /// 获取指定道具的集合
    /// </summary>
    /// <returns>The items by class identifier.</returns>
    public static List<Property> GetItemsByClassId(Property who, int classId)
    {
        List<Property> items = new List<Property>();

        // 玩家对象不存在
        if (who == null)
            return items;

        // 获取包裹中所有的道具
        Dictionary<string, Property> all_items = (who as Container).baggage.GetPageCarry(ContainerConfig.POS_ITEM_GROUP);
        if (all_items == null || all_items.Count <= 0)
            return items;

        // 遍历各个道具
        foreach (Property item in all_items.Values)
        {
            if (item == null)
                continue;

            if (item.Query<int>("class_id") != classId)
                continue;
            // 添加到列表中
            items.Add(item);
        }

        return items;
    }

    /// <summary>
    /// 根据玩家筛选类型获取装备道具
    /// </summary>
    /// <returns>符合条件的装备列表</returns>
    public static List<Property> GetItemsByCustom(Property who, LPCMapping condition)
    {
        // 对象不存在
        if (who == null)
            return new List<Property>();

        LPCArray mainProps = condition.GetValue<LPCArray>(EquipConst.MAIN_PROP);
        if (mainProps == null)
            mainProps = LPCArray.Empty;

        LPCArray minorProps = condition.GetValue<LPCArray>(EquipConst.MINOR_PROP);
        if (minorProps == null)
            minorProps = LPCArray.Empty;

        LPCArray suits = condition.GetValue<LPCArray>("suits");
        if (suits == null)
            suits = LPCArray.Empty;

        LPCArray stars = condition.GetValue<LPCArray>("stars");
        if (stars == null)
            stars = LPCArray.Empty;

        LPCArray types = condition.GetValue<LPCArray>("types");
        if (types == null)
            types = LPCArray.Empty;

        int onlyNew = condition.GetValue<int>("only_new");

        int hideIntensif = condition.GetValue<int>("hide_intensify");

        // 没有任何条件
        if (mainProps.Count == 0
            && minorProps.Count == 0
            && suits.Count == 0
            && stars.Count == 0
            && types.Count == 0)
            return new List<Property>();

        // 取得包裹的物品
        List<Property> all_items = GetItemsByPage(ME.user, ContainerConfig.POS_ITEM_GROUP);

        if (all_items == null || all_items.Count == 0)
            return new List<Property>();

        // 遍历各个道具
        List<Property> items = new List<Property>();

        foreach (Property item in all_items)
        {
            // 不是装备不处理
            if (!EquipMgr.IsEquipment(item))
                continue;

            // 获取装备属性
            LPCMapping prop = item.Query<LPCMapping>("prop");

            // 装备主属性
            LPCArray mainProp = prop.GetValue<LPCArray>(EquipConst.MAIN_PROP);
            if (mainProp == null)
                mainProp = LPCArray.Empty;

            // 符合要求的主属性的数量
            int mainAmount = 0;

            foreach (LPCValue v in mainProps.Values)
            {
                if (v == null || !v.IsInt)
                    continue;

                foreach (LPCValue m in mainProp.Values)
                {
                    if (m == null || !m.IsArray)
                        continue;

                    // 符合条件
                    if (m.AsArray[0].Equals(v))
                        mainAmount++;
                }
            }

            // 主属性不符合条件
            if (mainAmount < mainProps.Count)
                continue;

            // 装备次要属性
            LPCArray minorProp = prop.GetValue<LPCArray>(EquipConst.MINOR_PROP);
            if (minorProp == null)
                minorProp = LPCArray.Empty;

            // 是否包含筛选的次要属性
            int minorAmount = 0;

            foreach (LPCValue v in minorProps.Values)
            {
                if (v == null || !v.IsInt)
                    continue;

                foreach (LPCValue m in minorProp.Values)
                {
                    if (m == null || !m.IsArray)
                        continue;

                    // 符合条件
                    if (m.AsArray[0].Equals(v))
                        minorAmount++;
                }
            }

            // 次要属性不符合条件
            if (minorAmount < minorProps.Count)
                continue;

            // 装套不符合条件
            if (suits.IndexOf(item.Query<int>("suit_id")) == -1)
                continue;

            // 星级不符合要求
            if (stars.IndexOf(item.Query<int>("star")) == -1)
                continue;

            // 装备类型不符合要求
            if (types.IndexOf(item.Query<int>("equip_type")) == -1)
                continue;

            if (! IsNew(item) && onlyNew == 1)
                continue;

            if (item.GetRank() > 0 && hideIntensif == 1)
                continue;

            // 添加到列表中
            items.Add(item);
        }

        // 返回数据
        return items;

    }

    /// <summary>
    /// 根据玩家筛选类型获取装备道具(用于多选出售智能筛选)
    /// </summary>
    public static List<Property> GetItemsByCustom(Property who, List<Property> all_items)
    {
        // 需要筛选的列表为空
        if (all_items == null || all_items.Count <= 0)
            return new List<Property>();

        // 玩家对象不存在
        if (who == null)
            return new List<Property>();

        // 遍历各个道具
        List<Property> items = new List<Property>();

        // 获得装备增加百分比和敏捷属性
        LPCArray filterProp = CALC_EQUIP_FILTER_STANDARD.Call();

        foreach (Property item in all_items)
        {
            // 装备对象不存在
            if (item == null)
                continue;

            // 不是装备不处理
            if (!EquipMgr.IsEquipment(item))
                continue;

            // 装备类型
            int equipType = item.Query<int>("equip_type");

            // 装备属性
            LPCMapping prop = item.Query<LPCMapping>("prop");

            LPCArray arr = LPCArray.Empty;

            if (equipType == EquipConst.WEAPON
                || equipType == EquipConst.ARMOR
                || equipType == EquipConst.SHOES)
            {
                // 武器、盔甲、鞋子筛选副属性
                arr = prop.GetValue<LPCArray>(EquipConst.MINOR_PROP);
            }
            else
            {
                // 护符, 项链, 戒指筛选主属性
                arr.Append(prop.GetValue<LPCArray>(EquipConst.MAIN_PROP));

                LPCArray minor = prop.GetValue<LPCArray>(EquipConst.MINOR_PROP);
                if (minor != null)
                    arr.Append(minor);
            }

            if (arr == null)
                arr = LPCArray.Empty;

            bool contains = false;

            foreach (LPCValue v in filterProp.Values)
            {
                foreach (LPCValue a in arr.Values)
                {
                    if (a == null || !a.IsArray)
                        continue;

                    if (a.AsArray[0].Equals(v))
                    {
                        contains = true;

                        break;
                    }
                }
            }

            if (contains)
                continue;

            items.Add(item);
        }

        return items;
    }

    /// <summary>
    /// 根据包裹分页类型获取包裹道具
    /// </summary>
    public static List<Property> GetBaggageItemList(Property who, int itemType, int pageGroup = ContainerConfig.POS_ITEM_GROUP)
    {
        List<Property> items = new List<Property>();

        // 对象不存在
        if (who == null)
            return items;

        // 取得包裹的物品
        Dictionary<string, Property> all_items = (who as Container).baggage.GetPageCarry(pageGroup);
        if (all_items == null || all_items.Count <= 0)
            return items;

        // 遍历各个道具
        foreach (Property item in all_items.Values)
        {
            if (item.Query<int>("item_type") != itemType)
                continue;

            items.Add(item);
        }

        // 返回数据
        return items;
    }

    /// <summary>
    /// 返回自定义物品
    /// </summary>
    public static List<Property> GetItemsByCustom(Property who, string type, int value, int pageGroup = ContainerConfig.POS_ITEM_GROUP)
    {
        return new List<Property>();
    }

    /// <summary>
    /// 取得某种品质的所有装备
    /// </summary>
    public static List<Property> GetEquipsByRank(Property who, List<int> rankList, bool canSell = true, int pageGroup = ContainerConfig.POS_ITEM_GROUP)
    {
        // 对象不存在
        if (who == null)
            return new List<Property>();

        // 取得包裹的物品
        Dictionary<string, Property> all_items = (who as Container).baggage.GetPageCarry(pageGroup);
        if (all_items == null || all_items.Count <= 0)
            return new List<Property>();

        // 遍历各个道具
        List<Property> items = new List<Property>();
        foreach (Property item in all_items.Values)
        {
            // 不是装备不处理
            if (!EquipMgr.IsEquipment(item))
                continue;

            // 品质不是需求的品质不处理
            if (rankList.IndexOf(item.Query<int>("rank")) == -1)
                continue;

            // 过滤是否可出售
            // 判断道具是否可以出售
            if (canSell && !CAN_SELL_EQUIP.Call(ME.user, item))
                continue;

            // 添加到列表中
            items.Add(item);
        }

        // 返回数据
        return items;
    }

    /// <summary>
    /// 取得某种品质的所有装备
    /// </summary>
    public static List<Property> GetEquipsByRank(Property who, int rank, bool canSell = true, int pageGroup = ContainerConfig.POS_ITEM_GROUP)
    {
        // 对象不存在
        if (who == null)
            return new List<Property>();

        // 取得包裹的物品
        Dictionary<string, Property> all_items = (who as Container).baggage.GetPageCarry(pageGroup);
        if (all_items == null || all_items.Count <= 0)
            return new List<Property>();

        // 遍历各个道具
        List<Property> items = new List<Property>();
        foreach (Property item in all_items.Values)
        {
            // 不是装备不处理
            if (!EquipMgr.IsEquipment(item))
                continue;

            // 品质不是需求的品质不处理
            if (item.Query<int>("rank") != rank)
                continue;

            // 过滤是否可出售
            // 判断道具是否可以出售
            if (canSell && !CAN_SELL_EQUIP.Call(ME.user, item))
                continue;

            // 添加到列表中
            items.Add(item);
        }

        // 返回数据
        return items;
    }

    /// <summary>
    /// 取得某部位的所有装备
    /// </summary>
    public static List<Property> GetEquipsByEquipType(Property who, int equipType, int pageGroup = ContainerConfig.POS_ITEM_GROUP)
    {
        // 对象不存在
        if (who == null)
            return new List<Property>();

        // 取得包裹的物品
        Dictionary<string, Property> all_items = (who as Container).baggage.GetPageCarry(pageGroup);
        if (all_items == null || all_items.Count <= 0)
            return new List<Property>();

        // 遍历各个道具
        List<Property> items = new List<Property>();

        foreach (Property item in all_items.Values)
        {
            // 1. 不是equip
            // 2. equip_type和指定的不一致
            if (!EquipMgr.IsEquipment(item) ||
                item.Query<int>("equip_type") != equipType)
                continue;

            // 添加列表
            items.Add(item);
        }

        return items;
    }

    /// <summary>
    /// 根据class_id获取包裹道具.
    /// </summary>
    /// <returns>The baggage item by class identifier.</returns>
    /// <param name="who">Who.</param>
    /// <param name="classId">Class identifier.</param>
    public static Property GetBaggageItemByClassId(Property who, int classId)
    {
        if (who == null)
            return null;

        // 取得包裹的物品
        Dictionary<string, Property> allItems = (who as Container).baggage.GetPageCarry(ContainerConfig.POS_ITEM_GROUP);
        if (allItems == null || allItems.Count <= 0)
            return null;

        Property item = null;
        foreach (KeyValuePair<string, Property> kv in allItems)
        {
            if (kv.Value.Query<int>("class_id") != classId)
                continue;

            item = kv.Value;
            break;
        }

        return item;
    }

    /// <summary>
    /// 获取包裹中道具的数量
    /// </summary>
    /// <returns>The baggage item number by class identifier.</returns>
    /// <param name="who">Who.</param>
    /// <param name="classId">Class identifier.</param>
    public static int GetBaggageItemNumByClassId(Property who, int classId)
    {
        Property item = GetBaggageItemByClassId(who, classId);

        if (item == null)
            return 0;

        return item.Query<int>("amount");
    }

    /// <summary>
    /// 尝试升级包裹
    /// </summary>
    /// <returns><c>true</c>, if upgrade baggage was tryed, <c>false</c> otherwise.</returns>
    /// <param name="user">User.</param>
    /// <param name="page">Page.</param>
    public static bool TryUpgradeBaggage(Property user, int page)
    {
        // 如果能够扩充
        if (!BaggageMgr.CheckCanUpgradeBaggage(ME.user, page))
        {
            if(page == ContainerConfig.POS_STORE_GROUP)
                DialogMgr.Notify(LocalizationMgr.Get("StoreWnd_6"));
            else
                DialogMgr.Notify(LocalizationMgr.Get("StoreWnd_5"));
            
            return false;
        }

        // 打开购买界面
        GameObject wnd = WindowMgr.OpenWnd("BuyStorageWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return false;

        wnd.GetComponent<BuyStorageWnd>().BindData(page);

        return true;
    }

    /// <summary>
    /// 检查能否扩充包裹
    /// </summary>
    /// <returns>The can upgrade baggage.</returns>
    /// <param name="page">Page.</param>
    public static bool CheckCanUpgradeBaggage(Property user, int page)
    {
        int size = ME.user.baggage.ContainerSize[page].AsInt;

        int upgrade_size = 5;
        int max_size = 0;

        if (page == ContainerConfig.POS_PET_GROUP)
        {
            upgrade_size = GameSettingMgr.GetSettingInt("upgrade_pet_baggage_add_size");
            max_size = GameSettingMgr.GetSettingInt("max_pet_baggage_size");
        }
        else if (page == ContainerConfig.POS_STORE_GROUP)
        {
            upgrade_size = GameSettingMgr.GetSettingInt("upgrade_store_baggage_add_size");
            max_size = GameSettingMgr.GetSettingInt("max_store_baggage_size");
        }
        else
        {
            // 暂时不支持其他页面扩充
            return false;
        }

        // 返回是否允许扩充
        return ((size + upgrade_size) <= max_size);
    }

    /// <summary>
    /// 设置包裹（仓库）宠物排序方式
    /// </summary>
    /// <returns><c>true</c>, if sort type was set, <c>false</c> otherwise.</returns>
    /// <param name="type">Type.</param>
    public static void SetMonsterSortType(int type)
    {
        OptionMgr.SetOption(ME.user, "pet_sort_type", LPCValue.Create(type));
    }

    /// <summary>
    /// 获取包裹（仓库）宠物排序方式
    /// </summary>
    /// <returns><c>true</c>, if sort type was set, <c>false</c> otherwise.</returns>
    /// <param name="type">Type.</param>
    public static int GetMonsterSortType()
    {
        // 默认为等级排序
        LPCValue petSortType = OptionMgr.GetOption(ME.user, "pet_sort_type");
        if (petSortType == null)
            return MonsterConst.SORT_BY_LEVEL;

        // 返回设置的排序方式
        return petSortType.AsInt;
    }

    /// <summary>
    /// 仓库到包裹
    /// </summary>
    /// <returns>The to baggage.</returns>
    /// <param name="page">Page.</param>
    /// <param name="ridList">Rid list.</param>
    public static bool StoreToBaggage(Property who, int page, List<string> ridList)
    {
        // 对象必须是容器
        if (!(who is Container))
            return false;

        if (ridList.Count <= 0)
            return false;

        if (ContainerMgr.GetFreePosCount(who as Container, page) < ridList.Count)
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("StoreWnd_4"));
            return false;
        }

        List<string> posList = ContainerMgr.GetFreePosList(ME.user,
                                   page, ridList.Count);

        if (posList.Count != ridList.Count)
            return false;

        LPCArray baggaeList = new LPCArray();

        for (int i = 0; i < ridList.Count; i++)
        {
            LPCMapping storeMap = new LPCMapping();
            storeMap.Add("rid", ridList[i]);
            storeMap.Add("pos", posList[i]);

            baggaeList.Add(storeMap);
        }

        return Operation.CmdTake.Go(baggaeList);
    }

    /// <summary>
    /// 包裹到仓库
    /// </summary>
    /// <returns>The to baggage.</returns>
    /// <param name="page">Page.</param>
    /// <param name="ridList">Rid list.</param>
    public static bool BaggageToStore(Property who, List<string> ridList)
    {
        // 对象必须是容器
        if (!(who is Container))
            return false;

        if (ridList.Count <= 0)
            return false;

        if (ContainerMgr.GetFreePosCount(who as Container, ContainerConfig.POS_STORE_GROUP) < ridList.Count)
        {
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("StoreWnd_4"));
            return false;
        }

        List<string> posList = ContainerMgr.GetFreePosList(ME.user, ContainerConfig.POS_STORE_GROUP, ridList.Count);

        if (posList.Count != ridList.Count)
            return false;

        LPCArray baggaeList = new LPCArray();

        for (int i = 0; i < ridList.Count; i++)
        {
            LPCMapping storeMap = new LPCMapping();
            storeMap.Add("rid", ridList[i]);
            storeMap.Add("pos", posList[i]);

            baggaeList.Add(storeMap);
        }

        return Operation.CmdStore.Go(baggaeList);
    }

    /// <summary>
    /// 尝试存入仓库
    /// </summary>
    /// <returns><c>true</c>, if store to baggage was tryed, <c>false</c> otherwise.</returns>
    /// <param name="user">User.</param>
    /// <param name="page">Page.</param>
    /// <param name="num">Number.</param>
    public static bool TryStoreToBaggage(Property user, int page, int num)
    {
        if (!(user is Container))
            return false;

        // 检测格子够不够
        if (ContainerMgr.GetFreePosCount(user as Container, page) >= num)
            return true;

        // 宠物需要弹出扩充宠物提示
        if (page == ContainerConfig.POS_PET_GROUP)
        {
            // 宠物包裹已扩充至最大
            if (!CheckCanUpgradeBaggage(user, page))
                DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("BaggageMgr_1"));
            else
            {
                // 打开购买界面
                GameObject wnd = WindowMgr.OpenWnd("BuyStorageWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return false;

                wnd.GetComponent<BuyStorageWnd>().BindData(page);

            }
        }
        else if (page == ContainerConfig.POS_ITEM_GROUP)
        {
            // 弹出清理包裹提示
            DialogMgr.ShowSingleBtnDailog(null, LocalizationMgr.Get("BaggageMgr_2"));
        }

        return false;
    }

    /// <summary>
    /// 包裹中是否有相同的物品
    /// </summary>
    public static bool IsSameItem(Property ob)
    {
        if (MonsterMgr.IsMonster(ob))
        {
            List<Property> petList = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_PET_GROUP);
            if (petList == null || petList.Count == 0)
                return false;

            for (int i = 0; i < petList.Count; i++)
            {
                if (petList[i] == null)
                    continue;

                if (ob.GetRid().Equals(petList[i].GetRid()))
                    return true;
            }
        }
        else if (EquipMgr.IsEquipment(ob))
        {
            List<Property> equips = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_ITEM_GROUP);

            if (equips == null || equips.Count == 0)
                return false;

            for (int i = 0; i < equips.Count; i++)
            {
                if (equips[i] == null)
                    continue;

                if (ob.GetRid().Equals(equips[i].GetRid()))
                    return true;
            }
        }
        else
        {
        }

        return false;
    }
}
