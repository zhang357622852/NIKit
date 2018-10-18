/// <summary>
/// <summary>
/// CmdShareSuccess.cs
/// Created by fengxl 2015-3-19
/// 分享成功
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdShareSuccess:CmdHandler
    {
        public string GetName()
        {
            return "cmd_share_success";
        }

        public static bool Go()
        {
            LogMgr.Trace("[CmdShareSuccess.cs] 分享成功获得5钻石");
            Communicate.Send2GS("CMD_SHARE_SUCCESS", PackArgs());
            return true;
        }
    }
}
