/// <summary>
/// FirsdWnd.cs
/// Created by WinMi 2018/11/21
///
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class FirsdWnd : UIBaseForms<FirsdWnd>
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
        UIMgr.Instance.ShowForms<StartWnd>(StartWnd.FormsName);
    }

    #endregion

    #region 公共函数
    #endregion
}
