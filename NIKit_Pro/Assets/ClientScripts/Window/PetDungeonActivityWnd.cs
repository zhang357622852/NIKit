/// <summary>
/// PetDungeonActivityWnd.cs
/// Created by lizy 2018/05/16
/// 精英圣域活动界面
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class PetDungeonActivityWnd : WindowBase<SanctuaryActivityWnd>
{
    #region 成员变量

    // 背景
    public UITexture mBg;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 标题one
    public UILabel mTitleOne;

    // 标题
    public UILabel mTitle;

    // 副标题
    public UILabel mSubTitle;

    // 活动有效时间
    public UILabel mActivityTime;

    // 模型
    public GameObject mPetModel;

    // 前往隐藏神域按钮
    public UILabel mOpenDungeonsBtn;

    // 元素图标
    public UISprite mElement;

    // 模型背景图
    public UITexture mModelBg;

    // 查看使魔信息提示
    public UILabel mViewItemTips;

    // 使魔对应碎片提示
    public UILabel mTips;

    // 使魔item
    public SignItemWnd mItem;

    // 星级
    public UILabel[] mStars;

    Property mOb;

    LPCMapping mActivityInfo;

    int mMapId = 25;

    // 开启超链接按钮的文本颜色
    Color openBtnCol = new Color(0f, 0.85f, 1.0f, 1.0f);

    // 关闭超链接按钮的文本颜色
    Color closeBtnCol = new Color(0.83f, 0.3f, 0.38f, 1.0f);

    #endregion

    #region 内部函数

    void Start()
    {
        RegisterEvent();

        mTitleOne.text = LocalizationMgr.Get("PetDungeonActivityWnd_1");
        mTitle.text = LocalizationMgr.Get("PetDungeonActivityWnd_2");   
        mViewItemTips.text = LocalizationMgr.Get("PetDungeonActivityWnd_4");

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

    void OnDestroy()
    {
        // 销毁宠物对象
        if (mOb != null)
            mOb.Destroy();
        EventMgr.UnregisterEvent("PetDungeonActivityWnd");
    }

    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mOpenDungeonsBtn.gameObject).onClick = OnClickOpenDungeonsBtn;
        UIEventListener.Get(mItem.gameObject).onClick = OnClickItemBtn;
        EventMgr.RegisterEvent("PetDungeonActivityWnd", EventMgrEventType.EVENT_NOTIFY_ACTIVITY_LIST, OnNotifyActivityList);
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

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
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

        LPCMapping extrapPara = LPCMapping.Empty;
        extrapPara.Add("dynamic_id", GetValidPeriodId());
        extrapPara.Append(mActivityInfo.GetValue<LPCMapping>("extra_para"));

        // 绑定数据
        wnd.GetComponent<DungeonsWnd>().Bind(
            string.Empty,
            mMapId,
            targetPos,
            extrapPara);
    }

    /// <summary>
    /// Gets the valid period identifier.
    /// </summary>
    /// <returns>The valid period identifier.</returns>
    private string GetValidPeriodId()
    {
        // 没有mActivityInfo
        if (mActivityInfo == null)
            return string.Empty;

        // 获取
        LPCArray validPeriod = mActivityInfo.GetValue<LPCArray>("valid_period");
        if (validPeriod == null || validPeriod.Count == 0)
            return string.Empty;

        string validId = string.Empty;

        // 获取当前正在生效的valid_id
        foreach (LPCValue period in validPeriod.Values)
        {
            if (period == null)
                continue;

            // 获取该时段有效valid_id，该活动不应该出现多个有效时段的情况
            validId = period.AsMapping.GetValue<string>("valid_id");
        }

        // 返回valid_id
        return validId;
    }

    /// <summary>
    /// 活动列表通知事件回调
    /// </summary>
    /// <param name="eventID">Event I.</param>
    /// <param name="value">Value.</param>
    void OnNotifyActivityList(int eventID, MixedValue value)
    {
        if (this == null)
            return;

        //如果活动不存在，销毁窗口
        if (!ActivityMgr.IsOpenAcitvity(mActivityInfo.GetValue<string>("activity_id")))
            WindowMgr.DestroyWindow(gameObject.name);
        else
            Redraw();
    }

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
            mOpenDungeonsBtn.text = LocalizationMgr.Get("PetDungeonActivityWnd_5");
            mOpenDungeonsBtn.GetComponent<BoxCollider>().enabled = true;
            mOpenDungeonsBtn.color = openBtnCol;
        }
        else
        {
            mOpenDungeonsBtn.text = LocalizationMgr.Get("PetDungeonActivityWnd_7");
            mOpenDungeonsBtn.GetComponent<BoxCollider>().enabled = false;
            mOpenDungeonsBtn.color = closeBtnCol;
        }

        // 活动副标题
        mSubTitle.text = ActivityMgr.GetActivitySubTitle(activityId, extraPara);

        // 活动时间
        mActivityTime.text = ActivityMgr.GetActivityTimeDesc(activityId, mActivityInfo.GetValue<LPCArray>("valid_period"));

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

        int star = row.Query<int>("star");

        mTips.text = string.Format(LocalizationMgr.Get("PetDungeonActivityWnd_3"), row.Query<int>("piece_amount"));

        // 构建参数
        LPCMapping dbase = LPCMapping.Empty;
        dbase.Add("class_id", classId);
        dbase.Add("rid", Rid.New());
        dbase.Add("rank", row.Query<int>("rank"));
        dbase.Add("star", star);

        /// 销毁宠物对象
        if (mOb != null)
            mOb.Destroy();

        // 克隆宠物对象
        mOb = PropertyMgr.CreateProperty(dbase);

        mItem.ShowAmount(false);
        mItem.NormalItemBind(dbase, false);

        for (int i = 0; i < star; i++)
        {
            mStars[i].gameObject.SetActive(true);
            mStars[i].transform.localPosition = new Vector3(mStars[i].width * (i - (star - 1) * 0.5f), 0, 0);
        }

        string iconName = MonsterMgr.GetIcon(classId, MonsterMgr.GetDefaultRank(classId));

        if (string.IsNullOrEmpty(iconName))
            return;

        string path = string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconName);

        if (string.IsNullOrEmpty(path))
            return;

        Texture2D res = ResourceMgr.LoadTexture(path);

        if (res != null)
            mModelBg.mainTexture = res;
    }

    #endregion

    #region外部函数

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="activityInfo">Activity info.</param>
    public void Bind(LPCMapping activityInfo)
    {
        mActivityInfo = activityInfo;

        Redraw();
    }

    #endregion
}
