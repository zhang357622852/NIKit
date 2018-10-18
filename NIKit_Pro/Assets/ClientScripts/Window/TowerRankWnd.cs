/// <summary>
/// TowerRankWnd.cs
/// Created by lic 2017/08/31
/// 通天之塔排名窗口
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class TowerRankWnd  : WindowBase<TowerRankWnd>
{
    #region 成员变量

    public UIWrapContent mWrapContent;

    public GameObject mRankItem;

    public GameObject mScrollView;

    public UILabel mNoRankDesc;

    #endregion

    #region 私有变量

    int mType = 0;

    // 创建item个数
    int mColumnCount = 7;

    // 当前页面的数据
    LPCArray mData = new LPCArray();

    // 当前显示数据的index与实际数据的对应关系
    Dictionary<int, int> mIndexMap = new Dictionary<int, int>();

    // Itemlist
    List<GameObject> mItemObList = new List<GameObject>();

    // 起始位置
    Dictionary<GameObject,Vector3> rePosition = new Dictionary<GameObject,Vector3>();

    #endregion 

    #region 内部函数

    void Awake()
    {
        // 注册事件
        RegisterEvent();

        // 刷新格子
        CreateItem();
    }

	// Use this for initialization
	void Start () 
    {
        // 初始化窗口
        InitWnd();
	}

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        mNoRankDesc.text = LocalizationMgr.Get("TowerRankWnd_1");

        mRankItem.SetActive(false);
    }

    void OnDestroy()
    {
        // 移除事件监听
        EventMgr.UnregisterEvent("TowerRankWnd");
    }


    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 向委托中添加方法
        mWrapContent.onInitializeItem = OnUpdateItem;

        EventMgr.RegisterEvent("TowerRankWnd", EventMgrEventType.EVENT_GET_TOWER_TOP_LIST, OnGetTopListEvent);
    }

    /// <summary>
    /// 重绘排名窗口
    /// </summary>
    void Redraw(bool resetPosition =true)
    {
        // 当前窗口不存在不处理
        if (WindowMgr.GetWindow("TowerWnd") == null)
            return;

        // 界面没有激活不处理
        if (!gameObject.activeInHierarchy)
            return;

        // 获取排行榜数据
        mData = TowerMgr.GetTopList(mType);

        // 数据为空
        if (mData == null || mData.Count == 0)
        {
            mScrollView.SetActive(false);
            mNoRankDesc.gameObject.SetActive(true);
            return;
        }

        mScrollView.SetActive(true);
        mNoRankDesc.gameObject.SetActive(false);

        //固定滚动条
        mScrollView.GetComponent<UIScrollView>().DisableSpring();

        // 需要复位格子位置
        if(resetPosition)
        {
            foreach(GameObject item in rePosition.Keys)
                item.transform.localPosition = rePosition[item];

            // 此处是要还原scrollview的位置，
            // 但是没找到scrollview具体可用的接口
            mScrollView.GetComponent<UIPanel>().clipOffset = new Vector2(0f, 0f);
            mScrollView.transform.localPosition = new Vector3(0f, 0f, 0f);

            if(mIndexMap != null)
                mIndexMap.Clear();

            for(int i = 0; i < mColumnCount; i++)
                mIndexMap.Add(i, -i);
        }

        if (mData.Count <= mItemObList.Count)
        {
            mWrapContent.enabled = false;

            for (int i = 0; i < mItemObList.Count; i++)
            {
                if (i >= mData.Count)
                {
                    mItemObList[i].SetActive(false);
                    continue;
                }

                mItemObList[i].SetActive(true);

                // 绑定数据
                mItemObList[i].GetComponent<TowerRankItem>().Bind(mData[i].AsMapping, i);
            }

            return;
        }


        for (int i = 0; i < mItemObList.Count; i++)
            mItemObList[i].SetActive(true);

        mWrapContent.enabled = true;

        int rowAmount = mData.Count > mColumnCount ? mData.Count : mColumnCount;

        // 从0开始
        mWrapContent.maxIndex = 0;

        mWrapContent.minIndex = -(rowAmount - 1);

        if (mIndexMap.Count != 0)
        {
            // 填充数据
            foreach(KeyValuePair<int, int> kv in mIndexMap)
                FillData(kv.Key, kv.Value);
        }
    }

    /// <summary>
    /// 排名基础格子点击事件
    /// </summary>
    /// <param name="go">Go.</param>
    void OnClickRankItem(GameObject go)
    {
        TowerRankItem script = go.GetComponent<TowerRankItem>();
        if (script == null || string.IsNullOrEmpty(script.mRid) || script.mRid.Equals(ME.user.GetRid()))
            return;

        // 先显示界面后填写数据
        GameObject wnd = WindowMgr.OpenWnd(FriendViewWnd.WndType);

        // 窗口创建失败
        if (wnd == null)
        {
            LogMgr.Trace("FriendViewWnd窗口创建失败");
            return;
        }

        // 通知服务器请求数据
        Operation.CmdDetailAppearance.Go(DomainAddress.GenerateDomainAddress("c@" + script.mRid, "u", 0));
    }

    /// <summary>
    /// 创建排行榜格子(动态复用)
    /// </summary>
    void CreateItem()
    {
        for(int i = 0; i < mColumnCount; i++)
        {
            GameObject item = Instantiate (mRankItem) as GameObject;
            item.transform.parent = mWrapContent.transform;
            item.name = string.Format("item_{0}", i);
            item.transform.localScale = Vector3.one;
            item.transform.localPosition = new Vector3 (0f, - i * 75f, 0f);

            UIEventListener.Get(item).onClick = OnClickRankItem;

            item.SetActive(false);

            // 记录item初始位置
            rePosition.Add(item, item.transform.localPosition);

            mItemObList.Add (item);
        }
    }

    /// <summary>
    /// 获取排行榜列表事件回调
    /// </summary>
    void OnGetTopListEvent(int eventId, MixedValue para)
    {
        // 刷新界面数据
        Redraw();
    }
        
    /// <summary>
    ///设置滑动列表时复用宠物格子更改数据
    /// </summary>
    void OnUpdateItem(GameObject go, int index, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        if(!mIndexMap.ContainsKey(index))
            mIndexMap.Add(index, realIndex);
        else
            mIndexMap[index] = realIndex;

        FillData(index, realIndex);
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="realIndex">Real index.</param>
    void FillData(int index, int realIndex)
    {
        if (index >= mItemObList.Count)
            return;

        GameObject itemWnd = mItemObList[index];

        if(itemWnd == null)
            return;

        TowerRankItem item = itemWnd.GetComponent<TowerRankItem>();

        if(item == null)
            return;

        int dataIndex =  System.Math.Abs(realIndex);

        if (dataIndex < mData.Count)
            item.Bind(mData[dataIndex].AsMapping, dataIndex);
    }

    #endregion 

    #region 外部接口

    // 刷新界面
    public void Redraw(int type)
    {
        mType = type;

        // 数据需要重新请求
        if (TowerMgr.RequestTopList(type))
            return;

        // 刷新界面
        Redraw();
    }

    #endregion 
}
