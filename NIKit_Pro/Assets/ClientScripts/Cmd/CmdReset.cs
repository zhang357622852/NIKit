using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// 重新登录
    /// </summary>
    public class CmdReset : CmdHandler
    {
        public string GetName()
        {
            return "cmd_reset";
        }

        public static bool Go()
        {
            // 正在等待服务器的回执消息
            if (VerifyCmdMgr.IsVerifyCmd("CMD_RESET"))
                return false;

            // 构建消息参数
            LPCValue cmdArgs = PackArgs("cookie", Rid.New());

            // 添加缓存等待消息
            VerifyCmdMgr.AddVerifyCmd("CMD_RESET", "MSG_RESET", cmdArgs);

			// 发送消息
            Communicate.Send2GS("CMD_RESET", cmdArgs);
            return true;
        }
    }
}
