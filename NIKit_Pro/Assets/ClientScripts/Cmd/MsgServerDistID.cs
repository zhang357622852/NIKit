using LPC;

/// <summary>
/// 服务器通知客户端服务器编号
/// </summary>
public class MsgServerDistID : MsgHandler
{
    public string GetName()
    {
        return "msg_server_dist_id";
    }

    public void Go(LPCValue para)
    {
        Communicate.SetServerInfo("server_dist_id", para);
    }
}
