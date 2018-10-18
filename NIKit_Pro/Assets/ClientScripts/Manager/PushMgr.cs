/// <summary>
/// PushMgr.cs
/// Created by lic 2017-09-25
/// 推送管理
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using QCCommonSDK.Addition;
using LPC;

/// <summary>
/// 玩家管理
/// </summary>
public static class PushMgr
{
    #region 变量

    #endregion

    #region 属性

    #endregion

    #region 内部接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 注册玩家登陆成功
        EventMgr.UnregisterEvent("PushMgr");
        EventMgr.RegisterEvent("PushMgr", EventMgrEventType.EVENT_LOGIN_OK, WhenLoginOk);
    }

    /// <summary>
    /// 登陆成功回调
    /// </summary>
    private static void WhenLoginOk(int eventId, MixedValue para)
    {
        ChangeLifeFullPush(false);
    }

    // 改变体力满时的推送状态
    private static void ChangeLifeFullPush(bool isOutGame)
    {
        // 对象不存在
        if (ME.user == null)
            return;

        if (isOutGame)
        {
            // 获取玩家当前体力值;
            int mPower = ME.user.Query<int>("life");

            // 获取玩家体力值上限;
            int maxPower = ME.user.Query<int>("max_life");

            // 当前体力是满的
            if (mPower >= maxPower)
                return;

            // 疲劳恢复的时间间隔(单位为秒)
            int timeSpace = CALC_RECOVER_LIFE_INTERVAL.Call(ME.user);

            // 恢复量
            int baseValue = CALC_RECOVER_BASE_VALUE.Call(ME.user);

            int space = (maxPower - mPower)/baseValue;

            // 计算恢复体力满所需时间(单位为秒)
            int pushTime = (((maxPower - mPower)%baseValue == 0) ? space : (space + 1))*timeSpace;

            // 增加本地推送消息
            AddLocalNotify(PushConst.LIFE_FULL.ToString(), pushTime, LocalizationMgr.Get("PushMgr_1"));
        }
        else
        {
            // 取消某个本地推送
            CancelLocalNotify(PushConst.LIFE_FULL.ToString());
        }
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 应用程序切换到前段/后台
    /// </summary>
    /// <param name="pauseStatus">If set to <c>true</c> pause status.</param>
    public static void OnAppStatusChange(bool pauseStatus)
    {
        ChangeLifeFullPush(pauseStatus);
    }

    /// <summary>
    /// 当游戏退出游戏时调用
    /// </summary>
    public static void WhenUserQuitGame()
    {
        ChangeLifeFullPush(true);
    }

    /// <summary>
    /// 关闭推送功能（包括本地的推送和服务器的推送）
    /// 目前jpush只有android支持开启或关闭推送
    /// </summary>
    public static void OpenAndClosePush(bool isOpen)
    {
        if (isOpen)
            PushSupport.OpenPush();
        else
            PushSupport.ClosePush();
    }

    /// <summary>
    /// 取消所有离线提醒.
    /// </summary>
    public static void CancelAllLocalNotify()
    {
        PushSupport.CancelAlllocalNotify();
    }

    /// <summary>
    /// 取消某个即将推送的离线提醒
    /// </summary>
    public static void CancelLocalNotify(string notifyId)
    {
        PushSupport.CancellocalNotify(notifyId);
    }

    /// <summary>
    /// 添加一个本地离线提醒
    /// </summary>
    /// ******注意事项******
    /// style:ios不支持自定义推送样式，style只对android有效，0表示默认样式
    /// icon:只对android有效(icon为空默认为使用系统自带图标)，ios当前不支持自定义icon。
    /// title:android:默认样式下，title为空，推送界面直接显示当前应用程序名称,否者替换应用程序名称。
    /// title：ios，注意此处与android不同，ios始终会显示应用名称，title（包括subtitle）只是显示desc上面的两个单独分行的小标题而已。
    /// subtitle:subTitle只对ios有效,android不能设置subtitle。
    public static void AddLocalNotify(string notifyId, long delay, string desc, string title = "",
        string subTitle = "", string icon = "", int style = 0)
    {
        PushSupport.LocalNotify(notifyId, delay, desc, title, subTitle, icon, style);
    }

    #endregion
}
