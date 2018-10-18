using UnityEngine;
using System.Collections;

public partial class Operation
{
    public class CmdAdminFinishGuide : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_finish_guide";
        }

        public static bool Go()
        {
            Communicate.Send2GS("CMD_ADMIN_FINISH_GUIDE", PackArgs());
            return true;
        }
    }
}
