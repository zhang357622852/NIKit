/// <summary>
/// PetsmithMgr.cs
/// Created by lic 2016-09-08
/// 宠物工坊管理
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using LPC;

public static class PetsmithMgr
{
    #region 属性

    // 工坊模块集合
    private static Dictionary<string, Petsmith> mPetsmithList = new Dictionary<string, Petsmith>();

    // 道具合成配置表信息
    private static CsvFile mStarUpCsv;

    // 宠物合成配置表信息
    private static CsvFile mSynthesisCsv;
 
    #endregion

    #region 属性

    // 获取道具合成配置表信息
    public static CsvFile StarUpCsv { get { return mStarUpCsv; } }

    // 获取宠物合成配置表信息
    public static CsvFile SynthesisCsv { get { return mSynthesisCsv; } }

    #endregion

    #region 内部接口

    /// <summary>
    /// MSG_PetSMITH_ACTION的回调
    /// </summary>
    private static void OnMsgPetsmithAction(string cmd, LPCValue para)
    {
        // 玩家对象不存在
        if (ME.user == null || ME.user.IsDestroyed)
            return;

        LPCMapping args = para.AsMapping;
        string action = args.GetValue<string>("action");
        LPCMapping extra_para = args.GetValue<LPCMapping>("extra_para");

        Petsmith mod;
        mPetsmithList.TryGetValue(action, out mod);

        // 没有处理模块
        if (mod == null)
            return;

        // 调用子模块操作
        mod.DoActionResult(ME.user, extra_para);
    }

    /// <summary>
    /// 载入宠物工坊子模块
    /// </summary>
    private static void LoadSubmodule()
    {
        mPetsmithList.Clear();

        Assembly asse = Assembly.GetExecutingAssembly();
        Type[] types = asse.GetTypes();
        foreach (Type t in types)
        {
            // 不是工坊模块，无视
            if (!t.IsSubclassOf(typeof(Petsmith)))
                continue;

            mPetsmithList.Add(t.Name.ToLower(), asse.CreateInstance(t.Name) as Petsmith);
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 载入工坊子模块
        LoadSubmodule();

        // 载入升星规则表
        mStarUpCsv = CsvFileMgr.Load("starup_rule");

        // 载入宠物合成信息表
        mSynthesisCsv = CsvFileMgr.Load("synthesis_pet");

        // 关注MSG_BLACKSMITH_ACTION消息
        MsgMgr.RemoveDoneHook("MSG_PETSMITH_ACTION", "PetsmithMgr");
        MsgMgr.RegisterDoneHook("MSG_PETSMITH_ACTION", "PetsmithMgr", OnMsgPetsmithAction);
    }

    /// <summary>
    /// 检测玩家属性消耗是足够
    /// </summary>
    /// <returns><c>true</c>, if money enough was checked, <c>false</c> otherwise.</returns>
    /// <param name="cost_map">Cost map.</param>
    public static bool CheckMoneyEnough(LPCMapping cost_map)
    {
        if(cost_map == null || cost_map.Count == 0)
            return true;

        // 遍历消耗列表
        foreach(string field in cost_map.Keys)
        {
            if(string.IsNullOrEmpty(field))
                continue;

            // 金币或钻石要弹出窗口是否跳至商店，其他属性不够要给出提示
            if (field.Equals("money") && 
                ME.user.Query<int>(field) < cost_map.GetValue<int>(field))
            {
                // 弹出是否购买金钱提示框
                DialogMgr.ShowDailog(new CallBack(GotoShop, ShopConfig.MONEY_GROUP), string.Format(LocalizationMgr.Get("PetStrengthenWnd_18"), FieldsMgr.GetFieldName(field)));

                return false;
            }
            else if(field.Equals("gold_coin") &&
                ME.user.Query<int>(field) < cost_map.GetValue<int>(field))
            {
                //弹出是否购买钻石提示框
                DialogMgr.ShowDailog(new CallBack(GotoShop, ShopConfig.GOLD_COIN_GROUP), string.Format(LocalizationMgr.Get("PetStrengthenWnd_18"), FieldsMgr.GetFieldName(field)));

                return false;
            }
            else if(ME.user.Query<int>(field) < cost_map.GetValue<int>(field))
            {
                // 弹出属性不够提示
                DialogMgr.ShowSingleBtnDailog(null, string.Format(LocalizationMgr.Get("PetStrengthenWnd_17"), FieldsMgr.GetFieldName(field)));

                return false;
            }
        }

        return true;  
    }

    /// <summary>
    /// 前往商店
    /// </summary>
    public static void GotoShop(object para, params object[] _params)
    {
        if (!(bool)_params [0])
            return;

        // 前往商店
        GameObject wnd = WindowMgr.OpenWnd(QuickMarketWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        wnd.GetComponent<QuickMarketWnd>().Bind(para as string);
    }

    /// <summary>
    /// 根据强化材料获取所加经验值经验
    /// </summary>
    /// <returns>The add exp.</returns>
    /// <param name="pet_rid">Pet rid.</param>
    /// <param name="material_list">Material list.</param>
    public static int GetAddExp(string pet_rid, List<string> material_list)
    {
        if (string.IsNullOrEmpty(pet_rid))
            return 0;

        Property pet_ob = Rid.FindObjectByRid(pet_rid);
        if (pet_ob == null)
            return 0;

        int add_exp = 0;

        foreach (string rid in material_list)
        {
            if (string.IsNullOrEmpty(rid))
                continue;

            Property material_ob = Rid.FindObjectByRid(rid);
            if (material_ob == null)
                continue;

            int script_no = material_ob.Query<int>("exp_script");

            if (script_no <= 0)
                continue;

            LPCValue script_arg = material_ob.Query<LPCValue>("exp_arg");

            int exp = (int)ScriptMgr.Call(script_no, pet_ob, material_ob, script_arg);

            add_exp += exp;
        }

        return add_exp;
    }

    /// <summary>
    /// 获取升星所需材料的位置
    /// </summary>
    /// <returns>The material star position.</returns>
    /// <param name="star">Star.</param>
    public static LPCMapping GetStarUpMaterialCost(Property petOb)
    {
        // 对象不存在
        if (petOb == null)
            return LPCMapping.Empty;

        if (! MonsterMgr.IsMaxLevel(petOb))
            return LPCMapping.Empty;

        // 获取配置信息
        CsvRow data = PetsmithMgr.StarUpCsv.FindByKey(petOb.GetStar());

        int script_no = data.Query<int>("material_cost_script");
        if (script_no <= 0)
            return LPCMapping.Empty;

        // 通过脚本计算消耗
        LPCMapping costMap = (LPCMapping)ScriptMgr.Call(script_no, petOb, data.Query<LPCMapping>("material_cost_args"));
        if (costMap == null)
            return LPCMapping.Empty;

        // 返回消耗
        return costMap;
    }

    /// <summary>
    /// 获取升星所需材料的位置
    /// </summary>
    /// <returns>The material star position.</returns>
    /// <param name="star">Star.</param>
    public static List<int> GetMaterialStarPos(string pet_rid)
    {
        if (string.IsNullOrEmpty(pet_rid))
            return new List<int>();

        Property pet_ob = Rid.FindObjectByRid(pet_rid);

        if (pet_ob == null)
            return new List<int>();

        if (!MonsterMgr.IsMaxLevel(pet_ob))
            return new List<int>();

        CsvRow data = PetsmithMgr.StarUpCsv.FindByKey(pet_ob.GetStar());

        int script_no = data.Query<int>("material_cost_script");

        if (script_no <= 0)
            return new List<int>();

        LPCMapping cost_map = (LPCMapping)ScriptMgr.Call(script_no, pet_ob, data.Query<LPCMapping>("material_cost_args"));

        if (cost_map == null)
            return new List<int>();

        List<int> star_list = new List<int>();

        foreach (int star in cost_map.Keys)
        {
            for (int i = 0; i < cost_map.GetValue<int>(star); i++)
            {
                star_list.Add(star);
            }
        }

        return star_list;
    }

    /// <summary>
    /// 材料中是否包含技能材料
    /// </summary>
    /// <returns><c>true</c> if has level up material the specified pet_ob material_list; otherwise, <c>false</c>.</returns>
    /// <param name="pet_ob">Pet ob.</param>
    /// <param name="material_list">Material list.</param>
    public static bool HasLevelUpMaterial(Property pet_ob, List<Property> material_list)
    {
        if(pet_ob == null)
            return false;

        foreach (Property material in material_list)
        {
            if(material == null)
                continue;

            if (MonsterMgr.IsSkillLevelUpMaterial(pet_ob, material))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 执行宠物工坊操作
    /// </summary>
    public static bool DoAction(Property who, string actionName, LPCMapping para)
    {
        Petsmith mod;
        mPetsmithList.TryGetValue(actionName, out mod);

        if (mod == null)
        {
            LogMgr.Trace("工坊子模块{0}不存在。", actionName);
            return false;
        }

        // 调用子模块操作
        return mod.DoAction(who, para);
    }

    /// <summary>
    /// 取得合成数据
    /// </summary>
    /// <returns>The synthesis data.</returns>
    /// <param name="level">Level表示合成等级，默认level=0表示无限制</param>
    public static List<int> GetSynthesisData(int level = 0)
    {
        if (mSynthesisCsv == null)
            return new List<int>();

        List<int> petsList = new List<int>();

        foreach (CsvRow data in mSynthesisCsv.rows)
        {
            if (level > 0 && data.Query<int> ("level") != level)
                continue;
 
            petsList.Add (data.Query<int>("rule"));
        }

        return petsList;
    }

    /// <summary>
    /// 获取宠物的合成材料
    /// </summary>
    /// <returns>The synthesis materials.</returns>
    /// <param name="classId">Class identifier.</param>
    public static LPCArray GetSynthesisMaterials(int classId)
    {
        CsvRow row = mSynthesisCsv.FindByKey (classId);

        if (row == null)
            return null;

        return row.Query<LPCArray> ("material_cost");
    }

    /// <summary>
    /// 是否是基础材料（能不能合成）
    /// </summary>
    /// <returns><c>true</c> if is base material the specified classId; otherwise, <c>false</c>.</returns>
    /// <param name="classId">Class identifier.</param>
    public static bool IsSyntheTarget(int classId)
    {
        return GetSynthesisMaterials(classId) == null;
    }

    /// <summary>
    /// 获取合成对象（最终合成目标）
    /// </summary>
    /// <returns><c>true</c>, if synthe target was gotten, <c>false</c> otherwise.</returns>
    /// <param name="classId">Class identifier.</param>
    public static int GetSyntheTarget(int classId)
    {
        CsvRow petInfo = MonsterMgr.GetRow (classId);

        if (petInfo == null)
            return -1;

        return petInfo.Query<int>("synthe_pet");
    }

    /// <summary>
    /// 检测是否拥有某宠物(包含仓库中的宠物)
    /// </summary>
    /// <returns><c>true</c> if is own monster the specified classId; otherwise, <c>false</c>.</returns>
    /// <param name="classId">Class identifier.</param>
    public static bool IsOwnMonster(Property user, int classId)
    {
        CsvRow data = MonsterMgr.GetRow (classId);

        if (data == null)
            return false;

        // 获取玩家包裹中的宠物列表
        List<Property> userPets = BaggageMgr.GetItemsByPage(user, ContainerConfig.POS_PET_GROUP);

        // 取得玩家仓库中的宠物列表
        List<Property> storePets = BaggageMgr.GetItemsByPage(user, ContainerConfig.POS_STORE_GROUP);

        // 合并list
        userPets.AddRange(storePets);

        foreach (Property pet in userPets)
        {
            if (pet.GetClassID () == classId)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 该宠物进行合成操作
    /// </summary>
    /// <returns><c>true</c> if can synthesis the specified class_id; otherwise, <c>false</c>.</returns>
    /// <param name="class_id">Class identifier.</param>
    public static bool CanDoSynthe(Property user, int class_id)
    {
        LPCArray materialCost = GetSynthesisMaterials (class_id);

        if (materialCost == null || materialCost.Count == 0)
            return false;

        // 获取玩家包裹中的宠物列表
        List<Property> userPets = BaggageMgr.GetItemsByPage(user, ContainerConfig.POS_PET_GROUP);

        // 取得玩家仓库中的宠物列表
        List<Property> storePets = BaggageMgr.GetItemsByPage(user, ContainerConfig.POS_STORE_GROUP);

        // 合并list
        userPets.AddRange(storePets);

        bool isPetsMatch = false;
        bool isFieldsMatch = true;
        LPCMapping condition = new LPCMapping ();

        // 是否拥有满足条件的宠物
        for (int i = 0; i < materialCost.Count; i++)
        {
            isPetsMatch = false;
            condition = materialCost [i].AsMapping;
            foreach (Property pet in userPets)
            {
                isFieldsMatch = true;
                foreach (string field in condition.Keys)
                {
                    if (pet.Query<int> (field) != condition [field].AsInt) 
                    {
                        isFieldsMatch = false;
                        break;
                    }
                }

                if (isFieldsMatch)
                {
                    isPetsMatch = true;
                    break;
                }
            }

            if (! isPetsMatch)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 判断强化材料是否能够使用(前提是已经确定包裹或仓库中包含该强化材料)
    /// </summary>
    /// <returns><c>true</c> if can synthesis the specified class_id; otherwise, <c>false</c>.</returns>
    /// <param name="class_id">Class identifier.</param>
    public static object SlectMaterial(Property user, LPCMapping condition)
    {
        // 获取玩家包裹中的宠物列表
        List<Property> baggagePets = BaggageMgr.GetItemsByPage(user, ContainerConfig.POS_PET_GROUP);

        bool isFieldsMatch = true;
        string tip = string.Empty;
        List<Property> petList = new List<Property> ();

        // 检索玩家包裹
        foreach (Property pet in baggagePets)
        {
            isFieldsMatch = true;
            foreach (string field in condition.Keys)
            {
                if (pet.Query<int> (field) != condition [field].AsInt)
                {
                    isFieldsMatch = false;
                    break;
                }
            }

            if (! isFieldsMatch)
                continue;

            if (! string.IsNullOrEmpty(CheckPetSyntheState (user, pet)))
            {
                tip = CheckPetSyntheState (user, pet);
                continue;
            }

            petList.Add (pet);
        }

        if (petList.Count == 0)
        {
            if (string.IsNullOrEmpty (tip))
                tip = string.Format (LocalizationMgr.Get("PetSynthesisInfoWnd_3"),
                    MonsterMgr.GetName (condition.GetValue<int> ("class_id"), MonsterConst.RANK_AWAKED));
            
                return tip;
        }

        return petList;
    }

    /// <summary>
    /// 检测宠物状态
    /// </summary>
    /// <returns>The pet state.</returns>
    /// <param name="pet_ob">Pet ob.</param>
    public static string CheckPetSyntheState(Property user, Property pet_ob)
    {
        // 玩家的共享宠物 
        LPCValue sharePet = user.Query<LPCValue>("share_pet");

        if (sharePet != null && sharePet.IsString &&
            sharePet.AsString.Equals (pet_ob.GetRid ()))
            return string.Format (LocalizationMgr.Get("PetSynthesisInfoWnd_6"), pet_ob.Short());

        // 获取宠物的锁定信息
        LPCValue isLock = pet_ob.Query<LPCValue>("is_lock");

        if (isLock != null && isLock.IsInt
            && isLock.AsInt != 0)
            return string.Format (LocalizationMgr.Get("PetSynthesisInfoWnd_7"), pet_ob.Short());

        // 获取防守宠物
        LPCValue defensePet = user.Query<LPCValue>("defense_troop");

        if (defensePet != null && defensePet.IsArray
            && defensePet.AsArray.IndexOf(pet_ob.GetRid()) != -1)
            return string.Format (LocalizationMgr.Get("PetSynthesisInfoWnd_8"), pet_ob.Short());

        return string.Empty;
    }

    #endregion
}
