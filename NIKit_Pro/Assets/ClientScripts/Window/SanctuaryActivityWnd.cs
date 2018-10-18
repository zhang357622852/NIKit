/// <summary>
/// SanctuaryActivityWnd.cs
/// Created by fengsc 2018/03/28
/// 隐藏圣域活动界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class SanctuaryActivityWnd : WindowBase<SanctuaryActivityWnd>
{
    // 背景
    public UITexture mBg;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 副标题
    public UILabel mSubTitle;

    // 标题
    public UILabel mTitle;

    // 活动时间
    public UILabel mActivtyTime;

    // 宠物模型
    public GameObject mPetModel;

    // 打开圣域窗口按钮
    public UILabel mOpenDungeonsBtn;

    // 使魔元素
    public UISprite mElement;

    public UILabel mViewItemTips;

    public UILabel mTips;

    // 使魔item
    public SignItemWnd mItem;

    int mMapId = 19;

    Property mOb;

    // 活动数据
    LPCMapping mActivityInfo = LPCMapping.Empty;

    // 开启超链接按钮的文本颜色
    Color openBtnCol = new Color(0f, 0.85f, 1.0f, 1.0f);

    // 关闭超链接按钮的文本颜色
    Color closeBtnCol = new Color(0.83f, 0.3f, 0.38f, 1.0f);

    void Start()
    {
        // 注册事件
        RegisterEvent();

        mViewItemTips.text = LocalizationMgr.Get("SanctuaryActivityWnd_4");

        BoxCollider bc = mOpenDungeonsBtn.GetComponent<BoxCollider>();
        if (bc != null)
        {
            bc.size = new Vector3(mOpenDungeonsBtn.width, bc.size.y, bc.size.z);
            bc.center = new Vector3(-bc.size.x / 2f, bc.center.y, bc.center.z);
        }

        TweenScale mTweenScale = GetComponent<TweenScale>();

        if (mTweenScale == null)
            return;

        // 注册回调
        EventDelegate.Add(mTweenScale.onFinished, OnScaleFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// tween动画完成回调
    /// </summary>
    void OnScaleFinish()
    {
        if (mOb == null)
            return;

        // 获取窗口绑定的模型窗口组件
        ModelWnd pmc = mPetModel.GetComponent<ModelWnd>();

        // 载入模型
        pmc.LoadModel(mOb, LayerMask.NameToLayer("UI"));
    }

    void OnDestroy()
    {
        // 销毁宠物对象
        if (mOb != null)
            mOb.Destroy();
        EventMgr.UnregisterEvent("SanctuaryActivityWnd");
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 打开圣域窗口按钮点击事件
    /// </summary>
    void OnClickOpenDungeonsBtn(GameObject go)
    {
        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OnEnterMainCityScene));

        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);

        // 关闭活动窗口
        WindowMgr.DestroyWindow("ActivityWnd");

        // 隐藏主界面
        WindowMgr.HideMainWnd();
    }

    /// <summary>
    /// 打开主城回调
    /// </summary>
    private void OnEnterMainCityScene(object para, object[] param)
    {
        // 没有活动数据
        if (mActivityInfo == null)
            return;

        // 如果当前活动无效
        string cookie = mActivityInfo.GetValue<string>("cookie");
        if (! ActivityMgr.IsAcitvityValidByCookie(cookie))
            return;

        // 相机移动的目标位置
        Vector3 targetPos = new Vector3(-4.25f, 10.86f, -15f);

        // 创建地下城窗口
        GameObject wnd = WindowMgr.OpenWnd(DungeonsWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<DungeonsWnd>().Bind(string.Empty, mMapId, targetPos);
    }

    /// <summary>
    /// 使魔信息查看按钮点击事件
    /// </summary>
    void OnClickItemBtn(GameObject go)
    {
        GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();

        script.Bind(mOb);
        script.ShowBtn(true, false, false);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mOpenDungeonsBtn.gameObject).onClick = OnClickOpenDungeonsBtn;
        UIEventListener.Get(mItem.gameObject).onClick = OnClickItemBtn;
        EventMgr.RegisterEvent("SanctuaryActivityWnd", EventMgrEventType.EVENT_NOTIFY_ACTIVITY_LIST, OnNotifyActivityList);
    }

    /// <summary>
    /// 活动列表通知事件回调
    /// </summary>
    /// <param name="eventID">Event I.</param>
    /// <param name="value">Value.</param>
    void OnNotifyActivityList(int eventID, MixedValue value)
    {
        //如果活动不存在，销毁窗口
        if (!ActivityMgr.IsOpenAcitvity(mActivityInfo.GetValue<string>("activity_id")))
            WindowMgr.DestroyWindow(gameObject.name);
        else
            Redraw();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 附加参数
        LPCMapping extraPara = mActivityInfo.GetValue<LPCMapping>("extra_para");
        if (extraPara == null || extraPara.Count == 0)
            return;

        // 活动id
        string activityId = mActivityInfo.GetValue<string>("activity_id");

        // 根据活动是否有效，选择是否显示需要超链接功能的文本
        if (ActivityMgr.IsAcitvityValidByCookie(mActivityInfo.GetValue<string>("cookie")))
        {
            mOpenDungeonsBtn.text = LocalizationMgr.Get("SanctuaryActivityWnd_1");
            mOpenDungeonsBtn.GetComponent<BoxCollider>().enabled = true;
            mOpenDungeonsBtn.color = openBtnCol;
        }
        else
        {
            mOpenDungeonsBtn.text = LocalizationMgr.Get("SanctuaryActivityWnd_5");
            mOpenDungeonsBtn.GetComponent<BoxCollider>().enabled = false;
            mOpenDungeonsBtn.color = closeBtnCol;
        }

        // 活动配置数据
        LPCMapping config = ActivityMgr.GetActivityInfo(activityId);
        if (config == null || config.Count == 0)
            return;

        // 活动副标题
        mSubTitle.text = ActivityMgr.GetActivitySubTitle(activityId, extraPara);

        // 活动标题
        mTitle.text = ActivityMgr.GetActivityTitle(activityId, extraPara);

        // 活动时间
        mActivtyTime.text = ActivityMgr.GetActivityTimeDesc(activityId, mActivityInfo.GetValue<LPCArray>("valid_period"));

        // 使魔id
        int classId = extraPara.GetValue<int>("pet_id");

        int element = MonsterMgr.GetElement(classId);

        mBg.color = MonsterConst.MonsterElementRGBColorMap[element];

        // 使魔元素图片
        mElement.spriteName = PetMgr.GetElementIconName(element);

        // 使魔配置数据
        CsvRow row = MonsterMgr.GetRow(classId);
        if (row == null)
            return;

        mTips.text = string.Format(LocalizationMgr.Get("SanctuaryActivityWnd_3"), row.Query<int>("piece_amount"));

        // 构建参数
        LPCMapping dbase = LPCMapping.Empty;
        dbase.Add("class_id", classId);
        dbase.Add("rid", Rid.New());
        dbase.Add("rank", row.Query<int>("rank"));
        dbase.Add("star", row.Query<int>("star"));

        /// 销毁宠物对象
        if (mOb != null)
            mOb.Destroy();

        // 克隆宠物对象
        mOb = PropertyMgr.CreateProperty(dbase);

        mItem.ShowAmount(false);
        mItem.NormalItemBind(dbase, false);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping activityInfo)
    {
        if (activityInfo == null || activityInfo.Count == 0)
            return;

        mActivityInfo = activityInfo;

        // 绘制窗口
        Redraw();
    }
}
