using LPC;

/// <summary>
/// 有人在当前频道说话
/// </summary>
public class MsgChannel : MsgHandler
{
    public string GetName() { return "msg_channel"; }

    public void Go(LPCValue para)
    {
        LogMgr.Trace("", "{0}说：{1}", para.AsMapping["sender_name"].AsString,
                     para.AsMapping["message"].AsString);

        // ChatRoomManager.AddMessage(para);
    }
}
