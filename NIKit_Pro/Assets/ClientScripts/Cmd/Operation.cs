/// <summary>
/// 指令处理
/// </summary>
using System;
using System.Diagnostics;
using LPC;

public partial class Operation
{
    /// <summary>
    /// 生成发往服务器的参数
    /// </summary>
    public static LPCValue PackArgs(params object[] args)
    {
        // 参数一定是偶数个
        Debug.Assert(args.Length % 2 == 0);

        // 打包为mapping
        LPCValue m = LPCValue.CreateMapping();
        for (int i = 0; i < args.Length; i += 2)
        {
            string k = args [i] as string;
            object v = args [i + 1];
            Debug.Assert(v is int || v is string || v is LPCValue || v is float);

            if (v is int)
                m.AsMapping.Add(k, LPCValue.Create((int)v));
            else if (v is string)
                m.AsMapping.Add(k, LPCValue.Create(v as string));
            else if (v is float)
                m.AsMapping.Add(k, LPCValue.Create((float)v));
            else if (v is LPCMapping)
                m.AsMapping.Add(k, LPCValue.Create(v));
            else if (v is LPCArray)
                m.AsMapping.Add(k, LPCValue.Create(v));
            else
                m.AsMapping.Add(k, v as LPCValue);
        }

        // 返回结果
        return m;
    }
}