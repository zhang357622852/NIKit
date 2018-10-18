using System.Collections.Generic;
using System.Reflection;
using System;

public static class RegisterCmds
{
    public static void Init()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        // 注册MSG消息
        foreach (MsgHandler msg in GetImplementedObjectsByInterface<MsgHandler>(assembly, typeof(MsgHandler)))
        {
            if (MsgMgr.MsgID(msg.GetName().ToUpper()) == 0)
            {
                //LogMgr.Trace("处理器{0}没有定义相关的消息", msg.GetName().ToUpper());
                UnityEngine.Debug.Log(string.Format("处理器{0}没有定义相关的消息", msg.GetName().ToUpper()));
                continue;
            }

            MsgMgr.RegisterAgent(MsgMgr.MsgID(msg.GetName().ToUpper()), msg);
        }

        // 注册CMD消息
        foreach (CmdHandler cmd in GetImplementedObjectsByInterface<CmdHandler>(assembly, typeof(CmdHandler)))
        {
            if (MsgMgr.MsgID(cmd.GetName().ToUpper()) == 0)
            {
                // LogMgr.Trace("处理器{0}没有定义相关的消息", cmd.GetName().ToUpper());
                UnityEngine.Debug.Log(string.Format("处理器{0}没有定义相关的消息", cmd.GetName().ToUpper()));
                continue;
            }

            // 注册CMD消息
            MsgMgr.RegisterCmdAgent(MsgMgr.MsgID(cmd.GetName().ToUpper()), cmd);
        }
    }

    public static IEnumerable<TBaseInterface>
        GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly, Type targetType)
        where TBaseInterface : class
    {
        Type[] arrType = assembly.GetExportedTypes();

        var result = new List<TBaseInterface>();

        for (int i = 0; i < arrType.Length; i++)
        {
            var currentImplementType = arrType [i];

            if (currentImplementType.IsAbstract)
                continue;

            if (!targetType.IsAssignableFrom(currentImplementType))
                continue;

            result.Add((TBaseInterface)Activator.CreateInstance(currentImplementType));
        }

        return result;
    }
}
