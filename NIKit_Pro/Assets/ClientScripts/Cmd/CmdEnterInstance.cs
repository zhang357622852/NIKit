/// <summary>
/// Created by xuhd Jan/22/2015
/// 进入副本
/// </summary>
using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// 角色进入副本指令
    /// </summary>
    public class CmdEnterInstance : CmdHandler
    {
        public string GetName()
        {
            return "cmd_enter_instance";
        }

        /// <summary>
        /// Go the specified instanceId, fighterList and formationId.
        /// </summary>
        public static bool Go(string instanceId, string leaderRid, LPCMapping formationMap, LPCMapping extraPara)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 正在等待服务器的回执消息
            if (VerifyCmdMgr.IsVerifyCmd("CMD_ENTER_INSTANCE"))
                return false;

            // 构建消息参数
            LPCValue cmdArgs = PackArgs(
                                   "instance_id", instanceId,
                                   "leader_rid", leaderRid,
                                   "formation_map", formationMap,
                                   "extra_para", extraPara,
                                   "cookie", Rid.New());

            // 添加缓存等待消息
            VerifyCmdMgr.AddVerifyCmd("CMD_ENTER_INSTANCE", "MSG_ENTER_INSTANCE", cmdArgs);

            // 向服务器发送消息
            Communicate.Send2GS("CMD_ENTER_INSTANCE", cmdArgs);
            return true;
        }
    }
}