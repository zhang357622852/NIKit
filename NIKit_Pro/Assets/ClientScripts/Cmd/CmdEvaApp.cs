using LPC;

public partial class Operation
{
    public class CmdEvaApp : CmdHandler
    {
        public string GetName()
        {
            return "cmd_eva_app";
        }

        /// <summary>
        /// 通知玩家评价app
        /// </summary>
        public static bool Go(LPCMapping para)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            Communicate.Send2GS("CMD_EVA_APP", PackArgs("para", para));
            return true;
        }
    }
}
