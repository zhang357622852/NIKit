/// <summary>
/// ME.cs
/// Copy from zhangyg 2014-10-22
/// 维护玩家自己的数据
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using LPC;

public class ME
{
    #region 变量

    /// <summary>
    /// dbase数据
    /// </summary>
    public static Dbase dbase = new Dbase();

    /// <summary>
    /// 玩家对应的USER对象
    /// </summary>
    public static User user = null;

    #endregion

    #region 属性

    /// <summary>
    /// 是否正在游戏中
    /// </summary>
    private static bool _isInGame = false;

    public static bool isInGame
    {
        get { return _isInGame && ME.user != null; }
        set { _isInGame = value; }
    }

    /// <summary>
    /// 是否已经成功登陆
    /// </summary>
    private static bool _isLoginOk = false;

    public static bool isLoginOk
    {
        get { return _isLoginOk && ME.user != null; }
        set { _isLoginOk = value; }
    }

    /// <summary>
    /// 是否正在登出游戏中
    /// </summary>
    private static bool mIsLogouting = false;

    public static bool isLogouting
    {
        get { return mIsLogouting && ME.user != null; }
        set { mIsLogouting = value; }
    }

    #endregion

    #region 内部接口

    static ME()
    {
        user = null;
        isInGame = false;
        isLoginOk = false;
    }

    /// <summary>
    /// 打开登陆界面
    /// </summary>
    private static void OnEnterMainScene(object para, object[] param)
    {
        // 尝试恢复指引信息，这个地方有两种情况指引需要处理
        // 1. 继续上一次指引
        // 2. 第一次登陆指引
        // 如果有指引需要处理，则不在需要播放闪屏界面
        if (GuideMgr.DoLoginGuide())
            return;

        // 打开闪屏界面
        WindowMgr.OpenWnd("WhiteFlash");
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 取得我的rid
    /// </summary>
    public static string GetRid()
    {
        if (user == null)
            return null;

        return user.GetRid();
    }

    /// <summary>
    /// 玩家进入房间
    /// </summary>
    public static void EnteredRoom(string pos, int direction, LPCValue roomInfo)
    {
        if (user == null)
        {
            // Alart.Show ("通讯文件版本不匹配，登录失败。");
            LogMgr.Trace("通讯文件版本不匹配，登录失败。");
            return;
        }

        // 更新主角的坐标
        user.move.SetPos(pos);
        user.dbase.Set("direction", direction);
    }

    /// <summary>
    /// 获取登录数据完毕了
    /// </summary>
    public static void LoginNotifyOK()
    {
        // 移除恢复登陆等待状态
        WaitWndMgr.RemoveWaitWnd("recover_login", WaitWndType.WAITWND_TYPE_RELOGIN);

        // 如果玩家对象存在
        if (user == null)
            return;

        LogMgr.Trace("用户({0})进入了游戏", user.GetName());

        // 标记玩家正常游戏
        isInGame = true;

        // 如果已经在游戏中不处理，否则需要加载主场景
        if (isLoginOk)
            return;

        // 载入场景
        SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OnEnterMainScene));
    }

    /// <summary>
    /// 登陆成功
    /// </summary>
    public static void OnLoginOK()
    {
        // 登陆游戏成功
        isLoginOk = true;

        // 隐藏等待窗口
        WaitWndMgr.RemoveWaitWnd("LogIn", WaitWndType.WAITWND_TYPE_LOGIN);

        // 打开主场景窗口
        WindowMgr.OpenMainWnd();

        // 抛出玩家登陆成功事件
        LPCMapping eventPara = new LPCMapping();
        EventMgr.FireEvent(EventMgrEventType.EVENT_LOGIN_OK, MixedValue.NewMixedValue<LPCMapping>(eventPara));
    }

    /// <summary>
    /// 通知玩家退出游戏
    /// </summary>
    public static void StopGame()
    {
        // 重置标识
        isInGame = false;
        isLoginOk = false;
        isLogouting = false;

        //  结束当前副本
        InstanceMgr.LeaveInstance(ME.user, true);

        // 清除等待确认消息
        VerifyCmdMgr.DoResetAll();

        // 清除战斗系统
        CombatRootMgr.CleanAll();

        // 重置邮件系统
        MailMgr.DoResetAll();

        // 重置好友数据
        FriendMgr.DoResetAll();

        // 重置公会数据
        GangMgr.DoResetAll();

        // 重置视频相关数据
        VideoMgr.DoResetAll();

        // 重置聊天信息
        ChatRoomMgr.ClearChatMessage();

        // 开启推送
        PushMgr.WhenUserQuitGame();

        // 重置指引
        GuideMgr.ResetGuide();

        // 重置提示窗口
        WindowTipsMgr.DoResetAll();

        // 停止协成
        Coroutine.StopCoroutine("EnterSceneCoroutine");

        // 在离开游戏时 - 清除所有运行中对象
        foreach (Property ob in new List<Property>(Rid.objects.Values))
        {
            // 对象不存在
            if (ob == null)
                continue;

            // 销毁对象
            ob.Destroy();
        }

        // user对象设置为null
        ME.user = null;
    }

    /// <summary>
    /// 通知玩家开始游戏
    /// </summary>
    public static void StartGame()
    {
        LogMgr.Trace("[ME.cs] 你开始进行游戏");
    }

    #endregion
}
