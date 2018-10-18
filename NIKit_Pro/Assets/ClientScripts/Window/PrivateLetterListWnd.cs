/// <summary>
/// PrivateLetterListWnd.cs
/// Created by fengsc 2016/12/13
/// 最近私信玩家列表窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class PrivateLetterListWnd : WindowBase<PrivateLetterListWnd>
{
    // 窗口关闭按钮
    public GameObject mCloseBtn;

    public GameObject mItem;

    public GameObject mMask;

    public UITable mTable;

    // 界面背景图片
    public UISprite mBg;

    // item的间隔
    int mItemSpace = 10;

    // item的固定高度
    int mItemHeight = 42;

    int mBgDefauleHeight = 112;

    int mCacheLimit = 0;

    // 选择玩家的名称
    string mSelectUserName = string.Empty;

    // 缓存创建的item对象
    List<GameObject> mItemList = new List<GameObject>();

    // 最近私信的玩家列表
    LPCArray mWhisperCacheUserList = new LPCArray();

    Dictionary<string, string> mInfoCaChe = new Dictionary<string, string>();

    CallBack mCallBack;

    void Awake()
    {
        mCacheLimit = GameSettingMgr.GetSettingInt("max_whisper_user_cache_amount");

        // 脚本唤醒的时候创建一批GameObejct
        CreateItem();

        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask).onClick = OnClickMask;
    }

    /// <summary>
    /// 创建一批item
    /// </summary>
    void CreateItem()
    {
        mItem.SetActive(false);
        for (int i = 0; i < mCacheLimit; i++)
        {
            GameObject go = Instantiate(mItem);

            if (go == null)
                return;

            go.transform.SetParent(mTable.transform);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = Vector3.one;
            go.name = i.ToString();

            mItemList.Add(go);
        }
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 遍历列表填充数据
        int index = 0;
        foreach (LPCValue value in mWhisperCacheUserList.Values)
        {
            if (!value.IsArray || index > mCacheLimit - 1)
                return;

            LPCArray array = value.AsArray;

            string name = array[1].AsString;

            if (mInfoCaChe.ContainsKey(name))
                mInfoCaChe.Remove(name);

            mInfoCaChe.Add(name, array[0].AsString);

            GameObject go = mItemList[index];

            Transform userName = go.transform.Find("userName");

            if (userName == null)
                return;

            userName.GetComponent<UILabel>().text = name;

            if (index == mCacheLimit - 1)
            {
                Transform underLine = go.transform.Find("underline");

                if (underLine == null)
                    return;

                underLine.gameObject.SetActive(false);
            }

            go.SetActive(true);

            // 注册item的点击事件
            UIEventListener.Get(go).onClick = OnClickItem;

            index++;
        }

        mTable.repositionNow = true;

        // 动态设置bg的高度
        mBg.height = mBgDefauleHeight + (index - 1) * (mItemHeight + mItemSpace);
    }

    /// <summary>
    /// item的点击事件
    /// </summary>
    void OnClickItem(GameObject go)
    {
        Transform item = go.transform.Find("userName");

        if (item == null)
            return;

        // 获取label的值
        string name = item.GetComponent<UILabel>().text;

        if (string.IsNullOrEmpty(name))
            return;

        LPCArray data = new LPCArray();
        data.Add(mInfoCaChe[name]);
        data.Add(name);

        mCallBack.Go(data);
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭列表列表窗口
        this.gameObject.SetActive(false);

        if(string.IsNullOrEmpty(mSelectUserName) || !mInfoCaChe.ContainsKey(mSelectUserName))
            return;

        LPCArray data = new LPCArray();
        data.Add(mInfoCaChe[mSelectUserName]);
        data.Add(mSelectUserName);

        mCallBack.Go(data);
    }

    void OnClickMask(GameObject go)
    {
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCArray privateLetterList, CallBack callback)
    {
        if (privateLetterList == null || privateLetterList.Count == 0)
            return;

        mCallBack = callback;

        mWhisperCacheUserList = privateLetterList;

        // 绘制窗口
        Redraw();
    }
}
