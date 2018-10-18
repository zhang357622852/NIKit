/// <summary>
/// SceneNpcWnd
/// Created by fengsc 2018/08/27
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using LPC;

public class SceneNpcWnd : MonoBehaviour
{
    public SpriteRenderer NPC;

    public TextMeshPro mNpcName;

    public string mLocalText;

    void Awake()
    {
        mNpcName.text = LocalizationMgr.Get(mLocalText);

        if (ME.user == null)
            return;

        ME.user.tempdbase.RemoveTriggerField("SceneNpcWnd");
        ME.user.tempdbase.RegisterTriggerField("SceneNpcWnd", new string[]{ "evaluate_id" }, new CallBack(OnFieldsChange));

        ME.user.dbase.RemoveTriggerField("SceneNpcWnd");
        ME.user.dbase.RegisterTriggerField("SceneNpcWnd", new string[]{ "unlock_evaluate" }, new CallBack(OnFieldsChange));
    }

    // Use this for initialization
    void OnEnable ()
    {
        Redraw();
    }

    void OnDisable()
    {
        // 取消调用
        CancelInvoke("ShowNpcTips");
    }

    void OnDestroy()
    {
        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("SceneNpcWnd");
        ME.user.tempdbase.RemoveTriggerField("SceneNpcWnd");
    }

    void OnFieldsChange(object para, params object[] param)
    {
        // 销毁npc提示窗口
        if (!RefreshNpc())
            DestroyNpcTips();
    }

    void Redraw()
    {
        // 刷新npc
        bool refresh = RefreshNpc();

        if (refresh)
        {
            int showEvaluate = 0;
            LPCValue show_evaluate = OptionMgr.GetOption(ME.user, "show_evaluate");
            if (show_evaluate != null && show_evaluate.IsInt)
                showEvaluate = show_evaluate.AsInt;

            // 没有主动显示过评价窗口
            if (showEvaluate == 0)
            {
                // 打开评价入口界面
                GameObject wnd = WindowMgr.OpenWnd(EvaluateEntranceWnd.WndType);

                if (wnd != null)
                    wnd.GetComponent<EvaluateEntranceWnd>().Bind(new CallBack(OnEvaluateEntranceWndClose));

                // 标识自动显示过窗口
                OptionMgr.SetOption(ME.user, "show_evaluate", LPCValue.Create(1));
            }
            else
            {
                CancelInvoke("ShowNpcTips");
                InvokeRepeating("ShowNpcTips", 0, 60);
            }
        }
    }

    /// <summary>
    /// EvaluateEntranceWnd 窗口关闭回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="param">Parameter.</param>
    void OnEvaluateEntranceWndClose(object para, params object[] param)
    {
        if (! RefreshNpc())
            return;

        CancelInvoke("ShowNpcTips");

        // 每分钟显示一次
        InvokeRepeating("ShowNpcTips", 0, 60);
    }

    bool RefreshNpc()
    {
        // 没有配置评价地址
        if (string.IsNullOrEmpty(ConfigMgr.Get<string>("evaluate_url")))
        {
            gameObject.SetActive(false);
            return false;
        }

        if (ME.user == null)
        {
            gameObject.SetActive(false);
            return false;
        }

        // 评价没解锁
        if (ME.user.Query<int>("unlock_evaluate") == 0)
        {
            gameObject.SetActive(false);
            return false;
        }

        int active = 0;

        // 是否激活过评价系统
        LPCValue active_evaluate = OptionMgr.GetOption(ME.user, "active_evaluate");
        if (active_evaluate != null && active_evaluate.IsInt)
            active = active_evaluate.AsInt;

        string localEvaluateId = string.Empty;

        // 本地的评价标识
        LPCValue local_evaluate_id = OptionMgr.GetOption(ME.user, "evaluate_id");
        if (local_evaluate_id != null && local_evaluate_id.IsString)
            localEvaluateId = local_evaluate_id.AsString;

        string evaluateID = string.Empty;
        LPCValue evaluate_id = ME.user.QueryTemp<LPCValue>("evaluate_id");
        if (evaluate_id != null && evaluate_id.IsString)
            evaluateID = evaluate_id.AsString;

        if (string.IsNullOrEmpty(evaluateID))
        {
            gameObject.SetActive(false);
            return false;
        }

        // 激活过并且没有更新评价系统
        if (active == 1 && localEvaluateId.Equals(evaluateID))
        {
            gameObject.SetActive(false);
            return false;
        }

        gameObject.SetActive(true);

        return true;
    }

    /// <summary>
    /// 显示npc提示窗口
    /// </summary>
    void ShowNpcTips()
    {
        // 打开悬浮窗口
        GameObject wnd = WindowMgr.OpenWnd(NPCTipsWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<NPCTipsWnd>().Bind(
            new Vector3(
                NPC.transform.position.x + 1.28f * NPC.transform.localScale.x,
                NPC.transform.position.y + 1.28f * NPC.transform.localScale.y,
                NPC.transform.position.z)
        );


    }

    void DestroyNpcTips()
    {
        WindowMgr.DestroyWindow(NPCTipsWnd.WndType);
    }

    /// <summary>
    /// 点击npc
    /// </summary>
    public void OnClickWnd()
    {
        CancelInvoke("ShowNpcTips");

        // 打开评价入口界面
        GameObject wnd = WindowMgr.OpenWnd(EvaluateEntranceWnd.WndType);

        if (wnd == null)
            return;

        wnd.GetComponent<EvaluateEntranceWnd>().Bind(new CallBack(OnEvaluateEntranceWndClose));
    }
}

