/// <summary>
/// Attack.cs
/// Create by zhaozy 2014-11-18
/// 攻击策略
/// </summary>

using System;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 战斗输入指令方式
/// </summary>
public class AttackImputType
{
    // 自动战斗指令输入方式
    // 如果是BOSS关卡自动战斗（输入方式直接是攻击目标选择顺序id）
    public const int AIT_MANUAL = 0;     // 手动选择目标
    public const int AIT_LOCK = 1;       // 手动锁定目标
    public const int AIT_RANDOM = 2;     // 随机收取方式
}

public class Attack : Tactics
{
    #region 内部接口

    /// <summary>
    /// 收集各个hit点的hit目标
    /// </summary>
    private static LPCMapping DoCollectHitEntityList(Property sourceOb, int skillId, string cookie, LPCMapping para)
    {
        // 读取skill表中相关技能的信息
        CsvRow skillInfo = SkillMgr.GetSkillInfo(skillId);

        // 没有配置的技能不处理
        if (skillInfo == null)
            return LPCMapping.Empty;

        // 获取技能hit_scripts列表
        LPCValue hitScripts = skillInfo.Query<LPCValue>("hit_scripts");

        // 技能命中脚本配置错误
        if (!hitScripts.IsArray)
            return LPCMapping.Empty;

        // 转换数据格式
        LPCArray hitScriptList = hitScripts.AsArray;
        LPCArray hitScript = LPCArray.Empty;
        LPCMapping hitEntityMap = LPCMapping.Empty;
        int scriptNo = 0;

        // 获取连击次数
        int comboTimes = para.GetValue<int>("combo_times");

        // 遍历各个hit点
        for (int i = 0; i < hitScriptList.Count; i++)
        {
            // 获取脚本数据
            hitScript = hitScriptList[i].AsArray;

            // 数据格式不正确
            if (hitScript == null ||
                hitScript.Count != 4 ||
                !hitScript[0].IsInt ||
                !hitScript[1].IsInt ||
                !hitScript[2].IsInt)
                continue;

            // 获取收集脚本
            scriptNo = hitScript[1].AsInt;

            // 调用脚本收集目标
            List<Property> selectList = ScriptMgr.Call(scriptNo, sourceOb, skillId, para, skillInfo, hitScript[3], hitEntityMap, cookie) as List<Property>;

            // 没有收集到任何目标
            if (selectList.Count == 0)
                continue;

            // 遍历各个目标
            LPCArray entityList = LPCArray.Empty;
            foreach (Property ob in selectList)
            {
                // 对象不目标不存在
                if (ob == null)
                    continue;

                // 锁定目标
                ob.Lock(cookie, comboTimes);

                // 添加数据
                entityList.Add(ob.GetRid());
            }

            // 添加数据
            hitEntityMap.Add(i, entityList);
        }

        // 返回数据
        return hitEntityMap;
    }

    /// <summary>
    /// Gets the imput action.
    /// </summary>
    /// <returns><c>true</c>, if imput action was gotten, <c>false</c> otherwise.</returns>
    /// <param name="ob">Ob.</param>
    /// <param name="type">Type.</param>
    /// <param name="action">Action.</param>
    private bool GetImputAction(Property ob, int type, out LPCMapping action)
    {
        // 初始化数据
        action = LPCMapping.Empty;

        // 如果是非战斗客户端需要统计详细的操作流程
        // 副本怪物通过AI控制还原战斗过程
        // 只是需要记录ROUND_TYPE_NORMAL, ROUND_TYPE_ADDITIONAL, ROUND_TYPE_GASSER三个类型的战斗（其他类型都有输入攻击目标和相应技能）
        if (ob.Query<int>("instance_resource") != 0 ||
            (type != RoundCombatConst.ROUND_TYPE_NORMAL &&
             type != RoundCombatConst.ROUND_TYPE_ADDITIONAL) &&
             type != RoundCombatConst.ROUND_TYPE_GASSER)
            return true;

        // 获取副本信息
        string rid = ob.Query<string>("instance/rid");
        InstanceBase instanceOb = Rid.FindObjectByRid(rid) as InstanceBase;

        // 获取一个详细操作
        LPCArray imput = instanceOb.GetAction();

        // 无效操作
        if (imput == null)
        {
            // 通知失败
            instanceOb.DoInstanceFail(ob);
            return false;
        }

        // 检查sourOb是否合法
        Property sourOb = Property.FindObjectByUniqueId(imput[0].AsInt);
        if (sourOb == null || ob.UniqueId != sourOb.UniqueId)
        {
            LogMgr.Trace("{0} 获取到了无效输入行为{1}。", ob.UniqueId, LPCValue.Create(imput).AsString);

            // 通知失败
            instanceOb.DoInstanceFail(ob);
            return false;
        }

        // 判断imput对象是否合法
        Property imputOb = Property.FindObjectByUniqueId(imput[2].AsInt);
        if (imputOb == null)
        {
            LogMgr.Trace("imput_unique_id = {0} 对象不存在， 行为无效{1}。",
                imput[2].AsInt, LPCValue.Create(imput).AsString);

            // 通知失败
            instanceOb.DoInstanceFail(ob);
            return false;
        }

        // 如果是手动输入方式, 直接pick_rid和skill_id
        LPCValue imputType = imput[1];
        if (imputType != null &&
            imputType.IsInt &&
            imputType.AsInt == AttackImputType.AIT_MANUAL)
        {
            // 获取技能id
            action.Add("imput_type", imput[1]);
            action.Add("pick_rid", imputOb.GetRid());
            action.Add("skill_id", imput[3]);
        }
        else
        {
            // 获取技能id
            action.Add("imput_type", imput[1]);
            action.Add("imput_unique_id", imput[2]);
            action.Add("imput_skill_id", imput[3]);
        }

        // 获取输入成功
        return true;
    }

    /// <summary>
    /// Saves the imput action.
    /// </summary>
    /// <param name="ob">Ob.</param>
    /// <param name="type">Type.</param>
    /// <param name="skillId">Skill identifier.</param>
    /// <param name="action">Action.</param>
    private void SaveImputAction(Property sourOb, int type, Property targetOb, int skillId, LPCValue imputType)
    {
        // 如果是非战斗客户端需要统计详细的操作流程
        // 副本怪物不需要记录（AI控制还原战斗过程）
        // 只是需要记录ROUND_TYPE_NORMAL, ROUND_TYPE_ADDITIONAL, ROUND_TYPE_GASSER三个类型的战斗（其他类型都有输入攻击目标和相应技能）
        if (sourOb.Query<int>("instance_resource") != 0 ||
            (type != RoundCombatConst.ROUND_TYPE_NORMAL &&
             type != RoundCombatConst.ROUND_TYPE_ADDITIONAL && 
             type != RoundCombatConst.ROUND_TYPE_GASSER))
            return;

        // 获取副本信息
        string rid = sourOb.Query<string>("instance/rid");
        Instance instanceOb = Rid.FindObjectByRid(rid) as Instance;

        // 副本对象不存在
        if (instanceOb == null)
            return;

        // 保存操作到副本对象
        instanceOb.AddLevelAction(new LPCArray( sourOb.UniqueId, imputType, targetOb.UniqueId, skillId ));
    }

    /// <summary>
    /// 验证策略执行接口
    /// </summary>
    private bool DoRestoreTrigger(Property ob, LPCMapping args)
    {
        // 获取回合类型
        int type = args.GetValue<int>("type");

        // 获取输入行为失败
        LPCMapping action = LPCMapping.Empty;
        if (! GetImputAction(ob, type, out action))
        {
            LogMgr.Trace("{0} 获取输入行为失败, args = {1}。", ob.UniqueId, LPCValue.Create(args).AsString);
            return false;
        }

        // 追加操作行为
        args.Append(action);

        // 获取策略执行skill_id和pick_rid
        int skillId = args.GetValue<int>("skill_id");
        string pickRid = args.GetValue<string>("pick_rid");

        // 没有任何攻击信息，则默认使用AI自动脚本选择
        if (skillId == 0 && string.IsNullOrEmpty(pickRid))
        {
            // 调用脚本收集作用源信息
            int scriptNo = ob.BasicQueryNoDuplicate<int>("ai_script");

            // 没有配置脚本不处理
            if (scriptNo == 0)
            {
                LogMgr.Trace("{0} 缺少IA脚本，请检查！", ob.GetClassID());
                return false;
            }

            // 通过脚本选择释放技能和选择释放目标
            object ret = ScriptMgr.Call(scriptNo, ob,
                ob.BasicQueryNoDuplicate<LPCMapping>("ai_args"), args);

            // 返回数据不合格
            if (ret == null)
            {
                LogMgr.Trace("{0} AI脚本没有选择到目标 {1}！", ob.UniqueId, LPCValue.Create(args).AsString);
                return false;
            }

            // 转换数据类型
            LPCMapping aiRet = ret as LPCMapping;

            // 缓存数据
            skillId = aiRet.GetValue<int>("skill_id");
            pickRid = aiRet.GetValue<string>("pick_rid");
        }
        else
        {
            // 判断是否输入主动释放技能的技能id
            if (skillId == 0)
            {
                LogMgr.Trace("非AI模式，请指定释放技能id");
                return false;
            }

            // 判断是否输入主动释放技能的目标
            if (string.IsNullOrEmpty(pickRid))
            {
                LogMgr.Trace("非AI模式，请指定释放技能目标rid");
                return false;
            }
        }

        // 判断技能是能可以作用到目标
        Property targetOb = Rid.FindObjectByRid(pickRid);
        if (! SkillMgr.CanApplySkill(ob, skillId, targetOb))
            return false;

        // 生成一个唯一cookie
        string cookie = string.Empty;
        if (args.ContainsKey("cookie"))
            cookie = args.GetValue<string>("cookie");
        else
            cookie = Game.NewCookie(ob.GetRid());

        // 获取技能消耗
        LPCMapping costMap = SkillMgr.GetCasTCost(ob, skillId);

        // 构建参数
        LPCMapping para = new LPCMapping();
        para.Add("skill_id", skillId);   // 释放技能
        para.Add("cookie", cookie);      // 技能cookie
        para.Add("rid", ob.GetRid());    // 释放技能角色rid
        para.Add("pick_rid", pickRid);   // 技能拾取目标
        para.Add("skill_cost", costMap); // 获取当前技能消耗
        para.Add("type", type);          // 添加type

        // 获取技能original_cookie
        // 如果是二段技能则这个original_cookie是第一段技能的cookie
        // 二段技能在释放技能的时候需要主动传递original_cookie
        para.Add("original_cookie", args.GetValue<string>("original_cookie", cookie));

        // 如果角色当前执行攻击是在角色自身的攻击回合，则需要添加当前的回合数
        if (args.ContainsKey("rounds"))
            para.Add("rounds", args["rounds"]);

        // 添加无效击杀者信息
        if (args.ContainsKey("invalid_assailant"))
            para.Add("invalid_assailant", args["invalid_assailant"]);

        // 添加发起协同者信息
        if (args.ContainsKey("joint_rid"))
            para.Add("joint_rid", args["joint_rid"]);

        // 添加附加参数
        if (args.ContainsKey("extra_para") && args["extra_para"].IsMapping)
            para.Append(args["extra_para"].AsMapping);

        // 技能连击combo_times
        para.Add("combo_times", SkillMgr.GetComboTimes(ob, skillId, para));

        // 提前收集技能各个hit点命中目标，并且锁定目标
        para.Add("hit_entity_map", DoCollectHitEntityList(ob, skillId, cookie, para));

        // 触发释放技能属性
        TriggerPropMgr.DoCastSkillTrigger(ob, skillId, para);

        // 通知在战斗系统释放技能
        ob.Actor.DoActionSet(string.Format("{0}_cast", skillId), cookie, para);

        // 技能CD
        CdMgr.SkillCooldown(ob, skillId);

        // 扣除技能消耗
        ob.CostAttrib(costMap);

        // 触发成功
        return true;
    }

    /// <summary>
    /// 正常策略执行接口
    /// </summary>
    private bool DoTrigger(Property ob, LPCMapping args)
    {
        // 获取回合类型
        int type = args.GetValue<int>("type");

        // 获取策略执行skill_id和pick_rid
        int skillId = args.GetValue<int>("skill_id");
        string pickRid = args.GetValue<string>("pick_rid");

        // 战斗指令输入方式
        LPCValue imputType;

        // 没有任何攻击信息，则默认使用AI自动脚本选择
        if (skillId == 0 && string.IsNullOrEmpty(pickRid))
        {
            // 调用脚本收集作用源信息
            int scriptNo = ob.BasicQueryNoDuplicate<int>("ai_script");

            // 没有配置脚本不处理
            if (scriptNo == 0)
            {
                LogMgr.Error("{0} 缺少IA脚本，请检查！", ob.GetClassID());
                return false;
            }

            // 通过脚本选择释放技能和选择释放目标
            object ret = ScriptMgr.Call(scriptNo, ob,
                ob.BasicQueryNoDuplicate<LPCMapping>("ai_args"), args);

            // 返回数据不合格
            if (ret == null)
                return false;

            // 转换数据类型
            LPCMapping aiRet = ret as LPCMapping;

            // 缓存数据
            skillId = aiRet.GetValue<int>("skill_id");
            pickRid = aiRet.GetValue<string>("pick_rid");
            imputType = aiRet.GetValue<LPCValue>("imput_type");
        }
        else
        {
            // 判断是否输入主动释放技能的技能id
            if (skillId == 0)
            {
                LogMgr.Error("非AI模式，请指定释放技能id, args : {0}", args._GetDescription(4));
                return false;
            }

            // 判断是否输入主动释放技能的目标
            if (string.IsNullOrEmpty(pickRid))
            {
                LogMgr.Error("非AI模式，请指定释放技能目标rid, args : {0}", args._GetDescription(4));
                return false;
            }

            // 手动输入战斗指令
            imputType = LPCValue.Create(AttackImputType.AIT_MANUAL);
        }

        // 判断技能是能可以作用到目标
        Property targetOb = Rid.FindObjectByRid(pickRid);
        if (!SkillMgr.CanApplySkill(ob, skillId, targetOb))
            return false;

        // 生成一个唯一cookie
        string cookie = string.Empty;
        if (args.ContainsKey("cookie"))
            cookie = args.GetValue<string>("cookie");
        else
            cookie = Game.NewCookie(ob.GetRid());

        // 获取技能消耗
        LPCMapping costMap = SkillMgr.GetCasTCost(ob, skillId);

        // 构建参数
        LPCMapping para = new LPCMapping();
        para.Add("skill_id", skillId);   // 释放技能
        para.Add("cookie", cookie);      // 技能cookie
        para.Add("rid", ob.GetRid());    // 释放技能角色rid
        para.Add("pick_rid", pickRid);   // 技能拾取目标
        para.Add("skill_cost", costMap); // 获取当前技能消耗
        para.Add("type", type);          // 添加type

        // 获取技能original_cookie
        // 如果是二段技能则这个original_cookie是第一段技能的cookie
        // 二段技能在释放技能的时候需要主动传递original_cookie
        para.Add("original_cookie", args.GetValue<string>("original_cookie", cookie));

        // 如果角色当前执行攻击是在角色自身的攻击回合，则需要添加当前的回合数
        if (args.ContainsKey("rounds"))
            para.Add("rounds", args["rounds"]);

        // 添加无效击杀者信息
        if (args.ContainsKey("invalid_assailant"))
            para.Add("invalid_assailant", args["invalid_assailant"]);

        // 添加发起协同者信息
        if (args.ContainsKey("joint_rid"))
            para.Add("joint_rid", args["joint_rid"]);

        // 添加附加参数
        if (args.ContainsKey("extra_para") && args["extra_para"].IsMapping)
            para.Append(args["extra_para"].AsMapping);

        // 技能连击combo_times
        para.Add("combo_times", SkillMgr.GetComboTimes(ob, skillId, para));

        // 保存操作
        SaveImputAction(ob, type, targetOb, skillId, imputType);

        // 提前收集技能各个hit点命中目标，并且锁定目标
        para.Add("hit_entity_map", DoCollectHitEntityList(ob, skillId, cookie, para));

        // 触发释放技能属性
        TriggerPropMgr.DoCastSkillTrigger(ob, skillId, para);

        // 通知在战斗系统释放技能
        ob.Actor.DoActionSet(string.Format("{0}_cast", skillId), cookie, para);

        // 技能CD
        CdMgr.SkillCooldown(ob, skillId);

        // 扣除技能消耗
#if UNITY_EDITOR
        // 扣除施放技能的开销（如技力）
        if (ME.user.QueryTemp<int>("ignore_cost") != 1)
            ob.CostAttrib(costMap);
#else
        ob.CostAttrib(costMap);
#endif

        // 添加技能飘血框
        if (SkillMgr.IsShowSkillTip(skillId))
            BloodTipMgr.AddSkillTip(ob, skillId);

        // 构建事件参数
        LPCMapping eventPara = new LPCMapping();
        eventPara.Add("rid", pickRid);
        eventPara.Add("source_rid", ob.GetRid());

        // 抛出ATTACK事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_ATTACK, MixedValue.NewMixedValue<LPCMapping>(eventPara), false);

        // 触发成功
        return true;
    }

    #endregion

    #region 对外接口

    /// <summary>
    /// 策略执行入口
    /// </summary>
    public override bool Trigger(params object[] _params)
    {
        // 获取怪物对象
        Property ob = _params[0] as Property;
        LPCMapping args = _params[1] as LPCMapping;

        // 没有actor对象不能释放技能
        if (ob.Actor == null)
            return false;

        // 验证客户端和正常客户端处理流程不一致
        // 判断是否是回放战斗
        if (AuthClientMgr.IsAuthClient ||
            ob.Query<LPCValue>("is_video_actor") != null)
            return DoRestoreTrigger(ob, args);

        // 正常战斗
        return DoTrigger(ob, args);
    }

    #endregion
}
