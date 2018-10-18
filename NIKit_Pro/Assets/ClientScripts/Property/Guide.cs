/// <summary>
/// Guide.cs
/// Create by zhaozy 2017-10-25
/// 指引对象
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using LPC;

/// <summary>
/// 指引对象
/// </summary>
public class Guide
{
    #region 属性

    /// <summary>
    /// 指引组
    /// </summary>
    public int Group { get; private set; }

    /// <summary>
    /// 当前指引阶段
    /// </summary>
    public int CurStep { get; private set; }

    /// <summary>
    /// 开始指引阶段
    /// </summary>
    public int SatrtStep { get; private set; }

    /// <summary>
    /// 指引是否已经结束标识
    /// </summary>
    public bool IsEnd { get; private set; }

    /// <summary>
    /// Gets the cb.
    /// </summary>
    /// <value>The cb.</value>
    public CallBack cb { get; private set; }

    public MixedValue mEventPara { get; private set; }

    #endregion

    #region 内部接口

    /// <summary>
    /// 需要等待服务器指引消息
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="param">Parameter.</param>
    void OnWaitGuideMsg(object para, params object[] param)
    {
    }

    /// <summary>
    /// 某一步指引结束
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="param">Parameter.</param>
    void OnOneStepGuideEnd(object para, params object[] param)
    {
        // 步进指引步骤
        // 直接在当前阶段的基础上+1
        CurStep = CurStep + 1;

        // 转变指引阶段
        DoGuideStep(CurStep);
    }

    /// <summary>
    /// 执行事件
    /// </summary>
    private void DoGuide(CsvRow data)
    {
        // 已经结束
        if (IsEnd)
            return;

        // 不是int数据不处理，直接跳到下一步
        int scriptNo = data.Query<int>("script");

        // 必须保证有脚本
        Debug.Assert(scriptNo > 0);

        // 通知服务器缓存指引步骤
        CallBack guideCallBack = null;

        // 如果是关键步骤需要等待服务器消息
        if (data.Query<int>("sync") == 1)
        {
            // 通知服务器同步指引
            Operation.CmdDoOneGuide.Go(Group, CurStep);

            // 添加回调
            guideCallBack = new CallBack(OnWaitGuideMsg);
        }
        else
        {
            // 执行脚本
            guideCallBack = new CallBack(OnOneStepGuideEnd);
        }

        // 执行脚本
        ScriptMgr.Call(scriptNo,
            data.Query<LPCValue>("script_args"),   // 脚本参数
            guideCallBack,                         // 回调函数
            (SatrtStep > 0 && SatrtStep == CurStep),  // 该步骤是否是恢复指引操作
            Group,
            CurStep,
            mEventPara);
    }

    /// <summary>
    /// Changes the step.
    /// </summary>
    /// <param name="state">State.</param>
    private void DoGuideStep(int step)
    {
        // 已经结束
        if (IsEnd)
            return;

        // 获取阶段需要处理的事件
        CsvRow data = GuideMgr.GetGuideData(Group, CurStep);

        // 没有事件需要处理, 表示该组指引已经结束
        if (data == null)
        {
            // 结束指引
            DoEnd();
            return;
        }

        // 执行事件
        DoGuide(data);
    }

    /// <summary>
    /// 开始指引
    /// </summary>
    private void _DoStart(object para, object[] expara)
    {
        // 转变指引阶段
        DoGuideStep(CurStep);
    }

    /// <summary>
    /// 离开游戏回调
    /// </summary>
    private void OnMsgDoOneGuide(string cmd, LPCValue para)
    {
        // 转换消息格式
        LPCMapping args = para.AsMapping;

        // 获取group和guide_id
        int group = args.GetValue<int>("group");
        int guideId = args.GetValue<int>("guide_id");

        // 不是当前指引步骤的服务器回执消息
        if (group != Group ||
            guideId != CurStep)
            return;

        // 步进指引步骤
        // 直接在当前阶段的基础上+1
        CurStep = CurStep + 1;

        // 转变指引阶段
        DoGuideStep(CurStep);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 构造函数
    /// </summary>
    public Guide(int _group, int step, CallBack _cb, MixedValue para)
    {
        // 指引组编号
        Group = _group;
        CurStep = step;
        SatrtStep = step;
        cb = _cb;
        mEventPara = para;

        // 关注MSG_DO_ONE_GUIDE消息
        MsgMgr.RegisterDoneHook("MSG_DO_ONE_GUIDE", string.Format("Guide_{0}", Group), OnMsgDoOneGuide);
    }

    /// <summary>
    /// 开始指引
    /// </summary>
    public void DoStart(float delay = 0f)
    {
        // 转变指引阶段
        DoGuideStep(CurStep);
    }

    /// <summary>
    /// 结束指引
    /// </summary>
    public void DoEnd()
    {
        // 标识结束
        IsEnd = true;

        // 清除当前指引对象
        GuideMgr.GuideOb = null;

        // 指引数据
        CsvRow guideData = GuideMgr.GetGuideData(Group, 0);

        // 游戏外指引结束，数据缓存至本地
        if (guideData != null && guideData.Query<int>("in_game") == 0)
        {
            LPCMapping localGuide = LPCMapping.Empty;

            // 标识本组指引已经结束
            LPCValue account = Communicate.AccountInfo.Query("account");
            if (account != null && account.IsString)
            {
                string accountStr = account.AsString;

                LPCValue v = OptionMgr.GetAccountOption(accountStr, "guide");
                if (v != null && v.IsMapping)
                    localGuide = v.AsMapping;

                localGuide.Add(Group, 1);

                OptionMgr.SetAccountOption(accountStr,"guide", LPCValue.Create(localGuide));
            }
        }

        // 执行指引完成回调
        if (cb != null)
            cb.Go();
    }

    #endregion
}
