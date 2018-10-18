/// <summary>
/// DebugCommonInputWnd.cs
/// Created by xuhd Jan/08/2015
/// Debug窗口用的通用输入窗口
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class DebugCommonInputWnd : MonoBehaviour
{
    #region 公共字段
    public UIInput mInput;              // 输入框
    public GameObject mSureBtn;         // 确认按钮
    public GameObject mCancelBtn;       // 取消按钮
    public UILabel    mTipsLb;          // 输入提示

    public delegate void BtnClickedDelegate(LPCMapping para);     // 确认按钮点击事件
    #endregion

    #region 私有字段
    private BtnClickedDelegate OnSureBtnClicked;           // 确认按钮点击代理
    #endregion

    // Use this for initialization
    void Start()
    {
        RegisterEvent();
    }

    /// <summary>
    /// 注册窗口事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mSureBtn).onClick += OnSureBtn;
        UIEventListener.Get(mCancelBtn).onClick += OnCancelBtn;
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    public void InitWnd(string inputTips, BtnClickedDelegate onSureBtn)
    {
        mTipsLb.text = inputTips;
        OnSureBtnClicked = onSureBtn;
    }

    /// <summary>
    /// 确认按钮点击回调
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnSureBtn(GameObject go)
    {
        Destroy(gameObject);

        if (OnSureBtnClicked == null)
            return;

        string input = mInput.label.text.Trim();

        LPCMapping map = new LPCMapping();
        map.Add("value", LPCValue.Create(input));

        OnSureBtnClicked(map);
    }

    /// <summary>
    /// 取消按钮点击回调
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnCancelBtn(GameObject go)
    {
        Destroy(gameObject);
    }
}
