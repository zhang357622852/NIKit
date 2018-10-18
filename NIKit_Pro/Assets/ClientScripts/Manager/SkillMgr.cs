/// <summary>
/// SkillMgr.cs
/// Created by zhaozy 2014-10-24
/// 技能管理模块
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using LPC;
using UnityEngine;

// 技能管理
public static class SkillMgr
{
    #region 变量

    /// <summary>
    /// 技能配置表信息
    /// </summary>
    private static CsvFile mSkillCsv = new CsvFile("skill");

    /// <summary>
    /// 技能最大等级列表
    /// </summary>
    private static Dictionary<int, int> maxSkillLevelMap = new Dictionary<int, int>();

    #endregion

    #region 属性

    // 技能配置表信息
    public static CsvFile SkillCsv { get { return mSkillCsv; } }

    #endregion

    #region 内部接口

    /// <summary>
    /// 载入技能配置表
    /// </summary>
    private static void LoadSkillCsv(string fileName)
    {
        // 载入技能配置表
        mSkillCsv = CsvFileMgr.Load(fileName);

        // 清除maxSkillLevelMap数据
        maxSkillLevelMap.Clear();

        // 统计技能最大等级
        foreach (CsvRow data in mSkillCsv.rows)
        {
            // 获取该技能技能主要参数
            LPCValue baseMainValue = data.Query<LPCValue>("base_main_value");
            if (! baseMainValue.IsMapping)
                continue;

            // 转换数据格式
            LPCMapping baseMainValueMap = baseMainValue.AsMapping;
            if (baseMainValueMap.Count == 0)
                continue;

            int maxLevel = 0;

            // 获取该技能最大等级
            foreach (int level in baseMainValueMap.Keys)
            {
                // 当前level小于maxLvel
                if (maxLevel >= level)
                    continue;

                // 记录最大等级
                maxLevel = level;
            }

            // 记录该技能最大等级
            maxSkillLevelMap.Add(data.Query<int>("skill_id"), maxLevel);
        }
    }

    #endregion

    #region  公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 载入配置表信息
        LoadSkillCsv("skill");
    }

    // 获取某个技能的详细信息
    public static CsvRow GetSkillInfo(int skillId)
    {
        // 获取配置信息
        return SkillCsv.FindByKey(skillId);
    }

    /// <summary>
    /// 获取技能的相应等级的单条升级效果
    /// </summary>
    public static LPCMapping GetSingleBaseMainValue(int skillId, int level)
    {
        // 获取配置信息
        CsvRow data = GetSkillInfo(skillId);
        if (data == null)
            return null;

        // 获取base_main_value
        LPCMapping value = data.Query<LPCValue>("base_main_value").AsMapping;
        if (value == null)
            return null;

        // 获取配置信息
        return value.GetValue<LPCMapping>(level);
    }

    /// <summary>
    /// 获取技能的相应等级的全部升级效果
    /// </summary>
    public static LPCMapping GetAllBaseMainValue(int skillId)
    {
        // 获取配置信息
        CsvRow data = GetSkillInfo(skillId);
        if (data == null)
            return null;

        // 获取base_main_value
        return data.Query<LPCValue>("base_main_value").AsMapping;
    }

    /// <summary>
    /// 获取技能技能相应类型等级效果汇总数值
    /// </summary>
    public static int GetTotalBaseMainValue(int skillId, int level, int type)
    {
        // 获取配置信息
        CsvRow data = GetSkillInfo(skillId);
        if (data == null)
            return 0;

        // 获取base_main_value
        LPCMapping value = data.Query<LPCValue>("base_main_value").AsMapping;
        if (value == null)
            return 0;

        // 汇总数值
        int totalValue = 0;

        // 遍历数据
        foreach (int key in value.Keys)
        {
            // 等级不对
            if (key > level)
                continue;

            // 数据格式不正确
            if (!value[key].IsMapping)
                continue;

            // 汇总数值
            totalValue += value[key].AsMapping.GetValue<int>(type);
        }

        // 获取配置信息
        return totalValue;
    }

    /// <summary>
    /// 获取技能可选择目标的列表
    /// </summary>
    /// <returns>The can select list.</returns>
    /// <param name="skillId">Skill identifier.</param>
    public static List<Property> GetCanSelectList(Property pet, int skillId)
    {
        CsvRow skillRow = SkillCsv.FindByKey(skillId);
        if (skillRow == null)
        {
            LogMgr.Trace(string.Format("技能{0}不存在", skillId));
            return new List<Property>();
        }

        return (List<Property>)ScriptMgr.Call(skillRow.Query<int>("select_script"), skillId, pet);
    }

    /// <summary>
    /// 抽取技能
    /// </summary>
    /// <returns>The skill.</returns>
    public static bool FetchSkill(Property pet, int inclinationType, LPCMapping extraPara, out LPCMapping fetchRet)
    {
        // 复制LPCMapping
        fetchRet = LPCMapping.Empty;

        // 宠物对象不存在
        if (pet == null)
            return false;

        // 获取玩家全部技能
        LPCArray skills = pet.GetAllSkills();

        // 如果玩家没有技能不处理
        if (skills.Count == 0)
            return false;

        Dictionary<int, List<List<Property>>> allSelectList = new Dictionary<int, List<List<Property>>>();
        Dictionary<int, List<int>> skillIdList = new Dictionary<int, List<int>>();
        Dictionary<int, List<int>> weightList = new Dictionary<int, List<int>>();
        int fetchLevel = 0;
        int maxFetchLevel = 0;
        int fetchWeight = 0;
        CsvRow skillRow = null;
        int weightScript = 0;
        int id;

        // 遍历各个技能
        foreach (LPCValue mks in skills.Values)
        {
            // 获取技能id
            id = mks.AsArray[0].AsInt;

            // 如果不能释放该技能
            if (! CanApplySkill(pet, id))
                continue;

            // 判断当前技能能不能选择到目标
            List<Property> selectList = GetCanSelectList(pet, id);
            if (selectList == null || selectList.Count == 0)
                continue;

            // 获取技能配置信息
            skillRow = SkillCsv.FindByKey(id);
            if (skillRow == null)
                continue;

            // 获取权重脚本
            weightScript = skillRow.Query<int>(inclinationType.ToString());
            if (weightScript == 0)
                continue;

            // 获取脚本编号
            object ret = ScriptMgr.Call(weightScript, pet, skillRow.Query<LPCValue>(string.Format("args_{0}", inclinationType)), inclinationType);
            if (ret == null)
                continue;

            // 目前只是支持mapping和int类型，如果后续有需求在调整
            // 默认抽取等级为0
            if (ret is LPCMapping)
            {
                LPCMapping retMap = ret as LPCMapping;
                fetchLevel = retMap.GetValue<int>("fetch_level");
                fetchWeight = retMap.GetValue<int>("weight");
            }
            else if (ret is int)
            {
                // 默认抽取等级为0
                fetchLevel = 0;
                fetchWeight = (int)ret;
            }
            else
            {
                // 其他格式暂不支持
                continue;
            }

            // 初始化相关抽取等级数据
            if (! weightList.ContainsKey(fetchLevel))
            {
                weightList.Add(fetchLevel, new List<int>());
                skillIdList.Add(fetchLevel, new List<int>());
                allSelectList.Add(fetchLevel, new List<List<Property>>());
            }

            // 添加相关数据
            weightList[fetchLevel].Add(fetchWeight);
            skillIdList[fetchLevel].Add(id);
            allSelectList[fetchLevel].Add(selectList);

            // 记录最大抽取等级
            maxFetchLevel = Math.Max(maxFetchLevel, fetchLevel);
        }

        // 没有任何东西可以抽取
        if (! weightList.ContainsKey(maxFetchLevel))
            return false;

        // 根据权重抽取
        int index = RandomMgr.RandomSelect(weightList[maxFetchLevel]);
        if (index == -1)
            return false;

        // 赋值技能id
        int skillId = skillIdList[maxFetchLevel][index];

        // 获取技能配置信息
        skillRow = SkillCsv.FindByKey(skillId);
        weightScript = skillRow.Query<int>("weight_script");
        if (weightScript == 0)
            return false;

        // 获取脚本参数
        List<Property> finalSelectList = allSelectList[maxFetchLevel][index];

        // 如果是战斗客户端, 则这个地方必定传入了战斗指令输入方式
        // 如果战斗指令输入方式为AIT_LOCK，则走如下流程
        LPCValue imputType = extraPara["imput_type"];
        if (imputType != null && imputType.IsInt && imputType.AsInt == AttackImputType.AIT_LOCK)
        {
            // 如果有锁定攻击目标, 则直接攻击锁定目标
            Property pickOb = Property.FindObjectByUniqueId(extraPara.GetValue<int>("imput_unique_id"));
            if (pickOb == null ||
                ! finalSelectList.Contains(pickOb) ||
                skillId != extraPara.GetValue<int>("imput_skill_id"))
                return false;

            // 战斗指令
            fetchRet.Add("skill_id", skillId);
            fetchRet.Add("pick_rid", pickOb.GetRid());
            fetchRet.Add("imput_type", AttackImputType.AIT_LOCK);

            // 返回抽取成功
            return true;
        }
        else if (! extraPara.ContainsKey("ignore_target_lock"))
        {
            // 如果有锁定攻击目标,则直接攻击锁定目标
            // 自动战斗不可能锁定攻击己方成员对象
            Property lockOb = AutoCombatMgr.LockTargetOb;
            if (lockOb != null &&
                pet.CampId != lockOb.CampId &&
                finalSelectList.Contains(lockOb))
            {
                // 战斗指令
                fetchRet.Add("skill_id", skillId);
                fetchRet.Add("pick_rid", lockOb.GetRid());
                fetchRet.Add("imput_type", AttackImputType.AIT_LOCK);

                // 返回抽取成功
                return true;
            }
        }

        // 如果是战斗客户端, 则这个地方必定传入了战斗指令输入方式
        // 如果战斗指令输入方式为AIT_LOCK，则走如下流程
        if (imputType != null && imputType.IsString)
        {
            // 如果有锁定攻击目标, 则直接攻击锁定目标
            Property pickOb = Property.FindObjectByUniqueId(extraPara.GetValue<int>("imput_unique_id"));
            if (pickOb == null ||
                ! finalSelectList.Contains(pickOb) ||
                skillId != extraPara.GetValue<int>("imput_skill_id"))
                return false;

            // 战斗指令
            fetchRet.Add("skill_id", skillId);
            fetchRet.Add("pick_rid", pickOb.GetRid());
            fetchRet.Add("imput_type", imputType);

            // 返回抽取成功
            return true;
        }
        else if (! extraPara.ContainsKey("ignore_target_lock"))
        {
            // 获取当前副本id
            string instanceId = pet.Query<string>("instance/id");

            // 如果玩家有设置首领攻击策略
            LPCMapping selectMap = AutoCombatSelectTypeMgr.GetSelectMap(ME.user, instanceId);
            if (selectMap != null &&
                selectMap.ContainsKey(InstanceConst.ATTACK_ODER) &&
                pet.CampId == CampConst.CAMP_TYPE_ATTACK)
            {
                // 获取攻击顺序类型列表
                LPCArray typeList = selectMap.GetValue<LPCArray>(InstanceConst.ATTACK_ODER);

                // 遍历各个typeList类型
                for (int i = 0; i < typeList.Count; i++)
                {
                    if (typeList[i] == null || !typeList[i].IsString)
                        continue;

                    CsvRow config = AutoCombatSelectTypeMgr.GetSelectTypeConfig(typeList[i].AsString);
                    if (config == null)
                        continue;

                    // 抽取脚本
                    LPCValue scriptNo = config.Query<LPCValue>("fetch_script");

                    // 没有进入检测脚本
                    if (! scriptNo.IsInt || scriptNo.AsInt == 0)
                        continue;

                    // 调用抽取目标脚本
                    Property ret = ScriptMgr.Call(scriptNo.AsInt, finalSelectList,
                        typeList[i].AsString, config.Query<LPCValue>("fetch_args")) as Property;

                    // 抽取目标失败
                    if (ret == null)
                        continue;

                    // 战斗指令
                    fetchRet.Add("skill_id", skillId);
                    fetchRet.Add("pick_rid", ret.GetRid());
                    fetchRet.Add("imput_type", typeList[i].AsString);
                    return true;
                }
            }
        }

        // 获取脚本参数
        LPCValue scriptArgs = skillRow.Query<LPCValue>("weight_args");
        Dictionary<int, List<Property>> weightMDict = new Dictionary<int, List<Property>>();
        int maxWeight = ConstantValue.NUMBER_INFINITY;
        bool isInit = false;

        // 技能抽取目标权重抽取目标
        foreach (Property target in finalSelectList)
        {
            // 通过脚本计算权重
            int weight = (int) ScriptMgr.Call(weightScript, pet, target, scriptArgs);

            // 初始化数据
            if (! weightMDict.ContainsKey(weight))
                weightMDict.Add(weight, new List<Property>());

            // 添加到列表中
            weightMDict[weight].Add(target);

            // 第一次初始化数据
            if (!isInit)
            {
                maxWeight = weight;
                isInit = true;
            }

            // 如果maxWeight小于该目标的权重
            if (maxWeight >= weight)
                continue;

            // 重置maxWeight
            maxWeight = weight;
        }

        // 如果技能选择权重相同，则需要通过位置权重来抽取优先攻击目标
        Property seletTarget = CALC_AUTO_COMBAT_TARGET_POS_WEIGHT.Call(weightMDict[maxWeight]);

        // 没有选择到目标
        if (seletTarget == null)
            return false;

        // 战斗指令
        fetchRet.Add("skill_id", skillId);
        fetchRet.Add("pick_rid", seletTarget.GetRid());
        fetchRet.Add("imput_type", AttackImputType.AIT_RANDOM);

        // 返回抽取成功
        return true;
    }

    /// <summary>
    /// 获取技能普通效果描述
    /// </summary>
    public static string GetBaseEffectDesc(int mSkillId, int level)
    {
        if (mSkillId <= 0)
            return string.Empty;

        // 取技能信息
        CsvRow skillInfo = SkillMgr.GetSkillInfo(mSkillId);
        if (skillInfo == null)
        {
            LogMgr.Trace(string.Format("取不到技能{0}信息", mSkillId));
            return string.Empty;
        }

        // 获取技能脚本
        int scriptNo = skillInfo.Query<LPCValue>("skill_desc_script").AsInt;
        if (scriptNo <= 0)
            return LocalizationMgr.Get(skillInfo.Query<string>("skill_desc"));

        // 通过脚本计算附加参数
        string desc = ScriptMgr.Call(scriptNo, level, LocalizationMgr.Get(skillInfo.Query<string>("skill_desc")),
                          skillInfo.Query<LPCValue>("skill_desc_arg")) as string;

        return desc;
    }

    /// <summary>
    /// 获取特殊效果描述
    /// </summary>
    /// <returns>The special desc.</returns>
    /// <param name="skillInfo">Skill info.</param>
    public static string GetSkillLevelDesc(int mSkillId, int level)
    {
        if (mSkillId <= 0)
            return string.Empty;

        // 取技能信息
        CsvRow skillInfo = SkillMgr.GetSkillInfo(mSkillId);
        if (skillInfo == null)
        {
            LogMgr.Trace(string.Format("取不到技能{0}信息", mSkillId));
            return string.Empty;
        }

        // 获取技能脚本
        int scriptNo = skillInfo.Query<LPCValue>("skill_lev_desc_script").AsInt;
        if (scriptNo <= 0)
            return string.Empty;

        // 通过脚本计算附加参数
        string desc = ScriptMgr.Call(scriptNo, level,
            skillInfo.Query<LPCValue>("base_main_value"), false) as string;

        return desc;
    }

    /// <summary>
    /// 获取特殊效果描述
    /// </summary>
    /// <returns>The special desc.</returns>
    /// <param name="skillInfo">Skill info.</param>
    public static string GetSingleLevelDesc(int mSkillId, int level)
    {
        if (mSkillId <= 0)
            return string.Empty;

        // 取技能信息
        CsvRow skillInfo = SkillMgr.GetSkillInfo(mSkillId);
        if (skillInfo == null)
        {
            LogMgr.Trace(string.Format("取不到技能{0}信息", mSkillId));
            return string.Empty;
        }

        // 获取技能脚本
        int scriptNo = skillInfo.Query<LPCValue>("skill_lev_desc_script").AsInt;
        if (scriptNo <= 0)
            return string.Empty;

        return  ScriptMgr.Call(scriptNo, level,
            skillInfo.Query<LPCValue>("base_main_value"), true) as string;
    }

    /// <summary>
    /// 获取队长技能效果描述
    /// </summary>
    public static string GetLeaderSkillDesc(Property user)
    {
        if (user == null)
            return string.Empty;

        // 获取队长技能
        LPCMapping skillData = GetLeaderSkill(user);

        if (skillData == null || skillData.Count <= 0)
            return string.Empty;

        string desc = string.Empty;

        foreach (int id in skillData.Keys)
        {
            // 取得技能等级
            int level = user.GetSkillLevel(id);
            desc += GetBaseEffectDesc(id, level);
        }

        return desc;
    }

    /// <summary>
    /// 刷新技能带来的附加属性
    /// </summary>
    public static void RefreshSkillAffect(Property who)
    {
        // 获取角色的技能数据
        LPCArray skills = who.GetAllSkills();
        if (skills == null)
            skills = LPCArray.Empty;

        // 属性列表
        LPCArray allProps = new LPCArray();
        LPCArray leaderSkillProps = LPCArray.Empty;
        CsvRow   skillRow = null;
        LPCValue prop = null;
        object   skillProp;
        LPCArray skillData;
        int      skillId;

        // 收集玩家自身所带技能属性
        foreach (LPCValue mks in skills.Values)
        {
            // 获取技能id
            skillData = mks.AsArray;
            skillId = skillData[0].AsInt;

            // 获取技能配置信息
            skillRow = GetSkillInfo(skillId);

            // 没有配置的技能不处理
            if (skillRow == null)
                continue;

            // 如果玩家不是队长，则不能收集队长技能属性
            bool leaderSkill = IsLeaderSkill(skillId);
            if (! who.IsLeader && leaderSkill)
                continue;

            // 不是数组表示非法配置，不做处理
            prop = skillRow.Query<LPCValue>("prop");

            // 如果是配置固定的属性
            if (prop.IsArray)
            {
                // 如果是队长技能
                if (leaderSkill)
                {
                    leaderSkillProps = prop.AsArray;
                    allProps.Append(leaderSkillProps);
                }
                else
                {
                    allProps.Append(prop.AsArray);
                }

                continue;
            }

            // 需要通过脚本计算
            if (prop.IsInt)
            {
                // 需要通过脚本计算
                skillProp = ScriptMgr.Call(prop.AsInt, who, skillId, skillData[1].AsInt,
                    skillRow.Query<LPCValue>("prop_args"));

                // 数据不对格式不正确
                if (!(skillProp is LPCArray))
                    continue;

                // 如果是队长技能
                if (leaderSkill)
                {
                    leaderSkillProps = skillProp as LPCArray;
                    allProps.Append(leaderSkillProps);
                }
                else
                {
                    allProps.Append(skillProp as LPCArray);
                }

                continue;
            }
        }

        // 刷新技能的属性
        PropMgr.CalcAllProps(who, allProps, "skill");

        // 记录到角色技能属性上
        foreach(LPCValue tProp in leaderSkillProps.Values)
            allProps.Remove(tProp);

        // 记录到角色的技能属性
        who.SetTemp("skill_props", LPCValue.Create(allProps));
    }

    /// <summary>
    /// 获取技能图标
    /// </summary>
    /// <returns>The skill icon.</returns>
    /// <param name="skillId">Skill identifier.</param>
    public static string GetIcon(int skillId)
    {
        CsvRow row = SkillCsv.FindByKey(skillId);
        if (row == null)
            return string.Empty;

        // 返回技能图标
        return row.Query<string>("icon");
    }

    /// <summary>
    /// 获取技能texture
    /// </summary>
    /// <returns>The texture.</returns>
    /// <param name="skillId">Skill identifier.</param>
    public static Texture2D GetTexture(int skillId)
    {
        string textureName = GetIcon(skillId);

        if (string.IsNullOrEmpty(textureName))
            return null;

        string resPath = GetIconResPath(textureName);
        return ResourceMgr.LoadTexture(resPath);
    }

    /// <summary>
    /// 获取icon资源路径
    /// </summary>
    /// <returns>The icon res path.</returns>
    /// <param name="icon">Icon.</param>
    public static string GetIconResPath(string icon)
    {
        return string.Format("Assets/Art/UI/Icon/skill/{0}.png", icon);
    }

    /// <summary>
    /// 获取技能对应的图标路径
    /// </summary>
    /// <returns>The icon res path.</returns>
    /// <param name="skillId">Skill identifier.</param>
    public static string GetIconResPath(int skillId)
    {
        return GetIconResPath(GetIcon(skillId));
    }

    /// <summary>
    /// 获取角色指定位置的技能id
    /// </summary>
    public static int GetSkillByPosType(Property ob, int posType)
    {
        // 获取玩家的技能信息
        LPCArray skills = ob.GetAllSkills();
        int skillId;

        // 遍历数据
        foreach (LPCValue mks in skills.Values)
        {
            // 获取技能id
            skillId = mks.AsArray[0].AsInt;

            // 不是需要的位置技能
            if (GetSkillPosType(skillId) != posType)
                continue;

            // 返回技能id
            return skillId;
        }

        // 没有找到指定位置的技能
        return -1;
    }

    /// <summary>
    /// 获取技能的类型
    /// </summary>
    /// <returns>The skill type.</returns>
    /// <param name="skillId">Skill identifier.</param>
    public static int GetSkillPosType(int skillId)
    {
        CsvRow row = SkillCsv.FindByKey(skillId);
        if (row == null)
            return -1;

        // 返回技能的skill_type
        return row.Query<int>("skill_pos_type");
    }

    /// <summary>
    /// 获取技能的名称
    /// </summary>
    /// <returns>The skill name.</returns>
    /// <param name="skillId">Skill identifier.</param>
    public static string GetSkillName(int skillId)
    {
        CsvRow row = SkillCsv.FindByKey(skillId);
        if (row == null)
            return string.Empty;

        // 返回技能的skill_type
        return LocalizationMgr.Get(row.Query<string>("name"));
    }

    /// <summary>
    /// 初始化实体技能
    /// </summary>
    /// <param name="ob">Ob.</param>
    public static void InitSkill(Property ob)
    {
        // 对象已经不存在
        if (ob == null)
            return;

        // 技能已经初始化过
        if (ob.GetAllSkills().Count != 0)
            return;

        // 获取实体的初始化技能列表
        LPCMapping initSkills = ob.BasicQueryNoDuplicate<LPCMapping>("init_skills");

        // 没有技能列表
        if (initSkills.Count == 0)
            return;

        // 获取该rank阶段技能信息
        LPCArray skills = initSkills.GetValue<LPCArray>(ob.Query<int>("rank"));
        if (skills == null)
            return;

        // 记录技能信息
        foreach (LPCValue mks in skills.Values)
        {
            // 转换数据格式
            LPCArray info = mks.AsArray;

            // 设置技能信息
            ob.skill.Set(info[0].AsInt, info[1].AsInt);
        }
    }

    /// <summary>
    /// 获取技能消耗
    /// </summary>
    public static LPCMapping GetCasTCost(Property who, int skillId)
    {
        // 获取技能配置表
        CsvRow skillData = SkillMgr.GetSkillInfo(skillId);
        if (skillData == null)
            System.Diagnostics.Debug.Assert(false);

        // 获取消耗脚本
        int scriptNo = skillData.Query<int>("cost_script");

        // 没有消耗脚本
        if (scriptNo == 0)
            return new LPCMapping();

        // 调用脚本计算
        object ret = ScriptMgr.Call(scriptNo, who, skillId, skillData.Query<LPCMapping>("cost_arg"), skillData);

        // 脚本异常
        if (ret == null)
            System.Diagnostics.Debug.Assert(false);

        // 返回数据
        return (ret as LPCMapping);
    }

    /// <summary>
    /// 获取技能消耗
    /// </summary>
    public static int GetComboTimes(Property who, int skillId, LPCMapping extraPara)
    {
        // 获取技能配置表
        CsvRow skillData = SkillMgr.GetSkillInfo(skillId);
        if (skillData == null)
            System.Diagnostics.Debug.Assert(false);

        // 获取消耗脚本
        int scriptNo = skillData.Query<int>("combo_script");

        // 没有combo_script脚本, 则默认攻击1次
        if (scriptNo == 0)
            return 1;

        // 调用脚本计算
        object ret = ScriptMgr.Call(scriptNo, who, skillData.Query<LPCMapping>("combo_args"), extraPara);

        // 脚本异常
        if (ret == null)
            System.Diagnostics.Debug.Assert(false);

        // 返回数据
        return (int)ret;
    }

    /// <summary>
    /// 获取角色的队长技能
    /// </summary>
    public static LPCMapping GetLeaderSkill(Property ob)
    {
        if (ob == null)
            return LPCMapping.Empty;

        // 获取玩家当前技能列表
        LPCArray skills = ob.GetAllSkills();
        if (skills == null)
            return LPCMapping.Empty;

        CsvRow skillData = null;
        LPCMapping leaderSkill = LPCMapping.Empty;
        LPCArray skill_info;
        int skillId = 0;

        // 遍历玩家技能数据
        foreach (LPCValue mks in skills.Values)
        {
            // 转换数据格式
            skill_info = mks.AsArray;
            skillId = skill_info[0].AsInt;

            // 获取技能配置表
            skillData = SkillMgr.GetSkillInfo(skillId);
            if (skillData == null)
                continue;

            // 不是队长技能
            if (!IsLeaderSkill(skillId))
                continue;

            // 添加列表
            leaderSkill.Add(skillId, skill_info[1]);
        }

        // 返回队长技能列表
        return leaderSkill;
    }

    /// <summary>
    /// 获取在当前副本中起效的队长技能
    /// </summary>
    public static LPCMapping GetEffectctiveLeaderSkill(Property ob)
    {
        LPCMapping leaderSkill = GetLeaderSkill(ob);
        if (leaderSkill == null || leaderSkill.Count == 0)
            return LPCMapping.Empty;

        LPCMapping skills = LPCMapping.Empty;

        foreach (int skillId in leaderSkill.Keys)
        {
            CsvRow data = GetSkillInfo(skillId);
            if (data == null)
                continue;

            LPCValue scriptNo = data.Query<LPCValue>("prop");
            if (scriptNo == null || ! scriptNo.IsInt)
                continue;

            object skillProp = ScriptMgr.Call(scriptNo.AsInt, ob, skillId, leaderSkill[skillId].AsInt);

            // 数据不对格式不正确
            if (! (skillProp is LPCArray))
                continue;

            if ((skillProp as LPCArray).Count == 0)
                continue;

            skills.Add(skillId, leaderSkill[skillId].AsInt);
        }

        return skills;
    }

    /// <summary>
    /// 判断该技能是否为队长技能
    /// </summary>
    /// <returns><c>true</c> if is leader skill the specified skillId; otherwise, <c>false</c>.</returns>
    /// <param name="skillId">Skill identifier.</param>
    public static bool IsLeaderSkill(int skillId)
    {
        if (skillId <= 0)
            return false;

        return GetFamily(skillId) == SkillType.SKILL_LEADER;
    }

    /// <summary>
    /// 是否是被动技能
    /// </summary>
    /// <returns><c>true</c> if is passive skill the specified skillId; otherwise, <c>false</c>.</returns>
    /// <param name="skillId">Skill identifier.</param>
    public static bool IsPassiveSkill(int skillId)
    {
        if (skillId <= 0)
            return false;

        return GetAttribType(skillId) == SkillType.SKILL_TYPE_PASSIVE;
    }

    /// <summary>
    /// 获取技能类型
    /// </summary>
    /// <returns>The family.</returns>
    /// <param name="skillId">Skill identifier.</param>
    public static int GetFamily(int skillId)
    {
        if (skillId <= 0)
            return -1;

        CsvRow skillData = SkillMgr.GetSkillInfo(skillId);

        int family = skillData.Query<int>("skill_family");

        return family;
    }

    /// <summary>
    /// 获取技能属性
    /// </summary>
    /// <returns>The family.</returns>
    /// <param name="skillId">Skill identifier.</param>
    public static int GetAttribType(int skillId)
    {
        if (skillId <= 0)
            return -1;

        CsvRow skillData = SkillMgr.GetSkillInfo(skillId);

        return skillData.Query<int>("attrib_type");
    }

    /// <summary>
    /// 获取技能的最大等级
    /// </summary>
    /// <returns><c>true</c>, if MAX level was gotten, <c>false</c> otherwise.</returns>
    /// <param name="itemOb">Item ob.</param>
    public static int GetSkillMaxLevel(int skillId)
    {
        int maxLevel = 0;
        if (! maxSkillLevelMap.TryGetValue(skillId, out maxLevel))
            return 0;

        // 返回技能maxLevel
        return maxLevel;
    }

    // 获取某个技能的详细信息
    public static int GetOriginalSkillId(int skillId)
    {
        // 获取配置信息
        CsvRow skillRow = SkillCsv.FindByKey(skillId);
        if (skillRow == null)
            return skillId;

        // 返回配置信息
        return skillRow.Query<int>("original_skill_id");
    }

    /// <summary>
    /// 是否在战斗界面显示该技能
    /// </summary>
    /// <returns><c>true</c> if can show skill the specified who skillId; otherwise, <c>false</c>.</returns>
    /// <param name="who">Who.</param>
    /// <param name="skillId">Skill identifier.</param>
    public static bool CanShowSkill(Property who, int skillId)
    {
        // 目前的正解是通过脚本来判断战斗中是否显示该技能
        CsvRow skillRow = SkillCsv.FindByKey(skillId);

        int canShow = skillRow.Query<int>("can_show");

        // 没有配置不处理
        if (canShow == 1)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 判断某个技能技能是否可以释放
    /// </summary>
    public static bool CanApplySkill(Property who, int skillId, Property targetOb = null)
    {
        // ssource或者target为null
        if (who == null)
            return false;

        // 判断是否为被动技能
        if (SkillMgr.GetAttribType(skillId) == SkillType.SKILL_TYPE_PASSIVE)
            return false;

        // 技能正在cd中，不能作用
        if (CdMgr.SkillIsCooldown(who, skillId))
            return false;

        // 读取skill表中相关技能的信息
        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);
        if (skillInfo == null)
            return false;

        // 不足以支持开销
        if (!who.CanCastCost(skillId))
            return false;

        // 技能释放状态限制不能释放
        if (who.CheckStatus(skillInfo.Query<LPCArray>("status_limit_cast")))
            return false;

        // 获取释放判断条件
        int scriptNo = skillInfo.Query<int>("check_apply_script");
        if (scriptNo == 0)
            return true;

        // 脚本判断不能释放
        return (bool)ScriptMgr.Call(scriptNo, who, skillId, skillInfo.Query<LPCValue>("check_apply_args"), targetOb);
    }

    /// <summary>
    /// 判断技能是否起效
    /// </summary>
    public static bool IsValidSkill(Property who, int skillId, Property targetOb = null)
    {
        if (IsPassiveSkill(skillId))
        {
            // 技能正在cd中，不能作用
            if (CdMgr.SkillIsCooldown(who, skillId))
                return false;
            else
                return true;
        }
        else
        {
            return CanApplySkill(who, skillId, targetOb);
        }
    }

    /// <summary>
    /// 检测宠物是否所有技能都已满级
    /// </summary>
    /// <returns><c>true</c>, if skills full level was checked, <c>false</c> otherwise.</returns>
    /// <param name="pet_ob">Pet ob.</param>
    public static bool CheckSkillsFullLevel(Property pet_ob)
    {
        LPCArray skills = pet_ob.GetAllSkills();
        int skillId = 0;
        LPCArray skill_info;

        // 角色没有技能
        if (skills == null || skills.Count == 0)
            return false;

        // 遍历宠物各个技能
        foreach (LPCValue mks in skills.Values)
        {
            skill_info = mks.AsArray;
            skillId = skill_info[0].AsInt;

            // 判断技能是否已经达到了最高等级
            if (skill_info[1].AsInt < SkillMgr.GetSkillMaxLevel(skillId))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 是否显示技能提示
    /// </summary>
    /// <returns><c>true</c> if is show skill tip the specified skillId; otherwise, <c>false</c>.</returns>
    /// <param name="skillId">Skill identifier.</param>
    public static bool IsShowSkillTip(int skillId)
    {
        CsvRow skillData = SkillMgr.GetSkillInfo(skillId);

        // 普通攻击不需要显示提示
        if (SkillMgr.GetSkillPosType(skillId) == SkillType.SKILL_TYPE_1)
            return false;

        // 二级技能不需要提示
        if (skillData.Query<int>("original_skill_id") != skillId)
            return false;

        return true;
    }

    /// <summary>
    /// 是否是附加技能
    /// </summary>
    public static bool IsAppendSkill(int skillId)
    {
        CsvRow skillData = SkillMgr.GetSkillInfo(skillId);
        if (skillData == null)
            return false;

        // 是否是附加技能
        if (skillData.Query<int>("plus_skill") != 1)
            return false;

        return true;
    }

    /// <summary>
    /// 检查技能是否可以连击
    /// </summary>
    public static bool CanComboAttack(Property ob, int skillId, LPCMapping extraPara)
    {
        CsvRow skillData = SkillMgr.GetSkillInfo(skillId);
        if (skillData == null)
            return false;

        // 没有检查脚本
        int scriptNo = skillData.Query<int>("check_combo_script");
        if (scriptNo == 0)
            return false;

        // 返回脚本判断结果
        return (bool) ScriptMgr.Call(scriptNo, ob, skillId, skillData.Query<LPCValue>("check_combo_args"), extraPara);
    }

    /// <summary>
    /// 获取附加技能
    /// </summary>
    public static LPCArray GetAppendSkill(Property ob)
    {
        if (ob == null)
            return LPCArray.Empty;

        // 获取对象身上所有的技能
        LPCArray skills = ob.GetAllSkills();
        if (skills == null)
            return LPCArray.Empty;

        LPCArray appendSkills = LPCArray.Empty;

        foreach (LPCValue skill in skills.Values)
        {
            if (!skill.IsArray)
                continue;

            int skillId = skill.AsArray[0].AsInt;

            if (!IsAppendSkill(skillId))
                continue;

            appendSkills.Add(skillId);
        }

        // 附加技能列表
        return appendSkills;
    }

    #endregion
}
