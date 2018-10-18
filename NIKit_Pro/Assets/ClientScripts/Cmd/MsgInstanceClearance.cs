using LPC;

/// <summary>
/// 副本通关消息
/// </summary>
public class MsgInstanceClearance : MsgHandler
{
    public string GetName()
    {
        return "msg_instance_clearance";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
        // 消息参数
        //id : 副本id
        //result : 通关结果
        //clearance_time : 通关时间
        //bonus_map : 奖励信息(经验奖励，属性奖励，道具奖励)
        LPCMapping args = para.AsMapping;

        string instanceId = args.GetValue<string>("instance_id");

        LPCMapping instance = InstanceMgr.GetInstanceInfo(instanceId);

        int mapType = InstanceMgr.GetInstanceMapType(instanceId);

        if (args.GetValue<int>("result") == 1)
        {
            if (InstanceMgr.GetLoopFightByInstanceId(instanceId))
            {
                if (mapType.Equals(MapConst.TOWER_MAP))
                {
                    string next = instance.GetValue<string>("next_instance_id");
                    if (!string.IsNullOrEmpty(next))
                    {
                        // 设置下一关为自动战斗
                        InstanceMgr.SetLoopFight(next, true);
                    }
                    else
                    {
                        InstanceMgr.SetLoopFight(instanceId, false);
                    }
                }
            }
        }
        else
        {
            if (! mapType.Equals(MapConst.INSTANCE_MAP_1))
                InstanceMgr.SetLoopFight(instanceId, false);
        }

        // 取消循环战斗
        if (mapType.Equals(MapConst.INSTANCE_MAP_1))
            InstanceMgr.CancelLoopFight(instanceId, args);

        // 副本通关显示供方阵容
        RoundCombatMgr.ClearanceInstanceShowPet(InstanceMgr.GetFightList(), args);

        if (instance != null)
        {
            // 尝试解锁新地图
            MapMgr.TryUnlockMap(ME.user, instance.GetValue<int>("map_id"));
        }

        //抛出副本通关事件;
        EventMgr.FireEvent(EventMgrEventType.EVENT_INSTANCE_CLEARANCE, MixedValue.NewMixedValue<LPCMapping>(args));
    }
}
