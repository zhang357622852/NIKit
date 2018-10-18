/// <summary>
/// MonthCardIncreaseWnd.cs
/// Created by fengsc 2018/09/08
/// 月卡增幅活动窗口
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class MonthCardIncreaseWnd : WindowBase<MonthCardIncreaseWnd>
{
    #region 成员变量

    public UILabel mTime;
    public UILabel mTitle;
    public UILabel mShadowTitle;
    public UILabel mDesc;
    public UILabel mTips1;

    public GameObject mItem;
    public GameObject mCloseBtn;
    public Transform mBonusPanel;

    // item间距
    public float mItemSpace = 300f;

    #endregion

    #region 私有变量

    public LPCMapping ActivityInfo { get; private set; }

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();

        TweenScale mTweenScale = GetComponent<TweenScale>();

        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        mItem.SetActive(false);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;
        UIEventListener.Get(mTips1.gameObject).onClick = OnClickTips;
    }

    void OnClickTips(GameObject go)
    {
        LPCMapping gift_bag_data = LPCMapping.Empty;

        LPCValue v = ME.user.Query<LPCValue>("gift_bag_data");
        if (v != null && v.IsMapping)
            gift_bag_data = v.AsMapping;

        // 已经购买月卡了
        if (gift_bag_data.ContainsKey(ShopConfig.MONTH_CARD_ID))
        {
            // 您已购买了月卡，无需再次购买。
            DialogMgr.Notify(LocalizationMgr.Get("ActivityWnd_11"));

            return;
        }

        // 打开月卡窗口
        GameObject wnd = WindowMgr.OpenWnd(MonthCardWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return;

        MonthCardWnd script = wnd.GetComponent<MonthCardWnd>();

        // 绑定数据
        script.Bind(ShopConfig.MONTH_CARD_ID);
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    void Redraw()
    {
        // 活动配置数据
        LPCMapping activityConfig = ActivityMgr.GetActivityInfo(ActivityInfo.GetValue<string>("activity_id"));

        // 没有该活动的配置数据
        if (activityConfig == null || activityConfig.Count == 0)
            return;

        if (ME.user == null)
            return;

        // 有效时间段
        LPCArray validPeriod = ActivityInfo.GetValue<LPCArray>("valid_period");
        if (validPeriod == null)
            validPeriod = LPCArray.Empty;

        // 不包含提示列表
        if (! activityConfig.ContainsKey("tips_list"))
            return;

        // 提示列表
        LPCArray tipsList = activityConfig.GetValue<LPCArray>("tips_list");

        LPCArray showList = LPCArray.Empty;

        for (int i = 0; i < tipsList.Count; i++)
        {
            LPCMapping item = tipsList[i].AsMapping;

            LPCMapping arg = LPCMapping.Empty;

            if (item.ContainsKey("show_time") && item.GetValue<int>("show_time") == 1)
            {
                // 有效时间段的开始、结束时间
                arg.Append(validPeriod[i].AsMapping);
                arg.Append(item);

                showList.Add(arg);
            }
            else
            {
                showList.Add(item);
            }
        }

        // 克隆提示图标
        for (int i = 0; i < showList.Count; i++)
            CreateBonusIcon(i, showList.Count, showList[i].AsMapping);

        string activityId = ActivityInfo.GetValue<string>("activity_id");

        LPCMapping config = ActivityMgr.GetActivityInfo(activityId);

        string title = ActivityMgr.GetActivityTitle(activityId);

        // 活动标题
        mTitle.text = title;

        if (mShadowTitle != null)
            mShadowTitle.text = title;

        LPCArray tips = config.GetValue<LPCArray>("tips");

        mTips1.text = LocalizationMgr.Get(tips[0].AsString);

        // 活动描述
        mDesc.text = ActivityMgr.GetActivitySubTitle(activityId);

        // 活动时间描述
        mTime.text = ActivityMgr.GetActivityTimeDesc(activityId, validPeriod);
    }

    /// <summary>
    /// 创建奖励图标
    /// </summary>
    void CreateBonusIcon(int index, int totalNum, LPCMapping item)
    {
        float x;
        if (totalNum % 2 == 1)
            x = (index - (totalNum / 2)) * mItemSpace;
        else
            x = (index - ((totalNum - 1) / 2f)) * mItemSpace;

        GameObject clone = Instantiate(mItem) as GameObject;
        clone.transform.parent = mBonusPanel;
        clone.name = string.Format("icon_{0}", index);
        clone.transform.localScale = Vector3.one;
        clone.transform.localPosition = new Vector3 (x, mItem.transform.localPosition.y, 0);
        clone.SetActive(true);

        // 如果是texture模式
        if (item.ContainsKey("texture"))
        {
            // 加载提示图标
            Transform textureTrans = clone.transform.Find("texture_icon");
            UITexture icon = textureTrans.GetComponent<UITexture>();
            icon.mainTexture = ResourceMgr.LoadTexture(item.GetValue<string>("texture"));

            // 显示控件
            textureTrans.gameObject.SetActive(true);
        }
        else if (item.ContainsKey("sprite"))
        {
            Transform spriteTrans = clone.transform.Find("sprite_icon");
            UISprite spriteIcon = spriteTrans.GetComponent<UISprite>();
            spriteIcon.atlas = ResourceMgr.LoadAtlas(item.GetValue<string>("atlas"));
            spriteIcon.spriteName = item.GetValue<string>("sprite");

            // 显示控件
            spriteTrans.gameObject.SetActive(true);
        }
        else
        {
            // 其他情况暂不支持
            LogMgr.Error("其他情况暂不支持");
        }

        // 获取描述文本
        LPCValue desc = item.GetValue<LPCValue>("desc");
        UILabel numberTemp = clone.transform.Find("desc_label").GetComponent<UILabel>();

        if (desc.IsInt)
            numberTemp.text = (string) ScriptMgr.Call(desc.AsInt, ActivityInfo, item);
        else
            numberTemp.text = LocalizationMgr.Get(desc.AsString);
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="itemId">Item identifier.</param>
    public void Bind(LPCMapping activityInfo)
    {
        if (activityInfo == null)
            return;

        ActivityInfo = activityInfo;

        Redraw();
    }

    #endregion
}
