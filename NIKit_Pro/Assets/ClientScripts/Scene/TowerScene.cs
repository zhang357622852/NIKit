/// <summary>
/// TowerSceneAnimation.cs
/// Created by fengsc 2017/08/28
/// 通天之塔场景
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class TowerScene : MonoBehaviour
{
    // 地图角度变化动画组件
    public TweenRotation mMapTweenRotation;

    // 地图大小变化动画组件
    public TweenScale mMapTweenScale;

    public TweenPosition mCameraTweenPosition;

    public GameObject mTowerScene;

    public GameObject mMapParent;

    // 通天之塔实体
    public GameObject mTowerMap;

    public Vector3 mCameraFromPos;

    public Vector3 mCameraToPos;

    public GameObject mEasyTowerSceneItems;

    public GameObject mHardTowerSceneItems;

    public GameObject mEasyTower;

    public GameObject mHardTower;

    public AnimationCurve mAnimationCurve;

    // 当前难度的通关最高层数
    private int mMaxLayer = 0;

    // 通天塔当前选择的难度
    private int mDiff;

    private Dictionary<string, GameObject> mItems = new Dictionary<string, GameObject>();

    private string mInstanceId = string.Empty;

    // Use this for initialization
    void Start ()
    {
        if (mTowerScene.activeInHierarchy)
            mTowerScene.SetActive(false);

        // 注册事件
        RegisterEvent();
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("TowerScene");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 监听EVENT_TOWER_SCENE事件
        EventMgr.RegisterEvent("TowerScene", EventMgrEventType.EVENT_OPEN_TOWER_SCENE, OnEventOpenTowerScene);

        // 监听EVENT_CLOSE_TOWER_SCENE事件
        EventMgr.RegisterEvent("TowerScene", EventMgrEventType.EVENT_CLOSE_TOWER_SCENE, OnEventCloseTowerScene);

        // 监听EVENT_REFRESH_TOWER事件
        EventMgr.RegisterEvent("TowerScene", EventMgrEventType.EVENT_SWITCH_TOWER_DIFFICULTY, OnEventRefreshTower);
    }

    /// <summary>
    /// 获取通天塔层数基础格子
    /// </summary>
    void GetItems()
    {
        mItems.Clear();

        GameObject temp = null;

        if (mDiff.Equals(TowerConst.EASY_TOWER))
            temp = mEasyTowerSceneItems;
        else
            temp = mHardTowerSceneItems;

        for (int i = 0; i < temp.transform.childCount; i++)
        {
            GameObject item = temp.transform.GetChild(i).gameObject;
            if (item == null)
                continue;

            string name = item.name;

            if (mItems.ContainsKey(name))
                continue;

            mItems.Add(name, item);
        }
    }

    /// <summary>
    /// 重绘场景窗口
    /// </summary>
    void Redraw()
    {
        GetItems();

        CsvRow towerInfo = TowerMgr.GetTowerInfoByInstance(mInstanceId);

        if (towerInfo != null && TowerMgr.IsClearanced(ME.user, mDiff, towerInfo.Query<int>("layer")))
        {
            mMaxLayer = towerInfo.Query<int>("layer");
        }
        else
        {
            // 获取对应难度通关最高层数
            mMaxLayer = TowerMgr.GetMaxClearanceLayer(ME.user, mDiff);
        }

        // 获取某个难度的副本列表
        List<CsvRow> list = TowerMgr.GetTowerListByDiff(mDiff);
        if (list == null)
            return;

        if (list.Count > mItems.Count)
            return;

        if (mDiff.Equals(TowerConst.EASY_TOWER))
        {
            mEasyTower.SetActive(true);

            mHardTower.SetActive(false);

            mEasyTowerSceneItems.SetActive(true);

            mHardTowerSceneItems.SetActive(false);
        }
        else if (mDiff.Equals(TowerConst.HARD_TOWER))
        {
            mEasyTower.SetActive(false);

            mHardTower.SetActive(true);

            mEasyTowerSceneItems.SetActive(false);

            mHardTowerSceneItems.SetActive(true);
        }
        else
        {
        }

        for (int i = 0; i < list.Count; i++)
        {
            // 通天塔副本配置数据
            CsvRow row = list[i];
            if (row == null)
                continue;

            GameObject go;

            if (!mItems.TryGetValue((i + 1).ToString(), out go))
                continue;

            TowerSceneItem item = go.GetComponent<TowerSceneItem>();
            if (item == null)
                continue;

            // 绑定数据
            item.Bind(row, mDiff);
        }
    }

    /// <summary>
    /// 通天塔数据刷新事件回调
    /// </summary>
    void OnEventRefreshTower(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();
        if (data == null)
            return;

        mDiff = data.GetValue<int>("difficulty");

        SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
        if (control != null)
            control.SetSceneType(SceneCamera.SceneType.Tower);

        if (data.ContainsKey("instance_id"))
            mInstanceId = data.GetValue<string>("instance_id");
        else
            mInstanceId = string.Empty;

        // 刷新通天之塔
        Redraw();

        // 移动相机
        MoveCamera();
    }

    /// <summary>
    /// EVENT_TOWER_SCENE事件回调
    /// </summary>
    void OnEventOpenTowerScene(int eventId, MixedValue para)
    {
        mTowerScene.SetActive(true);

        if (mTowerMap != null)
            mTowerMap.SetActive(false);

        LPCMapping data = para.GetValue<LPCMapping>();
        if (data != null)
            mDiff = data.GetValue<int>("difficulty");

        mInstanceId = string.Empty;

        // 刷新通天之塔
        Redraw();

        // 播放动画切换
        if (!data.ContainsKey("is_play_forward") || data["is_play_forward"].AsInt != 0)
        {
            PlayForward();
        }
        else
        {
            mMapParent.transform.localScale = new Vector3(3, 3, 1);

            mMapParent.transform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
        }

        SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
        if (control != null)
            control.SetSceneType(SceneCamera.SceneType.Tower);

        MainSceneWnd script = transform.GetComponent<MainSceneWnd>();
        if (script != null)
            script.Is2DRaycastHit = false;
    }

    /// <summary>
    /// EVENT_CLOSE_TOWER_SCENE事件回调
    /// </summary>
    void OnEventCloseTowerScene(int eventId, MixedValue para)
    {
        mTowerScene.SetActive(false);

        if (mTowerMap != null)
            mTowerMap.SetActive(true);

        SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
        if (control != null)
            control.SetSceneType(SceneCamera.SceneType.WorldMap);

        mInstanceId = string.Empty;

        Revert();

        MainSceneWnd script = transform.GetComponent<MainSceneWnd>();
        if (script != null)
            script.Is2DRaycastHit = true;

        mEasyTower.SetActive(false);

        mHardTower.SetActive(false);

        mEasyTowerSceneItems.SetActive(false);

        mHardTowerSceneItems.SetActive(false);
    }

    /// <summary>
    /// 移动相机到指定位置
    /// </summary>
    void MoveCamera()
    {
        SceneCamera sceneCamera = SceneMgr.SceneCamera.transform.GetComponent<SceneCamera>();
        if (sceneCamera == null)
            return;

        // 重置相机位置为初始位置
        sceneCamera.transform.position = mCameraToPos;

        // 场景相机当前的位置
        Vector3 curPos = sceneCamera.transform.position;

        int index = 0;

        if (mMaxLayer < 99)
            index = (mMaxLayer + 10 + 1) / 10;
        else
            index = (mMaxLayer + 10) / 10;

        // 需要移动的距离
        float distance = sceneCamera.mCurCameraInfo.mDistance * (index - 1);

        // 相机需要移动的目标位置
        Vector3 targetPos = new Vector3(curPos.x, curPos.y + distance, curPos.z);

        // 当前位置和目标位置相同
        if (curPos.Equals(targetPos))
            return;

        // 移动相机
        sceneCamera.MoveCamera(curPos, targetPos);
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    void PlayForward()
    {
        SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();

        if (control != null)
            control.MoveCamera(mCameraFromPos, mCameraToPos, 0.5f, 0f, mAnimationCurve);

        mMapTweenScale.animationCurve = mAnimationCurve;

        mMapTweenScale.PlayForward();

        mMapTweenRotation.animationCurve = mAnimationCurve;

        // 添加动画执行完的回调
        mMapTweenRotation.AddOnFinished(new EventDelegate.Callback(OnRoationFinish));
        mMapTweenRotation.PlayForward();

        // 重置动画控件
        mMapTweenScale.ResetToBeginning();

        mMapTweenRotation.ResetToBeginning();
    }

    /// <summary>
    /// 旋转动画执行完回调
    /// </summary>
    void OnRoationFinish()
    {
        // 移动相机
        MoveCamera();

        mMapTweenRotation.RemoveOnFinished(new EventDelegate(OnRoationFinish));
    }

    /// <summary>
    /// 还原
    /// </summary>
    void Revert()
    {
        // 中止补间动画
        mCameraTweenPosition.enabled = false;

        mMapTweenRotation.enabled = false;

        mMapTweenScale.enabled = false;

        SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
        if (control == null)
            return;

        control.transform.localPosition = mCameraFromPos;

        mMapParent.transform.localScale = Vector3.one;

        mMapParent.transform.localRotation = Quaternion.Euler(Vector3.zero);
    }
}
