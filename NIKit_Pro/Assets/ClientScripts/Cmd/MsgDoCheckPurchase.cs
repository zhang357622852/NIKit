/// <summary>
/// MsgDoCheckPurchase.cs
/// Created by zhaozy 2015/06/09
/// 检测购买信息
/// </summary>
using LPC;
using System.Collections.Generic;

public class MsgDoCheckPurchase : MsgHandler
{
    public string GetName()
    {
        return "msg_do_check_purchase";
    }

    /// <summary>
    /// 入口
    /// </summary>
    /// <param name="para">Para.</param>
    public void Go(LPCValue para)
    {
    }
}
