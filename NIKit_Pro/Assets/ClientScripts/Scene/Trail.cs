/// <summary>
/// Trail.cs
/// Modify by zhaozy 2016-06-02
/// Trail组件脚本
/// </summary>

using UnityEngine;
using System;
using System.Collections;

[ExecuteInEditMode]
public class Trail : MonoBehaviour
{
    #region 成员变量

    /// <summary>
    /// The s render.
    /// </summary>
    private TrailRenderer mRenderOb;

    /// <summary>
    /// The name of the m sorting layer.
    /// </summary>
    public string mSortingLayer = "Default";

    /// <summary>
    /// The m sorting order.
    /// </summary>
    public int mSortingOrder = 0;

    #endregion

    /// <summary>
    /// Gets or sets the sorting layer.
    /// </summary>
    /// <value>The sorting layer.</value>
    public string SortingLayer
    {
        get
        {
            return mSortingLayer;
        }
        set
        {
            if (mSortingLayer != value)
            {
                mSortingLayer = value;

                // 重置
                ResetSortingLayer();
            }
        }
    }

    /// <summary>
    /// Gets or sets the sorting order.
    /// </summary>
    /// <value>The sorting order.</value>
    public int SortingOrder
    {
        get
        {
            return mSortingOrder;
        }
        set
        {
            if (mSortingOrder != value)
            {
                mSortingOrder = value;

                // 重置
                ResetSortingLayer();
            }
        }
    }

    /// <summary>
    /// Start this instance.
    /// </summary>
    void Start()
    {
        // 重置渲染顺序
        ResetSortingLayer();
    }

    /// <summary>
    /// 重置渲染顺序
    /// </summary>
    void ResetSortingLayer()
    {
        // 获取TrailRenderer主键
        mRenderOb = GetComponent<TrailRenderer>();

        // mRender对象不存在
        if (mRenderOb == null)
            return;

        // 设置渲染层级相关数据
        mRenderOb.sortingLayerName = mSortingLayer;
        mRenderOb.sortingOrder = mSortingOrder;
    }
}
