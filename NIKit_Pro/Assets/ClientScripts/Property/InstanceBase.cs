/// <summary>
/// InstanceBase.cs
/// Create by zhaozy 2014-11-12
/// 副本对象基类
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
public abstract class InstanceBase : Property
{
    #region 变量

    // 副本阶段
    private int mCurState = -1;

    // 副本资源列表
    private Dictionary<string, Property> mResourceMap = new Dictionary<string, Property>();

    // 副本中的攻方宠物列表
    private List<Property> mFighterList = new List<Property>();

    // 副本中的攻方宠物列表
    private List<Property> mCombatPropertyList = new List<Property>();

    // 副本攻方成员信息
    private LPCMapping mFighterMap = LPCMapping.Empty;

    // 防御者信息
    private LPCMapping mDefenders = LPCMapping.Empty;

    // 副本关卡操作详细信息
    private LPCMapping mLevelActions = LPCMapping.Empty;

    // 副本结束标识
    private bool mIsEnd = false;
    private int mEndTick = 0;

    /// <summary>
    /// 记录资源
    /// </summary>
    private List<KeyValuePair<string, bool>> mResList = new List<KeyValuePair<string, bool>>();

    #endregion

    #region 属性

    /// <summary>
    /// 副本id
    /// </summary>
    public string InstanceId { get; set; }

    // 副本当前关卡
    public int Level { get; set; }

    /// <summary>
    /// 副本当前阶段
    /// </summary>
    public int CurState
    {
        get { return mCurState; }
        set { mCurState = value; }
    }

    /// <summary>
    /// 副本结束标识
    /// </summary>
    public bool IsEnd
    {
        get { return mIsEnd; }
        set
        {
            mIsEnd = value;

            // 如果是验证客户端
            if (AuthClientMgr.IsAuthClient)
                return;

            // 如果是已经结束，则记录结束时间
            if (mIsEnd)
                mEndTick = TimeMgr.CombatTick;
        }
    }

    /// <summary>
    /// 副本进度暂停
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// 副本资源列表
    /// </summary>
    public Dictionary<string, Property> ResourceMap
    {
        get { return mResourceMap; }
    }

    /// <summary>
    /// 副本是否已经开始
    /// </summary>
    public bool IsStarted { get; set; }

    /// <summary>
    /// 是否验证结束
    /// </summary>
    public bool IsAuthEnd { get; set; }

    /// <summary>
    /// 副本中的攻方宠物列表
    /// </summary>
    public List<Property> FighterList
    {
        get { return mFighterList; }
        set { mFighterList = value; }
    }

    /// <summary>
    /// 副本中当前战斗宠物列表
    /// </summary>
    public List<Property> CombatPropertyList
    {
        get{ return mCombatPropertyList; }
        set{ mCombatPropertyList = value; }
    }

    /// <summary>
    /// 副本的复活次数
    /// </summary>
    public int ReviveTimes { get; set; }

    /// <summary>
    /// 副本回合数
    /// </summary>
    public int RoundCount { get; set; }

    /// <summary>
    /// 副本攻方成员信息
    /// </summary>
    public LPCMapping FighterMap
    {
        get{ return mFighterMap; }
        set{ mFighterMap = value; }
    }

    /// <summary>
    /// 防御者信息
    /// </summary>
    public LPCMapping Defenders
    {
        get{ return mDefenders; }
        set{ mDefenders = value; }
    }

    /// <summary>
    /// 副本关卡操作详细信息
    /// </summary>
    public LPCMapping LevelActions
    {
        get{ return mLevelActions; }
        set{ mLevelActions = value; }
    }

    /// <summary>
    /// 副本开始时间
    /// </summary>
    public int StartTick { get; set; }

    /// <summary>
    /// 上一次暂停tick
    /// </summary>
    public int LastPauseTick { get; set; }

    /// <summary>
    /// 人员回合次数
    /// </summary>
    public int ActorCrossTimes { get; set; }

    /// <summary>
    /// 循环标识
    /// </summary>
    public bool IsLoopFight { get; set; }

    /// <summary>
    /// 记录资源
    /// </summary>
    public List<KeyValuePair<string, bool>> ResList
    {
        get{ return mResList; }
        set{ mResList = value; }
    }

    #endregion

    #region abstract接口

    /// <summary>
    /// 派生接口
    /// </summary>
    public abstract void DoEnd();

    #endregion

    #region 公共接口

    /// <summary>
    /// 构造函数
    /// </summary>
    public InstanceBase(LPCMapping data) : base(data)
    {
        // 设置类别为OBJECT_TYPE_INSTANCE
        this.objectType = ObjectType.OBJECT_TYPE_INSTANCE;

        // 如果指明了dbase数据，需要吸收进来
        if (data != null && data["dbase"] != null && data["dbase"].IsMapping)
            this.dbase.Absorb(data["dbase"].AsMapping);

        // 获取副本id
        InstanceId = this.Query("instance_id").AsString;

        // 副本攻方成员信息
        mFighterMap = this.Query("fighter_map").AsMapping;

        // 防御者信息
        mDefenders = this.Query("defenders").AsMapping;

        // 获取副本通关操作详细信息
        LevelActions = this.Query("level_actions").AsMapping;
    }

    /// <summary>
    /// 暂停副本（只是暂停副本的时间进度，不是真正暂停副本进度）
    /// </summary>
    public void DoPause()
    {
        // 副本还没有开始
        if (!IsStarted)
            return;

        // 如果副本已经暂停，不处理
        if (IsPaused)
            return;

        // 标识副本已经
        IsPaused = true;

        // 记录副本暂停时间
        LastPauseTick = TimeMgr.CombatTick;
    }

    /// <summary>
    /// 继续副本
    /// </summary>
    public void DoContinue()
    {
        // 副本还没有开始
        if (!IsStarted)
            return;

        // 副本不在暂停中，不处理
        if (!IsPaused)
            return;

        // 标识副本已经
        IsPaused = false;

        // 修正副本开始时间
        StartTick = StartTick + (TimeMgr.CombatTick - LastPauseTick);

        // 重置副本暂停时间
        LastPauseTick = 0;
    }

    /// <summary>
    /// 获取副本进度时间
    /// </summary>
    public int GetProgressTick()
    {
        // 如果副本还没有开始
        if (!IsStarted)
            return 0;

        // 如果是已经结束
        if (IsEnd)
            return mEndTick - StartTick;

        // 当前没有处于暂停中
        if (!IsPaused)
            return TimeMgr.CombatTick - StartTick;

        // 处于暂停中
        return LastPauseTick - StartTick;
    }

    /// <summary>
    /// 是否是副本对象
    /// </summary>
    public bool IsInstance()
    {
        return true;
    }

    /// <summary>
    /// 添加缓存资源列表
    /// </summary>
    public void AddResource(string rid, Property resource)
    {
        // 添加到缓存列表中
        ResourceMap.Add(rid, resource);
    }

    /// <summary>
    /// start接口（需要各个子类重载）
    /// </summary>
    public virtual void DoStart()
    {
        // 开始战斗
        CombatMgr.EnterCombat();

        // 重置全局的随机
        int randomSeed = Query<int>("random_seed");
        RandomMgr.RestRandomSeed(randomSeed);
    }

    /// <summary>
    /// 获取副本当前阶段需要处理事件列表
    /// </summary>
    public LPCArray GetEvents()
    {
        // 获取副本当前阶段事件列表
        LPCArray events = InstanceMgr.GetInstanceEvents(InstanceId, CurState);
        if (events == null)
            return LPCArray.Empty;

        // 返回数据
        return events;
    }

    /// <summary>
    /// 执行副本失败
    /// </summary>
    public abstract void DoInstanceFail(Property ob);

    /// <summary>
    /// Adds the round count.
    /// </summary>
    /// <param name="times">Times.</param>
    public void AddRoundCount(int times)
    {
        // 增加回合数
        RoundCount += times;

        // 检查回合数是否超限
        int maxRoundCount = GameSettingMgr.GetSettingInt("max_combat_rounds");
        if (maxRoundCount >= RoundCount)
            return;

        // 结束战斗
        RoundCombatMgr.EndCombat(RoundCombatConst.END_TYPE_MAX_ROUNDS);
    }

    /// <summary>
    /// Adds the level action.
    /// </summary>
    /// <param name="action">Action.</param>
    public void AddLevelAction(LPCArray action)
    {
        // 初始化数据
        if (LevelActions == null)
            LevelActions = LPCMapping.Empty;

        // 如果不包含Level数据添加数据
        if (!LevelActions.ContainsKey(Level))
        {
            LevelActions.Add(Level, new LPCArray(action));
            return;
        }

        // 记录当前关卡详细操作列表
        LevelActions[Level].AsArray.Add(action);
    }

    /// <summary>
    /// 获取操作
    /// </summary>
    public LPCArray GetAction()
    {
        // 初始化数据
        if (LevelActions == null)
            return null;

        // 该关卡没有Action
        if (! LevelActions.ContainsKey(Level))
            return null;

        // 获取该Level的详细操作列表
        LPCArray actions = LevelActions[Level].AsArray;

        // 没有行动回合数据
        if (actions == null || actions.Count == 0)
            return null;

        // 获取第一个元素详细信息
        LPCArray action = actions[0].AsArray;

        // 移除第一个元素
        actions.RemoveAt(0);

        // 返回行动详细数据
        return action;
    }

    #endregion
}
