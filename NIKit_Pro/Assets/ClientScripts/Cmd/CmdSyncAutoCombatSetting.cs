/// <summary>
/// CmdSyncAutoCombatSetting.cs
/// Created by xuhd Apr/11/2015
/// 同步自动战斗设置到服务器
/// </summary>
using LPC;

public partial class Operation
{
    public class CmdSyncAutoCombatSetting : CmdHandler
    {
        public string GetName()
        {
            return "cmd_sync_auto_combat_setting";
        }

        /// <summary>
        /// 同步自动战斗设置.
        /// </summary>
        /// <param name="settings">Settings.</param>
        public static bool Go(LPCMapping settings)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            Communicate.Send2GS("CMD_SYNC_AUTO_COMBAT_SETTING", PackArgs("settings_map", settings));

            LogMgr.Trace("[CmdSyncAutoCombatSetting.cs] 同步自动战斗设置到服务器 auto_combat = {0}", settings._GetDescription(4));
            return true;
        }
    }
}
