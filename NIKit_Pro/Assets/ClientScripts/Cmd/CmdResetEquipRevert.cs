/// <summary>
/// CmdResetEquipRevert.cs
/// Created by cql 2015/07/17
/// 退回装备重置结果
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdResetEquipRevert : CmdHandler
    {
        public string GetName()
        {
            return "cmd_reset_equip_revert";
        }

        public static bool Go(string rid, int page)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            LogMgr.Trace("[CmdResetEquipRevert.cs] 请求退回重置装备结果。");
            Communicate.Send2GS("CMD_RESET_EQUIP_REVERT", PackArgs("equip_rid", rid, "page", page));
            return true;
        }
    }
}
