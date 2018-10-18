/// <summary>
/// CmdApCmdQueryCommentSharePetply.cs
/// 获取分享宠物
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdQueryCommentSharePet
    {
        public string GetName()
        {
            return "cmd_query_comment_share_pet";
        }

        public static bool Go(string userRid, int classId)
        {
            // 通知服务器获取分享宠物
            Communicate.Send2GS("CMD_QUERY_COMMENT_SHARE_PET", PackArgs("user_rid", userRid, "class_id", classId));
            return true;
        }
    }
}
