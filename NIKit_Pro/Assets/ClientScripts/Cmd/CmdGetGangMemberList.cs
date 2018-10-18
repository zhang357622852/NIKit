/// <summary>
/// CmdGetGangMemberList.cs
/// 获取公会成员列表
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdGetGangMemberList
    {
        public string GetName()
        {
            return "cmd_get_gang_member_list";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(string relationTag)
        {
            // 通知服务器获取公会成员列表
            Communicate.Send2GS("CMD_GET_GANG_MEMBER_LIST", PackArgs("relation_tag", relationTag));

            return true;
        }
    }
}
