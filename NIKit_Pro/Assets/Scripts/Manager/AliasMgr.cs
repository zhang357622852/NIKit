/// <summary>
/// AliasMgr.cs
/// Copy from zhangyg 2014-10-22
/// 全局别名资源管理模块
/// </summary>

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 别名管理
/// </summary>
public class AliasMgr
{
    #region 成员变量

    static string mAliasContent = @"
#define UNDEFINED                   0

#define FALSE                       0
#define TRUE                        1

// 技能AI倾向
#define INCLINATION_ASSISTANT       1
#define INCLINATION_ATTACK          2
#define INCLINATION_RANDOM          3

// 技能位置类型
#define SKILL_TYPE_1                1
#define SKILL_TYPE_2                2
#define SKILL_TYPE_3                3
#define SKILL_TYPE_4                4
#define SKILL_TYPE_5                5
#define SKILL_TYPE_6                6

// 技能类别
#define SKILL_NONE                  0
#define SKILL_NORMAL                1
#define SKILL_ACTIVE                2
#define SKILL_PASSIVE               3
#define SKILL_LEADER                4
#define SKILL_ASSIST                5

// 技能范围类型
#define RANGE_TYPE_NONE             0
#define RANGE_TYPE_SINGLE           1
#define RANGE_TYPE_AOE              2

// 技能属性类型
#define SKILL_TYPE_NORMAL           0
#define SKILL_TYPE_ATTACK           1
#define SKILL_TYPE_ASSIST           2
#define SKILL_TYPE_CURE             3
#define SKILL_TYPE_PASSIVE          4
#define SKILL_TYPE_LEADER           5

// 技能策略倾向类型
#define SKILL_TAC_NORMAL            0
#define SKILL_TAC_ATK               1
#define SKILL_TAC_DISPEL            2
#define SKILL_TAC_CLEAN             3
#define SKILL_TAC_ASSIST            4
#define SKILL_TAC_HEALING           5
#define SKILL_TAC_REVIVE            6

// 技能对象
#define APPLY_ALLIES                1
#define APPLY_OPPONENTS             2
#define APPLY_ALL                   3

// 装备类型
#define WEAPON                      0
#define ARMOR                       1
#define SHOES                       2
#define AMULET                      3
#define NECKLACE                    4
#define RING                        5

// 装备属性类型
#define DEFAULT_PROP_EQUIP_TYPE     7
#define ATK_PROP_EQUIP_TYPE         1
#define DEF_PROP_EQUIP_TYPE         2
#define HP_PROP_EQUIP_TYPE          3
#define SPD_PROP_EQUIP_TYPE         4
#define ACC_PROP_EQUIP_TYPE         5
#define CRT_PROP_EQUIP_TYPE         6

// 装备品质
#define RARITY_WHITE                0
#define RARITY_GREEN                1
#define RARITY_BLUE                 2
#define RARITY_PURPLE               3
#define RARITY_DARKGOLDENROD        4

// 宝石类型
#define GEM_PURPLE                  0
#define GEM_BLUE                    1
#define GEM_GREEN                   2
#define GEM_RED                     3
#define GEM_YELLOW                  4
#define GEM_DIAMOND                 5
#define GEM_SKULL                   6

// 物体类型
#define OBJECT_TYPE_USER            2
#define OBJECT_TYPE_MONSTER         4
#define OBJECT_TYPE_NPC             8
#define OBJECT_TYPE_ITEM            16

// 属性权重
#define PROP_TYPE_NO_MERGE          0
#define PROP_TYPE_BOOL              1
#define PROP_TYPE_WEIGHT            2

// 暴击率、暴击伤害、效果抵抗、反击伤害削弱默认值
#define DEFAULT_DEADLY_VALUE        150
#define DEFAULT_DEADLY_DAMAGE       500
#define DEFAULT_RESIST_VALUE        150
#define DEFAULT_COUNTER_DAMAGE      300

// 属性限制
#define DEFAULT_MAX_DEADLY_RATE     1000

// 药品类型
#define LIFE_DRUG                   0
#define PHY_DEF_DRUG                1
#define MAG_DEF_DRUG                2
#define ATK_DRUG                    3

// 作用空间
#define DIMENSION_COMBAT            0x00000002
#define DIMENSION_NORMAL            0x00000001
#define DIMENSION_ALL               0xFFFFFFFF

// 容器栏分类
#define POS_EQUIP_GROUP             0
#define POS_ITEM_GROUP              1
#define POS_PET_GROUP               2
#define POS_STORE_GROUP             3
#define USER_GROUP(1)               102

// 无限数量
#define NUMBER_INFINITY             -1

// 性别
#define NEUTER                      0
#define MALE                        1
#define FEMALE                      2

// 怪物攻击范围类型
#define STYLE_MONSTER_MELEE         101
#define STYLE_MONSTER_RANGE         102
#define STYLE_MONSTER_LONGRANGE     103

// 怪物系别
#define ELEMENT_NONE                0
#define ELEMENT_FIRE                1
#define ELEMENT_STORM               2
#define ELEMENT_WATER               3
#define ELEMENT_LIGHT               4
#define ELEMENT_DARK                5

// 使魔类型
#define MONSTER_NONE_TYPE           0
#define MONSTER_ATK_TYPE            1
#define MONSTER_DEF_TYPE            2
#define MONSTER_HP_TYPE             3
#define MONSTER_SUP_TYPE            4
#define MONSTER_SPD_TYPE            5

// 进化描述
#define NEW_SKILL                   0
#define ATTRIB_CRT                  1
#define ATTRIB_ACC                  2
#define ATTRIB_RES                  3
#define ATTRIB_AGI                  4
#define UPGRADE_SKILL               5
#define ATTRIB_VAMP                 6

// 使魔种族类型
#define RACE_NONE                   0
#define RACE_HUMAN                  1
#define RACE_MATERIAL               2
#define RACE_NATURE                 3
#define RACE_UNDEAD                 4
#define RACE_MONSTER                5
#define RACE_GOD                    6

// 使魔觉醒材料对应class_id
#define PRI_MS                      50100
#define MED_MS                      50104
#define SEN_MS                      50108
#define PRI_LS                      50112
#define MED_LS                      50113
#define SEN_LS                      50114
#define PRI_DS                      50115
#define MED_DS                      50116
#define SEN_DS                      50117
#define PRI_FS                      50101
#define MED_FS                      50105
#define SEN_FS                      50109
#define PRI_SS                      50102
#define MED_SS                      50106
#define SEN_SS                      50110
#define PRI_WS                      50103
#define MED_WS                      50107
#define SEN_WS                      50111

// 怪物类型，正常，精英，boss
#define MONSTER_NORMAL              1
#define MONSTER_ELITE               2
#define MONSTER_BOSS                3

#define MAX_VALUE                   0x7FFFFFFF

// 副本相关
#define INSTANCE_CREATE_RESOURCE    1
#define INSTANCE_ENTER_MAP          2
#define INSTANCE_CROSS_MAP          3
#define INSTANCE_CLEARANCE          4
#define INSTANCE_PREPARE_COMBAT     5

// 策略类型
#define TACTICS_TYPE_ATTACK          Attack
#define TACTICS_TYPE_ENTER_MAP       EnterMap
#define TACTICS_TYPE_CROSS_MAP       CrossMap

// 状态效果
#define STOP_ACTION                0x0001
#define STOP_MOVE                  0x0002
#define STOP_ALL                   0x0003

// 状态效果
#define TYPE_NONE                  0
#define TYPE_BUFF                  1
#define TYPE_DEBUFF                2

// 回合制状态作用阶段
#define ROUND_TYPE_NONE            0x0000
#define ROUND_TYPE_START           0x0001
#define ROUND_TYPE_END             0x0002

// 元素克制关系
#define ELEMENT_NEUTRAL            0
#define ELEMENT_ADVANTAGE          1
#define ELEMENT_DISADVANTAGE       2

// 除装备外，其余的颜色都用NC
#define NC_WHITE                   201
#define NC_GREEN                   202
#define NC_BLUE                    203
#define NC_RED                     204
#define NC_PURPLE                  205
#define NC_GREY                    206
#define NC_YELLOW                  207
#define NC_DIAMOND                 208

// 装备颜色
#define EC_POOR                    501
#define EC_WHITE                   502
#define EC_BLUE                    503
#define EC_GOLD                    504
#define EC_PURPLE                  505
#define EC_DARKGOLDENROD           506
#define EC_GOD                     507

// 治愈类型
#define CURE_TYPE_MAGIC            0x00001
#define CURE_TYPE_SYNC             0x00002
#define CURE_TYPE_ROUND_MAGIC      0x00004

// 闪避类型
#define DAMAGE_TYPE_DODGE          0x00008

// 伤害类型
#define DAMAGE_TYPE_ATTACK         0x00010
#define DAMAGE_TYPE_BLOCK          0x00020
#define DAMAGE_TYPE_DEADLY         0x00040
#define DAMAGE_TYPE_RESIST         0x00080
#define DAMAGE_TYPE_STRIKE         0x00100
#define DAMAGE_TYPE_IMMUNITY       0x00200
#define DAMAGE_TYPE_INJURY         0x00400
#define DAMAGE_TYPE_THORNS         0x00800
#define DAMAGE_TYPE_TRIGGER        0x01000
#define DAMAGE_TYPE_MP             0x02000
#define DAMAGE_TYPE_ABSORB         0x04000
#define DAMAGE_TYPE_SYNC           0x08000
#define DAMAGE_TYPE_REDUCE         0x10000
#define DAMAGE_TYPE_IGNORE_DEF     0x20000
#define DAMAGE_TYPE_PIERCE         0x40000
#define DAMAGE_TYPE_REFLECT        0x80000
#define DAMAGE_TYPE_CHANGE         0x100000
#define DAMAGE_TYPE_NO_TRAP        0x200000
#define CURE_TYPE_MAX_ATTRIB       0x400000
#define DAMAGE_TYPE_MAX_ATTRIB     0x800000

// 触发类属性
#define HIT                        0x0001
#define DIE                        0x0002
#define CHANGE_STAGE               0x0004
#define ROUND_END                  0x0008
#define APPLY_STATUS               0x0010
#define DEBUFF_BEFORE              0x0020
#define DEBUFF_AFTER               0x0040
#define CAST_SKILL                 0x0080
#define NEAR_DIE                   0x0100
#define ROUND_START                0x0200
#define CURE                       0x0400
#define DAMAGE_BEFORE              0x0800
#define DAMAGE                     0x1000
#define DIE_END                    0x2000
#define ACTIVE                     0x0
#define PASSIVE                    0x1
#define OTHER                      0x0
#define SELF                       0x1

// 普通受创音效类型
#define SOUND_DAMAGE_PHYSIC        1
#define SOUND_DAMAGE_MAGIC         2

// 奖励方式
#define BONUS_DIRECT               0
#define BONUS_AUTO_PICK_UP         1
#define BONUS_ACTIVE_PICK_UP       2
#define BONUS_INSTANCE_SETTLEMENT  3

// 奖励类型
#define BONUS_ATTRIB               0
#define BONUS_PROPERTY             1

//系统帮助信息类型
#define SYSTEM_HELP                0
#define SUIT_HELP                  1
#define SUMMON_RATE_HELP           2

// 副本难度
#define INSTANCE_DIFFICULTY_EASY   1
#define INSTANCE_DIFFICULTY_NORMAL 2
#define INSTANCE_DIFFICULTY_HARD   3

// 地图区域信息
#define NORMAL_MAP                 1
#define ARENA_MAP                  2
#define INSTANCE_MAP_1             3
#define INSTANCE_MAP_2             4
#define INSTANCE_MAP_3             5
#define ARENA_NPC_MAP              6
#define ARENA_REVENGE_MAP          7
#define DUNGEONS_MAP_1             8
#define DUNGEONS_MAP_2             9
#define SECRET_DUNGEONS_MAP        10
#define TOWER_MAP                  11
#define PET_DUNGEONS_MAP           12
#define GUIDE_INSTANCE_MAP         13

// 秘密地下城类型
#define SECRET_DUNGEON             1
#define PET_DUNGEON                2

#define DIST_TYPE_RECOMMAND        0
#define DIST_TYPE_HOT              1
#define DIST_TYPE_ALL              2

// 通用奖励
#define NEW_USER_BONUS             1
#define SIGN_BONUS                 2
#define LEVEL_BONUS                3

// 合成类型
#define SYNTHETIC_TYPE_SINGLE           0
#define SYNTHETIC_TYPE_ALL              1

// 技能升级效果类别
#define SE_DMG                          1
#define SE_CD                           2
#define SE_RATE                         3
#define SE_NUM                          4
#define SE_HP_CURE                      5
#define SE_ROUND                        6
#define SE_VAMP                         7
#define SE_SHIELD                       8
#define SE_MP_CURE                      9
#define SE_MP_COST                      10
#define SE_SUMMON                       11

// 限购类型
#define NO_LIMIT_MARKET                0
#define DAY_CYCLE_MARKET               1
#define WEEK_CYCLE_MARKET              2
#define MONTH_CYCLE_MARKET             3
#define YEAR_CYCLE_MARKET              4
#define ACCOUNT_LIMIT_MARKET           5
#define LEVEL_LIMIT_MARKET             6
#define EACH_LIMIT_MARKET              7
#define NONE_LIMIT_MARKET              8

// 限时礼包类型
#define NORMAL_EXP_PET_TYPE            1
#define SPECIAL_EXP_PET_TYPE           2

// 任务类型
#define DAILY_TASK                      0
#define CHALLENGE_TASK                  1
#define ACHIEVE_TASK                    2
#define RESEARCH_TASK                   3
#define EASY_TASK                       4
#define GROWTH_TASK_LEVEL_STAR          5
#define GROWTH_TASK_SUIT_INTENSIFY      6
#define GROWTH_TASK_AWAKE               7
#define GROWTH_TASK_ARENA               8
#define GROWTH_TASK                     9
#define INVITE_TASK                     10
#define ADVEN_TASK                      11
#define PET_TASK                        12
#define EQPT_TASK                       13
#define ARENA_TASK                      14

// 工坊合成类型常量
#define SYNTHESIS_ITEM                  0x01
#define SYNTHESIS_EQUIP                 0x02

// 阵型站位常量
#define RAW_NONE                        raw_none
#define RAW_FRONT                       raw_front
#define RAW_BACK                        raw_back

// 首领战攻击列表怪物类型
#define BOSS_TYPE                       boss
#define UP_JUSHI_TYPE                   up_jushi
#define DOWN_JUSHI_TYPE                 down_jushi
#define UP_MOYAN_TYPE                   up_moyan
#define DOWN_MOYAN_TYPE                 down_moyan
#define UP_HUNSHI_TYPE                  up_hunshi
#define LEFT_HUNSHI_TYPE                left_hunshi
#define DOWN_HUNSHI_TYPE                down_hunshi
#define RIGHT_HUNSH_TYPE                right_hunshi
#define UP_HEXIN_TYPE                   up_hexin
#define MIDDLE_HEXIN_TYPE               middle_hexin
#define DOWN_HEXIN_TYPE                 down_hexin

// 系统功能显示位置
#define SCREEN_LEFT                     0
#define SCREEN_RIGHT                    1

// 商店商品分组
#define GOLD_COIN_GROUP                 gold_coin_group
#define MONEY_GROUP                     money_group
#define AP_GROUP                        ap_group
#define LIFE_GROUP                      life_group
#define WEEKEND_GIFT_GROUP              weekend_gift
#define DAILY_GIFT_GROUP                daily_gift
#define INTENSIFY_GIFT_GROUP            intensify_gift_group
#define LEVEL_GIFT_GROUP                level_gift

// 道具分组
#define SOUL_M                          soul_m
#define SOUL_F                          soul_f
#define SOUL_S                          soul_s
#define SOUL_W                          soul_w
#define SOUL_L                          soul_l
#define SOUL_D                          soul_d

#define TIP_TYPE_TOWN                   main_town
#define TIP_TYPE_INSTANCE               instance
#define TIP_TYPE_RESOURCE_UPDATE        resource_update

#define SKILL_ATK_TYPE                  1
#define SKILL_DEF_TYPE                  2
#define SKILL_HP_TYPE                   3
#define SKILL_AGI_TYPE                  4
#define SKILL_SUP_TYPE                  5

// 提示按钮
#define RED_POINT                       1
#define NEW_TIPS                        2

";

    static Dictionary<string, object> mAliasTable = new Dictionary<string, object>();

    #endregion

    #region 内部接口

    static AliasMgr()
    {
        Init(mAliasContent);
    }

    static void Init(string aliasContent)
    {
        mAliasTable.Clear();

        string[] lines = GameUtility.Explode(aliasContent, "\n");
        string prefix = "#define";
        int prefix_len = prefix.Length;
        foreach (string line in lines)
        {
            if (!line.StartsWith(prefix))
                continue;

            string text = line.Substring(prefix_len).Trim();
            int first_space_index = text.IndexOf(' ');
            if (first_space_index < 1)
                continue;

            string alias_name = text.Substring(0, first_space_index);
            string alias_text = text.Substring(first_space_index + 1).Trim();
            if (alias_text.Length < 1)
                continue;

            if (alias_text.StartsWith("0x"))
            {
                mAliasTable.Add(alias_name, Convert.ToInt32(alias_text, 16));
                continue;
            }

            if (alias_text.StartsWith("-0x"))
            {
                mAliasTable.Add(alias_name, -Convert.ToInt32(alias_text.Substring(1), 16));
                continue;
            }

            int ch = alias_text[0];
            if (((ch >= '0') && (ch <= '9')) || (ch == '-'))
            {
                mAliasTable.Add(alias_name, Convert.ToInt32(alias_text, 10));
                continue;
            }

            // 整体作为字符
            mAliasTable.Add(alias_name, alias_text.Substring(0, alias_text.Length));
        }
    }

    #endregion

    #region 外部接口

    public static bool ContainsAlias(string alias)
    {
        return mAliasTable.ContainsKey(alias);
    }

    public static object Get(string alias)
    {
        return mAliasTable[alias];
    }

    public static int GetInt(string alias)
    {
        return (int)mAliasTable[alias];
    }

    public static string GetString(string alias)
    {
        return (string)mAliasTable[alias];
    }

    public static IEnumerable<string> Keys
    {
        get
        {
            foreach (string key in mAliasTable.Keys)
            {
                yield return key;
            }
        }
    }

    #endregion
}
