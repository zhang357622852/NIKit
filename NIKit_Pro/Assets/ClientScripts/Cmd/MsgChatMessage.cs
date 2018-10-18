using LPC;

/// <summary>
/// 聊天信息
/// </summary>
public class MsgChatMessage : MsgHandler
{
    public string GetName()
    {
        return "msg_chat_message";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping;

        // 缓存消息
        ChatRoomMgr.AddChatMessage(args ["type"].AsString,
                                   args ["message_list"].AsArray);
    }
}
