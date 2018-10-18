/// <summary>
/// TabBtn.cs
/// Created by fucj 2014-12-2
/// tab分页脚本
/// </summary>

using UnityEngine;
using System.Collections;

public partial class TabBtn : WindowBase<TabBtn>
{
    #region 成员变量
    public GameObject[] Btns;
    public int DefaultIndex = 0;
    public Vector2 offset = Vector2.zero;

    bool is_set_default = false;
    private GameObject mCurrent;
    #endregion

    #region 内部函数

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        if (Btns == null)
            return;

        foreach (GameObject ob in Btns)
        {
            if (ob == null)
                continue;

            // tab按钮注册点击事件
            UIEventListener.Get(ob).onClick += OnTabBtnClicked;
        }
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        // 设置默认选择项
        if (! is_set_default)
            SetDefaultBtn(DefaultIndex);
    }

    #endregion

    #region 公共函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        // 初始化窗口
        InitWnd();
    }

    /// <summary>
    /// 设置选中
    /// </summary>
    public void SetSelected(GameObject ob)
    {
        OnTabBtnClicked(ob);
    }

    /// <summary>
    /// 设置选中
    /// </summary>
    public void SetSelected(int index)
    {
        if (index > Btns.Length - 1)
            return;
        
        OnTabBtnClicked(Btns [index]);
    }

    /// <summary>
    /// 设置默认选择项
    /// </summary>
    public void SetDefaultBtn(int index = 0)
    {
        if (Btns == null)
            return;

        if (index > Btns.Length - 1)
            return;

        is_set_default = true;
        OnTabBtnClicked(Btns [index]);
    }

    /// <summary>
    /// tab按钮被点击
    /// </summary>
    /// <param name="btn">Button.</param>
    void OnTabBtnClicked(GameObject btn)
    {
        if (mCurrent == btn)
            return;

        Vector3 localPos;

        if (mCurrent != null)
        {
            localPos = mCurrent.transform.localPosition;
            mCurrent.transform.localPosition = new Vector3(localPos.x - offset.x, localPos.y - offset.y, localPos.z);
        }

        mCurrent = btn;

        // 偏移位置
        localPos = btn.transform.localPosition;
        btn.transform.localPosition = new Vector3(localPos.x + offset.x, localPos.y + offset.y, localPos.z);
    }

    #endregion
}
