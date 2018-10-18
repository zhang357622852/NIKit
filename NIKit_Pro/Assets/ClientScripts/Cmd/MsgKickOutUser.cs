using LPC;
using System.Diagnostics;

/// <summary>
/// 通知玩家被踢下线（被顶号）
/// </summary>
/// <author>weism</author>
public class MsgKickOutUser : MsgHandler
{
    public string GetName()
    {
        return "msg_kick_out_user";
    }

    public void Go(LPCValue para)
    {
        // 转换消息参数
        LPCMapping args = para.AsMapping;

        // 获取auth_key和seed数据
        int authKey = args.GetValue<int>("auth_key");
        int seed = args.GetValue<int>("seed");

        // 取得帐号信息
        Dbase accountInfo = Communicate.AccountInfo;

        // 正常的登录流程，一定会有authKey和seed
        int accountAuthKey = accountInfo.Query("auth_key", 0);
        int accountSeed = accountInfo.Query("seed", 0);

        // 自己发起登陆顶号不处理, 否则需要做下线处理
        if (authKey == accountAuthKey && seed == accountSeed)
            return;

        // 踢玩家下线
        LoginMgr.KickOut();
    }
}
