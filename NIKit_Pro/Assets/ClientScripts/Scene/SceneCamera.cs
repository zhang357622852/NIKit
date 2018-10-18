/// <summary>
/// SceneCamera.cs
/// Created by zhaozy 2016/06/15
/// 场景相机
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SceneCamera : MonoBehaviour
{
    // 可滑动的方向
    public enum SceneCameraMovement
    {
        Horizontal,
        Vertical,
        HorizontalAndVertical,
    }

    /// <summary>
    /// Scene type.
    /// </summary>
    public enum SceneType
    {
        MainCity,
        WorldMap,
        Tower,
    }

    [System.Serializable]
    public class CameraInfo
    {
        // 缩放限制
        public float zMinLimit = 0f;
        public float zMaxLimit = 0f;

        // 地图有效区域
        public float mLimitLeftX = 0f;
        public float mLimitRightX = 0f;
        public float mCacheOffsetX = 0f;

        public float mLimitTopY = 0f;
        public float mLimitBottomY = 0f;
        public float mCacheOffsetY = 0f;

        // 边界回弹动画曲线
        public AnimationCurve mAnimationCurve;
        public float backTime = 0.1f;

        // 相机移动速度
        public float mMoveSpeed = 20f;

        // 相机缩放速度
        public float mScaleSpeed = 2.5f;

        public SceneCameraMovement mMovement = SceneCameraMovement.HorizontalAndVertical;

        public SceneType sceneType = SceneType.MainCity;

        // 相机翻页移动的距离
        public float mDistance = 0f;

        // 滑动比例
        public float mSlideRate = 0f;

        // 相机初始位置
        public Vector3 mBottomCameraPos;

        public Vector3 mTopCameraPos;
    }

    // 场景相机
    public Camera sCamera;

    // 默认的场景了类型
    public SceneType mSceneType = SceneType.MainCity;

    // 相机详细信息
    public CameraInfo[] mCameraInfo;

    // 当前相机参数
    [HideInInspector]
    public CameraInfo mCurCameraInfo;

    // 相机的目标位置
    private Vector3 mTargetPos = Vector3.zero;

    // 是否滑动
    private bool mIsSlide = false;

    private Vector3 mMouseUpPos = Vector3.zero;

    private Vector3 mMouseDownPos = Vector3.zero;

    private CallBack mCallBack;

#if ! UNITY_EDITOR
    // 手势缩放地图
    private Vector3 mTouchPosition1 = Vector3.zero;
    private Vector3 mTouchPosition2 = Vector3.zero;
    private bool isScaleTouch = false;
#endif

    // 触屏点点击位置
    private Vector3 mTouchPosition = Vector3.zero;
    private bool isDrugTouch = false;

    // 相机目标位置
    private Vector3 mOffset = Vector3.zero;

    // 回弹位置
    private Vector3 mBackEndPostion = Vector3.zero;
    private Vector3 mBackStartPostion = Vector3.zero;
    private float mBackElapsedTime = 0f;

    // 相机动画
    // (第一个动画为打开场景时Z轴动画，第二个动画为外部调用的动画)
    private TweenPosition[] SceneCameraTween;

    // 编辑器模式下显示地图拖动边界范围框
#if UNITY_EDITOR

    /// <summary>
    /// Raises the draw gizmos event.
    /// </summary>
    void OnDrawGizmos()
    {
        // 获取相机参数
        RefreshCameraInfo();

        // 设置颜色
        Gizmos.color = Color.yellow;

        // 绘制
        Vector3 mLimitAreaBottomLeft = new Vector3(mCurCameraInfo.mLimitLeftX, mCurCameraInfo.mLimitBottomY, 0f);
        Vector3 mLimitAreaTopLeft = new Vector3(mCurCameraInfo.mLimitLeftX, mCurCameraInfo.mLimitTopY, 0f);
        Vector3 mLimitAreaTopRight = new Vector3(mCurCameraInfo.mLimitRightX, mCurCameraInfo.mLimitTopY, 0f);
        Vector3 mLimitAreaBottomRight = new Vector3(mCurCameraInfo.mLimitRightX, mCurCameraInfo.mLimitBottomY, 0f);

        Gizmos.DrawLine(mLimitAreaBottomLeft, mLimitAreaTopLeft); // UpperLeft -> UpperRight
        Gizmos.DrawLine(mLimitAreaTopLeft, mLimitAreaTopRight); // UpperRight -> LowerRight
        Gizmos.DrawLine(mLimitAreaTopRight, mLimitAreaBottomRight); // LowerRight -> LowerLeft
        Gizmos.DrawLine(mLimitAreaBottomRight, mLimitAreaBottomLeft); // LowerLeft -> UpperLeft

        // 计算缓冲区域边界
        Vector3 bottomLeft = mLimitAreaBottomLeft + new Vector3(mCurCameraInfo.mCacheOffsetX, mCurCameraInfo.mCacheOffsetY, 0f);
        Vector3 topLeft = mLimitAreaTopLeft + new Vector3(mCurCameraInfo.mCacheOffsetX, -mCurCameraInfo.mCacheOffsetY, 0f);
        Vector3 topRight = mLimitAreaTopRight - new Vector3(mCurCameraInfo.mCacheOffsetX, mCurCameraInfo.mCacheOffsetY, 0f);
        Vector3 bottomRight = mLimitAreaBottomRight + new Vector3(-mCurCameraInfo.mCacheOffsetX, mCurCameraInfo.mCacheOffsetY, 0f);

        // 设置颜色
        Gizmos.color = Color.red;

        // 绘制缓存区域
        Gizmos.DrawLine(bottomLeft, topLeft); // UpperLeft -> UpperRight
        Gizmos.DrawLine(topLeft, topRight); // UpperRight -> LowerRight
        Gizmos.DrawLine(topRight, bottomRight); // LowerRight -> LowerLeft
        Gizmos.DrawLine(bottomRight, bottomLeft); // LowerLeft -> UpperLeft
    }

#endif

    /// <summary>
    /// Start this instance.
    /// </summary>
    void Start()
    {
        // 获取相机动画组件
        SceneCameraTween = gameObject.GetComponents<TweenPosition>();

        // 刷新相机参数
        RefreshCameraInfo();
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
    }

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        // 1. 场景标准相机不存在
        if (SceneMgr.StdCamera == null)
        {
            // 重置mOffset位移
            mOffset = Vector3.zero;

            // 重置拖拽标识
            isDrugTouch = false;
            return;
        }

        // 如果相机当前处于Tween中不处理
        // 如果当前ui相机发生碰撞了优先处理UI碰撞
        for(int i = 0; i < SceneCameraTween.Length; i++)
        {
            if(SceneCameraTween[i] != null && SceneCameraTween[i].isActiveAndEnabled)
            {
                // 重置mOffset位移
                mOffset = Vector3.zero;

                // 重置拖拽标识
                isDrugTouch = false;

                return;
            }
        }

#if UNITY_EDITOR

        // 编辑器下只能相应鼠标事件

        // 获取鼠标位置
        Vector3 mousePos = Input.mousePosition;

        // 检查屏幕边界
        if (Game.IsOutOfScreen(mousePos))
            return;

        // 如果当前ui相机发生碰撞了优先处理UI碰撞
        if (UICamera.Raycast(mousePos))
            return;

        if (Input.GetMouseButtonDown(0))
        {
            mMouseDownPos = SceneMgr.StdCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, SceneMgr.StdCamera.nearClipPlane));

            mIsSlide = false;
        }
        if (Input.GetMouseButtonUp(0))
        {
            mMouseUpPos = SceneMgr.StdCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, SceneMgr.StdCamera.nearClipPlane));

            mIsSlide = true;
        }

        // 当点击鼠标左键时，执行地图
        if (Input.GetMouseButton(0))
        {
            // 将像素坐标转换为世界坐标
            // 这个地方只是需要转换到相机的近截面，我们需要达到的表现效果是相机移动跟随手势在屏幕上等距离移动
            Vector3 newPos = SceneMgr.StdCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, SceneMgr.StdCamera.nearClipPlane));
            if (!isDrugTouch)
                mTouchPosition = newPos;

            // 计算相机偏移距离
            Vector3 offset = (newPos - mTouchPosition) * sCamera.transform.position.z / sCamera.nearClipPlane;

            // xy的偏移量
            switch (mCurCameraInfo.mMovement)
            {
                // 只允许横向移动
                case SceneCameraMovement.Horizontal :

                    mOffset.x += offset.x;

                    break;

                    // 只允许纵向移动
                case SceneCameraMovement.Vertical :

                    mOffset.y += offset.y;

                    break;

                default :

                    mOffset.x += offset.x;

                    mOffset.y += offset.y;

                    break;
            }

            // 标识正在拖拽中
            isDrugTouch = true;

            // 设置鼠标当前位置
            mTouchPosition = newPos;
        }
        else
        {
            // 重置xy轴向上的位移
            mOffset.x = 0;
            mOffset.y = 0;

            // 重置拖拽标识
            isDrugTouch = false;
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            // Zoom out
            mOffset.z = mOffset.z - Input.GetAxis("Mouse ScrollWheel") * mCurCameraInfo.mScaleSpeed;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            // Zoom in
            mOffset.z = mOffset.z - Input.GetAxis("Mouse ScrollWheel") * mCurCameraInfo.mScaleSpeed;
        }

#else

        // 在正式运营环境中通过Touch获取相关数据

        //判断触摸数量为多点触摸
        if (Input.touchCount == 1)
        {
            // 获取Touch位置
            Vector2 touchPos = Input.GetTouch(0).position;

            // 检查屏幕边界
            if (Game.IsOutOfScreen(touchPos))
                return;

            // 判断当前手指touch状态
            switch (Input.GetTouch(0).phase)
            {
            case TouchPhase.Began:
                // 如果当前ui相机发生碰撞了优先处理UI碰撞
                if (UICamera.Raycast(touchPos))
                    return;

                // 鼠标按下时的世界坐标
                mMouseDownPos = SceneMgr.StdCamera.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, SceneMgr.StdCamera.nearClipPlane));

                // 重置滑动标识
                mIsSlide = false;
                break;

            case TouchPhase.Ended:
                // 如果当前ui相机发生碰撞了优先处理UI碰撞
                if (UICamera.Raycast(touchPos))
                    return;

                mMouseUpPos = SceneMgr.StdCamera.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, SceneMgr.StdCamera.nearClipPlane));

                mIsSlide = true;
                break;

            case TouchPhase.Moved:

                // 如果当前ui相机发生碰撞了优先处理UI碰撞
                if (UICamera.Raycast(touchPos))
                    return;

                // 将像素坐标转换为世界坐标
                // 这个地方只是需要转换到相机的近截面，我们需要达到的表现效果是相机移动跟随手势在屏幕上等距离移动
                Vector3 newPos = SceneMgr.StdCamera.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, SceneMgr.StdCamera.nearClipPlane));
                if (!isDrugTouch)
                    mTouchPosition = newPos;

                // 计算相机偏移距离
                Vector3 offset = (newPos - mTouchPosition) * sCamera.transform.position.z / SceneMgr.StdCamera.nearClipPlane;

                // xy的偏移量
                switch (mCurCameraInfo.mMovement)
                {
                    // 只允许横向移动
                    case SceneCameraMovement.Horizontal :

                        mOffset.x += offset.x;

                        break;

                    // 只允许纵向移动
                    case SceneCameraMovement.Vertical :

                        mOffset.y += offset.y;

                        break;

                    default :

                        mOffset.x += offset.x;

                        mOffset.y += offset.y;

                        break;
                }

                // 标识正在拖拽中
                isDrugTouch = true;

                // 设置鼠标当前位置
                mTouchPosition = newPos;
                break;

            default :
                // 重置xy轴向上的位移
                mOffset.x = 0;
                mOffset.y = 0;

                // 重置拖拽标识
                isDrugTouch = false;
                break;
            }
        }
        else if (Input.touchCount > 1)
        {
            // 两个触摸点都没有移动
            if (Input.GetTouch(0).phase != TouchPhase.Moved &&
                Input.GetTouch(1).phase != TouchPhase.Moved)
                return;

            //计算出当前两点触摸点的位置
            Vector3 tempPosition1 = Input.GetTouch(0).position;
            Vector3 tempPosition2 = Input.GetTouch(1).position;

            // 如果当前ui相机发生碰撞了优先处理UI碰撞
            if (UICamera.Raycast(tempPosition1) ||
                UICamera.Raycast(tempPosition2))
                return;

            // 将像素坐标转换为世界坐标
            Vector3 tempPos1 = SceneMgr.StdCamera.ScreenToWorldPoint(new Vector3(tempPosition1.x, tempPosition1.y, SceneMgr.StdCamera.nearClipPlane));
            if (! isScaleTouch)
                mTouchPosition1 = tempPos1;

            // 将像素坐标转换为世界坐标
            Vector3 tempPos2 = SceneMgr.StdCamera.ScreenToWorldPoint(new Vector3(tempPosition2.x, tempPosition2.y, SceneMgr.StdCamera.nearClipPlane));
            if (!isScaleTouch)
                mTouchPosition2 = tempPos2;

            // 计算位置偏移
            mOffset.z -= (Vector3.Distance(mTouchPosition1, mTouchPosition2) -
                Vector3.Distance(tempPos1, tempPos2)) * mCurCameraInfo.mScaleSpeed;

            // 标识正在缩放中
            isScaleTouch = true;

            //备份上一次触摸点的位置，用于对比
            mTouchPosition1 = tempPos1;
            mTouchPosition2 = tempPos2;
        }
        else
        {
            // 重置mOffset位移
            mOffset = Vector3.zero;

            // 重置拖拽标识
            isDrugTouch = false;

            // 标识正在缩放中
            isScaleTouch = false;
        }

#endif
    }

    /// <summary>
    /// Raises the application pause event.
    /// </summary>
    void OnApplicationPause(bool pauseStatus)
    {
#if ! UNITY_EDITOR
        // 手势缩放地图
        mTouchPosition1 = Vector3.zero;
        mTouchPosition2 = Vector3.zero;
        isScaleTouch = false;
#endif

        // 触屏点点击位置
        mTouchPosition = Vector3.zero;
        isDrugTouch = false;

        // 相机目标位置
        mOffset = Vector3.zero;

        // 回弹位置
        mBackEndPostion = Vector3.zero;
        mBackStartPostion = Vector3.zero;
        mBackElapsedTime = 0f;
    }

    /// <summary>
    /// Lates the update.
    /// </summary>
    void LateUpdate()
    {
        // 如果相机当前处于Tween中不处理
        // 如果当前ui相机发生碰撞了优先处理UI碰撞
        for(int i = 0; i < SceneCameraTween.Length; i++)
        {
            if((SceneCameraTween[i] != null && SceneCameraTween[i].isActiveAndEnabled))
                return;
        }

#if UNITY_EDITOR

        // 玩家当前没有进行屏幕操作
        if (!isTouched() && Game.FloatEqual(mOffset.sqrMagnitude, 0))
        {
            // 玩家不在没有操作相机检测回弹
            DoSpringBack();

            // 移动相机的位置,滑动翻页
            SpringCameraPosition();

            // 返回
            return;
        }

#else

        // 玩家当前没有进行屏幕操作
        if (!isTouched() && Game.FloatEqual(mOffset.sqrMagnitude, 0))
        {
            // 玩家不在没有操作相机检测回弹
            DoSpringBack();

            // 移动相机的位置,滑动翻页
            SpringCameraPosition();

            // 返回
            return;
        }

#endif

        // 没有位移不处理
        if (Game.FloatEqual(mOffset.sqrMagnitude, 0))
            return;

        // 重置数据
        mBackElapsedTime = 0f;

        // 获取当前相机位置
        Vector3 cameraPos = sCamera.transform.position;

        // 计算相机本次偏移XY
        Vector3 offset = Vector3.MoveTowards(Vector3.zero, mOffset, mCurCameraInfo.mMoveSpeed * Time.deltaTime);

        // 修正相机的z轴, z轴变化不做平滑处理直接一步到位
        offset.z = Mathf.Clamp(cameraPos.z + mOffset.z, mCurCameraInfo.zMinLimit, mCurCameraInfo.zMaxLimit) - cameraPos.z;

        // 减去已经位移的距离
        mOffset -= offset;
        mOffset.z = 0;

        sCamera.transform.Translate(offset);

        // 修正相机位置
        DoFixPostion();
    }

    /// <summary>
    /// 移动相机的位置，滑动翻页
    /// </summary>
    private void SpringCameraPosition()
    {
        switch (mSceneType)
        {
            case SceneType.Tower:

                if (mIsSlide)
                {
                    mIsSlide = false;

                    float zDistance = Mathf.Abs(sCamera.transform.position.z);

                    // 获取当前相机的视野边界
                    Vector3 topPos = sCamera.ViewportToWorldPoint(new Vector3(0.5f, 1, zDistance));
                    Vector3 bottomPos = sCamera.ViewportToWorldPoint(new Vector3(0.5f, 0, zDistance));

                    // 上下边缘不作处理
                    if (topPos.y >= mCurCameraInfo.mLimitTopY - mCurCameraInfo.mCacheOffsetY
                        || bottomPos.y <= mCurCameraInfo.mLimitBottomY + mCurCameraInfo.mCacheOffsetY)
                        return;

                    int index = (int) (sCamera.transform.position.y / mCurCameraInfo.mDistance);

                    float indexY = mCurCameraInfo.mBottomCameraPos.y + mCurCameraInfo.mDistance * index;

                    float remainY = sCamera.transform.position.y - indexY;

                    if (Game.FloatEqual(remainY, 0f, 0.001f))
                        return;

                    if (Mathf.Abs(remainY) > Mathf.Abs(topPos.y - bottomPos.y) * mCurCameraInfo.mSlideRate)
                    {
                        if (mMouseUpPos.y < mMouseDownPos.y)
                        {
                            mTargetPos = new Vector3(
                                sCamera.transform.position.x,
                                sCamera.transform.position.y + (mCurCameraInfo.mDistance - Mathf.Abs(remainY)),
                                sCamera.transform.position.z);
                        }
                        else
                        {
                            mTargetPos = new Vector3(
                                sCamera.transform.position.x,
                                sCamera.transform.position.y - (mCurCameraInfo.mDistance - Mathf.Abs(remainY)),
                                sCamera.transform.position.z);
                        }

                        int layer = ((int) (mTargetPos.y / mCurCameraInfo.mDistance) + 1) * 10 - 1;

                        // 抛出相机滑动完成事件
                        EventMgr.FireEvent(EventMgrEventType.EVENT_TOWER_SLIDE, MixedValue.NewMixedValue<int>(layer));
                    }
                    else
                    {
                        // 滑动没有超过指定比例回弹至原始位置
                        mTargetPos = new Vector3(
                            sCamera.transform.position.x,
                            sCamera.transform.position.y - remainY,
                            sCamera.transform.position.z);
                    }
                }

                if (mTargetPos.sqrMagnitude == 0)
                    return;

                if (Game.FloatEqual(mTargetPos.y, sCamera.transform.position.y, 0.001f))
                    return;

                // 修正相机位置
                sCamera.transform.position = Vector3.Lerp(sCamera.transform.position, mTargetPos, Time.unscaledDeltaTime * mCurCameraInfo.mMoveSpeed);

                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Ises the touched.
    /// </summary>
    private bool isTouched()
    {
#if UNITY_EDITOR

        // 编辑器下只能相应鼠标事件
        return Input.GetMouseButton(0);

#else

        // 在正式运营环境中通过Touch时间判断
        return Input.touchCount > 0;

#endif
    }

    /// <summary>
    /// 获取回弹位置
    /// </summary>
    /// <returns>The back postion.</returns>
    private bool CalcSpringBackPostion()
    {
        // 获取当前相机位置
        Vector3 cameraPos = sCamera.transform.position;
        float zDistance = Mathf.Abs(cameraPos.z);

        // 初始化相机起点位置
        mBackStartPostion = cameraPos;

        // 获取当前相机的视野边界
        Vector3 leftPos = sCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, zDistance));
        Vector3 topPos = sCamera.ViewportToWorldPoint(new Vector3(0.5f, 1, zDistance));
        Vector3 rightPos = sCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, zDistance));
        Vector3 bottomPos = sCamera.ViewportToWorldPoint(new Vector3(0.5f, 0, zDistance));

        // 1. x轴向上不能回弹
        if (Game.FloatGreat(mCurCameraInfo.mLimitLeftX + mCurCameraInfo.mCacheOffsetX, leftPos.x) &&
            Game.FloatGreat(rightPos.x, mCurCameraInfo.mLimitRightX - mCurCameraInfo.mCacheOffsetX))
            mBackEndPostion.x = mBackStartPostion.x;
        else if (Game.FloatEqual(rightPos.x, mCurCameraInfo.mLimitRightX, 0.001f))
            mBackEndPostion.x = mBackStartPostion.x - (rightPos.x - mCurCameraInfo.mLimitRightX + mCurCameraInfo.mCacheOffsetX);
        else if (Game.FloatEqual(leftPos.x, mCurCameraInfo.mLimitLeftX, 0.001f))
            mBackEndPostion.x = mBackStartPostion.x - (leftPos.x - mCurCameraInfo.mLimitLeftX - mCurCameraInfo.mCacheOffsetX);
        else
            mBackEndPostion.x = mBackStartPostion.x;

        // 2. y轴向上不能回弹
        if (Game.FloatGreat(topPos.y, mCurCameraInfo.mLimitTopY - mCurCameraInfo.mCacheOffsetY) &&
            Game.FloatGreat(mCurCameraInfo.mLimitBottomY + mCurCameraInfo.mCacheOffsetX, bottomPos.y))
            mBackEndPostion.y = mBackStartPostion.y;
        else if (Game.FloatEqual(topPos.y, mCurCameraInfo.mLimitTopY, 0.001f))
            mBackEndPostion.y = mBackStartPostion.y - (topPos.y - mCurCameraInfo.mLimitTopY + mCurCameraInfo.mCacheOffsetY);
        else if (Game.FloatEqual(bottomPos.y, mCurCameraInfo.mLimitBottomY, 0.001f))
            mBackEndPostion.y = mBackStartPostion.y - (bottomPos.y - mCurCameraInfo.mLimitBottomY - mCurCameraInfo.mCacheOffsetX);
        else
            mBackEndPostion.y = mBackStartPostion.y;

        // 设置相机z轴
        mBackEndPostion.z = cameraPos.z;

        // 不需要回弹
        float distance = (mBackEndPostion - mBackStartPostion).sqrMagnitude;
        if (Game.FloatEqual(distance, 0))
            return false;

        // 返回需要移动
        return true;
    }

    /// <summary>
    /// 检测边界回弹
    /// </summary>
    private void DoSpringBack()
    {
        // 回弹动画还没有开始
        if (Game.FloatEqual(mBackElapsedTime, 0))
        {
            // 计算回弹位置失败
            if (!CalcSpringBackPostion())
                return;
        }

        // 已经移动结束不在处理
        if (Game.FloatGreat(mBackElapsedTime, mCurCameraInfo.backTime))
            return;

        // 累计时间
        mBackElapsedTime += Time.deltaTime;

        // 计算位置
        float rate = Mathf.Clamp01(mBackElapsedTime / mCurCameraInfo.backTime);
        rate = Mathf.Clamp01(mCurCameraInfo.mAnimationCurve.Evaluate(rate));

        // 计算速度
        // 修正相机位置
        sCamera.transform.position = Vector3.Lerp(mBackStartPostion, mBackEndPostion, rate);
    }

    /// <summary>
    /// Dos the fix postion.
    /// </summary>
    private void DoFixPostion()
    {
        // 获取当前相机位置
        Vector3 cameraPos = sCamera.transform.position;
        float zDistance = Mathf.Abs(cameraPos.z);
        Vector3 fixOffset = Vector3.zero;

        // 计算相机当前视口左右边界位置
        Vector3 leftPos = sCamera.ViewportToWorldPoint(new Vector3(0, 0.5f, zDistance));
        Vector3 rightPos = sCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, zDistance));

        // 根据左右越界范围修正位置offset
        if (leftPos.x < mCurCameraInfo.mLimitLeftX)
            fixOffset.x = mCurCameraInfo.mLimitLeftX - leftPos.x;
        else if (rightPos.x > mCurCameraInfo.mLimitRightX)
            fixOffset.x = mCurCameraInfo.mLimitRightX - rightPos.x;
        else
            fixOffset.x = 0f;

        // 计算相机当前视口上下边界位置
        Vector3 topPos = sCamera.ViewportToWorldPoint(new Vector3(0.5f, 1, zDistance));
        Vector3 bottomPos = sCamera.ViewportToWorldPoint(new Vector3(0.5f, 0, zDistance));

        // 根据上下越界范围修正位置offset
        if (topPos.y > mCurCameraInfo.mLimitTopY)
            fixOffset.y = mCurCameraInfo.mLimitTopY - topPos.y;
        else if (bottomPos.y < mCurCameraInfo.mLimitBottomY)
            fixOffset.y = mCurCameraInfo.mLimitBottomY - bottomPos.y;
        else
            fixOffset.y = 0;

        // 修正相机位置
        sCamera.transform.Translate(fixOffset);
    }

    /// <summary>
    /// 初始化相机信息
    /// </summary>
    private void RefreshCameraInfo()
    {
        // 选择相机类型
        CameraInfo cameraPara = null;

        // 遍历数据
        foreach (CameraInfo info in mCameraInfo)
        {
            // 类型相同
            if (info.sceneType != mSceneType)
                continue;

            // 记录数据
            cameraPara = info;
            break;
        }

        // 没有获取到数据
        if (cameraPara == null)
            return;

        switch (mSceneType)
        {
            case SceneType.Tower:

                mCurCameraInfo.mDistance = Mathf.Abs(mCurCameraInfo.mBottomCameraPos.y - mCurCameraInfo.mTopCameraPos.y) / 9;

                break;
            default:
                break;
        }

#if UNITY_EDITOR

        // 记录数据，主要是为了编辑器下能够编辑地图范围参数
        // 如果不copy的话，不能编辑当前使用的cameraPara
        mCurCameraInfo = new CameraInfo();
        mCurCameraInfo.zMinLimit = cameraPara.zMinLimit;
        mCurCameraInfo.zMaxLimit = cameraPara.zMaxLimit;
        mCurCameraInfo.mLimitLeftX = cameraPara.mLimitLeftX;
        mCurCameraInfo.mLimitRightX = cameraPara.mLimitRightX;
        mCurCameraInfo.mCacheOffsetX = cameraPara.mCacheOffsetX;
        mCurCameraInfo.mLimitTopY = cameraPara.mLimitTopY;
        mCurCameraInfo.mLimitBottomY = cameraPara.mLimitBottomY;
        mCurCameraInfo.mCacheOffsetY = cameraPara.mCacheOffsetY;
        mCurCameraInfo.mAnimationCurve = cameraPara.mAnimationCurve;
        mCurCameraInfo.backTime = cameraPara.backTime;
        mCurCameraInfo.mMoveSpeed = cameraPara.mMoveSpeed;
        mCurCameraInfo.mScaleSpeed = cameraPara.mScaleSpeed;
        mCurCameraInfo.mMovement = cameraPara.mMovement;
        mCurCameraInfo.sceneType = cameraPara.sceneType;
        mCurCameraInfo.mDistance = cameraPara.mDistance;
        mCurCameraInfo.mSlideRate = cameraPara.mSlideRate;
        mCurCameraInfo.mBottomCameraPos = cameraPara.mBottomCameraPos;
        mCurCameraInfo.mTopCameraPos = cameraPara.mTopCameraPos;

#else

        // 直接赋值
        mCurCameraInfo = cameraPara;

# endif
    }

    /// <summary>
    /// 动画执行完成回调
    /// </summary>
    void OnTweenFinish()
    {
        // 执行回调
        if (mCallBack != null)
            mCallBack.Go();
    }

    /// <summary>
    /// Moves the camera.
    /// </summary>
    /// <param name="fromPosition">From position.</param>
    /// <param name="toPosition">To position.</param>
    /// <param name="duration">Duration.</param>
    /// <param name="delay">Delay.</param>
    /// <param name="trackCurve">Track curve.</param>
    public void MoveCamera(Vector3 fromPosition, Vector3 toPosition, float duration, float delay, AnimationCurve trackCurve, CallBack callBack = null)
    {
        // 相机动画组件不存在获取一下
        if (SceneCameraTween == null)
            SceneCameraTween = gameObject.GetComponents<TweenPosition>();

        // 没有动画组件
        if(SceneCameraTween == null || SceneCameraTween.Length == 0)
            return;

        // 获取动画组件
        TweenPosition tw = SceneCameraTween[SceneCameraTween.Length - 1];
        if (tw == null)
            return;

        mCallBack = callBack;

        // 重置动画效果
        tw.ResetToBeginning();
        tw.animationCurve = trackCurve;
        tw.duration = duration;
        tw.delay = delay;
        tw.from = fromPosition;
        tw.to = toPosition;

        tw.AddOnFinished(OnTweenFinish);

        tw.PlayForward();
    }

    /// <summary>
    /// 移动场景相机（以tween动画）
    /// </summary>
    /// <param name="toPosition">To position.</param>
    /// <param name="formPosition">Form position.</param>
    /// <param name="duiation">Duiation.</param>
    /// <param name="delay">Delay.</param>
    public void MoveCamera(Vector3 fromPosition, Vector3 toPosition, float duration = 0.3f, float delay = 0f, CallBack callBack = null)
    {
        MoveCamera(fromPosition, toPosition, duration, delay, AnimationCurve.EaseInOut(0, 0, 1, 1), callBack);
    }

    /// <summary>
    /// 设置场景类型
    /// </summary>
    public void SetSceneType(SceneType type)
    {
        // 重新mSceneType
        mSceneType = type;

        mTargetPos = Vector3.zero;

        // 刷新相机参数
        RefreshCameraInfo();
    }

    /// <summary>
    /// 结束当前场景相机的动画
    /// </summary>
    public void EndSceneCameraAnimation()
    {
        // 相机动画组件不存在获取一下
        if (SceneCameraTween == null)
            SceneCameraTween = gameObject.GetComponents<TweenPosition>();

        // 将
        for (int i = 0; i < SceneCameraTween.Length; i++)
        {
            SceneCameraTween[i].SetEndToCurrentValue();

            SceneCameraTween[i].enabled = false;
        }
    }
}
