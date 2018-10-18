/// <summary>
/// CmdAddComment.cs
/// 发布评论
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdAddComment
    {
        public string GetName()
        {
            return "cmd_add_comment";
        }

        public static bool Go(int classID, string comment, bool share)
        {
            // 通知服务器发布评论
            Communicate.Send2GS("CMD_ADD_COMMENT", PackArgs(
                "class_id", classID,
                "comment", comment,
                "share", share ? 1 : 0
            ));
            
            return true;
        }
    }
}
