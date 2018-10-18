/// <summary>
/// TriggerPropMgr.cs
/// Created by zhaozy 2014-12-10
/// 触发属性管理
/// </summary>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using LPC;

/// <summary>
/// 触发属性管理
/// </summary>
public class TriggerPropMgr
{
    #region 变量

    // 触发属性配置表信息
    private static CsvFile mTriggerPropCsv;

    // 忽视伤害类型
    private static int IGNORE_TYPE = (CombatConst.DAMAGE_TYPE_THORNS |
                                      CombatConst.DAMAGE_TYPE_TRIGGER |
                                      CombatConst.DAMAGE_TYPE_INJURY | 
                                      CombatConst.DAMAGE_TYPE_MAX_ATTRIB);

    // 触发属性
    private static Dictionary<int, List<CsvRow>> mTriggerStageMap = new Dictionary<int, List<CsvRow>>();

    #endregion

    #region 属性

    // 属性配置表信息
    public static CsvFile TriggerPropCsv { get { return mTriggerPropCsv; } }

    #endregion

    #region 内部接口

    static TriggerPropMgr()
    {
        // 注册角色受创的回调
        EventMgr.RegisterEvent("TriggerPropMgr", EventMgrEventType.EVENT_RECEIVE_DAMAGE, WhenReceiveDamage);

        // 关注角色闪避的回调
        EventMgr.RegisterEvent("TriggerPropMgr", EventMgrEventType.EVENT_DODGE, WhenDodge);

        // 关注角色死亡的回调
        EventMgr.RegisterEvent("TriggerPropMgr", EventMgrEventType.EVENT_DIE_END, WhenDieEnd);

        // 关注角色死亡的回调
        EventMgr.RegisterEvent("TriggerPropMgr", EventMgrEventType.EVENT_DIE, WhenDie);

        // 注册附加状态
        EventMgr.RegisterEvent("TriggerPropMgr", EventMgrEventType.EVENT_APPLY_STATUS, WhenApplyStatus);

        // 回合结束事件
        EventMgr.RegisterEvent("TriggerPropMgr", EventMgrEventType.EVENT_ROUND_END, WhenRoundEnd);

        // 回合开始事件
        EventMgr.RegisterEvent("TriggerPropMgr", EventMgrEventType.EVENT_ROUND_START, WhenRoundStart);

        // 注册角色受创的回调
        EventMgr.RegisterEvent("TriggerPropMgr", EventMgrEventType.EVENT_RECEIVE_CURE, WhenReceiveCure);
    }

    /// <summary>
    /// 角色受创的回调
    /// </summary>
    private static void WhenReceiveDamage(int eventId, MixedValue para)
    {
        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();

        // 获取受创目标
        Property target = Rid.FindObjectByRid(args.GetValue<string>("rid"));
        if (target == null)
            return;

        // 获取伤害类型
        int damageType = args.GetValue<int>("damage_type");
        LPCMapping sourceProfile = args.GetValue<LPCMapping>("source_profile");
        int skillId = sourceProfile.GetValue<int>("skill_id");

        // 获取攻击者
        Property source = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 构建参数
        LPCMapping triggerPara = new LPCMapping();
        triggerPara.Add("damage_type", damageType);
        triggerPara.Add("damage_map", args["damage_map"]);
        triggerPara.Add("original_damage_map", args["original_damage_map"]);
        triggerPara.Add("shield_absorb_before_damage", args.GetValue<LPCMapping>("shield_absorb_before_damage", new LPCMapping()));
        triggerPara.Add("cure_map", args.GetValue<LPCMapping>("cure_map", new LPCMapping()));
        triggerPara.Add("cookie", sourceProfile.GetValue<string>("cookie", string.Empty));
        triggerPara.Add("original_cookie", sourceProfile.GetValue<string>("original_cookie", string.Empty));
        triggerPara.Add("type", sourceProfile.GetValue<int>("type"));

        // 添加技能id信息
        if (skillId != 0)
            triggerPara.Add("skill_id", skillId);

        // 如果目标死亡，额外给个参数标志下
        if (target.CheckStatus("DIED"))
            triggerPara.Add("is_die", 1);

        // 执行触发DAMAGE节点属性
        DoTriggerProp(target, source, TriggerPropConst.DAMAGE, triggerPara);

        // TriggerPropConst.HIT类型没有技能或者是忽视伤害类型IGNORE_TYPE不处理
        // 如果是反弹伤害，不触发触发伤害
        if ((damageType & IGNORE_TYPE) != 0 || skillId == 0)
            return;

        // 执行触发HIT节点属性
        DoTriggerProp(target, source, TriggerPropConst.HIT, triggerPara);
    }

    /// <summary>
    /// 执行触发属性
    /// </summary>
    private static void DoTriggerProp(Property target, Property source, int triggerType, LPCMapping triggerPara)
    {
        // 1. 触发攻击角色的触发属性，该角色的触发属性是主动触发的
        // 根据stage_array分别触发需要触发属性
        ApplyTriggerProp(source, triggerType, triggerPara, TriggerPropConst.ACTIVE, target);

        // 2. 触发受创角色自己的触发属性，该角色的属性是被动触发的
        // 根据stage_array分别触发需要触发属性
        ApplyTriggerProp(target, triggerType, triggerPara, TriggerPropConst.PASSIVE, source);
    }

    /// <summary>
    /// 角色闪避回调
    /// </summary>
    private static void WhenDodge(int eventId, MixedValue para)
    {
        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();

        // 获取targetOb
        Property target = Rid.FindObjectByRid(args.GetValue<string>("target_rid"));
        if (target == null)
            return;

        // 如果不是技能造成的受创，不触发
        LPCMapping sourceProfile = args.GetValue<LPCMapping>("source_profile");
        int skillId = sourceProfile.GetValue<int>("skill_id");
        if (skillId == 0)
            return;

        // 构建参数
        LPCMapping triggerPara = new LPCMapping();
        triggerPara.Add("skill_id", skillId);

        // 获取攻击者
        Property source = Rid.FindObjectByRid(sourceProfile.GetValue<string>("rid"));

        // 2. 触发攻击角色的触发属性，该角色的触发属性是主动触发的
        // 根据stage_array分别触发需要触发属性
        ApplyTriggerProp(source, CombatConst.DAMAGE_TYPE_DODGE, triggerPara, TriggerPropConst.ACTIVE, target);

        // 1. 触发受创角色自己的触发属性，该角色的属性是被动触发的
        // 根据stage_array分别触发需要触发属性
        ApplyTriggerProp(target, CombatConst.DAMAGE_TYPE_DODGE, triggerPara, TriggerPropConst.PASSIVE, source);
    }

    /// <summary>
    /// 角色死亡结束的回调（模型渐变结束或者死亡动作播放结束）
    /// </summary>
    private static void WhenDieEnd(int eventId, MixedValue para)
    {
        // 获取参数获取targetOb
        Property target = para.GetValue<Property>();
        if (target == null)
            return;

        // 杀死怪物没有凶手信息，不处理
        LPCMapping assailantInfo = target.QueryTemp<LPCMapping>("assailant_info");
        if (assailantInfo == null)
            return;

        // 获取攻击者, 优先获取joint_rid
        string sourceRid = assailantInfo.GetValue<string>("joint_rid");
        if (string.IsNullOrEmpty(sourceRid))
            sourceRid = assailantInfo.GetValue<string>("rid");

        // 获取source对象
        Property source = Rid.FindObjectByRid(sourceRid);

        // 获取导致死亡的技能id
        LPCMapping triggerPara = new LPCMapping();
        triggerPara.Add("skill_id", assailantInfo.GetValue<int>("skill_id"));

        // 添加original_cookie信息
        if (assailantInfo.ContainsKey("original_cookie"))
            triggerPara.Add("original_cookie", assailantInfo["original_cookie"]);

        // 2. 触发攻击角色的触发属性，该角色的触发属性是主动触发的
        // 根据stage_array分别触发需要触发属性
        ApplyTriggerProp(source, TriggerPropConst.DIE_END, triggerPara, TriggerPropConst.ACTIVE, target);

        // 1. 触发受创角色自己的触发属性，该角色的属性是被动触发的
        // 根据stage_array分别触发需要触发属性
        ApplyTriggerProp(target, TriggerPropConst.DIE_END, triggerPara, TriggerPropConst.PASSIVE, source);
    }

    /// <summary>
    /// 角色死亡的回调
    /// </summary>
    private static void WhenDie(int eventId, MixedValue para)
    {
        // 获取参数获取targetOb
        Property target = para.GetValue<Property>();
        if (target == null)
            return;

        // 杀死怪物没有凶手信息，不处理
        LPCMapping assailantInfo = target.QueryTemp<LPCMapping>("assailant_info");
        if (assailantInfo == null)
            return;

        // 获取攻击者, 优先获取joint_rid
        string sourceRid = assailantInfo.GetValue<string>("joint_rid");
        if (string.IsNullOrEmpty(sourceRid))
            sourceRid = assailantInfo.GetValue<string>("rid");

        // 获取source对象
        Property source = Rid.FindObjectByRid(sourceRid);

        // 获取导致死亡的技能id
        LPCMapping triggerPara = new LPCMapping();
        triggerPara.Add("skill_id", assailantInfo.GetValue<int>("skill_id"));

        // 添加original_cookie信息
        if (assailantInfo.ContainsKey("original_cookie"))
            triggerPara.Add("original_cookie", assailantInfo["original_cookie"]);

        // 2. 触发攻击角色的触发属性，该角色的触发属性是主动触发的
        // 根据stage_array分别触发需要触发属性
        ApplyTriggerProp(source, TriggerPropConst.DIE, triggerPara, TriggerPropConst.ACTIVE, target);

        // 1. 触发受创角色自己的触发属性，该角色的属性是被动触发的
        // 根据stage_array分别触发需要触发属性
        ApplyTriggerProp(target, TriggerPropConst.DIE, triggerPara, TriggerPropConst.PASSIVE, source);
    }

    /// <summary>
    /// 当前角色附加状态
    /// </summary>
    private static void WhenApplyStatus(int eventId, MixedValue para)
    {
        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();

        // 获取参数获取targetOb
        Property target = Rid.FindObjectByRid(args.GetValue<string>("rid"));
        if (target == null)
            return;

        // 根据stage_array分别触发需要触发属性
        ApplyTriggerProp(target, TriggerPropConst.APPLY_STATUS, args, TriggerPropConst.PASSIVE, null);
    }

    /// <summary>
    /// 回合结束
    /// </summary>
    private static void WhenRoundEnd(int eventId, MixedValue para)
    {
        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();
        if (args == null)
            return;

        // 数据格式转换
        Property target = Rid.FindObjectByRid(args.GetValue<string>("rid"));
        if (target == null)
            return;

        // 根据stage_array分别触发需要触发属性
        ApplyTriggerProp(target, TriggerPropConst.ROUND_END, args, TriggerPropConst.ACTIVE, null);
    }

    /// <summary>
    /// 回合前处理
    /// </summary>
    private static void WhenRoundStart(int eventId, MixedValue para)
    {
        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();
        if (args == null)
            return;

        // 数据格式转换
        Property target = Rid.FindObjectByRid(args.GetValue<string>("rid"));
        if (target == null)
            return;

        // 根据stage_array分别触发需要触发属性
        ApplyTriggerProp(target, TriggerPropConst.ROUND_START, args, TriggerPropConst.ACTIVE, null);
    }

    /// <summary>
    /// 角色自愈回调
    /// </summary>
    private static void WhenReceiveCure(int eventId, MixedValue para)
    {
        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();

        // 获取受创目标
        Property target = Rid.FindObjectByRid(args.GetValue<string>("rid"));
        if (target == null)
            return;

        // 构建参数
        LPCMapping sourceProfile = args["source_profile"].AsMapping;
        LPCMapping triggerPara = new LPCMapping();
        triggerPara.Add("cure_type", args.GetValue<int>("cure_type"));
        triggerPara.Add("real_cure_map", args["real_cure_map"]);
        triggerPara.Add("cure_map", args["cure_map"]);
        triggerPara.Add("source_profile", sourceProfile);
        triggerPara.Add("cookie", sourceProfile.GetValue<string>("cookie"));

        // 根据stage_array分别触发需要触发属性
        ApplyTriggerProp(target, TriggerPropConst.CURE, triggerPara, TriggerPropConst.ACTIVE, null);
    }

    /// <summary>
    /// 触发属性作用
    /// </summary>
    private static bool ApplyTriggerProp(Property source, int stage, LPCMapping triggerPara, int triggerType, Property target = null)
    {
        // 需要触发触发属性对象不存在
        if (source == null)
            return false;

        // 获取角色的触发属性
        LPCMapping triggerPropMap = GetTriggerProp(source, stage, triggerType, triggerPara, target);

        // 没有触发属性
        if (triggerPropMap.Count == 0)
            return false;

        // 获取触发触发属性角色的信息
        LPCMapping sourceProfile = source.GetProfile();

        // 获取当前的时间
        LPCArray propData = null;
        CsvRow data = null;
        int calcScript = 0;
        bool isTriggered = false;

        // 遍历各个属性
        foreach (string propName in triggerPropMap.Keys)
        {
            // 获取属性数据
            propData = triggerPropMap.GetValue<LPCArray>(propName);

            // 获取配置信息
            data = TriggerPropCsv.FindByKey(propName);
            if (data == null)
                continue;

            // 计算触发属性
            LPCValue fixedValue = null;
            calcScript = data.Query<int>("calc_script");
            if (calcScript != 0)
            {
                // 调用触发脚本calc_script
                object ret = ScriptMgr.Call(calcScript, source, propData[0], triggerPara,
                    data.Query<LPCValue>("calc_args"), propData[1], propName);

                // 转换数据格式
                if (ret != null)
                    fixedValue = ret as LPCValue;
            }

            // 没有触发属性需要作用
            if (fixedValue == null)
                continue;

            // 更新触发属性cd
            TriggerPropCD(source, propName, propData[0]);

            // 该属性对作用方自己起效
            if (data.Query<int>("target_type") == TriggerPropConst.SELF)
                ApplyOn(source, target, data, fixedValue, propData[1], triggerPara, sourceProfile);
            else
                // 如果有反馈对象，则反弹，否则不处理
                ApplyOn(target, source, data, fixedValue, propData[1], triggerPara, sourceProfile);

            // 记录已经成功触发了触发属性
            isTriggered = true;
        }

        // 返回成功触发了触发属性标识
        return isTriggered;
    }

    /// <summary>
    /// 触发属性作用到角色身上
    /// </summary>
    private static void ApplyOn(Property target, Property source, CsvRow data, LPCValue propValue, LPCValue selectPara, LPCMapping triggerPara, LPCMapping sourceProfile)
    {
        // 对象不存在，不处理
        if (target == null)
            return;

        // 没有作用脚本，不处理
        int applyScript = data.Query<int>("apply_script");
        if (applyScript == 0)
            return;

        // 调用脚本
        ScriptMgr.Call(applyScript, target, source, sourceProfile, propValue, triggerPara,
            data.Query<LPCValue>("apply_args"), data.Query<string>("prop_name"), selectPara);
    }

    /// <summary>
    /// 角色的触发属性cd
    /// </summary>
    private static void TriggerPropCD(Property triggerOb, string propName, LPCValue propValue)
    {
        // 对象不存在，不处理
        if (triggerOb == null)
            return;

        // 获取配置信息
        CsvRow data = TriggerPropCsv.FindByKey(propName);
        if (data == null)
            return;

        // 获取配置信息
        LPCValue cdRounds = data.Query<LPCValue>("cd_rounds");
        int cdTi = 0;

        // 如果cd_time是配置的脚本，则调用公式计算
        if (cdRounds.IsInt)
        {
            cdTi = cdRounds.AsInt;
        }
        else if (cdRounds.IsString)
        {
            string splitStr = "script_";
            string[] arr = cdRounds.AsString.Split(splitStr.ToCharArray());
            if (arr.Length != 1)
                return;

            // 获取脚本编号
            int scriptNo = System.Convert.ToInt16(arr[0]);
            object ret = ScriptMgr.Call(scriptNo, propValue);
            if (ret == null)
                return;

            // 获取cd时间
            cdTi = (int)ret;
        }
        else
        {
            LogMgr.Trace("未知格式。");
        }

        // 触发属性不需要cd，不处理
        if (cdTi == 0)
            return;

        // 设角色的触发属性的cd信息
        triggerOb.SetTemp(string.Format("trigger_prop_cd/{0}", propName), LPCValue.Create(cdTi));
    }

    /// <summary>
    /// 获取角色的指定触发属性列表
    /// </summary>
    private static LPCMapping GetTriggerProp(Property source, int stage, int triggerType, LPCMapping triggerPara, Property target)
    {
        // 不包含该触发阶段
        List<CsvRow> triggerList;
        if (! mTriggerStageMap.TryGetValue(stage, out triggerList))
            return LPCMapping.Empty;

        // 获取角色的trigger属性列表
        LPCMapping triggerMap = source.QueryTemp<LPCMapping>("trigger");
        if (triggerMap == null || triggerMap.Count == 0)
            return LPCMapping.Empty;

        // 触发属性列表
        LPCMapping selectPropMap = new LPCMapping();
        LPCArray exclusionList = LPCArray.Empty;
        LPCArray dependSkills = null;
        LPCArray excludeStatus = null;
        string propName = string.Empty;
        int selectScript = 0;

        if (triggerList.Count > triggerMap.Count)
        {
            // 遍历属性
            foreach (string tPropName in triggerMap.Keys)
            {
                // 获取配置信息
                CsvRow data = TriggerPropCsv.FindByKey(tPropName);

                // 数据异常
                if (data == null)
                    continue;

                // 触发类型不一致，不处理
                // 触发阶段不一致，不处理
                if (data.Query<int>("stage") != stage ||
                    data.Query<int>("trigger_type") != triggerType)
                    continue;

                // 角色没有该属性
                propName = data.Query<string>("prop_name");

                // 该触发属性还在cd中，不处理
                if (IsCooldown(source, propName))
                    continue;

                // 判断触发该附加属性是否依赖技能
                dependSkills = data.Query<LPCArray>("depend_skill");
                if (dependSkills != null && dependSkills.Count != 0 &&
                    dependSkills.IndexOf(triggerPara.GetValue<int>("skill_id")) == -1)
                    continue;

                // 判断在角色当前状态下，是否可以触发该附加属性
                excludeStatus = data.Query<LPCArray>("exclude_status");
                if (excludeStatus != null && source.CheckStatus(excludeStatus))
                    continue;

                // 调用选择脚本
                selectScript = data.Query<int>("select_script");

                // 没有select_script脚本
                if (selectScript == 0)
                {
                    // 剔除互斥属性
                    exclusionList.Append(data.Query<LPCArray>("exclusion"));

                    // 添加到列表中
                    selectPropMap.Add(propName, new LPCArray(triggerMap[propName], LPCValue.Create()));

                    continue;
                }

                // 调用select_script脚本计算选择结果
                object selectRet = ScriptMgr.Call(selectScript, source, triggerMap[propName], triggerPara,
                    data.Query<LPCValue>("select_args"), target, propName);

                // 如果selectRet为true和false
                if (selectRet is Boolean)
                {
                    // 不能选择该属性
                    if (! (bool) selectRet)
                        continue;

                    // 剔除互斥属性
                    exclusionList.Append(data.Query<LPCArray>("exclusion"));

                    // 添加到列表中
                    selectPropMap.Add(propName, new LPCArray(triggerMap[propName], LPCValue.Create()));

                    continue;
                }

                // 如果返回的LPCValue(策划返回了特殊数据，该数据需要作为参数在后面使用)
                if (selectRet is LPCValue)
                {
                    // 剔除互斥属性
                    exclusionList.Append(data.Query<LPCArray>("exclusion"));

                    // 添加到列表中
                    selectPropMap.Add(propName, new LPCArray(triggerMap[propName], selectRet));

                    continue;
                }
            }
        }
        else
        {
            // 遍历各个属性
            foreach (CsvRow data in triggerList)
            {
                // 数据异常
                if (data == null)
                    continue;

                // 角色没有该属性
                propName = data.Query<string>("prop_name");
                if (! triggerMap.ContainsKey(propName))
                    continue;

                // 触发类型不一致，不处理
                if (data.Query<int>("trigger_type") != triggerType)
                    continue;

                // 该触发属性还在cd中，不处理
                if (IsCooldown(source, propName))
                    continue;

                // 判断触发该附加属性是否依赖技能
                dependSkills = data.Query<LPCArray>("depend_skill");
                if (dependSkills != null && dependSkills.Count != 0 &&
                    dependSkills.IndexOf(triggerPara.GetValue<int>("skill_id")) == -1)
                    continue;

                // 判断在角色当前状态下，是否可以触发该附加属性
                excludeStatus = data.Query<LPCArray>("exclude_status");
                if (excludeStatus != null && source.CheckStatus(excludeStatus))
                    continue;

                // 调用选择脚本
                selectScript = data.Query<int>("select_script");

                // 没有select_script脚本
                if (selectScript == 0)
                {
                    // 剔除互斥属性
                    exclusionList.Append(data.Query<LPCArray>("exclusion"));

                    // 添加到列表中
                    selectPropMap.Add(propName, new LPCArray(triggerMap[propName], LPCValue.Create()));

                    continue;
                }

                // 调用select_script脚本计算选择结果
                object selectRet = ScriptMgr.Call(selectScript, source, triggerMap[propName], triggerPara,
                    data.Query<LPCValue>("select_args"), target, propName);

                // 如果selectRet为true和false
                if (selectRet is Boolean)
                {
                    // 不能选择该属性
                    if (! (bool) selectRet)
                        continue;

                    // 剔除互斥属性
                    exclusionList.Append(data.Query<LPCArray>("exclusion"));

                    // 添加到列表中
                    selectPropMap.Add(propName, new LPCArray(triggerMap[propName], LPCValue.Create()));

                    continue;
                }

                // 如果返回的LPCValue(策划返回了特殊数据，该数据需要作为参数在后面使用)
                if (selectRet is LPCValue)
                {
                    // 剔除互斥属性
                    exclusionList.Append(data.Query<LPCArray>("exclusion"));

                    // 添加到列表中
                    selectPropMap.Add(propName, new LPCArray(triggerMap[propName], selectRet));

                    continue;
                }
            }
        }

        // 剔除需要剔除的属性
        for (int i = 0; i < exclusionList.Count; i++)
            selectPropMap.Remove(exclusionList[i].AsString);

        // 返回数据
        return selectPropMap;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        mTriggerStageMap.Clear();

        // 载入触发属性配置表信息
        mTriggerPropCsv = CsvFileMgr.Load("trigger_prop");

        // 触发阶段
        int stage = 0;

        // 遍历数据，统计触发属性阶段
        foreach (CsvRow data in mTriggerPropCsv.rows)
        {
            // 获取属性的触发阶段
            stage = data.Query<int>("stage");

            // 初始化数据
            if (!mTriggerStageMap.ContainsKey(stage))
            {
                mTriggerStageMap.Add(stage, new List<CsvRow>() { data });
                continue;
            }

            // 添加数据
            mTriggerStageMap[stage].Add(data);
        }
    }

    /// <summary>
    /// 判断属性是否在CD中
    /// </summary>
    public static bool IsCooldown(Property ob, string propName)
    {
        // 获取cd信息
        int cdTime = ob.QueryTemp<int>(string.Format("trigger_prop_cd/{0}", propName));

        // 判断是否在cd中
        return (cdTime == 0) ? false : true;
    }

    /// <summary>
    /// debuff之前触发属性
    /// </summary>
    public static bool DoDebuffBeforeTrigger(Property who, LPCMapping args)
    {
        // 根据stage_array分别触发需要触发属性
        return ApplyTriggerProp(who, TriggerPropConst.DEBUFF_BEFORE, args, TriggerPropConst.ACTIVE, null);
    }

    /// <summary>
    /// debuff之前触发属性
    /// </summary>
    public static bool DoDebuffAfterTrigger(Property who, LPCMapping args)
    {
        // 根据stage_array分别触发需要触发属性
        return ApplyTriggerProp(who, TriggerPropConst.DEBUFF_AFTER, args, TriggerPropConst.ACTIVE, null);
    }

    /// <summary>
    /// 受创之前触发属性
    /// </summary>
    public static bool DoReceiveDamageBeforeTrigger(Property who, LPCMapping args)
    {
        // 根据stage_array分别触发需要触发属性
        return ApplyTriggerProp(who, TriggerPropConst.DAMAGE_BEFORE, args, TriggerPropConst.PASSIVE, null);
    }

    /// <summary>
    /// 释放技能触发
    /// </summary>
    public static bool DoCastSkillTrigger(Property who, int skillId, LPCMapping args)
    {
        // 获取技能信息
        CsvRow data = SkillMgr.GetSkillInfo(skillId);
        if (data == null)
            return false;

        // 技能触发标识
        int triggerFlag = data.Query<int>("trigger_flag");
        if (triggerFlag != 1)
            return false;

        // 根据stage_array分别触发需要触发属性
        return ApplyTriggerProp(who, TriggerPropConst.CAST_SKILL, args, TriggerPropConst.ACTIVE, null);
    }

    /// <summary>
    /// 触发转换阶段
    /// </summary>
    public static bool DoTriggerChangeStage(Property who)
    {
        // 没有凶手信息，不处理
        LPCMapping assailantInfo = who.QueryTemp<LPCMapping>("assailant_info");
        if (assailantInfo == null)
            return false;

        // 没有攻击者不触发
        Property target = Rid.FindObjectByRid(assailantInfo.GetValue<string>("rid"));

        // 获取导致死亡的技能id
        LPCMapping triggerPara = new LPCMapping();
        triggerPara.Add("skill_id", assailantInfo["skill_id"]);

        // 触发受创角色自己的触发属性，该角色的属性是被动触发的
        // 是否触发了濒临死亡的属性
        return ApplyTriggerProp(who, TriggerPropConst.CHANGE_STAGE, triggerPara, TriggerPropConst.PASSIVE, target);
    }

    /// <summary>
    /// 触发不死的属性
    /// </summary>
    public static bool TriggerNearDie(Property who)
    {
        // 没有凶手信息，不处理
        LPCMapping assailantInfo = who.QueryTemp<LPCMapping>("assailant_info");
        if (assailantInfo == null)
            return false;

        // 没有攻击者不触发
        Property source = Rid.FindObjectByRid(assailantInfo.GetValue<string>("rid"));

        // 获取导致死亡的技能id
        LPCMapping triggerPara = new LPCMapping();
        triggerPara.Add("skillId", assailantInfo ["skillId"]);

        // 1. 触发受创角色自己的触发属性，该角色的属性是被动触发的
        return ApplyTriggerProp(who, TriggerPropConst.NEAR_DIE, triggerPara, TriggerPropConst.PASSIVE, source);
    }

    ///<summary>
    /// 回合结束，CD要减少
    /// </summary>
    public static void DoReduceCd(Property who)
    {
        // 获取当前的cd信息
        LPCMapping cdMap = who.QueryTemp<LPCMapping>("trigger_prop_cd");
        if (cdMap == null)
            return;

        // 需要清除列表
        LPCMapping newCdMap = LPCMapping.Empty;

        // 遍历各个cd
        foreach (string key in cdMap.Keys)
        {
            // 技能cd需要清除
            if (cdMap[key].AsInt <= 1)
                continue;

            // 技能cd减少一回合
            newCdMap.Add(key, Math.Max(cdMap[key].AsInt - 1, 0));
        }

        // 重置cd数据
        who.SetTemp("trigger_prop_cd", LPCValue.Create(newCdMap));
    }

    #endregion
}
