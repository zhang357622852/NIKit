using LPC;
using System.Diagnostics;

/// <summary>
/// 服务器通知客户端实体出现
/// </summary>
/// <author>weism</author>
public class MsgAppear : MsgHandler
{
    public string GetName()
    {
        return "msg_appear";
    }

    public void Go(LPCValue para)
    {
    }
}
