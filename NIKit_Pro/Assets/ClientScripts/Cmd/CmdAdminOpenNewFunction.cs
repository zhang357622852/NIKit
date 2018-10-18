/// <summary>
/// GD功能开启新功能
/// </summary>
using LPC;

public partial class Operation
{
    public class CmdAdminOpenNewFunction : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_open_new_function";
        }

        public static bool Go(string new_func_name)
        {
            Communicate.Send2GS("CMD_ADMIN_OPEN_NEW_FUNCTION", PackArgs("new_func_name", new_func_name));
            return true;
        }
    }
}
