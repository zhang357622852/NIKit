/// <summary>
/// RoundCombat.cs
/// create by zhaozy 2016-05-05
/// 回合制对象
/// </summary>

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine.SceneManagement;
using LPC;

/// <summary>
///  回合制对象
/// </summary>
public class RoundCombat
{
    #region 变量

    /// <summary>
    /// 战斗对象列表
    /// </summary>
    private List<Property> mCombatPropertyList = new List<Property>();

    /// <summary>
    /// 当前回合数据
    /// </summary>
    private int mRoundTimes = 0;

    /// <summary>
    /// 当前回合cookie
    /// </summary>
    private string mRoundCookie = string.Empty;

    /// <summary>
    /// 行动顺序列表
    /// </summary>
    private List<Property> mActionOrderList = new List<Property>();

    /// <summary>
    /// 回合列表
    /// </summary>
    private Dictionary<int, List<CombatData>> mRoundList = new Dictionary<int, List<CombatData>>();

    /// <summary>
    /// 行动列表
    /// </summary>
    private Dictionary<string, Dictionary<string, LPCMapping>> mActionMap =
        new Dictionary<string, Dictionary<string, LPCMapping>>();

    /// <summary>
    /// 异步行动列表
    /// </summary>
    private Dictionary<string, LPCMapping> mAsynActionMap = new Dictionary<string, LPCMapping>();

    /// <summary>
    /// 标识玩家顺序列表是否已经变化
    /// </summary>
    private bool mIsDirty = false;

    // 失败列表
    private Dictionary<int, int> mFailMap = new Dictionary<int, int>();

    #endregion

    #region 属性接口

    /// <summary>
    /// 运行标识
    /// </summary>
    public bool IsRunningFlag { get; set; }

    /// <summary>
    /// 回合制战斗类型
    /// </summary>
    public int RoundType { get; private set; }

    /// <summary>
    /// 回合制战斗暂停标识
    /// </summary>
    public bool IsPause { get; private set; }

    /// <summary>
    /// 回合制战斗owner
    /// </summary>
    public string OwnerRid { get; private set; }

    /// <summary>
    /// Cookie属性
    /// </summary>
    public string Cookie { get; private set; }

    /// <summary>
    /// 获取当前回合对象
    /// </summary>
    public Property CurRoundOb { get; private set; }

    #endregion

    #region 公共接口

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="name">角色名.</param>
    /// <param name="prefeb">预设资源路径.</param>
    public RoundCombat(string ownerRid, int type)
    {
        // 回合制战斗owner
        OwnerRid = ownerRid;

        // 记录战斗类型
        RoundType = type;

        // 生成Cookie
        Cookie = Rid.New();
    }

    /// <summary>
    /// 恢复回合制战斗
    /// </summary>
    public bool Continue()
    {
        // 取消暂停运行标识
        IsPause = false;

        // 返回结束成功
        return true;
    }

    /// <summary>
    /// 结束回合制战斗
    /// </summary>
    public bool Pause()
    {
        // 标识已经暂停运行
        IsPause = true;

        // 返回结束成功
        return true;
    }

    /// <summary>
    /// 结束回合制战斗
    /// </summary>
    public bool End(bool isForce)
    {
        // 停止相应Coroutine
        Coroutine.StopCoroutine(Cookie);

        // 清除数据
        mActionOrderList.Clear();
        mRoundList.Clear();
        mActionMap.Clear();
        mAsynActionMap.Clear();

        // 标识已经停止运行
        IsRunningFlag = false;

        // 返回结束成功
        return true;
    }

    /// <summary>
    /// 回合制战斗开始
    /// </summary>
    public void Start(List<Property> propertyList, float delayTime)
    {
        // 记录数据
        mCombatPropertyList = propertyList;

        // 重置所有数据
        mActionOrderList.Clear();
        mRoundList.Clear();
        mActionMap.Clear();
        mAsynActionMap.Clear();

        // 重置暂停标识
        IsPause = false;

        // 策划需求：需要副本开场动画播放完毕后在开始刷怪
        // 暂时做成固定固定延迟一段时间再开始副本
        // 正确做法是应该暂停副本进度（由于涉及较多临时处理）
        // 实际上需要暂停副本进度，玩家心跳，怪物心跳（策略），战斗系统等
        Coroutine.DispatchService(_DoStart(delayTime), Cookie, true);
    }

    /// <summary>
    /// 驱动回合制战斗
    /// </summary>
    public void Update()
    {
        // 战斗还没有开始, 战斗已经暂停不处理
        // 行动列表不为空不能执行，还有角色的回合行动没有结束
        if (! IsRunningFlag || IsPause ||
            mActionMap.Count != 0 || mAsynActionMap.Count != 0)
            return;

        // 执行连击回合
        // 连击回合列表中只能一个一个处理
        if (DoTypeRound(RoundCombatConst.ROUND_TYPE_COMBO, false))
            return;

        // 执行反击回合
        // 反击回合列表中全部反击一次全部处理掉
        if (DoTypeRound(RoundCombatConst.ROUND_TYPE_COUNTER, true))
            return;

        // 执行暴走回合
        // 暴走回合列表中全部暴走一次全部处理掉
        if (DoTypeRound(RoundCombatConst.ROUND_TYPE_RAMPAGE, true))
            return;

        // 执行插队回合
        // 插队回合列表中只能一个一个处理
        if (DoTypeRound(RoundCombatConst.ROUND_TYPE_GASSER, false))
            return;

        // 执行等待回合（普通回合，追加回合）
        // 等待回合列表中只能一个一个处理
        if (DoTypeRound(RoundCombatConst.ROUND_TYPE_WAIT, false))
            return;

        // 抽取一个需要执行正常回合的角色数据
        // 正常回个包含追加回合和普通回合
        CombatData data = FetchCombatData();

        // 执行回合前处理
        DoRoundStart(data);

        // 添加到ROUND_TYPE_WAIT回合中
        AddWaitRound(data);
    }

    /// <summary>
    /// DoRoundEnd
    /// </summary>
    public bool DoRoundActionEnd(string cookie, LPCMapping para)
    {
        // 获取角色rid
        string rid = para.GetValue<string>("rid");

        // 如果对象已经不存在了
        Property ob = Rid.FindObjectByRid(rid);
        if (ob == null)
        {
            // 删除数据
            mActionMap.Remove(rid);
            return true;
        }

        // 没有该角色回合信息
        if (! mActionMap.ContainsKey(rid))
            return true;

        // 获取绑定的对象
        mActionMap[rid].Remove(cookie);

        // mActionMap回合数据为空直接移除数据
        if (mActionMap[rid].Count == 0)
            mActionMap.Remove(rid);

        // 执行角色正常回合
        if (CheckStarActionEnd(ob) && mAsynActionMap.ContainsKey(rid))
        {
            // 获取详细数据
            LPCMapping data = mAsynActionMap[rid];
            string roundCookie = data.GetValue<string>("cookie");

            // 添加行动回合列表
            AddRoundAction(roundCookie, ob, data);

            // 移除数据
            mAsynActionMap.Remove(rid);

            // 判断玩家是否已经没有其他的回合
            // 判断是否有回合前行动还没结束
            Coroutine.DispatchService(AsncRound(ob, roundCookie, data.GetValue<int>("type"), data), true);

            // 执行Asyn回合
            return true;
        }

        // 角色当前所有行动回合是否已经结束
        if (! mActionMap.ContainsKey(rid) || mActionMap[rid].Count == 0)
        {
            // 正常回合才需要走ROUND_ACTION_END和触发属性
            if (ob.RoundType != RoundCombatConst.ROUND_TYPE_NONE)
            {
                // 如果没有可以操作的回合了删除cur_rounds
                if (! HasOperationRound(ob))
                    ob.DeleteTemp("cur_rounds");

                // 抛出回合结束事件
                LPCMapping eventPara = LPCMapping.Empty;
                eventPara.Append(para);
                eventPara.Add("rid", ob.GetRid());
                eventPara.Add("type", ob.RoundType);
                eventPara.Add("round_cookie", ob.RoundCookie);

                // 抛出事件
                EventMgr.FireEvent(EventMgrEventType.EVENT_ROUND_END, MixedValue.NewMixedValue<LPCMapping>(eventPara), true, true);

                // 调用回合结束，共策划使用
                ROUND_ACTION_END.Call(ob, ob.RoundType, ob.RoundCookie, para, true);

                // 重置RoundType
                ob.RoundType = RoundCombatConst.ROUND_TYPE_NONE;
                ob.RoundCookie = string.Empty;
            }
        }

        // 判断是否需要结束战斗
        TryEndRoundCombat(ob);

        // 返回添加追加回合成功
        return true;
    }

    /// <summary>
    /// 战斗角色速度发生变化
    /// </summary>
    public void OnAttackOrderUpdate(Property ob)
    {
        // 顺序列表中已经没有该角色
        if (mActionOrderList.IndexOf(ob) == -1)
            return;

        // 标识角色攻击顺序已经mis_dirty
        ob.Set("is_dirty", LPCValue.Create(1));

        // 标识数据已经脏了
        mIsDirty = true;
    }

    /// <summary>
    /// 角色当前回合是否还有效
    /// </summary>
    public bool CurRoundIsValid(Property ob)
    {
        // 顺序列表中已经没有该角色
        if (mActionOrderList.IndexOf(ob) == -1)
            return false;

        // 角色当前大回合还没有执行
        return true;
    }

    /// <summary>
    /// 添加行动
    /// </summary>
    public bool AddRoundAction(string cookie, Property ob, LPCMapping para)
    {
        // 不归该对象管理
        if (mCombatPropertyList.IndexOf(ob) == -1)
            return false;

        // 重复的cookie
        string rid = ob.GetRid();
        if (! mActionMap.ContainsKey(rid))
            mActionMap.Add(rid, new Dictionary<string, LPCMapping>());

        // 数据重复
        if (mActionMap[rid].ContainsKey(cookie))
            return false;

        // 添加数据
        mActionMap[rid].Add(cookie, para);

        // 返回添加追加回合成功
        return true;
    }

    /// <summary>
    /// 追加回合
    /// </summary>
    public bool AddAdditionalRound(Property ob, LPCMapping para)
    {
        // 列表中已经没有该角色
        if (mCombatPropertyList.IndexOf(ob) == -1)
            return false;

        // 记录玩家当前回合信息
        ob.SetTemp("cur_rounds", LPCValue.Create(mRoundTimes));

        // 记录数据
        if (!mRoundList.ContainsKey(RoundCombatConst.ROUND_TYPE_ADDITIONAL))
        {
            // 初始化数据
            mRoundList.Add(RoundCombatConst.ROUND_TYPE_ADDITIONAL,
                new List<CombatData>() { new CombatData(ob, RoundCombatConst.ROUND_TYPE_ADDITIONAL, para) });
        }
        else
        {
            // 添加数据
            mRoundList[RoundCombatConst.ROUND_TYPE_ADDITIONAL].Add(
                new CombatData(ob, RoundCombatConst.ROUND_TYPE_ADDITIONAL, para));
        }

        // 返回添加追加回合成功
        return true;
    }

    /// <summary>
    /// 插队回合
    /// </summary>
    public bool AddGasserRound(Property ob, LPCMapping para)
    {
        // 列表中已经没有该角色
        if (mCombatPropertyList.IndexOf(ob) == -1)
            return false;

        // 记录数据
        if (! mRoundList.ContainsKey(RoundCombatConst.ROUND_TYPE_GASSER))
        {
            // 初始化数据
            mRoundList.Add(RoundCombatConst.ROUND_TYPE_GASSER,
                new List<CombatData>() { new CombatData(ob, RoundCombatConst.ROUND_TYPE_GASSER, para) });
        }
        else
        {
            // 添加数据
            mRoundList[RoundCombatConst.ROUND_TYPE_GASSER].Add(
                new CombatData(ob, RoundCombatConst.ROUND_TYPE_GASSER, para));
        }

        // 返回添加回合成功
        return true;
    }

    /// <summary>
    /// 执行反击回合
    /// </summary>
    public bool AddCounterRound(Property ob, LPCMapping para)
    {
        // 不归该对象管理
        if (mCombatPropertyList.IndexOf(ob) == -1)
            return false;

        // 记录数据
        if (! mRoundList.ContainsKey(RoundCombatConst.ROUND_TYPE_COUNTER))
        {
            // 初始化数据
            mRoundList.Add(RoundCombatConst.ROUND_TYPE_COUNTER,
                new List<CombatData>() { new CombatData(ob, RoundCombatConst.ROUND_TYPE_COUNTER, para) });
        }
        else
        {
            // 如果角色已经在反击中不处理
            foreach (CombatData mks in mRoundList[RoundCombatConst.ROUND_TYPE_COUNTER])
            {
                // 如果已经在反击列表中，不能重复添加反击
                if (mks.CombatOb == ob)
                    return false;
            }

            // 添加数据
            mRoundList[RoundCombatConst.ROUND_TYPE_COUNTER].Add(
                new CombatData(ob, RoundCombatConst.ROUND_TYPE_COUNTER, para));
        }

        // 返回添加反击回合成功
        return true;
    }

    /// <summary>
    /// 添加暴走回合
    /// </summary>
    public bool AddRampageRound(Property ob, LPCMapping para)
    {
        // 不归该对象管理
        if (mCombatPropertyList.IndexOf(ob) == -1)
            return false;

        // 记录数据
        if (! mRoundList.ContainsKey(RoundCombatConst.ROUND_TYPE_RAMPAGE))
        {
            // 初始化数据
            mRoundList.Add(RoundCombatConst.ROUND_TYPE_RAMPAGE,
                new List<CombatData>() { new CombatData(ob, RoundCombatConst.ROUND_TYPE_RAMPAGE, para) });
        }
        else
        {
            // 如果角色已经在暴走回合中不处理
            foreach (CombatData mks in mRoundList[RoundCombatConst.ROUND_TYPE_RAMPAGE])
            {
                // 如果已经在暴走回合列表中，不能重复添加暴走回合
                if (mks.CombatOb == ob)
                    return false;
            }

            // 添加数据
            mRoundList[RoundCombatConst.ROUND_TYPE_RAMPAGE].Add(
                new CombatData(ob, RoundCombatConst.ROUND_TYPE_RAMPAGE, para));
        }

        // 返回添加暴走回合成功
        return true;
    }

    /// <summary>
    /// 添加连击回合
    /// </summary>
    public bool AddComboRound(Property ob, LPCMapping para)
    {
        // 不归该对象管理
        if (mCombatPropertyList.IndexOf(ob) == -1)
            return false;

        // 记录数据
        if (! mRoundList.ContainsKey(RoundCombatConst.ROUND_TYPE_COMBO))
        {
            // 初始化数据
            mRoundList.Add(RoundCombatConst.ROUND_TYPE_COMBO,
                new List<CombatData>() { new CombatData(ob, RoundCombatConst.ROUND_TYPE_COMBO, para) });
        }
        else
        {
            // 添加数据
            mRoundList[RoundCombatConst.ROUND_TYPE_COMBO].Add(
                new CombatData(ob, RoundCombatConst.ROUND_TYPE_COMBO, para));
        }

        // 返回添加追加回合成功
        return true;
    }

    /// <summary>
    /// 执行协同回合
    /// </summary>
    public bool AddJointRound(Property ob, LPCMapping para)
    {
        // 不归该对象管理
        if (mCombatPropertyList.IndexOf(ob) == -1)
            return false;

        // 如果角色已经协同回合中
        if (ob.RoundType == RoundCombatConst.ROUND_TYPE_JOINT)
            return false;

        // 执行协同回合
        DoRound(ob, RoundCombatConst.ROUND_TYPE_JOINT, para);

        // 返回添加追加回合成功
        return true;
    }

    /// <summary>
    /// 添加回合找都对象
    /// </summary>
    /// <param name="ob">Ob.</param>
    public void AddCombatEntity(Property ob)
    {
        // 已经在列表中不能重复添加
        if (mCombatPropertyList.IndexOf(ob) != -1)
            return;

        // 添加到战场中显示血条
        ob.Actor.ShowHp(true);

        // 添加到mCombatPropertyList数据
        // 新添加的角色需要等到当前大回合结束后才能够参与战斗
        mCombatPropertyList.Add(ob);
    }

    /// <summary>
    /// 移除回合找都对象
    /// </summary>
    /// <param name="ob">Ob.</param>
    public void RemoveCombatEntity(Property ob)
    {
        // 从列表中删除数据
        mCombatPropertyList.Remove(ob);
    }

    /// <summary>
    /// 获取回合次数
    /// </summary>
    /// <returns>The round times.</returns>
    public int GetRounds()
    {
        return mRoundTimes;
    }

    /// <summary>
    /// 获取战场上的战斗对象列表
    /// </summary>
    /// <returns>The property list.</returns>
    public List<Property> GetPropertyList()
    {
        return mCombatPropertyList;
    }

    /// <summary>
    /// 判断角色是否在当前回合中已经行动
    /// </summary>
    public bool IsRoundDone(Property ob)
    {
        // 获取角色是否在心动序列中
        return (mActionOrderList.IndexOf(ob) == -1);
    }

    /// <summary>
    /// 获取战场上的战斗对象列表
    /// </summary>
    public List<Property> GetPropertyList(int campId)
    {
        // 获取所有的战斗对象列表
        List<Property> AllPropertyList = GetPropertyList();

        List<Property> propertyList = new List<Property>();
        Property ob;

        // 遍历数据
        for (int i = 0; i < AllPropertyList.Count; i++)
        {
            ob = AllPropertyList[i];
            if (ob == null)
                continue;

            // 不是指定的阵营
            if (ob.CampId != campId)
                continue;

            // 添加列表
            propertyList.Add(ob);
        }

        // 返回
        return propertyList;
    }

    /// <summary>
    /// 放弃战斗
    /// </summary>
    public void EndCombat(int type)
    {
        // 构建事件参数
        LPCMapping ePara = new LPCMapping();
        ePara.Add("round_type", RoundType);
        ePara.Add("camp_id", CampConst.CAMP_TYPE_DEFENCE);
        ePara.Add("end_type", type);

        // 遍历全部对象附加无敌状态
        foreach(Property ob in mCombatPropertyList)
            ob.ApplyStatus("COMBAT_END", LPCMapping.Empty);

        // 抛出胜利事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_ROUND_COMBAT_END, MixedValue.NewMixedValue<LPCMapping>(ePara), false, true);

        // 结束战斗
        RoundCombatMgr.EndRoundCombat(false);
    }

    #endregion

    #region 内部接口


    /// <summary>
    /// 执行回合前处理
    /// </summary>
    /// <param name="data">Data.</param>
    private void DoRoundStart(CombatData data)
    {
        // 没有战斗数据
        // 角色对象不存在
        if (data == null ||
            data.CombatOb == null)
            return;

        // 抛出回合结束事件
        LPCMapping eventPara = LPCMapping.Empty;
        eventPara.Add("rid", data.CombatOb.GetRid());
        eventPara.Add("type", data.RoundType);

        // 抛出EVENT_ROUND_START事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_ROUND_START,
            MixedValue.NewMixedValue<LPCMapping>(eventPara), true, true);
    }

    /// <summary>
    /// Fetchs the combat data.
    /// </summary>
    /// <returns>The combat data.</returns>
    private CombatData FetchCombatData()
    {
        // 优先抽取追加回合
        List<CombatData> additionalRoundList;
        if (mRoundList.TryGetValue(RoundCombatConst.ROUND_TYPE_ADDITIONAL, out additionalRoundList) &&
            additionalRoundList.Count != 0)
        {
            // 提取第一个元素
            CombatData data = additionalRoundList[0];
            additionalRoundList.RemoveAt(0);
            return data;
        }

        // 在抽取普通回合
        // 获取普通回合角色对象
        Property roundOb = GetRoundProperty();

        // 构建战斗数据
        return new CombatData(roundOb, RoundCombatConst.ROUND_TYPE_NORMAL, LPCMapping.Empty);
    }

    /// <summary>
    /// 添加等待回合
    /// </summary>
    private bool AddWaitRound(CombatData data)
    {
        // 记录数据
        if (! mRoundList.ContainsKey(RoundCombatConst.ROUND_TYPE_WAIT))
        {
            // 初始化数据
            mRoundList.Add(RoundCombatConst.ROUND_TYPE_WAIT,
                new List<CombatData>() { data });
        }
        else
        {
            // 添加数据
            mRoundList[RoundCombatConst.ROUND_TYPE_WAIT].Add(data);
        }

        // 返回添加等待成功
        return true;
    }

    /// <summary>
    /// 添加行动
    /// </summary>
    private bool AddAsynRoundAction(Property ob, LPCMapping para)
    {
        // 获取角色rid
        string rid = ob.GetRid();

        // 如果列表中已经包含该数据直接覆盖
        if (mAsynActionMap.ContainsKey(rid))
        {
            mAsynActionMap[rid] = para;
            return true;
        }

        // 添加数据
        mAsynActionMap.Add(rid, para);

        // 返回添加追加回合成功
        return true;
    }

    /// <summary>
    /// 检测开始回合行动是否结束
    /// </summary>
    private bool CheckStarActionEnd(Property ob)
    {
        // 获取对象的rid
        string rid = ob.GetRid();

        // 没有数据
        if (! mActionMap.ContainsKey(rid))
            return true;

        // 获取回合类型
        Dictionary<string, LPCMapping> actionMap = mActionMap[rid];

        // 遍历行动回合数据
        foreach (string cookie in actionMap.Keys)
        {
            // 如果不是释放技能的行动不处理
            LPCMapping data = actionMap[cookie];
            if (! data.ContainsKey("skill_id"))
                continue;

            // 如果还有非回合行动
            return false;
        }

        // 返回true
        return true;
    }

    /// <summary>
    /// 目标执行回合
    /// </summary>
    private void DoRound(Property ob, int type, LPCMapping para)
    {
        // 角色对象不存在或者正在析构过程中
        if (ob == null || ob.IsDestroyed)
            return;

        // 记录当前回合角色
        if (RoundCombatConst.ROUND_TYPE_NORMAL == type ||
            RoundCombatConst.ROUND_TYPE_ADDITIONAL == type)
            CurRoundOb = ob;

        // 抛出回合开始事件
        LPCMapping triggerPara = LPCMapping.Empty;
        triggerPara.Add("type", type);
        triggerPara.Add("provoke_id", (ob.CheckStatus("D_PROVOKE") == true) ? 1 : 0);

        // 执行debuff之前的触发属性
        TriggerPropMgr.DoDebuffBeforeTrigger(ob, triggerPara);

        // 获取回合cookie
        string roundCookie = Game.NewCookie(ob.GetRid());

        // 执行回合开始操作
        ROUND_ACTION_STAR.Call(ob, type, roundCookie);

        // 判断该对象是否可以执行回合
        if (! RoundCombatMgr.CheckCanDoRound(ob, type))
        {
            // 如果没有可以操作的回合了删除cur_rounds
            if (! HasOperationRound(ob))
                ob.DeleteTemp("cur_rounds");

            // 抛出回合结束事件
            LPCMapping eventPara = LPCMapping.Empty;
            eventPara.Append(triggerPara);
            eventPara.Add("rid", ob.GetRid());
            eventPara.Add("round_cookie", roundCookie);

            // 抛出事件
            EventMgr.FireEvent(EventMgrEventType.EVENT_ROUND_END, MixedValue.NewMixedValue<LPCMapping>(eventPara), true, true);

            // 执行回合结束操作
            ROUND_ACTION_END.Call(ob, type, roundCookie, LPCMapping.Empty, false);

            // 判断是否需要结束战斗
            TryEndRoundCombat(ob);

            return;
        }

        // 执行debuff之后的触发属性
        TriggerPropMgr.DoDebuffAfterTrigger(ob, triggerPara);

        // 设置RoundType标识
        ob.RoundType = type;
        ob.RoundCookie = roundCookie;

        // 记录行动回合信息
        LPCMapping data = LPCMapping.Empty;
        data.Add("rid", ob.GetRid());
        data.Add("type", type);
        data.Add("cookie", roundCookie);
        data.Add("tick", TimeMgr.RealTick);
        data.Add("rounds", GetRounds());
        data.Append(para);

        // 判断是否有回合前行动还没结束
        if (! CheckStarActionEnd(ob))
        {
            // 添加延迟执行回合
            AddAsynRoundAction(ob, data);

            // 判断是否需要结束战斗
            TryEndRoundCombat(ob);

            return;
        }

        // 添加回合数据
        AddRoundAction(roundCookie, ob, data);

        // 执行回合
        Coroutine.DispatchService(AsncRound(ob, roundCookie, type, data), true);
    }

    /// <summary>
    /// 异步战斗回合协程
    /// </summary>
    /// <returns>The daemon.</returns>
    private IEnumerator AsncRound(Property ob, string cookie, int type, LPCMapping extraPara)
    {
        // 如果不是协同回合延迟0.1秒执行
        if (type != RoundCombatConst.ROUND_TYPE_JOINT)
            yield return TimeMgr.WaitForCombatSeconds(0.1f);

        // 判断该对象是否可以执行回合
        if (! RoundCombatMgr.CheckCanDoRound(ob, type))
        {
            // 执行回合结束
            DoRoundActionEnd(cookie, extraPara);
            yield break;
        }

        // 异步战斗回合
        DoAsncRound(ob, cookie, type, extraPara);
    }

    /// <summary>
    /// 判断是否需要自动战斗
    /// </summary>
    private bool IsAutoCombat(Property ob, int type)
    {
        // 如果是验证客户端必定自动战斗
        if (AuthClientMgr.IsAuthClient ||
            ob.Query<LPCValue>("is_video_actor") != null)
            return true;

        // 反击回合 协同回合 暴走回合 连击回合
        if (type == RoundCombatConst.ROUND_TYPE_COUNTER ||
            type == RoundCombatConst.ROUND_TYPE_JOINT ||
            type == RoundCombatConst.ROUND_TYPE_RAMPAGE ||
            type == RoundCombatConst.ROUND_TYPE_COMBO)
            return true;

        // 如果是状态"混乱","挑衅",只能自动攻击
        if (ob.CheckStatus("D_CHAOS") ||
            ob.CheckStatus("D_PROVOKE"))
            return true;

        // 抛出公式共策划判断
        if (IS_AUTO_COMBAT.Call(ob, type))
            return true;

        // 判断是否需要自动战斗，如果是攻方则需要判断玩家是否主动开启了自动战斗
        // 防守方默认是自动战斗
        if (ob.CampId == CampConst.CAMP_TYPE_DEFENCE)
        {
#if UNITY_EDITOR
            return ME.user.QueryTemp<int>("control_enemy") == 0 ? true : false;
#else
            return true;
#endif
        }
        else
            return AutoCombatMgr.IsAutoCombat();
    }

    /// <summary>
    /// 异步战斗回合
    /// </summary>
    private void DoAsncRound(Property ob, string cookie, int type, LPCMapping extraPara)
    {
        // 如果是自动攻击
        if (IsAutoCombat(ob, type))
        {
            LPCMapping para = new LPCMapping();
            para.Add("cookie", cookie); // 获取当前cookie
            para.Add("type", type); // 获取当前回合类型
            para.Append(extraPara); // 追加附加参数

            // 执行回合
            bool ret = TacticsMgr.DoTactics(ob, TacticsConst.TACTICS_TYPE_ATTACK, para);

            // 如果执行回合失败
            if (! ret)
                DoRoundActionEnd(cookie, extraPara);

            // 返回
            return;
        }

        // 构建事件参数
        LPCMapping eventPara = new LPCMapping();
        eventPara.Add("rid", ob.GetRid());
        eventPara.Add("type", type);
        eventPara.Add("cookie", cookie);

        // 抛出等待输入战斗指令
        EventMgr.FireEvent(EventMgrEventType.EVENT_WAIT_COMMAND, MixedValue.NewMixedValue<LPCMapping>(eventPara), false, true);
    }

    /// <summary>
    /// 战斗开始
    /// </summary>
    private IEnumerator _DoStart(float delayTime)
    {
        // 延迟开始副本
        yield return TimeMgr.WaitForCombatSeconds(delayTime);

        // 标识战斗开始
        IsRunningFlag = true;
    }

    /// <summary>
    /// 从回合行动顺序列表中红选择对象
    /// </summary>
    private Property GetRoundProperty()
    {
        // 行动顺序队列已经清空
        if (mActionOrderList.Count == 0)
        {
            // 初始化行动顺序
            // 重新排序重新计算攻击权重
            mActionOrderList = RoundCombatMgr.GetSortActionOrder(mCombatPropertyList, true);

            // 初始化回合次数
            mRoundTimes += 1;

            // 非验证客户端需要检查战斗回合数限制
            if (! AuthClientMgr.IsAuthClient)
                InstanceMgr.AddRoundCount(ME.user, 1);

            // 生成一个cookie
            mRoundCookie = Game.NewCookie("round_combat");

            // 记录玩家当前的回合数
            for (int i = 0; i < mActionOrderList.Count; i++)
            {
                // 角色对象不存在
                if (mActionOrderList[i] == null)
                    continue;

                // 设置当前回合cookie
                mActionOrderList[i].SetTemp("round_cookie", LPCValue.Create(mRoundCookie));

                // 如果角色已经死亡不在处理
                if (mActionOrderList[i].CheckStatus("DIED"))
                    continue;

                // 记录玩家当前回合信息
                mActionOrderList[i].SetTemp("cur_rounds", LPCValue.Create(mRoundTimes));
            }

            // 抛出事件战斗回合开始
            EventMgr.FireEvent(EventMgrEventType.EVENT_COMBAT_ROUND_START, null, true);
        }

        // 如果数据已经Dirty需要对行动列表重新排序
        if (mIsDirty)
        {
            // 重新排序如果影响攻击顺序条件没有发生变化不需要重新计算攻击权重
            mActionOrderList = RoundCombatMgr.GetSortActionOrder(mActionOrderList, false);
            mIsDirty = false;
        }

        // 行动顺序队列已经清空
        if (mActionOrderList.Count == 0)
            return null;

        // 提取第一个元素
        Property roundOb = mActionOrderList[0];
        mActionOrderList.RemoveAt(0);

        // 返回
        return roundOb;
    }

    /// <summary>
    /// 检测是否结束战斗
    /// </summary>
    private bool TryEndRoundCombat(Property ob)
    {
        // 如果战斗已经结束不处理
        // 或者战斗还没有开始不能尝试结束战斗
        if (! IsRunningFlag)
            return false;

        // 还有mActionMap没有结束，不处理
        if (mActionMap.Count != 0 || mAsynActionMap.Count != 0)
            return false;

        // 列表中已经没有该角色
        if (mCombatPropertyList.IndexOf(ob) == -1)
            return false;

        // 获取玩家的阵营id
        int campId = ob.CampId;

        // 获取所有的战斗对象列表
        List<Property> propertyList = GetPropertyList();
        mFailMap.Clear();

        // 判断是否需要结束战斗
        for (int i = 0; i < propertyList.Count; i++)
        {
            if (propertyList[i] == null)
                continue;

            // 如果角色已经死亡不处理
            // 如果是替身角色不处理
            if (propertyList[i].CheckStatus("DIED")||
                propertyList[i].Query<int>("is_standin") != 0)
                continue;

            // 如果该阵营已经记录过了不再处理
            if (mFailMap.ContainsKey(propertyList[i].CampId))
                continue;

            // 记录数据
            mFailMap.Add(propertyList[i].CampId, 0);
        }

        // 攻守双方都还没有
        if (mFailMap.ContainsKey(CampConst.CAMP_TYPE_ATTACK) &&
            mFailMap.ContainsKey(CampConst.CAMP_TYPE_DEFENCE))
            return false;

        // 构建事件参数
        LPCMapping ePara = new LPCMapping();
        ePara.Add("round_type", RoundType);

        // 攻方胜利
        if (mFailMap.ContainsKey(CampConst.CAMP_TYPE_ATTACK))
            ePara.Add("camp_id", CampConst.CAMP_TYPE_ATTACK);
        else if (mFailMap.ContainsKey(CampConst.CAMP_TYPE_DEFENCE))
            ePara.Add("camp_id", CampConst.CAMP_TYPE_DEFENCE);
        else
        {
            // 攻守双方角色都死了，默认campId胜利
            ePara.Add("camp_id", campId);
        }

        // 标识战斗已经结束
        IsRunningFlag = false;

        // 暂停回合战斗
        Pause();

        // 抛出胜利事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_ROUND_COMBAT_END, MixedValue.NewMixedValue<LPCMapping>(ePara), false, true);

        // 返回结束战斗
        return true;
    }

    /// <summary>
    /// 执行反击回合
    /// </summary>
    private bool DoTypeRound(int type, bool isAllDoRound)
    {
        // 该类型回合列表为空
        List<CombatData> roundList;
        if (! mRoundList.TryGetValue(type, out roundList) ||
            roundList.Count == 0)
            return false;

        // 判断是否需要全部执行
        if (! isAllDoRound)
        {
            // 提取第一个元素
            CombatData data = roundList[0];
            roundList.RemoveAt(0);

            // 如果反击对象已经死亡或者反击对象不存在不处理
            if (type == RoundCombatConst.ROUND_TYPE_COMBO)
            {
                // 获取参数
                LPCMapping para = data.Para;

                // 获取反击对象
                // 如果反击对象已经死亡或者反击对象不存在不处理
                string pickRid = para.GetValue<string>("pick_rid");
                Property counterOb = Rid.FindObjectByRid(pickRid);
                if (counterOb == null || counterOb.CheckStatus("DIED"))
                    return true;
            }

#if UNITY_EDITOR
            // 执行玩家的回合
            DoRound(data.CombatOb, data.RoundType, data.Para);
#else
            // 执行回合
            try
            {
                // 执行玩家的回合
                DoRound(data.CombatOb, data.RoundType, data.Para);
            }
            catch (Exception e)
            {
                LogMgr.Exception(e);
            }
#endif
        }
        else
        {
            // 遍历数据
            for (int i = 0; i < roundList.Count; i++)
            {
                // 获取战斗数据
                CombatData data = roundList[i];

                // 如果反击对象已经死亡或者反击对象不存在不处理
                if (type == RoundCombatConst.ROUND_TYPE_COUNTER)
                {
                    // 获取参数
                    LPCMapping para = data.Para;

                    // 获取反击对象
                    // 如果反击对象已经死亡或者反击对象不存在不处理
                    string pickRid = para.GetValue<string>("pick_rid");
                    Property counterOb = Rid.FindObjectByRid(pickRid);
                    if (counterOb == null || counterOb.CheckStatus("DIED"))
                        continue;
                }

#if UNITY_EDITOR
                // 执行玩家的回合
                DoRound(data.CombatOb, data.RoundType, data.Para);
#else
                // 执行回合
                try
                {
                    // 执行玩家的回合
                    DoRound(data.CombatOb, data.RoundType, data.Para);
                }
                catch (Exception e)
                {
                    LogMgr.Exception(e);
                }
#endif
            }

            // 清除数据
            roundList.Clear();
        }

        // 返回成功
        return true;
    }

    /// <summary>
    /// 是否有可以操作的回合
    /// </summary>
    private bool HasOperationRound(Property ob)
    {
        // 玩家是否还有普通回合没有执行
        if (mActionOrderList.IndexOf(ob) != -1)
            return true;

        // 如果没有ROUND_TYPE_ADDITIONAL回合
        List<CombatData> combatList;
        if (!mRoundList.TryGetValue(RoundCombatConst.ROUND_TYPE_ADDITIONAL, out combatList)
            || combatList.Count == 0)
            return false;

        // 检查玩家是否有追加回合
        foreach (CombatData cb in combatList)
        {
            // 如果玩家角色有最佳回合
            if (cb.CombatOb == ob)
                return true;
        }

        // 玩家没有可以操作的回合了
        return false;
    }

    #endregion
}

/// <summary>
/// 战斗数据类型
/// </summary>
public class CombatData
{
    /// <summary>
    /// 战斗对象
    /// </summary>
    public Property CombatOb;

    /// <summary>
    /// 回合类型
    /// </summary>
    public int RoundType;

    /// <summary>
    /// 详细参数
    /// </summary>
    public LPCMapping Para;

    /// <summary>
    /// Initializes a new instance of the <see cref="CombatData"/> class.
    /// </summary>
    /// <param name="combatOb">Combat ob.</param>
    /// <param name="roundType">Round type.</param>
    /// <param name="para">Para.</param>
    public CombatData(Property combatOb, int roundType, LPCMapping para)
    {
        CombatOb = combatOb;
        RoundType = roundType;
        Para = para;
    }
}
