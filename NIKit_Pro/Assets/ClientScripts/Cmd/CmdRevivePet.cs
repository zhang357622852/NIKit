/// <summary>
/// Created bu fengsc 2016/08/05
/// 副本复活
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public partial class Operation
{
    public class CmdRevivePet : CmdHandler
    {
        public string GetName()
        {
            return "cmd_revive_pet";
        }

        public static bool Go()
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 正在等待服务器的回执消息
            if (VerifyCmdMgr.IsVerifyCmd("CMD_REVIVE_PET"))
                return false;

            // 构建消息参数
            LPCValue cmdArgs = PackArgs("cookie", Rid.New());

            // 添加缓存等待消息
            VerifyCmdMgr.AddVerifyCmd("CMD_REVIVE_PET", "MSG_REVIVE_PET", cmdArgs);

            // 通知服务器复活宠物
            Communicate.Send2GS("CMD_REVIVE_PET", cmdArgs);

            // 返回成功
            return true;
        }
    }
}
