/// <summary>
/// AuthClientMgr.cs
/// Create by zhaozy 2017-05-04
/// 验证客户端管理模块
/// </summary>

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using QCCommonSDK;
using System.Threading;
using LPC;

/// 排行榜管理器
public static class AuthClientMgr
{
    /// <summary>
    /// 最大内存限制
    /// 256 * 1024 * 1024;
    /// </summary>
    private static long MaxMemory = 268435456;

    #region 属性

    /// <summary>
    /// 是否是验证客户端
    /// </summary>
    public static bool IsAuthClient
    {
        get
        {
            return ConfigMgr.Get<bool>("auth_client", false);
        }
    }

    #endregion

    #region 功能接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 关闭消息确认机制
        Communicate.GSConnector.GetConnect().CloseMsgVerify = true;

        // 协程中自动登陆
        Coroutine.DispatchService(DoLogin());
    }

    /// <summary>
    /// 验证战斗
    /// </summary>
    public static void DoAuthCombat(LPCMapping para)
    {
        // 开始战斗
        Coroutine.DispatchService(AuthCombat(para));
    }

    #endregion

    #region 内部方法

    /// <summary>
    /// 回收资源
    /// </summary>
    private static IEnumerator Recycle()
    {
        // 回收一下资源
        ResourceMgr.DoRecycleGC();

        // 获取当前内存消耗
        long totalMem = GC.GetTotalMemory(false);

        // 如果内存泄露已经达到了256M, 则需要通知服务器关闭自己
        if (totalMem < MaxMemory)
            yield break;

        // 内存超过256MB，关闭自己
        if (! Communicate.IsConnectedGS())
            Application.Quit();
        else
            Operation.CmdCCPrepareShutdown.Go();
    }

    /// <summary>
    /// 验证战斗
    /// </summary>
    private static IEnumerator AuthCombat(LPCMapping para)
    {
#if UNITY_EDITOR
        // 获取验证驱动开始时间
        DateTime startTime = DateTime.Now;
#endif

        // 获取参数
        string combatRid = para.GetValue<string>("combat_rid");
        string instanceId = para.GetValue<string>("instance_id");
        int randomSeed = para.GetValue<int>("random_seed");

        // 构建副本参数
        LPCMapping dbase = new LPCMapping();
        dbase.Add("rid", combatRid);
        dbase.Add("instance_id", instanceId);
        dbase.Add("random_seed", randomSeed);
        dbase.Add("fighter_map", para["fighter_map"]);
        dbase.Add("defenders", para["defenders"]);
        dbase.Add("revive_times", para["revive_times"]);
        dbase.Add("level_actions", para["level_actions"]);
        dbase.Append(para.GetValue<LPCMapping>("extra_para"));

        // 如果是通天塔副本需要增加通天塔难度和层级相关数据
        CsvRow data = TowerMgr.GetTowerInfoByInstance(instanceId);
        if (data != null)
        {
            dbase.Add("difficulty", data.Query<int>("difficulty"));
            dbase.Add("layer", data.Query<int>("layer"));
        }

        // 创建副本对象
        AuthInstance instanceOb = InstanceMgr.DoCreateAuthInstance(instanceId, dbase);

        // 创建副本失败
        if (instanceOb == null)
        {
            // 副本创建失败，直接通知服务器验证失败
            Operation.CmdACAuthResult.Go(combatRid, false);
            yield break;
        }

        // 副本开始
        instanceOb.DoStart();

        // 等待验证结束
        while(! instanceOb.IsAuthEnd)
        {
            // 驱动一次
            Coroutine.StepFixedOnce();
            // yield return null;
        }

        // 副本验证结束
        instanceOb.DoEnd();

        // 自动回收一下资源, 同时检查一下内存是否达到了最大限制
        Coroutine.DispatchService(Recycle());

#if UNITY_EDITOR
        LogMgr.Trace("战斗[{0}] 验证时间 {1} 毫秒", instanceOb.GetRid(),
                    (int) (DateTime.Now - startTime).TotalMilliseconds);
#endif
    }

    /// <summary>
    /// 确保与战斗服务器的连接没有断掉
    /// </summary>
    private static IEnumerator DoLogin()
    {
        // 验证客户端登陆ip端口默认值
        string ip = "127.0.0.1";
        int port = 9001;
        string cookie = string.Empty;
        string owner = string.Empty;

        // 获取启动参数
        string[] args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            // 获取owner
            if (args[i] == "-owner" && i + 1 < args.Length)
            {
                owner = args[i + 1];
                continue;
            }

            // 获取登陆ip
            if (args[i] == "-login_ip" && i + 1 < args.Length)
            {
                ip = args[i + 1];
                continue;
            }

            // 获取登陆port
            if (args[i] == "-login_port" && i + 1 < args.Length)
            {
                port = int.Parse(args[i + 1]);
                continue;
            }

            // 获取cookie
            if (args[i] == "-cookie" && i + 1 < args.Length)
            {
                cookie = args[i + 1];
                continue;
            }
        }

        // 1，如果断线自动重连
        // 2，如果断线超过10分钟，退出
        float disTime = 0f;
        while (true)
        {
            // 如果还没有连接服务器则自动连接
            if (! Communicate.IsConnectedGS())
            {
                UnityEngine.Debug.Log(string.Format("战斗客户端登录服务器 ip = {0}, port = {1}, cookie = {2}", ip, port, cookie));

                // 初始化disTime
                if (disTime == 0f)
                    disTime = Time.time;

                // 如果断线10分钟，关闭自己
                if (Time.time - disTime > 10 * 60)
                {
                    // 断线10分钟，关闭自己
                    LogMgr.Trace("无法连接服务器 ip = {0}, port = {1}, cookie = {2}, 关闭战斗客户端", ip, port, cookie);
                    Application.Quit();
                    yield break;
                }

                // 发起登陆请求
                Operation.CmdACLogin.Go(owner, ip, port, cookie);
            }
            else
            {
                disTime = 0f;
            }

            // 等待10s
            yield return new WaitForSeconds(10f);
        }
    }

    #endregion
}
