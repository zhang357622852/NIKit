/// <summary>
/// PauseWnd.cs
/// Created by fengsc 2017/01/12
/// 暂停窗口
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class PauseWnd : WindowBase<PauseWnd>
{
    public TweenRotation mTweenRotation;

    public UILabel mTips;

    public GameObject mMask;

    public GameObject mComBatSetWnd;

    CallBack mCallBack;

    // Use this for initialization
    void Start ()
    {
        mTips.text = LocalizationMgr.Get("CombatSetWnd_6");

        UIEventListener.Get(mMask).onClick = OnClickMask;
    }

    void OnEnable()
    {
        InitWnd();
    }

    void InitWnd()
    {
        mTweenRotation.PlayForward();
        mTweenRotation.ResetToBeginning();
    }

    void OnClickMask(GameObject go)
    {
        // 恢复
        TimeMgr.DoContinueCombatLogic("CombatSetPause");

        // 关闭战斗设置界面
        mComBatSetWnd.SetActive(false);

        // 关闭当前窗口
        WindowMgr.HideWindow(gameObject);

        if (mCallBack != null)
        {
            mCallBack.Go();

            mCallBack = null;
        }
    }

    public void Bind(CallBack cb)
    {
        mCallBack = cb;
    }
}
