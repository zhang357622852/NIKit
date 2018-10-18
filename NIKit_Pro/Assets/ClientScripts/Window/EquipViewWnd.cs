/// <summary>
/// EquipViewWnd.cs
/// Created by fengsc 2016/08/10
/// 装备查看
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class EquipViewWnd : WindowBase<EquipViewWnd>
{
    #region 成员变量

    // 强化等级
    public UILabel mStrengthenLevel;

    // 装备名称;
    public UILabel mEquipName;

    // 主属性;
    public UILabel mMainAttrib;

    // 词缀属性
    public UILabel mPrefixAttrib;

    // 附加属性
    public UILabel mAddAttrib;

    // 套装组件数量
    public UILabel mSuitSubCount;

    // 套装描述;
    public UILabel mSuitDesc;

    // 排序组件;
    public UIGrid mGrid;

    //强化按钮;
    public GameObject mStrengthenBtn;

    //装备按钮
    public GameObject mEquipBtn;

    //出售或卸下装备按钮
    public GameObject mSellOrUnLoadBtn;

    //关闭按钮
    public GameObject mCloseBtn;

    public GameObject mEquipItem;

    public UILabel mEquipLabel;

    public UILabel mSellOrUnLoadLabel;

    public UILabel mStrengthenLabel;

    // 分享按钮
    public GameObject mShareBtn;
    public UILabel mShareBtnLb;

    // 装备对象;
    [HideInInspector]
    public Property ob;

    private string mEquipRid;

    //当前宠物对象
    [HideInInspector]
    public Property mPetOb;

    private string mPetRid;

    // 预创建游戏物体缓存列表
    private List<GameObject> mCacheList = new List<GameObject>();

    // 关闭按钮悬浮回调
    private CallBack callBack;

    // 装备是否穿戴
    private bool mIsEquipd = false;

    // 窗口唯一标识
    private string instanceID = string.Empty;

    private bool mIsBtnActive = true;

    #endregion

    #region 内部函数

    void Awake()
    {
        instanceID = gameObject.GetInstanceID().ToString();

        CreateGameObject();
    }

    void OnEnable()
    {
        Redraw();

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    // Use this for initialization
    void Start()
    {
        RegisterEvent();

        InitLocalLabel();

        TweenScale mTweenScale = this.GetComponent<TweenScale>();

        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
        mTweenScale.AddOnFinished(OnTweenFinish);
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void OnDestroy()
    {
        ClearCacheList();

        Coroutine.DispatchService(RemoveDoneHook());
    }

    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    IEnumerator RemoveDoneHook()
    {
        // 等待一帧
        yield return null;

        // 解注册
        MsgMgr.RemoveDoneHook("MSG_SELL_ITEM", instanceID);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 关注msg_sell_equip消息
        MsgMgr.RegisterDoneHook("MSG_SELL_ITEM", instanceID, OnMsgSellEquip);

        if (mStrengthenBtn == null
            || mEquipBtn == null
            || mCloseBtn == null)
            return;

        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        if (!mIsBtnActive)
            return;

        UIEventListener.Get(mStrengthenBtn).onClick = OnClickStrtengthenBtn;

        if (ob == null)
            return;

        //获取装备可以装备的位置;
        string pos = EquipMgr.GetEquipPos(ob);

        //如果装备可以装备的位置与当前位置不相等，表示装备没有穿戴
        if (!pos.Equals(ob.Query<string>("pos")))
        {
            UIEventListener.Get(mSellOrUnLoadBtn).onClick = OnClickSellBtn;

            mIsEquipd = false;

            UIEventListener.Get(mEquipBtn).onClick = OnClickEquipBtn;

            mEquipBtn.SetActive(true);

            mSellOrUnLoadLabel.text = LocalizationMgr.Get("EquipViewWnd_4");
        }
        else
        {
            UIEventListener.Get(mSellOrUnLoadBtn).onClick = OnClickUnLoadBtn;
            mEquipBtn.SetActive(false);

            mSellOrUnLoadLabel.text = LocalizationMgr.Get("EquipViewWnd_5");

            mIsEquipd = true;
        }
    }

    /// <summary>
    /// 装备出售成功的回调
    /// </summary>
    void OnMsgSellEquip(string cmd, LPCValue para)
    {
        if (this == null)
            return;

        if (gameObject == null || !gameObject.activeInHierarchy)
            return;

        if (!gameObject.name.Contains("UnEquip"))
            return;

        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocalLabel()
    {
        if (mStrengthenLabel != null)
            mStrengthenLabel.text = LocalizationMgr.Get("EquipViewWnd_2");

        if (mShareBtnLb != null)
            mShareBtnLb.text = LocalizationMgr.Get("EquipViewWnd_11");
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 获取装备对象
        ob = Rid.FindObjectByRid(mEquipRid);

        // 查找宠物对象
        mPetOb = Rid.FindObjectByRid(mPetRid);

        if (ob == null)
            return;

        // 调整窗口
        if (mEquipLabel != null && mPetOb != null)
        {
            int equipType = EquipMgr.GetEquipType(ob.GetClassID());
            Property targetOb = MonsterMgr.GetPetByEquipPos(mPetOb, EquipMgr.GetEquipPos(equipType));
            if (targetOb == null)
                mEquipLabel.text = LocalizationMgr.Get("EquipViewWnd_3");
            else
                mEquipLabel.text = LocalizationMgr.Get("EquipViewWnd_7");
        }

        foreach (GameObject go in mCacheList)
            go.GetComponent<UILabel>().text = string.Empty;

        mEquipItem.GetComponent<EquipItemWnd>().SetBind(ob);

        //获取颜色标签;
        string colorLabel = ColorConfig.GetColor(ob.GetRarity());

        //获取装备的属性
        LPCMapping equipAttrib = ob.Query<LPCMapping>("prop");

        if (equipAttrib == null)
            return;

        int rank = ob.Query<int>("rank");

        // 未强化(level = 0)不显示等级
        if (rank > 0)
        {
            mStrengthenLevel.text = string.Format("[{0}]+{1}[-]", colorLabel, rank);

            mStrengthenLevel.gameObject.SetActive(true);
        }
        else
            mStrengthenLevel.text = string.Empty;

        //套装Id
        int suitId = ob.Query<int>("suit_id");

        //获取套装配置表数据
        CsvFile csv = EquipMgr.SuitTemplateCsv;

        if (csv == null)
        {
            LogMgr.Trace("没有获取到套装配置信息");
            return;
        }

        CsvRow row = csv.FindByKey(suitId);

        if (row == null)
        {
            LogMgr.Trace("没有获取到指定行的套装配置信息");
            return;
        }

        LPCArray props = row.Query<LPCArray>("props");

        if (props == null)
        {
            LogMgr.Trace("没有获取到套装附加属性数据");
            return;
        }

        mSuitSubCount.text = string.Format("{0}{1}", row.Query<int>("sub_count"), LocalizationMgr.Get("EquipViewWnd_1"));

        string suitDesc = string.Empty;

        //获取套装描述信息;
        foreach (LPCValue item in props.Values)
            suitDesc += PropMgr.GetPropDesc(item.AsArray, EquipConst.SUIT_PROP);

        mSuitDesc.text = suitDesc;

        //获取装备的主属性;
        LPCArray mainAttrib = equipAttrib.GetValue<LPCArray>(EquipConst.MAIN_PROP);

        //获取装备的词缀属性;
        LPCArray prefixAttrib = equipAttrib.GetValue<LPCArray>(EquipConst.PREFIX_PROP);

        //获取次要属性;
        LPCArray minorAttrib = equipAttrib.GetValue<LPCArray>(EquipConst.MINOR_PROP);

        string Desc = string.Empty;

        if (mainAttrib != null)
        {
            Desc = string.Empty;

            //获取主属性属性的描述;
            foreach (LPCValue item in mainAttrib.Values)
                Desc += PropMgr.GetPropDesc(item.AsArray, EquipConst.MAIN_PROP);

            mMainAttrib.text = Desc;
            mMainAttrib.gameObject.SetActive(true);
        }

        //获取装备短描述;
        string shortDesc = ob.Short();

        //有词缀属性
        if (prefixAttrib != null)
        {
            Desc = string.Empty;

            foreach (LPCValue item in prefixAttrib.Values)
                Desc += PropMgr.GetPropDesc(item.AsArray, EquipConst.PREFIX_PROP);

            mPrefixAttrib.text = Desc;
            mPrefixAttrib.gameObject.SetActive(true);

            mEquipName.text = string.Format("[{0}]{1}[-]", colorLabel, shortDesc);
        }
        else
        {
            mEquipName.text = string.Format("[{0}]{1}[-]", colorLabel, shortDesc);
            mPrefixAttrib.text = string.Empty;
        }

        //没有附加属性;
        if (minorAttrib == null)
        {
            mAddAttrib.text = string.Empty;
            return;
        }

        Desc = string.Empty;

        int index = 0;

        foreach (LPCValue item in minorAttrib.Values)
        {
            GameObject clone = mCacheList[index];

            UILabel label = clone.GetComponent<UILabel>();

            label.text = PropMgr.GetPropDesc(item.AsArray, EquipConst.MINOR_PROP);

            index++;
        }
        mGrid.repositionNow = true;
    }

    /// <summary>
    ///  绘制窗口前先创建一批gameobject缓存
    /// </summary>
    void CreateGameObject()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject clone = Instantiate(mAddAttrib.gameObject) as GameObject;

            clone.gameObject.SetActive(true);

            clone.transform.SetParent(mGrid.transform);

            clone.transform.localScale = Vector3.one;

            clone.transform.localPosition = Vector3.zero;

            clone.SetActive(true);

            clone.GetComponent<UILabel>().text = string.Empty;

            // 将创建的列表添加到列表中
            mCacheList.Add(clone);
        }
    }

    /// <summary>
    ///  清理缓存列表
    /// </summary>
    void ClearCacheList()
    {
        foreach (GameObject go in mCacheList)
            Destroy(go);

        mCacheList.Clear();
    }

    /// <summary>
    /// 强化按钮点击事件
    /// </summary>
    void OnClickStrtengthenBtn(GameObject go)
    {
        // 执行强化操作
        DoStrengthen();
    }

    /// <summary>
    /// 执行强化操作
    /// </summary>
    void DoStrengthen()
    {
        GameObject wnd = WindowMgr.OpenWnd(EquipStrengthenWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        //创建窗口失败;
        if (wnd == null)
            return;

        GameObject mainWnd = WindowMgr.GetWindow(MainWnd.WndType);

        if (mainWnd != null)
            WindowMgr.HideWindow(mainWnd);

        wnd.GetComponent<EquipStrengthenWnd>().Bind(ob.GetRid());

        // 隐藏装备信息窗口
        GameObject equipWnd = WindowMgr.GetWindow("EquipViewWnd_Equip");

        if (equipWnd != null)
            WindowMgr.HideWindow(equipWnd);

        GameObject unWnd = WindowMgr.GetWindow("EquipViewWnd_UnEquip");
        if (unWnd != null)
            WindowMgr.HideWindow(unWnd);
    }

    /// <summary>
    /// 装备按钮点击事件
    /// </summary>
    void OnClickEquipBtn(GameObject go)
    {
        // 执行装备操作
        DoEquip();
    }

    void DoEquip()
    {
        if (mPetOb == null || ob == null)
            return;

        // 获取该装备equip_type
        int equipType = EquipMgr.GetEquipType(ob.GetClassID());

        // 有装备要给出提示
        if (EquipMgr.IsEquippedPos(mPetOb, EquipMgr.GetEquipPos(equipType)))
            DialogMgr.ShowDailog(new CallBack(OnClickDiloagOkCallBack), LocalizationMgr.Get("EquipViewWnd_6"));
        else
        {
            // 装备道具
            EquipMgr.Equip(mPetOb, ob);

            if (! GuideMgr.IsGuiding())
                WindowMgr.DestroyWindow(gameObject.name);
        }
    }

    void OnClickDiloagOkCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        if (this == null || mPetOb == null || ob == null)
            return;

        //装备道具
        EquipMgr.Equip(mPetOb, ob);

        if (! GuideMgr.IsGuiding())
            WindowMgr.DestroyWindow(gameObject.name);
    }

    void ClickSecondDialogCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        //装备道具
        if (EquipMgr.Equip(mPetOb, ob) && ! GuideMgr.IsGuiding())
            WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 卸下装备按钮
    /// </summary>
    void OnClickUnLoadBtn(GameObject go)
    {
        if (mPetOb == null || ob == null || ME.user == null)
            return;

        // 卸下装备，关闭当前窗口
        if (EquipMgr.UnEquip(ME.user, ob))
            WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 装备脱下二次弹框
    /// </summary>
    void UnLoadSecondDialogCallBack(object para, params object[] param)
    {
        // 打开快捷购买窗口
        GameObject go = WindowMgr.OpenWnd(QuickMarketWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (go == null)
            return;

        go.GetComponent<QuickMarketWnd>().Bind(ShopConfig.MONEY_GROUP);
    }

    /// <summary>
    /// 出售按钮点击事件
    /// </summary>
    void OnClickSellBtn(GameObject go)
    {
        if (ob == null)
            return;

        LPCMapping sellPrice = PropertyMgr.GetSellPrice(ob);
        if (sellPrice == null || sellPrice.Count == 0)
            return;

        string fields = FieldsMgr.GetFieldInMapping(sellPrice);

        string iconDesc = string.Format(LocalizationMgr.Get("EquipViewWnd_8"), FieldsMgr.GetFieldIcon(fields), 46, 46);

        string desc = string.Format(LocalizationMgr.Get("MultipleSellWnd_10"), sellPrice.GetValue<int>(fields), iconDesc);

        DialogMgr.ShowDailog(new CallBack(SellDialogOk), desc, string.Empty, LocalizationMgr.Get("MultipleSellWnd_11"));
    }

    /// <summary>
    /// 装备出售确认弹框
    /// </summary>
    void SellDialogOk(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        if (ob == null)
            return;

        //构建参数;
        LPCMapping exPara = new LPCMapping();

        LPCMapping itemData = new LPCMapping();

        itemData.Add(string.IsNullOrEmpty(ob.Query<string>("original_rid")) ? ob.GetRid() : ob.Query<string>("original_rid"), 1);

        exPara.Add("rid", itemData);

        // 通知服务器出售装备
        Operation.CmdSellItem.Go(exPara);
    }

    /// <summary>
    /// 关闭按钮
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        if (callBack != null)
            callBack.Go();

        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 设置按钮的状态
    /// </summary>
    void SetBtnState(bool isActive)
    {
        Transform equipTran = mEquipBtn.transform.Find("maskSp");
        if (equipTran != null)
            equipTran.gameObject.SetActive(isActive);

        Transform strengthenTran = mStrengthenBtn.transform.Find("maskSp");
        if (strengthenTran != null)
            strengthenTran.gameObject.SetActive(isActive);

        Transform sellOrLoadTran = mSellOrUnLoadBtn.transform.Find("maskSp");
        if (sellOrLoadTran != null)
            sellOrLoadTran.gameObject.SetActive(isActive);
    }

    /// <summary>
    /// 分享按钮点击事件
    /// </summary>
    void OnClickShareBtn(GameObject go)
    {
        if (ob == null)
            return;

        // 打开聊天界面
        GameObject wnd = WindowMgr.OpenWnd(ChatWnd.WndType);

        if (wnd == null)
        {
            LogMgr.Trace("ChatWnd窗口创建失败");
            return;
        }

        string wndName = string.Empty;

        // 不在副本中才打开主城界面
        if (!InstanceMgr.IsInInstance(ME.user))
            wndName = MainWnd.WndType;

        // 绑定数据
        wnd.GetComponent<ChatWnd>().BindPublish(ob);
        wnd.GetComponent<ChatWnd>().Bind(wndName, null);

        // 关闭包裹界面
        WindowMgr.HideWindow(BaggageWnd.WndType);
    }

    #endregion

    #region 外部接口

    //刷新数据;
    public void Bind(string equipRid, string petRid, CallBack _callBack = null, bool isShare = true)
    {
        mEquipRid = equipRid;

        mPetRid = petRid;

        callBack = _callBack;

        if (mShareBtn != null)
        {
            if (isShare)
            {
                mShareBtn.SetActive(true);
                mStrengthenBtn.SetActive(true);
                mSellOrUnLoadBtn.SetActive(true);
                UIEventListener.Get(mShareBtn).onClick = OnClickShareBtn;
            }
            else
            {
                mShareBtn.SetActive(false);
                mStrengthenBtn.SetActive(false);
                mSellOrUnLoadBtn.SetActive(false);
            }
        }

        Redraw();
    }

    public void SetBtnActive(bool active)
    {
        mIsBtnActive = active;

        mShareBtn.SetActive(active);

        mStrengthenBtn.SetActive(active);

        mEquipBtn.SetActive(active);

        mSellOrUnLoadBtn.SetActive(active);
    }

    /// <summary>
    /// 悬浮窗口的状态
    /// </summary>
    public void SetState(int state)
    {
        if (state == 0)
        {
            UIEventListener.Get(mEquipBtn).onClick -= OnClickEquipBtn;

            UIEventListener.Get(mStrengthenBtn).onClick -= OnClickStrtengthenBtn;

            if (!mIsEquipd)
                UIEventListener.Get(mSellOrUnLoadBtn).onClick -= OnClickSellBtn;
            else
                UIEventListener.Get(mSellOrUnLoadBtn).onClick -= OnClickUnLoadBtn;

            SetBtnState(true);
        }
        else
        {
            UIEventListener.Get(mEquipBtn).onClick = OnClickEquipBtn;

            UIEventListener.Get(mStrengthenBtn).onClick = OnClickStrtengthenBtn;

            if (!mIsEquipd)
                UIEventListener.Get(mSellOrUnLoadBtn).onClick = OnClickSellBtn;
            else
                UIEventListener.Get(mSellOrUnLoadBtn).onClick = OnClickUnLoadBtn;

            SetBtnState(false);
        }
    }

    /// <summary>
    /// 显示查看
    /// </summary>
    public void ShowView(string equipRid, string petRid)
    {
        mEquipRid = equipRid;

        mPetRid = petRid;

        TweenAlpha alpha = gameObject.GetComponent<TweenAlpha>();

        alpha.from = 0;

        alpha.to = 1;

        alpha.enabled = true;

        alpha.ResetToBeginning();

        if (mShareBtn != null)
            mShareBtn.SetActive(false);

        //绘制窗口;
        Redraw();
    }

    public void HideView()
    {
        TweenAlpha alpha = gameObject.GetComponent<TweenAlpha>();

        alpha.from = 1;

        alpha.to = 0;

        alpha.enabled = true;

        alpha.ResetToBeginning();

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 指引点击装备按钮
    /// </summary>
    public void GuideOnClickEquipBtn()
    {
        // 执行装备操作
        DoEquip();
    }

    /// <summary>
    /// 指引点击强化按钮
    /// </summary>
    public void GuideOnClickStrengthBtn()
    {
        // 执行强化操作
        DoStrengthen();
    }

    #endregion
}
