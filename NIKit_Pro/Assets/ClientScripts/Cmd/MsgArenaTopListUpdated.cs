using LPC;
using System.Diagnostics;

/// <summary>
/// 服务器通知排行榜更新
/// </summary>
public class MsgArenaTopListUpdated : MsgHandler
{
    public string GetName()
    {
        return "msg_arena_top_list_updated";
    }

    public void Go(LPCValue para)
    {
        // 通知排行榜管理模块排行榜更新了
        ArenaMgr.SetTopListDirty();
    }
}
