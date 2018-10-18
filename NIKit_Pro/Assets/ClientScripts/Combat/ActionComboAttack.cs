/// <summary>
/// ActionComboAttack.cs
/// Created by zhaozy 2018-6-6
/// 连续攻击
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ActionComboAttack : ActionBase
{
    #region 成员变量

    /// <summary>
    /// 连击连击检查脚本
    /// </summary>
    private int mComboCheckScript = -1;

    /// <summary>
    /// 连击事件
    /// </summary>
    private string mComboEvent = string.Empty;

    /// <summary>
    /// 连击次数
    /// </summary>
    private int mComboTimes = 0;

    #endregion

    #region 内部函数

    /// <summary>
    /// 执行连击
    /// </summary>
    private void DoComboAttack()
    {
        // 没有连击脚本
        if (mComboCheckScript == -1)
        {
            IsFinished = true;
            return;
        }

        // 添加mComboTimes
        ActionSet.ExtraArgs.Add("combo_times", mComboTimes);

        // 调用循环检查脚本，判断是否需要连续攻击
        bool comboCheckRet = (bool) ScriptMgr.Call(mComboCheckScript, Actor.ActorName, ActionSet.Cookie, ActionSet.ExtraArgs);

        // 检查脚本没有通过
        if (! comboCheckRet)
        {
            IsFinished = true;
            return;
        }

        // 抛出eventName
        Actor.TriggerEvent("COMBO_ATTACK_HIT", ActionSet.Cookie);

        // 增加连击次数
        mComboTimes++;
    }

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="actionSet">所属序列</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionComboAttack(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        // 获取连击检查脚本
        mComboCheckScript = para.GetProperty<int>("combo_check_script", -1);

        // 连击事件
        mComboEvent = para.GetProperty<string>("combo_event", "COMBO_ATTACK_END");
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        base.Start();

        // 角色还没有加载
        if (! Actor.IsLoaded)
        {
            IsFinished = true;
            return;
        }

        // 执行连击
        DoComboAttack();
    }

    /// <summary>
    /// 结束节点
    /// </summary>
    /// <param name="isCancel">是否cancel方式结束</param>
    public override void End(bool isCancel = false)
    {
        // 结束战斗
        base.End(isCancel);

        // 抛出eventName
        Actor.TriggerEvent("ATTACK_END", ActionSet.Cookie);
    }

    /// <summary>
    /// 节点更新
    /// </summary>
    /// <param name="info">时间参数信息</param>
    public override void Update(TimeDeltaInfo info)
    {
    }

    /// <summary>
    /// 事件触发节点
    /// </summary>
    /// <param name="info">时间参数信息</param>
    public override void TriggerCombatEvent(string eventName)
    {
        // 事件不一致
        if (! string.Equals(eventName, mComboEvent))
            return;

        // 执行连击
        DoComboAttack();
    }
}
