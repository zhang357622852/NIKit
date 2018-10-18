/// <summary>
/// AdjustCamera.cs
/// Created by fucj 2014-11-14
/// 相机适配脚本
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdjustCamera : MonoBehaviour
{
    /// <summary>
    /// 当前相机
    /// </summary>
    public Camera mCamera;

    /// <summary>
    /// 相机原始位置
    /// </summary>T
    public Vector3 mCameraInitPosotion = Vector3.zero;

    /// <summary>
    /// 相机TweenPosotion列表
    /// </summary>
    public List<Vector3> mTweenPosotion = new List<Vector3>();

    /// <summary>
    /// 当前屏幕分辨率
    /// </summary>
    private float aspect = 0f;

    /// <summary>
    /// The m rate.
    /// </summary>
    private float mRate = 0f;

    /// <summary>
    /// 相机动画
    /// (第一个动画为打开场景时Z轴动画，第二个动画为外部调用的动画)
    /// </summary>
    private TweenPosition[] mSceneCameraTween;

    /// <summary>
    /// 登陆场景动画结束
    /// </summary>
    public void DoLoginTweenEnd()
    {
        // mSceneCameraTween动画
        if (mSceneCameraTween.Length == 0)
            return;

        // 找到需要播放动画的组件
        foreach(TweenPosition tween in mSceneCameraTween)
        {
            if (tween.tweenGroup == 0)
                continue;

            // 开始播放动画
            tween.enabled = true;
            tween.PlayForward();
        }
    }

    /// <summary>
    /// Awake this instance.
    /// </summary>
    void Awake()
    {
        // 获取rate
        aspect = mCamera.aspect;
        if (aspect > ConstantValue.StdAspect)
            mRate = 1f;
        else
            mRate = ConstantValue.StdAspect / aspect;

        // 获取动画组件
        mSceneCameraTween = gameObject.GetComponents<TweenPosition>();

        // 修正位置
        mCamera.transform.position = new Vector3(
            mCameraInitPosotion.x, 
            mCameraInitPosotion.y,
            mCameraInitPosotion.z * mRate);

        // 修正相机动画组件
        for (int i = 0; i < mSceneCameraTween.Length; i++)
        {
            // 动画原始位置
            if (mTweenPosotion.Count <= i)
                continue;

            // 设置动画开始位置
            int index = i * 2;
            mSceneCameraTween[i].from = new Vector3(
                mTweenPosotion[index].x,
                mTweenPosotion[index].y,
                mTweenPosotion[index].z * mRate);

            // 设置动画结束位置
            index = index + 1;
            mSceneCameraTween[i].to = new Vector3(
                mTweenPosotion[index].x,
                mTweenPosotion[index].y,
                mTweenPosotion[index].z * mRate);
        }
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        // 如果aspect没有变化
        if (Game.FloatEqual(aspect, mCamera.aspect))
            return;

        // 获取ShakeCamera组件, 如果分辨率变化则立马停止相机抖动
        ShakeCamera cam = mCamera.GetComponent<ShakeCamera>();
        if (cam != null && cam.isActiveAndEnabled)
            cam.DoShakeEnd();

        // 获取rate
        aspect = mCamera.aspect;

        float newRate = 0f;
        if (aspect > ConstantValue.StdAspect)
            newRate = 1f;
        else
            newRate = ConstantValue.StdAspect / aspect;

        // 修正位置
        Vector3 pos = mCamera.transform.position;
        mCamera.transform.position = new Vector3(
            pos.x, 
            pos.y,
            pos.z / mRate * newRate);

        // 重置mRate
        mRate = newRate;

        // 修正相机动画组件
        for (int i = 0; i < mSceneCameraTween.Length; i++)
        {
            // 动画原始位置
            if (mTweenPosotion.Count <= i)
                continue;

            // 设置动画开始位置
            int index = mSceneCameraTween[i].tweenGroup * 2;
            mSceneCameraTween[i].from = new Vector3(
                mTweenPosotion[index].x,
                mTweenPosotion[index].y,
                mTweenPosotion[index].z * mRate);

            // 设置动画结束位置
            index = index + 1;
            mSceneCameraTween[i].to = new Vector3(
                mTweenPosotion[index].x,
                mTweenPosotion[index].y,
                mTweenPosotion[index].z * mRate);
        }
    }
}
