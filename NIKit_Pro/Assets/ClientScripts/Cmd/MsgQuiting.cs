using LPC;

/// <summary>
/// 正在退出游戏
/// </summary>
public class MsgQuiting : MsgHandler
{
    /// <summary>
    /// 处理器的名字
    /// </summary>
    /// <returns>The name.</returns>
    public string GetName()
    {
        return "msg_quiting";
    }

    /// <summary>
    /// 入口
    /// </summary>
    /// <param name="para">Para.</param>
    public void Go(LPCValue para)
    {
        LPCMapping data = para.AsMapping;

        // 如果角色不在游戏中不处理
        if (!ME.isInGame)
            return;

        // 冻结账号退出游戏
        if (data.GetValue<int>("type").Equals(QuitConst.BLOCK_QUIT))
        {
            // 提示玩家，账号冻结
            DialogMgr.ShowSimpleSingleBtnDailog(new CallBack(OnDialogCallBack), LocalizationMgr.Get("16"));

            // 如果玩家正在副本中
            if (InstanceMgr.IsInInstance(ME.user))
            {
                // 暂停游戏
                TimeMgr.DoPauseCombatLogic("CombatSetPause");
            }
        }
        else
        {
            // 返回到登陆界面
            LoginMgr.ExitGame();
        }
    }

    /// <summary>
    /// 确认弹框回调
    /// </summary>
    void OnDialogCallBack(object para, params object[] param)
    {
        // 返回到登陆界面
        LoginMgr.ExitGame();
    }
}
