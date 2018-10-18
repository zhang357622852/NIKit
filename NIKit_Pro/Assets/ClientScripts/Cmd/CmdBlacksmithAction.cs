using LPC;

public partial class Operation
{
    public class CmdBlacksmithAction : CmdHandler
    {
        public string GetName()
        {
            return "cmd_blacksmith_action";
        }

        /// <summary>
        /// 装备熔炼
        /// </summary>
        public static bool Go(string action, LPCMapping extra_para)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 正在等待服务器的回执消息
            if (VerifyCmdMgr.IsVerifyCmd("CMD_BLACKSMITH_ACTION"))
                return false;

            // 构建消息参数
            LPCValue cmdArgs = PackArgs(
                "action", action,
                "extra_para", extra_para,
                "cookie", Rid.New());

            // 添加缓存等待消息
            VerifyCmdMgr.AddVerifyCmd("CMD_BLACKSMITH_ACTION", "MSG_BLACKSMITH_ACTION", cmdArgs);

            Communicate.Send2GS("CMD_BLACKSMITH_ACTION", cmdArgs);

            return true;
        }
    }
}
