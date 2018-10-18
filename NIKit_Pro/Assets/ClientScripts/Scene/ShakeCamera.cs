/// <summary>
/// InitCamera.cs
/// Created by fucj 2014-11-14
/// 初始化相机脚本
/// </summary>

using UnityEngine;
using System.Collections;

public class ShakeCamera : MonoBehaviour
{
    #region 变量

    private float mElapseTime = 0f;

    private TrackInfo mTrack;
    private float mDuration = 0f;

    // 相机对象
    private Camera mCamera;

    // 相机的原始位置
    private Vector3 OriginalPosition = Vector3.zero;
    private bool isInIt = false;

    // track结束回调地址
    private CallBack mCallBack = null;

    #endregion

    // Use this for initialization
    void Start()
    {
    }

    /// <summary>
    /// 场景激活的回调
    /// </summary>
    void OnEnable()
    {
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDisable()
    {
        // 重置初始化标识
        isInIt = false;

        // 重置数据
        mTrack = null;
        mDuration = 0f;
        mElapseTime = 0f;
        mCallBack = null;
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void Update()
    {
        // 相机对象不存在
        if (mTrack == null)
            return;

        // 计算累计消耗时间
        mElapseTime += Time.deltaTime;

        // 设置位置
        gameObject.transform.position = OriginalPosition + mTrack.Evaluate(mElapseTime / mDuration);

        // 抛出eventName
        if (Game.FloatGreat(mDuration, mElapseTime))
            return;

        // 重置相机到起始位置
        gameObject.transform.position = OriginalPosition;

        // 判断是否需要执行mCallBack
        if (mCallBack != null)
            mCallBack.Go();

        // Active(false)
        this.enabled = false;
    }

    /// <summary>
    /// Dos the shake end.
    /// </summary>
    public void DoShakeEnd()
    {
        // 结束track
        mTrack = null;

        // 重置相机到起始位置
        if (isInIt)
            gameObject.transform.position = OriginalPosition;

        // 标识已经初始化
        isInIt = false;

        // 判断是否需要执行mCallBack
        if (mCallBack != null)
            mCallBack.Go();

        // 重置数据
        mTrack = null;
        mDuration = 0f;
        mElapseTime = 0f;
        mCallBack = null;

        // Active(false)
        this.enabled = false;
    }

    /// <summary>
    /// Dos the shake.
    /// </summary>
    /// <param name="track">Track.</param>
    /// <param name="duration">Duration.</param>
    /// <param name="cookie">Cookie.</param>
    public void DoShake(TrackInfo track, float duration, string cookie, CallBack cb)
    {
        // 还没有初始化
        if (! isInIt)
        {
            // 获取相机的原始位置
            OriginalPosition = gameObject.transform.position;

            // 标识已经初始化
            isInIt = true;
        }

        // 记录数据
        mTrack = track;
        mDuration = duration;

        // 初始mElapseTime
        mElapseTime = 0f;

        // 记录mCallBack
        mCallBack = cb;
    }
}
