using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// 设置系统设置
    /// </summary>
    /// <author>weism</author>
    public class CmdSetOption : CmdHandler
    {
        public string GetName()
        {
            return "cmd_set_option";
        }

        /// <summary>
        /// 系统设置消息入口
        /// </summary>
        /// <param name="optionName">系统设置属性名</param>
        /// <param name="value">属性值</param>
        public static bool Go(string optionName, LPCValue value)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            Communicate.Send2GS("CMD_SET_OPTION", PackArgs("option_name", optionName, "value", value));
            return true;
        }
    }
}
