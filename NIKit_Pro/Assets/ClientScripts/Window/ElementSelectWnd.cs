/// <summary>
/// ElementSelectWnd.cs
/// Created by fengsc 2018/01/02
/// 使魔元素选择窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ElementSelectWnd : WindowBase<ElementSelectWnd>
{
    public UIToggle[] mToggles;

    public UISprite[] mElementSprites;

    public UISprite mToggleBg;

    public float space = 0f;

    public int mGroup = 0;

    int mElement;

    CallBack mCallBack;

    List<int> mElementList= new List<int>();

    int mClassId = 0;

    int mSelectElement = 0;

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        for (int i = 0; i < mToggles.Length; i++)
            UIEventListener.Get(mToggles[i].gameObject).onClick = OnClickToggleBtn;
    }

    /// <summary>
    /// 按钮点击回调
    /// </summary>
    void OnClickToggleBtn(GameObject go)
    {
        UIEventListener listener = go.GetComponent<UIEventListener>();
        if (listener == null)
            return;

        int classId = (int)listener.parameter;

        CsvRow row = MonsterMgr.GetRow(classId);
        if (row == null)
            return;

        int element = row.Query<int>("element");

        if (mSelectElement == element)
            return;

        mSelectElement = element;

        // 执行回调
        if (mCallBack != null)
            mCallBack.Go(classId);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mElementList.Clear();

        CsvRow row = MonsterMgr.GetRow(mClassId);
        if (row == null)
            return;

        List<CsvRow> list = MonsterMgr.GetMonsterListByGroup(row.Query<int>("group"), true);

        for (int i = 0; i < mToggles.Length; i++)
            mToggles[i].gameObject.SetActive(false);

        // 按钮排序

        float checkmark = 97f;

        // x的开始位置
        float startX = mToggleBg.transform.localPosition.x - ((list.Count - 1) * checkmark - space) / 2f;

        for (int i = 0; i < list.Count; i++)
        {
            mToggles[i].transform.localPosition = new Vector3(startX + checkmark * i, mToggleBg.transform.localPosition.y, 0f);

            mToggles[i].group = mGroup;

            int element = list[i].Query<int>("element");

            UIEventListener listener = mToggles[i].gameObject.GetComponent<UIEventListener>();
            if (listener != null)
                listener.parameter =  list[i].Query<int>("class_id");

            string elementName = MonsterConst.MonsterElementSpriteMap[element];

            if (element == mSelectElement)
                mToggles[i].Set(true);
            else
                mToggles[i].Set(false);

            mElementSprites[i].spriteName = elementName;

            mToggles[i].gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int classId, int element, CallBack cb)
    {
        mElement = element;

        mSelectElement = mElement;

        mCallBack = cb;

        mClassId = classId;

        // 绘制窗口
        Redraw();
    }
}
