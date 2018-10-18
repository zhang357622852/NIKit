using System;
using System.Collections;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 消息的回调
/// </summary>
public delegate void MsgHook(string cmd,LPCValue para);

/// <summary>
/// 过滤器回调
/// </summary>
public delegate bool MsgFilterHook(string cmd,LPCValue para);

/// <summary>
/// 消息被处理后的处理回调
/// </summary>
public delegate void MsgExecutedHook(string cmd);

/// <summary>
/// 消息结构
/// </summary>
public class MsgNode
{
    public string msg;
    public LPCValue para;
    public MsgExecutedHook f;

    public MsgNode(string msg, LPCValue para, MsgExecutedHook f)
    {
        this.msg = msg;
        this.para = para;
        this.f = f;
    }
}

/// <summary>
/// 消息处理 
/// </summary>
public class MsgMgr
{
    #region 成员变量

    /// <summary>
    /// 调试开关 
    /// </summary>
    public static bool debug = false;

    /// <summary>
    /// 掉线的回调处理
    /// </summary>
    public delegate void DisconnectHook();
    public static event DisconnectHook eventDisconnect;

    /// <summary>
    /// 网络恢复通畅.
    /// </summary>
    public delegate void NetUnobstructedHook();
    public static event NetUnobstructedHook eventNetUnobstructed;

    /// <summary>
    /// 收到消息回调（在消息处理前, 再添加消息队列前）.
    /// </summary>
    public delegate void MsgArrivedHook(string cmdName);
    public static event MsgArrivedHook eventMsgArrived;

    /// <summary>
    /// 网络出现延时.
    /// </summary>
    public delegate void NetDelayHook();
    public static event NetDelayHook eventNetDelay;

    // socket线程解析出来的消息队列
    public static Queue<MsgNode> msgQ = new Queue<MsgNode>();

    // 收到消息的处理回调列表
    private static Dictionary<string, Dictionary<string, MsgHook>> recvHooks = new Dictionary<string, Dictionary<string, MsgHook>>();

    // 消息过滤器列表
    private static Dictionary<string, MsgFilterHook> filterHooks = new Dictionary<string, MsgFilterHook>();

    // 消息执行完毕的回调
    private static Dictionary<string, Dictionary<string, MsgHook>> doneHooks = new Dictionary<string, Dictionary<string, MsgHook>>();

    // 消息名字与消息ID的映射表
    private static Dictionary<int, string> msgID2Name = new Dictionary<int, string>();
    private static Dictionary<string, int> msgName2ID = new Dictionary<string, int>();
    private static Dictionary<int, MsgHandler> msgID2Handler = new Dictionary<int, MsgHandler>();
    private static Dictionary<int, CmdHandler> cmdID2Handler = new Dictionary<int, CmdHandler>();

    #endregion

    #region 内部接口

    /// <summary>
    /// 静态构造
    /// </summary>
    static MsgMgr()
    {
    }

    /// <summary>
    /// 掉线的处理
    /// </summary>
    private static void OnDisconnect()
    {
        // 回调
        if (eventDisconnect != null)
            eventDisconnect();
    }

    private static void OnNetDelay()
    {
        // 回调
        if (eventNetDelay != null)
        {
            try
            {
                eventNetDelay();
            }
            catch (Exception e)
            {
                // 不处理
                LogMgr.Exception(e);
            }
        }
    }

    private static void OnNetUnobstructed()
    {
        if (eventNetUnobstructed != null)
        {
            try
            {
                eventNetUnobstructed();
            }
            catch (Exception e)
            {
                // 不处理
                LogMgr.Exception(e);
            }
        }
    }

    /// <summary>
    /// Raises the message arrived event.
    /// </summary>
    private static void OnMsgArrived(string cmdName)
    {
        if (eventMsgArrived != null)
        {
            try
            {
                eventMsgArrived(cmdName);
            }
            catch (Exception e)
            {
                // 不处理
                LogMgr.Exception(e);
            }
        }
    }

    // 执行收到消息的回调
    private static void InvokeRecvHook(string cmd, LPCValue para)
    {
        // 没有登记回调
        if (!recvHooks.ContainsKey(cmd))
            return;

        foreach (MsgHook f in recvHooks[cmd].Values)
            f(cmd, para);
    }
    
    // 消息过滤器
    private static bool Filter(string cmd, LPCValue para)
    {
        foreach (MsgFilterHook f in filterHooks.Values)
            if (f(cmd, para))
                // 消息被过滤掉了
                return true;
        
        return false;
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// Start this instance.
    /// </summary>
    public static void Start()
    {
        Coroutine.DispatchService(Update());
    }

    /// <summary>
    /// 添加一条消息，由socket线程调度过来
    /// </summary>
    public static void AddMsg(int cmd, LPCValue para, MsgExecutedHook f)
    {
        string cmdName = MsgName(cmd);
        if (cmdName == null)
        {
            // 消息ID号不存在
            LogMgr.Error(string.Format("消息ID {0}不存在", cmd));
            return;
        }

        // 通知VerifyCmdMgr消息到达
        OnMsgArrived(cmdName);

        // 创建一条消息
        MsgNode node = new MsgNode(cmdName, para, f);

        // 扔进队列中
        lock (msgQ)
        {
            msgQ.Enqueue(node);
        }
    }

    public static void AddMsg(string cmdName, LPCValue para, MsgExecutedHook f)
    {
        // 创建一条消息
        MsgNode node = new MsgNode(cmdName, para, f);

        // 扔进队列中
        lock (msgQ)
        {
            msgQ.Enqueue(node);
        }
    }

    /// <summary>
    /// 消息的执行
    /// </summary>
    public static IEnumerator Update()
    {
        while (true)
        {
            _Update();
            yield return null;
        }
    }

    public static void _Update()
    {
        int ti = TimeMgr.RealTick;
        for (int i = 0; i < 50; i++)
        {
            if (TimeMgr.RealTick - ti > 50)
                // 处理太久了
                return;

            MsgNode node = null;
            lock (msgQ)
            {
                if (msgQ.Count < 1)
                    return;
                node = msgQ.Dequeue();
            }

            if (node == null)
                return;

            if (node.msg == "DISCONNECT")
            {
                // 掉线的处理
                lock (msgQ)
                {
                    msgQ.Clear();
                    OnDisconnect();
                }
                return;
            }

            if (node.msg == "NETDELAY")
            {
                // 网络出现延迟
                OnNetDelay();
                continue;
            }

            if (node.msg == "NETUNOBSTRUCTED")
            {
                // 网络恢复通畅
                OnNetUnobstructed();
                continue;
            }

#if ! UNITY_EDITOR
            // 执行收到消息的回调处理
            try
            {
                InvokeRecvHook(node.msg, node.para);

                // 消息过滤器
                if (Filter(node.msg, node.para))
                {
                    // 消息被过滤了
                    LogMgr.Trace("消息{0}被过滤了", node.msg);
                    continue;
                }

                // 执行之
                Execute(node.msg, node.para);

                // 回调处理
                if (node.f != null)
                    node.f(node.msg.ToLower());
            } catch (Exception e)
            {
                LogMgr.Exception(e);
            }
#else
            InvokeRecvHook(node.msg, node.para);

            // 消息过滤器
            if (Filter(node.msg, node.para))
            {
                // 消息被过滤了
                LogMgr.Trace(string.Format("消息{0}被过滤了", node.msg));
                continue;
            }

            // 执行之
            Execute(node.msg, node.para);

            // 回调处理
            if (node.f != null)
                node.f(node.msg.ToLower());
#endif
        }
    }

    /// <summary>
    /// 根据消息ID取得消息名 
    /// </summary>
    public static string MsgName(int id)
    {
        if (msgID2Name.ContainsKey(id))
            return msgID2Name[id];
        return null;
    }

    /// <summary>
    /// 根据消息名字取得消息ID 
    /// </summary>
    public static int MsgID(string name)
    {
        if (msgName2ID.ContainsKey(name))
            return msgName2ID[name];
        return 0;
    }

    /// <summary>
    /// 登记一个MSG消息及其处理器 
    /// </summary>
    public static void RegisterAgent(int id, MsgHandler handler)
    {
        string cmd = handler.GetName().ToUpper();
                
        msgID2Name[id] = cmd;
        msgName2ID[cmd] = id;
        msgID2Handler[id] = handler;
    }

    /// <summary>
    /// 登记一个CMD消息及其处理器 
    /// </summary>
    public static void RegisterCmdAgent(int id, CmdHandler handler)
    {
        cmdID2Handler[id] = handler;
    }

    /// <summary>
    /// 返回CMD消息及其处理器 
    /// </summary>
    public static CmdHandler GetCmdAgent(int cmdId)
    {
        if (!cmdID2Handler.ContainsKey(cmdId))
            return null;

        // 返回数据
        return cmdID2Handler[cmdId];

    }

    /// <summary>
    /// 此消息ID是不是有经过压缩 
    /// </summary>
    public static bool IsCompressedMsgNo(int id)
    {
        return id == MsgID("MSG_ZLIB_MSG");
    }

    /// <summary>
    /// 登记消息名字和ID的映射 
    /// </summary>
    public static void RegisterMsgNo(string name, int id)
    {
        // 全部归一为大写
        name = name.ToUpper();
        msgID2Name[id] = name;
        msgName2ID[name] = id;
    }

    /// <summary>
    /// 读取定义了消息编号的头文件。可以重复调用
    /// </summary>
    /// <param name="lines">文件内容，按行打断了</param>
    /// <returns>如果成功，返回true；否则返回false</returns>
    public static bool LoadMessageFile(string[] lines)
    {
        // 解析这个文件的内容
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
    
            // 如果不是期望的形式："#define MSG_NOTIFY_ITEM_STAT                0x0157"
            if (!line.StartsWith("#define") ||
                (!line.Contains("0x") && !line.Contains("0X")))
                continue;
    
            // 按字符串打断
            string[] parts = line.Split(new string[1] { " " }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
                throw new Exception("Bad format: " + line);

            // 记录之
            RegisterMsgNo(parts[1], Convert.ToInt32(parts[2], 16));
        }
    
        return true;
    }

    /// <summary>
    /// 登记收到消息的回调 
    /// </summary>
    public static void RegisterRecvHook(string cmd, string k, MsgHook f)
    {
        cmd = cmd.ToUpper();
        if (!recvHooks.ContainsKey(cmd))
            recvHooks[cmd] = new Dictionary<string, MsgHook>();
        recvHooks[cmd][k] = f;
    }

    /// <summary>
    /// 反登记收到消息的回调 
    /// </summary>
    public static void RemoveRecvHook(string cmd, string k)
    {
        cmd = cmd.ToUpper();
        if (!recvHooks.ContainsKey(cmd) || !recvHooks[cmd].ContainsKey(k))
            return;
        recvHooks[cmd].Remove(k);
    }

    /// <summary>
    /// 登记消息过滤器
    /// </summary>
    public static void RegisterFilterHook(string k, MsgFilterHook f)
    {
        filterHooks[k] = f;
    }

    /// <summary>
    /// 反登记收到消息的回调 
    /// </summary>
    public static void RemoveFilterHook(string k)
    {
        if (!filterHooks.ContainsKey(k))
            return;
        filterHooks.Remove(k);
    }

    /// <summary>
    /// 登记消息处理完毕的回调 
    /// </summary>
    public static void RegisterDoneHook(string cmd, string k, MsgHook f)
    {
        cmd = cmd.ToUpper();
        if (!doneHooks.ContainsKey(cmd))
            doneHooks[cmd] = new Dictionary<string, MsgHook>();
        doneHooks[cmd][k] = f;
    }

    /// <summary>
    /// 反登记消息处理完毕的回调 
    /// </summary>
    public static void RemoveDoneHook(string cmd, string k)
    {
        cmd = cmd.ToUpper();
        if (!doneHooks.ContainsKey(cmd) || !doneHooks[cmd].ContainsKey(k))
            return;
        doneHooks[cmd].Remove(k);
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    public static void Execute(string cmd, LPCValue para)
    {
#if UNITY_EDITOR
        // 打印下消息
        LogMgr.Network(MsgID(cmd), para);
#endif

#if PHONE_DEBUG
        int ti = Util.TimeMgr.Tick;
#endif
        // 取得消息处理器
        MsgHandler handler;

        if (!msgID2Handler.ContainsKey(MsgID(cmd)))
        {
            LogMgr.Trace("消息({0})处理器不存在。", cmd);
            return;
        }
        handler = msgID2Handler[MsgID(cmd)];

        // 处理之
        handler.Go(para);

#if PHONE_DEBUG
        int cost = TimeMgr.Tick - ti;
        ti = TimeMgr.Tick;
#endif

        // 调用回调
        if (! doneHooks.ContainsKey(cmd) || doneHooks[cmd].Count == 0)
        {
            // 没有登记回调
#if PHONE_DEBUG
            LogMgr.Trace("消息{0}执行：{1}", cmd, TimeMgr.Tick - ti);
#endif
            return;
        }

        // copy一下doneHooks[cmd].Values，防止执行回调的时候在回调中操作doneHooks
        MsgHook[] msgHooks = new MsgHook[doneHooks[cmd].Count];
        doneHooks[cmd].Values.CopyTo(msgHooks, 0);

        // 执行回调
        foreach (MsgHook f in msgHooks)
            f(cmd, para);

#if PHONE_DEBUG
        LogMgr.Trace("消息{0}执行：{1}-{2}", cmd, cost, TimeMgr.Tick - ti);
#endif
    }

    #endregion
}
