/// <summary>
/// Created bu zhaozy 2018/02/27
/// 服务器通知视频操作结果
/// </summary>
using System;
using LPC;

/// <summary>
/// 服务器通知推荐视频列表
/// </summary>
public class MsgVideoOperateDone : MsgHandler
{
    public string GetName()
    {
        return "msg_video_operate_done";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping;

        // 执行视频操作结果
        VideoMgr.DoVideoOperateDone(
            args.GetValue<string>("oper"),
            (args.GetValue<int>("result") != 0),
            args.GetValue<LPCValue>("extra_data")
        );
    }
}
