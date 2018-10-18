/// <summary>
/// RoundCombatMgr.cs
/// create by zhaozy 2016-05-05
/// 回合制管理模块
/// </summary>

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using LPC;

/// <summary>
/// 回合制管理模块 
/// </summary>
public class RoundCombatMgr
{
    #region 属性接口

    /// <summary>
    /// 运行标识
    /// </summary>
    public static RoundCombat CombatOb { get; private set; }

    // 上一回合是否是自动战斗
    public static bool LastRoundIsAutoCombat {get; set;}

    #endregion

    #region 公共接口

    /// <summary>
    /// 开始回合制战斗
    /// </summary>
    public static bool StartRoundCombat(List<Property> propertyList, float delayTime, Property ownerOb, int roundType)
    {
        // 还有回合战斗对象, 且ownerRid不一致
        if (CombatOb != null && ! CombatOb.OwnerRid.Equals(ownerOb.GetRid()))
        {
            // 结束当前正在进行中战斗
            EndRoundCombat(true);
        }

        // new一个回合制战斗对象
        if (CombatOb == null)
            CombatOb = new RoundCombat(ownerOb.GetRid(), roundType);

        // 缓存boss对象
        List<Property> bossList = new List<Property>();

        // 战斗验证客户端不需要表现相关流程
        if (AuthClientMgr.IsAuthClient)
        {
            // 回合制战斗开始
            CombatOb.Start(propertyList, delayTime);

            // 刷新一下玩家属性
            foreach (Property combatOb in propertyList)
            {
                // 统一刷新一下玩家属性
                PropMgr.RefreshAffect(combatOb);

                // 刷新玩家光环
                HaloMgr.RefreshHaloAffect(combatOb);

                // 抛出公式
                LPCValue isFrist = combatOb.QueryTemp<LPCValue>("is_frist_enter_combat");
                if (isFrist == null)
                {
                    // 标识已经进入过战场
                    combatOb.SetTemp("is_frist_enter_combat", LPCValue.Create(1));
                    REFRESH_PET_HP_MP.Call(combatOb);
                }
            }

            // 刷新一下玩家技能属性
            foreach (Property combatOb in propertyList)
            {
                // 统一刷新一下玩家skill属性, 玩家有技能需要复制对方属性
                PropMgr.RefreshAffect(combatOb, "skill");
            }
        }
        else
        {
            // 获取当前副本id
            string instanceId = ME.user.Query<string>("instance/id");

            LPCMapping selectMap = AutoCombatSelectTypeMgr.GetSelectMap(ME.user, instanceId);

            int isManualOpre = 0;
            if (selectMap != null && selectMap.ContainsKey(InstanceConst.MANUAL_OPREATION))
                isManualOpre = selectMap.GetValue<int>(InstanceConst.MANUAL_OPREATION);

            // 还原自动战斗
            if (isManualOpre == 1 && LastRoundIsAutoCombat)
                AutoCombatMgr.SetAutoCombat(LastRoundIsAutoCombat, InstanceMgr.GetLoopFightByInstanceId(instanceId));

            // 遍历显示所有战斗对象的血条
            foreach (Property ob in propertyList)
            {
                // 取消所有角色当前的选择状态
                ob.Actor.CancelActionSetByName("select");

                LPCValue selectType = ob.Query<LPCValue>("auto_combat_select_type");

                // 手动操作
                if (ob.CampId.Equals(CampConst.CAMP_TYPE_DEFENCE)
                    && isManualOpre == 1
                    && selectType != null
                    && selectType.AsString.Equals(InstanceConst.BOSS_TYPE))
                {
                    LastRoundIsAutoCombat = AutoCombatMgr.IsAutoCombat();

                    // 取消自动战斗
                    if (LastRoundIsAutoCombat)
                        AutoCombatMgr.SetAutoCombat(false, InstanceMgr.GetLoopFightByInstanceId(instanceId));
                }

#if UNITY_EDITOR

                // 该功能在编辑下有效
                if (ME.user.QueryTemp<int>("show_debug_info") != 0)
                    ob.Actor.ShowValueWnd(!ob.CheckStatus("DIED"));

#endif

                // 判断该怪物是否是boss
                if (ob.Query<int>("is_boss") == 1)
                {
                    // 收集boss对象
                    bossList.Add(ob);
                    continue;
                }

                // 显示血条
                ob.Actor.ShowHp(!ob.CheckStatus("DIED"));
            }

            // 按照站位排序列表
            bossList.Sort((Comparison<Property>)delegate(Property pos1, Property pos2){
                return pos2.FormationPos.CompareTo(pos1.FormationPos);
            });

            // 显示boss血条
            for (int i = 0; i < bossList.Count; i++)
            {
                LPCMapping bossData = new LPCMapping();
                bossData.Add("index", i);
                bossData.Add("boss_amount", bossList.Count);
                bossList[i].SetTemp("boss_data", LPCValue.Create(bossData));

                // 显示boss血条
                bossList[i].Actor.ShowHp(! bossList[i].CheckStatus("DIED"));

#if UNITY_EDITOR

                // 该功能在编辑下有效
                if (ME.user.QueryTemp<int>("show_debug_info") != 0)
                    bossList[i].Actor.ShowValueWnd(!bossList[i].CheckStatus("DIED"));

#endif

            }

            // 回合制战斗开始
            CombatOb.Start(propertyList, delayTime);

            // 刷新一下玩家属性
            foreach (Property combatOb in propertyList)
            {
                // 统一刷新一下玩家属性
                PropMgr.RefreshAffect(combatOb);

                // 刷新玩家光环
                HaloMgr.RefreshHaloAffect(combatOb);

                // 抛出公式
                LPCValue isFrist = combatOb.QueryTemp<LPCValue>("is_frist_enter_combat");
                if (isFrist == null)
                {
                    // 标识已经进入过战场
                    combatOb.SetTemp("is_frist_enter_combat", LPCValue.Create(1));
                    REFRESH_PET_HP_MP.Call(combatOb);
                }
            }

            // 刷新一下玩家技能属性
            foreach (Property combatOb in propertyList)
            {
                // 统一刷新一下玩家skill属性, 玩家有技能需要复制对方属性
                PropMgr.RefreshAffect(combatOb, "skill");
            }

            // 刷新血条order
            RoundCombatMgr.RedrawHpSortOrder();
        }

        // 返回成功
        return true;
    }

    /// <summary>
    /// 设置血条的sortOrder
    /// </summary>
    /// <param name="order">Order.</param>
    public static void RedrawHpSortOrder()
    {
        List<Property> combatList = RoundCombatMgr.GetPropertyList();

        List<Property> fixedList = new List<Property>();
        foreach (Property ob in combatList)
        {
            // boss血条单独处理
            if (ob.Query<int>("is_boss") == 1)
                continue;

            fixedList.Add(ob);
        }

        // 根据z轴的位置对fixedList排序
        fixedList.Sort(delegate(Property ob1, Property ob2)
            {
                return ob1.Actor.GetPositionZ().CompareTo(ob2.Actor.GetPositionZ());
            });

        // 设置角色血条的sortOrder
        for (int i = 0; i < fixedList.Count; i++)
            fixedList[i].Actor.SetHpSortOrder(fixedList.Count - i);
    }

    /// <summary>
    /// 继续回合制战斗
    /// </summary>
    public static bool ContinueRoundCombat()
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return false;

        // 结束本回合战斗
        CombatOb.Continue();

        // 返回结束成功
        return true;
    }

    /// <summary>
    /// 获取最近正常行动回合角色对象（ROUND_TYPE_NORMAL和ROUND_TYPE_ADDITIONAL）
    /// </summary>
    public static Property GetCurRoundOb()
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return null;

        // 返回最近正常行动回合角色对象
        return CombatOb.CurRoundOb;
    }

    /// <summary>
    /// 暂停回合制战斗
    /// </summary>
    public static bool PauseRoundCombat()
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return false;

        // 结束本回合战斗
        CombatOb.Pause();

        // 返回结束成功
        return true;
    }

    /// <summary>
    /// 回合制战斗运行标识
    /// </summary>
    public static bool IsRoundCombatRunning()
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return false;

        // 回合制战斗运行标识
        return CombatOb.IsRunningFlag;
    }

    /// <summary>
    /// 结束回合制战斗
    /// </summary>
    public static bool EndRoundCombat(bool isForce)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return false;

        // 结束本回合战斗
        CombatOb.End(isForce);

        // 战斗结束
        CombatOb = null;

        // 重置lock目标
        AutoCombatMgr.UnlockCombatTarget(AutoCombatMgr.LockTargetOb);

        // 返回结束成功
        return true;
    }

    /// <summary>
    /// 驱动回合制战斗
    /// </summary>
    public static void Update()
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return;

        // 延迟开始时间还没有到
        CombatOb.Update();
    }

    /// <summary>
    /// 角色结束回合
    /// </summary>
    public static void DoActionRoundEnd(string cookie, LPCMapping para)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return;

        // 延迟开始时间还没有到
        CombatOb.DoRoundActionEnd(cookie, para);
    }

    /// <summary>
    /// 获取回合次数
    /// </summary>
    /// <returns>The round times.</returns>
    public static int GetRounds()
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return 0;

        // 获取回合次数
        return CombatOb.GetRounds();
    }

    /// <summary>
    /// 放弃战斗
    /// </summary>
    public static void EndCombat(int type)
    {
        // 如果是正在战斗不允许

        // 没有回合制战斗对象
        if (CombatOb == null)
            return;

        // 添加反击行动列表
        CombatOb.EndCombat(type);
    }

    /// <summary>
    /// 获取战场上的战斗对象列表
    /// </summary>
    /// <returns>The property list.</returns>
    public static List<Property> GetPropertyList()
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return new List<Property>();

        // 获取战场上的战斗对象列表
        return CombatOb.GetPropertyList();
    }

    /// <summary>
    /// 获取战场上的战斗对象列表
    /// </summary>
    public static List<Property> GetPropertyList(int campId)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return new List<Property>();

        // 获取战场上的战斗对象列表
        return CombatOb.GetPropertyList(campId);
    }

    /// <summary>
    /// 判断角色是否在当前回合中已经已经行动
    /// </summary>
    public static bool IsRoundDone(Property ob)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return false;

        // 获取战场上的战斗对象列表
        return CombatOb.IsRoundDone(ob);
    }

    /// <summary>
    /// 获取战场上的战斗对象列表
    /// </summary>
    public static List<Property> GetPropertyList(int campId, List<string> excludeStatus = null, List<string> naturalList = null, List<string> includeStatus = null)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return new List<Property>();

        // 获取战场上的战斗对象列表
        List<Property> propertyList = CombatOb.GetPropertyList(campId);

        // 列表为空
        if (propertyList.Count == 0)
            return new List<Property>();

        // 选择列表
        List<Property> selectList = new List<Property>();
        Property ob;

        // 尝试收集指定玩家
        if (naturalList != null)
        {
            for (int i = 0; i < naturalList.Count; i++)
            {
                // 查找对象
                ob = Rid.FindObjectByRid(naturalList[i]);

                // 对象不存在
                if (ob == null)
                    continue;

                // 阵营不对
                if (ob.CampId != campId)
                    continue;

                // 已经收集过了该角色
                if (selectList.Contains(ob))
                    continue;

                // 有指定的排斥状态，不能收集
                if (excludeStatus != null && ob.CheckStatus(excludeStatus))
                    continue;

                // 不包含指定状态， 不能收集
                if (includeStatus != null)
                {
                    for (int index = 0; index < includeStatus.Count; index++)
                    {
                        if (!ob.CheckStatus(includeStatus[index]))
                            continue;
                    }
                }

                // 添加到列表中
                selectList.Add(ob);
            }
        }

        // 遍历各个角色对象
        for (int i = 0; i < propertyList.Count; i++)
        {
            // 对象不存在
            if (propertyList[i] == null)
                continue;

            // 已经收集过了该角色
            if (selectList.Contains(propertyList[i]))
                continue;

            // 有指定的排斥状态，不能收集
            if (excludeStatus != null && propertyList[i].CheckStatus(excludeStatus))
                continue;

            // 不包含指定状态, 不能收集
            if (includeStatus != null)
            {
                for (int index = 0; index < includeStatus.Count; index++)
                {
                    if (!propertyList[i].CheckStatus(includeStatus[index]))
                        continue;
                }
            }

            // 添加到列表中
            selectList.Add(propertyList[i]);
        }

        // 返回选择列表
        return selectList;
    }

    /// <summary>
    /// 角色速度发生变化
    /// </summary>
    public static void OnAttackOrderUpdate(Property ob)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return;

        // 角色速度发生变化
        CombatOb.OnAttackOrderUpdate(ob);
    }

    /// <summary>
    /// 角色当前回合是否还有效
    /// </summary>
    public static bool CurRoundIsValid(Property ob)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return false;

        // 角色当前回合是否还有效
        return CombatOb.CurRoundIsValid(ob);
    }

    /// <summary>
    /// 添加行动列表
    /// </summary>
    public static bool AddRoundAction(string cookie, Property ob)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return false;

        // 添加行动列表
        return CombatOb.AddRoundAction(cookie, ob, LPCMapping.Empty);
    }

    /// <summary>
    /// 添加行动列表
    /// </summary>
    public static bool AddRoundAction(string cookie, Property ob, LPCMapping para)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return false;

        // 添加行动列表
        return CombatOb.AddRoundAction(cookie, ob, para);
    }

    /// <summary>
    /// 追加回合
    /// </summary>
    public static bool DoAdditionalRound(Property ob, LPCMapping para)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return false;

        // 添加追加回合
        return CombatOb.AddAdditionalRound(ob, para);
    }

    /// <summary>
    /// 插队回合
    /// </summary>
    public static bool DoGasserRound(Property ob, LPCMapping para)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return false;

        // 添加插队回合
        return CombatOb.AddGasserRound(ob, para);
    }

    /// <summary>
    /// 执行反击回合
    /// </summary>
    public static bool DoCounterRound(Property ob, LPCMapping para)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return false;

        // 添加反击行动列表
        return CombatOb.AddCounterRound(ob, para);
    }

    /// <summary>
    /// 执行协同回合
    /// </summary>
    public static bool DoJointRound(Property ob, LPCMapping para)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return false;

        // 添加协同行动列表
        return CombatOb.AddJointRound(ob, para);
    }

    /// <summary>
    /// 添加暴走回合
    /// </summary>
    public static void DoRampageRound(Property ob, LPCMapping para)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return;

        // 添加暴走行动列表
        CombatOb.AddRampageRound(ob, para);
    }

    /// <summary>
    /// 添加连击回合
    /// </summary>
    public static bool DoComboRound(Property ob, LPCMapping para)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return false;

        // 添加追加回合
        return CombatOb.AddComboRound(ob, para);
    }

    /// <summary>
    /// 添加回合找都对象
    /// </summary>
    /// <param name="ob">Ob.</param>
    public static void AddCombatEntity(Property ob)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return;

        // 标识已经进入过战场
        ob.SetTemp("is_frist_enter_combat", LPCValue.Create(1));

        // 添加暴走行动列表
        CombatOb.AddCombatEntity(ob);

        // 刷新玩家光环
        HaloMgr.RefreshHaloAffect(ob);

        // 刷新其他角色带给新进入战场角色的光环效果
        foreach (Property combatOb in GetPropertyList())
        {
            // 如果是ob自己不处理，已经在上面处理过了
            if (ob == combatOb)
                continue;

            // 刷新combatOb对ob的光环效果
            HaloMgr.RefreshHaloAffect(combatOb, ob);
        }

        // 统一刷新一下玩家skill属性, 玩家有技能需要复制对方属性
        foreach (Property combatOb in GetPropertyList())
            PropMgr.RefreshAffect(combatOb, "skill");

        // 刷新血条order
        RoundCombatMgr.RedrawHpSortOrder();
    }

    /// <summary>
    /// 移除回合找都对象
    /// </summary>
    /// <param name="ob">Ob.</param>
    public static void RemoveCombatEntity(Property ob)
    {
        // 没有回合制战斗对象
        if (CombatOb == null)
            return;

        // 添加暴走行动列表
        CombatOb.RemoveCombatEntity(ob);
    }

    /// <summary>
    /// 战斗对象排序
    /// </summary>
    public static List<Property> GetSortActionOrder(List<Property> propertyList, bool force = false)
    {
        // 根据道具权重排序
        IEnumerable<Property> propertyQuery = from ob in propertyList orderby RoundCombatMgr.GetOrderWeight(ob, force) descending
                                                    select ob;

        // 返回排序后的列表
        return propertyQuery.ToList();
    }

    /// <summary>
    /// 获取战斗对象的攻击顺序权重
    /// </summary>
    public static string GetOrderWeight(Property combatOb, bool force)
    {
        // 获取角色
        string attackOrder = combatOb.Query<string>("attack_order");
        LPCValue isDirty = combatOb.Query<LPCValue>("is_dirty");

        // attackOrder还没有初始化
        if (! string.IsNullOrEmpty(attackOrder) && isDirty == null && !force)
            return attackOrder;

        // 计算攻击顺序
        attackOrder = string.Format("@{0:D1}{1:D3}{2:D1}{3:D1}{4:D1}",
            combatOb.QueryAttrib("first_hand"),
            CALC_BATTLE_SPEED.Call(combatOb),
            combatOb.CampId,
            combatOb.FormationPos,
            (string.IsNullOrEmpty(combatOb.Query<string>("summoner_rid")) ? 0 : 1));

        // 删除isDirty标识
        combatOb.Delete("is_dirty");

        // 记录攻击顺序
        combatOb.Set("attack_order", LPCValue.Create(attackOrder));

        // 排序规则是优先攻击权重, 攻击速度, 阵营, 站位
        return attackOrder;
    }

    /// <summary>
    /// 判断对象是否可以执行回合
    /// </summary>
    public static bool CheckCanDoRound(Property ob, int type)
    {
        // 角色对象不存在或者正在析构过程中
        if (ob == null || ob.IsDestroyed)
            return false;

        // 1. 检测玩家状态是否选择玩家行动
        CsvRow info = null;
        int limitRoundScript;
        List<LPCMapping> allStatus = ob.GetAllStatus();

        // 遍历各个状态
        foreach (LPCMapping data in allStatus)
        {
            // 获取配置信息
            info = StatusMgr.GetStatusInfo(data.GetValue<int>("status_id"));

            // 没有配置的状态
            if (info == null)
                continue;

            // 判断状态是否限制行动
            if (info.Query<int>("affect") == StatusConst.STOP_ACTION)
                return false;

            // 获取限制行动脚本
            limitRoundScript = info.Query<int>("limit_round_script");
            if (limitRoundScript == 0)
                continue;

            // 如果限制回合行动
            if ((bool)ScriptMgr.Call(limitRoundScript, ob, type, info.Query<LPCValue>("limit_round_args")))
                return false;
        }

        LPCMapping attakMap;
        LPCMapping extraPara = LPCMapping.Empty;
        extraPara.Add("ignore_target_lock", 1);

        // 抽取玩家是否有技能可以释放，如果玩家没有技能可以释放，不处理
        if (! SkillMgr.FetchSkill(ob, SkillType.INCLINATION_RANDOM, extraPara, out attakMap))
            return false;

        // 3. 其他的情况

        // 返回可以执行回合
        return true;
    }

    /// <summary>
    /// 副本通关显示玩家阵容
    /// </summary>
    public static void ClearanceInstanceShowPet(List<Property> fightList, LPCMapping data)
    {
        if (fightList == null || data == null)
            return;

        // 显示各个战斗角色
        for (int i = 0; i < fightList.Count; i++)
        {
            // 清除召唤实体
            CombatSummonMgr.RemoveEmployeeList(fightList[i]);

            // 好友共享宠物不显示经验条,经验不共享
            if (!FriendMgr.IsFriendSharePet(fightList[i].Query<string>("original_rid")))
            {
                //显示玩家经验条;
                fightList[i].Actor.ShowExp(true, data);
            }

            //玩家通关
            if (fightList[i].CheckStatus("DIED"))
                fightList[i].Actor.SetTweenAlpha(0.5f);
            else
                fightList[i].Actor.SetTweenAlpha(1f);
        }
    }

    #endregion
}

