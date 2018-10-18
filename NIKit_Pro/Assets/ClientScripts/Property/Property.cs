/// <summary>
/// Property.cs
/// Copy from zhangyg 2014-10-22
/// 物件(玩家、怪物、NPC、道具等)的基类
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

/// <summary>
/// 物件(玩家、怪物、NPC、道具等)的基类
/// </summary>
public abstract class Property
{
    #region 变量

    // UniqueId与对象的映射表
    private static Dictionary<int, Property> UniqueIdObs = new Dictionary<int, Property>();

    /// <summary>
    /// dbase属性
    /// </summary>
    public TriggerDbase dbase = new TriggerDbase();
    public TriggerDbase tempdbase = new TriggerDbase();

    /// <summary>
    /// RID属性
    /// </summary>
    public Rid rid = null;

    /// <summary>
    /// 移动属性
    /// </summary>
    public Move move = null;

    /// <summary>
    /// 技能信息
    /// </summary>
    public Skill skill = null;

    /// <summary>
    /// 状态属性
    /// </summary>
    public Status status = null;

    /// <summary>
    /// 属性信息
    /// </summary>
    public Attrib attrib = null;

    // 对象类型
    public int objectType = 0;

    // name属性
    private Name name = new Name();

    /// <summary>
    /// 战斗对象
    /// </summary>
    private CombatActor mActor = null;

    // 实体的表格数据
    private List<CsvRow> basicAttrib = new List<CsvRow>();

    /// <summary>
    /// 资源是否已经被回收
    /// </summary>
    private bool mIsDestroyed = false;

    /// <summary>
    /// 模型世界缩放
    /// </summary>
    private float mWorldScale = 1f;

    /// <summary>
    /// 实体锁定标识
    /// </summary>
    private Dictionary<string, int> mLockMap = new Dictionary<string, int>();

    /// <summary>
    /// 角色唯一id
    /// </summary>
    private int mUniqueId = 0;

    #endregion

    #region 属性

    // 获取Char的actor对象
    public CombatActor Actor { get { return mActor; } }

    /// <summary>
    /// 资源是否已经被回收
    /// </summary>
    public bool IsDestroyed { get { return mIsDestroyed; } }

    /// <summary>
    /// 是否是队长
    /// </summary>
    public bool IsLeader{ get; set; }

    /// <summary>
    /// 归位位置
    /// </summary>
    public Vector3 MoveBackPos{ get; set; }

    /// <summary>
    /// 对象的阵营id
    /// </summary>
    public int CampId{ get; set; }

    /// <summary>
    /// 对象的阵型站位
    /// </summary>
    public int FormationPos{ get; set; }

    /// <summary>
    /// 对象的阵型站位前后排
    /// </summary>
    public string FormationRaw{ get; set; }

    /// <summary>
    /// 对象的阵型id
    /// </summary>
    public int FormationId{ get; set; }

    /// <summary>
    /// 对象场景id
    /// </summary>
    public string SceneId{ get; set; }

    /// <summary>
    /// 角色唯一id
    /// </summary>
    public int UniqueId
    {
        get
        {
            return mUniqueId;
        }

        private set
        {
            // 先取消旧的映射关系
            if (UniqueIdObs.ContainsKey(mUniqueId))
                UniqueIdObs.Remove(mUniqueId);

            // 如果重置mUniqueId == 0
            mUniqueId = value;
            if (mUniqueId == 0)
                return;

            // 重置UniqueIdObs
            UniqueIdObs[mUniqueId] = this;
        }
    }

    /// <summary>
    /// 角色行动回合cookie
    /// </summary>
    public string RoundCookie{ get; set; }

    /// <summary>
    /// 角色行动回合类型
    /// </summary>
    public int RoundType{ get; set; }

    #endregion

    #region 内部接口

    /// <summary>
    /// 当玩家血量变化回调
    /// </summary>
    private void OnHpUpdate(object param, params object[] paramEx)
    {
        // 当玩家血量变化时计算一下当前血量百分比
        int hpRate = Game.Divided(Query<int>("hp"), QueryAttrib("max_hp"));

        // 设置当前血量百分比
        this.SetTemp("hp_rate", LPCValue.Create(hpRate));
    }

    /// <summary>
    /// 速度变化
    /// </summary>
    private void OnSpeedUpdate(object param, params object[] paramEx)
    {
        Actor.MoveSpeed = CALC_MOVE_SPEED.Call(this); // 速度基准
        Actor.SetSpeedFactor(SpeedControlType.SCT_MOVE_RATE, CALC_MOVE_SPEED_RATE_PERCENT.Call(this)); // 移动速度缩放比
        Actor.SetSpeedFactor(SpeedControlType.SCT_ATTACK_SPEED, CALC_ATTACK_SPEED_RATE_PERCENT.Call(this)); // 攻击速度
    }

    /// <summary>
    /// move_speed速度变化
    /// </summary>
    private void OnMoveSpeedUpdate(object param, params object[] paramEx)
    {
        Actor.MoveSpeed = CALC_MOVE_SPEED.Call(this); // 速度基准
        Actor.SetSpeedFactor(SpeedControlType.SCT_MOVE_RATE, CALC_MOVE_SPEED_RATE_PERCENT.Call(this)); // 移动速度缩放比
    }

    /// <summary>
    /// attack_speed速度变化
    /// </summary>
    private void OnAttackSpeedUpdate(object param, params object[] paramEx)
    {
        Actor.SetSpeedFactor(SpeedControlType.SCT_ATTACK_SPEED, CALC_ATTACK_SPEED_RATE_PERCENT.Call(this)); // 攻击速度
    }

    /// <summary>
    /// speed速度变化
    /// </summary>
    private void OnAttackOrderUpdate(object param, params object[] paramEx)
    {
        // 通知回合制战斗速度变化
        RoundCombatMgr.OnAttackOrderUpdate(this);
    }

    #endregion

    public Property(LPCMapping data)
    {
        // 设置名字
        if (data["name"] != null && data["name"].IsString)
            SetName(data["name"].AsString);

        // 构建RID属性
        rid = new Rid(this, data);

        // 构建move属性
        move = new Move(this);

        // skill属性
        skill = new Skill(this);

        // attrib属性
        attrib = new Attrib(this);
    }

    /// <summary>
    /// 回收本物件
    /// </summary>
    virtual public void Destroy()
    {
        // 标识资源已经被回收
        mIsDestroyed = true;

        // 注销监听事件
        EventMgr.UnregisterEvent(GetRid());

        // 2. 析构actor对象
        if (mActor != null)
        {
            CombatActorMgr.DestroyCombatActor(mActor.ActorName);

            // 重置UniqueId
            UniqueId = 0;

            // 删除mActor
            mActor = null;
        }

        // 清除对象的锁定标识
        mLockMap.Clear();

        // 3. 删除数据
        move.Destroy();
        skill.Destroy();
        rid.Destroy();

        // 如果状态对象存在
        if (status != null)
            status.Destroy();

        // 一定要放在最后面，因为前面的析构流程有依赖于dbase数据
        dbase.Clear();
        tempdbase.Clear();
    }

    /// <summary>
    /// 初始化实体的csv表格行
    /// </summary>
    public void SetBasicAttrib(List<CsvRow> basicData)
    {
        basicAttrib = basicData;
    }

    /// <summary>
    /// 创建CombatActor
    /// </summary>
    public void CreateCombatActor(LPCMapping data)
    {
        // 初始化实体的csv表格行
        if (data["dbase"] == null || !data["dbase"].IsMapping)
            return;

        // 获取dbase数据
        LPCMapping dbase = data["dbase"].AsMapping;

        // 不需要创建战斗对象
        if (dbase.GetValue<int>("is_combat_actor") == 0)
            return;

        // 创建actor对象
        string actorName = rid.GetRid();

        // 如果获取不到actorName抛出异常
        System.Diagnostics.Debug.Assert(actorName != null);

        // 创建actor对象
        mActor = CombatActorMgr.CreateCombatActor(actorName, dbase);

        // status始化
        status = new Status(this);

        // 监听影响角色攻击顺序属性变化变化
        this.dbase.RegisterTriggerField("AttackOrder", new string[]
            {
                "final_speed",
                "final_first_hand",
            }, new CallBack(OnAttackOrderUpdate, null));

        // 监听速度变化
        this.dbase.RegisterTriggerField("MoveSpeedUpdate", new string[]
            {
                "move_speed",
                "move_speed_rate",
            }, new CallBack(OnMoveSpeedUpdate, null));

        // 监听attack变化
        this.dbase.RegisterTriggerField("AttackSpeedUpdate", new string[]
            {
                "attack_speed",
            }, new CallBack(OnAttackSpeedUpdate, null));

        // 监听速度变化
        this.tempdbase.RegisterTriggerField("TempMoveSpeedUpdate", new string[]
            {
                "improvement"
            }, new CallBack(OnSpeedUpdate, null));

        // 监听血量变化
        this.dbase.RegisterTriggerField("HpUpdate", new string[]
            {
                "hp",
                "improvement"
            }, new CallBack(OnHpUpdate, null));
    }

    /// <summary>
    /// 增加状态
    /// 该接口是直接修改数据
    /// </summary>
    public void AddStatus(int statusId, int cookie, LPCMapping statusMap)
    {
        if (status == null)
            return;

        // 增加状态
        status.AddStatus(statusId, cookie, statusMap);
    }

    /// <summary>
    /// 移除状态
    /// 该接口是直接修改数据
    /// </summary>
    public void RemoveStatus(int cookie)
    {
        if (status == null)
            return;

        // 获取玩家当前所有状态
        status.RemoveStatus(cookie);
    }

    /// <summary>
    /// 移除状态
    /// 该接口是直接修改数据
    /// </summary>
    public void RemoveStatus(LPCMapping data)
    {
        if (status == null)
            return;

        // 获取玩家当前所有状态
        status.RemoveStatus(data);
    }

    /// <summary>
    /// 获取状态的Condition
    /// </summary>
    public LPCMapping GetStatusCondition(int cookie)
    {
        if (status == null)
            return null;

        // 获取指定状态
        return status.GetStatusCondition(cookie);
    }

    /// <summary>
    /// 获取状态的Condition
    /// </summary>
    public List<LPCMapping> GetStatusCondition(string status)
    {
        // 没有状态信息
        if (status == null)
            return new List<LPCMapping>();

        // 获取状态列表
        List<LPCMapping> statusList = GetAllStatus();
        List<LPCMapping> conditionList = new List<LPCMapping>();
        int statusId = StatusMgr.GetStatusIndex(status);

        // 找到需要移除的数据
        for (int i = 0; i < statusList.Count; i++)
        {
            if (statusList[i] == null)
                continue;

            // status_id不一致
            if (statusList[i].GetValue<int>("status_id") != statusId)
                continue;

            // conditionList
            conditionList.Add(statusList[i]);
        }

        // 返回数据
        return conditionList;
    }

    /// <summary>
    /// 取得所有的状态详细数据
    /// 格式([statusData.....])
    /// </summary>
    public List<LPCMapping> GetAllStatus()
    {
        // 没有状态信息
        if (status == null)
            return new List<LPCMapping>();

        // 获取玩家当前所有状态
        return status.GetAllStatus();
    }

    /// <summary>
    /// 获取buf类型数量
    /// </summary>
    public int GetStatusAmountByType(int type)
    {
        if (status == null)
            return 0;

        // 获取玩家当前所有状态
        return status.GetStatusAmountByType(type);
    }

    /// <summary>
    /// 检测状态
    /// </summary>
    public bool CheckStatus(List<string> statusList)
    {
        // 遍历列表
        for (int i = 0; i < statusList.Count; i++)
        {
            // 有指定状态
            if (CheckStatus(StatusMgr.GetStatusIndex(statusList[i])))
                return true;
        }

        // 返回false
        return false;
    }

    /// <summary>
    /// 检测状态
    /// </summary>
    public bool CheckStatus(LPCArray statusList)
    {
        // 遍历列表
        for (int i = 0; i < statusList.Count; i++)
        {
            // 有指定状态
            if (CheckStatus(StatusMgr.GetStatusIndex(statusList[i].AsString)))
                return true;
        }

        // 返回false
        return false;
    }

    /// <summary>
    /// 检测状态
    /// </summary>
    public bool CheckStatus(string status)
    {
        return CheckStatus(StatusMgr.GetStatusIndex(status));
    }

    /// <summary>
    /// 检测状态
    /// </summary>
    public bool CheckStatus(int statusId)
    {
        // 没有status对象
        if (status == null)
            return false;

        // 返回check结果
        return status.CheckStatus(statusId);
    }

    /// <summary>
    /// 附加状态
    /// </summary>
    public void ApplyStatus(string status, LPCMapping condition)
    {
        // 附加状态
        StatusMgr.ApplyStatus(this, status, condition);
    }

    /// <summary>
    /// 增加状态作用回合
    /// </summary>
    public void AddStatusRounds(List<int> cookieList, int times)
    {
        // 附加状态
        StatusMgr.AddStatusRounds(this, cookieList, times);
    }

    /// <summary>
    /// 增加状态作用回合
    /// </summary>
    public void AddStatusRounds(int cookie, int times)
    {
        // 附加状态
        StatusMgr.AddStatusRounds(this, cookie, times);
    }

    /// <summary>
    /// 清除状态
    /// </summary>
    public void ClearStatusByCookie(List<int> cookieList, int clearType = StatusConst.CLEAR_TYPE_END_SELF)
    {
        StatusMgr.ClearStatusByCookie(this, cookieList, LPCMapping.Empty, clearType);
    }

    /// <summary>
    /// 清除状态
    /// </summary>
    public void ClearStatusByCookie(LPCArray cookieList, int clearType = StatusConst.CLEAR_TYPE_END_SELF)
    {
        StatusMgr.ClearStatusByCookie(this, cookieList, LPCMapping.Empty, clearType);
    }

    /// <summary>
    /// 清除状态
    /// </summary>
    public void ClearStatusByCookie(int cookie, int clearType = StatusConst.CLEAR_TYPE_END_SELF)
    {
        StatusMgr.ClearStatusByCookie(this, cookie, LPCMapping.Empty, clearType);
    }

    /// <summary>
    /// 清除状态
    /// </summary>
    public void ClearStatus(string status, int clearType = StatusConst.CLEAR_TYPE_END_SELF)
    {
        StatusMgr.ClearStatus(this, status, LPCMapping.Empty, clearType);
    }

    /// <summary>
    /// 清除状态
    /// </summary>
    public void ClearStatus(int status, int clearType = StatusConst.CLEAR_TYPE_END_SELF)
    {
        StatusMgr.ClearStatus(this, StatusMgr.GetStatusAlias(status), LPCMapping.Empty, clearType);
    }

    /// <summary>
    /// 清除状态
    /// </summary>
    public void ClearStatus(LPCArray statusArr, LPCMapping extraPara, int clearType = StatusConst.CLEAR_TYPE_END_SELF)
    {
        StatusMgr.ClearStatus(this, statusArr, extraPara, clearType);
    }

    /// <summary>
    /// 获取角色的世界位置信息
    /// </summary>
    public Vector3 GetPosition()
    {
        // 没有actor对象，Vector2.zero
        if (mActor == null)
            return Vector3.zero;

        // 设置实体世界坐标
        return mActor.GetPosition();
    }

    /// <summary>
    /// 设置角色的世界位置信息
    /// </summary>
    public void SetPosition(Vector3 pos)
    {
        // 没有actor对象不处理
        if (mActor == null)
            return;

        // 设置实体世界坐标
        mActor.SetPosition(pos);
    }

    /// <summary>
    /// 设置角色对象的Active状态
    /// </summary>
    public void SetActive(bool flag)
    {
        // 没有actor对象不处理
        if (mActor == null)
            return;

        // 设置对象的Active状态
        mActor.SetActive(flag);
    }

    /// <summary>
    /// 设置角色世界缩放比例
    /// </summary>
    public float GetWorldScale()
    {
        return mWorldScale;
    }

    /// <summary>
    /// 设置角色世界缩放比例
    /// </summary>
    public void SetWorldScale(float scale)
    {
        // 没有actor对象不处理
        if (mActor == null)
            return;

        // 如果世界缩放没有变化
        if (Game.FloatEqual(mWorldScale, scale))
            return;

        // 记录世界缩放
        mWorldScale = scale;

        // 获取角色的模型缩放
        LPCValue modleScale = this.Query<LPCValue>("scale");

        // 没有配置模型缩放，默认是1
        if (modleScale == null)
        {
            mActor.SetScale(scale, scale, scale);
            return;
        }

        // 实际表现缩放是模型缩放*世界缩放
        float newScale;
        switch (modleScale.type)
        {
            case LPCValue.ValueType.FLOAT:

                newScale = modleScale.AsFloat * scale;

                mActor.SetScale(newScale, newScale, newScale);

                break;

            case LPCValue.ValueType.INT:

                newScale = modleScale.AsInt * scale;

                mActor.SetScale(newScale, newScale, newScale);

                break;

            case LPCValue.ValueType.ARRAY:

                if (modleScale.AsArray.Count >= 3)
                    mActor.SetScale(
                        modleScale.AsArray[0].AsFloat * scale,
                        modleScale.AsArray[1].AsFloat * scale,
                        modleScale.AsArray[2].AsFloat * scale);
                else
                    mActor.SetScale(scale, scale, scale);

                break;

            default :
                mActor.SetScale(scale, scale, scale);
                break;
        }
    }

    /// <summary>
    /// 获取角色逻辑朝向
    /// </summary>
    /// <returns>The direction.</returns>
    public int GetDirection()
    {
        return (mActor == null) ? ObjectDirection2D.RIGHT : mActor.GetDirectoion2D();
    }

    /// <summary>
    /// 设置角色逻辑朝向
    /// </summary>
    /// <param name="dir">ObjectDirection2D朝向数据</param>
    public void SetDirection(int dir)
    {
        if (mActor == null)
            return;

        mActor.SetDirection2D(dir);
    }

    /// <summary>
    /// 进入地图
    /// </summary>
    public void EnterCombatMap(string sceneId, Vector3 pos, int dir, int uniqueId)
    {
        // 没有mActor对象
        if (mActor == null)
            return;

        // 设置对象的场景id
        SceneId = sceneId;

        // 记录角色唯一id
        UniqueId = uniqueId;

        // 进入战场前需要取消角色的所有ActionSet
        // 这个地方不能取消action, 目前有怪物进场带有状态光效等action
        // 在其他地方保证进场是action正确
        // mActor.CancelAllActionSet();

        // 设置位置信息
        SetPosition(pos);

        // 逻辑朝向
        SetDirection(dir);

        // 激活角色
        SetActive(true);
    }

    /// <summary>
    /// 离开地图
    /// </summary>
    public void LeaveCombatMap()
    {
        // 没有mActor对象
        if (mActor == null)
            return;

        // 重置UniqueId
        UniqueId = 0;

        // 离开战场前需要取消角色的所有ActionSet
        mActor.CancelAllActionSet();

        // 激活角色
        SetActive(false);
    }

    /// <summary>
    /// 获取角色对象的Active状态
    /// </summary>
    public bool isActive()
    {
        // 没有actor对象不处理
        if (mActor == null)
            return false;

        // 获取角色对象的Active状态
        return mActor.isActive();
    }

    /// <summary>
    /// 设置物件的名字
    /// </summary>
    public void SetName(string name)
    {
        this.name.SetName(name);
    }

    /// <summary>
    /// 取得物件的名字
    /// </summary>
    public string GetName()
    {
        string str = Query<string>("name");
        if (string.IsNullOrEmpty(str))
            return this.name.GetName();
        return str;
    }

    /// <summary>
    /// 取得RID
    /// </summary>
    public string GetRid()
    {
        if (this.rid == null)
            return null;

        return this.rid.GetRid();
    }

    /// <summary>
    /// 取得classID
    /// </summary>
    virtual public int GetClassID()
    {
        return Query<int>("class_id");
    }

    /// <summary>
    /// 取得level
    /// </summary>
    virtual public int GetLevel()
    {
        return Query<int>("level");
    }

    /// <summary>
    /// 取得rank
    /// </summary>
    virtual public int GetRank()
    {
        return Query<int>("rank");
    }

    /// <summary>
    /// 获取对象的稀有度
    /// </summary>
    public int GetRarity()
    {
        // 获取该装备的属性
        LPCMapping prop = this.Query<LPCMapping>("prop");

        // 该道具没有稀有度
        if (prop == null)
            return EquipConst.RARITY_WHITE;

        // 获取装备的附加属性
        LPCArray minorProp = prop.GetValue<LPCArray>(EquipConst.MINOR_PROP);

        // 装备的稀有度就是装备的附加属性条数
        return minorProp == null ? EquipConst.RARITY_WHITE : minorProp.Count;
    }

    /// <summary>
    /// 取得星级
    /// </summary>
    virtual public int GetStar()
    {
        return Query<int>("star");
    }

    /// <summary>
    /// 取得简介
    /// </summary>
    virtual public LPCMapping GetProfile()
    {
        LPCMapping pro = new LPCMapping();
        pro.Add("rid", LPCValue.Create(GetRid()));
        pro.Add("name", LPCValue.Create(GetName()));
        return pro;
    }

    /// <summary>
    /// 取得道具的短描述
    /// </summary>
    public string Short()
    {
        // 获取短描述脚本
        int scriptNo = Query<int>("short_desc_script");

        // 没有描述脚本
        if (scriptNo == 0)
            return LocalizationMgr.Get(Query<string>("name"));

        // 调用脚本计算
        return (string)ScriptMgr.Call(scriptNo, this);
    }

    /// <summary>
    /// 判断是否是玩家
    /// </summary>
    public bool IsUser()
    {
        return objectType == ObjectType.OBJECT_TYPE_USER ? true : false;
    }

    /// <summary>
    /// 判断是否是怪物
    /// </summary>
    public bool IsMonster()
    {
        return objectType == ObjectType.OBJECT_TYPE_MONSTER ? true : false;
    }

    /// <summary>
    /// 判断是否是NPC
    /// </summary>
    public bool IsNpc()
    {
        return objectType == ObjectType.OBJECT_TYPE_NPC ? true : false;
    }

    /// <summary>
    /// 判断是否是道具
    /// </summary>
    public bool IsItem()
    {
        return objectType == ObjectType.OBJECT_TYPE_ITEM ? true : false;
    }

    /// <summary>
    /// Lock the specified cookie.
    /// </summary>
    /// <param name="cookie">Cookie.</param>
    public bool Lock(string cookie, int times)
    {
        // Lock目标
        if (!mLockMap.ContainsKey(cookie))
        {
            mLockMap.Add(cookie, times);
            return true;
        }

        // 角色锁定
        mLockMap[cookie] = mLockMap[cookie] + times;
        return true;
    }

    /// <summary>
    /// Uns the lock.
    /// </summary>
    /// <param name="cookie">Cookie.</param>
    public void UnLock(string cookie, bool isAll = false)
    {
        // 如果不包含cookie，解锁定失败
        if (!mLockMap.ContainsKey(cookie))
            return;

        // 角色解锁定
        if (!isAll)
            mLockMap[cookie] = mLockMap[cookie] - 1;
        else
            mLockMap[cookie] = 0;

        // 判断是否需要移除该cookie锁定信息
        if (mLockMap[cookie] == 0)
            mLockMap.Remove(cookie);

        // 执行解锁后操作
        UNLOCK_END.Call(this);
    }

    /// <summary>
    /// 判断角色是否锁定
    /// </summary>
    public bool IsLocked()
    {
        return (mLockMap.Count > 0);
    }

    /// <summary>
    /// 判断角色是否在回合中
    /// </summary>
    public bool IsRoundIn()
    {
        // 返回角色是否在指定类型回合中
        return (RoundCombatConst.ROUND_TYPE_NONE != RoundType);
    }

    /// <summary>
    /// 根据UniqueId取得物件对象
    /// </summary>
    public static Property FindObjectByUniqueId(int uniqueId)
    {
        // 如果在列表中，返回对象
        if(UniqueIdObs.ContainsKey(uniqueId))
            return UniqueIdObs[uniqueId];

        return null;
    }

    /// <summary>
    /// 获取最终属性列表
    /// </summary>
    /// <returns>The final attrib.</returns>
    public LPCMapping GetFinalAttrib()
    {
        // 获取玩家的最终属性
        LPCMapping finalAttrib = QueryTemp<LPCMapping>("final_attrib");

        // 如果缓存最终属性没有过期直接返回
        if (finalAttrib != null)
        {
            // 添加当前HP,MP值
            finalAttrib.Add("cur_mp", Query<int>("mp"));
            finalAttrib.Add("cur_hp", Query<int>("hp"));
            finalAttrib.Add("max_hp", QueryAttrib("max_hp"));
            return finalAttrib;
        }

        // improvement格式转换
        LPCMapping improvementMap = QueryTemp<LPCMapping>("improvement");
        if (improvementMap == null)
            improvementMap = new LPCMapping();

        // 初始化数据
        finalAttrib = LPCMapping.Empty;
        string attribName;

        // 汇总final_attrib
        for (int i = 0; i < PropertyInitConst.FinalAttribs.Count; i++)
        {
            // 计算属性加值
            attribName = PropertyInitConst.FinalAttribs[i];

            // 记录 query_attrib 的计算结果
            finalAttrib.Add(attribName,
                QueryAttrib(attribName, false) + improvementMap.GetValue<int>(attribName));
        }

        // CopyTo ImprovementAttribList中的所有属性
        improvementMap.CopyTo(PropertyInitConst.ImprovementAttribList, finalAttrib);

        // 获取攻击加值
        finalAttrib.Add("imp_atk", improvementMap.GetValue<int>("attack"));

        // 添加当前HP,MP值
        finalAttrib.Add("cur_mp", Query<int>("mp"));
        finalAttrib.Add("cur_hp", Query<int>("hp"));
        finalAttrib.Add("max_hp", QueryAttrib("max_hp"));

        // 缓存最终属性, 等待下一次刷新属性时再重新刷新
        SetTemp("final_attrib", LPCValue.Create(finalAttrib));

        // 返回finalAttrib
        return finalAttrib;
    }

    #region 替换占有相关操作接口

    /// <summary>
    /// Determines whether this instance is employee.
    /// </summary>
    /// <returns><c>true</c> if this instance is employee; otherwise, <c>false</c>.</returns>
    public bool IsEmployee()
    {
        return (Query<LPCValue>("summoner_rid", true) != null);
    }

    /// <summary>
    /// 占有目标对象（设置一下数据）
    /// </summary>
    /// <param name="occupyOb">Occupy ob.</param>
    public void Occupy(Property occupyOb)
    {
        // 设置占有者
        LPCArray occupiedIds = occupyOb.QueryTemp<LPCArray>("occupied_ids");
        if (occupiedIds == null)
            occupiedIds = new LPCArray(GetRid());
        else
            occupiedIds.Add(GetRid());

        // 重置originalOb被占有列表
        occupyOb.SetTemp("occupied_ids", LPCValue.Create(occupiedIds));

        // 设置实体占有目标id
        SetTemp("occupy_id", LPCValue.Create(occupyOb.GetRid()));
    }

    /// <summary>
    /// 释放占有目标
    /// </summary>
    public void UnOccupy()
    {
        // 获取对象的占有目标id
        string occupyId = QueryTemp<string>("occupy_id");

        // 该对象没有占有任何目标
        if (string.IsNullOrEmpty(occupyId))
            return;

        // 设置占有者
        Property occupiedOb = Rid.FindObjectByRid(occupyId);
        if (occupiedOb != null)
        {
            LPCArray occupiedIds = occupiedOb.QueryTemp<LPCArray>("occupied_ids");
            if (occupiedIds == null)
                occupiedIds = new LPCArray();

            // 移除数据
            occupiedIds.Remove(GetRid());

            // 重置originalOb被占有列表
            occupiedOb.SetTemp("occupied_ids", LPCValue.Create(occupiedIds));
        }

        // 删除占有目标id
        DeleteTemp("occupy_id");
    }

    /// <summary>
    /// 是否是占有者
    /// </summary>
    /// <returns><c>true</c> if is occupier the specified ob; otherwise, <c>false</c>.</returns>
    /// <param name="ob">Ob.</param>
    public bool IsOccupier()
    {
        // 获取角色占有目标id信息
        LPCValue occupyId = QueryTemp("occupy_id");

        // 返回目标是否是占有者
        return (occupyId == null ? false : true);
    }

    /// <summary>
    /// 获取角色占有目标对象
    /// </summary>
    /// <returns><c>true</c> if this instance is occupy target; otherwise, <c>false</c>.</returns>
    public Property GetOccupyTarget()
    {
        // 获取角色占有目标id信息
        string occupyId = QueryTemp<string>("occupy_id");
        if (string.IsNullOrEmpty(occupyId))
            return null;

        // 返回目标是否是占有者
        return Rid.FindObjectByRid(occupyId);
    }

    /// <summary>
    /// 获取占有该角色列表
    /// </summary>
    /// <returns>The occupier list.</returns>
    public List<Property> GetOccupierList()
    {
        // 获取占有该角色列表
        LPCArray occupiedIds = QueryTemp<LPCArray>("occupied_ids");
        if (occupiedIds == null || occupiedIds.Count == 0)
            return new List<Property>();

        List<Property> occupierList = new List<Property>();
        foreach (LPCValue rid in occupiedIds.Values)
        {
            Property ob = Rid.FindObjectByRid(rid.AsString);
            if (ob == null)
                continue;

            // 添加到列表中
            occupierList.Add(ob);
        }

        // 返回占有该角色对象列表
        return occupierList;
    }

    /// <summary>
    /// 是否被占有
    /// </summary>
    /// <returns><c>true</c> if is occupier the specified ob; otherwise, <c>false</c>.</returns>
    /// <param name="ob">Ob.</param>
    public bool IsOccupied()
    {
        // 设置占有者
        LPCArray occupiedIds = QueryTemp<LPCArray>("occupied_ids");
        if (occupiedIds == null || occupiedIds.Count == 0)
            return false;

        // 返回角色被占有中
        return true;
    }

    #endregion

    #region Query相关操作接口

    /// <summary>
    /// 检索数据，检索时不要直接通过dbase属性来检索
    /// </summary>
    public LPCValue Query(string path)
    {
        return Query(path, false);
    }

    public LPCValue Query(string path, bool raw)
    {
        LPCValue v = this.dbase.Query(path);
        if (v != null)
            return v;

        if (raw)
            return null;

        // 从本初对象上去查找
        return BasicQuery(path);
    }

    /// <summary>
    /// 获取Dbase数据
    /// </summary>
    public LPCMapping QueryEntireDbase()
    {
        return this.dbase.QueryEntireDbase();
    }

    public T Query<T>(string path)
    {
        return Query<T>(path, false);
    }

    public T Query<T>(string path, bool raw)
    {
        LPCValue v = this.dbase.Query(path);
        if (v != null)
        {
            // 如果是LPCValue类型
            if (typeof(T) == typeof(LPCValue))
                return (T)(object)v;

            // 返回数据
            return v.As<T>();
        }

        // 不需要本初对象上去查找，返回默认值
        if (raw)
            return default(T);

        // 从本初对象上去查找
        v = BasicQuery(path);
        if (v != null)
        {
            // 如果是LPCValue类型
            if (typeof(T) == typeof(LPCValue))
                return (T)(object)v;

            // 返回配置表信息
            return v.As<T>();
        }

        // 返回默认值
        return default(T);
    }

    /// <summary>
    /// 删除属性
    /// </summary>
    public void Delete(string path)
    {
        this.dbase.Delete(path);
    }

    /// <summary>
    /// 设置属性信息
    /// </summary>
    public void Set(string path, LPCValue v)
    {
        // 普通属性设置
        this.dbase.Set(path, v);
    }

    #endregion

    #region QueryTemp相关操作接口

    /// <summary>
    /// 检索数据，检索时不要直接通过dbase属性来检索
    /// </summary>
    public LPCValue QueryTemp(string path)
    {
        return this.tempdbase.Query(path);
    }

    public T QueryTemp<T>(string path)
    {
        LPCValue v = this.tempdbase.Query(path);
        if (v != null)
        {
            // 如果是LPCValue类型
            if (typeof(T) == typeof(LPCValue))
                return (T)(object)v;

            // 返回数据
            return v.As<T>();
        }

        // 返回默认值
        return default(T);
    }

    /// <summary>
    /// 删除属性
    /// </summary>
    public void DeleteTemp(string path)
    {
        this.tempdbase.Delete(path);
    }

    /// <summary>
    /// 设置属性信息
    /// </summary>
    public void SetTemp(string path, LPCValue v)
    {
        this.tempdbase.Set(path, v);
    }

    #endregion

    /// <summary>
    /// 属性原始数值检索
    /// 只是包含技能，装备，队长光环带来的数值
    /// </summary>
    public int QueryOriginalAttrib(string attrib)
    {
        return this.tempdbase.Query(string.Format("original_attrib/{0}", attrib), 0);
    }

    /// <summary>
    /// 属性检索
    /// </summary>
    public int QueryAttrib(string path)
    {
        return QueryAttrib(path, true);
    }

    /// <summary>
    /// 属性检索
    /// </summary>
    /// <returns>The attrib.</returns>
    /// <param name="path">Path.</param>
    /// <param name="improvement">是否包含improvement数据</param>
    public int QueryAttrib(string path, bool all)
    {
        return attrib.Query(path, all);
    }

    /// <summary>
    /// 判断是否可用支付消耗
    /// </summary>
    public bool CanCostAttrib(LPCMapping data)
    {
#if UNITY_EDITOR
        // 忽视消耗
        if (! AuthClientMgr.IsAuthClient &&
            ME.user.QueryTemp<int>("ignore_cost") == 1)
            return true;
#endif

        return attrib.CanCostAttrib(data);
    }

    /// <summary>
    /// 消耗属性
    /// </summary>
    public bool CostAttrib(LPCMapping data)
    {
        // 消耗属性
        return attrib.CostAttrib(data);
    }

    /// <summary>
    /// 增加属性
    /// </summary>
    public bool AddAttrib(LPCMapping data)
    {
        return attrib.AddAttrib(data);
    }

    /// <summary>
    /// 判断是否可用支付技能开销
    /// </summary>
    public bool CanCastCost(int skillId)
    {
#if UNITY_EDITOR
        // 扣除施放技能的开销（如技力）
        if (! AuthClientMgr.IsAuthClient &&
            ME.user.QueryTemp<int>("ignore_cost") == 1)
            return true;
#endif

        return skill.CanCastCost(skillId);
    }

    /// <summary>
    /// 获得技能等级
    /// </summary>
    public int GetSkillLevel(int skillId)
    {
        return skill.GetLevel(skillId);
    }

    /// <summary>
    /// 获取所有技能
    /// </summary>
    public LPCArray GetAllSkills()
    {
        return skill.GetAllSkills();
    }

    /// <summary>
    /// 删除指定技能
    /// </summary>
    public void DeleteSkill(int skillId)
    {
        skill.Delete(skillId);
    }

    /// <summary>
    /// 取得数量
    /// </summary>
    virtual public int GetAmount()
    {
        LPCValue v = dbase.Query("amount");
        if (v == null || !v.IsInt)
            return 0;

        return v.AsInt;
    }

    /// <summary>
    /// 设置数量
    /// </summary>
    virtual public void SetAmount(int amount)
    {
        dbase.Set("amount", amount);
    }

    /// <summary>
    /// 本初数据检索
    /// </summary>
    virtual public LPCValue BasicQuery(string path)
    {
        for (int i = 0; i < basicAttrib.Count; i++)
        {
            LPCValue ret = QueryRow(basicAttrib[i], path);
            if (ret != null)
                return ret;
        }

        return null;
    }

    /// <summary>
    /// 本初数据检索
    /// 备注 : 该接口不会Duplicate
    /// 调用者需要确定数据不会被修改，否则请使用BasicQuery接口
    /// </summary>
    virtual public T BasicQueryNoDuplicate<T>(string path)
    {
        CsvRow row = null;
        for (int i = 0; i < basicAttrib.Count; i++)
        {
            row = basicAttrib[i];

            // 在行中获取路径path的信息
            if (row.Contains(path))
                return row.Query<T>(path);
            else if (row.Contains("dbase"))
            {
                LPCMapping dbase = row.Query<LPCMapping>("dbase");
                if (dbase == null)
                    return default(T);

                // 获取数据
                return dbase.GetValue<T>(path);
            }
        }

        return default(T);
    }

    /// <summary>
    /// CsvRow查询
    /// </summary>
    static LPCValue QueryRow(CsvRow row, string path)
    {
        LPCValue data = null;

        // 在行中获取路径path的信息
        if (row.Contains(path))
        {
            // 查询数据
            data = row.Query<LPCValue>(path);

            // 没有数据直接返回
            if (data == null)
                return null;

            // dup一份数据共外面使用，防止数据被修改
            return LPCValue.Duplicate(data);
        }
        else if (row.Contains("dbase"))
        {
            LPCValue dbase = row.Query<LPCValue>("dbase");

            // 获取数据
            data = dbase.AsMapping[path];

            // 没有数据直接返回
            if (data == null)
                return null;

            // dup一份数据共外面使用，防止数据被修改
            return LPCValue.Duplicate(data);
        }
        else
        {
            return null;
        }
    }
}
