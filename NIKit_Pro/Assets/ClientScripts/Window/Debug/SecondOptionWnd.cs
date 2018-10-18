/// <summary>
/// SecondOptionWnd.cs
/// Created by xuhd Sec/22/2014
/// 通用的二级选项子菜单窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 通用二级子菜单窗口
/// * 对上一级，它是一个窗口，暴露出一个句柄，被绑定到父窗口的某一个选项窗口上
/// * 对下一级，它管理着一排选项窗口
/// </summary>
public class SecondOptionWnd : MonoBehaviour
{
    #region 公共字段

    public TweenPosition tp;
    // 动画组件
    public bool mIsOpen = false;
    // 是否打开
    public GameObject mOptionItem;
    // 选项prefab
    public Transform mGrid;
    // 选项挂载点

    #endregion

    #region 内部成员

    private List<GameObject> mOptionList = new List<GameObject>();
    // 选项列表
    private const int WND_WIDTH = 200;
    // 当前窗口宽度

    #endregion

    // Use this for initialization
    void Start()
    {
    }

    void OnDestroy()
    {
        // 窗口宽度改变
        EventMgr.FireEvent(EventMgrEventType.EVENT_DEBUG_OPTION_WND_WIDTH_CHANGE, MixedValue.NewMixedValue<int>(-WND_WIDTH));
    }

    /// <summary>
    /// 打开二级选项窗口
    /// </summary>
    /// <param name="itemList">Item list.</param>
    public void OpenSecondOptionWnd(List<OptionItem> itemList)
    {
        if (itemList == null || itemList.Count <= 0)
            return;

        UIGrid grid = mGrid.GetComponent<UIGrid>();

        // 先清除原先的选项
        List<Transform> childList = grid.GetChildList();
        if (childList.Count > 0)
        {
            foreach (Transform tf in childList)
            {
                GameObject go = tf.gameObject;
                Destroy(go);
            }
            grid.repositionNow = true;
        }

        // 清空选项列表
        mOptionList.Clear();

        GameObject item;
        OptionItemWnd subWnd;
        for (int i = 0; i < itemList.Count; ++i)
        {
            item = Instantiate(mOptionItem) as GameObject;
            item.name = itemList [i].OptionName;

            subWnd = item.GetComponent<OptionItemWnd>();
            subWnd.SetOptionName(itemList [i].OptionName);
            subWnd.AddOnClickDelegate(itemList [i].Callback);
            mOptionList.Add(item);

            Transform ts = item.transform;
            ts.parent = mGrid;
            ts.localPosition = Vector3.zero;
            ts.localScale = Vector3.one;
            ts.gameObject.SetActive(true);
        }

        grid.Reposition();

        int totalCount = itemList.Count;
        UIPanel panel = gameObject.GetComponent<UIPanel>();
        panel.SetRect(0, -totalCount * (25 + totalCount / 4), 200, totalCount * (80 + totalCount));

        gameObject.SetActive(true);

        // 窗口宽度改变
        EventMgr.FireEvent(EventMgrEventType.EVENT_DEBUG_OPTION_WND_WIDTH_CHANGE, MixedValue.NewMixedValue<int>(WND_WIDTH));
    }

    /// <summary>
    /// 获取当前窗口的选项列表
    /// </summary>
    /// <returns>The item list.</returns>
    public List<GameObject> GetItemList()
    {
        return mOptionList;
    }
}
