using LPC;

/// <summary>
/// 处理区列表
/// </summary>
public class MsgLServerList : MsgHandler
{
    public string GetName() { return "msg_l_server_list"; }

    public void Go(LPCValue para)
    {
        LogMgr.Trace("获得服务器列表");

        Communicate.AccountInfo.Set("server_list", para.AsMapping["server_list"]);
    }
}
