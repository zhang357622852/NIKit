using LPC;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 玩家技能更新
/// </summary>
public class MsgVersionTips : MsgHandler
{
    public string GetName()
    {
        return "msg_version_tips";
    }

    /// <summary>
    /// 消息入口
    /// </summary>
    public void Go(LPCValue para)
    {
        // 获取提示消息
        LPCMapping args = para.AsMapping;
        LPCMapping tipData = args.GetValue<LPCMapping>("tip_data");

        // 设置到用户缓存
        ME.user.SetTemp("version_tips",LPCValue.Create(tipData));

        // 判断玩家登陆状态
        if (ME.isLoginOk)
        {
            // 打开版本提示窗口
            VersionTipMgr.ShowVersionTipWnd();
        }else
        {
            // 初始化版本提示管理器
            VersionTipMgr.Init();
        }
    }
}