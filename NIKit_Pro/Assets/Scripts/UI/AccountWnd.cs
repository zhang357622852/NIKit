/// <summary>
/// AccountWnd.cs
/// Created by WinMi 2018/11/21
///
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class AccountWnd : UIBaseForms<AccountWnd>
{
	#region 内部函数

	public override void Init()
	{
		RegisterEvent();
	}

	private void RegisterEvent()
	{
        UIEventListener.Get(mBackBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mGotoBtn).onClick = OnClickGotoBtn;
    }

	public override void Show()
	{
	}

	private void OnClickCloseBtn(GameObject go)
	{
        UIMgr.Instance.CloseForms(gameObject);
	}

    private void OnClickGotoBtn(GameObject go)
    {
        UIMgr.Instance.ShowForms<FirsdWnd>(FirsdWnd.FormsName);
    }

    #endregion

    #region 公共函数
    #endregion
}
