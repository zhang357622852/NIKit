using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DropSoulWnd : WindowBase<DropSoulWnd>
{
    #region 成员变量

    public GameObject mCloseBtn;
    public GameObject mItem;

    #endregion

    #region 私有变量

    private LPCArray mData;

    private Vector3 mItemPos;

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        mItem.SetActive(false);

        if (mData == null)
            return;

        List<int> mMapList = new List<int>();

        // 收集地图列表
        foreach (LPCValue material in mData.Values)
        {
            if (material == null || !material.IsArray)
                continue;

            int classId = material.AsArray[0].AsInt;

            List<int> dropList = MapMgr.GetItemDropMapList(classId);

            foreach (int mapId in dropList)
            {
                if (mMapList.Contains(mapId))
                    continue;

                mMapList.Add(mapId);
            }
        }

        GameObject item;
        for (int i = 0; i < mMapList.Count; i++)
        {
            item = Instantiate (mItem) as GameObject;
            item.transform.parent = transform;
            item.name = string.Format("item_{0}", i);
            item.transform.localPosition = new Vector3(mItem.transform.localPosition.x + i*390f, mItem.transform.localPosition.y, mItem.transform.localPosition.z);
            item.transform.localScale = new Vector3(1f, 1f, 1f);

            item.SetActive(true);
            item.GetComponent<DropSoulInfoWnd>().BindData(mMapList[i]);
        }
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="data">Data.</param>
    public void BindData(LPCArray data)
    {
        mData = data;

        Redraw();
    }

    #endregion
}
