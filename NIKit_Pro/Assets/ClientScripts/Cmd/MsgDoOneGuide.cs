using LPC;
using System.Diagnostics;

/// <summary>
/// 服务器通知执行指引
/// </summary>
/// <author>weism</author>
public class MsgDoOneGuide : MsgHandler
{
    public string GetName()
    {
        return "msg_do_one_guide";
    }

    public void Go(LPCValue para)
    {
    }
}
