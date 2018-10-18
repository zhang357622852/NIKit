using LPC;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 负责与AAA和GS的通讯逻辑 
/// </summary>
public class Communicate
{
    // 负责与AAA的连接与响应
    private static NetConnector aaaConnector = new NetConnectorImpl();
    private static NetResponsor aaaResponsor = new NetResponsorImpl(true);

    // 负责与GS的连接与响应
    private static NetConnector gsConnector = new NetConnectorImpl();
    private static NetResponsor gsResponsor = new NetResponsorImpl(false);

    // 从AAA取得的帐号信息和服务器信息
    private static Dbase accountInfo = new Dbase();
    private static Dbase serverInfo = new Dbase();

    // 当前登录的区组、帐号、角色信息
    private static Dictionary<string, object> currInfo = new Dictionary<string, object>();

    // 服务器时间与客户端时间的差
    private static int serverTime = 0;
    private static int zoneTime = 0;
    private static DateTime unityTime = DateTime.Now;

    static Communicate()
    {
        // 初始化连接信息
        aaaResponsor.SetConnector(aaaConnector);
        gsResponsor.SetConnector(gsConnector);
        aaaConnector.GetConnect().Responser = aaaResponsor;
        gsConnector.GetConnect().Responser = gsResponsor;
        
        Coroutine.DispatchService(Update());
    }

    /// <summary>
    /// 判断目前是否连接到服务器 
    /// </summary>
    public static bool IsConnected()
    {
        return IsConnectedAAA() || IsConnectedGS();
    }

    /// <summary>
    /// 判断目前是否连接到AAA
    /// </summary>
    public static bool IsConnectedAAA()
    {
        return aaaConnector.IsConnected();
    }

    /// <summary>
    /// 判断目前是否连接到GS
    /// </summary>
    public static bool IsConnectedGS()
    {
        return gsConnector.IsConnected();
    }

    /// <summary>
    /// AAA的连接者 
    /// </summary>
    public static NetConnector AAAConnector
    {
        get { return aaaConnector; }
    }

    /// <summary>
    /// gs的连接者 
    /// </summary>
    public static NetConnector GSConnector
    {
        get { return gsConnector; }
    }

    /// <summary>
    /// 更新连接器
    /// </summary>
    public static IEnumerator Update()
    {
        while (true)
        {
            AAAConnector.Update();
            GSConnector.Update();
            yield return null;
        }
    }

    /// <summary>
    /// 连接到GS 
    /// </summary>
    public static bool Connect2GS(string ip, int port)
    {
        if (gsConnector.IsConnected())
        {
            LogMgr.Trace("与GS已经处于连接中了，不可重复连接。");
            return false;
        }

        gsConnector.ConnectToServer(ip, port);
        return true;
    }

    /// <summary>
    /// 连接到AAA 
    /// </summary>
    public static bool Connect2AAA(string ip, int port)
    {
        if (aaaConnector.IsConnected())
        {
            LogMgr.Trace("与AAA已经处于连接中了，不可重复连接。");
            return false;
        }

        aaaConnector.ConnectToServer(ip, port);
        return true;
    }

    /// <summary>
    /// 断开连接 
    /// </summary>
    public static void Disconnect()
    {
        if (aaaConnector.IsConnected())
            aaaConnector.DisconnectFromServer();
        if (gsConnector.IsConnected())
            gsConnector.DisconnectFromServer();
    }

    /// <summary>
    /// 取得当前登录的帐号信息 
    /// </summary>
    public static Dbase AccountInfo
    {
        get { return accountInfo; }
    }

    /// <summary>
    /// 当前区组的信息 
    /// </summary>
    public static Dictionary<string, object> CurrInfo
    {
        get { return currInfo; }
    }

    /// <summary>
    /// 取服务器信息 
    /// </summary>
    public static Dbase GetServerInfo()
    {
        return serverInfo;
    }

    /// <summary>
    /// 设置服务器信息 
    /// </summary>
    public static void SetServerInfo(string path, LPCValue data)
    {
        serverInfo.Set(path, data);
    }

    /// <summary>
    /// 设置服务器启动时间
    /// </summary>
    public static void SetServerStartTime(int starTime)
    {
        serverTime = starTime;
        unityTime = DateTime.Now;
    }

    /// <summary>
    /// 设置服务器时间
    /// </summary>
    public static void SetServerStartTime(string server_time, int start_time)
    {
        if (server_time.Length != 14)
            return;

        DateTime data = new DateTime(int.Parse(server_time.Substring(0, 4)), int.Parse(server_time.Substring(4, 2)), int.Parse(server_time.Substring(6, 2)),
            int.Parse(server_time.Substring(8, 2)), int.Parse(server_time.Substring(10, 2)), int.Parse(server_time.Substring(12, 2)));

        serverTime = start_time;
        TimeSpan span = data - new DateTime(1970, 1, 1).AddSeconds(start_time);
        zoneTime = (int)span.TotalSeconds;
        unityTime = DateTime.Now;
    }

    /// <summary>
    /// 返回服务器当前时间 
    /// </summary>
    public static int ServerTime
    {
        get
        {
            return serverTime + (int) (DateTime.Now - unityTime).TotalSeconds;
        }
    }

    /// <summary>
    /// 带服务器时区的时间
    /// </summary>
    public static int ServerZoneTime
    {
        get
        {
            return zoneTime;
        }
    }

    /// <summary>
    /// 发送数据到AAA 
    /// </summary>
    public static bool Send2AAA(int msgNo, object para)
    {
        if (! aaaConnector.IsConnected())
        {
            LogMgr.Trace("与AAA断开中，无法发送数据。");
            return false;
        }

        return aaaConnector.SendToServer(msgNo, para);
    }
    public static bool Send2AAA(string cmd, object para)
    {
        return Send2AAA(MsgMgr.MsgID(cmd), para);
    }

    /// <summary>
    /// 发送数据到GS
    /// </summary>
    public static bool Send2GS(int msgNo, object para)
    {
        if (! gsConnector.IsConnected())
        {
            LogMgr.Trace("与GS断开中，无法发送数据。");

            // 直接抛出延时
            gsConnector.GetConnect().Responser.OnDelay();

            // 发送消息失败
            return false;
        }

        return gsConnector.SendToServer(msgNo, para);
    }
    public static bool Send2GS(string cmd, object para)
    {
        return Send2GS(MsgMgr.MsgID(cmd), para);
    }
}
