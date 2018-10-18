using System;
using System.Collections;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 处理经过 ZLIB 压缩的大数量信息
/// </summary>
public class MsgZlibMsg : MsgHandler
{
    public string GetName() { return "msg_zlib_msg"; }

    public void Go(LPCValue para)
    {
        // TODO
    }
}