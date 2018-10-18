using LPC;

/// <summary>
/// 全局信息更新了
/// </summary>
public class MsgUnivserseUpdated : MsgHandler
{
    public string GetName()
    {
        return "msg_universe_updated";
    }

    public void Go(LPCValue para)
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 转换数据格式
        LPCMapping m = para.AsMapping;

        // 更新数据
        ME.user.tempdbase.Absorb(m ["dbase"].AsMapping);
    }
}
