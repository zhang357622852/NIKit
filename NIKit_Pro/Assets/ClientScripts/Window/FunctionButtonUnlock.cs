﻿/// <summary>
/// FunctionButtonUnlock.cs
/// 功能按钮解锁脚本
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class FunctionButtonUnlock : MonoBehaviour
{
    // 指引组
    public int mGroup = -1;

    public int mStep = -1;

    GameObject mParent;

    string mListenerId = string.Empty;

    BoxCollider mBox;

    void Start()
    {
        mListenerId = Game.GetUniqueName("FunctionButtonUnlock");

        // 关注MSG_DO_ONE_GUIDE消息
        MsgMgr.RegisterDoneHook("MSG_DO_ONE_GUIDE", mListenerId, OnMsgDoOneGuide);

        mParent = transform.Find("parent").gameObject;

        mBox = transform.GetComponent<BoxCollider>();

        Redraw();
    }

    void OnDestroy()
    {
        // 移除消息关注
        MsgMgr.RemoveDoneHook("MSG_DO_ONE_GUIDE", mListenerId);
    }

    /// <summary>
    /// MSG_DO_ONE_GUIDE消息回调
    /// </summary>
    void OnMsgDoOneGuide(string cmd, LPCValue para)
    {
        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if (mParent == null)
            return;

        if (GuideMgr.IsGuided(GuideConst.ARENA_FINISH))
        {
            if (!mParent.activeSelf)
                mParent.SetActive(true);

            if (mBox != null)
                mBox.enabled = true;

            return;
        }

        // 当前分组指引是否完成，显示按钮
        if (GuideMgr.StepUnlock(mGroup, mStep))
        {
            if (!mParent.activeSelf)
                mParent.SetActive(true);

            if (mBox != null)
                mBox.enabled = true;
        }
        else
        {
            if (mParent.activeSelf)
                mParent.SetActive(false);

            if (mBox != null)
                mBox.enabled = false;
        }
    }
}
