/// <summary>
/// CmdQueryComments.cs
/// 获取评论信息
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdQueryComments
    {
        public string GetName()
        {
            return "cmd_query_comments";
        }

        public static bool Go(int classId, int type, string cookie, int startPos)
        {
            // 通知服务器获取评论信息
            Communicate.Send2GS("CMD_QUERY_COMMENTS", PackArgs(
                "class_id", classId,
                "type", type,
                "cookie", cookie,
                "start_pos", startPos));
            return true;
        }
    }
}
