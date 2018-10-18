/// <summary>
/// OthersPetToolTipWnd.cs
/// Created by fensgc 2016/12/17
/// 宠物信息界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class OthersPetToolTipWnd : WindowBase<OthersPetToolTipWnd>
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

    public GameObject mShare;
    public UILabel mShareLb;

    // 绑定的宠物对象
    private Property item_ob = null;

    private string mSharePet = string.Empty;

    // 当前显示的玩家的属性
    private int curPage = 0;

    // 第一次打开时待窗口动画完成后再加载模型
    private bool isTweenOver = false;

    // 当前选择的装备Rid
    private string mSelectRid = string.Empty;

    // 宠物属性分页标签
    private const int Baggage_page1 = 0;
    private const int Baggage_page2 = 1;
    private const int Baggage_page3 = 2;
    private const int Baggage_page4 = 3;


    #endregion

    #region 内部函数

    void Start()
    {
        // 初始化界面
        InitWnd();

        // 注册事件
        RegisterEvent();
    }

    /// <summary>
    /// 显示模型
    /// </summary>
    void ShowModel()
    {
        // 获取窗口绑定的模型窗口组件
        ModelWnd pmc = mPetModel.GetComponent<ModelWnd>();

        // 没有绑定模型窗口组件
        if (pmc == null)
            return;

        if (item_ob == null)
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
        // 本地化文字
        msuitLb.text = LocalizationMgr.Get("PetToolTipWnd_1");
        mGroupTabLb[0].text = LocalizationMgr.Get("PetToolTipWnd_2");
        mGroupTabLb[1].text = LocalizationMgr.Get("PetToolTipWnd_4");
        mGroupTabLb[2].text = LocalizationMgr.Get("PetToolTipWnd_5");
        mShareLb.text = LocalizationMgr.Get("PetToolTipWnd_6");

        // 设置按钮选中状态
        SetSelect();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        for (int i = 0; i < mEquipItem.Length; i++)
            UIEventListener.Get(mEquipItem[i]).onClick += OnEquipItemClick;

        UIEventListener.Get(mGroupTab[0]).onClick += OnTabBtn0Clicked;
        UIEventListener.Get(mGroupTab[1]).onClick += OnTabBtn1Clicked;
        UIEventListener.Get(mGroupTab[2]).onClick += OnTabBtn2Clicked;

        GameObject baggaeWnd = WindowMgr.GetWindow("ViewUserWnd");

        if (baggaeWnd != null)
        {
            if (baggaeWnd.GetComponent<TweenAlpha>() != null)
                baggaeWnd.GetComponent<TweenAlpha>().AddOnFinished(OnTweenFinished);
        }
    }

    /// <summary>
    /// TweenAlpha结束的回调
    /// </summary>
    private void OnTweenFinished()
    {
        isTweenOver = true;
        ShowModel();
    }

    /// <summary>
    /// 装备格子点击事件
    /// </summary>
    void OnEquipItemClick(GameObject ob)
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

            GameObject bagWnd = WindowMgr.GetWindow(ViewUserWnd.WndType);

            if (bagWnd != null)
                wnd.transform.SetParent(bagWnd.transform);
        }

        wnd.SetActive(true);

        string petRid = string.Empty;

        if (item_ob != null)
            petRid = item_ob.GetRid();

        wnd.GetComponent<EquipViewWnd>().Bind(equipItem.GetRid(), petRid, new CallBack(OnCloseEquipView), false);
    }

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

        mShare.SetActive(false);

        if (item_ob == null)
        {
            UnloadModel();

            return;
        }

        if (item_ob.GetRid().Equals(mSharePet))
            mShare.SetActive(true);

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

            if (equipData != null && equipData.GetRid().Equals(mSelectRid))
                isSelectEquipDestory = false;
        }

        if (isSelectEquipDestory)
            ResetEquipSelect();

        // 刷新套装
        msuitName.text = PetMgr.GetPetSuitName(item_ob);
    }

    /// <summary>
    /// 设置属性页面刷新
    /// </summary>
    /// <param name="page">Page.</param>
    void RedrawCurPage(bool redrawEquip = true)
    {
        switch (curPage)
        {
            case Baggage_page1:
                mGroupWnd[0].GetComponent<AttributeWnd>().SetBind(item_ob, false);
                break;
            case Baggage_page2:
                mGroupWnd[1].GetComponent<SkillWnd>().SetBind(item_ob);
                break;
            case Baggage_page3:
                mGroupWnd[2].GetComponent<AwakeWnd>().BindData(item_ob, false);
                break;       
        }
    }

    /// <summary>
    /// 重置当前装备选中状态
    /// </summary>
    void ResetEquipSelect()
    {
        for (int i = 0; i < mEquipItem.Length; i++)
            mEquipItem[i].GetComponent<EquipItemWnd>().SetSelected(false);

        mSelectRid = string.Empty;
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
                lbTrans.localPosition = new Vector3(10, lbTrans.localPosition.y, lbTrans.localPosition.z);
                sp.width = 152;
            }
            else
            {
                sp.spriteName = "right_unselect";
                sp.alpha = 0.5f;
                lbTrans.localPosition = new Vector3(0, lbTrans.localPosition.y, lbTrans.localPosition.z);
                sp.width = 132;
            }
        }
    }

    /// <summary>
    /// 第一个分页
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnTabBtn0Clicked(GameObject ob)
    {
        if (curPage == Baggage_page1)
            return;

        mGroupWnd[curPage].SetActive(false);

        curPage = Baggage_page1;

        // 设置按钮选中状态
        SetSelect();

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
        if (curPage == Baggage_page2)
            return;

        mGroupWnd[curPage].SetActive(false);

        curPage = Baggage_page2;

        // 设置按钮选中状态
        SetSelect();

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
        if (curPage == Baggage_page3)
            return;

        mGroupWnd[curPage].SetActive(false);

        curPage = Baggage_page3;

        // 设置按钮选中状态
        SetSelect();

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
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 窗口绑定实体
    /// </summary>
    public void SetBind(Property ob, string sharePet)
    {
        // 重置绑定对象
        item_ob = ob;

        mSharePet = sharePet;

        // 重绘窗口
        Redraw();

        // 重绘子窗口
        RedrawCurPage();
    }

    #endregion
}
