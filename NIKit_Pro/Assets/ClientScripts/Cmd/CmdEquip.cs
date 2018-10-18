/// <summary>
/// Created by xuhd Mar/13/2015.
/// 更换装备
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdEquip : CmdHandler
    {
        public string GetName()
        {
            return "cmd_equip";
        }

        public static bool Go(string targetRid, string equipRid)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 通知服务器装备道具
            Communicate.Send2GS("CMD_EQUIP", PackArgs(
                    "target_rid", targetRid,
                    "item_rid", equipRid));

            return true;
        }
    }
}