/// <summary>
/// MonsterMgr.cs
/// Copy from zhangyg 2014-10-22
/// 怪物管理
/// </summary>

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using LPC;

/// 怪物管理
public class MonsterMgr
{
    #region 变量

    private static int monsterId = 0;
    private static CsvRow monsterRow = null;

    // 怪物配置表信息
    private static CsvFile mMonsterCsv;

    // 模型体型半径
    private static CsvFile mModelCsv;

    // 配置表信息
    private static LPCMapping mMonsterInitMap = new LPCMapping();

    // 初始化属性列表
    private static List<string> mInitAttribList = new List<string>()
    {
        "max_hp", "max_mp", "attack", "defense", "speed","agility",
    };

    private static Dictionary<int, List<CsvRow>> mMonsterGroupMap = new Dictionary<int, List<CsvRow>>();

    private static Dictionary<int, CsvRow> mMonsterFlagMap = new Dictionary<int, CsvRow>();

    #endregion

    #region 属性

    // 获取怪物配置表信息
    public static CsvFile MonsterCsv { get { return mMonsterCsv; } }

    // 获取怪物初始化配置表信息
    public static LPCMapping MonsterInitCsv { get { return mMonsterInitMap; } }

    #endregion

    #region 内部接口

    // 载入怪物初始化列表
    private static void LoadMonsterInitFile(string fileName)
    {
        // 重置数据
        mMonsterInitMap = LPCMapping.Empty;

        // 载入副本配置表
        CsvFile initCsv = CsvFileMgr.Load(fileName);

        // 载入配置表信息失败
        if (initCsv == null)
            return;

        // 遍历各行数据
        foreach (CsvRow data in initCsv.rows)
        {
            // 转换数据格式为LPC mapping数据格式
            LPCValue dataMap = data.ConvertLpc();

            // 获取初始化规则rule
            string rule = dataMap.AsMapping["rule"].AsString;

            // 批次资源信息初始化
            LPCValue ruleList = LPCValue.CreateArray();
            if (mMonsterInitMap.ContainsKey(rule))
                ruleList.AsArray = mMonsterInitMap[rule].AsArray;

            // 记录数据
            ruleList.AsArray.Add(dataMap);
            mMonsterInitMap.Add(rule, ruleList);
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 重置数据
        mMonsterFlagMap.Clear();

        mMonsterGroupMap.Clear();

        // 验证客户端需要载入模型体型半径
        mModelCsv = CsvFileMgr.Load("model");

        // 载入配置表
        mMonsterCsv = CsvFileMgr.Load("monster");

        foreach (CsvRow row in mMonsterCsv.rows)
        {
            if (row == null)
                continue;

            int group = row.Query<int>("group");

            List<CsvRow> list = null;

            if (!mMonsterGroupMap.TryGetValue(group, out list))
                list = new List<CsvRow>();

            list.Add(row);

            mMonsterGroupMap[group] = list;

            int flag = row.Query<int>("flag");

            if (!mMonsterFlagMap.ContainsKey(flag))
                mMonsterFlagMap.Add(flag, row);
        }

        // 载入怪物初始化列表
        LoadMonsterInitFile("monster_init");
    }

    /// <summary>
    /// 获取怪物关联怪物列表
    /// </summary>
    public static LPCArray GetRelatePetList(int classId)
    {
        // 获取配置信息
        CsvRow data = MonsterCsv.FindByKey(classId);
        if (data == null)
            return LPCArray.Empty;

        // 获取技能信息
        return data.Query<LPCArray>("relate_list");
    }

    /// <summary>
    /// 是否是道具
    /// </summary>
    public static bool IsMonster(Property ob)
    {
        return IsMonster(ob.Query<int>("class_id"));
    }

    /// <summary>
    /// 根据flag获取使魔配置信息
    /// </summary>
    public static CsvRow GetMonsterByFlag(int flag)
    {
        if (mMonsterFlagMap == null)
            return null;

        if (!mMonsterFlagMap.ContainsKey(flag))
            return null;

        return mMonsterFlagMap[flag];
    }

    /// <summary>
    /// 获取模型详细信息
    /// </summary>
    public static CsvRow GetModelInfo(string model)
    {
        // 没有配置信息
        if (mModelCsv == null)
            return null;

        // 返回配置信息
        return mModelCsv.FindByKey(model);
    }

    /// <summary>
    /// 获取相同分组的宠物列表
    /// </summary>
    public static List<CsvRow> GetMonsterListByGroup(int group, bool isCommentMonster = false)
    {
        List<CsvRow> list = null;

        // 没有数据
        if (! mMonsterGroupMap.TryGetValue(group, out list))
            return new List<CsvRow>();

        // 如果不需要过滤推荐怪物
        if (! isCommentMonster)
            return list;

        List<CsvRow> finalList = new List<CsvRow>();

        // 过滤掉不能推荐怪物
        foreach (CsvRow data in list)
        {
            // 如果不需要在图鉴中显示
            if (data.Query<int>("show_in_manual") != 1)
                continue;

            // 添加到列表中
            finalList.Add(data);
        }

        // 返回finalList
        return finalList;
    }

    /// <summary>
    /// 获取模型体型半径
    /// </summary>
    public static float GetModelBodyRange(string model)
    {
        // 没有配置信息
        if (mModelCsv == null)
            return 0f;

        // 模型数据不存在
        CsvRow data = mModelCsv.FindByKey(model);
        if (data == null)
            return 0f;

        // 返回模型体型半径
        return data.Query<float>("body_range");
    }

    /// <summary>
    /// 获取模型动画信息
    /// </summary>
    public static LPCMapping GetModelAnimationInfo(string model, string animationName)
    {
        // 没有配置信息
        if (mModelCsv == null)
            return LPCMapping.Empty;

        // 模型数据不存在
        CsvRow data = mModelCsv.FindByKey(model);
        if (data == null)
            return LPCMapping.Empty;

        // 获取动画信息
        LPCMapping animation = data.Query<LPCMapping>("animation");

        // 返回模型指定动画名详细信息
        return animation.GetValue<LPCMapping>(animationName, LPCMapping.Empty);
    }

    /// <summary>
    /// 是否是道具
    /// </summary>
    public static bool IsMonster(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row == null)
            return false;

        return true;
    }

    /// <summary>
    /// 自动提升各种基础属性
    /// </summary>
    public static void AutoDistributeAttrib(Property ob)
    {
        // 设置角色各基本属性的数值
        foreach (string attrib in mInitAttribList)
        {
            ob.Set(attrib,
                LPCValue.Create(CALC_BASIC_ATTRIB.Call(ob, attrib)));
        }
    }

    /// <summary>
    /// Dos the upgrade.
    /// </summary>
    /// <param name="ob">Ob.</param>
    public static void DoUpgrade(Property ob)
    {
        // 分配属性
        AutoDistributeAttrib(ob);

        // 刷新属性
        PropMgr.RefreshAffect(ob);
    }

    /// <summary>
    /// 获得数据
    /// </summary>
    public static CsvRow GetRow(int classId)
    {
        if (monsterId != classId)
        {
            monsterId = classId;
            monsterRow = MonsterCsv.FindByKey(classId);
        }

        return monsterRow;
    }

    /// <summary>
    /// 获取宠物texture
    /// </summary>
    /// <returns>The pet texture.</returns>
    /// <param name="classId">Class identifier.</param>
    /// <param name="rank">Rank.</param>
    public static Texture2D GetTexture(int classId, int rank)
    {
        // 获取宠物头像
        string textureName = GetIcon(classId, rank);
        if (string.IsNullOrEmpty(textureName))
            return null;

        // 获取资源路径
        string resPath = GetIconResPath(textureName);

        // 宠物头像都不需要卸载
        // 防止界面打开卡的问题
        return ResourceMgr.LoadTexture(resPath, true);
    }

    /// <summary>
    /// 获取模型路径
    /// </summary>
    /// <returns>The model res path.</returns>
    /// <param name="modelName">Model name.</param>
    public static string GetModelResPath(string modelName)
    {
        return string.Format("Assets/Prefabs/Model/{0}.prefab", modelName);
    }

    /// <summary>
    /// 获取宠物头像
    /// </summary>
    /// <returns>The icon res path.</returns>
    /// <param name="iconName">Icon name.</param>
    public static string GetIconResPath(string iconName)
    {
        return string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconName);
    }

    /// <summary>
    /// 获得iconID
    /// </summary>
    public static string GetIcon(int classId, int rank)
    {
        CsvRow row = GetRow(classId);

        string icon = string.Empty;

        if (row == null)
            return string.Empty;

        icon = row.Query<string>("icon");

        // 目前觉醒
        if (rank == MonsterConst.RANK_AWAKED)
            icon = GetAwakeIcon(icon);

        return icon;
    }

    /// <summary>
    /// 获取宠物默认的rank
    /// </summary>
    public static int GetDefaultRank(int classId)
    {
        CsvRow row = GetRow(classId);

        if (row == null)
            return 0;

        return row.Query<int>("rank");
    }

    /// <summary>
    /// 获取宠物默认的star
    /// </summary>
    public static int GetDefaultStar(int classId)
    {
        CsvRow row = GetRow(classId);

        if (row == null)
            return 0;

        return row.Query<int>("star");
    }

    /// <summary>
    /// 根据monster表中配置的图标获取宠物觉醒图标
    /// 目前为默认图标前加"a"
    /// 例如:"8710"则觉醒图标为"a8710"
    /// </summary>
    /// <returns>The awake icon.</returns>
    /// <param name="icon">Icon.</param>
    public static string GetAwakeIcon(string icon)
    {
        return "a" + icon;
    }

    /// <summary>
    /// 判断怪物是否已觉醒
    /// </summary>
    public static bool IsAwaken(Property montser)
    {
        // 如果是已经觉醒的怪物则获取觉醒mine
        return montser.Query<int>("rank") == MonsterConst.RANK_AWAKED;
    }

    /// <summary>
    /// 判断宠物是否可以觉醒
    /// </summary>
    public static bool IsCanAwaken(int class_id)
    {
        // 获取宠物的配置表信息
        CsvRow row = GetRow(class_id);

        if (row == null)
            return false;

        return row.Query<int>("rank") != MonsterConst.RANK_UNABLEAWAKE;
    }

    /// <summary>
    /// 获取宠物名字
    /// </summary>
    /// <returns>The name.</returns>
    /// <param name="classId">Class identifier.</param>
    public static string GetName(int classId, int rank)
    {
        CsvRow row = GetRow(classId);
        if (row == null)
            return string.Empty;

        if(rank == MonsterConst.RANK_AWAKED)
            return LocalizationMgr.Get(row.Query<string>("awake_name"));
        else
            return LocalizationMgr.Get(row.Query<string>("name"));
    }

    /// <summary>
    /// 获取宠物类型
    /// </summary>
    /// <returns>The type.</returns>
    /// <param name="classId">Class identifier.</param>
    public static int GetType(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return row.Query<int>("type");

        return -1;
    }

    /// <summary>
    /// 获取召唤所需的碎片数量
    /// </summary>
    /// <returns>The type.</returns>
    /// <param name="classId">Class identifier.</param>
    public static int GetPieceAmount(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return row.Query<int>("piece_amount");

        return -1;
    }

    /// <summary>
    /// 获取宠物最大等级
    /// </summary>
    /// <param name="item_ob">Item ob.</param>
    public static int GetMaxLevel(Property ob)
    {
        return StdMgr.GetStdAttrib("max_level", ob.GetStar());
    }

    /// <summary>
    /// 获取宠物最大等级
    /// </summary>
    public static int GetMaxLevel(int star)
    {
        return StdMgr.GetStdAttrib("max_level", star);
    }

    /// <summary>
    /// 获取宠物元素
    /// </summary>
    /// <returns>The element.</returns>
    /// <param name="classId">Class identifier.</param>
    public static int GetElement(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return row.Query<int>("element");

        return -1;
    }

    /// <summary>
    /// 获取宠物经验参数
    /// </summary>
    /// <param name="classId"></param>
    /// <returns></returns>
    public static int GetExpArg(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return row.Query<int>("exp_arg");

        return 0;
    }

    /// <summary>
    /// 获取宠物种族
    /// </summary>
    /// <returns>The type.</returns>
    /// <param name="classId">Class identifier.</param>
    public static int GetRace(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return row.Query<int>("race");

        return -1;
    }

    ///<summary>
    /// 获取宠物模型
    ///</summary>
    public static string GetModel(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row != null)
            return row.Query<string>("model");

        return string.Empty;
    }

    /// <summary>
    /// 根据装备位置获取穿戴的宠物对象
    /// </summary>
    public static Property GetPetByEquipPos(Property who, string equipPos)
    {
        return (who as Container).baggage.GetCarryByPos(equipPos);
    }

    /// <summary>
    /// 怪物初始化
    /// </summary>
    public static void InitMonster(Property monster, string rule = "default")
    {
        // 没有此规则的初始化信息
        if (!mMonsterInitMap.ContainsKey(rule))
            return;

        // 获取初始化详细信息
        LPCArray initList = mMonsterInitMap[rule].AsArray;

        // 没有初始化详细信息不处理
        if (initList.Count == 0)
            return;

        LPCMapping data = null;
        object attribValue;
        int scriptNo = 0;

        // 获取召唤者rid
        string summonerRid = monster.Query<string>("summoner_rid", true);
        Property summonerOb = null;
        if (!string.IsNullOrEmpty(summonerRid))
            summonerOb = Rid.FindObjectByRid(summonerRid);

        // 遍历各个属性
        for (int i = 0; i < initList.Count; i++)
        {
            // 获取配置信息
            data = initList[i].AsMapping;

            // 配置的是公式
            if (data["calc_prop"].IsString)
            {
                // 获取公式名
                attribValue = FormulaMgr.InvokeFormulaByName(data["calc_prop"].AsString, monster, data["calc_arg"], summonerOb);

                // 不存在公式
                if (attribValue == null)
                    continue;

            }
            else if (data["calc_prop"].IsInt)
            {
                // 获取计算属性脚本
                scriptNo = data["calc_prop"].AsInt;

                // 不存在脚本
                if (!ScriptMgr.Contains(scriptNo))
                    continue;

                // 计算属性值
                attribValue = ScriptMgr.Call(scriptNo, monster, data["calc_arg"], summonerOb);
            }
            else
            {
                // 暂时不支持其他配置
                continue;
            }

            // 记录数据
            if (data["is_temp"].AsInt == 0)
                monster.Set(data["attrib"].AsString, (attribValue is LPCValue) ?
                            (attribValue as LPCValue) : LPCValue.Create(attribValue));
            else
                monster.SetTemp(data["attrib"].AsString, (attribValue is LPCValue) ?
                                (attribValue as LPCValue) : LPCValue.Create(attribValue));
        }

        // 刷新怪物属性
        PropMgr.RefreshAffect(monster);
    }

    /// <summary>
    /// 获取进化效果描述
    /// </summary>
    public static string GetEvolutionDesc(Property monster)
    {
        // 取技能信息
        CsvRow monsterInfo = GetRow(monster.GetClassID());
        if (monsterInfo == null)
        {
            LogMgr.Trace(string.Format("取不到怪物{0}信息", monster.GetClassID()));
            return string.Empty;
        }

        string desc = string.Empty;

        // 获取技能脚本
        LPCValue arg = monsterInfo.Query<LPCValue>("awake_desc");
        if (arg.IsInt)
            desc = ScriptMgr.Call(arg.AsInt, monster, monsterInfo.Query<LPCValue>("awake_arg")) as string;
        else if (arg.IsString)
            desc = LocalizationMgr.Get(arg.AsString);
        else
            return string.Empty;

        return desc;
    }

    /// <summary>
    /// 是否满级
    /// </summary>
    /// <returns><c>true</c> if is max level the specified monster; otherwise, <c>false</c>.</returns>
    /// <param name="monster">Monster.</param>
    public static bool IsMaxLevel(Property monster)
    {
        if (monster == null)
            return false;

        return (monster.GetLevel() == MonsterMgr.GetMaxLevel(monster)) ? true : false;
    }

    /// <summary>
    /// 是否最大星级
    /// </summary>
    /// <returns><c>true</c> if is max level the specified monster; otherwise, <c>false</c>.</returns>
    /// <param name="monster">Monster.</param>
    public static bool IsMaxStar(Property monster)
    {
        if (monster == null)
            return false;

        return (monster.GetStar() == monster.Query<int>("max_star")) ? true : false;
    }

    /// <summary>
    /// 是否是强化技能升级材料
    /// </summary>
    /// <returns><c>true</c> if is skill level up material the specified pet_ob material; otherwise, <c>false</c>.</returns>
    /// <param name="pet_ob">Pet ob.</param>
    /// <param name="material">Material.</param>
    public static bool IsSkillLevelUpMaterial(Property pet_ob, Property material)
    {
        if (pet_ob == null || material == null)
            return false;

        // 是否是恶魔宝宝
        if (material.Query<int>("can_level_up_skill") == 1)
            return true;

        // 是否是同组宠物
        if (material.Query<int>("group") == pet_ob.Query<int>("group"))
            return true;

        // 检测特殊加强技能宠物
        return (bool)CHECK_INCLUDE_SKILL_LEVELUP_MATERIAL.CALL(pet_ob, material);
    }

    /// <summary>
    /// 是否是合成材料
    /// </summary>
    /// <returns><c>true</c> if is synthesis material; otherwise, <c>false</c>.</returns>
    public static bool IsSyntheMaterial(int classId)
    {
        return PetsmithMgr.GetSyntheTarget(classId) > 0;
    }

    /// <summary>
    /// 取得宠物的坐标偏移
    /// </summary>
    /// <returns>The arrow offset.</returns>
    /// <param name="classId">Class identifier.</param>
    public static float GetArrowOffset(int classId)
    {
        CsvRow row = GetRow(classId);
        if (row == null)
            return 0f;

        LPCValue arrow_offset = row.Query<LPCValue>("arrow_offset");
        if (arrow_offset == null)
            return 0f;

        return arrow_offset.AsFloat;
    }

    /// <summary>
    /// 是否是材料宠物
    /// </summary>
    public static bool IsMaterialMonster(int classId)
    {
        // 获取宠物的配置信息
        CsvRow row = GetRow(classId);
        if (row == null)
            return false;

        int isMaterial = row.Query<int>("is_material");

        return isMaterial.Equals(1);
    }

    /// <summary>
    /// 是否是仓库中的宠物
    /// </summary>
    public static bool IsStorePet(Property who)
    {
        if (who == null)
            return false;

        // 宠物的位置信息
        string pos = who.Query<string>("pos");

        if (string.IsNullOrEmpty(pos))
            return false;

        int x = 0;
        int y = 0;
        int z = 0;
        POS.Read(pos, out x, out y, out z);

        return x.Equals(ContainerConfig.POS_STORE_GROUP);
    }

    #endregion
}
