/// <summary>
/// Intensify.cs
/// Create by fengsc 2016/08/16
/// 装备强化模块
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class Intensify : Blacksmith
{
    public override bool DoAction(params object[] args)
    {
        return true;
    }

    public override bool DoActionResult(params object[] args)
    {
        // 获取对象
        Property who = args[0] as Property;

        if(who == null)
            return false;

        LPCValue extra_data = args[1] as LPCValue;

        // 工坊操作失败
        if (! extra_data.IsMapping)
        {
            EventMgr.FireEvent(EventMgrEventType.EVENT_EQUIP_STRENGTHEN, MixedValue.NewMixedValue<LPCMapping>(LPCMapping.Empty));
            return false;
        }

        // 构建参数
        LPCMapping map = new LPCMapping ();

        // 获取服务器下发的参数
        LPCMapping para = extra_data.AsMapping;

        if(para == null)
            return false;

        // 获取装备宿主的rid
        string envRid = para.GetValue<string>("env_rid");

        // 装备宿主对象
        Property envOb = Rid.FindObjectByRid(envRid);

        // 刷新属性
        if(MonsterMgr.IsMonster(envOb))
            PropMgr.RefreshAffect(envOb, "equip");

        // 获取装备强化信息
        int rank = para.GetValue<int>("rank");

        // 获取强化结果
        int result = para.GetValue<int>("result");

        map.Add("rank", rank);
        map.Add("result", result);

        int tempRank = result == 1 ? rank - 1 : rank;

        if (who.QueryTemp<int>("gapp_world") == 1 || tempRank >= GameSettingMgr.GetSettingInt("limit_equip_intensify_rank"))
        {
            // 开启版署模式累计强化次数
            LPCMapping limitData = LPCMapping.Empty;

            LPCValue v = OptionMgr.GetLocalOption(ME.user, "limit_equip_intensify");
            if (v != null && v.IsMapping)
                limitData = v.AsMapping;

            if (!TimeMgr.IsSameDay(limitData.GetValue<int>("refresh_time"), TimeMgr.GetServerTime()))
                limitData = LPCMapping.Empty;

            // 累计次数
            limitData.Add("amount", Mathf.Min(limitData.GetValue<int>("amount") + 1, GameSettingMgr.GetSettingInt("max_equip_intensify")));

            // 缓存本次抽奖的时间
            limitData.Add("refresh_time", TimeMgr.GetServerTime());

            // 将数据缓存到本地
            OptionMgr.SetLocalOption(ME.user, "limit_equip_intensify", LPCValue.Create(limitData));
        }

        // 抛出装备强化成功事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_EQUIP_STRENGTHEN, MixedValue.NewMixedValue<LPCMapping>(map));

        return true;
    }
}
