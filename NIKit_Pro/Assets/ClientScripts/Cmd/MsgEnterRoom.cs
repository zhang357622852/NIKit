using LPC;

/// <summary>
/// 服务器通知客户端进入房间
/// </summary>
public class MsgEnterRoom : MsgHandler
{
    public string GetName() { return "msg_enter_room"; }

    public void Go(LPCValue para)
    {
        // 取得服务器数据
        LPCMapping m = para.AsMapping;

        LogMgr.Trace("进入房间{0}({1}) {2}。", m["room"].AsMapping["name"].AsString,
                     m["room"].AsMapping["rid"].AsString, m["pos"].AsString);

        // 进入房间
        ME.EnteredRoom(m["pos"].AsString, m["direction"].AsInt, m["room"]);
    }
}
