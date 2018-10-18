/// <summary>
/// CmdResetEquipApply.cs
/// Created by cql 2015/07/17
/// 保留装备重置结果
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdResetEquipApply : CmdHandler
    {
        public string GetName()
        {
            return "cmd_reset_equip_apply";
        }

        public static bool Go(string rid, int page)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            LogMgr.Trace("[CmdResetEquipApply.cs] 请求保留重置装备结果。");
            Communicate.Send2GS("CMD_RESET_EQUIP_APPLY", PackArgs("equip_rid", rid, "page", page));
            return true;
        }
    }
}
