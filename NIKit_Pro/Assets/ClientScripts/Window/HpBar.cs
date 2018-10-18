/// <summary>
/// HpBar.cs
/// Created by fucj 2014-11-27
/// 血条对象脚本
/// </summary>

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using LPC;

public partial class HpBar : WindowBase<HpBar>
{
    #region 成员变量

    // 蓝条
    public UISlider mpSlider;

    // 血条
    public UISlider hpSlider;

    //最大生命增加
    public UISlider mMaxHpSlider;

    // 宠物等级
    public UILabel lblevel;

    // 等级元素
    public UISprite petElement;

    // 宠物buff
    public UIGrid buffGrid;

    // 遮挡
    public GameObject Cover;

    // buff基础对象
    public GameObject buffItem;

    #endregion

    #region 私有变量

    // 宠物的Rid
    private string mRid;

    // 宠物对象
    private Char target;

    // 原始位置
    private Vector3 lastUiPos = Vector3.one;

    private int mHp;

    private int mMaxHp;

    private int mMaxHpAddition;

    private int mMp;

    private int mMaxMp;

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 初始化窗口
        Redraw();
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void Redraw()
    {
        // 设置英雄等级
        lblevel.text = target.GetLevel().ToString();

        // 获取对象元素
        int element = target.BasicQueryNoDuplicate<int>("element");

        switch (element)
        {
            case MonsterConst.ELEMENT_NONE:
                petElement.spriteName = "hpbar_element_none";
                break;
            case MonsterConst.ELEMENT_DARK:
                petElement.spriteName = "hpbar_element_dark";
                break;
            case MonsterConst.ELEMENT_FIRE:
                petElement.spriteName = "hpbar_element_fire";
                break;
            case MonsterConst.ELEMENT_LIGHT:
                petElement.spriteName = "hpbar_element_light";
                break;
            case MonsterConst.ELEMENT_STORM:
                petElement.spriteName = "hpbar_element_storm";
                break;
            case MonsterConst.ELEMENT_WATER:
                petElement.spriteName = "hpbar_element_water";
                break;
        }

        // 初始化设置血量
        setHpValue();

        // 初始化设置蓝量
        setMpValue();

        // 重绘技能窗口
        RedrawBuff();
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    private void Update()
    {
        // 调整窗口位置
        AdjustWnd();
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    private void OnDestroy()
    {
        // 注销事件
        EventMgr.UnregisterEvent(string.Format("HpBar_{0}", mRid));

        // 取消角色的属性字段变化回调
        if (target != null)
        {
            // Remove血条变化Trigger
            target.dbase.RemoveTriggerField("HpBar_mp");

            // Remove蓝条变化Trigger
            target.dbase.RemoveTriggerField("HpBar_hp");

            // Remove当前回合变化Trigger
            target.tempdbase.RemoveTriggerField("HpBar_rounds");
        }
    }

    ///<summary>
    /// 宠物生命值变化
    /// </summary>
    private void WhenHpChange(object param, params object[] paramEx)
    {
        setHpValue();
    }

    ///<summary>
    /// 宠物等级变化
    /// </summary>
    private void WhenLevelChange(object param, params object[] paramEx)
    {
        // 设置英雄等级
        lblevel.text = target.GetLevel().ToString();
    }

    /// <summary>
    /// Whens the mp change.
    /// </summary>
    /// <param name="param">Parameter.</param>
    /// <param name="paramEx">Parameter ex.</param>
    private void WhenMpChange(object param, params object[] paramEx)
    {
        setMpValue();
    }

    /// <summary>
    /// Whens the round change.
    /// </summary>
    /// <param name="param">Parameter.</param>
    /// <param name="paramEx">Parameter ex.</param>
    private void WhenRoundChange(object param, params object[] paramEx)
    {
        // cur_rounds存在则表示回合还没有结束，否则回合已经结束
        if (target.QueryTemp<int>("cur_rounds") == 0)
            SetInfoHide(true);
        else
            SetInfoHide(false);
    }

    ///<summary>
    /// 添加附加状态变化
    /// </summary>
    private void WhenStatusChange(int eventId, MixedValue para)
    {
        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();

        // 不是血条绑定对象id
        if (! string.Equals(args.GetValue<string>("rid"), mRid))
            return;

        // 延迟到下一帧调用;
        MergeExecuteMgr.DispatchExecute(DoDelayedRedrawBuff);
    }

    /// <summary>
    /// Whens the round status change.
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    private void WhenRoundStatusChange(int eventId, MixedValue para)
    {
        // 不是血条绑定对象id
        if (! string.Equals(para.GetValue<string>(), mRid))
            return;

        // 延迟到下一帧调用;
        MergeExecuteMgr.DispatchExecute(DoDelayedRedrawBuff);
    }

    ///<summary>
    /// 清除附加状态
    /// </summary>
    private void WhenStatusClear(int eventId, MixedValue para)
    {
        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();

        // 不是血条绑定对象id
        if (! string.Equals(args.GetValue<string>("rid"), mRid))
            return;

        // 判断角色是否是清除死亡状态
        LPCArray statusList = args.GetValue<LPCArray>("status_list");

        // 如果是清除死亡状态, 需要显示血条
        if (statusList.IndexOf(StatusMgr.GetStatusIndex("DIED")) != -1)
            target.Actor.ShowHp(!target.CheckStatus("DIED"));

        // 延迟到下一帧调用;
        MergeExecuteMgr.DispatchExecute(DoDelayedRedrawBuff);
    }

    /// <summary>
    /// 延迟重回buff窗口
    /// </summary>
    void DoDelayedRedrawBuff()
    {
        // 窗口已经析构，不在重绘
        if (this == null)
            return;

        // 重回buff窗口
        RedrawBuff();
    }

    /// <summary>
    /// 角色死亡事件回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    private void WhenCharDie(int eventId, MixedValue para)
    {
        // 获取参数获取targetOb
        Property ob = para.GetValue<Property>();
        if (ob == null)
            return;

        // 是否为同一个宠物
        if (! target.GetRid().Equals(ob.GetRid()))
            return;

        // 隐藏血条
        WindowMgr.HideWindow(gameObject);
    }

    /// <summary>
    /// 战斗结束回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    private void WhenRoundCombatEnd(int eventId, MixedValue para)
    {
        WindowMgr.HideWindow(gameObject);
    }

    /// <summary>
    /// Res the draw.
    /// </summary>
    private void RedrawBuff()
    {
        // 角色已经死亡不处理
        if (target.CheckStatus("DIED"))
            return;

        // 获取玩家当前所有状态
        List<LPCMapping> statusList = target.GetAllStatus();
        int statusId = -1;
        int childId = 0;
        GameObject btnOb;

        // 填充数据
        for (int i = 0; i < statusList.Count; i++)
        {
            statusId = statusList[i].GetValue<int>("status_id");

            // 无效状态id或者不需要显示的状态
            if (statusId <= 0 || ! StatusMgr.IsStatusShow(statusId))
                continue;

            // 如果没有窗口就创建
            if (childId >= buffGrid.transform.childCount)
            {
                // 将新的消息添加到列表的末尾
                btnOb = Instantiate(buffItem);
                btnOb.transform.SetParent(buffGrid.transform);
                btnOb.transform.localScale = Vector3.one;
                btnOb.transform.localPosition = Vector3.zero;
                btnOb.name = string.Format("BuffBtn{0}_{1}", target.GetRid(), childId);
            } else
            {
                btnOb = buffGrid.transform.GetChild(childId).gameObject;
            }

            // 显示图标
            if (! btnOb.activeSelf)
                btnOb.SetActive(true);

            // 设置按钮类型
            btnOb.GetComponent<BuffBtn>().SetType(statusId, statusList[i].GetValue<int>("round"));

            // childId++
            childId++;
        }

        // 隐藏多余的图标
        for (int i = childId; i < buffGrid.transform.childCount; i++)
        {
            // 获取子控件
            btnOb = buffGrid.transform.GetChild(i).gameObject;

            // 如果本身不是显示状态
            if (btnOb == null || ! btnOb.activeSelf)
                continue;

            // 隐藏控件
            btnOb.SetActive(false);
        }

        // 标识Grid需要重新排位置
        buffGrid.repositionNow = true;
    }

    /// <summary>
    /// 设置hp值
    /// </summary>
    private void setHpValue()
    {
        //获取最大生命加值 最大为0 最小为原始最大生命的负数
        int max_hp_addition = target.Query<int>("attrib_addition/max_hp");
        //原始最大生命值
        int max_hp = target.QueryOriginalAttrib("max_hp");
        if (max_hp <= 0)
            return;

        //最大生命加值如果大于原始最大生命，说明角色也死亡了
        if (Mathf.Abs(max_hp_addition) > max_hp)
            max_hp_addition = -max_hp;

        // 获取当前hp
        int hp = target.Query<int>("hp");
        if (hp <= 0)
            hp = 0;

        // 没有发生变化
        if (mHp == hp && mMaxHp == max_hp && mMaxHpAddition == max_hp_addition)
            return;

        // 设置血条进度
        hpSlider.value = (Mathf.Abs(max_hp_addition) + hp) * 1.0f / max_hp;

        //设置最大生命加值
        mMaxHpSlider.value = Mathf.Abs(max_hp_addition) * 1f / max_hp;

        mHp = hp;
        mMaxHp = max_hp;
        mMaxHpAddition = max_hp_addition;
    }


    /// <summary>
    /// Sets the mp value.
    /// </summary>
    private void setMpValue()
    {
        // 获取max_mp
        int max_mp = target.QueryAttrib("max_mp");
        if (max_mp <= 0)
            return;

        // 获取当前mp
        int mp = target.Query<int>("mp");
        if (mp <= 0)
            mp = 0;

        // 没有发生变化
        if (mMp == mp && mMaxMp == max_mp)
            return;

        // 设置mp进度
        mpSlider.value = mp * 1.0f / max_mp;

        mMp = mp;
        mMaxMp = max_mp;
    }

    // 宠物回合结束，置灰血条
    private void SetInfoHide(bool isHide)
    {
        if (isHide)
            Cover.SetActive(true);
        else
            Cover.SetActive(false);
    }

    /// <summary>
    /// 调整窗口位置
    /// </summary>
    private void AdjustWnd()
    {
        // 角色对象不存在
        if (target == null ||
            target.IsDestroyed ||
            target.Actor == null)
            return;

        // 计算当前位置
        Vector3 actorPos = target.Actor.GetPosition();
        Vector3 curPos = new Vector3(actorPos.x, actorPos.y + target.Actor.GetHpbarOffestY(), actorPos.z);
        Vector3 curUiPos = Game.WorldToUI(curPos);

        // 判断位置是否需要变化
        if (Game.FloatEqual((lastUiPos - curUiPos).sqrMagnitude, 0f))
            return;

        // 设置位置
        transform.position = curUiPos;
        lastUiPos = curUiPos;
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 设置当前宠物对象
    /// </summary>
    public void SetBind(string rid)
    {
        // 窗口绑定rid
        mRid = rid;

        // 获取绑定对象
        target = Rid.FindObjectByRid(mRid) as Char;
        string listenerId = string.Format("HpBar_{0}", mRid);

        // 注册附加状态
        EventMgr.RegisterEvent(listenerId, EventMgrEventType.EVENT_APPLY_STATUS, WhenStatusChange);

        // 注册清除状态
        EventMgr.RegisterEvent(listenerId, EventMgrEventType.EVENT_CLEAR_STATUS, WhenStatusClear);

        // 注册玩家死亡事件
        EventMgr.RegisterEvent(listenerId, EventMgrEventType.EVENT_DIE, WhenCharDie);

        // 注册玩家回合状态消失
        EventMgr.RegisterEvent(listenerId, EventMgrEventType.EVENT_STATUS_ROUND_UPDATE, WhenRoundStatusChange);

        // 注册战斗结束
        EventMgr.RegisterEvent(listenerId, EventMgrEventType.EVENT_ROUND_COMBAT_END, WhenRoundCombatEnd);

        // 关注角色属性字段变化
        if (target != null)
        {
            // 血条变化
            target.dbase.RegisterTriggerField("HpBar_mp", new string[]
                {
                    "mp",
                    "max_mp"
                }, new CallBack(WhenMpChange));

            // 蓝条变化
            target.dbase.RegisterTriggerField("HpBar_hp", new string[]
                {
                    "hp",
                    "max_hp",
                    "attrib_addition/max_hp"
                }, new CallBack(WhenHpChange));

            // 等级变化
            target.dbase.RegisterTriggerField("HpBar_level", new string[]
                {
                    "level",
                }, new CallBack(WhenLevelChange));

            // 攻击回合变化
            target.tempdbase.RegisterTriggerField("HpBar_rounds", new string[]
                {
                    "cur_rounds",
                }, new CallBack(WhenRoundChange));

            // 获取角色的世界缩放
            float worldScale = target.GetWorldScale();
            transform.localScale = new Vector3(worldScale, worldScale, worldScale);
        }

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 显示血条
    /// </summary>
    public void ShowHp()
    {
        // 获取角色的世界缩放
        if (target == null)
            return;

        // 获取角色的世界缩放
        float worldScale = target.GetWorldScale();
        transform.localScale = new Vector3(worldScale, worldScale, worldScale);
    }

    /// <summary>
    /// 隐藏血条
    /// </summary>
    public void HideHp()
    {
        // 隐藏血条窗口
        WindowMgr.HideWindow(gameObject);
    }

    /// <summary>
    /// 设置血条的sortOrder
    /// </summary>
    /// <param name="order">Order.</param>
    public void SetSortOrder(int order)
    {
        gameObject.GetComponent<UIPanel>().depth = order;
        //gameObject.GetComponent<UIPanel>().sortingOrder = order;
    }

    #endregion

}
