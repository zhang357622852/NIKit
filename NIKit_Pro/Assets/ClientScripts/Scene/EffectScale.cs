/// <summary>
/// EffectScale.cs
/// Created by zhaozy 2016/12/21(冬至)
/// 光效缩放
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class EffectScale : MonoBehaviour
{
    #region 变量

    /// <summary>
    /// The m scale.
    /// </summary>
    public float mScale = 0f;

    // 光效身子对象
    public Transform mBodyOb;

    // 光效尾部对象
    public Transform mEndOb;

    // 光效标准长度
    public float mBasicLength;

    // 光效原始缩放比例
    private Vector3 mBasicScale = Vector3.zero;

    #endregion

    #region 属性

    public float Scale
    { 
        get
        {
            // 获取比例
            return mScale;
        }
        set
        {
            // 设置缩放数值
            mScale = value;

            // 设置缩放
            mBodyOb.localScale = new Vector3(value, mBasicScale.y, mBasicScale.z);

            // 设置尾部的位置
            mEndOb.localPosition = new Vector3(mBasicLength * mScale, 0f, 0f);
        }
    }

    public float Length
    { 
        get
        {
            // 获取比例
            return mBasicLength * mScale;
        }
        set
        {
            // 计算当前缩放数值
            Scale = value / mBasicLength;
        }
    }

    #endregion

    /// <summary>
    /// Awake this instance.
    /// </summary>
    public void Awake()
    {
        // 获取原始基础缩放比例
        mBasicScale = mBodyOb.localScale;
    }

    /// <summary>
    /// Start this instance.
    /// </summary>
    public void Start()
    {
    }

    #if UNITY_EDITOR

    /// <summary>
    /// Raises the draw gizmos event.
    /// </summary>
    public void OnDrawGizmos()
    {
        // 获取原始基础缩放比例
        mBasicScale = mBodyOb.localScale;

        // 设置缩放比例
        mBodyOb.localScale = new Vector3(mScale, mBasicScale.y, mBasicScale.z);

        // 设置尾部的位置
        mEndOb.localPosition = new Vector3(mBasicLength * mScale, 0f, 0f);
    }

    #endif
}
