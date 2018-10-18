/// <summary>
/// EnterMap.cs
/// Create by zhaozy 2014-11-26
/// 进入地图策略
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class EnterMap : Tactics
{
    #region 内部接口

    /// <summary>
    /// Gets the position list.
    /// </summary>
    /// <returns>The position list.</returns>
    private List<Vector3> GetPosition(Property ob, string sceneId)
    {
        // 根据阵型站位计算位置信息和朝向
        CsvRow data = FormationMgr.GetFormationPosData(sceneId,
            ob.FormationRaw, ob.FormationPos, ob.FormationId);

        // 没有配置的数据
        if (data == null)
        {
            LogMgr.Error("{0}阵型配置表配置错误！", sceneId);
            return new List<Vector3>();
        }

        LPCArray pos = LPCArray.Empty;
        LPCArray enterPos = LPCArray.Empty;

#if UNITY_EDITOR

        // 获取GM功能换边标识
        if (AuthClientMgr.IsAuthClient ||
            ME.user.QueryTemp<int>("switch_postion") == 0)
        {
            if (ob.CampId == CampConst.CAMP_TYPE_ATTACK)
            {
                enterPos = data.Query<LPCArray>("attack_enter_pos");
                pos = data.Query<LPCArray>("attack_pos");
            }
            else
            {
                enterPos = data.Query<LPCArray>("defence_enter_pos");
                pos = data.Query<LPCArray>("defence_pos");
            }
        }
        else
        {
            if (ob.CampId == CampConst.CAMP_TYPE_ATTACK)
            {
                enterPos = data.Query<LPCArray>("defence_enter_pos");
                pos = data.Query<LPCArray>("defence_pos");
            }
            else
            {
                enterPos = data.Query<LPCArray>("attack_enter_pos");
                pos = data.Query<LPCArray>("attack_pos");
            }
        }

#else

        if (ob.CampId == CampConst.CAMP_TYPE_ATTACK)
        {
            enterPos = data.Query<LPCArray>("attack_enter_pos");
            pos = data.Query<LPCArray>("attack_pos");
        }
        else
        {
            enterPos = data.Query<LPCArray>("defence_enter_pos");
            pos = data.Query<LPCArray>("defence_pos");
        }

#endif

        // 检验数据的有效性
        if (enterPos.Count != 3 &&
            pos.Count != 3)
        {
            LogMgr.Error("阵型配置表配置错误， 进场位置或者最终位置必须配置一个 ！");
            return new List<Vector3>();
        }

        // 修正位置
        if (enterPos.Count != 3)
            enterPos = pos;
        else if (pos.Count != 3)
            pos = enterPos;

        // 是否有指定原地进场
        if (ob.Query<LPCValue>("direct_enter_map", true) != null)
            enterPos = pos;

        // 返回有效的坐标
        return new List<Vector3>()
        {
            new Vector3(enterPos[0].AsFloat, enterPos[1].AsFloat, enterPos[2].AsFloat),
            new Vector3(pos[0].AsFloat, pos[1].AsFloat, pos[2].AsFloat)
        };
    }

    /// <summary>
    /// 获取角色唯一id
    /// </summary>
    /// <returns>The unique identifier.</returns>
    /// <param name="ob">Ob.</param>
    private int GetUniqueId(Property ob)
    {
        // 角色唯一id信息构成CampId, FormationRaw, FormationId, FormationPos, 是否是召唤怪
        return ob.CampId * 10000 +
            FormationConst.GetRawID(ob.FormationRaw) * 1000 +
            ob.FormationId * 100 +
            ob.FormationPos * 10 + 
            (string.IsNullOrEmpty(ob.Query<string>("summoner_rid")) ? 0 : 1);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 策略执行入口
    /// </summary>
    public override bool Trigger(params object[] _params)
    {
        // 获取怪物对象
        Char charOb = _params[0] as Char;
        LPCMapping args = _params[1] as LPCMapping;

        // 获取场景scene_id
        string sceneId = args.GetValue<string>("scene_id");

        // 获取位置信息
        List<Vector3> posList = GetPosition(charOb, sceneId);

        // 没有位置信息
        if (posList.Count == 0)
            return false;

        // 设置模型的世界缩放
        charOb.SetWorldScale(args.GetValue<float>("scale", 1f));

#if UNITY_EDITOR

        // 执行进入房间, 如果有进场位置则需要按照进场位置进入地图
        // 否则直接设置最终位置
        if (AuthClientMgr.IsAuthClient)
        {
            charOb.EnterCombatMap(
                sceneId,
                posList[0],
                (charOb.CampId == CampConst.CAMP_TYPE_ATTACK ? ObjectDirection2D.RIGHT : ObjectDirection2D.LEFT),
                GetUniqueId(charOb)
            );
        } else
        {
            charOb.EnterCombatMap(
                sceneId,
                posList[0],
                (ME.user.QueryTemp<int>("switch_postion") == 0 ? 
                    (charOb.CampId == CampConst.CAMP_TYPE_ATTACK ? ObjectDirection2D.RIGHT : ObjectDirection2D.LEFT) :
                    (charOb.CampId == CampConst.CAMP_TYPE_ATTACK ? ObjectDirection2D.LEFT : ObjectDirection2D.RIGHT)),
                GetUniqueId(charOb));
        }

#else

        // 执行进入房间, 如果有进场位置则需要按照进场位置进入地图
        // 否则直接设置最终位置
        charOb.EnterCombatMap(
            sceneId,
            posList[0],
            (charOb.CampId == CampConst.CAMP_TYPE_ATTACK ? ObjectDirection2D.RIGHT : ObjectDirection2D.LEFT),
            GetUniqueId(charOb));

#endif

        // 设置角色的原始归位位置
        charOb.MoveBackPos = posList[1];

        // 如果入场位置和最终目标位置不一致，则需要在进场后自动发起移动
        // 直到移动到目标点结束
        if (!Game.FloatEqual((posList[1] - posList[0]).sqrMagnitude, 0))
        {
            // 发起移动，直到移动到MoveBackPos结束
            LPCMapping actionPara = new LPCMapping();
            actionPara.Add("target_pos", new LPCArray(posList[1].x, posList[1].y, posList[1].z));
            actionPara.Add("enter_map", 1);

            // 通知在战斗系统出场技能
            charOb.Actor.DoActionSet("move", Game.NewCookie(charOb.GetRid()), actionPara);

            // 需要等到进场动画播放完成后再抛出进程完成事件
            return true;
        }

        // 需要出场动作和出场光效等参数
        LPCValue appearAction = charOb.Query("appear_action");
        if (appearAction != null)
        {
            string actionName = string.Empty;
            if (appearAction.IsInt)
                actionName = ScriptMgr.Call(appearAction.AsInt, charOb) as string;
            else if (appearAction.IsString)
                actionName = appearAction.AsString;

            // 有出场动作，则播放actionName
            if (!string.IsNullOrEmpty(actionName))
            {
                // 构建参数
                LPCMapping actionPara = new LPCMapping();
                actionPara.Add("action", actionName);
                actionPara.Add("rid", charOb.GetRid());

                // 通知在战斗系统出场技能
                charOb.Actor.DoActionSet(actionName, Game.NewCookie(charOb.GetRid()), actionPara);

                // 需要等到进场动画播放完成后再抛出进程完成事件
                return true;
            }
        }

        // 抛出实体进场完成
        LPCMapping eventArgs = new LPCMapping();
        eventArgs.Add("rid", charOb.GetRid());
        EventMgr.FireEvent(EventMgrEventType.EVENT_READY_COMBAT, MixedValue.NewMixedValue<LPCMapping>(eventArgs), false, true);

        // 触发成功
        return true;
    }

    #endregion
}