using LPC;

public partial class Operation
{
    public class CmdDrop : CmdHandler
    {
        public string GetName()
        {
            return "cmd_drop";
        }

        // 丢弃道具物品指令
        // 指令包格式：string rid, int32 amount
        public static bool Go(string rid, int amount)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            LPCValue m = PackArgs("rid", rid, "amount", amount);
            Communicate.Send2GS("CMD_DROP", m);
            return true;
        }
    }
}
