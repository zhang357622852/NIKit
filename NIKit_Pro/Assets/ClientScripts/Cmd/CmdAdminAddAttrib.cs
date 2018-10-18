/// <summary>
/// CmdAdminAddAttrib.cs
/// Created by fengsc 2017/05/25
/// GM添加玩家属性
/// </summary>
using LPC;

public partial class Operation
{
    public class CmdAdminAddAttrib : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_add_attrib";
        }

        public static bool Go(LPCMapping attrib)
        {
            Communicate.Send2GS("CMD_ADMIN_ADD_ATTRIB", PackArgs("attrib", attrib));
            return true;
        }
    }
}
