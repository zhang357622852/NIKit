/// <summary>
/// SCRIPT_7000.cs
/// Created by zhaozy 2014-12-16
/// 掉落脚本
/// </summary>

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using LPC;

/// <summary>
/// 通用金钱奖励计算脚本(副本)
/// </summary>
public class SCRIPT_7000 : Script
{
    public override object Call(params object[] _param)
    {

        LPCMapping entityInfo = new LPCMapping();
        entityInfo = _param[0] as LPCMapping;

        LPCMapping instanceInfo = entityInfo.GetValue<LPCMapping>("instance_info");

        int dropRate = (_param[1] as LPCValue).AsInt;

        // 计算概率
        if (instanceInfo.GetValue<int>("map_id") == 1 &&
            instanceInfo.GetValue<int>("difficulty") == InstanceConst.INSTANCE_DIFFICULTY_EASY)
            dropRate = 1000;

        if (UnityEngine.Random.Range(0, 1000) >= dropRate)
            return LPCMapping.Empty;

        int level = entityInfo.GetValue<int>("level");

        // 构造金钱掉落数据
        int money = CALC_STD_MONEY.Call(level);
        // 正负百分之3修正
        if (UnityEngine.Random.Range(0, 1000) >= 500)
            money += money * 30 / 1000;
        else
            money -= money * 30 / 1000;

        // 缓存池修正
        LPCMapping bonusInfo = BonusMgr.RemainBonusMap;

        int remainMoney = bonusInfo.GetValue<int>("money");
        if (money > remainMoney)
            money = remainMoney;

        LPCMapping attrib = new LPCMapping();
        attrib.Add("money", money);

        return attrib;
    }
}

/// <summary>
/// 通用钻石奖励计算脚本(test)
/// </summary>
public class SCRIPT_7001 : Script
{
    public override object Call(params object[] _param)
    {
        int dropRate = (_param[1] as LPCValue).AsInt;
        // 计算概率
        if (UnityEngine.Random.Range(0, 1000) >= dropRate)
            return LPCMapping.Empty;

        LPCMapping attrib = new LPCMapping();
        attrib.Add("diamond", 1);

        return attrib;
    }
}

/// <summary>
/// 通用体力奖励计算脚本(test)
/// </summary>
public class SCRIPT_7002 : Script
{
    public override object Call(params object[] _param)
    {
        int dropRate = (_param[1] as LPCValue).AsInt;

        // 计算概率
        if (UnityEngine.Random.Range(0, 1000) >= dropRate)
            return LPCMapping.Empty;

        LPCMapping attrib = new LPCMapping();
        attrib.Add("life", 1);

        return attrib;
    }
}

/// <summary>
/// 通用竞技场入场券计算脚本(test)
/// </summary>
public class SCRIPT_7003 : Script
{
    public override object Call(params object[] _param)
    {
        // 构造体力掉落数据
        LPCMapping attrib = new LPCMapping();
        attrib.Add("arena", 1);

        return attrib;
    }
}

/// <summary>
/// 副本通关奖励计算脚本
/// </summary>
public class SCRIPT_7004 : Script
{
    public override object Call(params object[] _param)
    {
        return _param[0];
    }
}

/// <summary>
/// 是否显示副本计算脚本
/// </summary>
public class SCRIPT_7005 : Script
{
    public override object Call(params object[] _param)
    {
        // _param[0] show_script_args
        LPCMapping args = _param[0] as LPCMapping;

        // 副本通关条件、最小等级、最大等级
        int level = ME.user.GetLevel();
        if (InstanceMgr.IsClearanced(ME.user, args.GetValue<string>("instance_id")) &&
            level >= args.GetValue<int>("min_level") && level <= args.GetValue<int>("max_level"))
            return true;
        else
            return false;
    }
}

/// <summary>
/// 竞技场对战奖励脚本
/// </summary>
public class SCRIPT_7006 : Script
{
    public override object Call(params object[] _param)
    {
        LPCMapping topData = ME.user.Query<LPCMapping>("arena_top");

        LPCMapping args = _param[0] as LPCMapping;

        int rank = topData.GetValue<int>("rank");
        int score = topData.GetValue<int>("score");

        int step = ArenaMgr.GetStepByScoreAndRank(rank, score);

        // 参与者组、青铜组
        step = step <= 13 ? 13 : step;
        // 白银组
        step = step <= 6 ? 6 : step;
        // 黄金组
        step = step <= 3 ? 3 : step;

        LPCMapping bonus = new LPCMapping();

        bonus.Add("exploit", args[step]);

        return bonus;
    }
}

/// <summary>
/// 竞技场反击奖励脚本
/// </summary>
public class SCRIPT_7007 : Script
{
    public override object Call(params object[] _param)
    {
        LPCMapping topData = ME.user.Query<LPCMapping>("arena_top");

        LPCMapping args = _param[0] as LPCMapping;

        int rank = topData.GetValue<int>("rank");
        int score = topData.GetValue<int>("score");

        int step = ArenaMgr.GetStepByScoreAndRank(rank, score);

        // 参与者组、青铜组
        step = step <= 13 ? 13 : step;
        // 白银组
        step = step <= 6 ? 6 : step;
        // 黄金组
        step = step <= 3 ? 3 : step;

        LPCMapping bonus = new LPCMapping();

        bonus.Add("exploit", args[step]);

        return bonus;
    }
}

/// <summary>
/// 是否能够进入副本计算脚本
/// </summary>
public class SCRIPT_7008 : Script
{
    public override object Call(params object[] _param)
    {
        // _param[0] show_script_args
        Property user = _param[0] as Property;
        LPCMapping args = (_param[2] as LPCValue).AsMapping;

        // 副本通关条件、最小等级、最大等级
        int level = user.GetLevel();
        if (InstanceMgr.IsClearanced(user, args.GetValue<string>("instance_id")) &&
            level >= args.GetValue<int>("min_level") && level <= args.GetValue<int>("max_level"))
            return true;
        else
            return false;
    }
}

/// <summary>
/// 竞技场排位战金钱奖励计算脚本
/// </summary>
public class SCRIPT_7009 : Script
{
    public override object Call(params object[] _param)
    {

        //LPCMapping entityInfo = new LPCMapping();
        //entityInfo = _param[0] as LPCMapping;

        //LPCMapping instanceInfo = entityInfo.GetValue<LPCMapping>("instance_info");

        int moneyDropCo = (_param[1] as LPCValue).AsInt;

        //int level = entityInfo.GetValue<int>("level");

        // 构造金钱掉落数据
        int money = moneyDropCo;

        // 正负百分之5修正
        if (UnityEngine.Random.Range(0, 1000) >= 500)
            money += money * UnityEngine.Random.Range(0, 50) / 1000;
        else
            money -= money * UnityEngine.Random.Range(0, 50) / 1000;

        // 缓存池修正
        LPCMapping bonusInfo = BonusMgr.RemainBonusMap;

        int remainMoney = bonusInfo.GetValue<int>("money");
        if (money > remainMoney)
            money = remainMoney;

        LPCMapping attrib = new LPCMapping();
        attrib.Add("money", money);

        return attrib;
    }
}

/// <summary>
/// 竞技场NPC战的金钱奖励计算脚本
/// </summary>
public class SCRIPT_7010 : Script
{
    public override object Call(params object[] _param)
    {

        LPCMapping entityInfo = new LPCMapping();
        entityInfo = _param[0] as LPCMapping;

        LPCMapping instanceInfo = entityInfo.GetValue<LPCMapping>("instance_info");

        //int moneyDropCo = (_param[1] as LPCValue).AsInt;

        LPCMapping dbase = instanceInfo.GetValue<LPCMapping>("dbase");

        // 构造金钱掉落数据
        int money = (dbase.GetValue<int>("max_money") - 150) / 5;

        // 正负百分之5修正
        if (UnityEngine.Random.Range(0, 1000) >= 500)
            money += money * UnityEngine.Random.Range(0, 50) / 1000;
        else
            money -= money * UnityEngine.Random.Range(0, 50) / 1000;

        // 缓存池修正
        LPCMapping bonusInfo = BonusMgr.RemainBonusMap;

        int remainMoney = bonusInfo.GetValue<int>("money");
        if (money > remainMoney)
            money = remainMoney;

        LPCMapping attrib = new LPCMapping();
        attrib.Add("money", money);

        return attrib;
    }
}

/// <summary>
/// 进入竞技场检测脚本(客户端不做任何判断交给服务器判断)
/// </summary>
public class SCRIPT_7011 : Script
{
    public override object Call(params object[] _param)
    {
        return true;
    }
}

/// <summary>
/// 进入竞技场反击检测脚本(客户端不做任何判断交给服务器判断)
/// </summary>
public class SCRIPT_7012 : Script
{
    public override object Call(params object[] _param)
    {
        return true;
    }
}

/// <summary>
/// 进入判断通天塔检测脚本
/// </summary>
public class SCRIPT_7013 : Script
{
    public override object Call(params object[] _param)
    {
        Property user = _param[0] as Property;
        string instanceId = (string) _param[1];

        // 判断通天塔副本是否已经解锁
        // 如果没有解锁不能进入
        // 获取副本对应层级信息
        CsvRow data = TowerMgr.GetTowerInfoByInstance(instanceId);

        // 没有配置的通天塔
        if (data == null)
            return false;

        // 判断通天塔否已经解锁，没有解锁的通天塔不能进入
        if (! TowerMgr.IsUnlocked(user, data.Query<int>("difficulty"), data.Query<int>("layer")))
            return false;

        // 允许进入通天塔
        return true;
    }
}

/// <summary>
/// 普通副本进入检测脚本
/// </summary>
public class SCRIPT_7014 : Script
{
    public override object Call(params object[] _param)
    {
        Property user = _param[0] as Property;
        string instanceId = (string) _param[1];

        // 普通副本进入检测脚本
        return InstanceMgr.IsUnlocked(user, instanceId) && InstanceMgr.IsUnLockLevel(user, instanceId);
    }
}

/// <summary>
/// 是否显示指引用竞技场副本计算脚本
/// </summary>
public class SCRIPT_7015 : Script
{
    public override object Call(params object[] _param)
    {
        // _param[0] show_script_args
        LPCMapping args = _param[0] as LPCMapping;

        // 副本通关条件、最小等级、最大等级
        int level = ME.user.GetLevel();
        if (InstanceMgr.IsClearanced(ME.user, args.GetValue<string>("instance_id")) &&
            !InstanceMgr.IsClearanced(ME.user, args.GetValue<string>("guide_arena")) &&
            level >= args.GetValue<int>("min_level") && level <= args.GetValue<int>("max_level"))
            return true;
        else
            return false;
    }
}

/// <summary>
/// 通天塔奖励脚本
/// </summary>
public class SCRIPT_7020 : Script
{
    public override object Call(params object[] _param)
    {
        return _param[0];
    }
}

/// <summary>
/// 通天塔奖励脚本
/// </summary>
public class SCRIPT_7021 : Script
{
    public override object Call(params object[] _param)
    {
        return _param[0];
    }
}

/// <summary>
/// 副本消耗计算脚本
/// </summary>
public class SCRIPT_7030 : Script
{
    public override object Call(params object[] _param)
    {
        // _param[0] 玩家对象
        // _param[1] 副本id
        // _param[2] 消耗计算参数
        LPCMapping costMapArgs = _param[2] as LPCMapping;

        LPCArray freeActivity = _param[3] as LPCArray;

        // 动态副本参数
        //LPCMapping dynamicPara = _param[4] as LPCMapping;

        for (int i = 0; i < freeActivity.Count; i++)
        {
            if (!ActivityMgr.IsAcitvityValid(freeActivity[i].AsString))
                continue;

            LPCMapping freeCost = LPCMapping.Empty;

            // 消耗属性的字段
            string field = FieldsMgr.GetFieldInMapping(costMapArgs);

            if (string.IsNullOrEmpty(field))
                return freeCost;

            freeCost.Add(field, 0);

            return freeCost;
        }

        return _param[2] as LPCMapping;
    }
}

/// <summary>
/// 通天塔消耗计算脚本
/// </summary>
public class SCRIPT_7031 : Script
{
    public override object Call(params object[] _param)
    {
        // _param[0] 玩家对象
        // _param[1] 副本id
        // _param[2] 消耗计算参数

        Property user = _param[0] as Property;
        if (user == null)
            return LPCMapping.Empty;

        string instanceId = _param[1] as String;

        CsvRow towerConfig = TowerMgr.GetTowerInfoByInstance(instanceId);
        if (towerConfig == null)
            return LPCMapping.Empty;

        LPCMapping costMap = _param[2] as LPCMapping;

        string fields = FieldsMgr.GetFieldInMapping(costMap);

        LPCArray freeActivity = _param[3] as LPCArray;

        // 有免费消耗活动不消耗
        for (int i = 0; i < freeActivity.Count; i++)
        {
            if (!ActivityMgr.IsAcitvityValid(freeActivity[i].AsString))
                continue;

            LPCMapping freeCost = LPCMapping.Empty;

            freeCost.Add(fields, 0);

            return freeCost;
        }

        // 通天塔已经通关不消耗
        if (TowerMgr.IsClearanced(user, towerConfig.Query<int>("difficulty"), towerConfig.Query<int>("layer")))
        {
            LPCMapping cost = new LPCMapping();
            cost.Add(fields, 0);

            return cost;
        }

        return costMap;
    }
}

/// <summary>
/// 动态副本消耗计算脚本
/// </summary>
public class SCRIPT_7033 : Script
{
    public override object Call(params object[] _param)
    {
        // _param[0] 玩家对象
        // _param[1] 副本id
        // _param[2] 消耗计算参数
        LPCMapping costMapArgs = _param[2] as LPCMapping;

        //LPCArray freeActivity = _param[3] as LPCArray;

        // 动态副本参数
        LPCMapping dynamicPara = _param[4] as LPCMapping;

        // 动态副本获取消耗
        LPCMapping dynamicCost = new LPCMapping();

        if (dynamicPara != null && dynamicPara.ContainsKey("pet_id"))
        {
            CsvRow row = MonsterMgr.GetRow(dynamicPara.GetValue<int>("pet_id"));
            if (row != null)
            {
                int star = row.Query<int>("star");
                dynamicCost = costMapArgs.GetValue<LPCMapping>(star);
                return dynamicCost;
            }
        }

        dynamicCost.Add("life", 0);

        return dynamicCost;
    }
}

/// <summary>
/// 精英圣域副本消耗计算脚本
/// </summary>
public class SCRIPT_7036 : Script
{
    public override object Call(params object[] _param)
    {
        // _param[0] 玩家对象
        // _param[1] 副本id
        // _param[2] 消耗计算参数
        LPCMapping costMapArgs = _param[2] as LPCMapping;

        //LPCArray freeActivity = _param[3] as LPCArray;

        return costMapArgs;
    }
}

/// <summary>
/// 精英圣域副本名称计算脚本
/// </summary>
public class SCRIPT_7037 : Script
{
    public override object Call(params object[] _param)
    {
        // string 副本ID、mapping 副本信息
        string instanceId = _param[0] as String;
        LPCMapping instanceData = _param[1] as LPCMapping;

        int petId = instanceData.GetValue<int>("pet_id");

        CsvRow monsterInfo = MonsterMgr.GetRow(petId);

        string name = monsterInfo.Query<string>("name");

        // 副本使魔名称
        name = LocalizationMgr.Get(name);

        int element = monsterInfo.Query<int>("element");

        string elementString = MonsterConst.elementTypeMap[element];

        // 层数字符串
        string layerName = LocalizationMgr.Get(InstanceConst.eliteLayerMap[instanceId]);

        // 最终返回字符串
        string finalName = string.Format("{0}·{1}{2}", LocalizationMgr.Get(elementString), name, layerName);

        return finalName;
    }
}

/// <summary>
/// 好友圣域副本名称计算脚本
/// </summary>
public class SCRIPT_7038 : Script
{
    public override object Call(params object[] _param)
    {
        // string 副本ID、mapping 副本信息
        //string instanceId = _param[0] as String;
        LPCMapping instanceData = _param[1] as LPCMapping;

        // 没有秘密地下城详细信息
        if (instanceData.Count == 0)
            return LocalizationMgr.Get("instance_name_secret_1");

        int petId = instanceData.GetValue<int>("pet_id");
        CsvRow monsterInfo = MonsterMgr.GetRow(petId);
        string name = monsterInfo.Query<string>("name");

        // 副本使魔名称
        name = LocalizationMgr.Get(name);
        int element = monsterInfo.Query<int>("element");
        string elementString = MonsterConst.elementTypeMap[element];

        return string.Format("{0}{1}·{2}", LocalizationMgr.Get("instance_name_secret_1"), LocalizationMgr.Get(elementString), name);
    }
}

/// <summary>
/// 召唤项是否显示脚本
/// </summary>
public class SCRIPT_7500 : Script
{
    public override object Call(params object[] _param)
    {
        LPCMapping para = (_param[1] as LPCValue).AsMapping;

        if (para != null && para.ContainsKey("class_id"))
        {
            int amount = UserMgr.GetAttribItemAmount(ME.user, para.GetValue<int>("class_id"));

            if (amount > 0)
                return true;
        }

        return false;
    }
}

/// <summary>
/// 通用召唤消耗描述
/// </summary>
public class SCRIPT_7501 : Script
{
    public override object Call(params object[] _param)
    {
        LPCMapping arg = (LPCMapping)_param[1];

        if (arg != null && arg.ContainsKey("class_id"))
        {
            int classId = arg.GetValue<int>("class_id");

            int amount = UserMgr.GetAttribItemAmount(ME.user, classId);

            return string.Format(LocalizationMgr.Get("SummonWnd_21"), amount);
        }

        return string.Empty;
    }
}

/// <summary>
/// 货币召唤消耗描述
/// </summary>
public class SCRIPT_7502 : Script
{
    public override object Call(params object[] _param)
    {
        LPCMapping arg = (LPCMapping)_param[1];

        if (arg != null && arg.ContainsKey("type"))
        {
            int itemNum = ME.user.Query<int>(arg.GetValue<string>("type"));
            int maxNum = arg.GetValue<int>("cost");
            if (arg.GetValue<string>("type") == "fp")
                return string.Format(LocalizationMgr.Get("SummonWnd_26"), itemNum, maxNum);
            if (arg.GetValue<string>("type") == "gold_coin")
                return string.Format(LocalizationMgr.Get("SummonWnd_27"), itemNum, maxNum);
        }

        return string.Empty;
    }
}

/// <summary>
/// 碎片召唤消耗描述
/// </summary>
public class SCRIPT_7503 : Script
{
    public override object Call(params object[] _param)
    {
        LPCMapping arg = (LPCMapping)_param[1];

        if (arg != null && arg.ContainsKey("class_id"))
        {
            int classId = arg.GetValue<int>("class_id");

            int amount = UserMgr.GetAttribItemAmount(ME.user, classId);

            int maxNum = arg.GetValue<int>("amount");

            string itemName = ItemMgr.GetName(classId);

            return string.Format(LocalizationMgr.Get("SummonWnd_25"), itemName, amount, maxNum);
        }

        return string.Empty;
    }
}

/// <summary>
/// 碎片召唤消耗描述脚本
/// </summary>
public class SCRIPT_7504 : Script
{
    public override object Call(params object[] _param)
    {
        LPCMapping arg = (LPCMapping)_param[1];
        int pet_id = (int)_param[2];

        // 获取召唤所需的碎片数量
        int needPiece = MonsterMgr.GetPieceAmount(pet_id);

        if (ME.user == null)
            return string.Empty;

        // 获取当前拥有的数量
        int maxAmount = UserMgr.GetAttribItemAmount(ME.user, arg.GetValue<int>("class_id"), pet_id);

        // 如果宠物碎片不够，查看下万能碎片够不够抵消(1.4星以下，包括4星 2.最多能抵消25个万能碎片卷)
        if (maxAmount < needPiece)
        {
            int count = ME.user.Query<int>("chip_all");
            if (maxAmount + count >= needPiece)
                return string.Format(LocalizationMgr.Get("SummonWnd_35"), maxAmount, (needPiece-maxAmount), needPiece);
        }

        return string.Format(LocalizationMgr.Get("SummonWnd_28"), maxAmount, needPiece);
    }
}

/// <summary>
/// 碎片召唤显示脚本
/// </summary>
public class SCRIPT_7530 : Script
{
    public override object Call(params object[] _param)
    {
        LPCMapping para = (_param[1] as LPCValue).AsMapping;

        if (para != null && para.ContainsKey("class_id"))
        {
            // 获取道具对应的属性
            string field = FieldsMgr.GetAttribByClassId(para.GetValue<int>("class_id"));

            if (string.IsNullOrEmpty(field))
                return new List<int>();

            LPCValue attribInfo = ME.user.Query<LPCValue>(field);

            if (!attribInfo.IsMapping)
                return new List<int>();

            LPCMapping attribMap = attribInfo.AsMapping;

            List<int> petIdList = new List<int>();

            foreach (int petId in attribMap.Keys)
            {
                int needPiece = MonsterMgr.GetPieceAmount(petId);

                if (needPiece <= 0)
                    continue;

                if (attribMap.GetValue<int>(petId) <= 0)
                    continue;

                //如果宠物碎片不够，查看下万能碎片够不够抵消(1.4星以下，包括4星 2.最多能抵消25个万能碎片卷)
                if (attribMap.GetValue<int>(petId) < needPiece)
                {
                    int count = ME.user.Query<int>("chip_all");
                    if (attribMap.GetValue<int>(petId) + count < needPiece)
                        continue;

                    if ((needPiece - attribMap.GetValue<int>(petId)) > GameSettingMgr.GetSetting<int>("max_amount_use_chip_all"))
                        continue;

                    int star = MonsterMgr.GetDefaultStar(petId);
                    if (star > GameSettingMgr.GetSetting<int>("max_star_use_chip_all"))
                        continue;
                }

                petIdList.Add(petId);
            }

            return petIdList;
        }

        return new List<int>();
    }
}

/// <summary>
/// 碎片召唤显示脚本
/// </summary>
public class SCRIPT_7531 : Script
{
    public override object Call(params object[] _param)
    {
        LPCMapping para = (_param[1] as LPCValue).AsMapping;

        if (para != null && para.ContainsKey("class_id"))
        {
            // 获取道具对应的属性
            string field = FieldsMgr.GetAttribByClassId(para.GetValue<int>("class_id"));

            if (string.IsNullOrEmpty(field))
                return false;

            LPCValue attribInfo = ME.user.Query<LPCValue>(field);

            if (attribInfo == null || !attribInfo.IsMapping)
                return false;

            return  true;
        }

        return false;
    }
}

/// <summary>
/// 消耗脚本
/// </summary>
public class SCRIPT_7621 : Script
{
    public override object Call(params object[] _param)
    {
        // 第一个参数为user，第二个为cost_args
        // 第三个参数:如果是具体某个道具召唤，则为pet_id，否者为0。

        //Property who = _param[0] as Property;
        // LPCArray para = _param[1] as LPCValue;
        //int item = _param[2] as int;

        return _param[1] as LPCArray;
    }
}

/// <summary>
/// 碎片召唤消耗脚本
/// </summary>
public class SCRIPT_7622 : Script
{
    public override object Call(params object[] _param)
    {
        // 第一个参数为user，第二个为cost_args
        // 第三个参数:如果是具体某个道具召唤，则为property本身（比如碎片），否者为null。

        //Property who = _param[0] as Property;
        // LPCValue para = _param[1] as LPCValue;
        //Property item = _param[2] as Property;

        return _param[1] as LPCArray;

    }
}

/// <summary>
/// 道具作用描述脚本：套装箱子
/// </summary>
public class SCRIPT_7700 : Script
{
    public override object Call(params object[] _param)
    {
        Property itemOb = _param[0] as Property;

        // 获得套装ID
        int suitId = itemOb.Query<int>("suit_id");

        CsvRow suitInfo = EquipMgr.GetSuitTemplate(suitId);

        string suitName = LocalizationMgr.Get(suitInfo.Query<string>("name"));

        int subCount = suitInfo.Query<int>("sub_count");

        string itemDesc = string.Format(LocalizationMgr.Get("suitCase_desc"), subCount, itemOb.Query<int>("star"), suitName);

        return itemDesc;
    }
}

/// <summary>
/// 活动奖励显示脚本
/// </summary>
public class SCRIPT_7701 : Script
{
    public override object Call(params object[] _param)
    {
        //int taskId = (int)_param[0];
        LPCMapping bonusArgs = (_param[1] as LPCValue).AsMapping;

        LPCArray bonusList = new LPCArray();

        // 添加积分奖励
        if (bonusArgs.ContainsKey("score"))
        {
            LPCMapping scoreMap = new LPCMapping();
            scoreMap.Add("score", bonusArgs.GetValue<int>("score"));
            bonusList.Add(scoreMap);
        }

        // 添加正常道具奖励
        if (bonusArgs.GetValue<LPCArray>("goods_list") != null)
        {
            LPCArray goodsList = bonusArgs.GetValue<LPCArray>("goods_list");
            for (int i = 0; i < goodsList.Count; i++)
                bonusList.Add(goodsList[i].AsMapping);
        }

        return bonusList;
    }
}

/// <summary>
/// 活动奖励显示脚本
/// </summary>
public class SCRIPT_7702 : Script
{
    public override object Call(params object[] _param)
    {
        //int taskId = (int)_param[0];
        LPCMapping bonusArgs = (_param[1] as LPCValue).AsMapping;

        LPCArray bonusList = new LPCArray();

        LPCMapping scoreMap = new LPCMapping();
        scoreMap.Add("score", bonusArgs.GetValue<int>("score"));
        bonusList.Add(scoreMap);

        return bonusList;
    }
}

/// <summary>
/// 活动奖励显示脚本
/// </summary>
public class SCRIPT_7703 : Script
{
    public override object Call(params object[] _param)
    {
        //int taskId = (int)_param[0];
        LPCMapping bonusArgs = (_param[1] as LPCValue).AsMapping;

        LPCArray bonusList = new LPCArray();

        // 添加积分奖励
        if (bonusArgs.ContainsKey("score"))
        {
            LPCMapping scoreMap = new LPCMapping();
            scoreMap.Add("score", bonusArgs.GetValue<int>("score"));
            bonusList.Add(scoreMap);
        }

        // 添加正常道具奖励
        if (bonusArgs.GetValue<LPCArray>("goods_list") != null)
        {
            LPCArray goodsList = bonusArgs.GetValue<LPCArray>("goods_list");
            for (int i = 0; i < goodsList.Count; i++)
                bonusList.Add(goodsList[i].AsMapping);
        }

        return bonusList;
    }
}
