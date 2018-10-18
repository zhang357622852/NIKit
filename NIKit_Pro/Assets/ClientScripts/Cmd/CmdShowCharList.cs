using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// 载入角色指令
    /// </summary>
    public class CmdShowCharList : CmdHandler
    {
        public string GetName()
        {
            return "cmd_show_char_list";
        }

        public static bool Go()
        {
            LogMgr.Trace("[CmdShowCharList.cs] 登录成功，取得帐号下的角色列表。");
            Communicate.Send2GS("CMD_SHOW_CHAR_LIST", PackArgs());
            return true;
        }
    }
}