/// <summary>
/// CmdAdminSetDaliyBonus.cs
/// Created by cql 2015/04/28
/// 设置签到奖励窗口
/// </summary>
using System;
using LPC;

/// <summary>
/// 设置签到奖励状态.
/// </summary>
public partial class Operation
{
    public class CmdAdminSetDaliyBonus : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_set_daliy_bonus";
        }

        public static bool Go(bool isDaliyBonusOn)
        {
            Communicate.Send2GS("CMD_ADMIN_SET_DALIY_BONUS", PackArgs("is_daliy_bonus_on", isDaliyBonusOn ? 1 : 0));
            return true;
        }
    }
}
