using LPC;
using System.Diagnostics;

/// <summary>
/// 服务器通知排行榜数据事件
/// </summary>
public class MsgArenaTopData : MsgHandler
{
    public string GetName()
    {
        return "msg_arena_top_data";
    }

    public void Go(LPCValue para)
    {
        // 通知排行榜管理模块
        ArenaMgr.UpdateTopData(para.AsMapping.GetValue<LPCMapping>("top_data"));
    }
}
