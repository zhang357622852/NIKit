/// <summary>
/// SummonWnd.cs
/// Created by lic 2016-7-14
/// 召唤界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AnimationOrTween;
using LPC;

public class SummonWnd : WindowBase<SummonWnd>
{
    #region 成员变量
    public float delaySmallTime = 0.5f;

    public GameObject mcloseBtn;
    public GameObject mShowSummonWnd;
    public GameObject mSummonBtn;
    //一次召唤
    public GameObject mSummonOneBtn;
    //十次召唤
    public GameObject mSummonSeveralBtn;

    //跳过
    public GameObject mSkipBtn;

    public GameObject mSkipCover;

    public GameObject mPrefectEfect;
    public GameObject mPrefectEffectLb;
    public GameObject[] mStarEffects;
    public GameObject mWhiteFlag;
    //物品itemIcon
    public UITexture mItemIcon;

    public UITexture mPetIcon;
    public GameObject[] mStars;
    public GameObject mPiece;

    public UILabel mCostNumber;
    public UISprite mMoneyIcon;
    public UILabel mCostOneNumber;
    public UISprite mMoneyOneIcon;
    public UILabel mCostSeveralNumber;
    public UISprite mMoneySeveralIcon;

    public GameObject Container;
    public UIScrollView ScrollView;

    public float itemHeight = 0f;

    public float itemSpace = 0f;

    public Vector3[] petPos;

    // 本地化文字
    public UILabel mSummonlb;
    public UILabel mSummonOnelb;
    public UILabel mSummonSeverallb;
    public UILabel mSummonTitle;

    public GameObject mSummonItemModel;
    public GameObject mSummonPieceItemModel;

    // 概率查看按钮
    public UILabel mSummonRateBtn;

    // 完美召唤
    public UILabel mPrefectSummon;
    public UILabel mShadow;

    // 限制召唤
    public GameObject mLimitOb;
    public UILabel mLimitDesc;

    #endregion

    #region 私有变量

    // 技能光效预制名称
    private string[] mEffectNames = new string[]{ "E0002_star", "E0002_s", "E0002_n", "E0002_h", "E0002_end" };

    // 黑边动画对象
    private GameObject blackTweenCover;

    // 是否正在召唤
    private bool isSummoning = false;

    // 是否是完美召唤
    private bool isCriti = false;

    // 是否已显示过结果
    private bool hasShowResult = false;

    // 相机动画是否已经播放完毕
    private bool isCameraTweenOver = false;

    private List<Property> mSummonItems = new List<Property>();

    private SummonItemWnd mSelectItem = null;

    private string mSelectId = string.Empty;

    private string mWndName = string.Empty;

    // 当前播放的音乐名称
    private string mSoundName = string.Empty;

    private string mPrefectSoundName = string.Empty;

    // 召唤光效
    private List<ParticleSystem> mEffects = new List<ParticleSystem>();

    bool needResetSelect = true;

    // 指引回调
    CallBack mGuideClickSummonCb;
    CallBack mGuideSummonFinishCb;
    CallBack mGuideCloseCb;

    // itemOb的缓存池
    List<GameObject> mItemObList  = new List<GameObject>();

    // piceceItemOb的缓存池
    List<GameObject> mPieceItemObList  = new List<GameObject>();

    /// <summary>
    /// 当前屏幕分辨率
    /// </summary>
    private float aspect = 0f;
    private Vector3 mItemIconPos = new Vector3(-0.3f, 0.6f, 0.23f);

    TweenPosition[] tw;

    // 是否有新的元素强化限时礼包产生
    private bool mIsNewIntensifyGift = false;
    private ShareOperateWnd.ShareOperateType mCurShareType = ShareOperateWnd.ShareOperateType.None;
    #endregion


    #region 内部函数

    // Use this for initialization
    void Start()
    {
        tw = SceneMgr.SceneCamera.GetComponents<TweenPosition>();

        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();

        // 刷新召唤页面
        Redraw();

        // 刷新按钮消耗
        RedrawSummonBtn();

        // 刷新限制显示
        RefreshLimitSummon();

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        float scale = Game.CalcWndScale();
        transform.localScale = new Vector3(scale, scale, scale);
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void Update()
    {
        // 获取rate
        float newAspect = SceneMgr.SceneCamera.aspect;

        // 如果aspect没有变化
        if (Game.FloatEqual(aspect, newAspect))
            return;

        // 重置aspect
        aspect = newAspect;

        // 重新刷新窗口位置
        mItemIcon.transform.position = Game.WorldToUI(mItemIconPos);
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 销毁光效
        foreach (ParticleSystem effect_ob in mEffects)
        {
            if (effect_ob == null)
                continue;

            EffectMgr.DestroyEffect(effect_ob.gameObject);
        }

        mGuideSummonFinishCb = null;
        mGuideClickSummonCb = null;

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 移除属性字段关注回调
        ME.user.dbase.RemoveTriggerField("SummonWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mcloseBtn).onClick = OnCloseBtn;
        UIEventListener.Get(mSummonBtn).onClick = OnSummonBtn;
        UIEventListener.Get(mSummonOneBtn).onClick = OnSummonBtn;
        UIEventListener.Get(mSummonSeveralBtn).onClick = OnSummonSeveralBtn;
        UIEventListener.Get(mSkipCover).onClick = OnSkipBtn;
        UIEventListener.Get(mSummonRateBtn.gameObject).onClick = OnClickSummonRateBtn;

        // 关注召唤成功的回调
        EventMgr.RegisterEvent("summonwnd", EventMgrEventType.EVENT_SUMMON_SUCCESS, WhenSummonSuccess);

        ME.user.dbase.RemoveTriggerField("SummonWnd");

        // 关注金币，友情点等属性变化
        ME.user.dbase.RegisterTriggerField("SummonWnd", new string[]
            { "gold_coin", "fp", "money", "stone_unknown",
                "stone_mystery", "stone_fire", "stone_wind", "stone_water", "stone_ld", "stone_legend", "stone_ep", "stone_mark",
                "chip_ld", "chip_legend", "chip_all", "chip_pet"
            }, new CallBack(OnFieldChange));

        ME.user.dbase.RegisterTriggerField("SummonWnd", new string[]{ "intensify_gift" }, new CallBack(OnIntensifyGiftChange));
    }

    /// <summary>
    /// 当有新的元素强化限时礼包产生回调
    /// </summary>
    void OnIntensifyGiftChange(object param, params object[] paramEx)
    {
        mIsNewIntensifyGift = true;
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        // 本地化文字
        mSummonlb.text = LocalizationMgr.Get("SummonWnd_1");
        mSummonOnelb.text = LocalizationMgr.Get("SummonWnd_1");
        mSummonTitle.text = LocalizationMgr.Get("SummonWnd_2");

        mSkipBtn.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("BaggageWnd_1");

        mSummonRateBtn.text = LocalizationMgr.Get("SummonWnd_29");

        mPrefectSummon.text = LocalizationMgr.Get("SummonWnd_34");
        mShadow.text = LocalizationMgr.Get("SummonWnd_34");

        // 创建召唤光效
        CreateEffect();

        mSummonItemModel.SetActive(false);

        mSummonPieceItemModel.SetActive(false);
    }

    /// <summary>
    /// 刷新限制次数显示
    /// </summary>
    void RefreshLimitSummon()
    {
        if(ME.user.QueryTemp<int>("gapp_world") != 1)
        {
            mLimitOb.gameObject.SetActive(false);
            return;
        }

        mLimitOb.gameObject.SetActive(true);

        CsvRow item_data = mSelectItem.mSummonItem;

        int limitTimes = item_data.Query<int>("limit_summon_times");

        string title = LocalizationMgr.Get(item_data.Query<string>("title"));

        if(limitTimes == 0)
            mLimitDesc.text = string.Format(LocalizationMgr.Get("SummonWnd_30"), title);
        else
            mLimitDesc.text = string.Format(LocalizationMgr.Get("SummonWnd_31"), title, SummonMgr.GetLocalSummonTimes(item_data.Query<int>("type")), limitTimes);
    }

    /// <summary>
    /// 玩家属性变化
    /// </summary>
    void OnFieldChange(object param, params object[] paramEx)
    {
        // 正在进行召唤不需要立即刷新，召唤结束后会刷新
        if (isSummoning)
            return;

        Redraw();
    }

    // 创建召唤光效
    private void CreateEffect()
    {
        int i = 0;

        foreach (string name in mEffectNames)
        {
            string effName = string.Format("summonEffect{0}", i);
            string loadPath = string.Format("Prefabs/3DEffect/{0}", name);

            GameObject effect = EffectMgr.CreateEffect(effName, loadPath);

            if (effect == null)
                continue;

            ParticleSystem ani = effect.GetComponent<ParticleSystem>();

            // 现将光效隐藏起来
            HideEffect(ani);

            // 添加列表
            mEffects.Add(ani);

            i++;
        }
    }

    /// <summary>
    /// Shows the effect.
    /// </summary>
    /// <param name="effectOb">Effect ob.</param>
    void ShowEffect(ParticleSystem effectOb)
    {
        // 光效对象不存在
        if (effectOb == null)
            return;

        // 设置光效层级
        int layer = LayerMask.NameToLayer("Default");
        effectOb.gameObject.layer = layer;
        effectOb.transform.SetChildLayer(layer);
        effectOb.Play();
    }

    /// <summary>
    /// Hides the effect.
    /// </summary>
    /// <param name="effectOb">Effect ob.</param>
    void HideEffect(ParticleSystem effectOb)
    {
        // 光效对象不存在
        if (effectOb == null)
            return;

        // 设置光效层级
        int layer = LayerMask.NameToLayer("Hide");
        effectOb.gameObject.layer = layer;
        effectOb.transform.SetChildLayer(layer);
        effectOb.Stop();
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        mSummonItems.Clear();

        isCriti = false;

        needResetSelect = true;

        foreach (Transform item in Container.transform)
            item.gameObject.SetActive(false);

        int i = 0;
        int pieceIndex = 0;
        int itemIndex = 0;

        foreach (CsvRow summonItem in SummonMgr.SummonCsv.rows)
        {
            // 检测召唤参数能否显示
            int showScript = summonItem.Query<int>("show_script");

            // 脚本为空默认
            if (showScript != 0)
            {
                object result = ScriptMgr.Call(showScript, ME.user, summonItem.Query<LPCValue>("show_args"));

                // 此处的返回结果可能是bool，也可能是list(碎片召唤)
                if (result is bool)
                {
                    if (!(bool)result)
                        continue;
                }
                else
                {
                    //使魔碎片召唤 petId
                    List<int> itemArray = (List<int>)result;

                    foreach (int petId in itemArray)
                    {
                        BindItem(summonItem, pieceIndex, i, petId);
                        i++;
                        pieceIndex++;
                    }

                    continue;
                }

                // 调用脚本判断能否显示
                bool canShow = (bool)ScriptMgr.Call(showScript, ME.user, summonItem.Query<LPCValue>("show_args"));
                if (!canShow)
                    continue;
            }

            BindItem(summonItem, itemIndex, i);
            itemIndex++;
            i++;
        }

        if (needResetSelect)
        {
            OnSummonItemClick(mItemObList[0]);
            ScrollView.ResetPosition();
        }
    }

    /// <summary>
    /// 召唤item
    /// </summary>
    /// <param name="summonItem"></param>
    /// <param name="itemIndex"></param>
    /// <param name="index"></param>
    /// <param name="subId">使魔召唤petId</param>
    void BindItem(CsvRow summonItem, int itemIndex, int index, int subId = 0)
    {
        GameObject SummonItemOb;

        if (subId > 0)
            SummonItemOb = itemIndex < mPieceItemObList.Count ? mPieceItemObList[itemIndex] : CreateItem(itemIndex, subId);
        else
            SummonItemOb = itemIndex < mItemObList.Count ? mItemObList[itemIndex] : CreateItem(itemIndex, subId);

        SummonItemOb.SetActive(true);
        SummonItemOb.transform.localPosition = new Vector3(0f, -(itemHeight + itemSpace) * index, 0);

        // 填充数据
        SummonItemOb.GetComponent<SummonItemWnd>().BindData(summonItem, subId);

        // 取消所有选中
        SummonItemOb.GetComponent<SummonItemWnd>().SetSelected(false);

        if (!string.IsNullOrEmpty(mSelectId) &&
            mSelectId.Equals(SummonItemOb.GetComponent<SummonItemWnd>().mItemId))
        {
            OnSummonItemClick(SummonItemOb);
            needResetSelect = false;
        }
    }

    /// <summary>
    /// 创建item
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="subId">Sub identifier.</param>
    GameObject CreateItem(int index, int subId = 0)
    {
        GameObject SummonItemOb;

        GameObject model = subId > 0 ? mSummonPieceItemModel : mSummonItemModel;
        string name = string.Format("summon_{0}_{1}",  subId > 0 ? "piece_item" : "item", index);

        SummonItemOb = Instantiate (model) as GameObject;
        SummonItemOb.name = name;
        SummonItemOb.transform.parent = Container.transform;
        SummonItemOb.transform.localScale = Vector3.one;

        if (subId > 0)
            mPieceItemObList.Add(SummonItemOb);
        else
            mItemObList.Add(SummonItemOb);

        //注册主城界面的点击事件;
        UIEventListener.Get(SummonItemOb).onClick = OnSummonItemClick;

        return SummonItemOb;
    }

    /// <summary>
    /// 召唤概率按钮点击事件回调
    /// </summary>
    void OnClickSummonRateBtn(GameObject go)
    {
        // 获取帮助信息界面;
        GameObject wnd = WindowMgr.OpenWnd(HelpWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
            return;

        wnd.GetComponent<HelpWnd>().Bind(HelpConst.SUMMON_ID);
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        EventMgr.FireEvent(EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, null, true);

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<MaskWnd>().Play();

        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnSummonMaskCallBack));

        // 销毁本窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    void OnSummonMaskCallBack(object para, object[] param)
    {
        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OnEnterMainCityScene));
    }

    /// <summary>
    /// 打开主城回调
    /// </summary>
    private void OnEnterMainCityScene(object para, object[] param)
    {
        // 打开主窗口
        WindowMgr.OpenMainWnd();

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();
    }

    /// <summary>
    /// 召唤列表被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnSummonItemClick(GameObject ob)
    {
        CsvRow item_data = ob.GetComponent<SummonItemWnd>().mSummonItem;

        if (item_data == null)
            return;

        if (ob.GetComponent<SummonItemWnd>().mIsSelected)
            return;

        string wndName = item_data.Query<string>("select_arg");

        //执行被点击后的事件
        if (!string.IsNullOrEmpty(wndName))
        {
            GameObject mWnd = WindowMgr.GetWindow(wndName);

            // 窗口对象不存在则创建一个
            if (mWnd == null)
                mWnd = WindowMgr.CreateWindow(wndName,
                    string.Format("Assets/Prefabs/Window/{0}.prefab", WindowMgr.GetCustomWindowName(wndName)),
                    this.transform);

            // 创建窗口失败
            if (mWnd == null)
            {
                LogMgr.Trace("窗口创建失败");
                return;
            }

            // 消息窗口
            WindowMgr.ShowWindow(mWnd);
        }

        // 判断能否被选中
        if (item_data.Query<int>("can_select") == 0)
            return;

        if (!string.IsNullOrEmpty(mWndName) && ! mWndName.Equals(wndName))
            WindowMgr.DestroyWindow(mWndName);

        mWndName = wndName;

        // 如果之前有选中，需要先取消之前选中状态
        if (!string.IsNullOrEmpty(mSelectId))
            mSelectItem.SetSelected(false);

        // 设置选中
        ob.GetComponent<SummonItemWnd>().SetSelected(true);

        mSelectItem = ob.GetComponent<SummonItemWnd>();

        mSelectId = ob.GetComponent<SummonItemWnd>().mItemId;

        if (mSelectItem.mSubId <= 0)
        {
            mItemIcon.gameObject.SetActive(true);
            mPetIcon.gameObject.SetActive(false);

            string iconName = SummonMgr.GetSummonIcon(item_data.Query<string>("icon"));
            mItemIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/item/{0}.png", iconName));
        }
        else
        {
            mItemIcon.gameObject.SetActive(false);
            mPetIcon.gameObject.SetActive(true);

            CsvRow petItem = MonsterMgr.GetRow(mSelectItem.mSubId);

            mPetIcon.mainTexture = MonsterMgr.GetTexture(mSelectItem.mSubId, petItem.Query<int>("rank"));

            int star = petItem.Query<int>("star");

            string StarName = PetMgr.GetStarName(petItem.Query<int>("rank"));

            for (int i = 0; i < mStars.Length; i++)
            {
                if (i < star)
                {
                    mStars[i].GetComponent<UISprite>().spriteName = StarName;
                    mStars[i].SetActive(true);
                }
                else
                    mStars[i].SetActive(false);
            }
        }

        // 刷新消耗
        RedrawSummonBtn();

        // 刷新限制显示
        RefreshLimitSummon();
    }

    /// <summary>
    /// 刷新当前消耗
    /// </summary>
    void RedrawSummonBtn()
    {
        CsvRow item_data = mSelectItem.mSummonItem;

        int canSummonTimes = item_data.Query<int>("summon_times");

        int scriptNo = item_data.Query<int>("summon_cost_script");

        LPCArray costList = new LPCArray();

        if (scriptNo > 0)
            costList = (LPCArray)ScriptMgr.Call(scriptNo, ME.user, item_data.Query<LPCArray>("summon_cost_args"), mSelectItem.mSubId);

        // 默认值
        int costNumber = 0;
        string icon = "money";

        int classId = -1;
        int itemCostNumber = 0;
        string field = string.Empty;

        // 取得消耗的属性
        if (item_data != null && costList != null && costList.Count != 0)
        {
            for (int i = 0; i < costList.Count; i++)
            {
                LPCMapping costMap = costList[i].AsMapping;

                if (costMap.ContainsKey("class_id"))
                {
                    classId = costMap.GetValue<int>("class_id");
                    itemCostNumber = costMap.GetValue<int>("amount");

                    continue;
                }

                field = FieldsMgr.GetFieldInMapping(costMap);

                costNumber = costMap[field].AsInt;
                icon = FieldsMgr.GetFieldIcon(field);
            }

        }

        // 可召唤数大于1，并且拥有数量大于可召唤数，显示多次召唤按钮
        if (canSummonTimes > 1)
        {
            bool canSummonSeverTime = false;
            if (classId > 0)
            {
                int itemAmount = UserMgr.GetAttribItemAmount(ME.user, classId);

                if (itemAmount >= itemCostNumber * canSummonTimes)
                    canSummonSeverTime = true;
            }
            else
            {
                int fieldAmount = ME.user.Query<int>(field);

                if (fieldAmount >= costNumber * canSummonTimes)
                    canSummonSeverTime = true;
            }

            // 能否进行多次召唤
            if (canSummonSeverTime)
            {
                mSummonBtn.SetActive(false);
                mSummonOneBtn.SetActive(true);
                mSummonSeveralBtn.SetActive(true);

                mCostOneNumber.text = string.Format("{0:N0}", costNumber);
                mMoneyOneIcon.spriteName = icon;

                mCostSeveralNumber.text = string.Format("{0:N0}", costNumber * canSummonTimes);
                mMoneySeveralIcon.spriteName = icon;
                mSummonSeverallb.text = string.Format(LocalizationMgr.Get("SummonWnd_23"), canSummonTimes);

                return;
            }
        }

        mSummonBtn.SetActive(true);
        mSummonOneBtn.SetActive(false);
        mSummonSeveralBtn.SetActive(false);

        mCostNumber.text = string.Format("{0:N0}", costNumber);
        mMoneyIcon.spriteName = icon;
    }

    /// <summary>
    /// 召唤按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnSummonBtn(GameObject ob)
    {
        // 召唤完成执行指引回调
        if (mGuideClickSummonCb != null)
        {
            mGuideClickSummonCb.Go();
        }

        doSummon(1);
    }

    /// <summary>
    /// 多次召唤按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnSummonSeveralBtn(GameObject ob)
    {
        int canSummonTimes = mSelectItem.mSummonItem.Query<int>("summon_times");

        doSummon(canSummonTimes);
    }

    /// <summary>
    /// 执行召唤
    /// </summary>
    /// <param name="times">Times.</param>
    public void doSummon(int times)
    {
        if (!SummonMgr.CheckSummon(mSelectItem.mSummonItem, times, mSelectItem.mSubId))
            return;

        //使魔碎片召唤
        if (mSelectItem.mSubId > 0)
        {
            // 获取召唤所需的碎片数量
            int needPiece = MonsterMgr.GetPieceAmount(mSelectItem.mSubId);
            // 获取当前拥有的数量
            LPCMapping desLpcMap = mSelectItem.mSummonItem.Query<LPCMapping>("cost_desc_args");
            int maxAmount = UserMgr.GetAttribItemAmount(ME.user, desLpcMap.GetValue<int>("class_id"), mSelectItem.mSubId);

            // 如果宠物碎片不够，查看下万能碎片够不够抵消(1.4星以下，包括4星 2.最多能抵消25个万能碎片卷)
            if (maxAmount < needPiece)
            {
                int count = ME.user.Query<int>("chip_all");
                int star = MonsterMgr.GetDefaultStar(mSelectItem.mSubId);
                if (maxAmount + count >= needPiece && (needPiece - maxAmount) <= GameSettingMgr.GetSetting<int>("max_amount_use_chip_all")
                    && star <= GameSettingMgr.GetSetting<int>("max_star_use_chip_all"))
                {
                    DialogMgr.ShowDailog(new CallBack(GotoSummon, times), string.Format(LocalizationMgr.Get("SummonWnd_36"), (needPiece - maxAmount), MonsterMgr.GetName(mSelectItem.mSubId, 1)));
                    return;
                }
            }
        }

        if (!string.IsNullOrEmpty(mWndName))
        {
            WindowMgr.DestroyWindow(mWndName);
            mWndName = string.Empty;
        }

        LPCMapping args = new LPCMapping();

        if (mSelectItem.mSubId > 0)
            args.Add("pet_id", mSelectItem.mSubId);

        bool ret = Operation.CmdSummonPet.Go(mSelectItem.mSummonItem.Query<int>("type"), times, args);

        if (!ret)
            return;

        int type = mSelectItem.mSummonItem.Query<int>("type");

        // 如果开了审核并且有限制
        if(ME.user.QueryTemp<int>("gapp_world") == 1 && SummonMgr.GetLimitTimes(type) > 0)
            SummonMgr.SetLocalSummonTimes(type, times);

        // 标识正在进行召唤
        isSummoning = true;

        // 标识相机动画还没有播放完毕
        isCameraTweenOver = false;

        // 是否已显示过结果
        hasShowResult = false;

        // 开始召唤
        StartSummon();
    }

    /// <summary>
    /// 召唤
    /// </summary>
    private void GotoSummon(object para, params object[] _params)
    {
        if (!(bool)_params[0])
            return;

        if (!string.IsNullOrEmpty(mWndName))
        {
            WindowMgr.DestroyWindow(mWndName);
            mWndName = string.Empty;
        }

        LPCMapping args = new LPCMapping();

        if (mSelectItem.mSubId > 0)
            args.Add("pet_id", mSelectItem.mSubId);

        bool ret = Operation.CmdSummonPet.Go(mSelectItem.mSummonItem.Query<int>("type"), (int)para, args);

        if (!ret)
            return;

        int type = mSelectItem.mSummonItem.Query<int>("type");

        // 如果开了审核并且有限制
        if (ME.user.QueryTemp<int>("gapp_world") == 1 && SummonMgr.GetLimitTimes(type) > 0)
            SummonMgr.SetLocalSummonTimes(type, (int)para);

        // 标识正在进行召唤
        isSummoning = true;

        // 标识相机动画还没有播放完毕
        isCameraTweenOver = false;

        // 是否已显示过结果
        hasShowResult = false;

        // 开始召唤
        StartSummon();
    }

    /// <summary>
    /// 开始召唤
    /// </summary>
    void StartSummon()
    {
        // 执行召唤
        mShowSummonWnd.SetActive(false);

        // 图标淡出效果
        if (mItemIcon.gameObject.activeInHierarchy)
            mItemIcon.GetComponent<TweenAlpha>().PlayForward();

        if (mPetIcon.gameObject.activeInHierarchy)
            mPetIcon.GetComponent<TweenAlpha>().PlayForward();

        //mEffects[0].gameObject.SetActive(true);
        //mEffects[0].Play();
        ShowEffect(mEffects[0]);

        if (blackTweenCover == null)
            blackTweenCover = GameObject.Find("SceneRoot/Summon/black");

        blackTweenCover.GetComponent<TweenAlpha>().PlayForward();

        // 播放光效
        Coroutine.DispatchService(SyncPlaySmall(), "SyncPlaySmall");

        // 相机移动
        Coroutine.DispatchService(SyncCameraRemove(), "SyncCameraRemove");

        // 等待服务器消息
        Coroutine.DispatchService(SyncWaitSever(), "SyncWaitSever");

        // 等待开始音乐播放结束
        Coroutine.DispatchService(SynPlayStartMusic(), "SynPlayStartMusic");
    }


    /// <summary>
    /// 播放开始音乐
    /// </summary>
    /// <returns>The wait sever.</returns>
    IEnumerator SynPlayStartMusic()
    {
        // 一秒钟逐渐缩小背景音效音量
        GameSoundMgr.SetAudioVolumeFadeScale(0.4f, 1f);

        // 播放开始召唤音乐
        mSoundName = Game.GetUniqueName("summon_start");
        GameSoundMgr.PlayGroupSound("summon_start", mSoundName);

        yield return new WaitForSeconds(0.5f);

        GameSoundMgr.StopSound(mSoundName);

        mSoundName = Game.GetUniqueName("summon_cycle");
        GameSoundMgr.PlayGroupSound("summon_cycle", mSoundName);
    }

    /// <summary>
    /// 播放小光效
    /// </summary>
    /// <returns>The wait sever.</returns>
    IEnumerator SyncPlaySmall()
    {
        yield return new WaitForSeconds(0.3f);

        //mEffects[1].gameObject.SetActive(true);
        //mEffects[1].Play();
        ShowEffect(mEffects[1]);
    }

    /// <summary>
    /// 等待服务器消息(收到服务器消息也等待2s)
    /// </summary>
    /// <returns>The wait sever.</returns>
    IEnumerator SyncWaitSever()
    {
        yield return new WaitForSeconds(2f);

        while (mSummonItems == null || mSummonItems.Count == 0)
            yield return null;

        // 预先加载模型等资源
        yield return Coroutine.DispatchService(SynPreLoadRes());

        while (!isCameraTweenOver)
            yield return null;

        // 等待服务器消息
        Coroutine.DispatchService(SyncHideSmall(), "SyncHideSmall");

        if (isCriti)
        {
            //mEffects[3].gameObject.SetActive(true);
            //mEffects[3].Play();
            ShowEffect(mEffects[3]);

            // 播放暴击音效
            mPrefectSoundName = Game.GetUniqueName("summon_prefect");
            GameSoundMgr.PlayGroupSound("summon_prefect", mPrefectSoundName);

            // 移动相机
            tw[1].enabled = true;
            tw[1].ResetAllToBeginning();
            tw[1].PlayForward();
        }
        else
        {
            //mEffects[2].gameObject.SetActive(true);
            //mEffects[2].Play();
            ShowEffect(mEffects[2]);
        }

        mSkipBtn.SetActive(true);

        mSkipCover.SetActive(true);

        // 显示召唤结果
        Coroutine.DispatchService(SyncShowSummonResult(), "SyncShowSummonResult");
    }

    /// <summary>
    /// 预先加载模型等资源
    /// </summary>
    /// <returns>The wait sever.</returns>
    IEnumerator SynPreLoadRes()
    {
        for (int i = 0; i < mSummonItems.Count; i++)
        {
            if (mSummonItems[i] == null)
                continue;

            int classId = mSummonItems[i].GetClassID();

            string modelId = MonsterMgr.GetModel(classId);

            string icon = MonsterMgr.GetIcon(classId,  mSummonItems[i].GetRank());

            // 没有模型id
            if (string.IsNullOrEmpty(modelId) || string.IsNullOrEmpty(icon))
                continue;

            // 异步加载宠物模型
            yield return Coroutine.DispatchService(ResourceMgr.LoadAsync(MonsterMgr.GetModelResPath(modelId)));

            // 异步加载宠物头像
            yield return Coroutine.DispatchService(ResourceMgr.LoadAsync(MonsterMgr.GetIconResPath(icon)));
        }

        // 如果只召唤一个，需要提前异步加载技能图标
        if (mSummonItems.Count == 1)
        {
            // 获取绑定宠物的技能
            LPCArray skillInfo = mSummonItems[0].GetAllSkills();

            // 对字典按key（skillid）进行排序
            foreach (LPCValue mks in skillInfo.Values)
            {
                int skillId = mks.AsArray[0].AsInt;
                string icon = SkillMgr.GetIcon(skillId);

                if (string.IsNullOrEmpty(icon))
                    continue;

                // 异步加载技能图标
                yield return Coroutine.DispatchService(ResourceMgr.LoadAsync(SkillMgr.GetIconResPath(icon)));
            }
        }
    }

    /// <summary>
    /// 延迟显示小光效
    /// </summary>
    /// <returns>The wait sever.</returns>
    IEnumerator SyncHideSmall()
    {
        yield return new WaitForSeconds(delaySmallTime);

        //mEffects[1].gameObject.SetActive(false);
        HideEffect(mEffects[1]);
    }

    /// <summary>
    /// 召唤成功事件
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    void WhenSummonSuccess(int eventId, MixedValue para)
    {
        // 数据格式转换
        LPCMapping summonData = para.GetValue<LPCMapping>();

        foreach (string rid in summonData.Keys)
        {
            // 获取targetOb
            Property target = Rid.FindObjectByRid(rid);
            if (target == null)
                continue;

            mSummonItems.Add(target);

            if (summonData[rid].AsInt == 1)
                isCriti = true;
        }
    }

    /// <summary>
    /// 相机移动效果
    /// </summary>
    /// <returns>The camera remove.</returns>
    IEnumerator SyncCameraRemove()
    {
        yield return new WaitForSeconds(0.1f);
        tw[0].PlayForward();

        // 添加动画播放结束回调
        EventDelegate.Add(tw[0].onFinished, OnCameraTweenFinish, true);
    }

    /// <summary>
    /// 移动相机回调，播放相机呼吸灯效果
    /// </summary>
    void OnCameraTweenFinish()
    {
        // 等到相机动画播放结束
        isCameraTweenOver = true;

        tw[2].enabled = true;
        tw[2].ResetAllToBeginning();
        tw[2].PlayForward();
    }

    /// <summary>
    /// 跳过按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnSkipBtn(GameObject ob)
    {
        Coroutine.StopCoroutine("SyncShowSummonResult");

        ShowSummonResult(isCriti);
    }

    /// <summary>
    ///  显示召唤结果
    /// </summary>
    /// <returns>The configs.</returns>
    IEnumerator SyncShowSummonResult()
    {
        yield return new WaitForSeconds(4f);

        ShowSummonResult(isCriti);
    }

    /// <summary>
    /// 显示召唤结果
    /// </summary>
    void ShowSummonResult(bool isCriti)
    {
        if (hasShowResult)
            return;

        hasShowResult = true;

        mSkipBtn.SetActive(false);

        mSkipCover.SetActive(false);

        // 结束之前播放的音效
        GameSoundMgr.StopSound(mSoundName, FadeType.OUT, 1f);

        if(isCriti)
            GameSoundMgr.StopSound(mPrefectSoundName, FadeType.OUT, 1f);

        // 开始播放结束音效
        GameSoundMgr.PlayGroupSound("summon_complete");

        // 一秒钟回复背景音效音量
        GameSoundMgr.SetAudioVolumeFadeScale(1f, 1f);

        if (isCriti)
            tw[1].enabled = false;

        SummonMgr.LoadModel(mSummonItems, petPos);

        mWhiteFlag.SetActive(true);
        mWhiteFlag.GetComponent<TweenAlpha>().PlayForward();

        if (isCriti && mSummonItems.Count == 1)
        {
            mPrefectEfect.SetActive(true);

            // 召唤的星级
            int stars = mSummonItems[0].GetStar();

            if (stars < 3)
                stars = 3;

            mStarEffects[stars - 3].SetActive(true);

            // 宠物星级
            string starName = PetMgr.GetStarName(mSummonItems[0].GetRank());

            foreach (Transform star in mStarEffects[stars - 3].transform)
            {
                star.GetComponent<TweenAlpha>().PlayForward();

                star.GetComponent<TweenScale>().PlayForward();

                star.GetComponent<UISprite> ().spriteName = starName;
            }

            mPrefectEffectLb.GetComponent<TweenAlpha>().PlayForward();

            mPrefectEffectLb.GetComponent<TweenScale>().PlayForward();

            // 显示宠物信息
            Coroutine.DispatchService(SyncSummonInfo(), "SyncSummonInfo");
        }
        else
        {
            ShowSummonInfoWnd();
        }

        //mEffects[2].gameObject.SetActive(false);
        //mEffects[3].gameObject.SetActive(false);
        HideEffect(mEffects[2]);
        HideEffect(mEffects[3]);

        //mEffects[4].gameObject.SetActive(true);
        //mEffects[4].Play();
        ShowEffect(mEffects[4]);

        blackTweenCover.GetComponent<TweenAlpha>().PlayReverse();
    }

    /// <summary>
    /// 打开元素强化限时礼包
    /// </summary>
    void OpenIntensifyWnd()
    {
        if (ME.user == null)
            return;

        LPCArray gift = MarketMgr.GetLimitStrengthList(ME.user);

        if (gift == null || gift.Count == 0)
            return;

        // 打开限时商城界面
        GameObject wnd = WindowMgr.OpenWnd(LimitGiftBagWnd.WndType);

        if (wnd == null)
            return;

        wnd.GetComponent<LimitGiftBagWnd>().Bind(gift[gift.Count - 1].AsMapping["class_id"].AsInt);
    }

    /// <summary>
    /// 显示宠物信息界面
    /// </summary>
    /// <returns>The summon.</returns>
    IEnumerator SyncSummonInfo()
    {
        yield return new WaitForSeconds(2f);
        ShowSummonInfoWnd();
    }

    /// <summary>
    /// 显示宠物信息
    /// </summary>
    void ShowSummonInfoWnd()
    {
        tw[2].enabled = false;

        if (isCriti)
            mPrefectEfect.GetComponent<TweenAlpha>().PlayForward();

        // 移动相机
        tw[0].PlayReverse();

        // 多次召唤应该显示多个宠物面板
        if (mSummonItems.Count == 1)
        {
            GameObject wndOb = WindowMgr.OpenWnd("PetSimpleInfoWnd_Summon", transform);

            PetSimpleInfoWnd wnd = wndOb.GetComponent<PetSimpleInfoWnd>();

            wnd.Bind(mSummonItems[0], false, false);
            //需要引导才开启召唤分享功能
            if (GuideMgr.IsGuided(GuideMgr.SHARE_SHOW_GUIDE_GROUP) && ShareMgr.IsOpenShare())
            {
                wnd.ShowShareBtn();
                wnd.SetShareCallBack(new CallBack(DoShare));
            }
            else
            {
                wnd.ShowBtn(true);
            }

            wnd.SetCallBack(new CallBack(OnCloseInfoWnd));
            wndOb.transform.localPosition = new Vector3(333, 0, 0);
            mCurShareType = ShareOperateWnd.ShareOperateType.SingleSummon;
        }
        else
        {
            GameObject wndOb = WindowMgr.OpenWnd(SummonSeveralTimesWnd.WndType, transform);
            wndOb.GetComponent<SummonSeveralTimesWnd>().BindData(mSummonItems, new CallBack(OnCloseInfoWnd));
            mCurShareType = ShareOperateWnd.ShareOperateType.TenSummon;
            //需要引导结束了才开启召唤分享功能
            if (GuideMgr.IsGuided(GuideMgr.SHARE_SHOW_GUIDE_GROUP) && ShareMgr.IsOpenShare())
            {
                wndOb.GetComponent<SummonSeveralTimesWnd>().SetShareBtn(true);
                wndOb.GetComponent<SummonSeveralTimesWnd>().SetShareCallBack(new CallBack(DoShare));
            }
            else
            {
                wndOb.GetComponent<SummonSeveralTimesWnd>().SetShareBtn(false);
            }
        }

        // 召唤完成执行指引回调
        if (mGuideSummonFinishCb != null)
            mGuideSummonFinishCb.Go();
    }

    /// <summary>
    /// 处理分享操作
    /// </summary>
    /// <param name="para"></param>
    /// <param name="param"></param>
    private void DoShare(object para, params object[] param)
    {
        if (SceneMgr.SceneCamera != null)
        {

            Vector3 originPos = SceneMgr.SceneCamera.transform.localPosition;
            SceneMgr.SceneCamera.transform.localPosition = new Vector3(0f, 0f, originPos.z);

            //计算实际图片大小，但改变窗口屏幕大小时，图片的实际像素是不一样的。
            float captureWeight = 1f * Screen.width * 667f / 1280;
            float captureHeight = 1f * Screen.height * 400f / 720;

            Texture texture = Game.CaptureCamera(new Camera[] { SceneMgr.SceneCamera }, new Rect(0f, 0f, captureWeight, captureHeight));
            SceneMgr.SceneCamera.transform.localPosition = originPos;

            GameObject shareWnd = WindowMgr.OpenWnd(ShareOperateWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (shareWnd != null)
                shareWnd.GetComponent<ShareOperateWnd>().BindData(mCurShareType, texture, mSelectItem.mSummonItem, mSummonItems.Count > 0 ? mSummonItems[0] : null);
        }
    }

    /// <summary>
    /// 关闭宠物详细信息窗口回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="param">Parameter.</param>
    void OnCloseInfoWnd(object para, params object[] param)
    {
        if (mGuideCloseCb != null)
        {
            mGuideCloseCb.Go();
        }

        if (this != null)
        {
            mShowSummonWnd.SetActive(true);
            RedrawWnd();

            if (mIsNewIntensifyGift)
            {
                mIsNewIntensifyGift = false;
                OpenIntensifyWnd();
            }
        }
    }

    #endregion

    #region 外部接口

    public void RedrawWnd()
    {
        // 标识召唤状态
        isSummoning = false;

        mSummonItems.Clear();

        // 卸载就模型
        SummonMgr.UnLoadModel();

        foreach (ParticleSystem effect in mEffects)
        {
            //effect.gameObject.SetActive(false);
            HideEffect(effect);
        }

        mPrefectEfect.GetComponent<TweenAlpha>().ResetToBeginning();
        mPrefectEfect.SetActive(false);

        mPrefectEffectLb.GetComponent<TweenAlpha>().ResetToBeginning();

        mPrefectEffectLb.GetComponent<TweenScale>().ResetToBeginning();

        foreach (GameObject ob in mStarEffects)
        {
            foreach (Transform star in ob.transform)
            {
                star.GetComponent<TweenAlpha>().ResetToBeginning();

                star.GetComponent<TweenScale>().ResetToBeginning();
            }
        }

        mItemIcon.GetComponent<TweenAlpha>().ResetToBeginning();
        mPetIcon.GetComponent<TweenAlpha>().ResetToBeginning();

        // 还原白色闪屏的淡入淡出
        mWhiteFlag.GetComponent<TweenAlpha>().ResetToBeginning();
        mWhiteFlag.SetActive(false);

        // 刷新召唤页面
        Redraw();

        // 刷新按钮消耗
        RedrawSummonBtn();

        // 刷新限制显示
        RefreshLimitSummon();
    }

    /// <summary>
    /// 绑定指引回调
    /// </summary>
    public void BindGuideCallBack(CallBack clickSummonCb, CallBack finishCb, CallBack closeClickCb)
    {
        mGuideClickSummonCb = clickSummonCb;

        mGuideSummonFinishCb = finishCb;

        mGuideCloseCb = closeClickCb;

        mIsNewIntensifyGift = false;
    }

    #endregion
}
