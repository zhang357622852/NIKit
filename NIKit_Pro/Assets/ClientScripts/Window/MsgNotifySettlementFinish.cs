/// <summary>
/// MsgNotifySettlementFinish.cs
/// Created by fengsc 2017/07/01
/// 竞技场结算完毕通知客户端
/// </summary>
using LPC;

public class MsgNotifySettlementFinish : MsgHandler
{
    public string GetName()
    {
        return "msg_notify_settlement_finish";
    }

    public void Go(LPCValue para)
    {
        LPCMapping data = LPCMapping.Empty;

        LPCArray bonus = para.AsMapping.GetValue<LPCArray>("bonus");

        data.Add("last_bonus", bonus);

        // 更新结算奖励数据
        ArenaMgr.UpdateSettlementBonusData(ME.user, data);

        EventMgr.FireEvent(EventMgrEventType.EVENT_NOTIFY_SETTLEMENT_FINISH, null);
    }
}
