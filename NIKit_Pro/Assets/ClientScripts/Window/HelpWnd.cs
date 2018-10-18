/// <summary>
/// HelpWnd.cs
/// Created by fengsc 2016/07/12
///通用帮助信息窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class HelpWnd :WindowBase<HelpWnd>
{

    /// <summary>
    ///克隆体
    /// </summary>
    public GameObject mItem;

    public GameObject mHelpInfoList;

    public GameObject mDescList;

    public UIGrid mGrid;

    public UITable mTable;

    public GameObject mRichTextContent;

    public UILabel mTitle;

    /// <summary>
    ///返回按钮
    /// </summary>
    public GameObject mReturnBtn;

    /// <summary>
    ///关闭界面按钮;
    /// </summary>  
    public GameObject mClose;

    public UIScrollView mHelpInof;

    public UIScrollView mHelpDesc;

    public UIDragScrollView mDragScrollView;

    public BoxCollider mContentBg;

    public GameObject mMask;

    public TweenAlpha mTweenAlpha;

    public TweenScale mTweenScale;

    /// <summary>
    ///帮助信息类型
    /// </summary>
    private int mHelpId = -1;

    /// <summary>
    ///缓存某一类型的描述信息
    /// </summary>
    CsvRow mHelpData;

    Dictionary<int, GameObject> goContent = new Dictionary<int, GameObject>();

    Dictionary<string, int> mTypeDic = new Dictionary<string, int>();

    Vector3 mDescBoxPos = new Vector3(0, -24, 0);

    Vector3 mDescBoxSize = new Vector3(600, 320, 1);

    Vector3 mHelpListPos = Vector3.zero;

    Vector3 mHelpListSize = new Vector3(600, 364, 1);

    void Start ()
    {
        //注册事件;
        RegisterEvent();

        if (mTweenAlpha == null || mTweenScale == null)
            return;

        if (mTweenScale != null)
        {
            mTweenScale.ResetToBeginning();

            mTweenScale.PlayForward();
        }

        if (mTweenAlpha != null)
        {
            mTweenAlpha.ResetToBeginning();

            mTweenAlpha.PlayForward();
        }
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        // 清空缓存数据
        Clear();
    }

    void Redraw()
    {
        if (mHelpId >= 0)
            return;

        RedrawHelpInfoList();
    }

    /// <summary>
    ///绘制帮助信息列表
    /// </summary>
    void RedrawHelpInfoList()
    {
        mHelpInfoList.SetActive(true);

        mDescList.SetActive(false);

        for (int i = 0; i < mGrid.transform.childCount; i++)
        {
            if (mGrid.GetChild(i) == null)
                continue;

            Destroy(mGrid.GetChild(i).gameObject);
        }

        mDragScrollView.scrollView = mHelpInof;

        mContentBg.center = mHelpListPos;
        mContentBg.size = mHelpListSize;

        // 显示help帮助title
        foreach (CsvRow data in HelpInfoMgr.HelpCsv.rows)
        {
            //实例化一个对象;
            GameObject clone = Instantiate(mItem) as GameObject;

            //设置父级;
            clone.transform.SetParent(mGrid.transform);

            clone.transform.localPosition = Vector3.zero;

            clone.transform.localScale = Vector3.one;

            int helpId = data.Query<int>("help_id");
            clone.name = helpId.ToString();

            UILabel mTitle = clone.transform.Find("title").GetComponent<UILabel>();

            mTitle.text = LocalizationMgr.Get(data.Query<string>("title"));

            UIEventListener.Get(clone).onClick = OnClickHelpInfo;

            clone.SetActive(true);

            mTypeDic[clone.name] = helpId;
        }

        mGrid.Reposition();
    }

    /// <summary>
    ///设置描述信息内容
    /// </summary>
    void SetDescContent()
    {
        //没有该范围内的帮助信息类型;
        if(mHelpId < 0)
            return;

        mDragScrollView.scrollView = mHelpDesc;

        mContentBg.center = mDescBoxPos;
        mContentBg.size = mDescBoxSize;

        bool IsSet = true;

        //获取该描述类型的所有数据;
        mHelpData = HelpInfoMgr.GetDescList(mHelpId);

        // 没有数据;
        if(mHelpData == null)
            return;

        mRichTextContent.SetActive(false);

        GameObject go = Instantiate(mRichTextContent);

        go.transform.SetParent(mTable.transform);

        go.transform.localScale = Vector3.one;

        go.transform.localPosition = Vector3.zero;

        int helpId = mHelpData.Query<int>("help_id");

        go.name = helpId.ToString();

        string desc = LocalizationMgr.Get(mHelpData.Query<string>("desc"));

        go.GetComponent<RichTextContent>().ParseValue(desc);

        go.SetActive(true);

        if(IsSet)
        {
            mTitle.text = LocalizationMgr.Get(mHelpData.Query<string>("title"));
            IsSet = false;
        }

        if (goContent == null ||
            goContent.ContainsKey(helpId))
            return;

        goContent.Add(helpId, go);

        mTable.enabled = true;

        mTable.repositionNow = true;
    }

    /// <summary>
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mClose).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask).onClick = OnClickCloseBtn;
        UIEventListener.Get(mReturnBtn).onClick = OnClickReturnBtn;

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnFinish);
    }

    void OnFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void Clear()
    {
        int name = 0;

        GameObject obj = null;

        for (int i = 0; i < mTable.transform.childCount; i++)
        {
            if(!int.TryParse(mTable.transform.GetChild(i).name, out name))
                continue;

            if(!goContent.TryGetValue(name, out obj))
                continue;

            Destroy(obj);
        }

        mTypeDic.Clear();

        goContent.Clear();
    }

    /// <summary>
    ///返回按钮点击事件
    /// </summary>
    void OnClickReturnBtn(GameObject go)
    {
        Clear();

        mHelpDesc.ResetPosition();

        mHelpInfoList.SetActive(true);

        mDescList.SetActive(false);

        RedrawHelpInfoList();
    }

    /// <summary>
    ///关闭窗口按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow("HelpWnd");
    }

    /// <summary>
    ///帮助信息选择点击回调
    /// </summary>
    void OnClickHelpInfo(GameObject go)
    {
        for (int i = 0; i < mGrid.transform.childCount; i++)
            Destroy(mGrid.GetChild(i).gameObject);

        mHelpInfoList.SetActive(false);

        int helpId = -1;

        if (!mTypeDic.TryGetValue(go.name, out helpId))
            return;

        mHelpId = helpId;

        SetDescContent();

        mDescList.SetActive(true);
    }

    /// <summary>
    ///绑定数据,显示某一类型的帮助信息列表;
    /// </summary>
    /// <param name="type">帮助信息的类型</param>
    public void Bind(int helpId = -1)
    {
        mHelpId = helpId;

        Redraw();

        SetDescContent();

        mHelpInof.ResetPosition();

        mHelpDesc.ResetPosition();
    }
}
