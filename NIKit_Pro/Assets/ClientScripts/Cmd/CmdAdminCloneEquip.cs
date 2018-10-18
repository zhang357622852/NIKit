/// <summary>
/// CmdAdminCloneItem.cs
/// Created by fengsc 2015/05/26
/// GM克隆装备宠物
/// </summary>
using LPC;

public partial class Operation
{
    public class CmdAdminCloneEquip : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_clone_equip";
        }

        public static bool Go(int amount, LPCMapping para)
        {
            Communicate.Send2GS("CMD_ADMIN_CLONE_EQUIP", PackArgs("amount", amount, "para", para));
            return true;
        }
    }
}
