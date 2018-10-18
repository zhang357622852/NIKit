/// <summary>
/// GameCourseWnd.cs
/// Created by fengsc 2018/08/18
/// 游戏历程窗口
/// </summary>
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QCCommonSDK.Addition;
using QCCommonSDK;

public class GameCourseWnd : WindowBase<GameCourseWnd>
{
    #region 成员变量

    // 我的计划按钮
    public GameObject mPlanBtn;

    // 圣域之一按钮
    public GameObject mDungeons1Btn;

    // 圣域之二按钮
    public GameObject mDungeons2Btn;

    // 强者之路按钮
    public GameObject mEquipBtn;

    // 百塔挑战按钮
    public GameObject mTowerBtn;

    // 竞技荣耀按钮
    public GameObject mArenaBtn;

    public GameObject mToogle;

    // 分页选项名称
    public UILabel[] mPageNames;

    // 分页窗口
    public GameObject[] mWnds;

    // 红点提示
    public GameObject[] mRedPoints;

    // 关闭窗口
    public GameObject mCloseBtn;

    // 分享按钮
    public GameObject mShareBtn;
    public UILabel mShareBtnLb;

    public GameObject mShareTag;

    public UILabel mUserRid;

    public UILabel mUserName;

    public UILabel mShareTips;

    public TweenScale mTweenScale;

    public UITexture mBg;

    // 当前选中的分页
    private int mCurPage = GameCourseConst.PAGE_5;

    private Property mWho;

    private string mShareTitle;

    private string mShareDesc;

    private Dictionary<int, Texture2D> mBgs = new Dictionary<int, Texture2D>();

    private List<string> mPagePath = new List<string>()
        {
            "Assets/Art/UI/Window/Background/plan_course_bg.png",
            "Assets/Art/UI/Window/Background/dungeons_course_bg1.png",
            "Assets/Art/UI/Window/Background/dungeons_course_bg2.png",
            "Assets/Art/UI/Window/Background/equip_course_bg.png",
            "Assets/Art/UI/Window/Background/tower_course_bg.png",
            "Assets/Art/UI/Window/Background/arena_course_bg.png",
        };

    #endregion

    void Awake()
    {
        // 加载背景资源
        mBgs.Add(GameCourseConst.PAGE_0, ResourceMgr.LoadTexture(mPagePath[0]));
        mBgs.Add(GameCourseConst.PAGE_1, ResourceMgr.LoadTexture(mPagePath[1]));
        mBgs.Add(GameCourseConst.PAGE_2, ResourceMgr.LoadTexture(mPagePath[2]));
        mBgs.Add(GameCourseConst.PAGE_3, ResourceMgr.LoadTexture(mPagePath[3]));
        mBgs.Add(GameCourseConst.PAGE_4, ResourceMgr.LoadTexture(mPagePath[4]));
        mBgs.Add(GameCourseConst.PAGE_5, ResourceMgr.LoadTexture(mPagePath[5]));
    }

    // Use this for initialization
    void Start ()
    {
        float scale = Game.CalcWndScale();

        mTweenScale.to = new Vector3(scale, scale, scale);

        // 注册事件
        RegisterEvent();

        // 初始化本地化文本
        InitLabel();
    }

    void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        EventMgr.UnregisterEvent("GameCourseWnd");

        if (ME.user == null)
            return;

        if (mWho.GetRid() != ME.user.GetRid())
            return;

        ME.user.dbase.RemoveTriggerField("GameCourseWnd");

        // 移除字段关注
        ME.user.tempdbase.RemoveTriggerField("GameCourseWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mPlanBtn).onClick = OnClickPlanBtn;
        UIEventListener.Get(mDungeons1Btn).onClick = OnClickDungeons1Btn;
        UIEventListener.Get(mDungeons2Btn).onClick = OnClickDungeons2Btn;
        UIEventListener.Get(mEquipBtn).onClick = OnClickEquipBtn;
        UIEventListener.Get(mTowerBtn).onClick = OnClickTowerBtn;
        UIEventListener.Get(mArenaBtn).onClick = OnClickArenaBtn;
        UIEventListener.Get(mShareBtn).onClick = OnClickShareBtn;
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        EventMgr.RegisterEvent("GameCourseWnd", EventMgrEventType.EVENT_SHARE_SUCCESS, OnShareSuccess);

        // 注册mTweenScale动画播放完成回调
        EventDelegate.Add(mTweenScale.onFinished, OnTweenScaleFinish);

        if (ME.user == null)
            return;

        // 不是查看自己的游戏历程
        if (mWho.GetRid() != ME.user.GetRid())
            return;

        // 关注 unlock_new_course 字段变化
        ME.user.tempdbase.RemoveTriggerField("GameCourseWnd");
        ME.user.tempdbase.RegisterTriggerField("GameCourseWnd", new string[]{ "unlock_new_course" }, new CallBack(OnUnlockGameCourseChange));

        // 关注 game_course 字段变化
        ME.user.dbase.RemoveTriggerField("GameCourseWnd");
        ME.user.dbase.RegisterTriggerField("GameCourseWnd", new string[]{ "game_course" }, new CallBack(OnGameCourseChange));
    }

    /// <summary>
    /// game_course字段变化回调
    /// </summary>
    void OnGameCourseChange(object para, params object[] param)
    {
        // 重绘窗口
        Redraw();
    }

    void OnUnlockGameCourseChange(object para, params object[] param)
    {
        // 刷新红点提示
        RefreshRedPoint();
    }

    void OnTweenScaleFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 分享成功回调
    /// </summary>
    void OnShareSuccess(int eventId, MixedValue para)
    {
        mShareTag.SetActive(false);

        mToogle.SetActive(true);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLabel()
    {
        mShareBtnLb.text = LocalizationMgr.Get("GameCourseWnd_1");

        mShareTips.text = LocalizationMgr.Get("GameCourseWnd_2");

        mShareTitle = LocalizationMgr.Get("GameCourseWnd_3");

        mShareDesc = LocalizationMgr.Get("GameCourseWnd_4");
    }

    /// <summary>
    /// 刷新红点提示
    /// </summary>
    void RefreshRedPoint()
    {
        for (int i = 0; i < mRedPoints.Length; i++)
        {
            mRedPoints[i].SetActive(GameCourseMgr.IsNewUnLockCourse(mWho, i));
            if (i == mCurPage)
            {
                mRedPoints[i].transform.localPosition = new Vector3(-83, 28, 0);
            }
            else
            {
                mRedPoints[i].transform.localPosition = new Vector3(-46, 28, 0);
            }
        }
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mShareBtn.SetActive(ShareMgr.IsOpenShare() && GuideMgr.IsGuided(GuideMgr.SHARE_SHOW_GUIDE_GROUP));

        // 显示我的计划分页
        OnClickPlanBtn(null);

        // 刷新红点提示
        RefreshRedPoint();

        // 玩家对象不存在
        if (mWho == null)
            return;

        mUserRid.text = mWho.GetRid();

        mUserName.text = mWho.GetName();

        for (int i = 0; i < mPageNames.Length; i++)
            mPageNames[i].text = GameCourseMgr.GetPageName(mWho, i);
    }

    void DisablePage(int page)
    {
        for (int i = 0; i < mWnds.Length; i++)
        {
            if (i != page && mWnds[i].activeSelf)
                mWnds[i].SetActive(false);
        }
    }

    void RedrawCoursePlanWnd()
    {
        // 移除解锁标识
        GameCourseMgr.RemoveNewUnlockCourse(ME.user, mCurPage);

        // 显示选项按钮
        ShowToggle(mCurPage);

        // 显示背景图片
        ShowPageBg(mCurPage);

        // 索引越界
        if (mCurPage + 1 > mWnds.Length)
            return;

        GameObject wnd = mWnds[mCurPage];
        if (wnd == null)
            return;

        if (!wnd.activeSelf)
            wnd.SetActive(true);

        // 绑定数据
        wnd.GetComponent<CoursePlanWnd>().Bind(mCurPage, mWho);

        // 隐藏其他页面
        DisablePage(mCurPage);
    }

    void RedrawCourseDungeonsWnd()
    {
        // 移除解锁标识
        GameCourseMgr.RemoveNewUnlockCourse(ME.user, mCurPage);

        // 显示选项按钮
        ShowToggle(mCurPage);

        // 显示背景图片
        ShowPageBg(mCurPage);

        // 索引越界
        if (mCurPage + 1 > mWnds.Length)
            return;

        GameObject wnd = mWnds[mCurPage];
        if (wnd == null)
            return;

        if (!wnd.activeSelf)
            wnd.SetActive(true);

        // 绑定数据
        wnd.GetComponent<CourseDungeonsWnd>().Bind(mCurPage, mWho);

        // 隐藏其他页面
        DisablePage(mCurPage);
    }

    /// <summary>
    /// 我的计划按钮点击回调
    /// </summary>
    void OnClickPlanBtn(GameObject go)
    {
        if (mCurPage == GameCourseConst.PAGE_0)
            return;

        mCurPage = GameCourseConst.PAGE_0;

        // 绘制CoursePlanWnd窗口
        RedrawCoursePlanWnd();
    }

    /// <summary>
    /// 圣域之一按钮点击回调
    /// </summary>
    void OnClickDungeons1Btn(GameObject go)
    {
        if (mCurPage == GameCourseConst.PAGE_1)
            return;

        mCurPage = GameCourseConst.PAGE_1;

        // 绘制CourseDungeonsWnd
        RedrawCourseDungeonsWnd();
    }

    /// <summary>
    /// 圣域之二按钮点击回调
    /// </summary>
    void OnClickDungeons2Btn(GameObject go)
    {
        if (mCurPage == GameCourseConst.PAGE_2)
            return;

        mCurPage = GameCourseConst.PAGE_2;

        // 绘制CourseDungeonsWnd
        RedrawCourseDungeonsWnd();
    }

    /// <summary>
    /// 强者之路按钮点击回调
    /// </summary>
    void OnClickEquipBtn(GameObject go)
    {
        if (mCurPage == GameCourseConst.PAGE_3)
            return;

        mCurPage = GameCourseConst.PAGE_3;

        // 绘制CoursePlanWnd窗口
        RedrawCoursePlanWnd();
    }

    /// <summary>
    /// 百塔挑战按钮点击回调
    /// </summary>
    void OnClickTowerBtn(GameObject go)
    {
        if (mCurPage == GameCourseConst.PAGE_4)
            return;

        mCurPage = GameCourseConst.PAGE_4;

        // 绘制CoursePlanWnd窗口
        RedrawCoursePlanWnd();
    }

    /// <summary>
    /// 竞技荣耀按钮点击回调
    /// </summary>
    void OnClickArenaBtn(GameObject go)
    {
        if (mCurPage == GameCourseConst.PAGE_5)
            return;

        mCurPage = GameCourseConst.PAGE_5;

        // 绘制CoursePlanWnd窗口
        RedrawCoursePlanWnd();
    }

    /// <summary>
    /// 分享按钮点击回调
    /// </summary>
    void OnClickShareBtn(GameObject go)
    {
        // 显示分享标签
        mShareTag.SetActive(true);

        mToogle.SetActive(false);

        GameObject wnd = WindowMgr.OpenWnd(ShareSelectWnd.WndType, this.transform);
        if (wnd == null)
            return;

        wnd.transform.localPosition = new Vector3(470, -27, 0);

        ShareSelectWnd script = wnd.GetComponent<ShareSelectWnd>();
        if (script == null)
            return;

        script.SetBgAlpha(0.745f);

        // 计算实际图片大小，但改变窗口屏幕大小时，图片的实际像素是不一样的。
        float captureWeight = 1f * Screen.width * mBg.width / 1280;
        float captureHeight = 1f * Screen.height * mBg.height / 720;

        // 绑定数据
        script.Bind(new Vector2(captureWeight, captureHeight), mBg.transform.position, mShareTitle, mShareDesc);
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 显示选项按钮
    /// </summary>
    void ShowToggle(int page)
    {
        Color selectColor = new Color(246.0f / 255, 217.0f / 255, 138.0f / 255, 1.0f);

        Color NoSelectColor = new Color(142.0f / 255, 129.0f / 255, 95.0f / 255, 0.5f);

        for (int i = 0; i < mPageNames.Length; i++)
        {
            if (page != i)
            {
                // 没选中的按钮
                mPageNames[i].fontSize = 26;

                mPageNames[i].color = NoSelectColor;
            }
            else
            {
                // 选中的文字按钮状态
                mPageNames[i].fontSize = 30;

                mPageNames[i].color = selectColor;
            }
        }
    }

    /// <summary>
    /// 显示分页背景
    /// </summary>
    /// <param name="page">Page.</param>
    void ShowPageBg(int page)
    {
        Texture2D tex = null;

        // 该分页的背景资源不存在
        if (!mBgs.TryGetValue(page, out tex))
        {
            // 重新加载资源
            mBgs.Add(page, ResourceMgr.LoadTexture(mPagePath[page]));
        }

        // 资源载入失败
        if (! mBgs.ContainsKey(page))
        {
            LogMgr.Error("分页{0}的背景资源载入失败", page);

            return;
        }

        mBg.mainTexture = mBgs[page];
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(Property who)
    {
        // 玩家对象
        mWho = who;

        if (mWho.GetRid() == ME.user.GetRid())
        {
            if (GameCourseMgr.IsNewGameCourse(mWho))
            {
                // 请求更新游戏历程数据
                GameCourseMgr.RequestGameCourse(mWho);
            }
            else
            {
                // 没有新数据，直接绘制窗口
                Redraw();
            }
        }
        else
        {
            // 绘制窗口
            Redraw();
        }
    }
}
