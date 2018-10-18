/// <summary>
/// CmdRequestJoinGang.cs
/// 请求加入公会
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdRequestJoinGang
    {
        public string GetName()
        {
            return "cmd_request_join_gang";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(string relationTag)
        {
            // 通知服务器请求加入公会
            Communicate.Send2GS("CMD_REQUEST_JOIN_GANG", PackArgs(
                "relation_tag", relationTag
            ));

            return true;
        }
    }
}
