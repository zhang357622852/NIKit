/// <summary>
/// HonorWnd.cs
/// Created by fengsc 2017/04/20
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class HonorWnd : WindowBase<HonorWnd>
{
    public UILabel mAmount;

    // Use this for initialization
    void Start ()
    {
        RedrawHonorValue();

        // 注册事件
        RegisterEvent();
    }

    void OnDestroy()
    {
        if (ME.user == null)
            return;

        // 解注册事件
        ME.user.dbase.RemoveTriggerField("HonorWnd");
    }

    void RegisterEvent()
    {
        if (ME.user == null)
            return;

        // 解注册事件
        ME.user.dbase.RemoveTriggerField("HonorWnd");
        ME.user.dbase.RegisterTriggerField("HonorWnd", new string[] { "exploit" }, new CallBack(OnHonorChange));
    }

    void OnHonorChange(object para, params object[] _params)
    {
        RedrawHonorValue();
    }

    /// <summary>
    /// 刷新竞技场荣誉
    /// </summary>
    void RedrawHonorValue()
    {
        mAmount.text = Game.SetMoneyShowFormat(ME.user.Query<int>("exploit"));
    }
}
