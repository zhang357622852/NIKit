/// <summary>
/// AwakeMaterialViewWnd.cs
/// Created by fengsc 2016/10/29
/// 材料获得途径悬浮窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class AwakeMaterialViewWnd : WindowBase<AwakeMaterialViewWnd>
{
    // 地下城名称
    public UILabel mName;

    public UILabel mDesc;

    private bool mIsView = false;

    int classId;

    void OnEnable()
    {
        gameObject.GetComponent<UIPanel>().alpha = 0f;
        gameObject.GetComponent<TweenAlpha>().enabled = false;
    }

    // Use this for initialization
    void Start ()
    {

    }

    void Redraw()
    {
        if(mIsView)
        {
            mName.text = ItemMgr.GetName(classId);
            mDesc.text = ItemMgr.GetDesc(classId);
            // 等待一帧
            Coroutine.DispatchService(SyncCameraRemove());
        }
        else
            gameObject.GetComponent<UIPanel>().alpha = 0;
    }

    /// <summary>
    /// 等待一帧
    /// </summary>
    /// <returns>The camera remove.</returns>
    private IEnumerator SyncCameraRemove()
    {
        yield return null;

        // 窗口已经析构，不在处理
        if (gameObject == null)
            yield break;

        // UIPanel
        UIPanel panel = gameObject.GetComponent<UIPanel>();
        if (panel == null)
            yield break;

        // 设置panel的alpha
        panel.alpha = 1.0f;
    }


    /// <summary>
    /// 显示窗口
    /// </summary>
    public void ShowView(int classId)
    {
        mIsView = true;
        this.classId = classId;

        Redraw();
    }

    /// <summary>
    /// 隐藏窗口
    /// </summary>
    public void HideView()
    {
        mIsView = false;
        Redraw();
    }
}
