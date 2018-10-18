/// <summary>
/// TowerViewBonusWnd.cs
/// Created by lic 2017/08/31
/// 通天之塔奖励窗口
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class TowerViewBonusWnd  : WindowBase<TowerViewBonusWnd>
{
    #region 成员变量

    public UIWrapContent mWrapContent;

    public GameObject mBonusItem;

    public GameObject mScrollView;

    #endregion

    #region 私有变量

    int mDifficulty = -1;

    // 创建item个数
    int mColumnCount = 8;

    // 当前显示数据的index与实际数据的对应关系
    Dictionary<int ,List<LPCMapping>> mData = new Dictionary<int ,List<LPCMapping>>();

    Dictionary<int, int> mIndexMap = new Dictionary<int, int>();

    // Itemlist
    List<GameObject> mItemObList = new List<GameObject>();

    #endregion 

    #region 内部函数

    // Use this for initialization
    void Start () 
    {
        // 初始化窗口
        InitWnd();

        // 注册事件
        RegisterEvent();

        // 刷新格子
        CreateItem();
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        mBonusItem.SetActive(false);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 向委托中添加方法
        mWrapContent.onInitializeItem = OnUpdateItem;
    }

    /// <summary>
    /// 重绘排名窗口
    /// </summary>
    void Redraw()
    {
        // 当前窗口不存在不处理
        if (WindowMgr.GetWindow("TowerWnd") == null)
            return;

        // 界面没有激活不处理
        if (!gameObject.activeInHierarchy)
            return;

        // 初始化数据
        if (!mData.ContainsKey(mDifficulty))
        {
            List<LPCMapping> mBonusData = new List<LPCMapping>();

            List<int> layerList = new List<int>();
            layerList.Add(1);
            layerList.Add(5);
            layerList.Add(2);

            for (int i = 1; i <= 10; i++)
                layerList.Add(i * 10);

            foreach (int layer in layerList)
            {
                LPCMapping data = LPCMapping.Empty;
                data.Add("bonus", TowerMgr.GetBonusByLayer(mDifficulty, layer - 1));
                data.Add("layer", layer);

                mBonusData.Add(data);
            }

            mData.Add(mDifficulty, mBonusData);
        }

        mWrapContent.enabled = true;

        int rowAmount = mData[mDifficulty].Count > mColumnCount ? mData[mDifficulty].Count : mColumnCount;

        // 从0开始
        mWrapContent.maxIndex = 0;

        mWrapContent.minIndex = -(rowAmount - 1);

        // 填充数据
        if (mIndexMap.Count != 0)
        {
            foreach (KeyValuePair<int, int> kv in mIndexMap)
                FillData(kv.Key, kv.Value);
        }
    }

    /// <summary>
    /// 创建排行榜格子(动态复用)
    /// </summary>
    void CreateItem()
    {
        for(int i = 0; i < mColumnCount; i++)
        {
            GameObject item = Instantiate (mBonusItem) as GameObject;
            item.transform.parent = mWrapContent.transform;
            item.name = string.Format("item_{0}", i);
            item.transform.localScale = Vector3.one;
            item.transform.localPosition = new Vector3 (0f, - i * 60f, 0f);

            item.SetActive(true);

            // 注册点击事件
            UIEventListener.Get(item).onClick = OnItemClicked;

            mItemObList.Add (item);
        }
    }


    /// <summary>
    /// 任务item被点击
    /// </summary>
    void OnItemClicked(GameObject ob)
    {

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

        TowerBonusItem item = itemWnd.GetComponent<TowerBonusItem>();

        if(item == null)
            return;

        int dataIndex =  System.Math.Abs(realIndex);

        if (dataIndex < mData[mDifficulty].Count)
            item.Bind(mData[mDifficulty][dataIndex], dataIndex);
    }

    #endregion 

    #region 外部接口

    // 刷新界面
    public void Redraw(int type)
    {   
        // 类型相同，不需要刷新
        if (mDifficulty == type)
            return;

        mDifficulty = type;

        // 刷新界面
        Redraw();
    }

    #endregion 
}
