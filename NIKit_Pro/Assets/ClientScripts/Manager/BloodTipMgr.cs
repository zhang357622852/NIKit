/// <summary>
/// BloodTipMgr.cs
/// Created by lic 2016-8-11
/// 飘血管理器
/// </summary>

using System;
using UnityEngine;
using System.Collections.Generic;
using LPC;
using System.Collections;
using System.Reflection;

public enum TipsWndType
{
    DamageTip,
    CureTip,
    BlockTip,
    DeadlyTip,
    BuffTip,
    DeBuffTip,
    SkillTip,
    ActionTip,
}

public enum TipsWndType_DamageTip
{
    // 伤害生命类
    HpTip,
    // 伤害能量类
    MpTip,
    // 抵抗类
    ResTip,
    // 行动类
    ActTip,
}

public enum TipsWndType_CureTip
{
    HpTip,
    MpTip,
}

/// <summary>
/// 飘血管理模块
/// </summary>
public class BloodTipMgr
{
    // 相同类型的提示框延迟显示时间
    private static float sameTypeDelay = 0.2f;

    // 场景中所有的类型的窗口字典
    private static Dictionary<TipsWndType, List<GameObject>> mWndTypeDict = new Dictionary<TipsWndType, List<GameObject>>();

    // 提示缓存字典（索引为窗口类型和target的rid组合）
    private static Dictionary<string, List<object[]>> mTipsCacheDict = new Dictionary<string, List<object[]>>();

    //  各类飘血框初始化个数
    private static Dictionary<TipsWndType, int> mMaxNumberDict = new Dictionary<TipsWndType, int>()
    {
        { TipsWndType.DamageTip, 5 },
        { TipsWndType.CureTip, 5 },
        { TipsWndType.BlockTip, 3 },
        { TipsWndType.DeadlyTip, 3 },
        { TipsWndType.BuffTip, 5 },
        { TipsWndType.DeBuffTip, 5 },
        { TipsWndType.SkillTip, 2 },
        { TipsWndType.ActionTip, 3 },
    };

    // 各类飘血框预制名称
    private static Dictionary<TipsWndType, string> mWndNameDict = new Dictionary<TipsWndType, string>()
    {
        { TipsWndType.DamageTip, "MonsterTipDamage" },
        { TipsWndType.CureTip, "MonsterTipCure" },
        { TipsWndType.BlockTip, "MonsterTipBlock" },
        { TipsWndType.DeadlyTip, "MonsterTipDeadly" },
        { TipsWndType.BuffTip, "MonsterTipBuff" },
        { TipsWndType.DeBuffTip, "MonsterTipDeBuff" },
        { TipsWndType.SkillTip, "MonsterTipSkill" },
        { TipsWndType.ActionTip, "MonsterTipAction" },
    };

    // 显示伤害数字类型颜色对照表
    public static Dictionary<TipsWndType_DamageTip, string> mHurtColorDict = new Dictionary<TipsWndType_DamageTip, string>()
    {
        { TipsWndType_DamageTip.HpTip, "[ff7474]" },
        { TipsWndType_DamageTip.MpTip, "[ffff53]" },
        { TipsWndType_DamageTip.ResTip, "[afff6f]" },
        { TipsWndType_DamageTip.ActTip, "[ff8989]" },
    };

    // 显示治疗数字类型颜色对照表
    public static Dictionary<TipsWndType_CureTip, string> mCureColorDict = new Dictionary<TipsWndType_CureTip, string>()
    {
        { TipsWndType_CureTip.HpTip, "[afff6f]" },
        { TipsWndType_CureTip.MpTip, "[ffe7a5]" },
    };

    #region 内部接口

    /// <summary>
    /// 世界坐标到屏幕坐标的转换.
    /// </summary>
    /// <returns>The to screen position.</returns>
    /// <param name="who">Who.</param>
    private static Vector3 WorldToUIPos(Property target)
    {
        // 受创者不存在会在actor对象不存在不处理
        if (target == null || target.Actor == null)
            return Vector3.zero;

        // 场景相机或者UI相机不存在
        if (SceneMgr.SceneCamera == null ||
            SceneMgr.UiCamera == null)
            return Vector3.zero;

        // 人物高度加上偏移
        CombatActor actor = target.Actor;

        // 此处直接取模型的中心点即可
        Vector3 actorPos = actor.GetBoxColiderPos();

        // 世界坐标转屏幕坐标
        return Game.WorldToUI(actorPos);
    }

    /// <summary>
    /// 取得一个窗口
    /// </summary>
    /// <returns>The tips window.</returns>
    /// <param name="type">Type.</param>
    private static GameObject SpawnTipsWnd(TipsWndType type)
    {
        if (!mWndTypeDict.ContainsKey(type))
            return null;

        GameObject ob;

        if (mWndTypeDict[type].Count > 0)
        {
            ob = mWndTypeDict[type][0];
            ob.SetActive(true);
            mWndTypeDict[type].Remove(ob);

            return ob;
        }

        GameObject template = GameObject.Find(mWndNameDict[type]);

        if (template == null)
            return null;

        ob = GameObject.Instantiate(template) as GameObject;
        ob.transform.parent = WindowMgr.UIRoot;
        ob.transform.localScale = new Vector3(1f, 1f, 1f);
        ob.name = mWndNameDict[type];
        ob.SetActive(true);

        return ob;
    }

    /// <summary>
    /// 显示一个提示
    /// </summary>
    /// <param name="wndType">Window type.</param>
    /// <param name="target">Target.</param>
    /// <param name="args">Arguments.</param>
    private static void ShowTip(TipsWndType wndType, Property target, object[] args)
    {
        // 获得一个飘血对象
        GameObject ob = SpawnTipsWnd(wndType);

        if (ob == null)
            return;

        // 固定提示框位置
        ob.transform.position = WorldToUIPos(target);

        // 绑定提示框数据
        ob.GetComponent<BloodTip>().BindData(args);
    }

    /// <summary>
    /// 协程显示提示
    /// </summary>
    /// <returns>The blood tip.</returns>
    /// <param name="wndType">Window type.</param>
    /// <param name="target">Target.</param>
    /// <param name="args">Arguments.</param>
    private static IEnumerator SyncShowTip(TipsWndType wndType, Property target, string key)
    {
        while (mTipsCacheDict[key].Count > 0)
        {
            ShowTip(wndType, target, mTipsCacheDict[key][0]);

            yield return new WaitForSeconds(sameTypeDelay);

            mTipsCacheDict[key].RemoveAt(0);
        }

        yield break;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化模版池
    /// </summary>
    public static void Clear()
    {
        mWndTypeDict.Clear();
    }

    /// <summary>
    /// 初始化模版池
    /// </summary>
    public static void Init()
    {
        // 飘雪框有初始化过
        if (mWndTypeDict.Count != 0)
            return;

        // 初始化各个节点
        foreach (KeyValuePair<TipsWndType, string> item in mWndNameDict)
        {
            List<GameObject> tipTemplatesList = new List<GameObject>();

            // 预设资源路径
            string prefebResource = string.Format("Assets/Prefabs/Window/{0}.prefab",
                WindowMgr.GetCustomWindowName(item.Value));

            // 创建窗口
            GameObject tipTemplate = WindowMgr.CreateWindow(item.Value, prefebResource);
            if (tipTemplate == null)
                continue;

            tipTemplate.transform.parent = WindowMgr.UIRoot;
            tipTemplate.name = item.Value;
            tipTemplatesList.Add(tipTemplate);
            tipTemplate.SetActive(false);

            for (int i = 1; i < mMaxNumberDict[item.Key]; i++)
            {
                GameObject ob = GameObject.Instantiate(tipTemplate) as GameObject;
                ob.transform.parent = WindowMgr.UIRoot;
                ob.transform.localScale = new Vector3(1f, 1f, 1f);
                ob.name = item.Value;

                tipTemplatesList.Add(ob);
                ob.SetActive(false);
            }

            mWndTypeDict.Add(item.Key, tipTemplatesList);
        }
    }

    /// <summary>
    /// 添加一个提示框
    /// </summary>
    public static void AddTip(TipsWndType wndType, Property target, string desc, string icon = "", bool isDeadly = false)
    {
        // 验证客户端不处理
        if (AuthClientMgr.IsAuthClient)
            return;

        // 角色对象不处理
        if (target == null)
            return;

        // 可以为相同目标相同窗口类型的组合
        string key = mWndNameDict[wndType] + target.GetRid();

        // 缓存中不包含该类型
        if (!mTipsCacheDict.ContainsKey(key))
        {
            mTipsCacheDict.Add(key, new List<object[]>());
        }

        object[] args = new object[]{ desc, icon, isDeadly };

        // 将提示加入到缓存列表
        mTipsCacheDict[key].Add(args);

        // 当前该类型无缓存,直接提示
        if (mTipsCacheDict[key].Count == 1)
            Coroutine.DispatchService(SyncShowTip(wndType, target, key));
    }

    /// <summary>
    /// 添加技能提示
    /// </summary>
    /// <param name="target">Target.</param>
    /// <param name="skillId">Skill identifier.</param>
    public static void AddSkillTip(Property target, int skillId)
    {
        // 验证客户端不处理
        if (AuthClientMgr.IsAuthClient)
            return;

        // 无效技能不处理
        CsvRow skill_item = SkillMgr.GetSkillInfo(skillId);
        if (skill_item == null)
            return;

        AddTip(TipsWndType.SkillTip, target, SkillMgr.GetSkillName(skillId),
            SkillMgr.GetIcon(skillId));
    }

    /// <summary>
    /// 添加状态提示
    /// </summary>
    /// <param name="target">Target.</param>
    /// <param name="status_id">Status identifier.</param>
    public static void AddStatusTip(Property target, string status)
    {
        // 验证客户端不处理
        if (AuthClientMgr.IsAuthClient)
            return;

        // 无效状态不处理
        CsvRow status_data = StatusMgr.GetStatusInfo(status);
        if (status_data == null)
            return;

        string status_name = LocalizationMgr.Get(status_data.Query<string>("name"));
        int status_type = status_data.Query<int>("tip_type");

        // 取得窗口类型
        TipsWndType wndType;
        if (status_type == StatusConst.TYPE_BUFF)
            wndType = TipsWndType.BuffTip;
        else if (status_type == StatusConst.TYPE_DEBUFF)
            wndType = TipsWndType.DeBuffTip;
        else
            return;

        AddTip(wndType, target, status_name);
    }

    /// <summary>
    /// 添加一个飘血框
    /// </summary>
    /// <param name="target">Target.</param>
    /// <param name="damageType">Damage type.</param>
    /// <param name="tipsInfo">Tips info.</param>
    /// <param name="parent">Parent.</param>
    public static void AddDamageOrCureTip(Property target, int damageType, LPCMapping tipsInfo)
    {
        // 验证客户端不处理
        if (AuthClientMgr.IsAuthClient)
            return;

        string tips = string.Empty;
        bool isDeadly = false;
        bool isUnknowType = true;

#region 提示暴击
        if ((damageType & CombatConst.DAMAGE_TYPE_DEADLY) == CombatConst.DAMAGE_TYPE_DEADLY)
        {
            isUnknowType = false;

            // 没有伤害数值
            if (!tipsInfo.ContainsKey("hp"))
                return;

            isDeadly = true;
            tips = LocalizationMgr.Get("MonsterTipsInfoWnd_1");

            AddTip(TipsWndType.DeadlyTip, target, tips);
        }
#endregion

#region 提示格挡
        if ((damageType & CombatConst.DAMAGE_TYPE_BLOCK) == CombatConst.DAMAGE_TYPE_BLOCK)
        {
            isUnknowType = false;
            tips = LocalizationMgr.Get("MonsterTipsWnd_2");

            AddTip(TipsWndType.BlockTip, target, tips);
        }
#endregion

#region 提示吸收
        if ((damageType & CombatConst.DAMAGE_TYPE_ABSORB) == CombatConst.DAMAGE_TYPE_ABSORB)
        {
            isUnknowType = false;

            // 没有伤害数值
            if (!tipsInfo.ContainsKey("hp"))
                return;
            if (tipsInfo.GetValue<int>("hp") > 0)
                tips = string.Format("{0}-{1}", mHurtColorDict[TipsWndType_DamageTip.HpTip], tipsInfo.GetValue<int>("hp"));
            else
                tips = string.Format("{0}{1}", mHurtColorDict[TipsWndType_DamageTip.HpTip], tipsInfo.GetValue<int>("hp"));

            AddTip(TipsWndType.DamageTip, target, tips);
        }
#endregion

#region 无视防御
        if ((damageType & CombatConst.DAMAGE_TYPE_IGNORE_DEF) == CombatConst.DAMAGE_TYPE_IGNORE_DEF)
        {
            isUnknowType = false;
            tips = LocalizationMgr.Get("MonsterTipsWnd_3");

            AddTip(TipsWndType.DeBuffTip, target, tips);
        }
#endregion

#region 穿刺攻击

        if ((damageType & CombatConst.DAMAGE_TYPE_PIERCE) == CombatConst.DAMAGE_TYPE_PIERCE)
        {
            isUnknowType = false;
            tips = LocalizationMgr.Get("MonsterTipsWnd_6");

            AddTip(TipsWndType.DeBuffTip, target, tips);
        }
#endregion

#region 持续伤害
        if ((damageType & CombatConst.DAMAGE_TYPE_INJURY) == CombatConst.DAMAGE_TYPE_INJURY)
        {
            isUnknowType = false;

            // 没有伤害数值
            if (!tipsInfo.ContainsKey("hp"))
                return;

            tips = string.Format("{0}{1} -{2}", mHurtColorDict[TipsWndType_DamageTip.HpTip], LocalizationMgr.Get("MonsterTipsWnd_5"), tipsInfo.GetValue<int>("hp"));

            AddTip(TipsWndType.DamageTip, target, tips);
        }
        #endregion

#region 最大属性伤害 目前有max_hp/max_mp
        if ((damageType & CombatConst.DAMAGE_TYPE_MAX_ATTRIB) == CombatConst.DAMAGE_TYPE_MAX_ATTRIB)
        {
            isUnknowType = false;

            if (tipsInfo.ContainsKey("max_hp"))
            {
                tips = string.Format("{0}{1} -{2}", mHurtColorDict[TipsWndType_DamageTip.HpTip], LocalizationMgr.Get("MonsterTipsWnd_8"), tipsInfo.GetValue<int>("max_hp"));

                AddTip(TipsWndType.DamageTip, target, tips);
            }

            if (tipsInfo.ContainsKey("max_mp"))
            {
                tips = string.Format("{0}{1} -{2}", mHurtColorDict[TipsWndType_DamageTip.MpTip], LocalizationMgr.Get("MonsterTipsWnd_9"), tipsInfo.GetValue<int>("max_mp"));

                AddTip(TipsWndType.DamageTip, target, tips);
            }
        }
#endregion

#region 最大属性治疗 目前有max_hp/max_mp
        if ((damageType & CombatConst.CURE_TYPE_MAX_ATTRIB) == CombatConst.CURE_TYPE_MAX_ATTRIB)
        {
            isUnknowType = false;

            if (tipsInfo.ContainsKey("max_hp"))
            {
                tips = string.Format("{0}{1} +{2}", mHurtColorDict[TipsWndType_DamageTip.HpTip], LocalizationMgr.Get("MonsterTipsWnd_8"), tipsInfo.GetValue<int>("max_hp"));

                AddTip(TipsWndType.CureTip, target, tips);
            }

            if (tipsInfo.ContainsKey("max_mp"))
            {
                tips = string.Format("{0}{1} +{2}", mHurtColorDict[TipsWndType_DamageTip.MpTip], LocalizationMgr.Get("MonsterTipsWnd_9"), tipsInfo.GetValue<int>("max_mp"));

                AddTip(TipsWndType.CureTip, target, tips);
            }
        }
#endregion

#region 血条回复或蓝条回复
        if ((damageType & CombatConst.CURE_TYPE_MAGIC) == CombatConst.CURE_TYPE_MAGIC)
        {
            isUnknowType = false;

            if (tipsInfo == null || tipsInfo.Count <= 0)
            {
                LogMgr.Trace("受到治愈时，治愈参数为空，显示治愈提示失败");
                return;
            }

            if (tipsInfo.ContainsKey("hp"))
            {
                // 治愈血量
                tips = string.Format("{0}+{1}", mCureColorDict[TipsWndType_CureTip.HpTip], tipsInfo["hp"].AsInt);
                AddTip(TipsWndType.CureTip, target, tips);
            }

            if (tipsInfo.ContainsKey("mp"))
            {
                tips = string.Format("{0}{1}+{2}", mCureColorDict[TipsWndType_CureTip.MpTip], LocalizationMgr.Get("MonsterTipsWnd_4"), tipsInfo.GetValue<int>("mp"));
                AddTip(TipsWndType.CureTip, target, tips);
            }
        }
#endregion

#region 血量，蓝量受创
        // 能量受创
        if ((damageType & CombatConst.DAMAGE_TYPE_MP) == CombatConst.DAMAGE_TYPE_MP)
        {
            isUnknowType = false;

            // 没有伤害数值
            if (!tipsInfo.ContainsKey("mp"))
                return;

            // 伤害减少能量
            tips = string.Format("{0}{1}-{2}", mHurtColorDict[TipsWndType_DamageTip.MpTip], LocalizationMgr.Get("MonsterTipsWnd_4"), tipsInfo.GetValue<int>("mp"));

            AddTip(TipsWndType.DamageTip, target, tips);

        }

        // 血量受创
        if ((damageType & CombatConst.DAMAGE_TYPE_ATTACK) == CombatConst.DAMAGE_TYPE_ATTACK)
        {
            isUnknowType = false;

            // 没有伤害数值
            if (!tipsInfo.ContainsKey("hp"))
                return;

            // 普通受创
            tips = string.Format("{0}-{1}", mHurtColorDict[TipsWndType_DamageTip.HpTip], tipsInfo.GetValue<int>("hp"));

            AddTip(TipsWndType.DamageTip, target, tips, "", isDeadly);
        }
#endregion

#region 未知类型，目前以白色字体显示
        if (isUnknowType)
        {
            if (tipsInfo.ContainsKey("hp"))
            {
                tips = string.Format("{0}", tipsInfo["hp"].AsInt);

                AddTip(TipsWndType.CureTip, target, tips);
            }

            if (tipsInfo.ContainsKey("mp"))
            {
                tips = string.Format("{0}", tipsInfo.GetValue<int>("mp"));

                AddTip(TipsWndType.CureTip, target, tips);
            }
        }
#endregion
    }

    /// <summary>
    /// 回收窗口
    /// </summary>
    /// <param name="ob">Ob.</param>
    public static void Recycle(GameObject ob, TipsWndType type)
    {
        // 验证客户端不处理
        if (AuthClientMgr.IsAuthClient)
            return;

        // 不包含该类型提示窗口
        if (! mWndTypeDict.ContainsKey(type))
            return;

        ob.SetActive(false);

        // 此处没有做超过设定预制上限后即销毁操作
        // 是因为考虑到玩家如果此处需要怎么多提示
        // 接下来也必定需要再次创建这么多提示

        mWndTypeDict[type].Add(ob);
    }

    #endregion
}

