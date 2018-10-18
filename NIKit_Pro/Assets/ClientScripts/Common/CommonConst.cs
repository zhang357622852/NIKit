/// <summary>
/// CommonConst.cs
/// Created by zhaozy 2014-10-24
/// 公共常量声明（类似LPC中的头文件）
/// </summary>

using System.Collections.Generic;
using System.Diagnostics;
using LPC;

/// <summary>
/// 全局常量
/// </summary>
public class ConstantValue
{
    // 最大值
    public const int MAX_VALUE = 0x7FFFFFFF;

    // 表示无限数量
    public const int NUMBER_INFINITY = -1;

    // 场景标准分辨率（16 ： 9）
    // 场景适配方案是通过移动相机距离达到场景适配
    public const float StdAspect = 1.777778f;

    // 游戏Touch位置最大误差
    public const float MOVE_INCH = 0.1f;
}

/// <summary>
/// 阵营相关常量
/// </summary>
public class CampConst
{
    // 无阵营
    public const int CAMP_TYPE_NONE = 0X00;

    // 攻方阵营
    public const int CAMP_TYPE_ATTACK = 0X01;

    // 防守阵营
    public const int CAMP_TYPE_DEFENCE = 0X02;
}

/// <summary>
/// 工坊常量
/// </summary>
public class BlacksmithConst
{
    // 工坊合成类型常量
    public const int SYNTHESIS_ITEM = 0X01;
    public const int SYNTHESIS_EQUIP = 0X02;
}

/// <summary>
/// 回合制战斗相关常量
/// </summary>
public class RoundCombatConst
{
    // 副本战斗
    public const int ROUND_TYPE_INSTANCE = 0X01;

    // 同步对战战斗
    public const int ROUND_TYPE_SYNC = 0X02;

    // 战斗回放
    public const int ROUND_TYPE_PLAYBACK = 0X04;

    // 验证战斗
    public const int ROUND_TYPE_VALIDATION = 0X08;

    // 回合类型
    public const int ROUND_TYPE_NONE = 0;
    public const int ROUND_TYPE_NORMAL = 1;
    public const int ROUND_TYPE_ADDITIONAL = 2;
    public const int ROUND_TYPE_JOINT = 3;
    public const int ROUND_TYPE_COUNTER = 4;
    public const int ROUND_TYPE_RAMPAGE = 5;
    public const int ROUND_TYPE_COMBO = 6;
    public const int ROUND_TYPE_GASSER = 7;
    public const int ROUND_TYPE_WAIT = 8;

    // 结束类型
    public const int END_TYPE_WIN        = 0;
    public const int END_TYPE_GIVEUP     = 1;
    public const int END_TYPE_MAX_ROUNDS = 2;
}

/// <summary>
/// 邮件
/// </summary>
public class ExpressStateType
{
    // 邮件状态标识
    public const int EXPRESS_STATE_SENT = 0;
    public const int EXPRESS_STATE_STORAGE = 1;
    public const int EXPRESS_STATE_INVALID = 2;
    public const int EXPRESS_STATE_READ = 3;
    public const int EXPRESS_STATE_REJECT = 4;


    // 邮件类型
    public const string SYSTEM_EXPRESS = "system";
}

/// <summary>
/// 性别
/// </summary>
public class SexType
{
    // 性别
    public const int MALE = 1;
    public const int FEMALE = 2;
}

/// <summary>
/// Property init map.
/// </summary>
public class PropertyInitConst
{
    // 使魔BasicAttrib属性(与下面的finalAttribs必须顺序对应)
    public static List<string> PetBasicAttribs = new List<string>()
    {
        "max_hp",      "max_mp",        "agility",  "speed",       "attack",        "defense",
        "resist_rate", "accuracy_rate", "crt_rate", "crt_dmg_rate","skill_effect",
    };

    // 使魔OriginalAttrib属性
    public static List<string> PetOriginalAttribs = new List<string>()
    {
        "agility", "max_hp",
    };

    // 属性刷新后处理，目前包括
    // 1. 设置当前hp等需要保证数值没有溢出
    public static List<string> AttribList = new List<string>() { "hp" };

    // 使魔FinalAttribs属性
    public static List<string> FinalAttribs = new List<string>()
    {
        "agility",           "speed",             "attack",           "defense",        "resist_rate",
        "accuracy_rate",     "crt_rate",          "crt_dmg_rate",     "skill_effect",   "tarhp_high_crt_up",
        "tarhp_low_add_dmg", "tar_debuff_dmg_up", "hit_times_dmg_up", "low_hp_crt",     "rate_ignore_block",
        "max_mp",
    };

    // 收集improvement中的相关属性
    public static List<string> ImprovementAttribList = new List<string>()
    {
        "blind_rate",     "block_rate",       "rate_ignore_def",   "more_atk_low_hp",       "atk_up_by_hp",
        "hp_into_attack", "crt_dmg_co_by_hp", "de_damage_rate",    "die_skl_damage_up",     "rate_times_dmg",
        "debuff_atk_up",  "damage_up_rate",   "high_agi_reduce_dmg", "moyan_hp_low_atk_up", "sts_attack_rate",
        "low_hp_dmg_up",  "barasaru",         "mp_reduce_dmg",
    };
}

/// <summary>
/// EventMgr事件列表
/// </summary>
public class EventMgrEventType
{
    // null
    public const int EVENT_NULL = 0;

    // 打击点
    public const int EVENT_HIT = 1;

    // 状态节点
    public const int EVENT_STATUS = 2;

    // 闪避
    public const int EVENT_DODGE = 3;

    // 治疗
    public const int EVENT_RECEIVE_CURE = 4;

    // 受创
    public const int EVENT_RECEIVE_DAMAGE = 5;

    // 登陆成功
    public const int EVENT_LOGIN_OK = 6;

    // 移动节点到达目标
    public const int EVENT_MOVE_ARRIVED = 7;

    // 附加状态事件
    public const int EVENT_APPLY_STATUS = 8;

    // 清除状态事件
    public const int EVENT_CLEAR_STATUS = 9;

    // 序列结束事件（逻辑上的结束，包括打断结束和时间流逝自动结束）
    public const int EVENT_SEQUENCE_END = 10;

    // 死亡事件
    public const int EVENT_DIE = 11;

    // 奖励事件
    public const int EVENT_BONUS = 12;

    // Debug功能，选项窗口宽度改变
    public const int EVENT_DEBUG_OPTION_WND_WIDTH_CHANGE = 13;

    // 进入副本
    public const int EVENT_ENTER_INSTANCE = 15;

    // 包裹请清除新字段
    public const int EVENT_CLEAR_NEW = 17;

    // 断网登出
    public const int EVENT_DISCONNECT_LOGOUT = 18;

    // 网络连接失败
    public const int EVENT_LOGIN_FAILED = 19;

    // 战斗暂停标记改变
    public const int EVENT_COMBAT_PAUSE_CHANGED = 20;

    // 领取邮件奖励
    public const int EVENT_MAIL_TAKE_PROPERTY = 22;

    // 设置系统设置事件
    public const int EVENT_SET_OPTION = 23;

    // 发布道具事件
    public const int EVENT_PUBLISH_ITEM = 24;

    // 战斗准备完毕
    public const int EVENT_READY_COMBAT = 25;

    // 作用回合状态
    public const int EVENT_STATUS_ROUND_UPDATE = 26;

    // 等待输入战斗指令
    public const int EVENT_WAIT_COMMAND = 27;

    // 回合制战斗结束
    public const int EVENT_ROUND_COMBAT_END = 28;

    // 回合结束
    public const int EVENT_ROUND_END = 30;

    // LOADING END
    public const int EVENT_LOADING_END = 31;

    // 副本闪屏
    public const int EVENT_INSTANCE_SPLASH = 32;
    public const int EVENT_INSTANCE_SPLASH_END = 33;

    public const int EVENT_GUIDE_INSTANCE_END = 36;
    public const int EVENT_INSTANCE_CLEARANCE = 37;

    // 图标点击(按压)事件
    public const int EVENT_CLICK_PICTURE = 38;

    // Action治疗
    public const int EVENT_CURE = 39;

    // Action受创
    public const int EVENT_DAMAGE = 40;

    // 召唤成功事件
    public const int EVENT_SUMMON_SUCCESS = 41;

    // 是否复活提示事件;
    public const int EVENT_REVIVE_TIPS = 42;

    // 装备强化事件
    public const int EVENT_EQUIP_STRENGTHEN = 44;

    // 商城购买道具成功事件
    public const int EVENT_BUY_ITEM_SUCCESS = 45;

    // 宠物升级事件
    public const int EVENT_PET_UPGRADE = 46;

    // 宠物升星事件
    public const int EVENT_PET_STARUP = 47;

    // 精髓合成事件
    public const int EVENT_SYNTHESIS = 48;

    // CONTAINER打开事件
    public const int EVENT_CONTAINER_OPEN = 49;

    // 宠物觉醒事件
    public const int EVENT_PET_AWAKE = 50;

    // 搜索玩家详细信息成功事件
    public const int EVENT_SEARCH_DETAIL_INFO_SUCC = 51;

    // 收索玩家简单信息成功事件
    public const int EVENT_SEARCH_BAGGAGE_INFO_SUCC = 52;

    // 转换副本阶段事件
    public const int EVENT_INSTANCE_CHANGE_STATE = 54;

    // 系统公告事件
    public const int EVENT_SYSTEM_AFFICHE = 55;

    // hit事件
    public const int EVENT_ATTACK = 56;

    // 好友操作结果事件
    public const int EVENT_FRIEND_OPERATE_DONE = 57;

    // 好友请求事件
    public const int EVENT_FRIEND_REQUEST = 58;

    // 好友列表变化事件
    public const int EVENT_FRIEND_NOTIFY_LIST = 59;

    // 宠物合成事件
    public const int EVENT_PET_SYNTHESIS = 60;

    // 点击主界面小窗口
    public const int EVENT_CLICK_SECNE_WND = 61;

    // 点击地图小窗口
    public const int EVENT_CLICK_MAP_WND = 62;

    // 资源更新界面闪屏完成
    public const int EVENT_RES_LOADING_WND_SPLASH_OK = 63;

    // 活动列表通知事件
    public const int EVENT_NOTIFY_ACTIVITY_LIST = 64;

    // 副本通关宝箱开启事件
    public const int EVENT_BOX_OPEN_FINISH = 65;

    // clearance数据更新事件
    public const int EVENT_CLEARANCE_DATA_UPDATE = 66;

    // 聊天公告道具点击事件
    public const int EVENT_CHAT_ITEM_CLICK = 67;

    // 竞技场结算完毕通知
    public const int EVENT_NOTIFY_SETTLEMENT_FINISH = 68;

    // 关闭强化窗口通知
    public const int EVENT_CLOSE_EQUIP_STRENTHEN = 69;

    // 获取排行榜数据事件
    public const int EVENT_GET_ARENA_TOP_LIST = 70;

    // 副本通关宝箱下落完成事件
    public const int EVENT_BOX_FALL_FINISH = 71;

    // 回合开始事件
    public const int EVENT_ROUND_START = 72;

    // 通天之塔场景切换事件
    public const int EVENT_OPEN_TOWER_SCENE = 73;

    // 通天塔关闭事件
    public const int EVENT_CLOSE_TOWER_SCENE = 74;

    // 通天塔切换难度事件
    public const int EVENT_SWITCH_TOWER_DIFFICULTY = 75;

    // 通天塔滑动事件
    public const int EVENT_TOWER_SLIDE = 76;

    // 获取通天塔排行榜数据事件
    public const int EVENT_GET_TOWER_TOP_LIST = 77;

    // 获取通天塔使魔排行榜数据事件
    public const int EVENT_GET_TOWER_PET_TOP_LIST = 78;

    // 聊天频道宠物基础对象点击事件
    public const int EVENT_CHAT_MONSTER_CLICK = 79;

    // 聊天频道宠物基础对象点击事件
    public const int EVENT_REGISTER_ACCOUNT_SUCC = 80;

    // 功能按钮解锁事件
    public const int EVENT_FUNCTION_BUTTON_UNLOCK = 81;

    // 副本新关卡开始事件
    public const int EVENT_INSTANCE_LEVEL_START = 83;

    // 结算奖励显示完成
    public const int EVENT_SETTLEMENT_BONUS_SHOW_FINISH = 84;

    // 战斗窗口打开事件
    public const int EVENT_OPEN_COMBATWND = 85;

    // 装备强化完成
    public const int EVENT_EQUIP_STRENGTHEN_FINISH = 86;

    // 指引返回操作
    public const int EVENT_GUIDE_RETUEN_OPERATE = 87;

    // 宠物强化升级动画播放完成事件
    public const int EVENT_UPGREDE_ANIMATION_FINISH = 88;

    // 玩家升级事件
    public const int EVENT_USER_LEVEL_UP = 89;

    // 结算界面失败动画播放完成
    public const int EVENT_FAILED_ANIMATION_FINISH = 90;

    public const int EVENT_SHOW_EQUIP = 91;

    // 使魔强化弹框点击事件
    public const int EVENT_STRENGTHEN_PET_DIALOG_CLICK = 92;

    // 装备强化金钱不足事件
    public const int EVENT_EQUIP_INTENSIY_NO_MONEY = 93;

    // 快速强化达到最大次数
    public const int EVENT_FAST_INTENFISY_COUNT_LIMIT= 94;

    // 进入副本事件
    public const int EVENT_ENTER_INSTANCE_OK = 95;

    // 评论操作结果事件
    public const int EVENT_COMMENT_OPERATE_DONE = 96;

    // 启动logo动画完成事件
    public const int EVENT_MSG_START_ANIMATION_FINISH = 97;

    // 战场回合开始事件
    public const int EVENT_COMBAT_ROUND_START = 98;

    // 刷新成长手册任务提示事件
    public const int EVENT_REFRESH_GROWTH_TASK_TIPS = 99;

    // 获取公会成员列表事件
    public const int EVENT_GET_GANG_MEMBER_LIST = 100;

    // 转让公会会长成功事件
    public const int EVENT_TRANSFER_LEADER = 101;

    // 任命副会长成功事件
    public const int EVENT_TRANSFER_DEPUTY_LEADER = 102;

    // 获取公会数据
    public const int EVENT_NOTIFY_GANG_INFO = 103;

    // 修改公会信息事件
    public const int EVENT_CHANGE_GANG_INFO = 104;

    // 获取所有请求数据事件
    public const int EVENT_ALL_GANG_REQUEST = 105;

    // 创建公会成功事件
    public const int EVENT_CRETAE_GANG_SUCCESS = 106;

    // 申请公会成功刷新事件
    public const int EVENT_APPLICATION_GANG_SUCCESS = 107;

    // 移除公会成员成功
    public const int EVENT_REMOVE_GANG_MEMBER = 108;

    // 同意申请成功
    public const int EVENT_ACCEPT_GANG_REQUEST = 109;

    // 取消公会申请
    public const int EVENT_CANCEl_GANG_REQUEST = 110;

    // 获取公会详情事件
    public const int EVENT_GET_GANG_DETAILS = 111;

    // 发送公会宣传事件
    public const int EVENT_SEND_GANG_SLOGAN = 111;

    // 聊天公会按钮点击事件
    public const int EVENT_GANG_BUTTON_CLICK = 112;

    // 修改公会旗帜事件
    public const int EVENT_CHANGE_GANG_FLAG = 113;

    // 刷新邮件列表事件
    public const int EVENT_REFRESH_EXPRESS_LIST = 114;

    // 回放结束事件
    public const int EVENT_PLAYBACK_END = 115;

    // 视频发布事件
    public const int EVENT_PUBLISH_VIDEO = 116;

    // 查看战斗回放点击事件
    public const int EVENT_VIEW_COMBAT_VIDEO = 117;

    // 获取视频详细信息事件
    public const int EVENT_VIDEO_DETAILS = 118;

    // 播放视频事件
    public const int EVENT_PLAY_VIDEO = 119;

    // 分享视频事件
    public const int EVENT_SHARE_VIDEO = 120;

    // 刷新回放推荐列表事件
    public const int EVENT_REFRESH_PLAYBACK_LIST = 121;

    // 查询发布视频列表
    public const int EVNT_QUERY_PUBLISH_VIDEO_LIST = 122;

    // 完成任务事件
    public const int EVENT_COMPLETE_TASK = 123;

    // 进入副本失败事件
    public const int EVENT_ENTER_INSTANCE_FAIL = 124;

    // 死亡结束事件
    public const int EVENT_DIE_ACTION_END = 125;
    public const int EVENT_DIE_END = 126;

    //许愿动画结束
    public const int EVENT_LOTTERY_BONUS_ANI_DONE = 127;

    // 分享成功事件
    public const int EVENT_SHARE_SUCCESS = 128;

    // 关闭销毁窗口事件
    public const int EVENT_DESTROY_WINDOW = 129;

    // 发布推荐邀请人id
    public const int EVENT_PUBLISH_INVITE_ID = 130;

    // 装备快速字段检索列表
    private static Dictionary<string, int> mEventAliasMap = new Dictionary<string, int>()
    {
        { "EVENT_NULL",                             EVENT_NULL },
        { "EVENT_HIT",                              EVENT_HIT },
        { "EVENT_STATUS",                           EVENT_STATUS },
        { "EVENT_DODGE",                            EVENT_DODGE },
        { "EVENT_RECEIVE_CURE",                     EVENT_RECEIVE_CURE },
        { "EVENT_RECEIVE_DAMAGE",                   EVENT_RECEIVE_DAMAGE },
        { "EVENT_CURE",                             EVENT_CURE },
        { "EVENT_DAMAGE",                           EVENT_DAMAGE },
        { "EVENT_LOGIN_OK",                         EVENT_LOGIN_OK },
        { "EVENT_MOVE_ARRIVED",                     EVENT_MOVE_ARRIVED },
        { "EVENT_APPLY_STATUS",                     EVENT_APPLY_STATUS },
        { "EVENT_CLEAR_STATUS",                     EVENT_CLEAR_STATUS },
        { "EVENT_SEQUENCE_END",                     EVENT_SEQUENCE_END },
        { "EVENT_DIE",                              EVENT_DIE },
        { "EVENT_BONUS",                            EVENT_BONUS },
        { "EVENT_DEBUG_OPTION_WND_WIDTH_CHANGE",    EVENT_DEBUG_OPTION_WND_WIDTH_CHANGE },
        { "EVENT_ENTER_INSTANCE",                   EVENT_ENTER_INSTANCE },
        { "EVENT_CLEAR_NEW",                        EVENT_CLEAR_NEW },
        { "EVENT_DISCONNECT_LOGOUT",                EVENT_DISCONNECT_LOGOUT },
        { "EVENT_LOGIN_FAILED",                     EVENT_LOGIN_FAILED },
        { "EVENT_COMBAT_PAUSE_CHANGED",             EVENT_COMBAT_PAUSE_CHANGED },
        { "EVENT_MAIL_TAKE_PROPERTY",               EVENT_MAIL_TAKE_PROPERTY },
        { "EVENT_INSTANCE_SPLASH",                  EVENT_INSTANCE_SPLASH },
        { "EVENT_INSTANCE_SPLASH_END",              EVENT_INSTANCE_SPLASH_END },
        { "EVENT_READY_COMBAT",                     EVENT_READY_COMBAT },
        { "EVENT_INSTANCE_CHANGE_STATE",            EVENT_INSTANCE_CHANGE_STATE },
        { "EVENT_INSTANCE_LEVEL_START",             EVENT_INSTANCE_LEVEL_START },
        { "EVENT_SETTLEMENT_BONUS_SHOW_FINISH",     EVENT_SETTLEMENT_BONUS_SHOW_FINISH },
        { "EVENT_USER_LEVEL_UP",                    EVENT_USER_LEVEL_UP },
        { "EVENT_INSTANCE_CLEARANCE",               EVENT_INSTANCE_CLEARANCE },
        { "EVENT_FAILED_ANIMATION_FINISH",          EVENT_FAILED_ANIMATION_FINISH },
        { "EVENT_COMPLETE_TASK",                    EVENT_COMPLETE_TASK },
        { "EVENT_DIE_ACTION_END",                   EVENT_DIE_ACTION_END },
    };

    /// <summary>
    /// 根据别名获取EventType
    /// </summary>
    public static int GetEventTypeByAlias(string typeAlias)
    {
        int eventId = EVENT_NULL;
        if (!mEventAliasMap.TryGetValue(typeAlias, out eventId))
            eventId = EVENT_NULL;

        // 返回eventid
        return eventId;
    }
}

/// <summary>
/// 场景常量
/// </summary>
public class SceneConst
{
    public const string SCENE_START = "Start";
    public const string SCENE_LOGIN = "Login";
    public const string SCENE_SUMMON = "Summon";
    public const string SCENE_MAIN_CITY = "MainCity";
    public const string SCENE_WORLD_MAP = "WorldMap";
    public const string SCENE_COMBAT = "Combat";
    public const string SCENE_LOTTERY_BONUS = "LotteryBonus";
}

/// <summary>
/// 触发属性常量
/// </summary>
public class TriggerPropConst
{
    public const int HIT           = 0x0001;
    public const int DIE           = 0x0002;
    public const int CHANGE_STAGE  = 0x0004;
    public const int ROUND_END     = 0x0008;
    public const int APPLY_STATUS  = 0x0010;
    public const int DEBUFF_BEFORE = 0x0020;
    public const int DEBUFF_AFTER  = 0x0040;
    public const int CAST_SKILL    = 0x0080;
    public const int NEAR_DIE      = 0x0100;
    public const int ROUND_START   = 0x0200;
    public const int CURE          = 0x0400;
    public const int DAMAGE_BEFORE = 0x0800;
    public const int DAMAGE        = 0x1000;
    public const int DIE_END       = 0x2000;

    // 触发属性类型（触动触发还是被动触发）
    public const int ACTIVE = 0x0;
    public const int PASSIVE = 0x1;

    // 触发属性作用目标（自身，反弹其他角色）
    public const int OTHER = 0x0;
    public const int SELF = 0x1;
}

/// <summary>
/// 受创类型
/// </summary>
public class CombatConst
{
    // 治愈类型
    public const int CURE_TYPE_MAGIC       = 0x00001; // 魔法治愈
    public const int CURE_TYPE_SYNC        = 0x00002; // 治愈同步
    public const int CURE_TYPE_ROUND_MAGIC = 0x00004; // 回合治愈类型

    // 闪避类型
    public const int DAMAGE_TYPE_DODGE     = 0x00008; // 闪避类型

    // 以位的方式记录各伤害类型，以便各种伤害类型进行组合
    public const int DAMAGE_TYPE_ATTACK     = 0x00010;  // 物理伤害
    public const int DAMAGE_TYPE_BLOCK      = 0x00020;  // 格挡
    public const int DAMAGE_TYPE_DEADLY     = 0x00040;  // 暴击伤害
    public const int DAMAGE_TYPE_RESIST     = 0x00080;  // 抵抗
    public const int DAMAGE_TYPE_STRIKE     = 0x00100;  // 强击
    public const int DAMAGE_TYPE_IMMUNITY   = 0x00200;  // 免疫
    public const int DAMAGE_TYPE_INJURY     = 0x00400;  // 持续伤害
    public const int DAMAGE_TYPE_THORNS     = 0x00800;  // 反弹
    public const int DAMAGE_TYPE_TRIGGER    = 0x01000;  // 触发伤害
    public const int DAMAGE_TYPE_MP         = 0x02000;  // 减少能量
    public const int DAMAGE_TYPE_ABSORB     = 0x04000;  // 吸收
    public const int DAMAGE_TYPE_SYNC       = 0x08000;  // 同步伤害
    public const int DAMAGE_TYPE_REDUCE     = 0x10000;  // 减伤伤害
    public const int DAMAGE_TYPE_IGNORE_DEF = 0x20000;  // 无视防御
    public const int DAMAGE_TYPE_PIERCE     = 0x40000;  // 穿刺攻击（无视防御、护盾、无敌）
    public const int DAMAGE_TYPE_REFLECT    = 0x80000;  // 反伤
    public const int DAMAGE_TYPE_CHANGE     = 0x100000; // 伤害转换
    public const int DAMAGE_TYPE_NO_TRAP    = 0x200000; // 无视陷阱

    // 最大属性治愈和伤害类型
    public const int CURE_TYPE_MAX_ATTRIB   = 0x400000; // 最大属性治愈
    public const int DAMAGE_TYPE_MAX_ATTRIB = 0x800000; // 最大属性伤害

    // 暴击率、暴击伤害、效果抵抗、反击伤害削弱默认值
    public const int DEFAULT_DEADLY_VALUE = 150;
    public const int DEFAULT_DEADLY_DAMAGE = 500;
    public const int DEFAULT_RESIST_VALUE = 150;
    public const int DEFAULT_COUNTER_DAMAGE = 300;
    public const int DEFAULT_MAX_HP_DMG = 400;

    // 属性限制
    public const int DEFAULT_MAX_DEADLY_RATE = 1000;

    // 属性克制影响
    public const int RESTRAIN_DEADLY_VALUE = 150;
    public const int RESTRAIN_BLOCK_VALUE = 500;
    public const int RESTRAIN_ACCURACY_VALUE = 150;
    public const int RESTRAIN_DAMAGE_VALUE = 100;
    public const int RESTRAIN_STRIKE_VALUE = 300;

    // 普通受创音效类型
    public const int SOUND_DAMAGE_PHYSIC = 1;
    public const int SOUND_DAMAGE_MAGIC = 2;

    // 过图回血百分比
    public const int DEFAULT_CROSS_MAP_CURE = 200;
}

/// <summary>
///帮助信息ID
/// </summary>
public class HelpConst
{
    public const int MAIN_CITY_ID = 1;
    public const int MAP_ID = 2;
    public const int DUNGEONS_ID = 3;
    public const int COMBAT_DESC_ID = 4;
    public const int STATS_ID = 5;
    public const int ELEMENT_ID = 6;
    public const int GET_PET_ID = 7;
    public const int SUMMON_ID = 8;
    public const int PET_STAR_ID = 9;
    public const int PET_STRENGTH_ID = 10;
    public const int PET_RANK_ID = 11;
    public const int PET_STAR_UP_ID = 12;
    public const int EQUIP_ID = 13;
    public const int SUIT_ID = 14;
    public const int FRIEND_ID = 15;
    public const int TOWER_ID = 16;
    public const int EQUIP_INTENSIFY_ID = 17;

}

///<summary>
/// loading界面类型
///</summary>
public class LoadingType
{
    public const int LOAD_TYPE_INSTANCE = 0;
    public const int LOAD_TYPE_LOGIN = 1;
    public const int LOAD_TYPE_PVP = 2;
    public const int LOAD_TYPE_SPE_INSTANCE = 3;

    public const int LOAD_TYPE_UPDATE_RES = 4;
}

///<summary>
/// Resloading界面类型
///</summary>
public class ResourceLoadingConst
{
    public const int LOAD_TYPE_START_DECOMPRESS = 0;
    public const int LOAD_TYPE_UPDATE = 1;
    public const int LOAD_TYPE_DECOMPRESS = 2;
    public const int LOAD_TYPE_INIT = 3;
}

///<summary>
/// Resloading界面类型
///</summary>
public class ResourceLoadingStateConst
{
    public const int LOAD_STATE_CHECK = 0;
    public const int LOAD_STATE_UPDATE = 1;
}

/// <summary>
/// 状态清除类型
/// </summary>
public class StatusConst
{
    // 状态结束方式
    public const int CLEAR_TYPE_BREAK = 0x0001;
    public const int CLEAR_TYPE_END_SELF = 0x0002;
    public const int CLEAR_TYPE_EXCLUSION = 0x0004;

    // 附加状态效果
    public const int STOP_ACTION = 0x0001;
    public const int STOP_MOVE = 0x0002;
    public const int STOP_ALL = 0x0003;

    // 状态类型
    public const int TYPE_NONE = 0;
    public const int TYPE_BUFF = 1;
    public const int TYPE_DEBUFF = 2;

    // 回合制状态作用阶段
    public const int ROUND_TYPE_NONE = 0x0000;
    public const int ROUND_TYPE_START = 0x0001;
    public const int ROUND_TYPE_END = 0x0002;
}

/// <summary>
/// 策略相关常量
/// </summary>
public class TacticsConst
{
    // 策略类型
    public const string TACTICS_TYPE_ATTACK = "Attack";
    public const string TACTICS_TYPE_ENTER_MAP = "EnterMap";
    public const string TACTICS_TYPE_CROSS_MAP = "CrossMap";
}

/// <summary>
/// 奖励方式
/// </summary>
public class BonusConst
{
    // 奖励方式
    public const int BONUS_DIRECT = 0;
    public const int BONUS_AUTO_PICK_UP = 1;
    public const int BONUS_ACTIVE_PICK_UP = 2;
    public const int BONUS_INSTANCE_SETTLEMENT = 3;

    // 奖励类型
    public const int BONUS_ATTRIB = 0;
    public const int BONUS_PROPERTY = 1;

    // 掉落方式
    public const int BONUS_DROP_TYPE_DAMAGE = 0;
    public const int BONUS_DROP_TYPE_DIE = 1;
}

/// <summary>
/// 心跳相关常量定义
/// </summary>
public class HeartbeatConst
{
    public const float SAFE_INTERVAL = 15.0f;
    public const float MIN_INTERVAL = 0.02f;
    public const float MIN_WAKEUP_TIME = 0.02f;

    // 状态心跳间隔默认时间
    public const float STATUS_HEARTBEAT_INTERVAL = 0.05f;

    // 心跳类型
    public const int HEARTBEAT_DEFAULT = 0;
    // 默认心跳
    public const int HEARTBEAT_STATUS = 1;
    // 状态心跳
    public const int HEARTBEAT_TACTICS = 2;
    // 策略心跳
    public const int HEARTBEAT_HALO = 3;
    // 光环心跳

    // 心跳类型类表
    public static List<int> HeartbeatTypeList = new List<int>()
    {
        HEARTBEAT_DEFAULT,
        HEARTBEAT_STATUS,
        HEARTBEAT_TACTICS,
        HEARTBEAT_HALO,
    };
}

/// <summary>
/// 物件类型定义
/// </summary>
public class ObjectType
{
    public const int OBJECT_TYPE_NONE = 0x0000;
    public const int OBJECT_TYPE_USER = 0x0002;
    public const int OBJECT_TYPE_MONSTER = 0x0004;
    public const int OBJECT_TYPE_NPC = 0x0008;
    public const int OBJECT_TYPE_ITEM = 0x0010;
    public const int OBJECT_TYPE_INSTANCE = 0x0020;
    public const int OBJECT_TYPE_EQUIP = 0x0040;
}

/// <summary>
/// 地图相关常量
/// </summary>
public class MapConst
{
    // 地图区域信息
    public const int INVALID_MAP = 0;
    public const int NORMAL_MAP = 1;
    public const int ARENA_MAP = 2;
    public const int INSTANCE_MAP_1 = 3;
    public const int INSTANCE_MAP_2 = 4;
    public const int INSTANCE_MAP_3 = 5;
    public const int ARENA_NPC_MAP = 6;
    public const int ARENA_REVENGE_MAP = 7;
    public const int DUNGEONS_MAP_1 = 8;
    public const int DUNGEONS_MAP_2 = 9;
    public const int SECRET_DUNGEONS_MAP = 10;
    public const int TOWER_MAP = 11;
    public const int PET_DUNGEONS_MAP = 12;
    public const int GUIDE_INSTANCE_MAP = 13;

    // NPC地图id
    public const int ARENA_NPC_MAP_ID = 8;

    // 动态圣域类型
    public const int SECRET_DUNGEON = 1;
    public const int PET_DUNGEON = 2;
}

/// <summary>
/// 副本相关常量
/// </summary>
public class InstanceConst
{
    // 副本事件
    public const int INSTANCE_CREATE_RESOURCE = 1;
    public const int INSTANCE_ENTER_MAP = 2;
    public const int INSTANCE_CROSS_MAP = 3;
    public const int INSTANCE_CLEARANCE = 4;
    public const int INSTANCE_PREPARE_COMBAT = 5;

    // 副本闪屏事件
    public const int ENTER_MAP = 0;
    public const int CROSS_MAP = 1;

    // 副本难度
    public const int INSTANCE_DIFFICULTY_EASY = 1;
    public const int INSTANCE_DIFFICULTY_NORMAL = 2;
    public const int INSTANCE_DIFFICULTY_HARD = 3;

    // 首领战攻击列表怪物类型
    public const string BOSS_TYPE = "boss";
    public const string UP_JUSHI_TYPE = "up_jushi";
    public const string DOWN_JUSHI_TYPE = "down_jushi";
    public const string UP_MOYAN_TYPE = "up_moyan";
    public const string DOWN_MOYAN_TYPE = "down_moyan";
    public const string UP_HUNSHI_TYPE = "up_hunshi";
    public const string LEFT_HUNSHI_TYPE = "left_hunshi";
    public const string DOWN_HUNSHI_TYPE = "down_hunshi";
    public const string RIGHT_HUNSH_TYPE = "right_hunshi";
	public const string UP_HEXIN_TYPE = "up_hexin";
	public const string MIDDLE_HEXIN_TYPE = "middle_hexin";
	public const string DOWN_HEXIN_TYPE = "down_hexin";

    // 首领战策略
    public const string MANUAL_OPREATION = "manual_opre";
    public const string AUTO_COMBAT = "auto_combat";
    public const string ATTACK_ODER = "atk_oder";

    // 战场回合提示阶段
    public const int FIRST_STAGE = 50;
    public const int SECOND_STAGE = 30;
    public const int THIRD_STAGE = 20;
    public const int FOURTH_STAGE = 10;

    // 精英圣域层数对应中文名
    public static Dictionary<string,string> eliteLayerMap = new Dictionary<string, string>()
    {
        { "elite_1", LocalizationMgr.Get("elite_layer_1") },
        { "elite_2", LocalizationMgr.Get("elite_layer_2") },
        { "elite_3", LocalizationMgr.Get("elite_layer_3") },
        { "elite_4", LocalizationMgr.Get("elite_layer_4") },
        { "elite_5", LocalizationMgr.Get("elite_layer_5") },
        { "elite_6", LocalizationMgr.Get("elite_layer_6") },
        { "elite_7", LocalizationMgr.Get("elite_layer_7") },
        { "elite_8", LocalizationMgr.Get("elite_layer_8") },
        { "elite_9", LocalizationMgr.Get("elite_layer_9") },
        { "elite_10", LocalizationMgr.Get("elite_layer_10") }
    };
}

/// <summary>
/// 阵型常量类型
/// </summary>
public class FormationConst
{
    // 站位前后排标识
    public const string RAW_NONE = "raw_none";
    public const string RAW_FRONT = "raw_front";
    public const string RAW_BACK = "raw_back";

    /// <summary>
    /// 全部站位列表
    /// </summary>
    public static List<string> ALL_RAW_TYPE = new List<string>() {
        RAW_NONE,
        RAW_FRONT,
        RAW_BACK
    };

    /// <summary>
    /// raw id映射表
    /// </summary>
    private static Dictionary<string, int> RAW_MAP = new Dictionary<string, int>()
    {
        { RAW_NONE, 0 },
        { RAW_FRONT, 1 },
        { RAW_BACK, 2 },
    };

    /// <summary>
    /// 获取raw id
    /// </summary>
    public static int GetRawID(string raw)
    {
        // 没有该信息
        if (!RAW_MAP.ContainsKey(raw))
            return 0;

        // 返回id信息
        return RAW_MAP[raw];
    }
}

/// <summary>
/// 怪物类型
/// </summary>
public class MonsterConst
{
    // 怪物角色阶位(觉醒)
    public const int RANK_UNABLEAWAKE = 0;
    public const int RANK_UNAWAKE = 1;
    public const int RANK_AWAKED = 2;

    // 怪物类型
    public const int STYLE_MONSTER_MELEE = 101;
    public const int STYLE_MONSTER_RANGE = 102;
    public const int STYLE_MONSTER_LONGRANGE = 103;

    // 怪物技能类型
    public const int SKILL_ATK_TYPE = 1;
    public const int SKILL_DEF_TYPE = 2;
    public const int SKILL_HP_TYPE = 3;
    public const int SKILL_AGI_TYPE = 4;
    public const int SKILL_SUP_TYPE = 5;

    //怪物元素类型
    public const int ELEMENT_NONE = 0;
    public const int ELEMENT_FIRE = 1;
    public const int ELEMENT_STORM = 2;
    public const int ELEMENT_WATER = 3;
    public const int ELEMENT_LIGHT = 4;
    public const int ELEMENT_DARK = 5;

    // 使魔类型
    public const int MONSTER_NONE_TYPE = 0;
    public const int MONSTER_ATK_TYPE = 1;
    public const int MONSTER_DEF_TYPE = 2;
    public const int MONSTER_HP_TYPE = 3;
    public const int MONSTER_SUP_TYPE = 4;
    public const int MONSTER_SPD_TYPE = 5;

    // 排序方式
    public const int SORT_BY_LEVEL = 0;
    public const int SORT_BY_STAR = 1;
    public const int SORT_BY_PROPERTY = 2;
    public const int SORT_BY_LATELY = 3;

    public static Dictionary<int, int> sortElement = new Dictionary<int, int>()
    {
        { ELEMENT_LIGHT, 5 },
        { ELEMENT_DARK, 4 },
        { ELEMENT_FIRE, 3 },
        { ELEMENT_STORM, 2 },
        { ELEMENT_WATER, 1 },
        { ELEMENT_NONE, 0},
    };

    // 进化描述
    public const int NEW_SKILL = 0;
    public const int ATTRIB_CRT = 1;
    public const int ATTRIB_ACC = 2;
    public const int ATTRIB_RES = 3;
    public const int ATTRIB_AGI = 4;
    public const int UPGRADE_SKILL = 5;
    public const int ATTRIB_VAMP = 6;

    public static Dictionary<int,string> awakeDescMap = new Dictionary<int, string>()
    {
        { NEW_SKILL, LocalizationMgr.Get("new_skill") },
        { ATTRIB_CRT, LocalizationMgr.Get("attrib_crt") },
        { ATTRIB_ACC, LocalizationMgr.Get("attrib_acc") },
        { ATTRIB_RES, LocalizationMgr.Get("attrib_res") },
        { ATTRIB_AGI, LocalizationMgr.Get("attrib_agi") },
        { UPGRADE_SKILL, LocalizationMgr.Get("upgrade_skill") },
        { ATTRIB_VAMP, LocalizationMgr.Get("attrib_vamp") },
    };

    // 使魔类型对应中文名
    public static Dictionary<int,string> MonsterStyleTypeMap = new Dictionary<int, string>()
    {
        { MONSTER_NONE_TYPE, LocalizationMgr.Get("MonsterType_none") },
        { MONSTER_ATK_TYPE,LocalizationMgr.Get("MonsterType_atk") },
        { MONSTER_DEF_TYPE, LocalizationMgr.Get("MonsterType_def") },
        { MONSTER_HP_TYPE,LocalizationMgr.Get("MonsterType_hp") },
        { MONSTER_SUP_TYPE,LocalizationMgr.Get("MonsterType_sup") },
        { MONSTER_SPD_TYPE,LocalizationMgr.Get("MonsterType_spd") },
    };

    // 使魔元素类型对应中文名
    public static Dictionary<int,string> MonsterElementTypeMap = new Dictionary<int, string>()
    {
        { ELEMENT_FIRE , LocalizationMgr.Get("PetManualWnd_4") },
        { ELEMENT_STORM, LocalizationMgr.Get("PetManualWnd_5") },
        { ELEMENT_WATER, LocalizationMgr.Get("PetManualWnd_6") },
        { ELEMENT_LIGHT, LocalizationMgr.Get("PetManualWnd_7") },
        { ELEMENT_DARK,  LocalizationMgr.Get("PetManualWnd_8") },
    };

    // 使魔元素类型对应中文名
    public static Dictionary<int,string> elementTypeMap = new Dictionary<int, string>()
    {
        { ELEMENT_FIRE , LocalizationMgr.Get("element_info_FIRE") },
        { ELEMENT_STORM, LocalizationMgr.Get("element_info_STORM") },
        { ELEMENT_WATER, LocalizationMgr.Get("element_info_WATER") },
        { ELEMENT_LIGHT, LocalizationMgr.Get("element_info_LIGHT") },
        { ELEMENT_DARK,  LocalizationMgr.Get("element_info_DARK") },
    };

    // 使魔元素类型对应图片名称
    public static Dictionary<int,string> MonsterElementSpriteMap = new Dictionary<int, string>()
    {
        { ELEMENT_FIRE , "pet_fire" },
        { ELEMENT_STORM, "pet_wind" },
        { ELEMENT_WATER, "pet_water" },
        { ELEMENT_LIGHT, "pet_light" },
        { ELEMENT_DARK,  "pet_dark" },
    };

    // 使魔元素类型对应颜色
    public static Dictionary<int,string> MonsterElementColorMap = new Dictionary<int, string>()
    {
        { ELEMENT_FIRE , "[FF6F6F]" },
        { ELEMENT_STORM, "[FDDD59]" },
        { ELEMENT_WATER, "[5991FD]" },
        { ELEMENT_LIGHT, "[FFFADB]" },
        { ELEMENT_DARK,  "[E856FF]" },
    };

    // 使魔元素类型对应颜色
    public static Dictionary<int, UnityEngine.Color> MonsterElementRGBColorMap = new Dictionary<int, UnityEngine.Color>()
    {
        { ELEMENT_FIRE , new UnityEngine.Color(255f / 255f, 111f / 255f, 111f / 255f) },
        { ELEMENT_STORM, new UnityEngine.Color(253f / 255f, 221f / 255f, 89f / 255f) },
        { ELEMENT_WATER, new UnityEngine.Color(89f / 255f, 145f / 255f, 253f / 255f) },
        { ELEMENT_LIGHT, new UnityEngine.Color(255f / 255f, 250f / 255f, 219f / 255f) },
        { ELEMENT_DARK,  new UnityEngine.Color(232f / 255f, 86f / 255f, 255f / 255f) },
    };

    // 使魔种族类型
    public const int RACE_NONE = 0;
    public const int RACE_HUMAN = 1;
    public const int RACE_MATERIAL = 2;
    public const int RACE_NATURE = 3;
    public const int RACE_UNDEAD = 4;
    public const int RACE_MONSTER = 5;
    public const int RACE_GOD = 6;

    // 使魔种族类型对应中文名
    public static Dictionary<int,string> MonsterRaceTypeMap = new Dictionary<int, string>()
    {
        { RACE_NONE, LocalizationMgr.Get("RaceNone") },
        { RACE_HUMAN,LocalizationMgr.Get("RaceHuman") },
        { RACE_MATERIAL, LocalizationMgr.Get("RaceMaterial") },
        { RACE_NATURE,LocalizationMgr.Get("RaceNature") },
        { RACE_UNDEAD,LocalizationMgr.Get("RaceUndead") },
        { RACE_MONSTER,LocalizationMgr.Get("RaceMonster") },
        { RACE_GOD,LocalizationMgr.Get("RaceGod") },
    };

    // 觉醒材料对应class_id
    public const int PRI_MS = 50100;    // 初级魔力魂石
    public const int MED_MS = 50104;    // 中级魔力魂石
    public const int SEN_MS = 50108;    // 高级魔力魂石
    public const int PRI_LS = 50112;    // 初级光明魂石
    public const int MED_LS = 50113;    // 中级光明魂石
    public const int SEN_LS = 50114;    // 高级光明魂石
    public const int PRI_DS = 50115;    // 初级黑暗魂石
    public const int MED_DS = 50116;    // 中级黑暗魂石
    public const int SEN_DS = 50117;    // 高级黑暗魂石
    public const int PRI_FS = 50101;    // 初级火之魂石
    public const int MED_FS = 50105;    // 中级火之魂石
    public const int SEN_FS = 50109;    // 高级火之魂石
    public const int PRI_SS = 50102;    // 初级风之魂石
    public const int MED_SS = 50106;    // 中级风之魂石
    public const int SEN_SS = 50110;    // 高级风之魂石
    public const int PRI_WS = 50103;    // 初级水之魂石
    public const int MED_WS = 50107;    // 中级水之魂石
    public const int SEN_WS = 50111;    // 高级水之魂石
}

// 觉醒相关常量
public class AwakeConst
{
    // 觉醒消耗品类型
    public const int MATERIAL_AWAKE = 0;
    public const int ITEM_AWAKE     = 1;
}

public class ElementConst
{
    // 元素克制关系
    public const int ELEMENT_NEUTRAL = 0;
    public const int ELEMENT_ADVANTAGE = 1;
    public const int ELEMENT_DISADVANTAGE = 2;

    // 元素克制二维表0 相同或无; 1 克制; 2 被克制,
    // 如ELEMENT_RESTRAINT_ARRAY[1,3]表示ELEMENT_FIRE对ELEMENT_WATER的克制关系
    // 值为2，表示被克制
    public static readonly int[,] ELEMENT_COUNTER_ARRAY = new int[,]
    {
        { 0, 0, 0, 0, 0, 0 },
        { 0, 0, 1, 2, 0, 0 },
        { 0, 2, 0, 1, 0, 0 },
        { 0, 1, 2, 0, 0, 0 },
        { 0, 0, 0, 0, 0, 1 },
        { 0, 0, 0, 0, 1, 0 },
    };
}

/// <summary>
/// 技能效果类型
/// </summary>
public class SkillType
{
    // 技能AI倾向
    public const int INCLINATION_ASSISTANT = 0;
    public const int INCLINATION_ATTACK = 1;
    public const int INCLINATION_RANDOM = 2;

    // 技能位置类型
    public const int SKILL_TYPE_1 = 1;
    public const int SKILL_TYPE_2 = 2;
    public const int SKILL_TYPE_3 = 3;
    public const int SKILL_TYPE_4 = 4;
    public const int SKILL_TYPE_5 = 5;
    public const int SKILL_TYPE_6 = 6;

    // 技能类别
    public const int SKILL_NONE = 0;
    public const int SKILL_NORMAL = 1;
    public const int SKILL_ACTIVE = 2;
    public const int SKILL_PASSIVE = 3;
    public const int SKILL_LEADER = 4;
    public const int SKILL_ASSIST = 5;

    // 技能范围类型
    public const int RANGE_TYPE_NONE = 0;
    public const int RANGE_TYPE_SINGLE = 1;
    public const int RANGE_TYPE_AOE = 2;

    // 技能效果类别
    // 技能升级伤害加成（攻速、防御、体力、对方体力等等）
    public const int SE_DMG = 1;
    // 技能升级冷却时间
    public const int SE_CD = 2;
    // 技能升级几率
    public const int SE_RATE = 3;
    // 技能升级目标数量
    public const int SE_NUM = 4;
    // 技能升级治疗加成
    public const int SE_HP_CURE = 5;
    // 技能升级状态持续回合
    public const int SE_ROUND = 6;
    // 技能升级伤害吸血百分比加成
    public const int SE_VAMP = 7;
    // 技能升级护盾百分比加成
    public const int SE_SHIELD = 8;
    // 技能升级能量恢复加成
    public const int SE_MP_CURE = 9;
    // 技能升级能量恢复加成
    public const int SE_MP_COST = 10;
    // 技能升级召唤物属性加成
    public const int SE_SUMMON = 11;

    // 技能类别名称
    public static Dictionary<int,string> SkillFamilyMap = new Dictionary<int, string>()
    {
        { SKILL_NONE, LocalizationMgr.Get("SkillFamilyNone") },
        { SKILL_NORMAL,LocalizationMgr.Get("SkillFamilyNormal") },
        { SKILL_ACTIVE,LocalizationMgr.Get("SkillFamilyActive") },
        { SKILL_PASSIVE, LocalizationMgr.Get("SkillFamilyPassive") },
        { SKILL_LEADER,LocalizationMgr.Get("SkillFamilyLeader") },
        { SKILL_NORMAL,LocalizationMgr.Get("SkillFamilyAssist") },

    };

    // 技能属性类型
    public const int SKILL_TYPE_NORMAL = 0;
    public const int SKILL_TYPE_ATTACK = 1;
    public const int SKILL_TYPE_ASSIST = 2;
    public const int SKILL_TYPE_CURE = 3;
    public const int SKILL_TYPE_PASSIVE = 4;
    public const int SKILL_TYPE_LEADER = 5;

    // 属性类型名称
    public static Dictionary<int,string> SkillTypeMap = new Dictionary<int, string>()
    {
        { SKILL_TYPE_NORMAL, LocalizationMgr.Get("SkillTypeNormal") },
        { SKILL_TYPE_ATTACK,LocalizationMgr.Get("SkillTypeAttack") },
        { SKILL_TYPE_ASSIST, LocalizationMgr.Get("SkillTypeAssist") },
        { SKILL_TYPE_CURE, LocalizationMgr.Get("SkillTypeCure") },
        { SKILL_TYPE_PASSIVE,LocalizationMgr.Get("SkillTypePassive") },
        { SKILL_TYPE_LEADER,LocalizationMgr.Get("SkillTypeLeader") },
    };

    // 技能策略倾向类型
    public const int SKILL_TAC_NORMAL = 0;
    public const int SKILL_TAC_ATK = 1;
    public const int SKILL_TAC_DISPEL = 2;
    public const int SKILL_TAC_CLEAN = 3;
    public const int SKILL_TAC_ASSIST = 4;
    public const int SKILL_TAC_HEALING = 5;
    public const int SKILL_TAC_REVIVE = 6;
}

public class EquipConst
{
    // 装备类型(武器, 护甲, 鞋子, 护符, 项链, 戒指)
    public const int WEAPON = 0;
    public const int ARMOR = 1;
    public const int SHOES = 2;
    public const int AMULET = 3;
    public const int NECKLACE = 4;
    public const int RING = 5;

    // 装备属性类型
    public const int ATTACK = 1;
    public const int DEFENSE = 2;
    public const int MAX_HP = 3;

    public const int ATTACK_RATE = 5;
    public const int DEFENSE_RATE = 6;
    public const int MAX_HP_RATE = 7;

    // 效果命中+%
    public const int ACCURACY_RATE = 10;
    // 效果抵抗+%
    public const int RESIST_RATE = 11;

    // 敏捷+
    public const int AGILITY = 8;
    // 暴击伤害+%
    public const int CRT_DMG_RATE = 13;
    // 暴击率+%
    public const int CRT_RATE = 12;

    // 装备星级
    public const int STAR_1 = 1;
    public const int STAR_2 = 2;
    public const int STAR_3 = 3;
    public const int STAR_4 = 4;
    public const int STAR_5 = 5;
    public const int STAR_6 = 6;

    // 装备属性类型
    public const int MAIN_PROP = 0;
    public const int MINOR_PROP = 1;
    public const int PREFIX_PROP = 2;
    public const int SUIT_PROP = 3;

    // 装备品质
    public const int RARITY_WHITE = 0;
    public const int RARITY_GREEN = 1;
    public const int RARITY_BLUE = 2;
    public const int RARITY_PURPLE = 3;
    public const int RARITY_DARKGOLDENROD = 4;

    public static Dictionary<int, int> sortEquipType = new Dictionary<int, int>()
    {
        {WEAPON,   5},
        {ARMOR,    4},
        {SHOES,    3},
        {AMULET,   2},
        {NECKLACE, 1},
        {RING,     0},
    };

    public static Dictionary<int, string> EquipTypeToName = new Dictionary<int, string>()
    {
        {WEAPON, LocalizationMgr.Get("equip_weapon")},
        {ARMOR, LocalizationMgr.Get("equip_armor")},
        {SHOES, LocalizationMgr.Get("equip_shoes")},
        {AMULET, LocalizationMgr.Get("equip_amulet")},
        {NECKLACE, LocalizationMgr.Get("equip_necklace")},
        {RING, LocalizationMgr.Get("equip_ring")},
    };

    public static List<int> EquipStarList = new List<int>()
    {
        1, 2, 3, 4, 5, 6
    };

    public static List<int> EquipTypeList = new List<int>()
    {
        0, 1, 2, 3, 4, 5
    };
}

/// <summary>
/// 定义prop类型
/// </summary>
public class PropType
{
    // 初始化属性值类型
    public const int PROP_INIT_MAX = 0;
    public const int PROP_INIT_RANDOM = 1;
    public const int PROP_INIT_MIN = 2;

    // -1代表全部主属性
    public static Dictionary<string, int> MainPropNameToId = new Dictionary<string, int>()
    {
        {LocalizationMgr.Get("FilterWnd_0"), -1},
        {LocalizationMgr.Get("FilterWnd_1"), 8},
        {LocalizationMgr.Get("FilterWnd_2"), 12},
        {LocalizationMgr.Get("FilterWnd_3"), 13},
        {LocalizationMgr.Get("FilterWnd_4"), 5},
        {LocalizationMgr.Get("FilterWnd_5"), 6},
        {LocalizationMgr.Get("FilterWnd_6"), 7},
        {LocalizationMgr.Get("FilterWnd_7"), 11},
        {LocalizationMgr.Get("FilterWnd_8"), 10},
        {LocalizationMgr.Get("FilterWnd_9"), 1},
        {LocalizationMgr.Get("FilterWnd_10"), 2},
        {LocalizationMgr.Get("FilterWnd_11"), 3},
    };

    // -1代表全部次要属性
    public static Dictionary<string, int> MinorPropNameToId = new Dictionary<string, int>()
    {
        {LocalizationMgr.Get("FilterWnd_21"), -1},
        {LocalizationMgr.Get("FilterWnd_12"), 8},
        {LocalizationMgr.Get("FilterWnd_13"), 12},
        {LocalizationMgr.Get("FilterWnd_14"), 13},
        {LocalizationMgr.Get("FilterWnd_4"), 5},
        {LocalizationMgr.Get("FilterWnd_5"), 6},
        {LocalizationMgr.Get("FilterWnd_6"), 7},
        {LocalizationMgr.Get("FilterWnd_15"), 11},
        {LocalizationMgr.Get("FilterWnd_16"), 10},
        {LocalizationMgr.Get("FilterWnd_9"), 1},
        {LocalizationMgr.Get("FilterWnd_10"), 2},
        {LocalizationMgr.Get("FilterWnd_11"), 3},
    };
}

// 窗口互斥组
public class WindowMutexGrop
{
    // 主窗口
    public const string MAIN_WND = "main";
}

// 正在打开的窗口分组
public class WindowOpenGroup
{
    // 单个窗口分组
    public const string SINGLE_OPEN_WND = "single_open";

    // 多个窗口分组
    public const string MULTIPLE_OPEN_WND = "multiple_open";
}

/// <summary>
/// 容器相关的配置信息
/// </summary>
public class ContainerConfig
{
    /// <summary>
    /// 容器栏分类
    /// </summary>
    public const int POS_EQUIP_GROUP = 0;
    public const int POS_ITEM_GROUP = 1;
    public const int POS_PET_GROUP = 2;
    public const int POS_STORE_GROUP = 3;

    /// <summary>
    /// 通用仓库
    /// </summary
    public const int NORMAL_STORE = 101;

    // 仓库宠物页面
    public const int STORE_PET_GROUP = 102;

    public static bool IS_ITEM_POS(string pos)
    {
        return IsPage(pos, POS_ITEM_GROUP);
    }

    public static bool IS_PET_POS(string pos)
    {
        return IsPage(pos, POS_PET_GROUP);
    }

    public static bool IS_EQUIP_POS(string pos)
    {
        return IsPage(pos, POS_EQUIP_GROUP);
    }

    public static bool IS_STORE_POS(string pos)
    {
        return IsPage(pos, POS_STORE_GROUP);
    }

    // 判断某位置是不是属于某页面
    private static bool IsPage(string pos, int page)
    {
        int x, y, z;
        if (!POS.Read(pos, out x, out y, out z) || x != page)
            return false;
        return true;
    }
}

/// <summary>
/// 战斗系统中对象方向的定义，目前只有左右
/// </summary>
public class ObjectDirection2D
{
    // 朝右
    public const int RIGHT = 1;

    // 朝左
    public const int LEFT = -1;

    /// <summary>
    /// 根据别名获取ObjectDirection2D
    /// </summary>
    public static int GetDirection2DByAlias(string typeAlias)
    {
        // 朝左
        if (string.Equals(typeAlias, "LEFT"))
            return LEFT;

        // 朝右
        return RIGHT;
    }
}

/// <summary>
/// 战斗相关配置
/// </summary>
public class CombatConfig
{
    // 技能文件名通配字段
    public const string SKILL_ACTION_FILENAME = "skill_action_*.xml";

    // 动画基本层
    public const string ANIMATION_BASE_LAYER = "state.";
    public const int ANIMATION_BASE_LAYER_INEDX = 0;

    // 变色器基础数据
    public const string CCID_PRE = "CCid_";
    public const string CCID_BASE = "CCid_base";

    // 默认动画
    public const string DEFAULT_PLAY = "idle";

    // 光效前缀
    public const string EFF_PRE_ARROW = "Arrow_";
    public const string EFF_PRE_SPRITE = "Effect_";
}

/// <summary>
/// 窗口深度
/// </summary>
public class WindowDepth
{
    const int max_depth = 99999999;

    // 版本提示
    public const int VersionTip = max_depth + 3;

    // 断开连接
    public const int Disconnect = max_depth + 1;

    // 错误提示
    public const int Notify = max_depth;

    // 确认框
    public const int Dialog = max_depth - 1;

    // 带描述的购买弹框
    public const int BuyDialog = max_depth - 2;

    // 显示奖励窗口占2个
    public const int ShowBonusWnd = max_depth - 7;

    // 玩家升级效果弹框
    public const int UpgradeEffect = max_depth - 3;
}

/// <summary>
/// 道具类型.
/// 类型值需要与配置表里面的item_type字段值一致
/// </summary>
public class ItemType
{
    // 药品
    public const int ITEM_TYPE_DRUG = 2;

    // 碎片
    public const int ITEM_TYPE_SYNTHESIS = 4;

    // 珠宝
    public const int ITEM_TYPE_JEWEL = 8;

    // 装备
    public const int ITEM_TYPE_EQUIP = 43;

    // 镶嵌物
    public const int ITEM_TYPE_INSERT = 107;

    // 宠物
    public const int Item_TYPE_PET = 48;

    // 魂石
    public const int ITEM_TYPE_SOUL = 8;
}

/// <summary>
/// 区组类型.
/// </summary>
public class ZoneType
{
    // 热门区组
    public const int ZONE_TYPE_HOT = 0x00000001;

    // 推荐区组
    public const int ZONE_TYPE_RECOMMAND = 0x00000002;

    // 所有区组
    public const int ZONE_TYPE_ALL = 0x00000004;
}

/// <summary>
/// 区组显示的标记.
/// </summary>
public class ZoneTag
{
    // 区组显示HOT标记
    public const int ZONE_TAG_SHOW_HOT = 0x00000001;

    // 区组显示NEW标记
    public const int ZONE_TAG_SHOW_NEW = 0x00000002;
}

/// <summary>
/// 区组状态.
/// </summary>
public class ServerStatus
{
    // 维护中
    public const int SERVER_STATUS_FIX = 0;

    // 火爆
    public const int SERVER_STATUS_HOT = 1;

    // 畅通
    public const int SERVER_STATUS_FLUENCY = 2;

    // 未开放
    public const int SERVER_STATUS_UNOPEN = 3;
}

/// <summary>
/// 仓库页面.
/// </summary>
public class StorePage
{
    // 所有物品选项
    public const int STORE_PAGE_ALL_ITEM = 0;

    // 自定义
    public const int STORE_PAGE_CUSTOM = 2;
}

/// <summary>
/// 商店相关配置
/// </summary>
public class ShopConfig
{
    // 限购周期
    public const int NO_LIMIT_MARKET = 0;
    public const int DAY_CYCLE_MARKET = 1;
    public const int WEEK_CYCLE_MARKET = 2;
    public const int MONTH_CYCLE_MARKET = 3;
    public const int YEAR_CYCLE_MARKET = 4;
    public const int ACCOUNT_LIMIT_MARKET = 5;
    public const int LEVEL_LIMIT_MARKET = 6;
    public const int EACH_LIMIT_MARKET = 7;
    public const int NONE_LIMIT_MARKET = 8;


    // 购买方式
    public const int BUY_TYPE_MONEY = 1;
    public const int BUY_TYPE_GOLD_COIN = 2;

    // 商品分组
    public const string GOLD_COIN_GROUP = "gold_coin_group";
    public const string MONEY_GROUP = "money_group";
    public const string AP_GROUP = "ap_group";
    public const string LIFE_GROUP = "life_group";
    public const string WEEKEND_GIFT_GROUP = "weekend_gift";
    public const string DAILY_GIFT_GROUP = "daily_gift";
    public const string INTENSIFY_GIFT_GROUP = "intensify_gift_group";
    public const string LEVEL_GIFT_GROUP = "level_gift";

    // 礼包类型
    // 火风水经验波利
    public const int NORMAL_EXP_PET_TYPE = 1;

    // 光暗经验波利
    public const int SPECIAL_EXP_PET_TYPE = 2;

    // 月卡礼包id
    public const int MONTH_CARD_ID = 50332;
}

/// <summary>
/// 技能效果类型
/// </summary>
public class ArenaConst
{
    // 竞技场刷新对战列表方式
    public const int ARENA_MATCH_TYPE_NORMAL = 0;
    public const int ARENA_MATCH_TYPE_RETAIN = 1;
    public const int ARENA_MATCH_TYPE_OVERDUE = 2;

    // 竞技场积分和排名是否限制，-1表示没有限制
    public const int ARENA_RANK_SCORE_LIMIT = -1;

    // 反击状态
    public const int ARENA_REVENGE_TYPE_NONE = 0;
    public const int ARENA_REVENGE_TYPE_WIN = 1;
    public const int ARENA_REVENGE_TYPE_LOSE = 2;

    // 技能类别名称
    public static Dictionary<int,string> arenaTopNameMap = new Dictionary<int, string>()
    {
        { 1, LocalizationMgr.Get("arena_top_name_1") },
        { 2, LocalizationMgr.Get("arena_top_name_2") },
        { 3, LocalizationMgr.Get("arena_top_name_3") },
        { 4, LocalizationMgr.Get("arena_top_name_4") },
        { 5, LocalizationMgr.Get("arena_top_name_5") },
        { 6, LocalizationMgr.Get("arena_top_name_6") },
        { 7, LocalizationMgr.Get("arena_top_name_7") },
        { 8, LocalizationMgr.Get("arena_top_name_8") },
        { 9, LocalizationMgr.Get("arena_top_name_9") },
        { 10, LocalizationMgr.Get("arena_top_name_10") },
        { 11, LocalizationMgr.Get("arena_top_name_11") },
        { 12, LocalizationMgr.Get("arena_top_name_12") },
        { 13, LocalizationMgr.Get("arena_top_name_13") },
    };
}

/// <summary>
/// 任务常量
/// </summary>
public class TaskConst
{
    // 任务类型
    public const int DAILY_TASK = 0;
    public const int CHALLENGE_TASK = 1;
    public const int ACHIEVE_TASK = 2;
    public const int RESEARCH_TASK = 3;
    public const int EASY_TASK = 4;
    public const int GROWTH_TASK_LEVEL_STAR = 5;
    public const int GROWTH_TASK_SUIT_INTENSIFY = 6;
    public const int GROWTH_TASK_AWAKE = 7;
    public const int GROWTH_TASK_ARENA = 8;
    public const int GROWTH_TASK = 9;
    public const int INVITE_TASK = 10;
    public const int ADVEN_TASK = 11;
    public const int PET_TASK = 12;
    public const int EQPT_TASK = 13;
    public const int ARENA_TASK = 14;

    // 任务状态
    public const int ASSIGN_STATE = 0;
    public const int COMPLETED_STATE = 1;
}

/// <summary>
/// 聊天模块
/// </summary>
public class ChatConfig
{
    // 聊天频道
    public const string WORLD_CHANNEL = "world";
    public const string GUILD_CHANNEL = "system_gang";
    public const string WHISPER = "whisper";

    // 系统消息类型
    // 聊天系统消息
    public const string SYSTEM_CHAT = "system_message";

    // 聊天系统公告
    public const string SYSTEM_NOTIFY = "system_notify";

    // 游戏维护公告
    public const string GAME_NOTIFY = "system_affiche";

    // 公会宣传公告
    public const string SYSTEM_MESSAGE_GANG = "system_gang";
}

/// <summary>
/// 好友常量
/// </summary>
public class FriendConst
{
    // 好友操作结果
    public const int ERESULT_FAILED = -1;           // 好友操作失败
    public const int ERESULT_OK = 0;                // 好友操作成功
    public const int ERESULT_MAX_FRIEND = 1;        // 好友数量已经达到了上限
    public const int ERESULT_MAX_REQUEST = 2;       // 发出好友请求数量已经达到了上限
    public const int ERESULT_MAX_RECEIVE = 3;       // 收到好友请求数量已经达到了上限
    public const int ERESULT_REQUEST_DUPLICATE = 4; // 重复发送过了请求
    public const int ERESULT_BEEN_FRIEND = 5;       // 已经是好友了
    public const int ERESULT_INVALID = 6;           // 请求无效
    public const int ERESULT_BEEN_REQUESTED = 7;    // 已经被请求过了
}

/// <summary>
/// 公会常量
/// </summary>
public class GangConst
{
    // 创建帮派结果
    public const int CREATE_GANG_OK = 0;            // 创建公会成功
    public const int CREATE_GANG_FAILED = -1;       // 创建公会失败
    public const int CREATE_GANG_LEVEL_LIMIT = -2;  // 等级限制
    public const int CREATE_GANG_NAME_INVALID = -3; // 公会名字无效
    public const int CREATE_GANG_IN_GANG = -4;      // 已经有公会
    public const int CREATE_GANG_NAME_REPEAT = -5;  // 公会名字重复
    public const int CREATE_GANG_COST_FAILED = -6;  // 创建公会消耗不够

    // 帮派操作结果
    public const int GANG_OPERATE_OK = 0;                 // 操作成功
    public const int GANG_OPERATE_FAILED = -1;            // 操作失败
    public const int GANG_OPERATE_NONE_GANG = -2;         // 操作失败，不在公会中
    public const int GANG_OPERATE_INVALID_OPER = -3;      // 操作失败，权限不允许
    public const int GANG_OPERATE_MEMBER_LIMIT = -4;      // 操作失败，成员数量限制
    public const int GANG_OPERATE_NONE_MEMBER = -5;       // 操作失败，不是公会成员
    public const int GANG_OPERATE_AREADY_STATION = -6;    // 操作失败，目标已经是该职位
    public const int GANG_OPERATE_STATION_NUM_LIMIT = -7; // 操作失败，职位人数限制
    public const int GANG_OPERATE_NONE_STATION = -8;      // 操作失败，目标已经不是该职位
    public const int GANG_OPERATE_FORBID_REMOVE_SELF = -9;// 操作失败，不能移除自己
    public const int GANG_OPERATE_COST_FAILED = -10;      // 操作失败，扣除消耗失败
    public const int GANG_OPERATE_INVALID_GANG = -11;     // 操作失败，无效公会
    public const int GANG_OPERATE_CONDITION_UNSATISFIED = -12; // 操作失败，入会条件不满足
    public const int GANG_OPERATE_IN_GANG = -13; // 操作失败，已经在公会中了
    public const int GANG_OPERATE_CD_LIMIT = -14; // 操作失败，处于离开公会cd中，暂时不能接收请求和发出申请
    public const int GANG_OPERATE_REQUESTED = -15; // 操作失败，重复申请（或者对方已经发出过邀请）
    public const int GANG_OPERATE_MAX_REQUEST = -16; // 操作失败，请求数量达到了最大值限制
    public const int GANG_OPERATE_MAX_RECEIVE = -17; // 操作失败，公会收到的请求数量已经达到了上限
    public const int GANG_OPERATE_MAX_MEMBER = -18; // 操作失败，公会人数是否已经满
    public const int GANG_OPERATE_NOT_ACCEPT = -19; // 操作失败，拒绝接收成员
    public const int GANG_OPERATE_INVALID_USER = -20; // 操作失败，无效玩家
    public const int GANG_OPERATE_INVALID_REQUEST = -21; // 操作失败，无效请求/邀请
    public const int GANG_OPERATE_CONTENT_SIZE_LIMIT = -22; // 操作失败，消耗不够

}

public class LocalizationConst
{
    public const string ZH_CN = "zh_cn";

    public const string START = "start";
}

/// <summary>
/// 红点提示常量
/// </summary>
public class RedTipsConst
{
    public const int SHOP_TIPS_TYPE = 1;

    public const int LOTTERY_TIPS_TYPE = 2;

    public const int ARENA_TIPS_TYPE = 3;

    public const int GANG_TIPS_TYPE = 4;
}

/// <summary>
/// 系统功能常量
/// </summary>
public class SystemFunctionConst
{
    // 系统功能显示位置

    // 屏幕左边
    public const int SCREEN_LEFT = 0;

    // 屏幕右边
    public const int SCREEN_RIGHT = 1;

    // 提示按钮类型
    public const int RED_POINT = 1;

    public const int NEW_TIPS = 2;
}

public class ActivityConst
{
    public const string FREE_UNEQUIP = "free_unequip";
}

public class ItemConst
{
    // 魔力魂石
    public const string SOUL_M = "soul_m";

    // 火之魂石
    public const string SOUL_F = "soul_f";

    // 风之魂石
    public const string SOUL_S = "soul_s";

    // 水之魂石
    public const string SOUL_W = "soul_w";

    // 光明魂石
    public const string SOUL_L = "soul_l";

    // 黑暗魂石
    public const string SOUL_D = "soul_d";


    public static List<string> SoulMap = new List<string>()
    {
            SOUL_M,
            SOUL_F,
            SOUL_S,
            SOUL_W,
            SOUL_L,
            SOUL_D,
    };
}

// 通天之塔常量
public class TowerConst
{
    public const int EASY_TOWER = 0;

    public const int HARD_TOWER = 1;
}

// 推送常量(1-100)
public class PushConst
{
    // 满体力推送提醒
    public const int LIFE_FULL = 1;
}

// 角色常量
public class CharConst
{
    public const int MALE = 1;       // 男性
    public const int FEMALE = 2;       // 女性

    // 角色常量
    public static Dictionary<int,string> genderStringMap = new Dictionary<int, string>()
    {
        { MALE , LocalizationMgr.Get("gender_male") },
        { FEMALE, LocalizationMgr.Get("gender_female") }
    };
}

// 退出游戏常量
public class QuitConst
{
    // 正常退出
    public const int NORMAL_QUIT = 1;

    // 冻结账号，退出游戏
    public const int BLOCK_QUIT = 2;
}

/// <summary>
/// 指引常量
/// </summary>
public class GuideConst
{
    // 指引分组
    // 创建角色窗口显示之前
    public const int SHOW_CREATE_CHAR_BEFORE = 0;

    // 创建角色窗口显示之后
    public const int SHOW_CREATE_CHAR_AFTER = 1;

    // 玩家登录游戏成功后
    public const int LOGIN_OK = 2;

    // 竞技场指引完成
    public const int ARENA_FINISH = 4;


    // 指引返回操作

    // 返回副本选择界面
    public const int RETURN_SELECT_INSTANCE = 0;

    // 返回
    public const int RETURN_RISK = 1;

    // 返回竞技场操作
    public const int RETURN_ARENA = 2;

    // 竞技场再来一次
    public const int AGAIN_ARENA = 3;
}

/// <summary>
/// 分享平台类型
/// </summary>
public class SharePlatformType
{
    public const string Facebook = "facebook";
    public const string Twitter = "twitter";
    public const string Line = "line";
    /// <summary>
    /// 微信-分享朋友圈
    /// </summary>
    public const string WechatMoments = "wechat_moments";
    /// <summary>
    /// 微信-分享好友
    /// </summary>
    public const string Wechat = "wechat";
    /// <summary>
    /// 新浪
    /// </summary>
    public const string Sina = "sina";
}

public class MesssgeBoxConst
{
    public const string CLOSE = "close";

    public const string SHARE = "share";

    public const string GOTO = "goto";
}

public class GameCourseConst
{
    // 游戏历程分页
    public const int PAGE_0 = 0;
    public const int PAGE_1 = 1;
    public const int PAGE_2 = 2;
    public const int PAGE_3 = 3;
    public const int PAGE_4 = 4;
    public const int PAGE_5 = 5;
}

public class ActivityBonusType
{
    public const int NORMAL_BONUS = 1;

    public const int EXTRA_BONUS = 2;
}
