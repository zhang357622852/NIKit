/// <summary>
/// DebugCloneItemWnd.cs
/// Created by xuhd Mar/26/2015
/// Debug窗口由于克隆道具
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class DebugCloneItemWnd : WindowBase<DebugCloneItemWnd>
{
    #region 公共字段

    public UIInput mClassIdInput;
    // class_id输入框
    public UIInput mNumInput;
    // 数量输入框
    public GameObject mSureBtn;
    // 确认按钮
    public GameObject mCancelBtn;
    // 取消按钮
    public UIInput mArgsInput;
    // 附加参数输入框


    #endregion

    #region 私有字段

    #endregion

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        // 初始化
        mArgsInput.label.text = "([])";
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
        int classId = -1;
        if (!int.TryParse(mClassIdInput.label.text.Trim(), out classId))
        {
            DialogMgr.Notify("class_id格式不正确");
            return;
        }

        int num = -1;
        if (!int.TryParse(mNumInput.label.text.Trim(), out num))
        {
            DialogMgr.Notify("数量格式不正确");
            return;
        }

        if (num <= 0)
        {
            DialogMgr.Notify("克隆的数量必须大于0");
            return;
        }

        LPCValue para = LPCRestoreString.RestoreFromString(mArgsInput.label.text.Trim());
        if (!para.IsMapping)
            para = LPCValue.CreateMapping();

        // 通知服务器载入道具
        Operation.CmdAdminClone.Go(classId.ToString(), num, para);
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
