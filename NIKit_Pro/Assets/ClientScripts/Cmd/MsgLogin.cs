using LPC;

/// <summary>
/// 通知登录结果
/// </summary>
public class MsgLogin : MsgHandler
{
    public string GetName()
    {
        return "msg_login";
    }

    public void Go(LPCValue para)
    {
        // 取得服务器数据
        LPCMapping args = para.AsMapping;
        if (args.Count == 0)
            return;

        // 记录服务器当前的时间
        Communicate.SetServerStartTime(args ["server_time"].AsString, args ["start_time"].AsInt);

        LogMgr.Trace("[MsgLogin.cs] 登录结果:{0} 提示: {1}", args ["succ"].GetDescription(), args ["msg"].GetDescription());

        // 登录成功了，选择1线登录
        if (args ["succ"].AsInt == 1)
        {
            // 选择线路
            Operation.CmdSelectLoginThread.Go(1);
        } else
        {
            // Alart.Show(args["msg"].AsString);
            // 断开连接
            Communicate.Disconnect();
        }
    }
}
