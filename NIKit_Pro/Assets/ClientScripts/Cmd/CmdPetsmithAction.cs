using LPC;

public partial class Operation
{
    public class CmdPetsmithAction : CmdHandler
    {
        public string GetName()
        {
            return "cmd_petsmith_action";
        }

        /// <summary>
        /// 宠物强化
        /// </summary>
        public static bool Go(string action, LPCMapping extra_para)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 正在等待服务器的回执消息
            if (VerifyCmdMgr.IsVerifyCmd("CMD_PETSMITH_ACTION"))
                return false;

            // 构建消息参数
            LPCValue cmdArgs = PackArgs(
                "action", action,
                "extra_para", extra_para,
                "cookie", Rid.New()
            );

            // 添加缓存等待消息
            VerifyCmdMgr.AddVerifyCmd("CMD_PETSMITH_ACTION", "MSG_PETSMITH_ACTION", cmdArgs);

            // 发送消息到服务器
            Communicate.Send2GS("CMD_PETSMITH_ACTION", cmdArgs);

            return true;
        }
    }
}
