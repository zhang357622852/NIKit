/// <summary>
/// Char.cs
/// Copy from zhangyg 2014-10-22
/// 角色基类
/// </summary>

using System;
using UnityEngine;
using LPC;

/// <summary>
/// 角色基类
/// </summary>
public abstract class Char : Property
{
    #region 内部接口

    /// <summary>
    /// 执行死亡
    /// </summary>
    private void DoDieImpl()
    {
        // 附加死亡状态
        this.ApplyStatus("DIED", CALC_DIE_CONDITION.Call(this));

        // 结束角色当前主要行动序列
        if (this.Actor != null)
            this.Actor.StopActionSet(ActionSetType.AST_ACT);

        // 角色死亡0.2渐变隐藏模型, 如果角色有死亡动作需要播放释放动作
        string dieActionName = string.Empty;
        if (this.Query<LPCValue>("die_action") == null)
            dieActionName = "alpha_die";
        else
            dieActionName = "action_die";

        // 添加回合round列表
        string actionCookie = string.Format("{0}_{1}", this.GetRid(), "die");
        RoundCombatMgr.AddRoundAction(actionCookie, this);

        // 播放状态序列
        this.Actor.DoActionSet(dieActionName, actionCookie, LPCMapping.Empty);

        // 抛出死亡事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_DIE, MixedValue.NewMixedValue<Property>(this), true, true);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 构造函数
    /// </summary>
    public Char(LPCMapping data)
        : base(data)
    {
    }

    /// <summary>
    /// 执行怪物死亡
    /// </summary>
    public void DoDie()
    {
        // 1. 角色已经死亡处理
        if (this.CheckStatus("DIED"))
            return;

        // 尝试触发濒死的属性
        if (TriggerPropMgr.TriggerNearDie(this))
            return;

        // 对象死亡尝试触发转换阶段
        if (TriggerPropMgr.DoTriggerChangeStage(this))
        {
            // 暂停当前回合战斗
            RoundCombatMgr.PauseRoundCombat();
            return;
        }

        // 正在死亡中，不处理
        if (this.QueryTemp<int>("dying") == 1)
            return;

        // 打上死亡标识
        this.SetTemp("dying", LPCValue.Create(1));

        try
        {
            // 执行死亡
            DoDieImpl();
        }
        catch (Exception e)
        {
            LogMgr.Exception(e);
        }

        // 删除标识
        this.DeleteTemp("dying");
    }

    /// <summary>
    /// 闪避了来自source_profile的攻击
    /// </summary>
    public void Dodge(LPCMapping sourceProfile)
    {
        // 构建事件参数
        LPCMapping eventPara = new LPCMapping();
        eventPara.Add("target_rid", LPCValue.Create(this.GetRid()));
        eventPara.Add("source_profile", LPCValue.Create(sourceProfile));

        // 抛出受创事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_DODGE, MixedValue.NewMixedValue<LPCMapping>(eventPara), true, true);
    }

    /// <summary>
    /// 受到了来自source_profile指明对象造成的治疗
    /// 类别：cure_type，治疗点数：points 映射(属性 : 点数)
    /// </summary>
    public bool ReceiveCure(LPCMapping sourceProfile, int cureType, LPCMapping cureMap)
    {
        LPCMapping args = LPCMapping.Empty;
        args.Add("rid", this.GetRid());
        args.Add("cure_type", cureType);
        args.Add("source_profile", sourceProfile);
        args.Add("cure_map", cureMap);

        // 如果是技能造成的回血
        if (!sourceProfile.ContainsKey("skill_id"))
        {
            // 直接执行args
            DoReceiveCure(args);

            // 治愈成功
            return true;
        }

        // 获取技能id
        int skillId = sourceProfile.GetValue<int>("skill_id");
        string cureStatus = string.Empty;
        string cureAct = string.Empty;

        // 如果包含hit_index，则需要判断是否有配置专门的hit_index受创动作和光效序列名
        if (sourceProfile.ContainsKey("hit_index"))
        {
            int hitIndex = sourceProfile.GetValue<int>("hit_index");

            cureStatus = string.Format("{0}_cure_status#{1}", skillId, hitIndex);
            cureAct = string.Format("{0}_cure_act#{1}", skillId, hitIndex);

            // 获取受创动作和光效序列名
            if (!CombatActionMgr.HasActionSetData(cureStatus) &&
                !CombatActionMgr.HasActionSetData(cureAct))
            {
                cureStatus = string.Format("{0}_cure_status", skillId);
                cureAct = string.Format("{0}_cure_act", skillId);
            }
        }
        else
        {
            cureStatus = string.Format("{0}_cure_status", skillId);
            cureAct = string.Format("{0}_cure_act", skillId);
        }

        // 如果不包含status和act
        if (!CombatActionMgr.HasActionSetData(cureStatus) &&
            !CombatActionMgr.HasActionSetData(cureAct))
        {
            // 直接执行args
            DoReceiveCure(args);

            // 治愈成功
            return true;
        }

        // 添加回合行动列表中
        string cookie = Game.NewCookie(this.GetRid());
        LPCMapping roundPara = LPCMapping.Empty;
        roundPara.Add ("skill_id", skillId);

        // 播放cure effect
        bool doRet = this.Actor.DoActionSet(
                         cureStatus,
                         cookie,
                         args);

        // 如果播放damage_status成功
        if (doRet)
            RoundCombatMgr.AddRoundAction(cookie, this, roundPara);

        // 播放cure action
        if (NEED_CURE_ACT.Call(this, args))
        {
            // 添加回合行动列表中
            cookie = Game.NewCookie(this.GetRid());

            // 播放cure action
            doRet = this.Actor.DoActionSet(
                cureAct,
                cookie,
                args);

            // 如果播放action成功
            if (doRet)
                RoundCombatMgr.AddRoundAction(cookie, this, roundPara);
        }

        // 治愈成功
        return true;
    }

    /// <summary>
    /// Dos the receive cure.
    /// </summary>
    /// <param name="args">Arguments.</param>
    public void DoReceiveCure(LPCMapping args)
    {
        // 获取参数
        int cureType = args.GetValue<int>("cure_type");
        LPCMapping sourceProfile = args.GetValue<LPCMapping>("source_profile");
        LPCMapping cureMap = args.GetValue<LPCMapping>("cure_map");

        // 限制被治疗不处理
        if (! CAN_RECEIVE_CURE.Call(this, cureType, cureMap))
            return;

        LPCMapping eventPara = new LPCMapping();
        LPCMapping realCureMap = LPCMapping.Empty;
        int attribValue = 0;
        int addValue = 0;

        // 如果是最大属性治愈，需要单独处理
        if ((cureType & CombatConst.CURE_TYPE_MAX_ATTRIB) == CombatConst.CURE_TYPE_MAX_ATTRIB)
        {
            string attribPath = string.Empty;

            // 遍历最大属性伤害，这个地方只是记录的是扣除数值（纯粹是一个加值）
            foreach (string attrib in cureMap.Keys)
            {
                // 获取当前属性数值
                attribPath = string.Format("attrib_addition/{0}", attrib);
                attribValue = this.Query<int>(attribPath);

                // 计算属性加值
                addValue = Math.Min(0 - attribValue, cureMap[attrib].AsInt);
                realCureMap.Add(attrib, addValue);

                // 重新设置属性数值
                this.Set(attribPath, LPCValue.Create(attribValue + addValue));

                // 刷新Improvement下指定属性
                PropMgr.RefreshAttrib(this, attrib);
            }

            // 记录治愈相关数据
            eventPara.Add("cure_map", cureMap);
            eventPara.Add("real_cure_map", realCureMap);
        }
        else
        {
            LPCMapping fixedCureMap = new LPCMapping();
            fixedCureMap = CALC_FIXED_CURE_MAP.Call(this, cureMap);

            // 遍历各个属性
            foreach (string attribName in fixedCureMap.Keys)
            {
                // 获取当前属性数值
                attribValue = this.Query<int>(attribName);

                // 获取实际add数值
                addValue = Math.Min(this.QueryAttrib(string.Format("max_{0}", attribName)) - attribValue,
                    fixedCureMap[attribName].AsInt);

                // 添加到实际治愈列表中
                realCureMap.Add(attribName, addValue);

                // 计算新的属性数值
                this.Set(attribName, LPCValue.Create(attribValue + addValue));
            }

            // 记录治愈相关数据
            eventPara.Add("cure_map", fixedCureMap);
            eventPara.Add("real_cure_map", realCureMap);
        }

        // 构建事件参数
        eventPara.Add("rid", this.GetRid());
        eventPara.Add("cure_type", cureType);
        eventPara.Add("source_profile", sourceProfile);

        // 治疗以后附加处理
        WHEN_CURED.Call(this, sourceProfile, cureType, cureMap);

        // 抛出治疗事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_RECEIVE_CURE, MixedValue.NewMixedValue<LPCMapping>(eventPara), true, true);
    }

    /// <summary>
    /// 受到了来自source_profile指明对象造成的伤害
    /// 类别：damage_type，伤害信息：points 映射(属性 : 点数)
    /// </summary>
    public bool ReceiveDamage(LPCMapping sourceProfile, int damageType, LPCMapping damageMap)
    {
        LPCMapping args = LPCMapping.Empty;
        args.Add("rid", this.GetRid());
        args.Add("damage_type", damageType);
        args.Add("source_profile", sourceProfile);
        args.Add("damage_map", damageMap);

        // 执行受创前触发
        TriggerPropMgr.DoReceiveDamageBeforeTrigger(this, args);

        // 如果不是技能造成的伤害
        // 如果是分摊伤害效果也不需要做技能序列表现
        if (!sourceProfile.ContainsKey("skill_id") ||
            damageMap.ContainsKey("link_id"))
        {
            // 直接执行args
            DoReceiveDamage(args);

            // 治愈成功
            return true;
        }

        // 获取技能id
        int skillId = sourceProfile.GetValue<int>("skill_id");
        string damageStatus = string.Empty;
        string damageAct = string.Empty;

        // 如果包含hit_index，则需要判断是否有配置专门的hit_index受创动作和光效序列名
        if (sourceProfile.ContainsKey("hit_index"))
        {
            int hitIndex = sourceProfile.GetValue<int>("hit_index");

            damageStatus = string.Format("{0}_damage_status#{1}", skillId, hitIndex);
            damageAct = string.Format("{0}_damage_act#{1}", skillId, hitIndex);

            // 获取受创动作和光效序列名
            if (!CombatActionMgr.HasActionSetData(damageStatus) &&
                !CombatActionMgr.HasActionSetData(damageAct))
            {
                damageStatus = string.Format("{0}_damage_status", skillId);
                damageAct = string.Format("{0}_damage_act", skillId);
            }
        }
        else
        {
            damageStatus = string.Format("{0}_damage_status", skillId);
            damageAct = string.Format("{0}_damage_act", skillId);
        }

        // 如果不包含damage status和act
        if (!CombatActionMgr.HasActionSetData(damageStatus) &&
            !CombatActionMgr.HasActionSetData(damageAct))
        {
            // 直接执行args
            DoReceiveDamage(args);

            // 治愈成功
            return true;
        }

        // 添加回合行动列表中
        string cookie = Game.NewCookie(this.GetRid());

        // 播放受创effect
        bool doRet = this.Actor.DoActionSet(
                         damageStatus,
                         cookie,
                         args);

        // 如果播放damage_status成功
        if (doRet)
            RoundCombatMgr.AddRoundAction(cookie, this);

        // 播放受创action
        if (NEED_DAMAGE_ACT.Call(this, args))
        {
            // 添加回合行动列表中
            cookie = Game.NewCookie(this.GetRid());

            // 播放受创action
            doRet = this.Actor.DoActionSet(
                damageAct,
                cookie,
                args);

            // 如果播放action成功
            if (doRet)
                RoundCombatMgr.AddRoundAction(cookie, this);
        }

        // 返回受创成功
        return true;
    }

    /// <summary>
    /// Dos the receive damage.
    /// </summary>
    /// <param name="args">Arguments.</param>
    public void DoReceiveDamage(LPCMapping args)
    {
        // 获取参数
        int damageType = args.GetValue<int>("damage_type");

        // 特殊情况，角色不受创
        if (! CAN_RECEIVE_DAMAGE.Call(this, damageType))
            return;

        LPCMapping sourceProfile = args.GetValue<LPCMapping>("source_profile");
        LPCMapping damageMap = args.GetValue<LPCMapping>("damage_map");

        // 记录原始伤害
        LPCValue originalDamage = LPCValue.Duplicate(damageMap);
        LPCValue shieldAbsorbDamage = null;

        // 如果是mp伤害
        if ((damageType & CombatConst.DAMAGE_TYPE_MP) == CombatConst.DAMAGE_TYPE_MP)
        {
            // 扣篮伤害特殊处理
            damageMap = CALC_MP_DAMAGE_FIXED.Call(this, damageType, damageMap, sourceProfile);
        }
        else if ((damageType & CombatConst.DAMAGE_TYPE_MAX_ATTRIB) == CombatConst.DAMAGE_TYPE_MAX_ATTRIB)
        {
            // 暂时不做任何处理
        }
        else
        {
            // 伤害分摊
            damageMap = LINK_DAMAGE.Call(this, damageType, damageMap, sourceProfile);

            // 伤害转移
            damageMap = TRANS_DAMAGE.Call(this, damageType, damageMap, sourceProfile);

            // 无敌
            damageMap = INVINCIBLE_DAMAGE.Call(this, damageType, damageMap, sourceProfile);

            // 记录护盾吸收伤害前的伤害数值
            shieldAbsorbDamage = damageMap.GetValue<LPCValue>("points");

            // 护盾吸收伤害
            damageMap = SHIELD_ABSORB_DAMAGE.Call(this, damageType, damageMap, sourceProfile);

            // 免死专用处理
            damageMap = B_NO_DIE_DAMAGE.Call(this, damageType, damageMap, sourceProfile);
        }

        // 如果削减以后的伤害为空则直接返回
        if (damageMap == null)
            return;

        // 检查damageType有没有被篡改
        LPCMapping points = damageMap.GetValue<LPCMapping>("points");
        damageType = damageMap.GetValue<int>("damage_type");

        // 当前属性数值
        int attribValue = 0;

#if UNITY_EDITOR

        // 获取无敌标识
        bool invincibleFlag = false;

        // 如果是验证客户端不需要判断无敌标识
        if (! AuthClientMgr.IsAuthClient)
        {
            if (this.CampId == CampConst.CAMP_TYPE_DEFENCE)
                invincibleFlag = (ME.user.QueryTemp<int>("defence_invincible") == 1);
            else
                invincibleFlag = (ME.user.QueryTemp<int>("attack_invincible") == 1);
        }

        // 执行受创（如果是攻方成员则需要判断是否有无敌标识）
        if (! invincibleFlag)
        {
            // 如果是最大属性伤害，需要单独处理
            if ((damageType & CombatConst.DAMAGE_TYPE_MAX_ATTRIB) == CombatConst.DAMAGE_TYPE_MAX_ATTRIB)
            {
                string attribPath = string.Empty;

                // 遍历最大属性伤害，这个地方只是记录的是扣除数值（纯粹是一个加值）
                foreach (string attrib in points.Keys)
                {
                    // 获取当前属性数值
                    attribPath = string.Format("attrib_addition/{0}", attrib);
                    attribValue = this.Query<int>(attribPath);

                    // 重新设置属性数值
                    this.Set(attribPath, LPCValue.Create(attribValue - points[attrib].AsInt));

                    // 刷新Improvement下指定属性
                    PropMgr.RefreshAttrib(this, attrib);
                }
            } else
            {
                // 遍历各个属性
                foreach (string attrib in points.Keys)
                {
                    // 获取当前属性数值
                    attribValue = this.Query<int>(attrib);

                    // 重新设置属性数值
                    this.Set(attrib, LPCValue.Create(Math.Max(attribValue - points[attrib].AsInt, 0)));
                }
            }
        }
#else
        // 如果是最大属性伤害，需要单独处理
        if ((damageType & CombatConst.DAMAGE_TYPE_MAX_ATTRIB) == CombatConst.DAMAGE_TYPE_MAX_ATTRIB)
        {
            string attribPath = string.Empty;

            // 遍历最大属性伤害，这个地方只是记录的是扣除数值（纯粹是一个加值）
            foreach (string attrib in points.Keys)
            {
                // 获取当前属性数值
                attribPath = string.Format("attrib_addition/{0}", attrib);
                attribValue = this.Query<int>(attribPath);

                // 重新设置属性数值
                this.Set(attribPath, LPCValue.Create(attribValue - points[attrib].AsInt));

                // 刷新Improvement下指定属性
                PropMgr.RefreshAttrib(this, attrib);
            }
        } else
        {
            // 遍历各个属性
            foreach (string attrib in points.Keys)
            {
                // 获取当前属性数值
                attribValue = this.Query<int>(attrib);

                // 重新设置属性数值
                this.Set(attrib, LPCValue.Create(Math.Max(attribValue - points[attrib].AsInt, 0)));
            }
        }
#endif

        // 是否有不夺取击杀归属标识， 如果不夺取击杀归属标识则直接跳过
        if (! sourceProfile.ContainsKey("invalid_assailant"))
        {
            // 记录凶手信息
            LPCMapping assailantInfo = new LPCMapping();
            assailantInfo.Add("skill_id", sourceProfile.GetValue<int>("skill_id"));
            assailantInfo.Add("rid", sourceProfile["rid"]);

            // 添加original_cookie信息
            if (sourceProfile.ContainsKey("original_cookie"))
                assailantInfo.Add("original_cookie", sourceProfile["original_cookie"]);
            else if (args.ContainsKey("original_cookie"))
                assailantInfo.Add("original_cookie", args["original_cookie"]);

            // 添加发起协同攻击者
            if (sourceProfile.ContainsKey("joint_rid"))
                assailantInfo.Add("joint_rid", sourceProfile["joint_rid"]);

            // 记录凶手信息
            this.SetTemp("assailant_info", LPCValue.Create(assailantInfo));
        }

        // 构建事件参数
        LPCMapping eventPara = new LPCMapping();
        eventPara.Add("rid", this.GetRid());
        eventPara.Add("damage_type", damageType);
        eventPara.Add("source_profile", sourceProfile);
        eventPara.Add("damage_map", damageMap);
        eventPara.Add("original_damage_map", originalDamage);

        // 添加护盾吸收前的伤害
        if (shieldAbsorbDamage != null)
            eventPara.Add("shield_absorb_before_damage", shieldAbsorbDamage);

        // 抛出受创事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_RECEIVE_DAMAGE, MixedValue.NewMixedValue<LPCMapping>(eventPara), true, true);

        // 受伤以后附加处理
        RECEICE_DAMAGED.Call(this);
    }

    #endregion
}
