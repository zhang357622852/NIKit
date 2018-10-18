/// <summary>
/// PetToolTipWnd.cs
/// Created by lic 2016-6-24
/// 宠物信息界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BAGGAGE_PAGE
{
    ATTRIB_PAGE,
    EQUIP_PAGE,
    SKILL_PAGE,
    AWAKE_PAGE,
}

public class PetToolTipWnd : WindowBase<PetToolTipWnd>
{
    #region 成员变量

    // 本地化label文字
    public UILabel msuitLb;

    public UILabel[] mGroupTabLb;

    public UISprite[] mStars;
    // 宠物星级
    public UISprite mElement;
    // 宠物元素
    public Vector3 mElementPos;
    // 元素位置
    public UILabel mlvAndName;
    // 等级和名称显示在一起
    public UILabel msuitName;
    // 套装名称

    // 武器部位
    public GameObject[] mEquipItem;

    // 按钮组
    public GameObject[] mGroupTab;

    // 窗口
    public GameObject[] mGroupWnd;

    // 宠物模型
    public GameObject mPetModel;

    // 没有使魔提示
    public UILabel mNoPetDesc;

    public UISpriteAnimation mNewEquipTips;

    // 绑定的宠物对象
    private Property item_ob = null;

    // 当前显示的玩家的属性
    private int curPage = 0;

    // 当前选择的装备Rid
    private string mSelectRid = string.Empty;

    // 第一次打开时待窗口动画完成后再加载模型
    private bool isTweenOver = false;

    // 取得当前所有的装备数据
    List<Property> equipData = new List<Property>();

    private bool mIsResetPage = true;

    #endregion

    #region 内部函数

    /// <summary>
    /// Registers the event.
    /// </summary>
    void RegisterWndEvent()
    {
        // 注册EquipItem点击事件
        for (int i = 0; i < mEquipItem.Length; i++)
            UIEventListener.Get(mEquipItem[i]).onClick += OnPetItemClick;

        // 注册tap页点击事件
        UIEventListener.Get(mGroupTab[0]).onClick += OnTabBtn0Clicked;
        UIEventListener.Get(mGroupTab[1]).onClick += OnTabBtn1Clicked;
        UIEventListener.Get(mGroupTab[2]).onClick += OnTabBtn2Clicked;
        UIEventListener.Get(mGroupTab[3]).onClick += OnTabBtn3Clicked;
    }

    void Start()
    {
        // 初始化界面
        InitWnd();

        // 注册窗口事件
        RegisterWndEvent();
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        // 注册新物品信息被清除
        EventMgr.RegisterEvent(gameObject.name, EventMgrEventType.EVENT_CLEAR_NEW, ClearNewInfo);

        // 检测是否有新装备
        DoCheckNewTips();

        // 如果玩家对象不存在，不处理
        if (ME.user == null)
            return;

        // 注册玩家装备道具事件
        ME.user.baggage.eventCarryChange += BaggageChange;
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDisable()
    {
        mIsResetPage = true;

        // 重置isTweenOver标识
        isTweenOver = false;

        // 隐藏模型
        UnloadModel();

        // 取消原来绑定镀对象包裹变化回调
        if (item_ob != null)
        {
            (item_ob as Container).baggage.eventCarryChange -= OnEquipChange;
            item_ob.dbase.RemoveTriggerField("PetToolTipWnd");
        }

        // 取消事件关注
        EventMgr.UnregisterEvent(gameObject.name);

        // 如果玩家对象不存在，不处理
        if (ME.user == null)
            return;

        // 取消注册玩家装备道具事件
        ME.user.baggage.eventCarryChange -= BaggageChange;
    }

    /// <summary>
    /// 显示模型
    /// </summary>
    void ShowModel()
    {
        // 道具对象不存在
        if (item_ob == null)
            return;

        // 获取窗口绑定的模型窗口组件
        ModelWnd pmc = mPetModel.GetComponent<ModelWnd>();

        // 没有绑定模型窗口组件
        if (pmc == null)
            return;

        // 异步载入模型
        pmc.LoadModelSync(item_ob.GetClassID(), item_ob.GetRank(), LayerMask.NameToLayer("UI"));
    }

    void UnloadModel()
    {
        // 获取窗口绑定的模型窗口组件
        ModelWnd pmc = mPetModel.GetComponent<ModelWnd>();

        // 没有绑定模型窗口组件
        if (pmc == null)
            return;

        pmc.UnLoadModel();
    }

    /// <summary>
    /// 初始化窗口.
    /// </summary>
    void InitWnd()
    {
        mNewEquipTips.namePrefix = ConfigMgr.IsCN ? "cnew" : "new";

        // 本地化文字
        msuitLb.text = LocalizationMgr.Get("PetToolTipWnd_1");
        mGroupTabLb[0].text = LocalizationMgr.Get("PetToolTipWnd_2");
        mGroupTabLb[1].text = LocalizationMgr.Get("PetToolTipWnd_3");
        mGroupTabLb[2].text = LocalizationMgr.Get("PetToolTipWnd_4");
        mGroupTabLb[3].text = LocalizationMgr.Get("PetToolTipWnd_5");
        mNoPetDesc.text = LocalizationMgr.Get("PetToolTipWnd_7");

        // 设置equip的check状态
        foreach (GameObject item in mEquipItem)
            item.GetComponent<EquipItemWnd>().SetCheck(false);

        // 设置按钮选中状态
        SetSelect();
    }

    void ClearNewInfo(int eventId, MixedValue para)
    {
        // 检测新物品提示
        DoCheckNewTips();
    }

    /// <summary>
    /// 检测新物品提示
    /// </summary>
    void DoCheckNewTips()
    {
        if (ME.user == null)
        {
            mNewEquipTips.gameObject.SetActive(false);
            return;
        }

        List<Property> equips = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_ITEM_GROUP);

        if (BaggageMgr.HasNewItem(equips))
        {
            mNewEquipTips.gameObject.SetActive(true);

            mNewEquipTips.ResetToBeginning();
        }
        else
        {
            mNewEquipTips.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 包裹变化回调
    /// </summary>
    void BaggageChange(string[] pos)
    {
        DoCheckNewTips();

        equipData = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_ITEM_GROUP);

        for (int i = 0; i < pos.Length; i++)
        {
            if (ContainerConfig.IS_ITEM_POS(pos[i]))
            {
                if (equipData == null || equipData.Count == 0)
                {
                    if (item_ob != null)
                        break;

                    mGroupWnd[1].SetActive(false);
                    if (curPage == 1)
                    {
                        mNoPetDesc.gameObject.SetActive(true);
                    }
                }
                else
                {
                    if (item_ob != null)
                        break;

                    if (curPage == 1)
                    {
                        mGroupWnd[curPage].SetActive(true);
                        mNoPetDesc.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 宠物按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnPetItemClick(GameObject ob)
    {
        // 取得格子上绑定的装备对象
        Property equipItem = ob.GetComponent<EquipItemWnd>().ItemOb;

        // 不响应空格子
        if (equipItem == null || item_ob == null)
            return;

        // 当前装备为已选中状态
        if (equipItem.GetRid().Equals(mSelectRid))
            return;

        // 重置选中状态
        ResetEquipSelect();

        mSelectRid = equipItem.GetRid();
        ob.GetComponent<EquipItemWnd>().SetSelected(true);

        // 打开悬浮窗口
        GameObject wnd = WindowMgr.GetWindow(EquipViewWnd.WndType + "_Equip");

        if (wnd == null)
        {
            wnd = WindowMgr.CreateWindow("EquipViewWnd_Equip", EquipViewWnd.PrefebResource);

            if (wnd == null)
                return;

            WindowMgr.AddToOpenWndList(wnd, WindowOpenGroup.SINGLE_OPEN_WND);

            Vector3 pos = wnd.transform.localPosition;

            wnd.transform.localPosition = new Vector3(pos.x, 211, pos.z);

            GameObject bagWnd = WindowMgr.GetWindow(BaggageWnd.WndType);

            if (bagWnd != null)
                wnd.transform.SetParent(bagWnd.transform);
        }

        wnd.SetActive(true);
        wnd.GetComponent<EquipViewWnd>().Bind(equipItem.GetRid(), item_ob.GetRid(), new CallBack(OnCloseEquipView));
    }

    /// <summary>
    /// 关闭装备悬浮回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="_params">Parameters.</param>
    void OnCloseEquipView(object para, params object[] _params)
    {
        ResetEquipSelect();
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    void Redraw()
    {
        // 取消原来的选择
        ResetEquipSelect();

        for (int i = 0; i < mStars.Length; i++)
            mStars[i].gameObject.SetActive(false);

        mlvAndName.gameObject.SetActive(false);
        msuitName.gameObject.SetActive(false);
        mElement.gameObject.SetActive(false);
        mNoPetDesc.gameObject.SetActive(false);

        equipData = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_ITEM_GROUP);

        if ((equipData == null || equipData.Count == 0) && curPage == 1)
        {
            mNoPetDesc.gameObject.SetActive(true);
        }
        if (equipData.Count > 0 && curPage == 1)
        {
            mNoPetDesc.gameObject.SetActive(false);
        }

        if (item_ob == null)
        {
            UnloadModel();

            mNoPetDesc.gameObject.SetActive(true);

            mGroupWnd[curPage].SetActive(false);

            return;
        }

        mNoPetDesc.gameObject.SetActive(false);

        mGroupWnd[curPage].SetActive(true);

        // 显示等级与名字
        int Level = item_ob.GetLevel();

        string nameAndLv = string.Format(LocalizationMgr.Get("PetToolTipWnd_8"), Level, item_ob.Short());

        mlvAndName.text = string.Format("{0}{1}",
            PetMgr.GetAwakeColor(item_ob.GetRank()), nameAndLv);
        mlvAndName.gameObject.SetActive(true);

        // 获取宠物的元素
        int element = MonsterMgr.GetElement(item_ob.GetClassID());
        mElement.spriteName = PetMgr.GetElementIconName(element);
        mElement.gameObject.SetActive(true);

        // 检测宠物觉醒状态
        string starName = PetMgr.GetStarName(item_ob.GetRank());

        // 宠物的星级
        int stars = item_ob.GetStar();

        int count = stars < mStars.Length ? stars : mStars.Length;

        for (int i = 0; i < count; i++)
        {
            mStars[i].gameObject.SetActive(true);

            mStars[i].GetComponent<UISprite>().spriteName = starName;
        }

        mElement.transform.localPosition = mElementPos;

        // 固定星级与元素的位置
        float offset = (2 - (count - 1) / 2) * 18;
        Vector3 pos = mElement.transform.localPosition;

        mElement.transform.localPosition = new Vector3(pos.x + offset, pos.y, pos.z);

        // 检测宠物的元素
        mElement.spriteName = PetMgr.GetElementIconName(MonsterMgr.GetElement(item_ob.GetClassID()));
        mElement.gameObject.SetActive(true);

        // 设置绑定宠物模型
        if(isTweenOver)
            ShowModel();

        // 刷新当前属性页面
        RedrawCurPage(false);

        // 宠物套装名称
        msuitName.text = PetMgr.GetPetSuitName(item_ob);
        msuitName.gameObject.SetActive(true);

        // 填入宠物装备数据
        RedrawPetEquip();
    }

    /// <summary>
    /// 填入宠物装备数据
    /// </summary>
    void RedrawPetEquip()
    {
        if (item_ob == null)
            return;

        bool isSelectEquipDestory = true;

        for (int i = 0; i < mEquipItem.Length; i++)
        {
            int type = mEquipItem[i].GetComponent<EquipItemWnd>().equipType;

            // 获取宠物身上该位置的装备
            Property equipData = (item_ob as Container).baggage.GetCarryByPos(EquipMgr.GetEquipPos(type));

            mEquipItem[i].GetComponent<EquipItemWnd>().SetBind(equipData);

            mEquipItem[i].GetComponent<EquipItemWnd>().SetNewTips(equipData);

            if (equipData != null && equipData.GetRid().Equals(mSelectRid))
                isSelectEquipDestory = false;
        }

        if (isSelectEquipDestory)
            ResetEquipSelect();

        // 刷新套装
        msuitName.text = PetMgr.GetPetSuitName(item_ob);
    }

    /// <summary>
    /// 宠物身上的装备发生变化
    /// </summary>
    void OnEquipChange(string[] pos)
    {
        if (!gameObject.activeSelf || !gameObject.activeInHierarchy)
            return;

        RedrawPetEquip();
    }

    /// <summary>
    /// 设置属性页面刷新
    /// </summary>
    /// <param name="page">Page.</param>
    void RedrawCurPage(bool redrawEquip = true)
    {
        switch (curPage)
        {
            case (int)BAGGAGE_PAGE.ATTRIB_PAGE:
                mGroupWnd[0].GetComponent<AttributeWnd>().SetBind(item_ob, true);
                break;

            case (int)BAGGAGE_PAGE.EQUIP_PAGE:
                mGroupWnd[1].GetComponent<EquipWnd>().SetBind(item_ob, redrawEquip);
                break;

            case (int)BAGGAGE_PAGE.SKILL_PAGE:
                mGroupWnd[2].GetComponent<SkillWnd>().SetBind(item_ob);
                break;

            case (int)BAGGAGE_PAGE.AWAKE_PAGE:
                mGroupWnd[3].GetComponent<AwakeWnd>().BindData(item_ob);
                break;
        }
    }

    /// <summary>
    /// 重置当前装备选中状态
    /// </summary>
    public void ResetEquipSelect()
    {
        for (int i = 0; i < mEquipItem.Length; i++)
            mEquipItem[i].GetComponent<EquipItemWnd>().SetSelected(false);

        mSelectRid = string.Empty;

        // 关闭悬浮窗口
        GameObject equipViewWnd = WindowMgr.GetWindow("EquipViewWnd_Equip");

        if (equipViewWnd != null && equipViewWnd.activeInHierarchy)
            WindowMgr.HideWindow(equipViewWnd);
    }

    /// <summary>
    /// 设置当前页面按钮选中
    /// </summary>
    /// <param name="ob">Ob.</param>
    void SetSelect()
    {
        for (int i = 0; i < mGroupTab.Length; i++)
        {
            UISprite sp = mGroupTab[i].GetComponent<UISprite>();
            Transform lbTrans = mGroupTabLb[i].transform;

            if (i == curPage)
            {
                sp.spriteName = "right_select";
                sp.alpha = 1.0f;
                lbTrans.localPosition = new Vector3(10, 3, lbTrans.localPosition.z);
                sp.width = 152;
            }
            else
            {
                sp.spriteName = "right_unselect";
                sp.alpha = 0.5f;
                lbTrans.localPosition = new Vector3(0, 0, lbTrans.localPosition.z);
                sp.width = 132;
            }
        }

        if (curPage == (int)BAGGAGE_PAGE.EQUIP_PAGE)
        {
            mNewEquipTips.transform.localPosition = new Vector3(361,
                mNewEquipTips.transform.localPosition.y,
                mNewEquipTips.transform.localPosition.z);
        }
        else
        {
            mNewEquipTips.transform.localPosition = new Vector3(348,
                mNewEquipTips.transform.localPosition.y,
                mNewEquipTips.transform.localPosition.z);
        }
    }

    /// <summary>
    /// 第一个分页
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnTabBtn0Clicked(GameObject ob)
    {
        if (curPage == (int)BAGGAGE_PAGE.ATTRIB_PAGE)
            return;

        mGroupWnd[curPage].SetActive(false);

        curPage = (int)BAGGAGE_PAGE.ATTRIB_PAGE;

        // 设置按钮选中状态
        SetSelect();

        if (item_ob == null)
        {
            if (! mNoPetDesc.gameObject.activeSelf)
                mNoPetDesc.gameObject.SetActive(true);

            return;
        }

        if (mNoPetDesc.gameObject.activeSelf)
            mNoPetDesc.gameObject.SetActive(false);

        mGroupWnd[curPage].SetActive(true);

        // 刷新当前属性页面
        RedrawCurPage();

        HideEquipViewWnd();
    }

    /// <summary>
    /// 第二个分页
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnTabBtn1Clicked(GameObject ob)
    {
        if (curPage == (int)BAGGAGE_PAGE.EQUIP_PAGE)
            return;

        mGroupWnd[curPage].SetActive(false);

        curPage = (int)BAGGAGE_PAGE.EQUIP_PAGE;

        // 设置按钮选中状态
        SetSelect();

        if ((equipData == null || equipData.Count == 0) && item_ob == null)
        {
            if (! mNoPetDesc.gameObject.activeSelf)
                mNoPetDesc.gameObject.SetActive(true);

            return;
        }

        if (mNoPetDesc.gameObject.activeSelf)
            mNoPetDesc.gameObject.SetActive(false);

        mGroupWnd[curPage].SetActive(true);

        // 刷新当前属性页面
        RedrawCurPage();

        HideEquipViewWnd();
    }

    /// <summary>
    /// 第三个分页
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnTabBtn2Clicked(GameObject ob)
    {
        if (curPage == (int)BAGGAGE_PAGE.SKILL_PAGE)
            return;

        mGroupWnd[curPage].SetActive(false);

        curPage = (int)BAGGAGE_PAGE.SKILL_PAGE;

        // 设置按钮选中状态
        SetSelect();

        if (item_ob == null)
        {
            if (! mNoPetDesc.gameObject.activeSelf)
                mNoPetDesc.gameObject.SetActive(true);

            return;
        }

        if (mNoPetDesc.gameObject.activeSelf)
            mNoPetDesc.gameObject.SetActive(false);

        mGroupWnd[curPage].SetActive(true);

        // 刷新当前属性页面
        RedrawCurPage();

        HideEquipViewWnd();
    }

    /// <summary>
    /// 第四个分页
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnTabBtn3Clicked(GameObject ob)
    {
        if (curPage == (int)BAGGAGE_PAGE.AWAKE_PAGE)
            return;

        mGroupWnd[curPage].SetActive(false);

        curPage = (int)BAGGAGE_PAGE.AWAKE_PAGE;

        // 设置按钮选中状态
        SetSelect();

        if (item_ob == null)
        {
            if (! mNoPetDesc.gameObject.activeSelf)
                mNoPetDesc.gameObject.SetActive(false);

            return;
        }

        if (mNoPetDesc.gameObject.activeSelf)
            mNoPetDesc.gameObject.SetActive(true);

        mGroupWnd[curPage].SetActive(true);

        // 刷新当前属性页面
        RedrawCurPage();

        HideEquipViewWnd();
    }

    /// <summary>
    /// 销毁装备查看窗口
    /// </summary>
    void HideEquipViewWnd()
    {
        // 销毁装备信息窗口
        GameObject equipWnd = WindowMgr.GetWindow("EquipViewWnd_UnEquip");

        if (equipWnd != null && equipWnd.activeInHierarchy)
            WindowMgr.HideWindow(equipWnd);
    }

    /// <summary>
    /// 绑定宠物事件回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    private void OnPetItemChange(object param, params object[] paramEx)
    {
        // 当前界面没有绑定宠物不处理
        if (item_ob == null)
            return;

        // 重绘窗口
        Redraw();
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// TweenAlpha结束的回调
    /// </summary>
    public void OnTweenFinished()
    {
        // 标识isTweenOver为ture
        isTweenOver = true;

        // 载入模型
        ShowModel();
    }

    /// <summary>
    /// 窗口绑定实体
    /// </summary>
    public void SetBind(Property ob)
    {
        // 取消原来绑定镀对象包裹变化回调
        if (item_ob != null)
        {
            (item_ob as Container).baggage.eventCarryChange -= OnEquipChange;
            item_ob.dbase.RemoveTriggerField("PetToolTipWnd");
        }

        if (ob != null)
            ob.dbase.RegisterTriggerField("PetToolTipWnd", new string[]
                {
                    "level",
                    "star",
                    "rank"
                }, new CallBack(OnPetItemChange));

        // 重置绑定对象
        item_ob = ob;

        if (mIsResetPage)
        {
            // 刷新当前属性页面
            OnTabBtnClicked(0);

            mIsResetPage = false;
        }

        // 重绘窗口
        Redraw();

        mGroupWnd[(int) BAGGAGE_PAGE.EQUIP_PAGE].GetComponent<EquipWnd>().RefreshEquipViewWnd();

        // 没有绑定宠物
        if (item_ob == null)
            return;

        // 注册宠物包裹格子变化回调
        (item_ob as Container).baggage.eventCarryChange += OnEquipChange;
    }

    /// <summary>
    /// 绑定页面
    /// </summary>
    /// <param name="page">Page.</param>
    public void BindPage(int page)
    {
        // 刷新当前属性页面
        OnTabBtnClicked(page);
    }

    /// <summary>
    /// 指引点击第二个分页
    /// </summary>
    public void OnTabBtnClicked(int page)
    {
        switch (page)
        {
            case 0:
                OnTabBtn0Clicked(mGroupTab[0]);
                break;

            case 1:
                OnTabBtn1Clicked(mGroupTab[1]);
                break;

            case 2:
                OnTabBtn2Clicked(mGroupTab[2]);
                break;

            case 3:
                OnTabBtn3Clicked(mGroupTab[3]);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// 指引点击装备
    /// </summary>
    public void GuideOnClickEquipItem(int index)
    {
        OnPetItemClick(mEquipItem[index]);
    }

    #endregion
}
