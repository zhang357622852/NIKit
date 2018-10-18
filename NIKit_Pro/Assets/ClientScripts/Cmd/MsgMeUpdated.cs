using LPC;

/// <summary>
/// 主角的信息更新了
/// </summary>
public class MsgMeUpdated : MsgHandler
{
    public string GetName()
    {
        return "msg_me_updated";
    }

    public void Go(LPCValue para)
    {
        // 取得服务器数据
        LPCMapping m = para.AsMapping;

        // 获取该玩家的备份数据
        // 完整的玩家数据应该数本地数据 + 服务器玩家数据
        string userRid = m["rid"].AsString;

        // 如果user对象已经存在则直接替换数据，否则需要重新创建角色对象
        if (ME.user == null || ! string.Equals(userRid, ME.GetRid()))
            ME.user = UserMgr.CreateUser(userRid, m["dbase"].AsMapping);
        else
            ME.user.dbase.Absorb(m["dbase"].AsMapping);
    }
}
