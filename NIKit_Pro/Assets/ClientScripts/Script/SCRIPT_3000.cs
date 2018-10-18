/// <summary>
/// SCRIPT_3000.cs
/// Create by zhaozy 2016-05-27
/// AI脚本（3000 --> 3999）
/// </summary>

using System;
using System.Collections.Generic;
using LPC;

// 通用AI脚本
public class SCRIPT_3001 : Script
{
    public override object Call(params object[] _params)
    {
        Property ob = _params[0] as Property;
        //LPCMapping args = _params[1] as LPCMapping;
        LPCMapping extraArgs = _params[2] as LPCMapping;

        // 获取需要收集阵营
        int skillId = 0;
        string targetRid = string.Empty;
        LPCMapping attakMap = LPCMapping.Empty;

        // 1、检查嘲讽
        if (ob.CheckStatus("D_PROVOKE"))
        {
            List<LPCMapping> allStatus = ob.GetStatusCondition("D_PROVOKE");

            // 只可能同时存在一种陷阱
            LPCMapping data = allStatus[0]; 
            LPCMapping dataSourceProfile = data.GetValue<LPCMapping>("source_profile");
            targetRid = dataSourceProfile.GetValue<string>("rid");

            LPCArray skills = ob.GetAllSkills();
            foreach (LPCValue mks in skills.Values)
            {
                int id = mks.AsArray[0].AsInt;
                if (SkillMgr.GetFamily(id) != SkillType.SKILL_NORMAL)
                    continue;

                // 获取技能id
                skillId = id;
                break;
            }

            // 添加战斗指令
            attakMap.Add("skill_id", skillId);
            attakMap.Add("pick_rid", targetRid);
            attakMap.Add("imput_type", AttackImputType.AIT_RANDOM);

            // 返回数据
            return attakMap;
        }

        // 2、检查混乱
        if (ob.CheckStatus("D_CHAOS"))
        {
            // 收集己方目标
            List<Property> ownTargetList = RoundCombatMgr.GetPropertyList(ob.CampId,
                new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" });

            // 收集敌方目标
            int opCampId = (ob.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
            List<Property> opTargetList = RoundCombatMgr.GetPropertyList(opCampId,
                new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" });

            List<Property> finalList = new List<Property>();
            List<Property> checkList = new List<Property>();

            if (RandomMgr.GetRandom() < 500)
            {
                // 本方只有 1 人，此处情况下，只可能是自身，不做额外判断
                if (ownTargetList.Count == 1)
                    checkList = opTargetList;
                else
                    checkList = ownTargetList;
            }
            else
                checkList = opTargetList;

            // 筛选非死亡单位
            foreach (Property targetOb in checkList)
            {
                if (!ob.GetRid().Equals(targetOb.GetRid()))
                    finalList.Add(targetOb);
            }

            // 如果列表为空，则打自己
            if (finalList.Count == 0)
                targetRid = ob.GetRid();
            // 不为空则确定随机抽取的目标
            else
                targetRid = finalList[RandomMgr.GetRandom(finalList.Count)].GetRid();

            LPCArray skills = ob.GetAllSkills();
            foreach (LPCValue mks in skills.Values)
            {
                int id = mks.AsArray[0].AsInt;
                if (SkillMgr.GetFamily(id) != SkillType.SKILL_NORMAL)
                    continue;

                // 获取技能id
                skillId = id;
                break;
            }

            // 添加战斗指令
            attakMap.Add("skill_id", skillId);
            attakMap.Add("pick_rid", targetRid);
            attakMap.Add("imput_type", AttackImputType.AIT_RANDOM);

            // 返回数据
            return attakMap;
        }

        // 计算倾向抽取技能和目标
        // 获取需要收集阵营
        int campId = (ob.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
        List<Property> entityList = RoundCombatMgr.GetPropertyList(campId);
        int restrainNum = 0;
        int liveNum = 0;
        for (int i = 0; i < entityList.Count; i++)
        {
            if (ElementMgr.GetMonsterCounter(ob, entityList[i]) == ElementConst.ELEMENT_ADVANTAGE)
                restrainNum += 1;
            if (!entityList[i].CheckStatus("DIED"))
                liveNum += 1;
        }

        if (liveNum == 0)
            liveNum = 1;

        // 辅助倾向概率
        int weightAss = 850 * restrainNum / liveNum;

        // 攻击倾向概率
        int weightAtk = 850 - weightAss;

        // 乱序倾向概率，副本竞技场防守方不走乱序
        int weightRan = 150;
        if (ob.CampId == CampConst.CAMP_TYPE_DEFENCE)
        {
            int type = InstanceMgr.GetMapTypeByInstanceId(ob.Query<string>("instance/id"));
            if (type == MapConst.ARENA_MAP || type == MapConst.ARENA_REVENGE_MAP || type == MapConst.ARENA_NPC_MAP)
            {
                weightRan = 0;
            }
        }

        // 选择一种攻击倾向
        List<int> weightList = new List<int>(){ weightAtk, weightAss, weightRan };
        List<int> typeList = new List<int>()
        {
            SkillType.INCLINATION_ATTACK,
            SkillType.INCLINATION_ASSISTANT,
            SkillType.INCLINATION_RANDOM
        };
        int index = RandomMgr.RandomSelect(weightList);

        // 根据攻击倾向收取技能和技能作用目标
        bool fetchRet = SkillMgr.FetchSkill(ob, typeList[index], extraArgs, out attakMap);
        if (! fetchRet)
            return null;

        // 如果是连击回合，执行连击技能
        if (extraArgs.GetValue<int>("type") == RoundCombatConst.ROUND_TYPE_RAMPAGE)
            attakMap.Add("skill_id", extraArgs.GetValue<int>("skill_id"));

        // 返回数据
        return attakMap;
    }
}

// 自动战斗选择目标脚本
public class SCRIPT_3002 : Script
{
    public override object Call(params object[] _params)
    {
        // 抽取列表
        List<Property> fetchList = (List<Property>) _params[0];

        if (fetchList == null)
            return null;

        // 自动战斗攻击类型
        string type = _params[1] as string;

        // 抽取参数
//        LPCMapping fetchArgs = _params[2] as LPCMapping;

        for (int i = 0; i < fetchList.Count; i++)
        {
            Property ob = fetchList[i];
            if (ob == null)
                continue;

            if (type.Equals(ob.Query<string>("auto_combat_select_type")))
                return ob;
        }

        return null;
    }
}

// 死神使者·火 专用用AI脚本
public class SCRIPT_3003 : Script
{
    public override object Call(params object[] _params)
    {
        Property ob = _params[0] as Property;
        //LPCMapping args = _params[1] as LPCMapping;
        LPCMapping extraArgs = _params[2] as LPCMapping;

        // 获取需要收集阵营
        int campId = (ob.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
        int skillId = 0;
        string targetRid = string.Empty;
        LPCMapping attakMap = LPCMapping.Empty;

        // 优先检查安魂弥撒状态，与嘲讽不冲突，因为嘲讽是控制技能，如果被嘲讽，安魂弥撒状态会被排斥掉
        if (ob.CheckStatus("B_NEXT_ROUND_ATK"))
        {
            // 获取状态信息
            List<LPCMapping> statusData = ob.GetStatusCondition("B_NEXT_ROUND_ATK");

            // 选取pick_rid
            List<Property> finalList = RoundCombatMgr.GetPropertyList(campId,
                new List<string>(){ "DIED" });

            attakMap.Add("pick_rid", finalList[0].GetRid());
            attakMap.Add("skill_id", statusData[0].GetValue<int>("skill_id"));
            attakMap.Add("imput_type", AttackImputType.AIT_RANDOM);
            ob.ClearStatus("B_NEXT_ROUND_ATK");
            return attakMap;
        }

        // 1、检查嘲讽
        if (ob.CheckStatus("D_PROVOKE"))
        {
            List<LPCMapping> allStatus = ob.GetStatusCondition("D_PROVOKE");

            // 只可能同时存在一种陷阱
            LPCMapping data = allStatus[0]; 
            LPCMapping dataSourceProfile = data.GetValue<LPCMapping>("source_profile");
            targetRid = dataSourceProfile.GetValue<string>("rid");

            LPCArray skills = ob.GetAllSkills();
            foreach (LPCValue mks in skills.Values)
            {
                int id = mks.AsArray[0].AsInt;
                if (SkillMgr.GetFamily(id) != SkillType.SKILL_NORMAL)
                    continue;

                // 获取技能id
                skillId = id;
                break;
            }

            // 添加战斗指令
            attakMap.Add("skill_id", skillId);
            attakMap.Add("pick_rid", targetRid);
            attakMap.Add("imput_type", AttackImputType.AIT_RANDOM);

            // 返回数据
            return attakMap;
        }

        // 2、检查混乱
        if (ob.CheckStatus("D_CHAOS"))
        {
            // 收集己方目标
            List<Property> ownTargetList = RoundCombatMgr.GetPropertyList(ob.CampId,
                new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" });

            // 收集敌方目标
            int opCampId = (ob.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
            List<Property> opTargetList = RoundCombatMgr.GetPropertyList(opCampId,
                new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" });

            List<Property> finalList = new List<Property>();
            List<Property> checkList = new List<Property>();

            if (RandomMgr.GetRandom() < 500)
            {
                // 本方只有 1 人，此处情况下，只可能是自身，不做额外判断
                if (ownTargetList.Count == 1)
                    checkList = opTargetList;
                else
                    checkList = ownTargetList;
            }
            else
                checkList = opTargetList;

            // 筛选非死亡单位
            foreach (Property targetOb in checkList)
            {
                if (!ob.GetRid().Equals(targetOb.GetRid()))
                    finalList.Add(targetOb);
            }

            // 确定随机抽取的目标
            targetRid = finalList[RandomMgr.GetRandom(finalList.Count)].GetRid();

            LPCArray skills = ob.GetAllSkills();
            foreach (LPCValue mks in skills.Values)
            {
                int id = mks.AsArray[0].AsInt;
                if (SkillMgr.GetFamily(id) != SkillType.SKILL_NORMAL)
                    continue;

                // 获取技能id
                skillId = id;
                break;
            }

            // 添加战斗指令
            attakMap.Add("skill_id", skillId);
            attakMap.Add("pick_rid", targetRid);
            attakMap.Add("imput_type", AttackImputType.AIT_RANDOM);

            // 返回数据
            return attakMap;
        }

        // 计算倾向抽取技能和目标
        // 获取需要收集阵营
        List<Property> entityList = RoundCombatMgr.GetPropertyList(campId);
        int restrainNum = 0;
        int liveNum = 0;
        for (int i = 0; i < entityList.Count; i++)
        {
            if (ElementMgr.GetMonsterCounter(ob, entityList[i]) == ElementConst.ELEMENT_ADVANTAGE)
                restrainNum += 1;
            if (!entityList[i].CheckStatus("DIED"))
                liveNum += 1;
        }

        if (liveNum == 0)
            liveNum = 1;

        // 辅助倾向概率
        int weightAss = 850 * restrainNum / liveNum;

        // 攻击倾向概率
        int weightAtk = 850 - weightAss;

        // 乱序倾向概率
        int weightRan = 150;

        // 选择一种攻击倾向
        List<int> weightList = new List<int>(){ weightAtk, weightAss, weightRan };
        List<int> typeList = new List<int>()
            {
                SkillType.INCLINATION_ATTACK,
                SkillType.INCLINATION_ASSISTANT,
                SkillType.INCLINATION_RANDOM
            };
        int index = RandomMgr.RandomSelect(weightList);

        // 根据攻击倾向收取技能和技能作用目标
        bool fetchRet = SkillMgr.FetchSkill(ob, typeList[index], extraArgs, out attakMap);
        if (! fetchRet)
            return null;

        // 如果是连击回合，执行连击技能
        if (extraArgs.GetValue<int>("type") == RoundCombatConst.ROUND_TYPE_RAMPAGE)
            attakMap.Add("skill_id", extraArgs.GetValue<int>("skill_id"));

        // 返回数据
        return attakMap;
    }
}

// 流浪南瓜·风 专用用AI脚本
public class SCRIPT_3004 : Script
{
    public override object Call(params object[] _params)
    {
        Property ob = _params[0] as Property;
        LPCMapping args = _params[1] as LPCMapping;
        LPCMapping extraArgs = _params[2] as LPCMapping;

        // 获取需要收集阵营
        int campId = (ob.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
        int skillId = 0;
        string targetRid = string.Empty;
        LPCMapping attakMap = LPCMapping.Empty;

        // 优先检查爆炸南瓜头技能
        // 检查血量低于设定百分比起效类
        List<LPCMapping> allStatus = ob.GetAllStatus();
        List<LPCMapping> ctrlList = new List<LPCMapping>();
        foreach (LPCMapping statusData in allStatus)
        {
            CsvRow statusInfo;
            statusInfo = StatusMgr.GetStatusInfo(statusData.GetValue<int>("status_id"));
            LPCMapping statusMap = statusInfo.Query<LPCMapping>("limit_round_args");
            if (statusMap.GetValue<int>("ctrl_id") > 0)
                ctrlList.Add(statusData);
        }

        if (ob.QueryAttrib("low_hp_cast_skill") > 0 && ctrlList.Count  == 0 && !ob.CheckStatus("D_SILENCE"))
        {
            int originSkillId = SkillMgr.GetOriginalSkillId(args.GetValue<int>("skill_id"));
            if (! CdMgr.SkillIsCooldown(ob, originSkillId))
            {
                // 判断血量条件是否满足，不满足则直接跳出判断
                int hpRate = Game.Divided(ob.Query<int>("hp"), ob.QueryAttrib("max_hp"));
                if (hpRate < ob.QueryAttrib("low_hp_cast_skill"))
                {
                    // 选取pick_rid
                    List<Property> finalList = RoundCombatMgr.GetPropertyList(campId,
                        new List<string>(){ "DIED" });

                    attakMap.Add("pick_rid", finalList[0].GetRid());
                    attakMap.Add("skill_id", args.GetValue<int>("skill_id"));
                    attakMap.Add("imput_type", AttackImputType.AIT_RANDOM);
                    return attakMap;
                }
            }
        }

        // 1、检查嘲讽
        if (ob.CheckStatus("D_PROVOKE"))
        {
            List<LPCMapping> allProvokeStatus = ob.GetStatusCondition("D_PROVOKE");

            // 只可能同时存在一种陷阱
            LPCMapping data = allProvokeStatus[0]; 
            LPCMapping dataSourceProfile = data.GetValue<LPCMapping>("source_profile");
            targetRid = dataSourceProfile.GetValue<string>("rid");

            LPCArray skills = ob.GetAllSkills();
            foreach (LPCValue mks in skills.Values)
            {
                int id = mks.AsArray[0].AsInt;
                if (SkillMgr.GetFamily(id) != SkillType.SKILL_NORMAL)
                    continue;

                // 获取技能id
                skillId = id;
                break;
            }

            // 添加战斗指令
            attakMap.Add("skill_id", skillId);
            attakMap.Add("pick_rid", targetRid);
            attakMap.Add("imput_type", AttackImputType.AIT_RANDOM);

            // 返回数据
            return attakMap;
        }

        // 2、检查混乱
        if (ob.CheckStatus("D_CHAOS"))
        {
            // 收集己方目标
            List<Property> ownTargetList = RoundCombatMgr.GetPropertyList(ob.CampId,
                new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" });

            // 收集敌方目标
            int opCampId = (ob.CampId == CampConst.CAMP_TYPE_ATTACK) ? CampConst.CAMP_TYPE_DEFENCE : CampConst.CAMP_TYPE_ATTACK;
            List<Property> opTargetList = RoundCombatMgr.GetPropertyList(opCampId,
                new List<string>(){ "DIED", "B_CAN_NOT_CHOOSE" });

            List<Property> finalList = new List<Property>();
            List<Property> checkList = new List<Property>();

            if (RandomMgr.GetRandom() < 500)
            {
                // 本方只有 1 人，此处情况下，只可能是自身，不做额外判断
                if (ownTargetList.Count == 1)
                    checkList = opTargetList;
                else
                    checkList = ownTargetList;
            }
            else
                checkList = opTargetList;

            // 筛选非死亡单位
            foreach (Property targetOb in checkList)
            {
                if (!ob.GetRid().Equals(targetOb.GetRid()))
                    finalList.Add(targetOb);
            }

            // 确定随机抽取的目标
            targetRid = finalList[RandomMgr.GetRandom(finalList.Count)].GetRid();

            LPCArray skills = ob.GetAllSkills();
            foreach (LPCValue mks in skills.Values)
            {
                int id = mks.AsArray[0].AsInt;
                if (SkillMgr.GetFamily(id) != SkillType.SKILL_NORMAL)
                    continue;

                // 获取技能id
                skillId = id;
                break;
            }

            // 添加战斗指令
            attakMap.Add("skill_id", skillId);
            attakMap.Add("pick_rid", targetRid);
            attakMap.Add("imput_type", AttackImputType.AIT_RANDOM);

            // 返回数据
            return attakMap;
        }

        // 计算倾向抽取技能和目标
        // 获取需要收集阵营
        List<Property> entityList = RoundCombatMgr.GetPropertyList(campId);
        int restrainNum = 0;
        int liveNum = 0;
        for (int i = 0; i < entityList.Count; i++)
        {
            if (ElementMgr.GetMonsterCounter(ob, entityList[i]) == ElementConst.ELEMENT_ADVANTAGE)
                restrainNum += 1;
            if (!entityList[i].CheckStatus("DIED"))
                liveNum += 1;
        }

        if (liveNum == 0)
            liveNum = 1;

        // 辅助倾向概率
        int weightAss = 850 * restrainNum / liveNum;

        // 攻击倾向概率
        int weightAtk = 850 - weightAss;

        // 乱序倾向概率
        int weightRan = 150;

        // 选择一种攻击倾向
        List<int> weightList = new List<int>(){ weightAtk, weightAss, weightRan };
        List<int> typeList = new List<int>()
            {
                SkillType.INCLINATION_ATTACK,
                SkillType.INCLINATION_ASSISTANT,
                SkillType.INCLINATION_RANDOM
            };
        int index = RandomMgr.RandomSelect(weightList);

        // 根据攻击倾向收取技能和技能作用目标
        bool fetchRet = SkillMgr.FetchSkill(ob, typeList[index], extraArgs, out attakMap);
        if (! fetchRet)
            return null;

        // 如果是连击回合，执行连击技能
        if (extraArgs.GetValue<int>("type") == RoundCombatConst.ROUND_TYPE_RAMPAGE)
            attakMap.Add("skill_id", extraArgs.GetValue<int>("skill_id"));

        // 返回数据
        return attakMap;
    }
}