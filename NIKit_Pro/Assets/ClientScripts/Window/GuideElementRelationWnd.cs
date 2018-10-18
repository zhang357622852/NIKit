/// <summary>
/// GuideElementRelationWnd.cs
/// Created by fengsc 2017/11/07
/// </summary>
using UnityEngine;
using System.Collections;

public class GuideElementRelationWnd : WindowBase<GuideElementRelationWnd>
{
    public GameObject mCloseBtn;

    public UILabel mTitle;

    public UILabel mDesc1;

    public UILabel mDesc2;

    CallBack mCallback;

    // Use this for initialization
    void Start ()
    {
        InitLable();

        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
    }

    void InitLable()
    {
        mTitle.text = LocalizationMgr.Get("GuideElementRelationWnd_1");
        mDesc1.text = LocalizationMgr.Get("GuideElementRelationWnd_2");
        mDesc2.text = LocalizationMgr.Get("GuideElementRelationWnd_3");
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        if (mCallback != null)
            mCallback.Go();

        WindowMgr.DestroyWindow(gameObject.name);
    }

    public void Bind(CallBack cb)
    {
        mCallback = cb;
    }
}
