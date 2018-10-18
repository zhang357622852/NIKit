/// <summary>
/// CmdUnEquip.cs
/// Created by xuhd Mar/16/2015
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdUnEquip : CmdHandler
    {
        public string GetName()
        {
            return "cmd_unequip";
        }

        public static bool Go(string targetRid, string equipRid)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 通知服务器卸载装备道具
            Communicate.Send2GS("CMD_UNEQUIP", PackArgs(
                    "target_rid", targetRid,
                    "item_rid", equipRid));

            return true;
        }
    }
}