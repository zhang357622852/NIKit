using LPC;
using System.Diagnostics;

/// <summary>
/// 服务器通知客户端开启debug功能
/// </summary>
/// <author>weism</author>
public class MsgOpenDebug : MsgHandler
{
    public string GetName()
    {
        return "msg_open_debug";
    }

    public void Go(LPCValue para)
    {
    }
}
