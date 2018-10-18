/// <summary>
/// Created by zhaozy 2017/07/13
/// 提交报错日志
/// </summary>

using System;
using System.Collections.Generic;
using System.Text;
using LPC;

public partial class Operation
{
    public class CmdErrorLog : CmdHandler
    {
        public string GetName()
        {
            return "cmd_error_log";
        }

        // 提交报错日志
        public static bool Go(string log)
        {
            if (! Communicate.IsConnectedGS())
                return false;

            // 向服务器发送消息
            Communicate.Send2GS("CMD_ERROR_LOG",
                PackArgs("log", LPCValue.Create(Zlib.Compress(Encoding.UTF8.GetBytes(log)))));

            return true;
        }
    }
}
