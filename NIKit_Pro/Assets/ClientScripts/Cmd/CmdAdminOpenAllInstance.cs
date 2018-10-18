/// <summary>
/// CmdAdminOpenAllInstance.CS
/// Created by fengsc by 2017/06/05
/// GM 功能开启所有副本
/// </summary>
using UnityEngine;
using System.Collections;

public partial class Operation
{
    public class CmdAdminOpenAllInstance : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_open_all_instance";
        }

        public static bool Go(int is_open)
        {
            Communicate.Send2GS("CMD_ADMIN_OPEN_ALL_INSTANCE", PackArgs("is_open", is_open));
            return true;
        }
    }
}
