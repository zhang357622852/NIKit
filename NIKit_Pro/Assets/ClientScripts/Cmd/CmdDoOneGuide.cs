/// <summary>
/// Created by xuhd Jan/22/2015
/// 做完一个指引
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdDoOneGuide : CmdHandler
    {
        public string GetName()
        {
            return "cmd_do_one_guide";
        }

        public static bool Go(int group, int guideId)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 正在等待服务器的回执消息
            if (VerifyCmdMgr.IsVerifyCmd("CMD_DO_ONE_GUIDE"))
                return false;

            // 构建消息参数
            LPCValue cmdArgs = PackArgs(
                "group", group,
                "guide_id", guideId,
                "cookie", Rid.New()
            );

            // 添加缓存等待消息
            VerifyCmdMgr.AddVerifyCmd("CMD_DO_ONE_GUIDE", "MSG_DO_ONE_GUIDE", cmdArgs);

            // 通知完成指引
            Communicate.Send2GS("CMD_DO_ONE_GUIDE", cmdArgs);

            // 返回true
            return true;
        }
    }
}
