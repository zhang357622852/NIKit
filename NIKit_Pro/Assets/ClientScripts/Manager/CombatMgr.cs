/// <summary>
/// CombatMgr.cs
/// Created by zhaozy 2014-11-21
/// 逻辑战斗管理模块
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using LPC;

// 技能管理
public static class CombatMgr
{
    #region 变量

    /// <summary>
    /// 安全执行一段代码
    /// </summary>
    public delegate void SafeCallFunc();

    #endregion

    #region 私有接口

    /// <summary>
    /// 战斗系统驱动
    /// </summary>
    private static IEnumerator UpdateCombat()
    {
        while (true)
        {
#if UNITY_EDITOR

            // 战斗系统更新，编辑器模式下不用catch，直接暴露错误
            CombatRootMgr.Update();

            // 回合制系统驱动
            RoundCombatMgr.Update();

#else

            // 战斗系统更新
            Call(() => {
                CombatRootMgr.Update();
            });

            // 回合制系统驱动
            Call(() => {
                RoundCombatMgr.Update();
            });

#endif

            yield return null;
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 策略管理模块初始化借口
    /// </summary>
    public static void Init()
    {
        // hit初始化
        CombatHit.InIt();

        // status初始化
        CombatStatus.InIt();

        // CombatActionEnd初始化
        CombatActionEnd.InIt();

        // ReceiveDamage初始化
        CombatReceiveDamage.InIt();

        // CombatDie初始化
        CombatDie.InIt();

        // CombatDodge初始化
        CombatDodge.InIt();

        // CombatReceiveCure初始化
        CombatReceiveCure.InIt();

        // TODO
    }

    /// <summary>
    /// Call the specified f.
    /// </summary>
    /// <param name="f">F.</param>
    public static void Call(SafeCallFunc f)
    {
        try
        {
            f();
        }
        catch (System.Exception e)
        {
            LogMgr.Exception(e);
        }
    }

    /// <summary>
    /// 开始战斗
    /// </summary>
    public static bool EnterCombat()
    {
        // 战斗客户端忽略所有消息
        if (Communicate.IsConnectedGS() && AuthClientMgr.IsAuthClient)
            Communicate.GSConnector.GetConnect().checkGreenPassage = (cmdNo) => { return true; };

        // 开始战斗
        Coroutine.StopCoroutine("UpdateCombat");
        Coroutine.DispatchService(UpdateCombat(), "UpdateCombat", true);

        return true;
    }

    /// <summary>
    /// 结束战斗
    /// </summary>
    public static void QuitCombat()
    {
        // 删除战斗携程
        Coroutine.StopCoroutine("UpdateCombat");

        // 设置不忽略服务器回执
        if (Communicate.IsConnectedGS() && AuthClientMgr.IsAuthClient)
            Communicate.GSConnector.GetConnect().checkGreenPassage = null;
    }

    /// <summary>
    /// Dos the cast skill.
    /// </summary>
    public static void DoCastSkill(Property sourceOb, Property targetOb, int skillId, LPCMapping args)
    {
        // 只要有一个对象不存在
        if (sourceOb == null || targetOb == null)
            return;

        // 生成新的cookie
        string rid = sourceOb.GetRid();
        string cookie = Game.NewCookie(rid);

        // 构建参数
        LPCMapping para = new LPCMapping();
        para.Add("skill_id", skillId);                    // 释放技能
        para.Add("pick_rid", targetOb.GetRid());          // 技能拾取目标
        para.Add("cookie", cookie);                       // 技能cookie
        para.Add("rid", rid);                             // 释放技能角色rid
        para.Append(args);

        // 执行释放技能策略
        bool ret = TacticsMgr.DoTactics(sourceOb, TacticsConst.TACTICS_TYPE_ATTACK, para);

        // 执行攻击策略失败
        if (!ret)
            return;

        // 添加回合操作列表
        RoundCombatMgr.AddRoundAction(cookie, sourceOb, para);
    }

    #endregion
}
