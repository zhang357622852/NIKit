/// <summary>
/// CmdCreateGang.cs
/// 创建帮派
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdCreateGang
    {
        public string GetName()
        {
            return "cmd_create_gang";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        /// <param name="gangName">Gang name.</param>
        /// <param name="flag">Flag.</param>
        /// <param name="introduce">Introduce.</param>
        /// <param name="condition">Condition.</param>
        public static bool Go(string gangName, LPCArray flag, string introduce, LPCMapping condition)
        {
            // 通知服务器创建工会
            Communicate.Send2GS("CMD_CREATE_GANG", PackArgs(
                "gang_name", gangName,
                "gang_flag", flag,
                "introduce", introduce,
                "condition", condition
            ));

            return true;
        }
    }
}
