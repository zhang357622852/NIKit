/// <summary>
/// CmdAddPraise.cs
/// 评论点赞
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdAddPraise
    {
        public string GetName()
        {
            return "cmd_add_praise";
        }

        public static bool Go(string rid)
        {
            // 通知服务器评论点赞
            Communicate.Send2GS("CMD_ADD_PRAISE", PackArgs("rid", rid));
            return true;
        }
    }
}
