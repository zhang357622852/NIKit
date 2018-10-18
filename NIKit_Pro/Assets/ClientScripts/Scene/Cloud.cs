/// <summary>
/// Cloud.cs
/// Created by zhaozy 2018/08/24
/// 动态云处理模块
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cloud : MonoBehaviour
{
    [System.Serializable]
    public class CloudMoveRange
    {
        // 云层X位置范围
        public float mLeftX = 0f;
        public float mRightX = 0f;

        // 云层Y位置范围
        public float mTopY = 0f;
        public float mBottomY = 0f;

        // 云层高度范围
        public float mBackZ = 0f;
        public float mFrontZ = 0f;
    }

    [System.Serializable]
    public class CloudMoveSpeed
    {
        // 移动速度
        public float mMax = 0f;
        public float mMin = 0f;
    }

    /// <summary>
    /// 云移动方向类型
    /// </summary>
    [System.Serializable]
    public enum MoveDirectionType
    {
        // 从做左向右
        MoveToRight,

        // 从做右向左
        MoveToLeft,
    }

    /// <summary>
    /// 云移动方向类型
    /// </summary>
    [System.Serializable]
    public enum DriveType
    {
        // 没有任何驱动方式
        None,

        // 渐入驱动方式
        FadeIn,

        // 渐出驱动方式
        FadeOut,

        // 移动驱动方式
        Move,
    }

#region 公共属性变量

    /// <summary>
    /// 云移动方向
    /// </summary>
    public MoveDirectionType mMoveDirection = MoveDirectionType.MoveToRight;

    /// <summary>
    /// 移动范围
    /// </summary>
    public CloudMoveRange mMoveRange;

    /// <summary>
    /// 移动速度
    /// </summary>
    public CloudMoveSpeed mMoveSpeed;

    /// <summary>
    /// 最小移动距离
    /// </summary>
    public float mMinMoveDistance = 10f;

    /// <summary>
    /// 渐入渐出时间
    /// </summary>
    public float mFadeTime = 2f;

    /// <summary>
    /// 相机高度带来的mHighAlpha
    /// </summary>
    public float mAlphaHigh = 5f;

    /// <summary>
    /// 云实体
    /// </summary>
    public SpriteRenderer mCloud;

    /// <summary>
    /// 云阴影
    /// </summary>
    public SpriteRenderer mShadow;

#endregion

#region 内部变量

    /// <summary>
    /// 当前移动速度
    /// </summary>
    private float mCurSpeed = 0f;

    /// <summary>
    /// 云移动起点
    /// </summary>
    private Vector3 mStarPos = Vector3.zero;

    /// <summary>
    /// 云移动终点
    /// </summary>
    private Vector3 mEndPos = Vector3.zero;

    /// <summary>
    /// 渐变剩余时间
    /// </summary>
    private float mFadeRemainTime = 0f;

    /// <summary>
    /// 当前驱动方式
    /// </summary>
    private DriveType mDriveType = DriveType.None;

    /// <summary>
    /// 相机z轴位置
    /// </summary>
    private float mCameraZ = 0f;

    /// <summary>
    /// 获取相机透明度
    /// </summary>
    private float mCameraAlpha = 1f;

    /// <summary>
    /// 获取渐变透明度
    /// </summary>
    private float mFadeAlpha = 1f;

#endregion

#region 内部接口

    /// <summary>
    /// 生成云效果
    /// </summary>
    private void GenerateCloud()
    {
        // 云移动Y坐标不需要变化，一旦确定知道云消失
        float posY = Random.Range(mMoveRange.mTopY, mMoveRange.mBottomY);

        // 生成云云移动起点
        if (mMoveDirection == MoveDirectionType.MoveToRight)
        {
            // 生成起点
            mStarPos = new Vector3(
                Random.Range(mMoveRange.mLeftX, mMoveRange.mRightX - mMinMoveDistance),
                posY,
                0f
            );

            // 生成云移动终点
            mEndPos = new Vector3(
                mStarPos.x + Random.Range(mMinMoveDistance, mMoveRange.mRightX - mStarPos.x),
                posY,
                0f
            );
        }
        else
        {
            // 生成起点
            mStarPos = new Vector3(
                Random.Range(mMoveRange.mLeftX + mMinMoveDistance, mMoveRange.mRightX),
                posY,
                0f
            );

            // 生成云移动终点
            mEndPos = new Vector3(
                mStarPos.x - Random.Range(mMinMoveDistance, mStarPos.x - mMoveRange.mLeftX),
                posY,
                0f
            );
        }

        // 设置云的起始位置
        gameObject.transform.localPosition = mStarPos;

        // 设置云层高度
        Vector3 pos = mCloud.transform.localPosition;
        pos.z = Random.Range(mMoveRange.mBackZ, mMoveRange.mFrontZ);
        mCloud.transform.localPosition = pos;

        // 设置透明度
        SetCloudFadeAlpha(0f);

        // 生成云的移动范围
        mCurSpeed = Random.Range(mMoveSpeed.mMin, mMoveSpeed.mMax);

        // 渐变剩余时间
        mFadeRemainTime = mFadeTime;

        // 标识正在渐变中
        mDriveType = DriveType.FadeIn;
    }

    /// <summary>
    /// 设置云相机带来的透明度
    /// </summary>
    /// <param name="alpha">Alpha.</param>
    private void SetCloudCameraAlpha(float cameraAlpha)
    {
        // 如果cameraAlpha没有发生任何变化不处理
        if (Game.FloatEqual(cameraAlpha, mCameraAlpha))
            return;

        // 缓存数据
        mCameraAlpha = cameraAlpha;

        // 设置云透明度
        Color cloudColor = mCloud.color;
        cloudColor.a = mCameraAlpha * mFadeAlpha;
        mCloud.color = cloudColor;
    }

    /// <summary>
    /// 设置云的透明度
    /// </summary>
    /// <param name="alpha">Alpha.</param>
    private void SetCloudFadeAlpha(float fadeAlpha)
    {
        // 如果cameraAlpha没有发生任何变化不处理
        if (Game.FloatEqual(mFadeAlpha, fadeAlpha))
            return;

        // 缓存数据
        mFadeAlpha = fadeAlpha;

        // 设置云透明度
        Color cloudColor = mCloud.color;
        cloudColor.a = mCameraAlpha * mFadeAlpha;
        mCloud.color = cloudColor;

        // 设置云阴影的alpha
        Color shadowColor = mCloud.color;
        shadowColor.a = mFadeAlpha;
        mShadow.color = shadowColor;
    }

    /// <summary>
    /// Start this instance.
    /// </summary>
    void Start()
    {
        // 生成云效果
        GenerateCloud();
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>
    void Update()
    {
        // 根据相机位置调整透明度
        DoAdjustCameraAlpha();

        // 驱动更新位置
        DoDrive();
    }

    /// <summary>
    /// Dos the adjust camera alpha.
    /// </summary>
    private void DoAdjustCameraAlpha()
    {
        // 获取场景相机
        Camera camera = SceneMgr.SceneCamera;

        // 场景相机不存在
        if (camera == null)
            return;

        // 获取当前相机位置
        float cameraZ = camera.transform.position.z;

        // 位置没有发生变化
        if (Game.FloatEqual(mCameraZ, cameraZ))
            return;

        // 记录位置
        mCameraZ = cameraZ;

        // 获取近界面位置
        float nearClipPlaneZ = mCameraZ + camera.nearClipPlane;

        // 设置云层高度
        float cloudz = mCloud.transform.localPosition.z;

        // 云层在近界面后面
        if (nearClipPlaneZ > cloudz)
        {
            SetCloudCameraAlpha(0f);
        }
        else
        {
            SetCloudCameraAlpha(Mathf.Min(cloudz - nearClipPlaneZ, mAlphaHigh) / mAlphaHigh);
        }
    }

    /// <summary>
    /// 驱动移动
    /// </summary>
    private void DoDrive()
    {
        // 如果当前没有驱动方式，不处理
        if (mDriveType == DriveType.None)
            return;

        // 如果当前是渐入
        if (mDriveType == DriveType.FadeIn)
        {
            // 计算剩余时间
            mFadeRemainTime -= Time.unscaledDeltaTime;

            // 渐变结束
            if (mFadeRemainTime < 0)
            {
                // 标识驱动类型为None
                mDriveType = DriveType.Move;

                // 设置云的alpha
                SetCloudFadeAlpha(1f);
            }
            else
            {
                // 设置云的alpha
                SetCloudFadeAlpha(1f - mFadeRemainTime / mFadeTime);
            }
        }

        // 如果当前是渐出
        if (mDriveType == DriveType.FadeOut)
        {
            // 计算剩余时间
            mFadeRemainTime -= Time.unscaledDeltaTime;

            // 渐变结束
            if (mFadeRemainTime < 0)
            {
                // 标识驱动类型为None
                mDriveType = DriveType.None;

                // 设置云的alpha
                SetCloudFadeAlpha(0f);

                // 生成云效果
                GenerateCloud();

                // 退出驱动
                return;
            }

            // 设置云的alpha
            SetCloudFadeAlpha(mFadeRemainTime / mFadeTime);
        }

        // 两者距离已经小于deltaDistance
        Vector3 curPos = gameObject.transform.localPosition;
        float curPosX = 0f;

        // 计算目标位置
        if (mMoveDirection == MoveDirectionType.MoveToRight)
        {
            // 获取下一次位置坐标
            curPosX = curPos.x + Time.unscaledDeltaTime * mCurSpeed;

            // 判断是否需要渐变退出
            if (mEndPos.x > curPos.x && curPosX > mEndPos.x)
            {
                // 设置渐变退出
                mDriveType = DriveType.FadeOut;
                mFadeRemainTime = mFadeTime;
            }

            // 重置x坐标
            curPos.x = curPosX;
        }
        else
        {
            // 获取下一次位置坐标
            curPosX = curPos.x - Time.unscaledDeltaTime * mCurSpeed;

            // 判断是否需要渐变退出
            if (mEndPos.x < curPos.x && curPosX < mEndPos.x)
            {
                // 设置渐变退出
                mDriveType = DriveType.FadeOut;
                mFadeRemainTime = mFadeTime;
            }

            // 重置x坐标
            curPos.x = curPosX;
        }

        // 设置位置
        gameObject.transform.localPosition = curPos;
    }

#endregion
}
