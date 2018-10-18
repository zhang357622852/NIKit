using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BindAccountItem : WindowBase<BindAccountItem>
{
    #region 成员变量

    public UISprite mIcon;
    public UILabel mDesc;
    public GameObject mState;
    public UISprite mBg;
 
    private string mType;

    #endregion

    #region 内部函数

    /// <summary>
    /// 绑定按钮被点击
    /// </summary>
    /// <param name="go">Go.</param>
    void OnClickBindBtn(GameObject go)
    {
        // 绑定账号走统一的流程
        QCCommonSDK.Addition.AuthSupport.bindAccount(mType);
    }


    #endregion

    #region 公共函数

    public void SetData(string type, string icon, string name, string bindId)
    {
        mType = type;
        mIcon.spriteName = icon;
        mDesc.text = name;

        if(string.IsNullOrEmpty(bindId))
        {
            mState.GetComponent<UILabel>().text = string.Format("[51A1C5FF]{0}[-]", LocalizationMgr.Get("SystemWnd_35"));
            UIEventListener.Get(mState).onClick += OnClickBindBtn;
            mBg.color = new Color(1f, 1f, 1f, 39/(float)255);
        }
        else
        {
            mState.GetComponent<UILabel>().text = bindId;
            UIEventListener.Get(mState).onClick -= OnClickBindBtn;
            mBg.color = new Color(1f, 1f, 1f, 120/(float)255);
        }
    }

    #endregion
}
