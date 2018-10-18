using LPC;

/// <summary>
/// 服务器通知客户端创建角色结果
/// </summary>
public class MsgCreateNewCharResult : MsgHandler
{
    public string GetName()
    {
        return "msg_create_new_char_result";
    }

    public void Go(LPCValue para)
    {

    }
}
