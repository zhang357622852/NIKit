/// <summary>
/// OptionItemWindow.cs
/// Created by xuhd Sec/04/2014
/// 调试选项窗口项
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OptionItemWnd : MonoBehaviour
{
    #region 公共字段
    public UILabel mOptionName;                  // 选项名
    public delegate void ItemClickedDelegate(GameObject item);     // 选项被选中事件
    public delegate void HideThridLevelMenuDelegate(GameObject item);      // 隐藏第三层按钮
    #endregion

    #region 私有字段
    private ItemClickedDelegate mOnItemClicked;                // 选项点击代理
    private GameObject mSubWnd;                                // 该按钮呼出的子窗口
    private HideThridLevelMenuDelegate mHideThridLevelMenu;    // 隐藏第三级菜单
    #endregion

    // Use this for initialization
    void Start()
    {
        RegisterEvent();
    }

    #region 外部接口

    /// <summary>
    /// 设置选项名称
    /// </summary>
    /// <param name="name">Name.</param>
    public void SetOptionName(string name)
    {
        mOptionName.text = name;
    }

    /// <summary>
    /// 添加点击回调
    /// </summary>
    /// <param name="itemClick">Item click.</param>
    public void AddOnClickDelegate(ItemClickedDelegate itemClick)
    {
        mOnItemClicked = itemClick;
    }

    /// <summary>
    /// 注册子窗口
    /// </summary>
    /// <param name="subWnd">Sub window.</param>
    public void RegisterSubWnd(GameObject subWnd)
    {
        if (mSubWnd != null)
        {
            Debug.Log("该按钮已经有子窗口了，不能在注册");
            return;
        }

        mSubWnd = subWnd;
    }

    /// <summary>
    /// 获取子窗口
    /// </summary>
    /// <returns>The sub window.</returns>
    public GameObject GetSubWnd()
    {
        return mSubWnd;
    }

    #endregion

    #region 内部方法

    /// <summary>
    /// 注册窗口事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(gameObject).onClick += OnItemClick;
    }

    /// <summary>
    /// 技能项点击事件
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnItemClick(GameObject go)
    {
        if (mOnItemClicked != null)
        {
            GameObject thirdOpetionWnd = GameObject.Find("ThirdOptionWnd");
            if (thirdOpetionWnd != null)
            {
                Destroy(thirdOpetionWnd);
            }
            mOnItemClicked(gameObject);
        }
    }


    #endregion
}
