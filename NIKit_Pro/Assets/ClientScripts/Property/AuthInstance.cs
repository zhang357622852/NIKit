/// <summary>
/// AuthInstance.cs
/// Create by zhaozy 2017/05/08
/// 验证战斗副本对象(该对象实际上就是正常副本对象的copy版本去除掉了界面表现相关的逻辑)
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using LPC;

/// <summary>
/// 副本对象
/// </summary>
public class AuthInstance : InstanceBase
{
    #region 内部接口

    /// <summary>
    /// 执行事件
    /// </summary>
    private void DoEvent(LPCMapping eventData)
    {
        // 副本已经结束
        if (IsEnd)
            return;

        // 获取事件
        int eventId = eventData["event"].AsInt;
        LPCMapping eventArg = eventData["event_arg"].AsMapping;

        // 根据不同事件行不同的操作
        switch (eventId)
        {
            case InstanceConst.INSTANCE_CREATE_RESOURCE:

                // 通知副本进场
                Coroutine.DispatchService(DoCreateResource(eventArg), true);

                break;

            case InstanceConst.INSTANCE_PREPARE_COMBAT:

                // 通知副本进场
                Coroutine.DispatchService(DoPrepareCombat(eventArg), true);

                break;

            case InstanceConst.INSTANCE_ENTER_MAP:

                // 通知副本进场
                Coroutine.DispatchService(DoEnterMap(eventArg), true);

                break;

            case InstanceConst.INSTANCE_CROSS_MAP:

                // 通知玩家过图
                Coroutine.DispatchService(DoCrossMap(eventData["event_arg"].AsMapping), true);

                break;

            case InstanceConst.INSTANCE_CLEARANCE:

                // 通知玩家副本通关
                DoInstanceClearance(true);

                break;
        }
    }

    /// <summary>
    /// 创建资源
    /// </summary>
    private IEnumerator DoCreateResource(LPCMapping eventArg)
    {
        // 创建攻方成员
        CreateFighterProperty();

        // 创建守方成员
        CreateDefenderProperty();

        // 创建副本固有固有资源
        CreateBatchResource();

        // 资源创建完成自动转变副本阶段
        ChangeState();

        // 退出协程
        yield break;
    }

    /// <summary>
    /// 副本准备战斗阶段
    /// </summary>
    private IEnumerator DoPrepareCombat(LPCMapping eventArg)
    {
        // 副本已经结束
        if (IsEnd)
            yield break;

        // 战斗对象列表
        CombatPropertyList.Clear();

        // 获取当前阶段准备战斗批次
        LPCValue batch = eventArg["batch"];

        // 必须保证配置了batch
        System.Diagnostics.Debug.Assert(batch != null);

        LPCArray curBatch;
        if (batch.IsInt)
            curBatch = new LPCArray(batch);
        else if (batch.IsArray)
            curBatch = batch.AsArray;
        else
            curBatch = LPCArray.Empty;

        // 将下一个阶段需要进入战斗的成员添加到战斗列表中
        foreach (Property ob in FighterList)
        {
            // 资源对象已经不存在或者正在析构过程中
            if (ob == null || ob.IsDestroyed)
                continue;

            // 清除召唤实体排除替身
            CombatSummonMgr.RemoveEmployeeList(ob, false);

            // 添加替身列表
            CombatPropertyList.AddRange(CombatSummonMgr.GetEmployeeList(ob));

            // 标识对象进入地图完成
            CombatPropertyList.Add(ob);
        }

        // 通知副本怪物进场
        foreach (Property ob in ResourceMap.Values)
        {
            // 资源对象已经不存在或者正在析构过程中
            if (ob == null || ob.IsDestroyed)
                continue;

            // 不是当前批次的怪物不允许进场
            if (curBatch.IndexOf(ob.Query<int>("batch")) == -1)
                continue;

            // 怪物已经死亡了不能进入
            if (ob.CheckStatus("DIED"))
                continue;

            // 设置阵营为CAMP_TYPE_ATTACK
            ob.CampId = CampConst.CAMP_TYPE_DEFENCE;

            // 添加列表
            CombatPropertyList.Add(ob);
        }

        // 自动转换副本阶段
        ChangeState();
    }

    /// <summary>
    /// 攻守双方进场
    /// </summary>
    private IEnumerator DoEnterMap(LPCMapping eventArg)
    {
        // 副本已经结束
        if (IsEnd)
            yield break;

        // Copy一份配置表数据
        LPCMapping tacticsArg = eventArg.Copy();

        // 载入场景
        if (eventArg.ContainsKey("scene_id"))
        {
            // 获取前景id
            SceneId = eventArg.GetValue<string>("scene_id");

            // 获取当前攻击者数量
            Dictionary<string, int> fighterCount = new Dictionary<string, int>();
            foreach (Property ob in FighterList)
            {
                if (!fighterCount.ContainsKey(ob.FormationRaw))
                    fighterCount.Add(ob.FormationRaw, 1);
                else
                    fighterCount[ob.FormationRaw] += 1;
            }

            // 如果是载入场景的方式进场则需要通知攻方进场
            foreach (Property ob in FighterList)
            {
                // 资源对象已经不存在或者正在析构过程中
                if (ob == null || ob.IsDestroyed)
                    continue;

                // 标识对象进入地图完成
                ob.Delete("is_ready_combat");

                // 设置阵营为CAMP_TYPE_ATTACK
                ob.CampId = CampConst.CAMP_TYPE_ATTACK;

                // 根据宠物个数设置阵型id
                ob.FormationId = fighterCount[ob.FormationRaw];

                // 执行进场策略
                TacticsMgr.DoTactics(ob, TacticsConst.TACTICS_TYPE_ENTER_MAP, tacticsArg);

                // 通知召唤实体执行进场策略
                foreach (Property summonOb in CombatSummonMgr.GetEmployeeList(ob))
                {
                    // 标识对象进入地图完成
                    summonOb.Delete("is_ready_combat");
                    summonOb.Delete("direct_enter_map");

                    // 执行进场策略
                    TacticsMgr.DoTactics(summonOb, TacticsConst.TACTICS_TYPE_ENTER_MAP, tacticsArg);
                }
            }
        }
        else
        {
            // 没有配置的数据只是部分怪物延迟进场或则转换阶段进场
            // 这个位置需要添加scene_id，因为宠物进入战斗场景需要用到scene_id
            tacticsArg.Add("scene_id", SceneId);
        }

        // 记录当前批次
        LPCArray curBatch;
        if (eventArg["batch"].IsInt)
            curBatch = new LPCArray(eventArg["batch"]);
        else if (eventArg["batch"].IsArray)
            curBatch = eventArg.GetValue<LPCArray>("batch");
        else
            curBatch = LPCArray.Empty;

        // 获取当前防守人员数量
        int defenseCount = 0;
        List<Property> defenseList = new List<Property>();

        // 通知副本怪物进场
        foreach (Property ob in ResourceMap.Values)
        {
            // 资源对象已经不存在或者正在析构过程中
            if (ob == null || ob.IsDestroyed)
                continue;

            // 不是当前批次的怪物不允许进场
            if (curBatch.IndexOf(ob.Query<int>("batch")) == -1)
                continue;

            // 怪物已经死亡了不能进入
            if (ob.CheckStatus("DIED"))
                continue;

            // 添加防御列表
            defenseCount++;
            defenseList.Add(ob);
        }

        // 通知副本怪物进场
        foreach (Property ob in defenseList)
        {
            // 设置阵营
            LPCValue campId = ob.Query<LPCValue>("camp_id");
            if (campId != null)
                ob.CampId = campId.AsInt;
            else
                ob.CampId = CampConst.CAMP_TYPE_DEFENCE;

            // 根据宠物个数设置阵型id
            ob.FormationId = defenseCount;

            // 执行进场策略
            TacticsMgr.DoTactics(ob, TacticsConst.TACTICS_TYPE_ENTER_MAP, tacticsArg);
        }
    }

    /// <summary>
    /// 攻方过图
    /// </summary>
    private IEnumerator DoCrossMap(LPCMapping para)
    {
        // 副本已经结束
        if (IsEnd)
            yield break;

        // 通知副本怪物过场，副本怪物过场直接隐藏目标
        foreach (Property ob in ResourceMap.Values)
        {
            // 资源对象已经不存在或者正在析构过程中
            if (ob == null || ob.IsDestroyed)
                continue;

            // 清除召唤实体
            CombatSummonMgr.RemoveEmployeeList(ob);

            // 已经析构的怪物退场
            ob.LeaveCombatMap();
        }

        // 通知攻方过图
        foreach (Property ob in FighterList)
        {
            // 资源对象已经不存在或者正在析构过程中
            if (ob == null || ob.IsDestroyed)
                continue;

            // 通知召唤实体过图
            foreach (Property summonOb in CombatSummonMgr.GetEmployeeList(ob))
            {
                // 抛出公式供策划使用副本过图
                INSTANCE_CROSS_MAP.Call(summonOb);

                // 怪物已经死亡了不能进入, 直接析构
                if (summonOb.CheckStatus("DIED"))
                {
                    // 已经析构的怪物退场
                    summonOb.LeaveCombatMap();
                    continue;
                }

                // 执行过图策略
                TacticsMgr.DoTactics(summonOb, TacticsConst.TACTICS_TYPE_CROSS_MAP, LPCMapping.Empty);
            }

            // 抛出公式供策划使用副本过图
            INSTANCE_CROSS_MAP.Call(ob);

            // 怪物已经死亡了不能进入, 直接析构
            if (ob.CheckStatus("DIED"))
            {
                // 已经析构的怪物退场
                ob.LeaveCombatMap();
                continue;
            }
        }

        // 自动转变副本阶段
        ChangeState();
    }

    /// <summary>
    /// 创建一批资源
    /// </summary>
    private void CreateBatchResource()
    {
        // 获取副本地图类型
        int mapType = InstanceMgr.GetInstanceMapType(InstanceId);

        // 如果是通天塔副本资源获取方式不同于普通副本
        if (mapType == MapConst.TOWER_MAP)
        {
            // 获取通天他副本资源
            LPCMapping resourceMap = Query<LPCMapping>("resource_map");

            // 没有需要创建的资源
            if (resourceMap == null || resourceMap.Count == 0)
                return;

            // 遍历各个资源逐个创建
            foreach (int batch in resourceMap.Keys)
            {
                // 创建各个资源
                List<CsvRow> resourceList = TowerMgr.GetResources(resourceMap[batch].AsString);
                foreach (CsvRow resource in resourceList)
                {
                    // 数据格式不正确
                    if (resource == null)
                        continue;

                    // 创建资源
                    CreateResource(batch, resource, LPCMapping.Empty);
                }
            }
        }
        else if (mapType == MapConst.PET_DUNGEONS_MAP)
        {
            // 获取副本配置资源
            List<CsvRow> resourceList = InstanceMgr.GetInstanceResources(InstanceId);

            // 没有需要创建的资源
            if (resourceList == null || resourceList.Count == 0)
                return;

            int batch;

            // 遍历各个资源逐个创建
            foreach (CsvRow resource in resourceList)
            {
                // 数据格式不正确
                if (resource == null)
                    continue;

                // 获取资源批次
                batch = resource.Query<int>("batch");

                // 创建资源
                CreateResource(
                    batch,
                    resource,
                    InstanceMgr.GetPetDungeonAttrib(Query<int>("pet_id"), InstanceId, batch, resource.Query<int>("pos")));
            }
        }
        else
        {
            // 获取副本配置资源
            List<CsvRow> resourceList = InstanceMgr.GetInstanceResources(InstanceId);

            // 没有需要创建的资源
            if (resourceList == null || resourceList.Count == 0)
                return;

            // 遍历各个资源逐个创建
            foreach (CsvRow resource in resourceList)
            {
                // 数据格式不正确
                if (resource == null)
                    continue;

                // 创建资源
                CreateResource(resource.Query<int>("batch"), resource, LPCMapping.Empty);
            }
        }
    }

    /// <summary>
    /// 创建单个资源
    /// </summary>
    private void CreateResource(int batch, CsvRow data, LPCMapping attribMap)
    {
        // 必须有class_id_script
        int classIdScript = data.Query<int>("class_id_script");

        // Assert判断是否存在class_id_script
        System.Diagnostics.Debug.Assert(classIdScript != 0);

        // 如果副本有指定宠物pet_id
        LPCValue args = data.Query<LPCValue>("class_id_args");
        LPCValue petId = Query<LPCValue>("pet_id");
        if (petId != null && ! petId.IsUndefined)
            args = petId;

        // 调用脚本参数计算怪物class_id;
        int classId = (int) ScriptMgr.Call(classIdScript, Query<int>("level"), args, attribMap);

        // 获取怪物信息
        CsvRow monsterInfo = MonsterMgr.MonsterCsv.FindByKey(classId);

        // 是没有配置的怪物不处理
        if (monsterInfo == null)
            return;

        // 构建新的rid
        string rid = Rid.New();

        // 构建参数
        LPCMapping para = new LPCMapping();
        para.Add("class_id", classId);
        para.Add("instance_resource", 1);
        para.Add("batch", batch);
        para.Add("rid", rid);
        para.Add("is_combat_actor", 1);
        para.Add("difficulty", Query<int>("difficulty"));
        para.Add("layer", Query<int>("layer"));

        // 如果有指定属性, 直接吸入
        para.Append(attribMap);

        // 获取始化参数;
        int initScript = data.Query<int>("init_script");
        LPCMapping initArgs = ScriptMgr.Call(initScript, Query<int>("level"),
            data.Query<LPCValue>("init_script_args"), para) as LPCMapping;

        // 获取始化参数
        para.Append(initArgs);

        // 添加副本资源记录
        LPCMapping instance = LPCMapping.Empty;
        instance.Add("id", this.Query("instance_id"));
        instance.Add("rid", this.GetRid());
        para.Add("instance", instance);

        // 获取怪物的modelId
        para.Add("model", monsterInfo.Query<LPCValue>("model"));

        // 创建资源
        Property propertyOb = PropertyMgr.CreateProperty(para);

        // 创建资源失败
        if (propertyOb == null)
        {
            LogMgr.Trace("创建副本资源batch:{0},pos:{1}失败", data.Query<int>("batch"), data.Query<int>("pos"));
            return;
        }

        // 设置对象的阵营站位信息
        int posId = data.Query<int>("pos");

        // 默认站在0号位置的为队长
        propertyOb.IsLeader = (posId == 0) ? true : false;

        // 设置角色阵型信息
        propertyOb.FormationPos = posId;
        propertyOb.FormationRaw = FormationConst.RAW_NONE;

        // 添加到资源列表中
        AddResource(rid, propertyOb);
    }

    /// <summary>
    /// 清除副本资源
    /// </summary>
    private void CleanUp()
    {
        // 逐个析构副本资源
        foreach (Property ob in ResourceMap.Values)
        {
            // 资源对象已经不存在或者正在析构过程中
            if (ob == null || ob.IsDestroyed)
                continue;

            // 析构玩家召唤怪物列表
            CombatSummonMgr.RemoveEmployeeList(ob);

            // 析构自身资源
            ob.Destroy();
        }

        // 清除副本攻方宠物
        foreach (Property ob in FighterList)
        {
            // 资源对象已经不存在或者正在析构过程中
            if (ob == null || ob.IsDestroyed)
                continue;

            // 析构自身资源
            ob.Destroy();
        }
    }

    /// <summary>
    /// 创建攻方成员
    /// </summary>
    private void CreateFighterProperty()
    {
        // 没有设置攻击对象信息
        if (FighterMap == null || FighterMap.Count == 0)
            return;

        // 获取角色列表数量
        int formationPos = 0;

        // 获取玩家的竞技场连胜buff
        LPCArray assignedProps = LPCArray.Empty;
        int mapType = InstanceMgr.GetMapTypeByInstanceId(InstanceId);
        if (mapType.Equals(MapConst.ARENA_MAP))
            assignedProps = ArenaMgr.GetArenaBuff(Query<int>("win_times"));

        // 遍历各个排出战宠物
        foreach(string raw in FormationConst.ALL_RAW_TYPE)
        {
            // 获取该排出战宠物列表
            LPCArray fighters = FighterMap.GetValue<LPCArray>(raw);

            // 该排没有出战宠物
            if (fighters == null || fighters.Count == 0)
                continue;

            // 重置formationPos
            formationPos = 0;

            // 便利该排各个宠物数据
            for (int i = 0; i < fighters.Count; i++)
            {
                // 数据格式不正确
                if (fighters[i] == null ||
                    ! fighters[i].IsMapping ||
                    fighters[i].AsMapping.Count == 0)
                    continue;

                // 转换数据格式
                LPCMapping fighterDbase = fighters[i].AsMapping;

                // 获取怪物信息
                CsvRow monsterInfo = MonsterMgr.MonsterCsv.FindByKey(fighterDbase["class_id"].AsInt);

                // 是没有配置的怪物不处理
                if (monsterInfo == null)
                    continue;

                // 添加副本资源记录
                LPCMapping instance = LPCMapping.Empty;
                instance.Add("id", this.Query("instance_id"));
                instance.Add("rid", this.GetRid());
                fighterDbase.Add("instance", instance);

                // 获取怪物的modelId
                fighterDbase.Add("model", monsterInfo.Query<LPCValue>("model"));

                // clone一份数据出来，为什么这个地方需要clone一份数据
                // 主要是放在宠物数据在战斗过程中发生了变化
                // 标识需要创建战斗对象
                string originalRid = fighterDbase.GetValue<string>("rid");
                fighterDbase.Add("original_rid", originalRid);
                fighterDbase.Add("rid", Rid.New());
                fighterDbase.Add("is_combat_actor", 1);
                fighterDbase.Add("assigned_props", assignedProps); // 分配临时数据

                // 创建出战宠物对象
                Property fighterOb = PropertyMgr.CreateProperty(fighterDbase, true);

                // 宠物对象创建失败
                if (fighterOb == null)
                    continue;

                // 包含了下属物件信息
                DoPropertyLoaded(fighterOb);

                // 我们始终使用0号位置为队长位置
                fighterOb.IsLeader = (fighterOb.Query<int>("is_leader") == 1);

                // 设置对象的阵营站位信息, 默认fighterAmount为阵型id
                fighterOb.FormationPos = formationPos;
                fighterOb.FormationRaw = raw;

                // formationPos++
                formationPos++;

                // 添加列表
                // 记录攻方列表，在副本中玩家宠物属于攻方成员，副本怪物属于守方成员
                FighterList.Add(fighterOb);
            }
        }
    }

    /// <summary>
    /// 创建防守成员
    /// </summary>
    private void CreateDefenderProperty()
    {
        // 没有防守信息
        if (Defenders == null || Defenders.Count == 0)
            return;

        // 1. 创建防守宠物
        LPCArray defensePetList = Defenders.GetValue<LPCArray>("defense_list");

        // 获取角色列表数量
        int formationPos = 0;

        // 遍历创建各个对象
        for (int i = 0; i < defensePetList.Count; i++)
        {
            // 数据格式不正确
            if (defensePetList[i] == null ||
                ! defensePetList[i].IsMapping ||
                defensePetList[i].AsMapping.Count == 0)
                continue;

            // 转换数据格式
            LPCMapping petDbase = defensePetList[i].AsMapping;

            // 获取怪物信息
            CsvRow monsterInfo = MonsterMgr.MonsterCsv.FindByKey(petDbase["class_id"].AsInt);

            // 是没有配置的怪物不处理
            if (monsterInfo == null)
                continue;

            // 添加副本资源记录
            LPCMapping instance = LPCMapping.Empty;
            instance.Add("id", this.Query("instance_id"));
            instance.Add("rid", this.GetRid());
            petDbase.Add("instance", instance);

            // 获取怪物的modelId
            petDbase.Add("model", monsterInfo.Query<LPCValue>("model"));

            // clone一份数据出来，为什么这个地方需要clone一份数据
            // 主要是放在宠物数据在战斗过程中发生了变化
            // 标识需要创建战斗对象
            petDbase.Add("original_rid", petDbase.GetValue<string>("rid"));
            petDbase.Add("rid", Rid.New());
            petDbase.Add("is_combat_actor", 1);
            petDbase.Add("batch", 0); // 默认批次为0

            // 创建出战宠物对象
            Property petOb = PropertyMgr.CreateProperty(petDbase, true);

            // 宠物对象创建失败
            if (petOb == null)
                continue;

            // 包含了下属物件信息
            DoPropertyLoaded(petOb);

            // 我们始终使用0号位置为队长位置
            petOb.IsLeader = (i == 0) ? true : false;

            // 设置对象的阵营站位信息, 默认fighterAmount为阵型id
            petOb.FormationPos = formationPos;
            petOb.FormationRaw = FormationConst.RAW_NONE;

            // formationPos++
            formationPos++;

            // 添加到资源列表中
            AddResource(petOb.GetRid(), petOb);
        }
    }

    /// <summary>
    /// 载入宠物的附属道具
    /// </summary>
    private void DoPropertyLoaded(Property owner)
    {
        // 获取角色的附属道具
        LPCArray propertyList = owner.Query<LPCArray>("properties");

        // 角色没有附属装备信息
        if (propertyList == null ||
            propertyList.Count == 0)
            return;

        // 转换Container
        Container container = owner as Container;
        LPCMapping dbase = LPCMapping.Empty;
        Property proOb;

        // 遍历各个附属道具
        foreach (LPCValue data in propertyList.Values)
        {
            // 转换数据格式
            dbase = data.AsMapping;

            // 重置一下rid
            dbase.Add("rid", Rid.New());

            // 构建对象
            proOb = PropertyMgr.CreateProperty(dbase, true);

            // 构建对象失败
            if (proOb == null)
                continue;

            // 将道具载入包裹中
            container.LoadProperty(proOb, dbase["pos"].AsString);
        }
    }

    /// <summary>
    /// 战斗准备
    /// </summary>
    private void WhenReadyCombat(int eventId, MixedValue para)
    {
        // 玩家不存在
        LPCMapping eventArgs = para.GetValue<LPCMapping>();

        // rid为Empty
        string rid = eventArgs.GetValue<string>("rid");
        if (string.IsNullOrEmpty(rid))
            return;

        // 角色对象不存在
        // 不是参与战斗的角色不处理
        Property ob = Rid.FindObjectByRid(rid);
        if (ob == null || CombatPropertyList.IndexOf(ob) == -1)
            return;

        // 标识对象进入地图完成
        ob.Set("is_ready_combat", LPCValue.Create(1));

        // 检测是否全部战斗对象准备完成
        foreach (Property combatOb in CombatPropertyList)
        {
            // 如果还有角色没有准备好战斗
            if (combatOb.Query<int>("is_ready_combat", true) == 0)
                return;
        }

        // 副本关卡+1
        Level = Level + 1;

        // 开始回合制战斗, 延迟一秒钟开始战斗
        RoundCombatMgr.StartRoundCombat(CombatPropertyList, 0f, this, RoundCombatConst.ROUND_TYPE_VALIDATION);
    }

    /// <summary>
    /// Whens the round combat end.
    /// </summary>
    private void WhenRoundCombatEnd(int eventId, MixedValue para)
    {
        // 玩家不存在
        LPCMapping args = para.GetValue<LPCMapping>();
        if (args == null)
            return;

        // 不是副本类型
        if (args.GetValue<int>("round_type") != RoundCombatConst.ROUND_TYPE_VALIDATION)
            return;

        // 如果是攻方胜利
        if (args.GetValue<int>("camp_id") == CampConst.CAMP_TYPE_ATTACK)
        {
            ChangeState();
            return;
        }

        // 判断是否需要执行复活处理
        int reviveTimes = Query<int>("revive_times");
        if (ReviveTimes < reviveTimes)
        {
            // 执行复活操作
            RevivePet(CampConst.CAMP_TYPE_ATTACK);
            return;
        }

        // 执行副本通关失败（走到这个地方则默认为验证成功）
        // 攻方角色全部死亡才能走到这个地方
        DoInstanceClearance(true);
    }

    /// <summary>
    /// Whens the loading end.
    /// </summary>
    private void WhenSplashEnd(int eventId, MixedValue para)
    {
        // 玩家不存在
        int type = para.GetValue<int>();
        if (type != InstanceConst.CROSS_MAP)
            return;

        // 转换副本阶段
        ChangeState();
    }

    /// <summary>
    /// 转变副本事件回调
    /// </summary>
    private void DoChangeState(int eventId, MixedValue para)
    {
        // 转换副本阶段
        ChangeState();
    }

    /// <summary>
    /// 复活副本宠物
    /// </summary>
    private void RevivePet(int campId)
    {
        // ReviveTimes++
        ReviveTimes++;

        List<Property> propertyList = new List<Property>();

        // 遍历当前战斗宠物
        foreach (Property ob in CombatPropertyList)
        {
            // 阵营id不一致
            // 如果不是召唤怪物，不在这个地方处理
            if (ob.CampId != campId ||
                ob.Query<LPCValue>("summoner_rid") != null)
                continue;

            // 复活宠物清除死亡状态;
            propertyList.Add(ob);
        }

        // 复活需要复活的角色
        foreach (Property ob in propertyList)
        {
            // 清除死亡状态
            ob.ClearStatus("DIED");

            // 宠物复活后需要做的操作;
            REVIVE_AFTER_PET_OPERATION.Call(ob);
        }

        // 开始回合制战斗, 延迟一秒钟开始战斗
        RoundCombatMgr.StartRoundCombat(CombatPropertyList, 0f, this, RoundCombatConst.ROUND_TYPE_VALIDATION);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 构造函数
    /// </summary>
    public AuthInstance(LPCMapping data) : base(data)
    {
        // 获取副本对象的rid
        string rid = GetRid();

        // 注册战斗准备的回调
        EventMgr.RegisterEvent(rid, EventMgrEventType.EVENT_READY_COMBAT, WhenReadyCombat);

        // 注册战斗结束
        EventMgr.RegisterEvent(rid, EventMgrEventType.EVENT_ROUND_COMBAT_END, WhenRoundCombatEnd);

        // 注册退场回调
        EventMgr.RegisterEvent(rid, EventMgrEventType.EVENT_INSTANCE_CHANGE_STATE, DoChangeState);
    }

    /// <summary>
    /// 转变副本阶段
    /// </summary>
    public void ChangeState()
    {
        // 没有指定副本阶段的直接在当前阶段的基础上+1
        ChangeState(CurState + 1);
    }

    /// <summary>
    /// 转变副本阶段
    /// </summary>
    public void ChangeState(int state)
    {
        // 副本已经结束
        if (IsEnd)
            return;

        // 相同阶段不处理
        if (CurState == state)
            return;

        // 记录当前的阶段
        CurState = state;

        // 获取副本阶段需要处理的副本事件
        LPCArray eventList = GetEvents();

        // 没有事件需要处理
        if (eventList.Count == 0)
            return;

        // 遍历各个事件
        foreach (LPCValue data in eventList.Values)
        {
            // 没有信息，不处理
            if (!data.IsMapping)
                continue;

            // 执行事件
            DoEvent(data.AsMapping);
        }
    }

    /// <summary>
    /// 开始副本
    /// </summary>
    public override void DoStart()
    {
        // 执行基类Start
        base.DoStart();

        // 标识副本已经开始
        IsStarted = true;

        // 转变副本阶段
        ChangeState();
    }

    /// <summary>
    /// 结束副本
    /// </summary>
    public override void DoEnd()
    {
        // 标识副本结束
        IsEnd = true;

        // 停止回合制战斗
        RoundCombatMgr.EndRoundCombat(false);

        // 退出战斗
        CombatMgr.QuitCombat();

        // 清除资源
        CleanUp();

        // 析构副本对象
        this.Destroy();
    }

    /// <summary>
    /// 执行副本失败
    /// </summary>
    public override void DoInstanceFail(Property ob)
    {
        // 副本验证失败
        DoInstanceClearance(false);
    }

    /// <summary>
    /// 通知玩家副本通关
    /// </summary>
    public void DoInstanceClearance(bool result)
    {
        // 副本已经结束
        if (IsEnd)
            return;

        // 标识验证结束
        IsAuthEnd = true;

        int mapType = InstanceMgr.GetMapTypeByInstanceId(InstanceId);
        if (mapType.Equals(MapConst.SECRET_DUNGEONS_MAP))
        {
            // 获取boss血量
            foreach (Property ob in ResourceMap.Values)
            {
                // 资源对象已经不存在
                if (ob == null || ob.IsDestroyed)
                    continue;

                // 如果不是boss不处理
                if (ob.Query<int>("is_boss") != 1)
                    continue;

                // 如果boss血量匹配这表明验证通过
                if (ob.QueryTemp<int>("hp_rate") <= Query<int>("kill_amount"))
                    result = true;

                break;
            }
        }

        // 通知服务器验证结果
        Operation.CmdACAuthResult.Go(GetRid(), result);
    }

    #endregion
}
