/// <summary>
/// SceneWnd.cs
/// Created by zhaozy 2016/06/15
/// 场景窗口处理脚本
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainSceneWnd : MonoBehaviour
{
    // 场景相机
    public Camera SceneCamera;

    // 相机动画
    public TweenPosition SceneCameraTween;

    // 是否是2d碰撞
    public bool Is2DRaycastHit = false;

    // 获取鼠标上一次点击位置
    private Vector3 mTouchPosition = Vector3.zero;
    private bool mIsTouched = false;

#if (UNITY_ANDROID || UNITY_IPHONE) && ! UNITY_EDITOR
    private int curTouchId = -1;
#endif

#if (UNITY_ANDROID || UNITY_IPHONE) && ! UNITY_EDITOR
    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        // 如果相机当前处于Tween中不处理
        // 当角色没有登陆成功，或者登陆动画还在变化中不处理
        if (SceneCameraTween != null &&
            (!ME.isLoginOk || SceneCameraTween.isActiveAndEnabled))
        {
            // 标识mIsTouched
            mIsTouched = false;
            return;
        }

        // 只是相应单指点击
        if (Input.touchCount != 1)
        {
            mIsTouched = false;
            mTouchPosition = Vector3.zero;
            curTouchId = -1;

            return;
        }

        // 判断当前手指touch状态
        switch (Input.GetTouch(0).phase)
        {
            case TouchPhase.Began:

                // 获取当前Touch
                Touch touch = Input.GetTouch(0);

                // 如果当前ui相机发生碰撞了优先处理UI碰撞
                if (UICamera.Raycast(touch.position))
                {
                    // 重置mIsTouched标识
                    mIsTouched = false;
                    curTouchId = -1;
                    mTouchPosition = Vector3.zero;
                    return;
                }

                // 标识mIsTouched
                mIsTouched = true;
                curTouchId = touch.fingerId;
                mTouchPosition = touch.position;

                // 开始
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:

                // 获取当前Touch
                Touch endedTouch = Input.GetTouch(0);

                // 如果当前ui相机发生碰撞了优先处理UI碰撞
                if (UICamera.Raycast(endedTouch.position))
                {
                    // 重置mIsTouched标识
                    mIsTouched = false;
                    curTouchId = -1;
                    mTouchPosition = Vector3.zero;
                    return;
                }

                Vector3 curPos = endedTouch.position;
                Vector3 delta = curPos - mTouchPosition;

                // 如果没有mIsTouched
                // curTouchId发生了变化
                // 位置变化太大了也不触发点击事件
                if (! mIsTouched ||
                    curTouchId != endedTouch.fingerId ||
                    Game.convertDistanceFromPointToInch(delta.magnitude) > ConstantValue.MOVE_INCH)
                    break;

                // 重置mIsTouched标识
                mIsTouched = false;
                curTouchId = -1;
                mTouchPosition = Vector3.zero;

                // 执行碰撞
                DoRayCast(curPos);

                break;

            default:
                break;
        }
    }
#else
    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        // 如果相机当前处于Tween中不处理
        // 当角色没有登陆成功，或者登陆动画还在变化中不处理
        if (SceneCameraTween != null &&
            (!ME.isLoginOk || SceneCameraTween.isActiveAndEnabled))
        {
            // 标识mIsTouched
            mIsTouched = false;
            return;
        }

        // 如果是鼠标右键Down事件
        if (Input.GetMouseButtonDown(0))
        {
            // 如果当前ui相机发生碰撞了优先处理UI碰撞
            if (UICamera.Raycast(Input.mousePosition))
            {
                mIsTouched = false;
                mTouchPosition = Vector3.zero;
                return;
            }

            // 记录当前鼠标位置
            mTouchPosition = Input.mousePosition;

            // 标识mIsTouched
            mIsTouched = true;

            return;
        }

        // 如果是鼠标右键up事件
        if (Input.GetMouseButtonUp(0))
        {
            // 获取鼠标当前位置
            Vector3 curPos = Input.mousePosition;

            // 如果当前ui相机发生碰撞了优先处理UI碰撞
            if (UICamera.Raycast(curPos))
            {
                mIsTouched = false;
                mTouchPosition = Vector3.zero;
                return;
            }

            // 如果鼠标位置有变化不处理
            // 如果mIsTouched为false表示没有点击
            Vector3 delta = curPos - mTouchPosition;
            if (Game.convertDistanceFromPointToInch(delta.magnitude) > ConstantValue.MOVE_INCH ||
                ! mIsTouched)
            {
                // 重置mIsTouched标识和mTouchPosition
                mIsTouched = false;
                mTouchPosition = Vector3.zero;
                return;
            }

            // 重置mIsTouched标识和mMousePosition
            mIsTouched = false;
            mTouchPosition = Vector3.zero;

            // 执行碰撞
            DoRayCast(curPos);

            return;
        }

        // 如果是其他鼠标按键事件
        if (Input.GetMouseButton(1) ||
            Input.GetMouseButton(2) ||
            Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            mIsTouched = false;
            mTouchPosition = Vector3.zero;
        }
    }
#endif

    /// <summary>
    /// Raises the application pause event.
    /// </summary>
    void OnApplicationPause(bool pauseStatus)
    {
#if (UNITY_ANDROID || UNITY_IPHONE) && ! UNITY_EDITOR
        curTouchId = -1;
#endif

        mIsTouched = false;
        mTouchPosition = Vector3.zero;
    }

    /// <summary>
    /// 场景激活的回调
    /// </summary>
    void OnEnable()
    {
        // 当角色登陆成功，或者SceneCameraTween不存在
        if (ME.isLoginOk || SceneCameraTween == null)
            return;

        // 激活相机动画
        SceneCameraTween.enabled = true;

        // 添加回调函数,SceneCameraTween动画播放完成执行TweenPositionOnfinish方法
        EventDelegate.Add(SceneCameraTween.onFinished, OnCameraTweenFinish);
    }

    /// <summary>
    /// Whens the camera tween.
    /// </summary>
    private void OnCameraTweenFinish()
    {
        // 标识玩家登陆成功
        ME.OnLoginOK();
    }

    /// <summary>
    /// Dos the ray cast.
    /// </summary>
    private void DoRayCast(Vector3 position)
    {
        // 检查屏幕边界
        if (Game.IsOutOfScreen(position))
            return;

        GameObject gameObject;

        // 如果是2D碰撞
        if (Is2DRaycastHit)
        {
            // 默认2d碰撞放在z为0的位置
            Vector3 wp = SceneCamera.ScreenToWorldPoint(new Vector3(position.x, position.y, -SceneCamera.transform.position.z));
            RaycastHit2D hit2D = Physics2D.Raycast(wp, Vector2.zero);

            // 没有碰撞到任何目标
            if (hit2D.collider == null)
                return;

            // 获取碰撞到的GameObject
            gameObject = hit2D.transform.gameObject;
        }
        else
        {
            // 发出射线
            Ray ray = SceneCamera.ScreenPointToRay(position);
            RaycastHit hit;

            // 执行碰撞检测
            if (!Physics.Raycast(ray, out hit, SceneCamera.farClipPlane))
                return;

            // 获取碰撞到的GameObject
            gameObject = hit.transform.gameObject;
        }

        // 通知碰撞实体执行点击场景wnd事件
        SceneWnd wnd = gameObject.GetComponent<SceneWnd>();
        if (wnd != null)
        {
            wnd.OnClickWnd();
            return;
        }

        SceneNpcWnd sceneNpcWnd = gameObject.GetComponent<SceneNpcWnd>();
        if (sceneNpcWnd != null)
        {
            sceneNpcWnd.OnClickWnd();

            return;
        }

        // 执行点击事件
        MapNameWnd mapWnd = gameObject.GetComponent<MapNameWnd>();
        if (mapWnd != null)
        {
            mapWnd.OnClickWnd();
            return;
        }

        // 通天之塔
        TowerSceneItem towerSceneItem = gameObject.GetComponent<TowerSceneItem>();
        if (towerSceneItem != null)
        {
            // 执行点击
            towerSceneItem.OnClickWnd();

            return;
        }
    }
}
