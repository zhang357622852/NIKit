using LPC;

// 通知新的探索奖励信息
public class MsgNotifyNewResearchBonus : MsgHandler
{
    public string GetName()
    {
        return "msg_notify_new_research_bonus";
    }

    public void Go(LPCValue para)
    {
        TaskMgr.DoCacheResearchhBonus (para.AsMapping.GetValue<LPCMapping>("bonus_data"));
    }
}

