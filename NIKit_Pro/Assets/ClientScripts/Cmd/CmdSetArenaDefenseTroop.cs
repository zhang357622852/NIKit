using LPC;

public partial class Operation
{
    public class CmdSetArenaDefenseTroop : CmdHandler
    {
        public string GetName()
        {
            return "cmd_set_arena_defense_troop";
        }

        /// <summary>
        /// Go the specified troopList.
        /// </summary>
        public static bool Go(LPCArray troopList)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            Communicate.Send2GS("CMD_SET_ARENA_DEFENSE_TROOP", PackArgs("troop_list", troopList));
            return true;
        }
    }
}
