/// EquipBatchSellWnd.cs
/// Created by zhangwm 2018/08/21
/// 装备批量出售窗口
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class EquipBatchSellWnd : WindowBase<EquipBatchSellWnd>
{
    #region 成员变量

    // 窗口标题
    public UILabel mTitle;
    public GameObject mCloseBtn;
    public TweenScale mTweenScale;
    public GameObject mToggleSpritePrefab;
    public GameObject mToggleLabelPrefab;

    public ToggleLabelCtrl mSellIntensifyCtrl;

    // ****套装选择****
    public UILabel mSuitTitle;
    public GameObject mSuitItemGo;
    public UIScrollView mSuitSV;
    public UIGrid mSuitGrid;
    public ToggleLabelCtrl mSuitAllSelectCtrl;

    // ****星级选择*****
    public UIGrid mStarGrid;
    public ToggleLabelCtrl mStarAllSelectCtrl;

    // ****部位选择*****
    public UIGrid mPlaceGrid;
    public ToggleLabelCtrl mPlaceAllSelectCtrl;

    // ****品质选择*****
    public UIGrid mRarityGrid;
    public ToggleLabelCtrl mRarityAllSelectCtrl;

    // ****饰品主属性选择*****
    public UILabel mAccTitle;
    public UIScrollView mAccSV;
    public UIGrid mAccGrid;
    public UILabel mAccNone;
    public ToggleLabelCtrl mAccAllSelectCtrl;
    public UIProgressBar mAccProgressBar;

    public GameObject mSellBtn;
    public GameObject mSellBtnMask;
    public UILabel mSellBtnLabel;

    // 记录缓存套装item
    private List<EquipSuitItemWnd> mCacheSuitItemList = new List<EquipSuitItemWnd>();

    private List<ToggleSpriteCtrl> mCacheStarItemList = new List<ToggleSpriteCtrl>();

    private List<ToggleLabelCtrl> mCachePlaceItemList = new List<ToggleLabelCtrl>();

    private List<ToggleLabelCtrl> mCacheRarityItemList = new List<ToggleLabelCtrl>();

    private List<ToggleLabelCtrl> mCacheAccItemList = new List<ToggleLabelCtrl>();

    // 星级列表
    private List<int> mStarList = new List<int>() { EquipConst.STAR_1, EquipConst.STAR_2, EquipConst.STAR_3, EquipConst.STAR_4, EquipConst.STAR_5, EquipConst.STAR_6 };

    // 部位列表
    private List<int> mPlaceList = new List<int>(){EquipConst.WEAPON, EquipConst.ARMOR, EquipConst.SHOES, EquipConst.AMULET, EquipConst.NECKLACE, EquipConst.RING };

    // 品质列表
    private List<int> mRarityList = new List<int>() { EquipConst.RARITY_WHITE, EquipConst.RARITY_GREEN, EquipConst.RARITY_BLUE, EquipConst.RARITY_PURPLE, EquipConst.RARITY_DARKGOLDENROD };

    // 属性列表 效果抵抗+% (项链) 效果命中+% (项链) 暴击率+% (戒指) 暴击伤害+% (戒指) 敏捷+ (护符)
    private List<int> mAccList = new List<int>()
    {
        EquipConst.ATTACK, EquipConst.DEFENSE, EquipConst.MAX_HP,
        EquipConst.ACCURACY_RATE, EquipConst.RESIST_RATE, EquipConst.ATTACK_RATE, EquipConst.DEFENSE_RATE, EquipConst.MAX_HP_RATE,
        EquipConst.CRT_RATE, EquipConst.CRT_DMG_RATE, EquipConst.AGILITY,
    };

    // 当前选中的套装id列表 int->suit_id
    private LPCValue mCurSelectSuitArray;

    // 当前选中的星级列表 int->star
    private LPCValue mCurSelectStarArray;

    // 当前选中的部位列表 int->place
    private LPCValue mCurSelectPlaceArray;

    // 当前选中的品质列表 int->rarity
    private LPCValue mCurSelectRarityArray;

    // 当前选中的饰品属性列表 int->prop
    private LPCValue mCurSelectAccArray;

    // 筛选后有效装备
    private List<Property> mValidEquipList;

    // 默认出售强化的装备
    private int mSellIntensifyEquip = 1;

    private bool isClick = false;

    #endregion

    private void Start()
    {
        InitText();

        RegisterEvent();

        InitInfo();

        Redraw();
    }

    private void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        // 存储临时记录套装选择
        SaveSelectRecord(mCurSelectSuitArray, "equip_batch_sell/suit");

        // 存储临时记录星级选择
        SaveSelectRecord(mCurSelectStarArray, "equip_batch_sell/star");

        // 存储临时记录部位选择
        SaveSelectRecord(mCurSelectPlaceArray, "equip_batch_sell/place");

        // 存储临时记录品质选择
        SaveSelectRecord(mCurSelectRarityArray, "equip_batch_sell/rarity");

        // 存储临时记录饰品属性选择
        SaveSelectRecord(mCurSelectAccArray, "equip_batch_sell/acc");

        SaveSelectRecord(LPCValue.Create(mSellIntensifyEquip), "equip_batch_sell/sell_intensify");

        // 开启协程
        Coroutine.DispatchService(RemoveMsg());
    }

    private IEnumerator RemoveMsg()
    {
        // 等待一帧
        yield return null;

        // 解注册
        MsgMgr.RemoveDoneHook("MSG_SELL_ITEM", "EquipBatchSellWnd");
    }

    /// <summary>
    /// 存储选择
    /// </summary>
    /// <param name="value"></param>
    /// <param name="path"></param>
    private void SaveSelectRecord(LPCValue value, string path)
    {
        if (value != null && ME.user != null)
            ME.user.SetTemp(path, value);
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    private void InitText()
    {
        mTitle.text = LocalizationMgr.Get("EquipBatchSellWnd_1");

        mSellIntensifyCtrl.SetDes("EquipBatchSellWnd_13");

        mSuitTitle.text = LocalizationMgr.Get("EquipBatchSellWnd_3");
        mSuitAllSelectCtrl.SetDes("EquipBatchSellWnd_2");

        mStarAllSelectCtrl.SetDes("EquipBatchSellWnd_2");

        mPlaceAllSelectCtrl.SetDes("EquipBatchSellWnd_2");

        mRarityAllSelectCtrl.SetDes("EquipBatchSellWnd_2");

        mAccTitle.text = LocalizationMgr.Get("EquipBatchSellWnd_7");
        mAccAllSelectCtrl.SetDes("EquipBatchSellWnd_2");
        mAccNone.text = LocalizationMgr.Get("EquipBatchSellWnd_9");
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    private void InitInfo()
    {
        if (ME.user == null)
            return;

        LPCValue v = ME.user.QueryTemp<LPCValue>("equip_batch_sell/sell_intensify");
        if (v != null && v.IsInt)
            mSellIntensifyEquip = v.AsInt;

        mSellIntensifyCtrl.SetStateNoTrigger(mSellIntensifyEquip == 1 ? false : true);

        // **** 套装 ****

        // 获取临时记录套装选择
        mCurSelectSuitArray = ME.user.QueryTemp<LPCValue>("equip_batch_sell/suit");

        if (mCurSelectSuitArray == null)
            mCurSelectSuitArray = LPCValue.CreateArray();

        // 初始化套装选择全选状态
        mSuitAllSelectCtrl.SetStateNoTrigger(mCurSelectSuitArray.AsArray.Count >= EquipMgr.SuitTemplateCsv.count ? true : false);

        // **** 星级 ****

        // 获取临时记录星级选择
        mCurSelectStarArray = ME.user.QueryTemp<LPCValue>("equip_batch_sell/star");

        if (mCurSelectStarArray == null)
            mCurSelectStarArray = LPCValue.CreateArray();

        // 初始化星级选择全选状态
        mStarAllSelectCtrl.SetStateNoTrigger(mCurSelectStarArray.AsArray.Count >= 6 ? true : false);

        // 如果没有记录选择的话，就默认选择1星级
        if (mCurSelectStarArray.AsArray.Count <= 0)
            mCurSelectStarArray.AsArray.Add(EquipConst.STAR_1);

        // **** 部位 ****

        // 获取临时记录部位选择
        mCurSelectPlaceArray = ME.user.QueryTemp<LPCValue>("equip_batch_sell/place");

        if (mCurSelectPlaceArray == null)
            mCurSelectPlaceArray = LPCValue.CreateArray();

        // 初始化部位选择全选状态
        mPlaceAllSelectCtrl.SetStateNoTrigger(mCurSelectPlaceArray.AsArray.Count >= mPlaceList.Count ? true : false);

        // **** 品质 ****

        // 获取临时记录品质选择
        mCurSelectRarityArray = ME.user.QueryTemp<LPCValue>("equip_batch_sell/rarity");

        if (mCurSelectRarityArray == null)
            mCurSelectRarityArray = LPCValue.CreateArray();

        // 初始化品质选择全选状态
        mRarityAllSelectCtrl.SetStateNoTrigger(mCurSelectRarityArray.AsArray.Count >= mRarityList.Count ? true : false);

        // 如果没有记录选择的话，就默认选择普通
        if (mCurSelectRarityArray.AsArray.Count <= 0)
            mCurSelectRarityArray.AsArray.Add(EquipConst.RARITY_WHITE);

        // **** 饰品属性 ****

        // 获取临时记录饰品属性选择
        mCurSelectAccArray = ME.user.QueryTemp<LPCValue>("equip_batch_sell/acc");

        if (mCurSelectAccArray == null)
            mCurSelectAccArray = LPCValue.CreateArray();

        // 初始化饰品属性选择全选状态 这个顺序需要在读取记录部位选择之后
        mAccAllSelectCtrl.SetStateNoTrigger(mCurSelectAccArray.AsArray.Count >= GetValidProps().Count ? true : false);

        // 初始化所有饰品主属性
        GameObject go;

        mToggleLabelPrefab.SetActive(false);
        for (int i = 0; i < mAccList.Count; i++)
        {
            go = NGUITools.AddChild(mAccGrid.gameObject, mToggleLabelPrefab);
            go.name = mAccList[i].ToString();
            ToggleLabelCtrl ctrl = go.GetComponent<ToggleLabelCtrl>();

            ctrl.BindData(mAccList[i]);
            ctrl.Callback = ClickAccToggle;
            ctrl.SetDes(LocalizationMgr.Get("EquipBatchSellWnd_Prop_" + mAccList[i]));
            ctrl.SetDesColor("F9E2B8");
            ctrl.gameObject.AddComponent<UIDragScrollView>();

            mCacheAccItemList.Add(ctrl);
        }
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;

        UIEventListener.Get(mSellBtn).onClick = OnClickSellBtn;

        mSellIntensifyCtrl.Callback = ClickShowIntensify;

        // 绑定套装选择全选事件
        mSuitAllSelectCtrl.Callback = ClickSuitAllSelect;

        // 绑定星级选择全选事件
        mStarAllSelectCtrl.Callback = ClickStarAllSelect;

        // 绑定部位选择全选事件
        mPlaceAllSelectCtrl.Callback = ClickPlaceAllSelect;

        // 绑定品质选择全选事件
        mRarityAllSelectCtrl.Callback = ClickRarityAllSelect;

        // 绑定饰品主属性选择全选事件
        mAccAllSelectCtrl.Callback = ClickAccAllSelect;

        // 关注msg_sell_equip消息
        MsgMgr.RegisterDoneHook("MSG_SELL_ITEM", "EquipBatchSellWnd", OnMsgSellEquip);

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    private void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    private void Redraw()
    {
        RefreshSuitInfo();

        RefreshStarInfo();

        RefreshPlaceInfo();

        RefreshRarityInfo();

        RefreshAccInfo();

        RefreshSellState();
    }

    /// <summary>
    /// 装备出售成功消息回调
    /// </summary>
    /// <param name="cmd">Cmd.</param>
    /// <param name="para">Para.</param>
    private void OnMsgSellEquip(string cmd, LPCValue para)
    {
        // 出售装备的属性奖励
        LPCMapping paraData = para.AsMapping.GetValue<LPCMapping>("para");

        LPCMapping attribBonus = paraData.GetValue<LPCMapping>("attrib_bonus");

        string fields = FieldsMgr.GetFieldInMapping(attribBonus);

        string iconDesc = string.Format(LocalizationMgr.Get("EquipViewWnd_8"), FieldsMgr.GetFieldIcon(fields), 46, 46);

        string desc = string.Format(LocalizationMgr.Get("EquipBatchSellWnd_12"), attribBonus.GetValue<int>(fields), iconDesc);

        DialogMgr.ShowSingleBtnDailog(
            new CallBack(SellSucSureBtn),
            desc,
            LocalizationMgr.Get("EquipBatchSellWnd_11"),
            string.Empty,
            true,
            this.transform
        );

        RefreshSuitInfo();

        RefreshSellState();

        isClick = false;
    }

    private void SellSucSureBtn(object para, params object[] param)
    {
    }

    private void RefreshSuitInfo()
    {
        // 获取玩家分类装备列表
        Dictionary<int, List<Property>> equipMap = BaggageMgr.GetSuitByPage(ME.user, ContainerConfig.POS_ITEM_GROUP);

        // 绑定绘制各个套装信息
        CsvFile tempCsvFile = EquipMgr.SuitTemplateCsv;

        LPCArray curSelectSuit = mCurSelectSuitArray.AsArray;

        mCacheSuitItemList.Clear();

        List<Property> mEquipList;
        mSuitItemGo.SetActive(true);
        int suitId;
        Transform tran;
        EquipSuitItemWnd item;

        foreach (CsvRow data in tempCsvFile.rows)
        {
            suitId = data.Query<int>("suit_id");

            tran = mSuitGrid.transform.Find(suitId.ToString());

            if (tran == null)
            {
                tran = NGUITools.AddChild(mSuitGrid.gameObject, mSuitItemGo).transform;
                tran.name = suitId.ToString();
            }

            item = tran.GetComponent<EquipSuitItemWnd>();

            if (!equipMap.TryGetValue(suitId, out mEquipList))
                mEquipList = new List<Property>();

            // 绑定重绘窗口的数据;
            item.BindData(data, mEquipList);

            // 添加回调事件;
            item.CallBack = ClickSuitItem;

            // 设置选择状态
            item.SetSelectNoTrriger(curSelectSuit.IndexOf(suitId) >= 0 ? true : false);

            mCacheSuitItemList.Add(item);
        }
        mSuitItemGo.SetActive(false);
        mSuitGrid.Reposition();
        mSuitSV.ResetPosition();
    }

    /// <summary>
    /// 刷新星级信息
    /// </summary>
    private void RefreshStarInfo()
    {
        LPCArray curSelectStar = mCurSelectStarArray.AsArray;

        GameObject go;

        mToggleSpritePrefab.SetActive(true);
        for (int i = 0; i < mStarList.Count; i++)
        {
            go = NGUITools.AddChild(mStarGrid.gameObject, mToggleSpritePrefab);
            go.name = mStarList[i].ToString();
            ToggleSpriteCtrl ctrl = go.GetComponent<ToggleSpriteCtrl>();

            ctrl.BindData(mStarList[i]);
            ctrl.SetStateNoTrigger(curSelectStar.IndexOf(mStarList[i]) >= 0);
            ctrl.Callback = ClickStarToggle;

            mCacheStarItemList.Add(ctrl);
        }
        mToggleSpritePrefab.SetActive(false);
        mStarGrid.Reposition();
    }

    /// <summary>
    /// 刷新部位信息
    /// </summary>
    private void RefreshPlaceInfo()
    {
        LPCArray curSelectPlace = mCurSelectPlaceArray.AsArray;

        GameObject go;

        mToggleLabelPrefab.SetActive(true);
        for (int i = 0; i < mPlaceList.Count; i++)
        {
            go = NGUITools.AddChild(mPlaceGrid.gameObject, mToggleLabelPrefab);
            go.name = mPlaceList[i].ToString();
            ToggleLabelCtrl ctrl = go.GetComponent<ToggleLabelCtrl>();

            ctrl.BindData(mPlaceList[i]);
            ctrl.Callback = ClickPlaceToggle;
            ctrl.SetDes(EquipMgr.GetEquipTypeNameByEquipType(mPlaceList[i]));
            ctrl.SetDesColor("F9E2B8");
            ctrl.SetStateNoTrigger(curSelectPlace.IndexOf(mPlaceList[i]) >= 0);

            mCachePlaceItemList.Add(ctrl);
        }
        mToggleLabelPrefab.SetActive(false);
        mPlaceGrid.Reposition();
    }

    /// <summary>
    /// 刷新品质信息
    /// </summary>
    private void RefreshRarityInfo()
    {
        LPCArray curSelectRarity = mCurSelectRarityArray.AsArray;

        GameObject go;

        mToggleLabelPrefab.SetActive(true);
        for (int i = 0; i < mRarityList.Count; i++)
        {
            go = NGUITools.AddChild(mRarityGrid.gameObject, mToggleLabelPrefab);
            go.name = mRarityList[i].ToString();
            ToggleLabelCtrl ctrl = go.GetComponent<ToggleLabelCtrl>();

            ctrl.BindData(mRarityList[i]);
            ctrl.Callback = ClickRarityToggle;
            ctrl.SetDes(EquipMgr.GetRarityAlias(mRarityList[i]));
            ctrl.SetDesColor(EquipMgr.GetEquipRarityColorByRarity(mRarityList[i]));
            // 这个需要放在设置文本颜色后，未选择中的文本透明度需要改变
            ctrl.SetStateNoTrigger(curSelectRarity.IndexOf(mRarityList[i]) >= 0);

            mCacheRarityItemList.Add(ctrl);
        }
        mToggleLabelPrefab.SetActive(false);
        mRarityGrid.Reposition();
    }

    /// <summary>
    /// 刷新饰品主属性
    /// </summary>
    private void RefreshAccInfo()
    {
        List<int> validProps = GetValidProps();

        if (validProps.Count < 1)
        {
            mAccSV.ResetPosition();
            mAccProgressBar.alpha = 0f;
        }
        else
            mAccProgressBar.alpha = 1f;

        LPCArray curSelectAcc = mCurSelectAccArray.AsArray;;

        int prop;

        for (int i = 0; i < mCacheAccItemList.Count; i++)
        {
            prop = int.Parse(mCacheAccItemList[i].transform.name);

            if (validProps.Contains(prop))
            {
                mCacheAccItemList[i].gameObject.SetActive(true);

                // 如果是全选，还没有被记录的话，加入记录里面
                if (mAccAllSelectCtrl.IsOn && curSelectAcc.IndexOf(prop) == -1)
                    curSelectAcc.Add(prop);

                mCacheAccItemList[i].SetStateNoTrigger(curSelectAcc.IndexOf(prop) >= 0);
            }
            else
            {
                mCacheAccItemList[i].SetStateNoTrigger(false);
                mCacheAccItemList[i].gameObject.SetActive(false);

                if (curSelectAcc.IndexOf(prop) >= 0)
                    curSelectAcc.Remove(prop);
            }
        }

        if (validProps.Count < 1)
        {
            mAccSV.gameObject.SetActive(false);
            mAccNone.gameObject.SetActive(true);

            mAccAllSelectCtrl.GetComponent<BoxCollider>().enabled = false;
            mAccAllSelectCtrl.SetStateNoTrigger(false);
        }
        else
        {
            mAccSV.gameObject.SetActive(true);
            mAccNone.gameObject.SetActive(false);

            mAccGrid.Reposition();

            mAccAllSelectCtrl.GetComponent<BoxCollider>().enabled = true;
        }
    }

    /// <summary>
    /// 刷新出售按钮状态
    /// </summary>
    private void RefreshSellState()
    {
        mValidEquipList = GetSellEquipAmount();

        mSellBtnMask.SetActive(mValidEquipList.Count <= 0);

        mSellBtnLabel.text = string.Format(LocalizationMgr.Get("EquipBatchSellWnd_8"), mValidEquipList.Count);
    }

    /// <summary>
    /// 获取筛选后或出售的装备列表
    /// </summary>
    /// <returns></returns>
    private List<Property> GetSellEquipAmount()
    {
        // 获取玩家包裹的装备数据
        List<Property> allEquipList = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_ITEM_GROUP);

        // 符合条件有效装备列表
        List<Property> validEquipList = new List<Property>();

        // 装备属性
        LPCMapping equipAttrib;

        // 主属性
        LPCArray mainAttrib;

        bool isExistProp = false;

        // 是否选择饰品
        bool isSelectAcc = false;

        if (mCurSelectPlaceArray.AsArray.IndexOf(EquipConst.AMULET) >= 0 || mCurSelectPlaceArray.AsArray.IndexOf(EquipConst.NECKLACE) >= 0 || mCurSelectPlaceArray.AsArray.IndexOf(EquipConst.RING) >= 0)
            isSelectAcc = true;

        foreach (Property equipOb in allEquipList)
        {
            // 不是装备
            if (!EquipMgr.IsEquipment(equipOb))
                continue;

            // 套装筛选
            if (mCurSelectSuitArray.AsArray.IndexOf(equipOb.Query<int>("suit_id")) < 0)
                continue;

            // 星级筛选
            if (mCurSelectStarArray.AsArray.IndexOf(equipOb.Query<int>("star")) < 0)
                continue;

            // 部位筛选
            if (mCurSelectPlaceArray.AsArray.IndexOf(equipOb.Query<int>("equip_type")) < 0)
                continue;

            // 品质筛选
            if (mCurSelectRarityArray.AsArray.IndexOf(equipOb.GetRarity()) < 0)
                continue;

            // 筛选强化的装备
            if (equipOb.GetRank() > 0 && mSellIntensifyEquip == 0)
                continue;

            // 饰品主属性筛选
            if (isSelectAcc && (equipOb.Query<int>("equip_type") == EquipConst.AMULET || equipOb.Query<int>("equip_type") == EquipConst.NECKLACE
                || equipOb.Query<int>("equip_type") == EquipConst.RING))
            {
                equipAttrib = equipOb.Query<LPCMapping>("prop");

                if (equipAttrib == null)
                    continue;

                // 获取装备的主属性;
                mainAttrib = equipAttrib.GetValue<LPCArray>(EquipConst.MAIN_PROP);

                if (mainAttrib == null)
                    continue;

                isExistProp = false;

                foreach (LPCValue item in mainAttrib.Values)
                    if (mCurSelectAccArray.AsArray.IndexOf(item.AsArray[0].AsInt) >= 0)
                        isExistProp = true;

                if (!isExistProp)
                    continue;
            }

            validEquipList.Add(equipOb);
        }

        return validEquipList;
    }

    /// <summary>
    /// 获取批量出售的装备的价格
    /// </summary>
    private LPCMapping GetSellEquipPrice()
    {
        if (mValidEquipList == null || mValidEquipList.Count <= 0)
            return null;

        LPCMapping price = LPCMapping.Empty;

        foreach (Property equipOb in mValidEquipList)
        {
            // 获取装备的出售价格
            LPCMapping sell = PropertyMgr.GetSellPrice(equipOb);

            if (sell == null || sell.Count <= 0)
                continue;

            string fields = FieldsMgr.GetFieldInMapping(sell);

            if (price.ContainsKey(fields))
                price[fields].AsInt += sell[fields].AsInt;
            else
                price.Add(fields, sell[fields].AsInt);
        }

        return price;
    }

    /// <summary>
    /// 获取饰品主属性列表
    /// </summary>
    /// <returns></returns>
    private List<int> GetValidProps()
    {
        List<int> props = new List<int>();

        // 是否部位选择饰品（项链、护符、戒指）
        bool isExistAcc = false;

        if (mCurSelectPlaceArray != null)
        {
            LPCArray curSelectPlace = mCurSelectPlaceArray.AsArray;

            foreach (var item in curSelectPlace.Values)
            {
                if (item.AsInt == EquipConst.NECKLACE)
                {
                    isExistAcc = true;

                    props.Add(EquipConst.ACCURACY_RATE);
                    props.Add(EquipConst.RESIST_RATE);
                }
                else if (item.AsInt == EquipConst.RING)
                {
                    isExistAcc = true;

                    props.Add(EquipConst.CRT_DMG_RATE);
                    props.Add(EquipConst.CRT_RATE);
                }
                else if (item.AsInt == EquipConst.AMULET)
                {
                    isExistAcc = true;

                    props.Add(EquipConst.AGILITY);
                }
            }
        }

        if (isExistAcc)
        {
            props.Add(EquipConst.ATTACK);
            props.Add(EquipConst.ATTACK_RATE);
            props.Add(EquipConst.DEFENSE);
            props.Add(EquipConst.DEFENSE_RATE);
            props.Add(EquipConst.MAX_HP);
            props.Add(EquipConst.MAX_HP_RATE);
        }

        return props;
    }

    /// <summary>
    /// 执行点击各个星级统一处理
    /// </summary>
    /// <param name="tStar"></param>
    /// <param name="isSelect"></param>
    private void DoClickStar(int star, bool isSelect)
    {
        LPCArray curSelectStar = mCurSelectStarArray.AsArray;

        int maxStar = 6;//GameSettingMgr.GetSetting<int>("equip_max_star");

        if (isSelect && curSelectStar.IndexOf(star) == -1)
            curSelectStar.Add(star);
        else if (!isSelect && curSelectStar.IndexOf(star) != -1)
            curSelectStar.Remove(star);

        // 全选状态
        mStarAllSelectCtrl.SetStateNoTrigger(curSelectStar.Count >= maxStar ? true : false);

        // 刷新出售按钮
        RefreshSellState();
    }

    /// <summary>
    /// 执行点击各个部位统一处理
    /// </summary>
    /// <param name="place"></param>
    /// <param name="isSelect"></param>
    private void DoClickPlace(int place, bool isSelect)
    {
        LPCArray curSelectPlace = mCurSelectPlaceArray.AsArray;

        if (isSelect && curSelectPlace.IndexOf(place) == -1)
            curSelectPlace.Add(place);
        else if (!isSelect && curSelectPlace.IndexOf(place) != -1)
            curSelectPlace.Remove(place);

        // 全选状态
        mPlaceAllSelectCtrl.SetStateNoTrigger(curSelectPlace.Count >= mPlaceList.Count ? true : false);

        // 刷新饰品主属性
        RefreshAccInfo();

        // 刷新出售按钮
        RefreshSellState();
    }

    /// <summary>
    /// 执行点击各个品质统一处理
    /// </summary>
    /// <param name="rarity"></param>
    /// <param name="isSelect"></param>
    private void DoClickRarity(int rarity, bool isSelect)
    {
        LPCArray curSelectRarity = mCurSelectRarityArray.AsArray;

        if (isSelect && curSelectRarity.IndexOf(rarity) == -1)
            curSelectRarity.Add(rarity);
        else if (!isSelect && curSelectRarity.IndexOf(rarity) != -1)
            curSelectRarity.Remove(rarity);

        // 全选状态
        mRarityAllSelectCtrl.SetStateNoTrigger(curSelectRarity.Count >= mRarityList.Count ? true : false);

        // 刷新出售按钮
        RefreshSellState();
    }

    /// <summary>
    /// 执行点击各个饰品属性统一处理
    /// </summary>
    /// <param name="prop"></param>
    /// <param name="isSelect"></param>
    private void DoClickAcc(int prop, bool isSelect)
    {
        LPCArray curSelectAcc = mCurSelectAccArray.AsArray;

        List<int> validProps = GetValidProps();

        if (isSelect && curSelectAcc.IndexOf(prop) == -1)
            curSelectAcc.Add(prop);
        else if (!isSelect && curSelectAcc.IndexOf(prop) != -1)
            curSelectAcc.Remove(prop);

        // 全选状态
        mAccAllSelectCtrl.SetStateNoTrigger(curSelectAcc.Count >= validProps.Count ? true : false);

        // 刷新出售按钮
        RefreshSellState();
    }

    private void ClickShowIntensify(bool isSelect, object arg)
    {
        mSellIntensifyEquip = isSelect ? 0 : 1;

        RefreshSellState();
    }

    /// <summary>
    /// 点击套装选择全选按钮
    /// </summary>
    /// <param name="isSelect"></param>
    private void ClickSuitAllSelect(bool isSelect, object arg)
    {
        CsvFile tempCsvFile = EquipMgr.SuitTemplateCsv;

        LPCArray curSelectSuit = mCurSelectSuitArray.AsArray;

        int suitId;

        foreach (CsvRow data in tempCsvFile.rows)
        {
            suitId = data.Query<int>("suit_id");

            if (isSelect && curSelectSuit.IndexOf(suitId) == -1)
                curSelectSuit.Add(suitId);
            else if (!isSelect && curSelectSuit.IndexOf(suitId) != -1)
                curSelectSuit.Remove(suitId);
        }

        // 刷新状态
        for (int i = 0; i < mCacheSuitItemList.Count; i++)
            mCacheSuitItemList[i].SetSelectNoTrriger(isSelect);

        // 刷新出售按钮
        RefreshSellState();
    }

    /// <summary>
    /// 套装类型格子点击事件
    /// </summary>
    private void ClickSuitItem(int suitId, bool isSelect)
    {
        LPCArray curSelectSuit = mCurSelectSuitArray.AsArray;

        CsvFile tempCsvFile = EquipMgr.SuitTemplateCsv;

        if (isSelect && curSelectSuit.IndexOf(suitId) == -1)
            curSelectSuit.Add(suitId);
        else if (!isSelect && curSelectSuit.IndexOf(suitId) != -1)
            curSelectSuit.Remove(suitId);

        // 全选状态
        mSuitAllSelectCtrl.SetStateNoTrigger(curSelectSuit.Count >= tempCsvFile.count ? true : false);

        // 刷新出售按钮
        RefreshSellState();
    }

    /// <summary>
    /// 点击星级Toggle事件
    /// </summary>
    /// <param name="isSelect"></param>
    /// <param name="arg"></param>
    private void ClickStarToggle(bool isSelect, object arg)
    {
        int star = (int)arg;

        DoClickStar(star, isSelect);
    }

    /// <summary>
    /// 星级全选事件
    /// </summary>
    /// <param name="isSelect"></param>
    private void ClickStarAllSelect(bool isSelect, object arg)
    {
        int maxStar = 6;//GameSettingMgr.GetSetting<int>("equip_max_star");

        LPCArray curSelectStar = mCurSelectStarArray.AsArray;

        // 记录
        for (int i = 1; i <= maxStar; i++)
        {
            if (isSelect && curSelectStar.IndexOf(i) == -1)
                curSelectStar.Add(i);
            else if (!isSelect && curSelectStar.IndexOf(i) != -1)
                curSelectStar.Remove(i);
        }

        // 刷新状态
        for (int i = 0; i < mCacheStarItemList.Count; i++)
            mCacheStarItemList[i].SetStateNoTrigger(isSelect);

        // 刷新出售按钮
        RefreshSellState();
    }

    /// <summary>
    /// 点击部位Toggle事件
    /// </summary>
    /// <param name="isSelect"></param>
    /// <param name="arg"></param>
    private void ClickPlaceToggle(bool isSelect, object arg)
    {
        int place = (int)arg;

        DoClickPlace(place, isSelect);
    }

    /// <summary>
    /// 部位全选事件
    /// </summary>
    /// <param name="isSelect"></param>
    private void ClickPlaceAllSelect(bool isSelect, object arg)
    {
        LPCArray curSelectPlace = mCurSelectPlaceArray.AsArray;

        // 记录
        for (int i = 0; i < mPlaceList.Count; i++)
        {
            if (isSelect && curSelectPlace.IndexOf(mPlaceList[i]) == -1)
                curSelectPlace.Add(mPlaceList[i]);
            else if (!isSelect && curSelectPlace.IndexOf(mPlaceList[i]) != -1)
                curSelectPlace.Remove(mPlaceList[i]);
        }

        // 刷新状态
        for (int i = 0; i < mCachePlaceItemList.Count; i++)
            mCachePlaceItemList[i].SetStateNoTrigger(isSelect);

        // 刷新饰品主属性
        RefreshAccInfo();

        // 刷新出售按钮
        RefreshSellState();
    }

    /// <summary>
    /// 点击部位Toggle事件
    /// </summary>
    /// <param name="isSelect"></param>
    /// <param name="arg"></param>
    private void ClickRarityToggle(bool isSelect, object arg)
    {
        int rarity = (int)arg;

        DoClickRarity(rarity, isSelect);
    }

    /// <summary>
    /// 品质全选事件
    /// </summary>
    /// <param name="isSelect"></param>
    private void ClickRarityAllSelect(bool isSelect, object arg)
    {
        LPCArray curSelectRarity = mCurSelectRarityArray.AsArray;

        // 记录
        for (int i = 0; i < mRarityList.Count; i++)
        {
            if (isSelect && curSelectRarity.IndexOf(mRarityList[i]) == -1)
                curSelectRarity.Add(mRarityList[i]);
            else if (!isSelect && curSelectRarity.IndexOf(mRarityList[i]) != -1)
                curSelectRarity.Remove(mRarityList[i]);
        }

        // 刷新状态
        for (int i = 0; i < mCacheRarityItemList.Count; i++)
            mCacheRarityItemList[i].SetStateNoTrigger(isSelect);

        // 刷新出售按钮
        RefreshSellState();
    }

    /// <summary>
    /// 点击饰品属性
    /// </summary>
    /// <param name="isSelect"></param>
    /// <param name="arg"></param>
    private void ClickAccToggle(bool isSelect, object arg)
    {
        int prop = (int)arg;

        DoClickAcc(prop, isSelect);
    }

    /// <summary>
    /// 饰品主属性全选事件
    /// </summary>
    /// <param name="isSelect"></param>
    private void ClickAccAllSelect(bool isSelect, object arg)
    {
        List<int> validProps = GetValidProps();

        LPCArray curSelectAcc = mCurSelectAccArray.AsArray;

        // 记录
        for (int i = 0; i < validProps.Count; i++)
        {
            if (isSelect && curSelectAcc.IndexOf(validProps[i]) == -1)
                curSelectAcc.Add(validProps[i]);
            else if (!isSelect && curSelectAcc.IndexOf(validProps[i]) != -1)
                curSelectAcc.Remove(validProps[i]);
        }

        // 刷新状态
        int prop;

        for (int i = 0; i < mCacheAccItemList.Count; i++)
        {
            prop = int.Parse(mCacheAccItemList[i].transform.name);

            if (validProps.Contains(prop))
                mCacheAccItemList[i].SetStateNoTrigger(isSelect);
            else
            {
                mCacheAccItemList[i].SetStateNoTrigger(false);
                mCacheAccItemList[i].gameObject.SetActive(false);

                if (curSelectAcc.IndexOf(prop) >= 0)
                    curSelectAcc.Remove(prop);
            }
        }

        // 刷新出售按钮
        RefreshSellState();
    }

    /// <summary>
    /// 出售按钮点击回调
    /// </summary>
    private void OnClickSellBtn(GameObject go)
    {
        if (mValidEquipList != null && mValidEquipList.Count > 0)
        {
            DialogMgr.ShowDailog(
                new CallBack(SecondDialog),
                string.Format(LocalizationMgr.Get("EquipBatchSellWnd_10"), mValidEquipList.Count),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                this.transform);
        }
    }


    /// <summary>
    /// 出售按钮点击弹框，按钮点击回调
    /// </summary>
    void ClickSellBtnCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        LPCMapping sellPrice = GetSellEquipPrice();

        if (sellPrice == null || sellPrice.Count <= 0)
            return;

        string fields = FieldsMgr.GetFieldInMapping(sellPrice);

        string iconDesc = string.Format(LocalizationMgr.Get("EquipViewWnd_8"), FieldsMgr.GetFieldIcon(fields), 46, 46);

        string desc = string.Format(LocalizationMgr.Get("EquipBatchSellWnd_12"), sellPrice.GetValue<int>(fields), iconDesc);

        DialogMgr.ShowDailog(
            new CallBack(SecondDialog),
            desc,
            LocalizationMgr.Get("EquipBatchSellWnd_11"),
            string.Empty,
            string.Empty,
            true,
            this.transform
        );

    }

    /// <summary>
    /// 批量出售二级确认弹框
    /// </summary>
    private void SecondDialog(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        if (isClick)
            return;

        isClick = true;

        //// 构建批量出售参数
        LPCMapping batchSell = new LPCMapping();

        batchSell.Add("suit", mCurSelectSuitArray);
        batchSell.Add("star", mCurSelectStarArray);
        batchSell.Add("equip_type", mCurSelectPlaceArray);
        batchSell.Add("rarity", mCurSelectRarityArray);
        batchSell.Add("props", mCurSelectAccArray);
        batchSell.Add("sell_intensify", mSellIntensifyEquip);

        LPCMapping extraPara = new LPCMapping();
        extraPara.Add("batch_sell", batchSell);

        //// 通知服务器出售装备
        Operation.CmdSellItem.Go(extraPara);
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    private void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }
}
