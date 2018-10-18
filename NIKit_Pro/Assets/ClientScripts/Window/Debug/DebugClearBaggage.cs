/// <summary>
/// DebugClearBaggage.cs
/// Created by tanzy 2016/05/11
/// DebugClearBaggage窗口由于清除道具
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class DebugClearBaggage : WindowBase<DebugClearBaggage>
{
    #region 公共字段

    public UIInput mIdInput;
    // 包裹id输入框
    public GameObject mSureBtn;
    // 确认按钮
    public GameObject mCancelBtn;
    // 取消按钮

    #endregion

    #region 私有字段

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
    /// 确认按钮点击回调
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnSureBtn(GameObject go)
    {
        Destroy(gameObject);

        // 解析输入
        int page = -1;
        if (!int.TryParse(mIdInput.label.text.Trim(), out page))
        {
            DialogMgr.Notify("class_id格式不正确");
            return;
        }
            
        if (page < 0)
        {
            DialogMgr.Notify("包裹page不能小于0");
            return;
        }

        // 通知服务器载入道具
        Operation.CmdAdminClearBaggage.Go(page);
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
