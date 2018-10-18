/// <summary>
/// MsgPublishEntity.cs
/// Created by zhaozy 2015/09/01
/// 检索到发布道具的详细信息
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;
public class MsgPublishEntity : MsgHandler
{

    public string GetName()
    {
        return "msg_publish_entity";
    }
    
    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
    }
}

