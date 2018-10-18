using LPC;

public partial class Operation
{
    public class CmdSummonPet : CmdHandler
    {
        public string GetName()
        {
            return "cmd_summon_pet";
        }

        // 召唤宠物指令
        public static bool Go(int type, int times, LPCMapping args)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 正在等待服务器的回执消息
            if (VerifyCmdMgr.IsVerifyCmd("CMD_SUMMON_PET"))
                return false;

            // 构建消息参数
            LPCValue cmdArgs = PackArgs("type", type, "times", times, "args", args, "cookie", Rid.New());

            // 添加缓存等待消息
            VerifyCmdMgr.AddVerifyCmd("CMD_SUMMON_PET", "MSG_SUMMON_PET", cmdArgs);

            // 通知服务器召唤
            Communicate.Send2GS("CMD_SUMMON_PET", cmdArgs);
            return true;
        }
    }
}
