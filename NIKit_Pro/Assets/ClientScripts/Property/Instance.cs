/// <summary>
/// Instance.cs
/// Create by zhaozy 2014-11-12
/// 副本对象
/// </summary>

using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using LPC;

/// <summary>
/// 副本对象
/// </summary>
public class Instance : InstanceBase
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
                DoInstanceClearance(true, RoundCombatConst.END_TYPE_WIN);

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

        // 收集所有角色的技能
        // 这个地方需要添加3个通用技能id（通用回血，回蓝，受创）
        List<int> skillList = new List<int>() { 11000, 11001, 12000 };
        LPCArray skills = LPCArray.Empty;
        List<KeyValuePair<string, bool>> resList = new List<KeyValuePair<string, bool>>();
        int skillId = 0;

        // 遍历攻方角色
        foreach (Property ob in FighterList)
        {
            // 获取角色的技能
            skills = ob.GetAllSkills();

            // 筛选技能
            foreach (LPCValue mks in skills.Values)
            {
                // 获取技能id
                skillId = mks.AsArray[0].AsInt;

                // 技能已经在列表中
                if (skillList.Contains(skillId))
                    continue;

                // 添加到技能列表中
                skillList.Add(skillId);

                // 添加技能icon
                resList.Add(new KeyValuePair<string, bool>(
                    string.Format(SkillMgr.GetIconResPath(SkillMgr.GetIcon(skillId))), true)
                );
            }

            // 添加该角色的关联怪物资源
            foreach(LPCValue classId in MonsterMgr.GetRelatePetList(ob.Query<int>("class_id")).Values)
            {
                // 获取技能信息
                CsvRow data = MonsterMgr.GetRow(classId.AsInt);
                if (data == null)
                    continue;

                // 添加关联宠物模型信息
                resList.Add(new KeyValuePair<string, bool>(
                    string.Format("Assets/Prefabs/Model/{0}.prefab", data.Query<string>("model")), true)
                );

                LPCMapping initSkills = data.Query<LPCMapping>("init_skills");
                foreach (LPCValue tSkills in initSkills.Values)
                {
                    foreach (LPCValue mks in tSkills.AsArray.Values)
                    {
                        // 获取技能id
                        skillId = mks.AsArray[0].AsInt;

                        // 技能已经在列表中
                        if (skillList.Contains(skillId))
                            continue;

                        // 添加到技能列表中
                        skillList.Add(skillId);

                        // 添加技能icon
                        resList.Add(new KeyValuePair<string, bool>(
                            string.Format(SkillMgr.GetIconResPath(SkillMgr.GetIcon(skillId))), true)
                        );
                    }
                }
            }

        }

        // 遍历副本资源（防守方）
        foreach (Property ob in ResourceMap.Values)
        {
            // 获取角色的技能
            skills = ob.GetAllSkills();

            // 筛选技能
            foreach (LPCValue mks in skills.Values)
            {
                // 获取技能id
                skillId = mks.AsArray[0].AsInt;

                // 技能已经在列表中
                if (skillList.Contains(skillId))
                    continue;

                // 添加到技能列表中
                skillList.Add(skillId);
            }

            // 添加该角色的关联怪物资源
            foreach(LPCValue classId in MonsterMgr.GetRelatePetList(ob.Query<int>("class_id")).Values)
            {
                // 获取技能信息
                CsvRow data = MonsterMgr.GetRow(classId.AsInt);
                if (data == null)
                    continue;

                // 添加关联宠物模型信息
                resList.Add(new KeyValuePair<string, bool>(
                    string.Format("Assets/Prefabs/Model/{0}.prefab", data.Query<string>("model")), true)
                );

                LPCMapping initSkills = data.Query<LPCMapping>("init_skills");
                foreach (LPCValue tSkills in initSkills.Values)
                {
                    foreach (LPCValue mks in tSkills.AsArray.Values)
                    {
                        // 获取技能id
                        skillId = mks.AsArray[0].AsInt;

                        // 技能已经在列表中
                        if (skillList.Contains(skillId))
                            continue;

                        // 添加到技能列表中
                        skillList.Add(skillId);
                    }
                }
            }
        }

        // 执行预加载资源
        ResList = PreloadMgr.GetResourceList(skillList, true);

        // 获取副本场景列表
        ResList.AddRange(InstanceMgr.GetPreloadList(InstanceId));

        // 添加技能icon
        ResList.AddRange(resList);

        // 预加载资源(如果有需要可以调整为根据副本id预加载副本资源)
        PreloadMgr.DoPreload("Combat", ResList, true, true);

        // 判断资源是否预加载结束
        while (! PreloadMgr.IsLoadEnd("Combat"))
            yield return null;

        // 记录副本开始时间
        StartTick = TimeMgr.CombatTick;

        // 标识副本已经开始
        IsStarted = true;
    }

    /// <summary>
    /// 副本准备战斗阶段
    /// </summary>
    private IEnumerator DoPrepareCombat(LPCMapping eventArg)
    {
        // 副本已经结束
        if (IsEnd)
            yield break;

        // 取消过图标识
        this.SetTemp("is_cross_map", LPCValue.Create(0));

        // 每次重新开始战斗是需要Unlock目标
        AutoCombatMgr.UnlockCombatTarget(AutoCombatMgr.LockTargetOb);

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

            // 载入场景
            SceneMgr.LoadScene("Main", SceneId, new CallBack(OnLoadSceneAfter));

            // 等待场景加载结束
            while(! SceneMgr.IsActiveScene(SceneId))
                yield return null;

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
    /// 打开战斗场景回调
    /// </summary>
    private void OnLoadSceneAfter(object para, object[] param)
    {
        // 延迟掉落
        Coroutine.DispatchService(DoDelayLoadSceneAfter());
    }

    /// <summary>
    /// 协程播放掉落动画
    /// </summary>
    private IEnumerator DoDelayLoadSceneAfter()
    {
        // 播放副本刷屏界面
        WindowMgr.PlaySplashWnd(InstanceConst.ENTER_MAP);

        // 等待一帧
        yield return null;

        // 关闭副本加载界面
        LoadingMgr.HideLoadingWnd(LoadingType.LOAD_TYPE_INSTANCE);
    }

    /// <summary>
    /// 攻方过图
    /// </summary>
    private IEnumerator DoCrossMap(LPCMapping para)
    {
        // 副本已经结束
        if (IsEnd)
            yield break;

        // 标识正在过图中
        this.SetTemp("is_cross_map", LPCValue.Create(1));

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

            // 执行进场策略
            TacticsMgr.DoTactics(ob, TacticsConst.TACTICS_TYPE_CROSS_MAP, LPCMapping.Empty);
        }

        // 抛出副本闪屏事件
        WindowMgr.PlaySplashWnd(InstanceConst.CROSS_MAP);
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
        para.Add("auto_combat_select_type", data.Query<string>("auto_combat_select_type"));

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
            LogMgr.Trace("创建副本资源batch:{0},pos:{1}失败", batch, data.Query<int>("pos"));
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
    /// 开始副本
    /// </summary>
    private void _DoStart(object para, object[] expara)
    {
        // 初始化飘雪框
        BloodTipMgr.Init();

        // 打开战斗界面
        WindowMgr.OpenCombatWnd(this);

        // 转变副本阶段
        ChangeState();
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
        // 如果副本已经结束
        if (IsEnd)
            return;

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
        RoundCombatMgr.StartRoundCombat(CombatPropertyList, 0.1f, this, RoundCombatConst.ROUND_TYPE_INSTANCE);

        // 副本新关卡开始事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_INSTANCE_LEVEL_START, MixedValue.NewMixedValue<Instance>(this), true);
    }

    /// <summary>
    /// Whens the loading end.
    /// </summary>
    private void WhenLoadingEnd(int eventId, MixedValue para)
    {
        // 玩家不存在
        LPCMapping args = para.GetValue<LPCMapping>();
        if (args == null)
            return;

        // 不是副本类型
        if (args.GetValue<int>("type") != LoadingType.LOAD_TYPE_INSTANCE)
            return;

        // 播放副本对应地图相应的场景背景音效
        LPCMapping data = InstanceMgr.GetInstanceInfo(InstanceId);
        GameSoundMgr.PlayBgmMusic(data["map_id"].AsString);

        // 副本load结束转换副本阶段
        ChangeState();
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
        if (args.GetValue<int>("round_type") != RoundCombatConst.ROUND_TYPE_INSTANCE)
            return;

        // 如果是攻方胜利
        if (args.GetValue<int>("camp_id") == CampConst.CAMP_TYPE_ATTACK)
        {
            ChangeState();
            return;
        }

        // 执行副本通关失败
        DoInstanceClearance(false, args.GetValue<int>("end_type"));
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

        // 回收一下GC
        ResourceMgr.DoRecycleGC();

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

    ///<summary>
    /// 清除附加状态
    /// </summary>
    private void WhenStatusClear(int eventId, MixedValue para)
    {
        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();

        // 判断是否是清除死亡状态
        LPCArray statusList = args.GetValue<LPCArray>("status_list");
        if (statusList.IndexOf(StatusMgr.GetStatusIndex("DIED")) == -1)
            return;

        // 刷新人数重叠次数
        RefreshActorCrossTimes();
    }

    /// <summary>
    /// 角色死亡事件回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    private void WhenCharDie(int eventId, MixedValue para)
    {
        // 刷新人数重叠次数
        RefreshActorCrossTimes();
    }

    /// <summary>
    /// 判断当前两派人数发生了交汇
    /// </summary>
    private void RefreshActorCrossTimes()
    {
        LPCMapping remainMap = LPCMapping.Empty;

        // 遍历当前参战人数
        foreach (Property ob in CombatPropertyList)
        {
            // 资源对象已经不存在或者正在析构过程中
            // 怪物已经死亡了不处理
            // 如果是召唤怪物不处理
            if (ob == null ||
                ob.CheckStatus("DIED") ||
                ob.Query<LPCValue>("summoner_rid") != null)
                continue;

            // 汇总人数
            remainMap.Add(ob.CampId, remainMap.GetValue<int>(ob.CampId) + 1);
        }

        // 是否一致
        if (remainMap.GetValue<int>(CampConst.CAMP_TYPE_ATTACK) !=
            remainMap.GetValue<int>(CampConst.CAMP_TYPE_DEFENCE))
            return;

        // 增加战斗人员教会次数
        ActorCrossTimes += 1;
    }

    /// <summary>
    /// 获取存活使魔数量
    /// </summary>
    /// <returns><c>true</c> if this instance is all alive; otherwise, <c>false</c>.</returns>
    private int GetAliveAmount()
    {
        int aliveAmount = 0;

        // 通知攻方过图
        foreach (Property ob in FighterList)
        {
            // 对象已经不存在
            // 对象已经死亡
            if (ob == null ||
                ob.CheckStatus("DIED"))
                continue;

            // 统计存活数量
            aliveAmount++;
        }

        // 返回存活使魔数量
        return aliveAmount;
    }

    /// <summary>
    /// 获取战斗双方剩余人数
    /// </summary>
    private int GetRemainCombatPropertyAmount(int campId)
    {
        int remainAmount = 0;

        // 遍历当前参战人数
        foreach (Property ob in CombatPropertyList)
        {
            // 资源对象已经不存在或者正在析构过程中
            // campId不一致不处理
            // 怪物已经死亡了不处理
            // 如果是召唤怪物不处理
            if (ob == null ||
                campId != ob.CampId ||
                ob.CheckStatus("DIED") ||
                ob.Query<LPCValue>("summoner_rid") != null)
                continue;

            // 汇总人数
            remainAmount += 1;
        }

        // 增加战斗人员教会次数
        return remainAmount;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 构造函数
    /// </summary>
    public Instance(LPCMapping data) : base(data)
    {
        // 获取副本对象的rid
        string rid = GetRid();

        // 副本开始设置自动战斗标识
        AutoCombatMgr.AutoCombat = true;

        // 注册战斗准备的回调
        EventMgr.RegisterEvent(rid, EventMgrEventType.EVENT_READY_COMBAT, WhenReadyCombat);

        // loading结束
        EventMgr.RegisterEvent(rid, EventMgrEventType.EVENT_LOADING_END, WhenLoadingEnd);

        // 注册战斗结束
        EventMgr.RegisterEvent(rid, EventMgrEventType.EVENT_ROUND_COMBAT_END, WhenRoundCombatEnd);

        // 注册副本闪屏结束事件
        EventMgr.RegisterEvent(rid, EventMgrEventType.EVENT_INSTANCE_SPLASH_END, WhenSplashEnd);

        // 注册退场回调
        EventMgr.RegisterEvent(rid, EventMgrEventType.EVENT_INSTANCE_CHANGE_STATE, DoChangeState);

        // 注册清除状态
        EventMgr.RegisterEvent(rid, EventMgrEventType.EVENT_CLEAR_STATUS, WhenStatusClear);

        // 注册玩家死亡事件
        EventMgr.RegisterEvent(rid, EventMgrEventType.EVENT_DIE, WhenCharDie);
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

        float scale = 0f;
        IsLoopFight = InstanceMgr.GetLoopFightByInstanceId(InstanceId);
        if (IsLoopFight)
        {
            // 开启循环战斗默认使用三倍速
            TimeMgr.TempTimeScale = TimeMgr.GetScale(GameSettingMgr.GetSettingInt("loop_fight_scale_multiple"));

            // 默认开启自动战斗
            AutoCombatMgr.SetAutoCombat(true, IsLoopFight);

            scale = TimeMgr.TempTimeScale;
        }
        else
        {
            scale = TimeMgr.TimeScale;
        }

        // 重置时间系统时间缩放
        TimeMgr.DoInitCombatLogic(scale);

        // 策划需求：需要副本开场动画播放完毕后在开始刷怪
        // 暂时做成固定固定延迟一段时间再开始副本
        // 正确做法是应该暂停副本进度（由于涉及较多临时处理）
        // 实际上需要暂停副本进度，玩家心跳，怪物心跳（策略），战斗系统等
        Timer.New(0.5f, new CallBack(_DoStart));
    }

    /// <summary>
    /// 结束副本
    /// </summary>
    public override void DoEnd()
    {
        // 副本结束开启资源回收机制
        ResourceMgr.AutoRecycle = true;

        // 重置时间系统时间缩放, 默认为1倍速度
        TimeMgr.DoInitCombatLogic(1f);

        // 标识副本结束
        IsEnd = true;

        // 停止回合制战斗
        RoundCombatMgr.EndRoundCombat(false);

        // 清除临时自动战斗
        AutoCombatMgr.RemoveTempAutoCombat();

        // 循环战斗清除指定地图的自动战斗标识
        if (IsLoopFight)
            AutoCombatMgr.RemoveAutoCombatByMapId(InstanceMgr.GetMapIdByInstanceId(InstanceId));

        // 退出战斗
        CombatMgr.QuitCombat();

        // 清除资源
        CleanUp();

        // 析构副本对象
        this.Destroy();

        // 更新循环战斗标识
        IsLoopFight = false;

        RoundCombatMgr.LastRoundIsAutoCombat = false;

        // 移除列表中的循环战斗标识
        InstanceMgr.RemoveLoopFight(InstanceId);

        // 预加载资源(如果有需要可以调整为根据副本id预加载副本资源)
        PreloadMgr.Unload("Combat", ResList);
    }

    /// <summary>
    /// 复活副本宠物
    /// </summary>
    public void RevivePet(int campId)
    {
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

        // 恢复副本暂停
        DoContinue();

        // 取消过图标识
        this.SetTemp("is_cross_map", LPCValue.Create(0));

        // 每次重新开始战斗是需要Unlock目标
        AutoCombatMgr.UnlockCombatTarget(AutoCombatMgr.LockTargetOb);

        // 开始回合制战斗, 延迟一秒钟开始战斗
        RoundCombatMgr.StartRoundCombat(CombatPropertyList, 0.1f, this, RoundCombatConst.ROUND_TYPE_INSTANCE);
    }

    /// <summary>
    /// 执行副本失败
    /// </summary>
    public override void DoInstanceFail(Property ob)
    {
        // 该接口暂不支持(验证和回放副本需要该接口)
    }

    /// <summary>
    /// 通知玩家副本通关
    /// </summary>
    public void DoInstanceClearance(bool result, int endType)
    {
        // 副本已经结束
        if (IsEnd)
            return;

        // 标识副本暂停
        DoPause();

        // 标识副本结束
        if (endType == RoundCombatConst.END_TYPE_GIVEUP)
            IsEnd = true;

        // 副本杀怪个数，如果好友秘密地下城则显示boss数量
        int killAmount = 0;
        int mapType = InstanceMgr.GetMapTypeByInstanceId(InstanceId);

        // 如果是指引副本
        if (mapType.Equals(MapConst.GUIDE_INSTANCE_MAP))
        {
            // 抛出指引副本结束事件;
            EventMgr.FireEvent(EventMgrEventType.EVENT_GUIDE_INSTANCE_END, MixedValue.NewMixedValue<string>(InstanceId));
            return;
        }

        // 如果是秘密地下城副本
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

                // 获取boss血量
                killAmount = ob.QueryTemp<int>("hp_rate");
                break;
            }
        }
        else
        {
            // 统计计击杀怪物数量
            foreach (Property ob in ResourceMap.Values)
            {
                // 资源对象已经不存在
                if (ob == null || ob.IsDestroyed)
                    continue;

                // 怪物没有死亡
                if (!ob.CheckStatus("DIED"))
                    continue;

                // 击杀怪物数量++
                killAmount++;
            }
        }

        // 统计胜利方存活人数

        // 普通副本通关
        InstanceMgr.DoInstanceClearance(
            this.GetRid(),
            result,
            endType,
            InstanceId,
            BonusMgr.DropBonusMap,
            LevelActions,
            GetAliveAmount(),
            killAmount,
            ActorCrossTimes,
            GetRemainCombatPropertyAmount(result ? CampConst.CAMP_TYPE_ATTACK : CampConst.CAMP_TYPE_DEFENCE),
            IsLoopFight,
            GetProgressTick() / 1000,
            RoundCount
        );
    }

    #endregion
}
