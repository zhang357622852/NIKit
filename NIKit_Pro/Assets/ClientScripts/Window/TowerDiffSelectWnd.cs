/// <summary>
/// TowerDiffSelectWnd.cs
/// 通天塔难度选择窗口
/// Created by fengsc 2017/08/21
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class TowerDiffSelectWnd : WindowBase<TowerDiffSelectWnd>
{
    public GameObject mMask;

    // 困难按钮
    public GameObject mDiffBtn;
    public UILabel mDiffBtnLb;

    // 普通按钮
    public GameObject mNormalBtn;
    public UILabel mNormalBtnLb;

    private int mDiff = 0;

    // Use this for initialization
    void Start ()
    {
        // 注册点击事件
        RegisterEvent();

        // 初始化本地化文本
        InitLabel();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mDiffBtn).onClick = OnClickDiffBtn;

        UIEventListener.Get(mNormalBtn).onClick = OnClickNormalBtn;

        UIEventListener.Get(mMask).onClick = OnClickMask;

        UIEventListener.Get(mMask).onPress = OnPressMask;
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLabel()
    {
        mDiffBtnLb.text = LocalizationMgr.Get("TowerWnd_5");

        mNormalBtnLb.text = LocalizationMgr.Get("TowerWnd_6");
    }

    void MoveCamera(Vector3 position)
    {
        SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
        if (control != null)
            control.MoveCamera(SceneMgr.SceneCamera.transform.position, position);

        // 缓存场景相机的位置
        SceneMgr.SceneCameraToPos = position;
        SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
    }

    /// <summary>
    /// 打开通天之塔界面
    /// </summary>
    void OpenTower()
    {
        GameObject mainWnd = WindowMgr.GetWindow(MainWnd.WndType);
        if (mainWnd == null)
            return;

        WindowMgr.HideWindow(mainWnd);

        // 打开通天之塔主界面
        GameObject wnd = WindowMgr.OpenWnd(TowerWnd.WndType);
        if (wnd == null)
            return;

        TowerWnd script = wnd.GetComponent<TowerWnd>();
        if (script == null)
            return;

        script.Bind(mDiff);
    }

    /// <summary>
    /// 困难按钮点击回调
    /// </summary>
    void OnClickDiffBtn(GameObject go)
    {
        mDiff = TowerConst.HARD_TOWER;

        // 打开通天之塔界面
        OpenTower();
    }

    /// <summary>
    /// 普通按钮点击回调
    /// </summary>
    void OnClickNormalBtn(GameObject go)
    {
        mDiff = TowerConst.EASY_TOWER;

        // 打开通天之塔界面
        OpenTower();
    }

    void OnClickMask(GameObject go)
    {
        CloseWnd();
    }

    void OnPressMask(GameObject go, bool isPress)
    {
        // 手指抬起时执行
        if (! isPress)
            CloseWnd();
    }

    void CloseWnd()
    {
        gameObject.SetActive(false);

        // 移动相机位置
        MoveCamera(SceneMgr.SceneCameraFromPos);
    }
}
