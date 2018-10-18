/// <summary>
/// CombatSecondWnd.cs
/// Created by tanzy 2016/05/10
/// CombatSetWnd的子窗口，玩家操作主要集中在该部分
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;
using System.Collections.Generic;

/// <summary>
/// CombatSetWnd的子窗口，玩家操作主要集中在该部分
/// </summary>
public class CombatSecondWnd : WindowBase<CombatSecondWnd>
{
    #region
    public GameObject mFirstBtn;    //第一个按钮
    public GameObject mSecondBtn;   //第二个
    public GameObject mThridBtn;      // 第三个

    public UILabel    lbFirstBtn;   // 第一个标签
    public UILabel    lbSecondBtn;  // 第二个标签
    public UILabel    lbThirdBtn;   // 第三个标签

    public Transform  WndPos;

    public bool mIsopen = false;

    void Start()
    {
        //初始化窗口
        InitWnd();
        //注册事件
        RegisterEvent();
    }

    public void InitWnd()
    {
        mIsopen = true;
    }

    public void SetHeight(float y)
    {
        WndPos.localPosition = new Vector3(0, y, 0);
    }

    public void OnFirstBtn(GameObject ob)
    {
    }

    public void OnSecondBtn(GameObject ob)
    {

    }

    public void OnThirdBtn(GameObject ob)
    {
        
    }

    public void RegisterEvent()
    {
        UIEventListener.Get(mFirstBtn).onClick += OnFirstBtn;
        UIEventListener.Get(mSecondBtn).onClick += OnSecondBtn;
        UIEventListener.Get(mThridBtn).onClick += OnThirdBtn;
    }

    public void UpdateText()
    {
        lbFirstBtn.text = LocalizationMgr.Get("CombatSetWnd_1");
        lbSecondBtn.text = LocalizationMgr.Get("CombatSetWnd_2");
        lbThirdBtn.text = LocalizationMgr.Get("CombatSetWnd_3");
    }


    #endregion
}


