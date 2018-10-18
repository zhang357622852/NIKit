/// <summary>
/// PetMgr.cs
/// Copy from zhangyg 2014-10-22
/// 宠物管理
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;
using LPC;

/// 宠物管理
public class PetMgr
{
    #region 内部接口

    static PetMgr()
    {
    }

    /// <summary>
    /// 升级
    /// </summary>
    private static void Upgrade(Property pet)
    {
        // 获取使魔基本信息
        int star = pet.Query<int>("star");
        int curExp = pet.QueryAttrib("exp");
        int curLevel = pet.Query<int>("level");
        int newLevel = curLevel;

        // 获取该星级最大等级
        int maxLevel = StdMgr.GetStdAttrib("max_level", star);

        // 升级需要消耗总经验
        int totalCostExp = 0;

        do
        {
            // 玩家已达最高等级，不能继续升级
            if (newLevel >= maxLevel)
                break;

            // 获取等级经验消耗
            int costExp = StdMgr.GetPetStdExp(newLevel + 1, star);

            // 返回信息错误
            if (costExp == 0)
                break;

            // 经验不足，不能升级
            if (curExp < (totalCostExp + costExp))
                break;

            // 汇总totalCostExp
            totalCostExp += costExp;

            // 提升等级
            newLevel++;

        } while(true);

        // 等级没有发生变化
        if (newLevel == curLevel)
            return;

        // 扣除玩家相应经验值
        LPCMapping costMap = new LPCMapping();
        costMap.Add("exp", totalCostExp);
        pet.CostAttrib(costMap);

        // 提升玩家等级
        LPCMapping addMap = new LPCMapping();
        addMap.Add("level", (newLevel - curLevel));
        pet.AddAttrib(addMap);

        // 玩家满级后，修正当前经验值
        if (newLevel >= maxLevel)
            pet.Set("exp", LPCValue.Create(0));

        // 分配属性
        MonsterMgr.AutoDistributeAttrib(pet);

        // 刷新属性
        PropMgr.RefreshAffect(pet);

        // 抛出公式共策划使用
        WHEN_UPGRADE.Call(pet);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 获取星星图片名称
    /// </summary>
    /// <returns>The star name.</returns>
    /// <param name="rank">Rank.</param>
    public static string GetStarName(int rank)
    {
        switch (rank)
        {
            case 0:
                return "silver_start";
            case 1:
                return "gold_start";
            case 2:
                return "vilot_start";
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取元素图标名称
    /// </summary>
    /// <returns>The element icon name.</returns>
    /// <param name="element">Element.</param>
    public static string GetElementIconName(int element)
    {
        switch (element)
        {
            case MonsterConst.ELEMENT_NONE:
                return "pet_noElement";

            case MonsterConst.ELEMENT_FIRE:
                return "pet_fire";

            case MonsterConst.ELEMENT_STORM:
                return "pet_wind";

            case MonsterConst.ELEMENT_WATER:
                return "pet_water";

            case MonsterConst.ELEMENT_LIGHT:
                return "pet_light";

            case MonsterConst.ELEMENT_DARK:
                return "pet_dark";
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取元素名称
    /// </summary>
    public static string GetElementName(int element)
    {
        switch (element)
        {
            case MonsterConst.ELEMENT_NONE:
                return LocalizationMgr.Get("element_info_NA");

            case MonsterConst.ELEMENT_FIRE:
                return LocalizationMgr.Get("element_info_FIRE");

            case MonsterConst.ELEMENT_STORM:
                return LocalizationMgr.Get("element_info_STORM");

            case MonsterConst.ELEMENT_WATER:
                return LocalizationMgr.Get("element_info_WATER");

            case MonsterConst.ELEMENT_LIGHT:
                return LocalizationMgr.Get("element_info_LIGHT");

            case MonsterConst.ELEMENT_DARK:
                return LocalizationMgr.Get("element_info_DARK");
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取觉醒颜色
    /// </summary>
    /// <returns>The awake color.</returns>
    /// <param name="rank">Rank.</param>
    public static string GetAwakeColor(int rank)
    {
        switch (rank)
        {
            case 0:
                return "[eaeaea]";
            case 1:
                return "[ffdd8a]";
            case 2:
                return "[f58aff]";
        }

        return string.Empty;
    }

    /// <summary>
    ///获取宠物身上套装名称
    /// </summary>
    public static string GetPetSuitName(Property who)
    {
        //获取宠物身上穿戴的装备;
        List<Property> equipList = BaggageMgr.GetItemsByPage(who, ContainerConfig.POS_EQUIP_GROUP);

        //获取套装的配置信息表;
        CsvFile suitCsv = EquipMgr.SuitTemplateCsv;

        Dictionary<int, List<int>> suitIdDic = new Dictionary<int, List<int>>();

        if (equipList == null)
            return string.Empty;

        foreach (Property data in equipList)
        {
            //获取装备的套装Id;
            int suit_id = data.Query<int>("suit_id");

            if (suitIdDic.ContainsKey(suit_id))
                suitIdDic[suit_id].Add(suit_id);
            else
            {
                suitIdDic.Add(suit_id, new List<int>());
                suitIdDic[suit_id].Add(suit_id);
            }
        }

        if (suitIdDic == null || suitIdDic.Count < 1)
            return string.Empty;

        string allSuitName = string.Empty;

        foreach (KeyValuePair<int, List<int>> data in suitIdDic)
        {
            //根据套装id获取该套装的配置信息;
            CsvRow row = suitCsv.FindByKey(data.Key);

            //没有该套装配置信息
            if (row == null)
                continue;

            //获取组件数量;
            int sub_count = row.Query<int>("sub_count");

            if (data.Value.Count < 1 || sub_count == 0)
                continue;

            int result = data.Value.Count / sub_count;

            for (int i = 0; i < result; i++)
            {
                allSuitName += LocalizationMgr.Get(row.Query<string>("name")) + " ";
            }
        }
        return allSuitName.Trim();
    }

    /// <summary>
    /// 获取最大等级的宠物
    /// </summary>
    public static Property GetMaxLevelPet(Property who)
    {
        // 玩家包裹的宠物列表
        List<Property> petList = BaggageMgr.GetItemsByPage(who, ContainerConfig.POS_PET_GROUP);

        Property petOb = null;
        for (int i = 0; i < petList.Count; i++)
        {
            // 过滤材料使魔
            if (petList[i].Query<int>("is_material") == 1)
                continue;

            if (petOb == null)
            {
                petOb = petList[i];
            }
            else
            {
                int tempLevel = petOb.Query<int>("level");

                int level = petList[i].Query<int>("level");

                if (level > tempLevel)
                {
                    petOb = petList[i];
                }
                else if (level == tempLevel)
                {
                    if (petOb.Query<int>("exp") < petList[i].Query<int>("exp"))
                        petOb = petList[i];
                }
            }
        }

        // 返回宠物对象
        return petOb;
    }

    /// <summary>
    /// 尝试升级
    /// </summary>
    public static bool TryLevelUp(Property pet)
    {
        // 可能此时对象已经不存在了
        if (pet == null)
            return false;

        // 正在升级过程中，不能再次尝试升级
        if (pet.QueryTemp<int>("upgrading") == 1)
            return false;

        // 标记升级过程中
        pet.SetTemp("upgrading", LPCValue.Create(1));

        // 检查玩家是否可以升级
        Upgrade(pet);

        // 删除标记
        pet.DeleteTemp("upgrading");

        return true;
    }

    /// <summary>
    /// 检查玩家是否可以升级
    /// </summary>
    public static bool CanLevelUp(Property pet)
    {
        // 获取宠物的星级
        int star = pet.Query<int>("star");

        // 玩家已达最高等级，不能继续升级
        int maxLevel = StdMgr.GetStdAttrib("max_level", star);
        if (pet.Query<int>("level") >= maxLevel)
            return false;

        // 经验不足，不能升级
        int costExp = StdMgr.GetPetStdExp(pet.Query<int>("level") + 1, star);
        if (pet.Query<int>("exp") < costExp)
            return false;

        // 通过检查
        return true;
    }

    /// <summary>
    /// 是否是任务使魔
    /// </summary>
    public static bool IsGuidePet(Property user, string petRid)
    {
        // 玩家对象不存在
        if (user == null)
            return false;

        LPCValue guidePet = user.Query<LPCValue>("guide_pet");

        // 没有指引宠物列表
        if (guidePet == null || !guidePet.IsArray)
            return false;

        // 列表中是否存在petRid
        return guidePet.AsArray.IndexOf(petRid) == -1 ? false : true;
    }

    /// <summary>
    /// 判断是否是共享魔灵
    /// </summary>
    public static bool IsSharePet(string petRid)
    {
        if (string.IsNullOrEmpty(petRid))
            return false;

        LPCValue v = ME.user.Query<LPCValue>("share_pet");

        if (v == null || !v.IsString)
            return false;

        if (v.AsString.Equals(petRid))
            return true;

        return false;
    }

    /// <summary>
    /// 判断魔灵是否被锁定
    /// </summary>
    public static bool IsLockPet(Property petOb)
    {
        if (petOb == null)
            return false;

        LPCValue v = petOb.Query<LPCValue>("is_lock");

        if (v == null || !v.IsInt)
            return false;

        return v.AsInt == 1 ? true : false;
    }

    /// <summary>
    /// 是否是防御宠物
    /// </summary>
    public static bool IsDefenceTroop(Property user, Property petOb)
    {
        if (user == null || petOb == null)
            return false;

        // 获取防守宠物
        LPCValue defensePet = user.Query<LPCValue>("defense_troop");

        if (defensePet != null && defensePet.IsArray
            && defensePet.AsArray.IndexOf(petOb.GetRid()) != -1)
            return true;

        return false;
    }

    #endregion
}
