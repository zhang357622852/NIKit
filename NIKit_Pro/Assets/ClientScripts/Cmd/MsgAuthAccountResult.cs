using LPC;

/// <summary>
/// 通知帐号验证结果
/// </summary>
public class MsgAuthAccountResult : MsgHandler
{
    public string GetName()
    {
        return "msg_auth_account_result";
    }

    public void Go(LPCValue para)
    {
        // 验证结果码
        int result = para.AsMapping ["result"].AsInt;

        // 提示语
        string msg = para.AsMapping ["msg"].AsString;

        // 给出提示信息
        if (! string.IsNullOrEmpty(msg))
        {
            DialogMgr.Notify(LocalizationMgr.GetServerDesc(LPCRestoreString.SafeRestoreFromString(msg)));
        }

        // 记录结果
        Communicate.AccountInfo.Set("account_result", result);
        Communicate.AccountInfo.Set("msg", msg);
        Communicate.AccountInfo.Set("auth_key", para.AsMapping ["auth_key"]);
    }
}
