/// <summary>
/// StartWnd.cs
/// Created by WinMi 2018/11/9
///
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class StartWnd : UIBaseForms<StartWnd>
{
    #region 内部函数
    public override void Init()
	{
		RegisterEvent();

        //float scale = GameUtility.CalcWndScale();
        //transform.localScale = new Vector3(scale, scale, scale);
    }

	private void RegisterEvent()
	{
        UIEventListener.Get(mAccountBtn).onClick = OnClickAccount;
        UIEventListener.Get(mSectionBtn).onClick = OnClickSection;
        UIEventListener.Get(mGotoGameBtn).onClick = OnClickGotoGame;
    }

	public override void Show()
	{
	}

	private void OnClickAccount(GameObject go)
	{
        NIDebug.Log("==OnClickAccount=");

        UIMgr.Instance.ShowForms<AccountWnd>(AccountWnd.FormsName);
	}

    private void OnClickSection(GameObject go)
    {
        NIDebug.Log("==OnClickSection=");
    }

    private void OnClickGotoGame(GameObject go)
    {
        NIDebug.Log("==OnClickGotoGame=");

        UIMgr.Instance.CloseForms(StartWnd.FormsName);
    }

    #endregion

    #region 公共函数
    #endregion
}
