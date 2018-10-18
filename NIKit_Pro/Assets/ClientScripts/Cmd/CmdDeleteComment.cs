/// <summary>
/// CmdDeleteComment.cs
/// 删除评论
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdDeleteComment
    {
        public string GetName()
        {
            return "cmd_delete_comment";
        }

        public static bool Go(string rid)
        {
            // 通知服务器删除评论
            Communicate.Send2GS("CMD_DELETE_COMMENT", PackArgs("rid", rid));
            return true;
        }
    }
}
