using UnityEngine;
using System.Collections;
using LPC;

public class SetButtonPosition : MonoBehaviour
{
    // 指引组
    public int mGroup = -1;

    public int mStep = -1;

    string mListenerId = string.Empty;

    void Start()
    {
        mListenerId = Game.GetUniqueName("FunctionButtonUnlock");

        // 关注MSG_DO_ONE_GUIDE消息
        MsgMgr.RegisterDoneHook("MSG_DO_ONE_GUIDE", mListenerId, OnMsgDoOneGuide);

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
        // 当前分组指引是否完成，显示按钮
        if (GuideMgr.StepUnlock(mGroup, mStep))
        {
        }
        else
        {
        }
    }
}
