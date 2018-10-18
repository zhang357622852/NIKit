/// <summary>
/// Exp.cs
/// Created by fengsc 2016/08/02
/// 经验条脚本
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class Exp : MonoBehaviour
{
    //经验条;
    GameObject mExpBar;

    string prefix = "Exp_";

    string mRid = string.Empty;

    void OnDestroy()
    {
        // 销毁血条窗口
        if (mExpBar != null)
            WindowMgr.DestroyWindow(prefix + mRid);
    }

    public void ShowExp(LPCMapping map, string rid)
    {
        mRid = rid;

        // 创建血条
        string wndName = prefix + mRid;
        mExpBar = WindowMgr.GetWindow(wndName);
        if (mExpBar != null)
        {
            mExpBar.SendMessage("ShowExp");
            return;
        }

        // 创建血条
        mExpBar = WindowMgr.CreateWindow(wndName, ExpBar.PrefebResource, null, 1.0f, true);
        if (mExpBar == null)
        {
            LogMgr.Trace("经验条创建失败。");
            return;
        }

        // 绑定对象显示血条
        mExpBar.GetComponent<ExpBar>().Bind(mRid, map);
    }

    public void HideExp()
    {
        // 隐藏经验条窗口
        if (mExpBar != null)
        {
            mExpBar.SendMessage("HideExp");
        }
    }
}
