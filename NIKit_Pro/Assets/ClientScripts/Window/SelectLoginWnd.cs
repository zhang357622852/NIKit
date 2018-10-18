using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectLoginWnd : MonoBehaviour {

    #region 成员变量
    public UILabel mTitle;

    public GameObject GuestBtn;
    public UILabel mGuestBtnLb;

    public UISprite mLoginBtn1;
    public UILabel mLoginBtn1Lb;
    public UISprite mLoginIcon1;
    public UISprite mBtn1BG;

    public UISprite mLoginBtn2;
    public UILabel mLoginBtn2Lb;
    public UISprite mLoginIcon2;
    public UISprite mBtn2BG;

    private string mChannel;

    #endregion


    #region 私有函数

    /// <summary>
    /// Raises the login btn2 event.
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnClickLoginBtn2(GameObject ob)
    {
        string type = string.Empty;

        if (mChannel.Equals("qc"))
        {
            type = "qc";
        }
        else
        {
            type = "google";
        }

        QCCommonSDK.Addition.AuthSupport.Auth(type);
    }

    /// <summary>
    /// Raises the login btn1 event.
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnClickLoginBtn1(GameObject ob)
    {
        QCCommonSDK.Addition.AuthSupport.Auth("facebook");
    }

    /// <summary>
    /// Guest登录
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnGuestBtn(GameObject ob)
    {
        QCCommonSDK.Addition.AuthSupport.Auth("guest");
    }

    #endregion

    #region 内部函数

    void OnEnable()
    {
        
    }

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        // 初始化窗口
        InitWnd();
    }

    void OnDisable()
    {
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(GuestBtn).onClick = OnGuestBtn;
        UIEventListener.Get(mLoginBtn1.gameObject).onClick = OnClickLoginBtn1;
        UIEventListener.Get(mLoginBtn2.gameObject).onClick = OnClickLoginBtn2;
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        mTitle.text = LocalizationMgr.Get("LoginWnd_22");
        mGuestBtnLb.text = LocalizationMgr.Get("LoginWnd_18");

        // 渠道
        mChannel = QCCommonSDK.QCCommonSDK.FindNativeSetting("CHANNEL");
        if (mChannel.EndsWith("qc"))
        {
            mLoginBtn1.gameObject.SetActive(false);

            mLoginBtn2Lb.text = LocalizationMgr.Get("LoginWnd_21");

            mLoginIcon2.spriteName = "login_qc";
            GuestBtn.transform.localPosition = new Vector3(-212, -13, 0);
            mLoginBtn2.transform.localPosition = new Vector3(243, -13, 0);
            mLoginBtn2.color = new Color(53 / 255f, 82 / 255f, 114 / 255f);
        }
        else
        {
            // en
            mLoginBtn1.gameObject.SetActive(true);

            mLoginBtn1Lb.text = LocalizationMgr.Get("LoginWnd_19");
            mLoginBtn2Lb.text = LocalizationMgr.Get("LoginWnd_20");

            // 设置登录按钮图标
            mLoginIcon1.spriteName = "login_fb";
            mLoginIcon2.spriteName = "login_google";

            // 初始化按钮位置
            GuestBtn.transform.localPosition = new Vector3(-344, -13, 0);
            mLoginBtn1.transform.localPosition = new Vector3(9.3f, -13, 0);
            mLoginBtn2.transform.localPosition = new Vector3(363, -13, 0);

            mLoginBtn1.color = new Color(53 / 255f, 88 / 255f, 161 / 255f);
            mLoginBtn2.color = new Color(52 / 255f, 168 / 255f, 85 / 255f);

            // 设置按钮背景颜色
            mBtn1BG.color = new Color(53 / 255f, 88 / 255f, 161 / 255f);
            mBtn2BG.color = new Color(52 / 255f, 168 / 255f, 85 / 255f);
        }
    }
    #endregion
}
